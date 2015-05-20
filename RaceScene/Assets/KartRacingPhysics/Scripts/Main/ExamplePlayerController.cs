using System;
using UnityEngine;
[RequireComponent(typeof(KartController))]
public class ExamplePlayerController : MonoBehaviour
{
	private KartController kart;
	private void Start()
	{
		this.kart = base.GetComponent<KartController>();
	}
	private void Update()
	{
		this.kart.Thrust = Input.GetAxis("Vertical");
		this.kart.Steering = Input.GetAxis("Horizontal");
		if (Input.GetKeyDown(KeyCode.Q))
		{
			this.kart.Spin(2f);
		}
		if (Input.GetKeyDown(KeyCode.E))
		{
			this.kart.Wiggle(2f);
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			this.kart.Jump(1f);
		}
		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			this.kart.SpeedBoost();
		}
		if (Input.GetKeyDown(KeyCode.S))
		{
			this.kart.SpeedPenalty();
		}
	}
}
