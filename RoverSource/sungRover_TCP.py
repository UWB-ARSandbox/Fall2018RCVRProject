from bottle import route, run, template, request, static_file
from Adafruit_MotorHAT import Adafruit_MotorHAT, Adafruit_DCMotor
import argparse
import Adafruit_PCA9685
import time
import atexit
import socket
import logging
import sys
import time
from Adafruit_BNO055 import BNO055

"""
    Instances of the TCP_CarControl class accept a TCP connection
    and use the Adafruit_PCA9685, Adafruit_MotorHAT, BNO055 to provide
    the connected machine with a means to control the car and receive
    heading information.
"""
class TCP_CarControl:
    """
            A parameter of '-c' will allow the user to skip the initial
            calibration if the tool has already run once since powering
            on.
    """
    parser = argparse.ArgumentParser(description='Optionally skip calibration.')
    parser.add_argument("-c", "--cskip", help="Disable calibration.",
                        action="store_true")
    parser.add_argument("-v", "--verbose", help="Enable verbose debug logging.",
                        action="store_true")
    args = parser.parse_args()
    
    """
        The init function for the TCP_CarControl class initializes
        IP address and port number variables for the socket used by
        the class and calls additional initialization functions for 
        the class.
        @param myIP The IP address that the socket for the class will 
        be bound to.
        @param connectionPort The port number that the socket for the class
        will be bound to.
    """
    def __init__(self, myIP = None, connectionPort = None):
        print("Instantiating new CarControlTest")
        # NOTE: Wheel rotations per second, wheel circumference and feet per 
        # world coordinate unit are temporary values that need to updated when
        # actual values are known
        # self.WHEEL_ROT_S = Number of Wheel Rotations per Second = 1
        # self.WHEEL_ROT_S = 81 / 30
        #self.WHEEL_ROT_S = 3.243
        self.WHEEL_ROT_S = 1.62
        # self.WHEEL_CIRC = Circumference of the Car's Wheels = 
        # 2 * PI * Wheel Radius (2") = 2 * 3.14 * 0.165 ft = 1.05 ft   
        self.WHEEL_CIRC = 0.23 * 3.14 
        # Conversion factor for feet per world coordinate unit, 100ft per WC unit
        self.WORLD_FT = 5.33
        self.MAX_MESSAGE_LENGTH = 16
        self.sock = None
        self.connectionOpen = False
        self.bno = None
        self.headingOffset = None
        self.pwm = None
        self.mh = None
        self.myMotor1 = None
        self.myMotor2 = None
        self.dcRotSpd = 150
        self.dcTranSpd = 255
        self.dcOneTime = 0
        self.dcTwoTime = 0
        self.heading = 0
        self.isMoving = False
        self.backward = False

        if connectionPort is None:
            self.TCP_PORT = 1070
        else:
            self.TCP_PORT = connectionPort
        if myIP is None:
            self.TCP_IP = "172.24.1.1"
        else:
            self.TCP_IP = myIP
        self.initHardware()
        self.acceptConnections()
        

    """
        The initHardware function initializes an instance of the BNO55 class, 
        prints status and diagnostic information, initializes an instance
        of the Adafruit_PCA9685 class, and initializes an instance of the 
        Adafruit_MotorHAT class.
    """
    def initHardware(self):
        print("Initializing the BNO")
        self.initMotors()
        self.initBNO()
        
    
    """
        The initBNO function initializes the BNO055 orientation sensor. 
        The function prints the system status, versions, and IDs before
        calling the calibrateBNO function to calibrate the magnetometer
    """
    def initBNO(self):
        self.bno = BNO055.BNO055(serial_port='/dev/serial0')

        # Enable verbose debug logging if -v is passed as a parameter.
        if self.args.verbose:
           logging.basicConfig(level=logging.DEBUG)

        # Initialize the BNO055 and stop if something went wrong.
        if not self.bno.begin():
            raise RuntimeError('Failed to initialize BNO055! Is the sensor connected?')

        # Print system status and self test result.
        status, self_test, error = self.bno.get_system_status()
        print('System status: {0}'.format(status))
        print('Self test result (0x0F is normal): 0x{0:02X}'.format(self_test))
        # Print out an error if system status is in error mode.
        if status == 0x01:
            print('System error: {0}'.format(error))
            print('See datasheet section 4.3.59 for the meaning.')

        # Print BNO055 software revision and other diagnostic data.
        sw, bl, accel, mag, gyro = self.bno.get_revision()
        print('Software version:   {0}'.format(sw))
        print('Bootloader version: {0}'.format(bl))
        print('Accelerometer ID:   0x{0:02X}'.format(accel))
        print('Magnetometer ID:    0x{0:02X}'.format(mag))
        print('Gyroscope ID:       0x{0:02X}\n'.format(gyro))

        """
            This is where parameter '-c' comes into play. 
        """
        if not self.args.cskip:
            self.calibrateBNO()
        else:
            self.setHeadingOffset(self.headingOffset)

    """
        The initMotors function creates new instances of the Adafruit
        motor controllers used by the class and assigns them to their
        respective fields. The function also initializes the myMotor1
        and myMotor2 fields and sets the frequency for the pwm controller.
    """
    def initMotors(self):
        print("Initializing the PCA9685")
        # Initialize the PCA9685 using the default address (0x40).
        self.pwm = Adafruit_PCA9685.PCA9685()
        self.mh = Adafruit_MotorHAT(addr=0x60)
        self.pwm.set_pwm_freq(60)
        self.myMotor1 = self.mh.getMotor(1)
        self.myMotor2 = self.mh.getMotor(2)
        self.mh.getMotor(1).run(Adafruit_MotorHAT.RELEASE)
        self.mh.getMotor(2).run(Adafruit_MotorHAT.RELEASE)

    """
        The calibrateBNO() function uses console input and output to
        walk the user through the calibration routine for the magnetometer
        of the BNO055.
    """
    def calibrateBNO(self):
        self.printEulerAndCalibration()
        s, a, g, m = self.bno.get_calibration_status()
        count = 1
        messages = ["Please Enter ", "W", "D", "W", "D", "W", "A", "W", "A"]
        if(m == 0):
            print("Calibration Required - Please follow prompts to calibrate")
       
        while m == 0:
            answer = raw_input(messages[0] + messages[count] + " ")
            count += 1
            if count % 9 == 0:
                count = 1
            yaw, roll, pitch = self.bno.read_euler()
            print("Current Heading: " + str(yaw))
            self.setDistanceDrive(answer)
            print("New Heading: " + str(yaw))
            s, a, g, m = self.bno.get_calibration_status()
            print("Current Calibration - Magnetometer: " + str(m))
        print("Calibration Complete - The sensor was successfully calibrated")
        self.setHeadingOffset(self.headingOffset)

    """
        Given a string command the setDistanceDrive function sends appropriate 
        commands to the dc motors to run the motors for a set time quantum.
        Recognized commands are 'W', 'A', 'S', and 'D': forward, left, backward,
        and right. No action is taken for unrecognized commands.
        @param command A string representing the direction resulting from commands
        sent to the dc motors
    """    
    def setDistanceDrive(self, command):
        timeSlice = 2
        timeSliceTwo = 0.5
        runTime = 0
        startTime = time.time()
       
        if command == 'W':
            # Run both motors forward
            self.setDCMotors(1)
            while(runTime < timeSlice):
                runTime = time.time() - self.dcOneTime
        elif command == 'A':
            self.setDCMotors(3)
        elif command == 'S':
            self.setDCMotors(2)
            while(runTime < timeSlice):
                runTime = time.time() - startTime
        elif command == 'D':
            self.setDCMotors(4)
            while(runTime < timeSliceTwo):
                runTime = time.time() - startTime
        else:
            print("Unrecognized Command: received " + command + " in setDistanceDrive()")
        self.setDCMotors(0)
        self.dcOneTime = 0
        self.dcTwoTime = 0
        self.isMoving = False
       
    """
        The acceptConnections function initializes the socket, binds it to
        the port used for communication, and sets the socket in a listening state.
    """
    def acceptConnections(self):
        print("Initializing the socket with IP: localhost and Port: " + str(self.TCP_PORT))
        if self.sock is None:
            self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            self.sock.bind((self.TCP_IP, self.TCP_PORT))
            self.sock.listen(1)
            self.connectionOpen = True
        while self.connectionOpen :
            print("Listening for connections")
            (clientSock, cAddress) = self.sock.accept()
            self.handleConnections(clientSock, cAddress)

    """
        The handleConnections function iteratively calls the receiveCommand
        and updateHeading functions to service the current connection until 
        an exit command is received and the socket is closed.
        @param cSock The socket used for receiving commands and sending the
        heading.
        @param cAddr The address of the machine providing commands and receiving
        heading data through a TCP connection
    """
    def handleConnections(self, cSock, cAddr):
        # handle the connection by sending heading data and recieving commands
        # Currently the heading is only sent if a command is recieved and the 
        # car has moved, requiring a heading update
        print("Handling the New Connection")
        while self.connectionOpen:
            if self.isMoving:
                d = self.calculateDistance()
                self.sendData(cSock, d)
            if self.receiveCommand(cSock):
                self.sendHeading(cSock)
        cSock.close()
     
    """
        The receiveCommand function blocks until a command can be received
        from the socket. When a command is received the appropriate action 
        is taken to control the motors. The function returns a dirty status
        for the car's heading.
        @param cSock The socket used to receive the next command for the car
        @return bool Returns True if a command was received that changes the 
        car's heading.
    """
    def receiveCommand(self, cSock): 
        data = cSock.recv(16) # buffer size is 1024 bytes
        string = data.decode('utf-8')
        print("Command received: " + string)

        cmdList = string.split(',') # Commands are comma delimited
        for cmdString in cmdList:
            cmdParams = cmdString.split()   # Params are space delimited
            if len(cmdParams) > 0:
                cmd = cmdParams[0]
                print("Cmd char: " + cmd)

                leftSpeed = None
                rightSpeed = None
                # Commands should be of format: Char + ?int + ?int
                if len(cmdParams) > 3:
                    print("Invalid cmd too long: " + string)
                elif len(cmdParams) > 2:
                    if cmdParams[1].isdigit(): # Use param speed or
                        leftSpeed = int(cmdParams[1])    # default.
                    else:
                        print("Invalid cmd[1] not int: " + string)
                    if cmdParams[2].isdigit():
                        rightSpeed = int(cmdParams[2])
                    else:
                        print("Invalid cmd[2] not int: " + string)
                elif len(cmdParams) > 1:
                    if cmdParams[1].isdigit():
                        leftSpeed = int(cmdParams[1])
                        rightSpeed = int(cmdParams[1])
                    else:
                        print("Invalid cmd[1] not int: " + string)

                if cmd == "F":
                    self.setDCMotors(1, leftSpeed, rightSpeed)
                    return True
                elif cmd == "B":
                    self.setDCMotors(2, leftSpeed, rightSpeed)
                    return True
                elif cmd == "L":
                    self.setDCMotors(3, leftSpeed, rightSpeed)
                    self.heading += 2
                    return True
                elif cmd == "R":
                    self.setDCMotors(4, leftSpeed, rightSpeed)
                    self.heading -= 2
                    return True
                elif cmd == "S":
                    self.setDCMotors(0)
                    return False
                elif cmd == "E":
                    print("Closing the connection")
                    self.connectionOpen = False
                    return True
                elif cmd == 'camLeft':
                    self.pwm.set_pwm(0, 0, 390)
                elif cmd == 'camRight':
                    self.pwm.set_pwm(0, 0, 570)
                elif cmd == 'camHor':
                    self.pwm.set_pwm(0, 0, 0)
                elif cmd == 'camDown':
                    self.pwm.set_pwm(1, 0, 390)
                elif cmd == 'camVer':
                    self.pwm.set_pwm(1, 0, 0)
                elif cmd == 'camUp':
                    self.pwm.set_pwm(1, 0, 570)
                else:
                    print("Invalid cmd key: " + string)

    """
        The setDCMotors function sends appropriate commands to the 
        DC motors according to the parameter. If the parameter is
        less than 0 or greater than 4, then no action is taken.
        @param dcSet A numeric value between 0 and 4 inclusive that
        signals to set the motors to stop, forward, backward, Left,
        and right. 
    """
    def setDCMotors(self, dcSet, leftSpeed=None, rightSpeed=None):
        leftRotSpd = self.dcRotSpd
        leftTranSpd = self.dcTranSpd
        rightRotSpd = self.dcRotSpd
        rightTranSpd = self.dcTranSpd
        if leftSpeed is not None:
            leftRotSpd = leftSpeed
            leftTranSpd = leftSpeed

        if rightSpeed is not None:
            rightRotSpd = rightSpeed
            rightTranSpd = rightSpeed

        if dcSet == 0:
            # Stop - Release the motors
            self.myMotor1.run(Adafruit_MotorHAT.RELEASE)
            self.myMotor2.run(Adafruit_MotorHAT.RELEASE)
            self.pwm.set_pwm(1, 0, 0) 
            self.pwm.set_pwm(0, 0, 0)
        elif dcSet == 1:
            # Forward
            self.isMoving = True
            self.dcOneTime = time.time()
            self.dcTwoTime = time.time()
            self.myMotor1.setSpeed(leftTranSpd)
            self.myMotor1.run(Adafruit_MotorHAT.BACKWARD)
            self.myMotor2.setSpeed(rightTranSpd)
            self.myMotor2.run(Adafruit_MotorHAT.BACKWARD)
        elif dcSet == 2:
            # Backward
            self.isMoving = True
            self.backward = True
            self.dcOneTime = time.time()
            self.dcTwoTime = time.time()
            self.myMotor1.setSpeed(leftTranSpd)
            self.myMotor2.run(Adafruit_MotorHAT.FORWARD)
            self.myMotor2.setSpeed(rightTranSpd)
            self.myMotor1.run(Adafruit_MotorHAT.FORWARD)
        elif dcSet == 3:
            # Left
            self.myMotor1.setSpeed(leftRotSpd)
            self.myMotor1.run(Adafruit_MotorHAT.FORWARD)
            self.myMotor2.setSpeed(rightRotSpd)
            self.myMotor2.run(Adafruit_MotorHAT.BACKWARD)
        elif dcSet == 4:
            # Right
            self.myMotor1.setSpeed(leftRotSpd)
            self.myMotor2.run(Adafruit_MotorHAT.FORWARD)
            self.myMotor2.setSpeed(rightRotSpd)
            self.myMotor1.run(Adafruit_MotorHAT.BACKWARD) 
        else:
            print("setDCMotors: No action taken," +
            " because the parameter is not a valid command")
    
    """
        The getHeading function returns the Euler angle for the 
        yaw axis as retrieved from the BNO055 orientation sensor
        @return Numeric The Euler angle for the yaw axis
    """
    def getHeading(self):
        yaw, roll, pitch = self.bno.read_euler()
        return yaw
    """
        The sendHeading function reads the current heading value from the BNO
        and sends the value to the socket.
        @param cSock The socket used for sending the car's current heading as
        reported by the BNO orientation sensor.
    """
    def sendHeading(self, cSock) :
        heading = self.getHeading()
        heading -= self.headingOffset
        self.sendData(cSock, heading)
  
    """
        The calculateDistance function determines the time that the dcMotors
        have been running, calculates the distance traveled, and returns the
        calculated distance
    """
    def calculateDistance(self) :
        # Update the time each motor has been running
        self.dcOneTime = time.time() - self.dcOneTime
        self.dcTwoTime = time.time() - self.dcTwoTime
        self.isMoving = False
        # Distance = (motor running seconds) * (rotations/s) * (wheel circumference) * (wc/feet)
        dist = min(self.dcOneTime, self.dcTwoTime)
        dist = dist * self.WHEEL_ROT_S * self.WHEEL_CIRC * self.WORLD_FT
        if(self.backward) :
            dist = dist * -1
            self.backward = False
        return dist
        
    # NOTE: The sendDistance() and sendHeading() functions need to be refactored
    #       to eliminate duplicate code.
    """
        The sendDistance function sends the given numeric distance
        to the given socket using sendall. A string representation
        of the numberic distance is sent with the length of the string
        preceding it. If the number is more than 15 digits, then it is
        truncated to 15 digits to ensure that is may be parsed as a single
        precision float
        @param cSock The socket that the given data is written to
        @param distance The distance to be written to the given socket
    """
    def sendDistance(self, cSock, distance):
        toSend = str(distance)
        if len(toSend) > self.MAX_MESSAGE_LENGTH :
            toSend = toSend[:self.MAX_MESSAGE_LENGTH]
        msHead = str(len(toSend)) + "l"
        message = msHead + toSend
        cSock.sendall(message.encode('utf-8'))

    """
        The sendData function sends the given numeric data
        to the given socket using sendall. A string representation
        of the numberic data is sent with the length of the string
        and the character 'l' preceding it. If the number is more 
        than 15 digits, then it is truncated to 15 digits.
        @param cSock The socket that the given data is written to
        @param data The data to be written to the given socket
    """
    def sendData(self, cSock, data):
        toSend = str(data)
        if len(toSend) > self.MAX_MESSAGE_LENGTH :
            toSend = toSend[:self.MAX_MESSAGE_LENGTH]
        msHead = str(len(toSend)) + "l"
        message = msHead + toSend
        cSock.sendall(message.encode('utf-8'))

    """
        The printEulerAndCalibration function retrieves current
        roll, pitch and yaw values and respective calibration 
        values from the BNO055 and prints them to console output.
    """
    def printEulerAndCalibration(self) :
        heading, roll, pitch = self.bno.read_euler()
        # Read the calibration status, 0=uncalibrated and 3=fully calibrated.
        sys, gyro, accel, mag = self.bno.get_calibration_status()
        print('Heading={0:0.2F} Roll={1:0.2F} Pitch={2:0.2F} Sys_cal={3} Gyro_cal={4} Accel_cal={5} Mag_cal={6}'.format(
               heading, roll, pitch, sys, gyro, accel, mag))
    
    """
        Given a value, the setHeadingOffset function assigns the 
        given value to the heading offset field. If the parameter
        is None, then the current yaw value is retrieved from the
        BNO055 and assigned to the heading offset field
        @param offset The value to be assigned as the new heading offset
    """
    def setHeadingOffset(self, offset):
        if(offset is None):
            print("Preparing to set a new heading offset.")
            userReady = False
            while not userReady:
                userIn = raw_input("When the Current Heading May be Used for the Offset Please Enter 'Y'")
                if userIn == 'Y':
                    userReady = True
            yaw, roll, pitch = self.bno.read_euler()
            self.headingOffset = yaw
        else:
            self.headingOffset = offset
        print("Set Heading Offset: " + str(self.headingOffset))

    # atexit.register(setDCMotors(0, None))

# Instantiate an instance of the TCP_CarControl class
car = TCP_CarControl()
