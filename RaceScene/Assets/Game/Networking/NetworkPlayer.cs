using UnityEngine;
using System.Collections;

public class NetworkPlayer : Photon.MonoBehaviour {

	public GameObject myCamera;

//	bool isAlive = true;
	public float lerpSmoothing = 20.0f;
	Vector3 position;
	Quaternion rotation;

	//float lastUpdateTime;
	
	//private Vector3 correctPlayerPos;
	//private Quaternion correctPlayerRot;


	// Use this for initialization
	void Start () {
		if(photonView.isMine) {
	//		gameObject.name = "Me";
		myCamera.SetActive (true);
			GetComponent<HoverCarControl> ().enabled = true;
			GetComponent<Rigidbody>().useGravity = true;
		
					
		} 
		else {
			gameObject.name = "Network Player";
//			StartCoroutine("Alive");
		}

		}


	
	void Update () {
		if (photonView.isMine) {
		} 

		else {
			transform.position = Vector3.Lerp(transform.position, position, 0.1f);
			transform.rotation = Quaternion.Lerp (transform.rotation, rotation, 0.1f);

		}

	}


	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.isWriting) {
			//Player owns this player, send the others the data
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
		
			
		}
		else {
			position = (Vector3)stream.ReceiveNext();
			rotation = (Quaternion)stream.ReceiveNext();
			
			GetComponent<Rigidbody>().velocity = (Vector3)stream.ReceiveNext();
		}

	}

//	IEnumerator Alive() {
//	while (isAlive){
//			transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * lerpSmoothing);
//			transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * lerpSmoothing);
//
//			yield return null; }
//	}			                                     




}
