using UnityEngine;
using System.Collections;

public class RestarterNew : MonoBehaviour {


	public string playerPrefabName = "Car2";
	public Transform spawnPoint1;

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			PhotonNetwork.Destroy(other.gameObject);
		//	Destroy(other.gameObject);
			GameObject me = PhotonNetwork.Instantiate (playerPrefabName, spawnPoint1.position, spawnPoint1.rotation, 0);
		}
	}
}

