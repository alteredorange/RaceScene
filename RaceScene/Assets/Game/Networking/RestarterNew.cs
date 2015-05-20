using UnityEngine;
using System.Collections;

public class RestarterNew : MonoBehaviour {


	public string playerPrefabName = "Car2";
	public Transform spawnPoint1;
	public GameObject playerPrefab;

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			//PhotonNetwork.Destroy(other.gameObject);
			Destroy(other.gameObject);
			Instantiate (playerPrefab, spawnPoint1.position, spawnPoint1.rotation);
		}
	}
}

