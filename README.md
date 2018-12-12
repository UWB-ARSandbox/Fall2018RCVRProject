# Fall2018RCVRProject
## Summary
A spinoff of the VR/AR/PC/RC teamwork done on and before Summer2018CRCSProject. The goal was to refine motion and interaction between a simple Unity scene and the RCCar, using an MR headset or keyboard. The scene is isolated from ASL and Orientation data in order to better troubleshoot and operate the RCCar during maintenance or upgrades.
More information: https://docs.google.com/document/d/1Ibl1n827gBUOSSv8uxn19rc6gJQeWYWg8i9vjRXo-wo/edit?usp=sharing
## How To Operate the RC Car
### Setting up the RC Car
1. Ensure AAA batteries are fresh; turn on switch on battery pack.
2. Connect large battery pack to rPi with USB cable.
3. Connect PC wifi to SSID: `Sung-net` Password: `Sungvibe`
4. Use a command prompt to open 3 separate SSH connections to the rPi: `ssh 172.24.1.1 -l pi` Password: `SungGate`
4. At the first terminal, type `./startLeftCam.sh`
5. At the second terminal, type `./startRightCam.sh`
- Note: These connections can be tested in a browser at: `http://172.24.1.1:8080/?action=stream` (or Port: 8070 for Right Camera)
6. At the third terminal, type `sudo python sungRover_TCP.py` and follow instructions.
### Setting up a Windows MR Headset
1. Windows MR and SteamVR apps need to be installed, as well as SteamVR's Windows MR support.
2. In the Windows MR application, set up your room scale.
### Setting up the Scene
1. Run `git clone https://github.com/UWB-ARSandbox/Fall2018RCVRProject.git` to clone the repository
2. Open the `Fall2018RCVRProject/Assets/Scenes/RCControl.unity` scene
3. For keyboard operation without an MR headset:
  a. Select the `System/VR` GameObject in the hierarchy and disable `Auto Load` on the VRTK_SDK Manager (Script) Component.
  b. Select the `RCCar/CarFixture/MainCamera` GameObject in the heirarchy and enable it.
4. Press play and you will appear to be viewing the car scene from above (MR Mode) or in the cockpit (Keyboard Mode). MR controls are listed on a panel nearby. Keyboard controls are the arrow keys.
## Modifying the Scene for Troubleshooting
Select `RCCar` GameObject in Hierarchy:
- Numeral values on DirectRCControl_TCP Script adjust rate of forward/turning speed with and without high gear active. Disable this script to test video streaming without RC control.
- WebStream Left and Right can be disabled to test RCCar movement without an active stream.
## Known Issues
- If the scene is unable to connect to the RCCar, either with `DirectRCControl_TCP` or either `WebStream` then the Editor will freeze until timeout.
- While WebStream scripts are active on RCCar, MR/VR video and motion is choppy and limited FPS.
- Ending game mode will require that `sungRover_TCP.py` is restarted on the third SSH terminal. You can use `sungRover_TCP.py -c` as a parameter to skip the initial calibration.
- Game model car movement has no collision, and is simply an approximation of motion. (Not directly tied to motion of real RCCar.)
## Future Enhancements
- WebStream.cs should be handling its tasks on a secondary thread.
- Have variable speed based on the controller's axis, rather than a flat value.
- Incorporate optional orientation/direction sensor values to have game model car mirror real RCCar motion more accurately.
