using UnityEngine;

using SplineUtilities;

//This class animates a gameobject along the spline at a specific speed. 
public class SplineAnimator : MonoBehaviour
{
	public Spline spline;
	
	public WrapMode wrapMode = WrapMode.Clamp;
	
	public float speed = 1f;
	public float offSet = 0f;
	
	public float passedTime = 0f;
	
	void Update( ) 
	{
		passedTime += Time.deltaTime * speed;
		
		float clampedParam = SplineUtils.WrapValue( passedTime + offSet, 0f, 1f, wrapMode );
		
		transform.position = spline.GetPositionOnSpline( clampedParam );
		transform.rotation = spline.GetOrientationOnSpline( clampedParam );
	}
}
