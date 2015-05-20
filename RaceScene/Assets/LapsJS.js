#pragma strict

var checkPointArray : Transform[]; //Checkpoint GameObjects stored as an array
 static var currentCheckpoint : int = 0; //Current checkpoint
 static var currentLap : int = 0; //Current lap
 static var startPos : Vector3; //Starting position
 private var moveMultiplier : int = 8; //Multiplies the Input.GetAxis movement of our player
 
 function Start () {
     //Set a simple visual aid for the Checkpoints
     for (objAlpha in checkPointArray) {
         objAlpha.GetComponent.<Renderer>().material.color.a = 0.2;
     }
     checkPointArray[0].GetComponent.<Renderer>().material.color.a = 0.8;
     
     //Store the starting position of the player
     startPos = transform.position;
 }
 
 function FixedUpdate () {
     //Movement for the player (Standard Input: Arrow Keys/W,A,S,D)
     GetComponent.<Rigidbody>().AddForce(Input.GetAxis("Horizontal")*moveMultiplier, 0, Input.GetAxis("Vertical")*moveMultiplier);
 }
 