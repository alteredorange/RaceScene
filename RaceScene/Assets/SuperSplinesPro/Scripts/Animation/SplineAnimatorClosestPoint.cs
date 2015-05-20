using UnityEngine;
using SplineUtilities;

//This class computes a point on a spline that is as close as possible to a given gameobject
public class SplineAnimatorClosestPoint : MonoBehaviour
{
	public Spline spline;
	
	public WrapMode wMode = WrapMode.Clamp;
	
	public Transform target;
	
	public int iterations = 5;
	public float diff = 0.5f;
	public float offset = 0;
	
//	private float lastParam;
	
	void Update( ) 
	{
		if( target == null || spline == null )
			return;
		
		float param = SplineUtils.WrapValue( spline.GetClosestPointParam( target.position, iterations ) + offset, 0f, 1f, wMode );
//		float param = SplineUtils.WrapValue( spline.GetClosestPointParam( target.position, iterations, lastParam, diff ) + offset, 0f, 1f, wMode );
		
		transform.position = spline.GetPositionOnSpline( param );
		transform.rotation = spline.GetOrientationOnSpline( param );
		
//		lastParam = param;
	}
}
