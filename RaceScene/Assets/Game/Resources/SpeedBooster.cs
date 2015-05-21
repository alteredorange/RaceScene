using UnityEngine;
using System.Collections;

public class SpeedBooster : MonoBehaviour {

	public GameObject playerCar;

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
		
			playerCar.GetComponent<HoverCarControl>().m_forwardAcl = 9999.0f;
			StartCoroutine(speedTime());

		}
	}


	IEnumerator speedTime ()
	{
		yield return new WaitForSeconds (3);
		revertSpeed ();
	}

	void revertSpeed ()
	{
		playerCar.GetComponent<HoverCarControl>().m_forwardAcl = 6000.0f;
	}
}
