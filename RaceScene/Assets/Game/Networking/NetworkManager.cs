using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	const string VERSION = "v0.0.1";
	public string roomName = "RaceScene";
	public string playerPrefabName = "Car2";
	public Transform spawnPoint1;

	// Use this for initialization
	void Start () {
		PhotonNetwork.ConnectUsingSettings (VERSION);
	}
	void OnGUI()
	{
		GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
	}
	void OnJoinedLobby()
	{
		RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 4 };
		PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
	}
	


	void OnJoinedRoom() {
		GameObject me = PhotonNetwork.Instantiate (playerPrefabName, spawnPoint1.position, spawnPoint1.rotation, 0);
	

	}

}
