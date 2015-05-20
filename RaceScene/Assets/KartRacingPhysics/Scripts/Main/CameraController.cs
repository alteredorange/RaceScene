using System;
using UnityEngine;
public class CameraController : MonoBehaviour
{
	public Transform target;
	public Vector3 followOffset = new Vector3(0f, 2f, -5f);
	public float smoothTime = 1f;
	private Vector3 velocity;
	private void FixedUpdate()
	{
		Vector3 vector = this.target.TransformPoint(this.followOffset);
		Vector3 vector2 = base.transform.position;
		vector2 = Vector3.SmoothDamp(vector2, vector, ref this.velocity, this.smoothTime);
		transform.position = vector2;
		base.transform.LookAt(this.target.position);
	}
}
