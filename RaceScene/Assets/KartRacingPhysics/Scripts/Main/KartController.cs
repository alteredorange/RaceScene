using System;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class KartController : MonoBehaviour
{
	private const float metresToMiles = 0.0006213712f;
	private const float secondsToHours = 3600f;
	public float topSpeedMPH = 30f;
	public float accelTime = 1f;
	public float traction = 0.4f;
	public float decelerationSpeed = 0.5f;
	public Transform body;
	public Transform wheelFL;
	public Transform wheelFR;
	public Transform wheelBL;
	public Transform wheelBR;
	public float wheelRadiusFront = 0.5f;
	public float wheelRadiusBack = 0.6f;
	public float maxSteerAngle = 30f;
	public float steerSpeed = 0.5f;
	public float offRoadDrag = 2f;
	public float airDrag = 0.5f;
	public float visualOversteerAmount = 0.2f;
	private float thrust;
	private float steer;
	private float currentMPH;
	private bool isGrounded = true;
	private bool isOffRoad;
	private bool isOverturned;
	private float engineThrust;
	private float visualOversteerVel;
	private WheelCollider[] wheelColliders;
	private GameObject steeringFL;
	private GameObject steeringFR;
	private float spinTime;
	private float spinTimer;
	private float wiggleTime;
	private float wiggleMaxAngle = 15f;
	private float wiggleTimer;
	private float boostMPH;
	private float boostAccel;
	private float boostAmount;
	private float boostAmountVel;
	private float boostTimer;
	private float boostFadeTime;
	private float penaltyAmount;
	private float penaltyAmountVel;
	private float penaltyTimer;
	private float penaltyFadeTime;
	private float penaltyDrag = 10f;
	public float Thrust
	{
		get
		{
			return this.thrust;
		}
		set
		{
			this.thrust = value;
		}
	}
	public float Steering
	{
		get
		{
			return this.steer;
		}
		set
		{
			this.steer = value;
		}
	}
	public float MPH
	{
		get
		{
			return this.currentMPH;
		}
	}
	public bool IsGrounded
	{
		get
		{
			return this.isGrounded;
		}
	}
	public bool IsOffRoad
	{
		get
		{
			return this.isOffRoad;
		}
	}
	public bool IsOverturned
	{
		get
		{
			return this.isOverturned;
		}
	}
	public void Spin(float time)
	{
		this.spinTime = time;
		this.spinTimer = 0f;
	}
	public void Wiggle(float time)
	{
		this.wiggleTime = time;
		this.wiggleTimer = 0f;
	}
	public void Jump(float height)
	{
		if (this.isGrounded)
		{
			float num = 9.81f;
			float num2 = Mathf.Sqrt(2f * height * num);
			Vector3 velocity = base.GetComponent<Rigidbody>().velocity;
			velocity.y += num2;
			base.GetComponent<Rigidbody>().velocity = velocity;
		}
	}
	public void SpeedBoost(float boostTopSpeedMPH, float boostAccelTime, float boostTime, float fadeTime)
	{
		this.boostMPH = boostTopSpeedMPH;
		this.boostAccel = boostAccelTime;
		this.boostAmount = 1f;
		this.boostAmountVel = 0f;
		this.boostTimer = boostTime;
		this.boostFadeTime = fadeTime;
		this.penaltyAmount = 0f;
	}
	public void SpeedBoost()
	{
		this.SpeedBoost(1.6f * this.topSpeedMPH, 0.25f * this.accelTime, 1f, 1f);
	}
	public void SpeedPenalty(float amount, float penaltyTime, float fadeTime)
	{
		this.penaltyAmount = amount;
		this.penaltyTimer = penaltyTime;
		this.penaltyFadeTime = fadeTime;
		this.penaltyAmountVel = 0f;
		this.boostAmount = 0f;
	}
	public void SpeedPenalty()
	{
		this.SpeedPenalty(0.4f, 0.5f, 1f);
	}
	private void Start()
	{
		this.wheelColliders = new WheelCollider[4];
		this.wheelColliders[0] = this.CreateWheelCollider(this.wheelFL.position, this.wheelRadiusFront);
		this.wheelColliders[1] = this.CreateWheelCollider(this.wheelFR.position, this.wheelRadiusFront);
		this.wheelColliders[2] = this.CreateWheelCollider(this.wheelBL.position, this.wheelRadiusBack);
		this.wheelColliders[3] = this.CreateWheelCollider(this.wheelBR.position, this.wheelRadiusBack);
		this.steeringFL = new GameObject("SteeringFL");
		this.steeringFR = new GameObject("SteeringFR");
		this.steeringFL.transform.position = this.wheelFL.position;
		this.steeringFR.transform.position = this.wheelFR.position;
		this.steeringFL.transform.rotation = this.wheelFL.rotation;
		this.steeringFR.transform.rotation = this.wheelFR.rotation;
		this.steeringFL.transform.parent = this.wheelFL.parent;
		this.steeringFR.transform.parent = this.wheelFR.parent;
		this.wheelFL.parent = this.steeringFL.transform;
		this.wheelFR.parent = this.steeringFR.transform;
		Vector3 vector = 0.5f * (this.wheelFL.localPosition + this.wheelFR.localPosition);
		Vector3 vector2 = 0.5f * (this.wheelBL.localPosition + this.wheelBR.localPosition);
		Vector3 vector3 = 0.5f * (vector + vector2);
		float num = 0.5f * (this.wheelRadiusFront + this.wheelRadiusBack);
		base.GetComponent<Rigidbody>().centerOfMass = vector3 - 0.8f * num * Vector3.up;
	}
	private WheelCollider CreateWheelCollider(Vector3 position, float radius)
	{
		GameObject gameObject = new GameObject("WheelCollision");
		gameObject.transform.parent = this.body;
		gameObject.transform.position = position;
		gameObject.transform.localRotation = Quaternion.identity;
		WheelCollider wheelCollider = gameObject.AddComponent<WheelCollider>();
		wheelCollider.radius = radius;
		wheelCollider.suspensionDistance = 0.1f;
		return wheelCollider;
	}
	private void FixedUpdate()
	{
		Vector3 vector = base.transform.InverseTransformDirection(base.GetComponent<Rigidbody>().velocity);
		this.currentMPH = vector.z * 0.0006213712f * 3600f;
		this.engineThrust = this.thrust;
		Vector3 vector2 = 0.5f * (this.wheelFR.position + this.wheelFL.position) - new Vector3(0f, 0.5f, 1f) * this.wheelRadiusFront;
		RaycastHit raycastHit;
        isGrounded = Physics.Raycast(vector2, -Vector3.up, out raycastHit, wheelRadiusFront);
		if (this.isGrounded)
		{
			this.isOffRoad = raycastHit.collider.gameObject.CompareTag("OffRoad");
		}
		this.isOverturned = (base.transform.up.y < 0f);
		this.engineThrust *= 1f - this.penaltyAmount * this.penaltyAmount;
		if (this.isGrounded)
		{
			this.ApplyThrust();
		}
		this.ApplyDrag();
		this.ApplySteering();
		this.ApplyEffects();
		float num = vector.z / this.wheelRadiusFront * Time.deltaTime * 57.29578f;
		float num2 = vector.z / this.wheelRadiusBack * Time.deltaTime * 57.29578f;
		this.wheelFL.Rotate(num, 0f, 0f);
		this.wheelFR.Rotate(num, 0f, 0f);
		this.wheelBL.Rotate(num2, 0f, 0f);
		this.wheelBR.Rotate(num2, 0f, 0f);
	}
	private void ApplyDrag()
	{
		Vector3 vector = base.transform.InverseTransformDirection(base.GetComponent<Rigidbody>().velocity);
		Vector3 vector2 = Vector3.zero;
		float num = Mathf.Lerp(0.1f, 0.5f, this.traction);
		float num2 = Mathf.Lerp(1f, 5f, this.traction);
		float num3 = Mathf.Lerp(0f, 5f, this.decelerationSpeed);
		vector2.z = vector.z * (num + (1f - Mathf.Abs(this.engineThrust)) * num3);
		if (this.isOffRoad)
		{
			vector2.z += vector.z * this.offRoadDrag;
		}
		vector2.x = vector.x * num2;
		if (!this.isGrounded)
		{
			vector2 *= this.airDrag;
		}
		vector2 = Vector3.Lerp(vector2, this.penaltyDrag * vector2, this.penaltyAmount);
		vector2 = base.transform.TransformDirection(vector2);
		Vector3 vector3 = base.GetComponent<Rigidbody>().velocity;
		vector3 -= vector2 * Time.deltaTime;
		base.GetComponent<Rigidbody>().velocity = vector3;
	}
	private void ApplyThrust()
	{
		float num = Mathf.Lerp(this.topSpeedMPH, this.boostMPH, this.boostAmount);
		float num2 = Mathf.Lerp(this.accelTime, this.boostAccel, this.boostAmount);
		num2 = Mathf.Max(0.01f, num2);
		float num3 = num / 2.23693633f;
		float num4 = 0.2f * num;
		float num5 = num3 / num2;
		if (this.currentMPH >= num || this.currentMPH <= -num4)
		{
			num5 = 0f;
		}
		Vector3 forward = base.transform.forward;
		Vector3 vector = num5 * forward * this.engineThrust;
		Vector3 vector2 = base.GetComponent<Rigidbody>().velocity;
		vector2 += vector * Time.deltaTime;
		base.GetComponent<Rigidbody>().velocity = vector2;
		float brakeTorque = 0f;
		if (this.engineThrust == 0f && this.currentMPH < 10f)
		{
			brakeTorque = 20f * this.decelerationSpeed * (10f - this.currentMPH);
		}
		WheelCollider[] array = this.wheelColliders;
		for (int i = 0; i < array.Length; i++)
		{
			WheelCollider wheelCollider = array[i];
			wheelCollider.brakeTorque = brakeTorque;
		}
	}
	private void ApplySteering()
	{
		float num = this.steer * this.maxSteerAngle;
		this.steeringFL.transform.localRotation = Quaternion.Euler(0f, num, 0f);
		this.steeringFR.transform.localRotation = Quaternion.Euler(0f, num, 0f);
		if (this.isGrounded && base.GetComponent<Rigidbody>().velocity.sqrMagnitude > 0.1f)
		{
			num *= Mathf.Sign(base.transform.InverseTransformDirection(base.GetComponent<Rigidbody>().velocity).z);
			Quaternion quaternion = Quaternion.Euler(0f, num * Time.deltaTime * (1f + 2f * this.steerSpeed), 0f);
			base.GetComponent<Rigidbody>().MoveRotation(base.transform.rotation * quaternion);
			float y = this.body.localRotation.eulerAngles.y;
			float num2 = Mathf.SmoothDampAngle(y, this.visualOversteerAmount * num, ref this.visualOversteerVel, 0.5f);
			this.body.localRotation = Quaternion.Euler(0f, num2, 0f);
		}
	}
	private float WiggleCurve(float t)
	{
		float num = 3f;
		float num2 = Mathf.Sin(t * num * 2f * 3.14159274f);
		float num3 = 1f + 2f * t * t * t - 3f * t * t;
		return num2 * num3;
	}
	private void UpdateSpin()
	{
		if (this.spinTime > 0f)
		{
			this.spinTimer += Time.deltaTime;
			float num = this.spinTimer / this.spinTime;
			if (num >= 1f)
			{
				num = 0f;
				this.spinTime = 0f;
			}
			Vector3 eulerAngles = this.body.localRotation.eulerAngles;
			float num2 = 1f - (1f - num) * (1f - num);
			eulerAngles.y = 720f * num2;
			this.body.localRotation = Quaternion.Euler(eulerAngles);
		}
	}
	private void UpdateWiggle()
	{
		if (this.wiggleTime > 0f)
		{
			this.wiggleTimer += Time.deltaTime;
			float num = this.wiggleTimer / this.wiggleTime;
			if (num >= 1f)
			{
				num = 0f;
				this.wiggleTime = 0f;
			}
			Vector3 eulerAngles = this.body.localRotation.eulerAngles;
			eulerAngles.y = this.wiggleMaxAngle * this.WiggleCurve(num);
			this.body.localRotation = Quaternion.Euler(eulerAngles);
		}
	}
	private void UpdateBoost()
	{
		this.boostTimer -= Time.deltaTime;
		if (this.boostTimer < 0f)
		{
			this.boostAmount = Mathf.SmoothDamp(this.boostAmount, 0f, ref this.boostAmountVel, this.boostFadeTime);
		}
	}
	private void UpdatePenalty()
	{
		this.penaltyTimer -= Time.deltaTime;
		if (this.penaltyTimer < 0f)
		{
			this.penaltyAmount = Mathf.SmoothDamp(this.penaltyAmount, 0f, ref this.penaltyAmountVel, this.penaltyFadeTime);
		}
	}
	private void ApplyEffects()
	{
		this.UpdateWiggle();
		this.UpdateSpin();
		this.UpdateBoost();
		this.UpdatePenalty();
	}
}
