﻿using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {



		public static int currentCheckpoint = 0; 
	public static int currentLap = 0; 

		
		void  OnTriggerEnter ( Collider other  )
		{
			//Is it the Player who enters the collider?
			if (!other.CompareTag ("Player")) 
			return; //If it's not the player dont continue
		
		if (other.tag == "Player")	
		// if (transform == Laps.checkpointA[Laps.currentCheckpoint].transform) 
			{
				//Check so we dont exceed our checkpoint quantity
				if (Laps.currentCheckpoint + 1 < Laps.checkpointA.Length) 
				{
					//Add to currentLap if currentCheckpoint is 0
					if(Laps.currentCheckpoint == 0)
						Laps.currentLap++;
					Laps.currentCheckpoint++;
				} 
				else 
				{
					//If we dont have any Checkpoints left, go back to 0
					Laps.currentCheckpoint = 0;
				}
			}
			
			
		}
		
	}