// Author(s): Aaron Holloway
// Fall 2018
// Last Modified: 12/2/2018

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Allows the player to switch the position of this gameObject
// between two transform points.
public class MRViewportBehavior : MonoBehaviour {

	public Transform DefaultSocket = null;
	public Transform SwapSocket = null;

	private Transform currentSocket;

	public void MoveViewportTo(Transform socket) {
		if (currentSocket != socket) {
			// Boolean false updates the facing of the moved gameObject.
			gameObject.transform.SetParent(socket, false);
			currentSocket = socket;
		}
	}

	public void SwapViewport() {
		if (SwapSocket != null && currentSocket == DefaultSocket) {
			MoveViewportTo(SwapSocket);
		} else if (DefaultSocket != null) {
			MoveViewportTo(DefaultSocket);
		} else {
			Debug.Log("MRViewportBehavior | SwapViewport: No Socket Assigned");
		}
	}

}
