using UnityEngine;

using System;

/**
* @class SplineSegment
*
* @brief This class represents a pair of two control nodes that define a segment on the Spline.
*
* A spline segment is represented by two control nodes. This class stores two references to such nodes and provides
* useful functions that allow you to convert a parameter that represents a point on the segment to a normalized 
* spline parameter that represents the same point on the spline. 
* This class becomes quite useful when handling Bézier curves!
*/ 
public class SplineSegment
{
	private readonly Spline parentSpline;
	private readonly SplineNode startNode;
	private readonly SplineNode endNode; 
	
	public Spline ParentSpline { get{ return parentSpline; } }  	///< Returns a reference to the containing spline.
	public SplineNode StartNode { get{ return startNode; } } 		///< Returns a reference to the spline segment's start point.
	public SplineNode EndNode { get{ return endNode; } } 			///< Returns a reference to the spline segment's end point.
	
	public float Length { get { return (float) (startNode.length * parentSpline.Length); } } 	///< Returns the actual length of the spline segment.
	public float NormalizedLength { get { return (float)startNode.length; } }  					///< Returns the normlaized length of the segment in the spline.
	
	/** 
	* Constructor
    * @param pSpline The spline that contains the segment.
    * @param sNode The segment's start node.
    * @param eNode The segment's end node.
	*/
	public SplineSegment( Spline pSpline, SplineNode sNode, SplineNode eNode )
	{
		if( pSpline != null )
		{
			parentSpline = pSpline;
			
			startNode = sNode;
			endNode = eNode;
		}
		else
		{
			throw new ArgumentNullException( "pSpline" );
		}
	}
	
	/** 
	* This method converts a parameter [0..1] representing a point on the segment to a normalized parameter [0..1] representing a point on the whole spline.
    * @param param The normalized segment parameter.
    * @return Returns a normalized spline parameter.
	*/
	public float ConvertSegmentToSplineParamter( float param )
	{
		return (float) (startNode.posInSpline + param * startNode.length);
	}
	
	/** 
	* This method converts a parameter [0..1] representing a point on the whole spline to a normalized parameter [0..1] representing a point on the segment.
    * @param param The normalized spline parameter.
    * @return Returns a normalized segment parameter.
	*/
	public float ConvertSplineToSegmentParamter( float param )
	{
		if( param < startNode.posInSpline )
			return 0;
		
		if( param >= endNode.posInSpline )
			return 1;
		
		return (float) ( (param - startNode.posInSpline) / startNode.length );
	}
	
	/** 
	* This method clamps a normalized spline parameter to spline parameters defining the segment. The returned parameter will only represent points on the segment.
    * @param param A normalized spline parameter.
    * @return Returns a clamped spline parameter that will only represent points on the segment.
	*/
	public float ClampParameterToSegment( float param )
	{
		if( param < startNode.posInSpline )
			return (float) startNode.posInSpline;
		
		if( param >= endNode.posInSpline )
			return (float) endNode.posInSpline;
		
		return param;
	}
}
