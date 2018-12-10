// Author(s): Aaron Holloway
// Fall 2018
// Last Modified: 12/2/2018

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple Script to Move a GameObject in the Scene in a manner similar to
// the motion of the RCCar.
public class GameCarMovement : MonoBehaviour {

	public float DriveSpeed = 1f;
	public float RotateSpeed = 1f;

	public bool HighGear = false;

	private float currentDriveSpeed;
	private float currentRotateSpeed;

	private int driveDir = 0;
	private int rotateDir = 0;

	void Start () {
		currentDriveSpeed = DriveSpeed;
		currentRotateSpeed = RotateSpeed;
	}

	void Update () {
		if (driveDir != 0) {
			MoveCar();
		}
		if (rotateDir != 0) {
			RotateCar();
		}
	}

	private void MoveCar() {
		gameObject.transform.position += (gameObject.transform.forward * (driveDir * currentDriveSpeed * Time.deltaTime));
	}

	private void RotateCar() {
		transform.Rotate(Vector3.up * (rotateDir * currentRotateSpeed * Time.deltaTime));
	}

	public void forwardAction() {
		driveDir = 1;
		rotateDir = 0;
	}

	public void forwardLeftAction() {
		driveDir = 1;
		rotateDir = -1;
	}

	public void forwardRightAction() {
		driveDir = 1;
		rotateDir = 1;
	}

	public void backAction() {
		driveDir = -1;
		rotateDir = 0;
	}

	public void backLeftAction() {
		driveDir = -1;
		rotateDir = 1;
	}

	public void backRightAction() {
		driveDir = -1;
		rotateDir = -1;
	}

	public void leftAction() {
		driveDir = 0;
		rotateDir = -1;
	}

	public void rightAction() {
		driveDir = 0;
		rotateDir = 1;
	}

	public void stopAction() {
		driveDir = 0;
		rotateDir = 0;
	}

	public void setHighGear() {
		HighGear = true;
		currentDriveSpeed = DriveSpeed * 1.5f;
		currentRotateSpeed = RotateSpeed * 1.5f;
	}

	public void unsetHighGear() {
		HighGear = false;
		currentDriveSpeed = DriveSpeed;
		currentRotateSpeed = RotateSpeed;
	}

}
