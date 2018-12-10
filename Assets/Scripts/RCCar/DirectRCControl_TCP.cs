// Author(s): Aaron Holloway
// Fall 2018
// Last Modified: 12/2/2018

using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;

// Derived from Summer2018CRCSProject/Assets/ASL/RC/Scripts RCBehavior_TCP.cs

// Allows operation of the RC Car without use of orientation sensor or connection
// to ASL library. Useful for troubleshooting or improving mechanical behaviors.
public class DirectRCControl_TCP : MonoBehaviour
{
    // Public
    ///////////////////////////
    public int DriveSpeed = 180;
    public int HighDriveSpeed = 255;
    public int TurnSpeed = 180;
    public int HighTurnSpeed = 255;

    public bool HighGear = false;

    // Private
    ///////////////////////////
    private const string rcAddress = "172.24.1.1";
    private const short rcCPort = 1070;
    private const short MAX_MESSAGE_LENGTH = 16;

    private TcpClient client;
    private IPEndPoint ep;
    private NetworkStream sock;

    private bool connected;
    private bool connectionClosed;

        // private Byte[] rBuff;
    private Byte[] sBuff;

    private short currentCommand;

        // private float headingOffset;
        // private bool headingDirty;
        // private float lastHeading;

        // private bool distanceDirty;

    void Start() {
        connected = false;
        connectionClosed = true;

            // headingDirty = false;
            // distanceDirty = false;
            // isMoving = false;

        sBuff = new Byte[MAX_MESSAGE_LENGTH];
            //rBuff = new Byte[MAX_MESSAGE_LENGTH];

        connectRemote();
    }

    void Update() {
        if (connected) {
                // if (headingDirty) {
                //     updateHeading();
                // }
                //
                // if (distanceDirty) {
                //     updateDistance();
                // }
        } else {
            if (!connectionClosed) {
                connectionClosed = true;
                print("Closing the TCP connection to the remote control car");
                sock.Close();
                client.Close();
            }
        }
    }

    void connectRemote()
    {
        client = new TcpClient();
        ep = new IPEndPoint(IPAddress.Parse(rcAddress), rcCPort);
        client.Connect(ep);
        sock = client.GetStream();
        connected = true;
        connectionClosed = false;
        print("TcpClient is connected to the end point with address: " +
        rcAddress + " and port: " + rcCPort);
    }

    ///////////////////
    // Movement Methods
    //
    // Notes: Commands are received by sungRover_TCP.py on the RCCar's rPi.
    // Each command is comma delimited, while certain commands can have space
    // delimited parameters. In the case of movement, two numbers following the
    // command indicate the desired speed of the left/right motors, respectively.
    ///////////////////////////////////////////////////////////////////////////

    public void forwardAction() {
        string cmd = "F";
        if (HighGear) {
            cmd += " " + HighDriveSpeed + " " + HighDriveSpeed + ",";
        } else {
            cmd += " " + DriveSpeed + " " + DriveSpeed + ",";
        }
        sBuff = Encoding.Default.GetBytes(cmd);
        currentCommand = 1;
        Debug.Log("Sending Forward command: " + cmd);
        sendCommand();
    }

    public void forwardLeftAction() {
        string cmd = "F";
        if (HighGear) {
            cmd += " " + (HighTurnSpeed / 2) + " " + HighDriveSpeed + ",";
        } else {
            cmd += " " + (TurnSpeed / 2) + " " + DriveSpeed + ",";
        }
        sBuff = Encoding.Default.GetBytes(cmd);
        currentCommand = 2;
        Debug.Log("Sending Forward/Left command: " + cmd);
        sendCommand();
    }

    public void forwardRightAction() {
        string cmd = "F";
        if (HighGear) {
            cmd += " " + HighDriveSpeed + " " + (HighTurnSpeed / 2) + ",";
        } else {
            cmd += " " + DriveSpeed + " " + (TurnSpeed / 2) + ",";
        }
        sBuff = Encoding.Default.GetBytes(cmd);
        currentCommand = 3;
        Debug.Log("Sending Forward/Right command: " + cmd);
        sendCommand();
    }

    public void backAction() {
        string cmd = "B";
        if (HighGear) {
            cmd += " " + HighDriveSpeed + " " + HighDriveSpeed + ",";
        } else {
            cmd += " " + DriveSpeed + " " + DriveSpeed + ",";
        }
        sBuff = Encoding.Default.GetBytes(cmd);
        currentCommand = 4;
        Debug.Log("Sending Back command: " + cmd);
        sendCommand();
    }

    public void backLeftAction() {
        string cmd = "B";
        if (HighGear) {
            cmd += " " + (HighTurnSpeed / 2) + " " + HighDriveSpeed + ",";
        } else {
            cmd += " " + (TurnSpeed / 2) + " " + DriveSpeed + ",";
        }
        sBuff = Encoding.Default.GetBytes(cmd);
        currentCommand = 5;
        Debug.Log("Sending Back/Left command: " + cmd);
        sendCommand();
    }

    public void backRightAction() {
        string cmd = "B";
        if (HighGear) {
            cmd += " " + HighDriveSpeed + " " + (HighTurnSpeed / 2) + ",";
        } else {
            cmd += " " + DriveSpeed + " " + (TurnSpeed / 2) + ",";
        }
        sBuff = Encoding.Default.GetBytes(cmd);
        currentCommand = 6;
        Debug.Log("Sending Back/Right command: " + cmd);
        sendCommand();
    }

    public void leftAction() {
        string cmd = "L";
        if (HighGear) {
            cmd += " " + HighTurnSpeed + " " + HighTurnSpeed + ",";
        } else {
            cmd += " " + TurnSpeed + " " + TurnSpeed + ",";
        }
        sBuff = Encoding.Default.GetBytes(cmd);
        currentCommand = 7;
        Debug.Log("Sending Left command: " + cmd);
        sendCommand();
    }

    public void rightAction() {
        string cmd = "R";
        if (HighGear) {
            cmd += " " + HighTurnSpeed + " " + HighTurnSpeed + ",";
        } else {
            cmd += " " + TurnSpeed + " " + TurnSpeed + ",";
        }
        sBuff = Encoding.Default.GetBytes(cmd);
        currentCommand = 8;
        Debug.Log("Sending Right command: " + cmd);
        sendCommand();
    }

    public void stopAction() {
        if (currentCommand > 0) {
            sBuff = Encoding.ASCII.GetBytes("S,");
            currentCommand = 0;
            Debug.Log("Sending Stop command: S,");
            sendCommand();
        }
    }

    public void exitAction() {
        sBuff = Encoding.Default.GetBytes("E,");
        Debug.Log("Sending Exit command: E,");
        sendCommand();
        connected = false;
        // headingDirty = false;
    }

    public void setHighGear() {
        if(!HighGear) {
            HighGear = true;
            renewCurrentCommand();
        }
    }

    public void unsetHighGear() {
        if(HighGear) {
            HighGear = false;
            renewCurrentCommand();
        }
    }

    void renewCurrentCommand() {
        switch(currentCommand) {
            case 1:
                forwardAction();
                break;
            case 2:
                forwardLeftAction();
                break;
            case 3:
                forwardRightAction();
                break;
            case 4:
                backAction();
                break;
            case 5:
                backLeftAction();
                break;
            case 6:
                backRightAction();
                break;
            case 7:
                leftAction();
                break;
            case 8:
                rightAction();
                break;
            default:
                break;
        }
    }

    void sendCommand()
    {
        if (connected)
        {
            if (sock.CanWrite)
            {
                sock.Write(sBuff, 0, sBuff.Length);
            }
        }
        else
        {
            print("The sendCommand() function cannot write to the socket," +
            " because the socket is not connected.");
        }
    }

    // Remnant Methods from RCBehavior_TCP.cs that utilize BMO Orientation
    ///////////////////////////////////////////////////////////////////////////

    // bool updateHeading()
    // {
    //     float heading = getData();
    //     if (heading != -1)
    //     {
    //         // setNewHeading(heading);
    //         headingDirty = false;
    //     }
    //     else
    //     {
    //         print("A new heading could not be read from the stream");
    //         return false;
    //     }
    //     return true;
    // }

    // float getData()
    // {
    //     int headLen = 0;
    //     string tempS = "";
    //     int temp = 0;
    //     while (temp != -1 && (char)temp != 'l')
    //     {
    //         temp = sock.ReadByte();
    //         if ((char)temp != 'l')
    //             tempS += (char)temp;
    //     }
    //     headLen = Int32.Parse(tempS);
    //     int bRead = 0;
    //     if (sock.CanRead)
    //     {
    //         while (bRead < headLen)
    //         {
    //             bRead += sock.Read(rBuff, bRead, headLen);
    //         }
    //         return Single.Parse(Encoding.UTF8.GetString(rBuff, 0, bRead));
    //     }
    //     else
    //         return -1;
    // }

    // void setNewHeading(float target)
    // {
    //     // Quaternion rQ = Quaternion.Euler(0, (target + headingOffset), 0);
    //     // xform.rotation = rQ;
    //     lastHeading = target;
    // }

    // bool updateDistance()
    // {
    //     float dist = getData();
    //     if (dist != -1)
    //     {
    //         // setTranslation(dist);
    //         distanceDirty = false;
    //     }
    //     else
    //     {
    //         print("A new distance could not be read from the stream");
    //         return false;
    //     }
    //     return true;
    // }

}
