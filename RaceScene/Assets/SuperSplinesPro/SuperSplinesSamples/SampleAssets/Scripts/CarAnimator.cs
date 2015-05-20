using UnityEngine;
using System.Collections;

using SplineUtilities;

public class CarAnimator : MonoBehaviour 
{
	public Spline spline;
	
	public WrapMode wrapMode = WrapMode.Clamp;
	
	public float speed = 1f;
	
	public float passedTime = 0f;
	
	public float rotationOffset;
	
	void Update( ) 
	{
		passedTime += Time.deltaTime * speed;
		
		float clampedParam = SplineUtils.WrapValue( passedTime, 0f, 1f, wrapMode );
		
		transform.rotation = spline.GetOrientationOnSpline( SplineUtils.WrapValue( passedTime + rotationOffset, 0f, 1f, wrapMode ) );
		transform.position = spline.GetPositionOnSpline( clampedParam ) - transform.right * spline.GetCustomValueOnSpline( clampedParam ) * .5f;
	}
}
