using System;
using UnityEngine;
public abstract class PickupItemBase : MonoBehaviour
{
	protected abstract void OnPickupCollected(KartController kart);
	private void OnTriggerEnter(Collider other)
	{
		KartController component = other.attachedRigidbody.GetComponent<KartController>();
		if (component != null)
		{
			this.OnPickupCollected(component);
		}
		this.Hide();
		base.Invoke("Show", 5f);
	}
	private void Hide()
	{
		base.gameObject.SetActive(false);
	}
	private void Show()
	{
		base.gameObject.SetActive(true);
	}
	private void Update()
	{
		Camera main = Camera.main;
		base.transform.LookAt(base.transform.position + main.transform.rotation * Vector3.back, main.transform.rotation * Vector3.up);
	}
}
