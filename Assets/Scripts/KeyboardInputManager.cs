// Author(s): Aaron Holloway
// Fall 2018
// Last Modified: 11/18/2018

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Monitors Keyboard Events and resulting behavior.
public class KeyboardInputManager : MonoBehaviour {

	public DirectRCControl_TCP rc;

	void Update () {

		// As Up is Pressed
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			if (Input.GetKey(KeyCode.LeftArrow) && !(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.DownArrow))) {
				rc.forwardLeftAction();
			} else if (Input.GetKey(KeyCode.RightArrow) && !(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.DownArrow))) {
				rc.forwardRightAction();
			} else if (!(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.DownArrow))) {
				rc.forwardAction();
			} else {
				rc.stopAction();
			}
		}

		// As Up is Released
		if (Input.GetKeyUp(KeyCode.UpArrow)) {
			if ((Input.GetKey(KeyCode.LeftArrow) && (Input.GetKey(KeyCode.DownArrow))) && !Input.GetKey(KeyCode.RightArrow)) {
				rc.backLeftAction();
			} else if (Input.GetKey(KeyCode.LeftArrow) && !(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.DownArrow))) {
				rc.leftAction();
			} else if ((Input.GetKey(KeyCode.RightArrow) && (Input.GetKey(KeyCode.DownArrow))) && !Input.GetKey(KeyCode.LeftArrow)) {
				rc.backRightAction();
			} else if (Input.GetKey(KeyCode.RightArrow) && !(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.DownArrow))) {
				rc.rightAction();
			} else if (Input.GetKey(KeyCode.DownArrow) && !(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))) {
				rc.backAction();
			} else {
				rc.stopAction();
			}
		}

		// As Down is Pressed
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			if (Input.GetKey(KeyCode.LeftArrow) && !(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.UpArrow))) {
				rc.backLeftAction();
			} else if (Input.GetKey(KeyCode.RightArrow) && !(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.UpArrow))) {
				rc.backRightAction();
			} else if (!(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.UpArrow))) {
				rc.backAction();
			} else {
				rc.stopAction();
			}
		}

		// As Down is Released
		if (Input.GetKeyUp(KeyCode.DownArrow)) {
			if ((Input.GetKey(KeyCode.LeftArrow) && (Input.GetKey(KeyCode.UpArrow))) && !Input.GetKey(KeyCode.RightArrow)) {
				rc.forwardLeftAction();
			} else if (Input.GetKey(KeyCode.LeftArrow) && !(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.UpArrow))) {
				rc.leftAction();
			} else if ((Input.GetKey(KeyCode.RightArrow) && (Input.GetKey(KeyCode.UpArrow))) && !Input.GetKey(KeyCode.LeftArrow)) {
				rc.forwardRightAction();
			} else if (Input.GetKey(KeyCode.RightArrow) && !(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.UpArrow))) {
				rc.rightAction();
			} else if (Input.GetKey(KeyCode.UpArrow) && !(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))) {
				rc.forwardAction();
			} else {
				rc.stopAction();
			}
		}

		// As Left is Pressed
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			if (Input.GetKey(KeyCode.UpArrow) && !(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow))) {
				rc.forwardLeftAction();
			} else if (Input.GetKey(KeyCode.DownArrow) && !(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.RightArrow))) {
				rc.backLeftAction();
			} else if (!(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow))) {
				rc.leftAction();
			} else {
				rc.stopAction();
			}
		}

		// As Left is Released
		if (Input.GetKeyUp(KeyCode.LeftArrow)) {
			if ((Input.GetKey(KeyCode.UpArrow) && (Input.GetKey(KeyCode.RightArrow))) && !Input.GetKey(KeyCode.DownArrow)) {
				rc.forwardRightAction();
			} else if (Input.GetKey(KeyCode.UpArrow) && !(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow))) {
				rc.forwardAction();
			} else if ((Input.GetKey(KeyCode.DownArrow) && (Input.GetKey(KeyCode.RightArrow))) && !Input.GetKey(KeyCode.UpArrow)) {
				rc.backRightAction();
			} else if (Input.GetKey(KeyCode.DownArrow) && !(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.RightArrow))) {
				rc.backAction();
			} else if (Input.GetKey(KeyCode.RightArrow) && !(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))) {
				rc.rightAction();
			} else {
				rc.stopAction();
			}
		}

		// As Right is Pressed
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			if (Input.GetKey(KeyCode.UpArrow) && !(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow))) {
				rc.forwardRightAction();
			} else if (Input.GetKey(KeyCode.DownArrow) && !(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow))) {
				rc.backRightAction();
			} else if (!(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow))) {
				rc.rightAction();
			} else {
				rc.stopAction();
			}
		}

		// As Right is Released
		if (Input.GetKeyUp(KeyCode.RightArrow)) {
			if ((Input.GetKey(KeyCode.UpArrow) && (Input.GetKey(KeyCode.LeftArrow))) && !Input.GetKey(KeyCode.DownArrow)) {
				rc.forwardLeftAction();
			} else if (Input.GetKey(KeyCode.UpArrow) && !(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow))) {
				rc.forwardAction();
			} else if ((Input.GetKey(KeyCode.DownArrow) && (Input.GetKey(KeyCode.LeftArrow))) && !Input.GetKey(KeyCode.UpArrow)) {
				rc.backLeftAction();
			} else if (Input.GetKey(KeyCode.DownArrow) && !(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow))) {
				rc.backAction();
			} else if (Input.GetKey(KeyCode.LeftArrow) && !(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))) {
				rc.leftAction();
			} else {
				rc.stopAction();
			}
		}

		// As Shift is Pressed
		if (Input.GetKeyDown(KeyCode.LeftShift)) {
			rc.setHighGear();
		}

		// As Shift is Released
		if (Input.GetKeyUp(KeyCode.LeftShift)) {
			rc.unsetHighGear();
		}

		// As E is Pressed
		if (Input.GetKeyDown(KeyCode.E)) {
			rc.exitAction();
		}

	}
}
