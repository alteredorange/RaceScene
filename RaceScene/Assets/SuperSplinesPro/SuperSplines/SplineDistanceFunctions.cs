using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class Spline : MonoBehaviour 
{
	/** 
	* This function calculates the closest point on the spline to a given point.
	* @param point A given point.
	* @param iterations Define how accurate the calculation will be. A value of 5 should be high enough for most purposes.
	* @param start A spline parameter from 0 to 1 that specifies the lower bound for the numeric search. (default is 0.0)
	* @param end A spline parameter from 0 to 1 that specifies the upper bound for the numeric search. (default is 1.0)
	* @param step Specifies the step between two sample points on the spline for the 1st iteration. (default is 0.01) 
    * @return Returns the closest point to point on the spline.
	*/
	public float GetClosestPointParam( Vector3 point, int iterations, float start = 0, float end = 1, float step = .01f )
	{
		return GetClosestPointParamIntern( (splinePos) => (point-splinePos).sqrMagnitude, iterations, start, end, step );
	}
	
	/** 
	* This function calculates the closest point on the spline to a given ray.
	* @param ray A given ray.
	* @param iterations Define how accurate the calculation will be. A value of 5 should be high enough for most purposes.
	* @param start A spline parameter from 0 to 1 that specifies the lower bound for the numeric search. (default is 0.0)
	* @param end A spline parameter from 0 to 1 that specifies the upper bound for the numeric search. (default is 1.0)
	* @param step Specifies the step between two sample points on the spline for the 1st iteration. (default is 0.01) 
    * @return Returns the closest point to ray on the spline.
	*/
	public float GetClosestPointParamToRay( Ray ray, int iterations, float start = 0, float end = 1, float step = .01f )
	{	
		return GetClosestPointParamIntern( (splinePos) => Vector3.Cross( ray.direction, splinePos - ray.origin ).sqrMagnitude, iterations, start, end, step );
	}
	
	/** 
	* This function calculates the closest point on the spline to a given plane.
	* @param plane A given plane.
	* @param iterations Define how accurate the calculation will be. A value of 5 should be high enough for most purposes.
	* @param start A spline parameter from 0 to 1 that specifies the lower bound for the numeric search. (default is 0.0)
	* @param end A spline parameter from 0 to 1 that specifies the upper bound for the numeric search. (default is 1.0)
	* @param step Specifies the step between two sample points on the spline for the 1st iteration. (default is 0.01) 
    * @return Returns the closest point to plane on the spline.
	*/
	public float GetClosestPointParamToPlane( Plane plane, int iterations, float start = 0, float end = 1, float step = .01f )
	{
		return GetClosestPointParamIntern( (splinePos) => plane.GetDistanceToPoint( splinePos ), iterations, start, end, step );
	}
	
	private float GetClosestPointParamIntern( DistanceFunction distFnc, int iterations, float start, float end, float step )
	{
		iterations = Mathf.Clamp( iterations, 0, 5 );
		
		float minParam = GetClosestPointParamOnSegmentIntern( distFnc, start, end, step );
		
		for( int i = 0; i < iterations; i++ )
		{
			float searchOffset = Mathf.Pow( 10f, -(i+2f) );
			
			start = Mathf.Clamp01( minParam-searchOffset );
			end = Mathf.Clamp01( minParam+searchOffset );
			step = searchOffset * .1f;
			
			minParam = GetClosestPointParamOnSegmentIntern( distFnc, start, end, step );
		}
		
		return minParam;
	}
	
	private float GetClosestPointParamOnSegmentIntern( DistanceFunction distFnc, float start, float end, float step )
	{
		float minDistance = Mathf.Infinity;
		float minParam = 0f;
		
		for( float param = start; param <= end; param += step )
		{
			float distance = distFnc( GetPositionOnSpline( param ) );
			
			if( minDistance > distance )
			{
				minDistance = distance;
				minParam = param;
			}
		}
		
		return minParam;
	}
	
	private delegate float DistanceFunction( Vector3 splinePos );
}
