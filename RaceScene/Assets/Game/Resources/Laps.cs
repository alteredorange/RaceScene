﻿using UnityEngine;
using System.Collections;

public class Laps : MonoBehaviour {
		
		// These Static Variables are accessed in "checkpoint" Script
		public Transform[] checkPointArray;
		public static Transform[] checkpointA;
		public static int currentCheckpoint = 0; 
		public static int currentLap = 0; 
		public Vector3 startPos;
		public int Lap;
		public int currCheck;
		public int currLap ;


		void  Start ()
		{
		startPos = transform.position;
			currentCheckpoint = 0;
			currentLap = 0; 
			
		}
		
		void  Update ()
		{
			Lap = currentLap;
			checkpointA = checkPointArray;
		currLap = currentLap;
		currCheck = currentCheckpoint;
		}
		
	}