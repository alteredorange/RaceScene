using UnityEngine;

using System;
using System.Collections.Generic;

using SplineUtilities;

/**
* @class Spline
*
* @brief The Spline class represents three-dimensional curves.
*
* It provides the most important functions that are necessary to create, calculate and render Splines.
* The class derives from MonoBehaviour so it can be attached to gameObjects and used like any other self-written script.
*/ 

[AddComponentMenu("SuperSplines/Spline")]
public partial class Spline : MonoBehaviour
{
	public List<SplineNode> splineNodesArray = new List<SplineNode>( ); 									///< A collection of SplineNodes that are used as control nodes.
	
	public InterpolationMode interpolationMode = InterpolationMode.Hermite; 								///< Specifies what kind of curve interpolation will be used.
	public RotationMode rotationMode = RotationMode.Tangent; 												///< Specifies how to calculate rotations on the spline.
	public TangentMode tangentMode = TangentMode.UseTangents; 												///< Specifies how tangents are calculated in hermite mode.
	
	public AutomaticUpdater autoUpdate = new AutomaticUpdater( );											///< Specifies when the spline will be updated.
	
	public Vector3 tanUpVector = Vector3.up; 																///< Up-Vector used for calculation of rotations (only needed for RotationMode.Tangent).
	public float tension = 0.5f; 																			///< Curve Tension (only has an effect on Hermite splines).
	public bool autoClose = false; 																			///< If set to true the spline start and end points of the spline will be connected. (Note that Bézier-Curves can't be auto-closed!)
	public int interpolationAccuracy = 5; 																	///< Defines how accurately numeric calculations will be done.
	
	private List<SplineNode> splineNodesInternal = new List<SplineNode>( );
	private LengthData lengthData = new LengthData( );
	
	public float Length{ get { return (float) lengthData.length; } } 										///< Returns the length of the spline in game units.
	public bool AutoClose{ get{ return autoClose && !IsBezier; } } 											///< Returns true if spline is auto-closed. If the spline is a Bézier-Curve, false will always be returned.
	public int Step{ get{ return IsBezier ? 3 : 1; } }														///< Returns the number of spline nodes that are needed to describe a spline segment.
	public int SegmentCount{ get { return (ControlNodeCount-1)/Step; } } 									///< Returns the number of spline segments. (Note that a spline segment of a Bézier-Curve is defined by 4 control nodes!)
	public int ControlNodeCount{ get { return AutoClose ? splineNodesInternal.Count + 1 : splineNodesInternal.Count; } }	///< Returns the number of control nodes that are used internally.
	
	private double InvertedAccuracy{ get{ return 1.0 / interpolationAccuracy; } }
	private bool IsBezier{ get{ return interpolationMode == InterpolationMode.Bezier; } }
	private bool HasNodes{ get{ return splineNodesInternal == null ? false : splineNodesInternal.Count > 0; } }
	
	public SplineNode[] SplineNodes
	{
		get{ 
			if( splineNodesInternal == null ) 
				splineNodesInternal = new List<SplineNode>( );
			
			return splineNodesInternal.ToArray( ); 
		} 
	} ///< Returns an array containing all relevant control nodes that are used internally. It might differ from the values in the splineNodesArray.
	
	public SplineSegment[] SplineSegments
	{
		get {
			SplineSegment[] sSegments = new SplineSegment[SegmentCount];
			
			for( int i = 0; i < sSegments.Length; i++ )
				sSegments[i] = new SplineSegment( this, GetNode( i*Step, 0 ), GetNode( i*Step, 1 ) );
			
			return sSegments;
		}
	} ///< Returns an array containing the spline's segments. 
	
	
	public void OnEnable( ) 
	{
		UpdateSpline( );
	}
	
	public void LateUpdate( ) 
	{
		if( autoUpdate.Update( ) )
			UpdateSpline( );
	}
	
	/** 
	* This function updates the spline. It is called automatically once in a while, if updateMode isn't set to DontUpdate.
	*/
	public void UpdateSpline( )
	{
		splineNodesArray.Remove( null );
		
		for( int i = 0; i < splineNodesArray.Count; i++ )
			splineNodesArray[i].spline = this;
		
		int relevantNodeCount = GetRelevantNodeCount( splineNodesArray.Count );
		
		if( splineNodesInternal == null )
			splineNodesInternal = new List<SplineNode>( );
		
		splineNodesInternal.Clear( );
		
		if( !EnoughNodes( relevantNodeCount ) )
			return;
		
		splineNodesInternal.AddRange( splineNodesArray.GetRange( 0, relevantNodeCount ) );
		
		ReparameterizeCurve( );
	}
	
	
	/** 
	* This function returns a point on the spline for a parameter between 0 and 1
	* @param param A normalized spline parameter ([0..1]).
    * @return Returns a point on the spline.
	*/
	public Vector3 GetPositionOnSpline( float param )
	{
		if( !HasNodes )
			return Vector3.zero;
		
		return GetPositionInternal( RecalculateParameter( param ) );
	}
	
	/** 
	* This function returns a tangent to the spline for a parameter between 0 and 1
	* @param param A normalized spline parameter ([0..1]).
    * @return Returns a tangent to the spline.
	*/
	public Vector3 GetTangentToSpline( float param )
	{
		if( !HasNodes )
			return Vector3.zero;
		
		return GetTangentInternal( RecalculateParameter( param ) );
	}
	
	/** 
	* This function returns a rotation on the spline for a parameter between 0 and 1
	* @param param A normalized spline parameter ([0..1]).
    * @return Returns a rotation on the spline.
	*/
	public Quaternion GetOrientationOnSpline( float param )
	{
		if( !HasNodes )
			return Quaternion.identity;
		
		switch( rotationMode )
		{
		case RotationMode.Tangent:
			Vector3 tangent = GetTangentToSpline( param );
			
			if( tangent.sqrMagnitude == 0f || tanUpVector.sqrMagnitude == 0f )
				return Quaternion.identity;
			
			return Quaternion.LookRotation( tangent.normalized, tanUpVector.normalized );
			
		case RotationMode.Node:
			return GetRotationInternal( RecalculateParameter( param ) );
			
		default:
			return Quaternion.identity;
		}
		
	}
	
	/** 
	* This function returns an interpolated custom value on the spline for a parameter between 0 and 1.
	* The control values can be set in the SplineNode inspector or in the SplineNode script. These control values will be interpolated just like 
	* the SplineNodes' control positions are. Depending on the used interpolation mode, the actual control values won't be elements of the set of the interpolated values. 
	* Such a behaviour applies to B-splines for example. Just like the B-spline doesn't necessarily contain all control positions, its interpolated
	* custom values don't necessarily contain all custom control values.
	* @param param A normalized spline parameter ([0..1]).
    * @return Returns an interpolated custom value on the spline.
	*/
	public float GetCustomValueOnSpline( float param )
	{
		if( !HasNodes )
			return 0f;
		
		return (float) GetValueInternal( RecalculateParameter( param ) );
	}
	
	/** 
	* This function returns a spline segment that contains the point on the spline that is defined by a normalized parameter.
	* @param param A normalized spline parameter ([0..1]).
    * @return Returns a spline segment containing the point corresponding to param.
	*/
	public SplineSegment GetSplineSegment( float param )
	{
		param = Mathf.Clamp( param, 0, 1f );
		
		for( int i = 0; i < ControlNodeCount-1; i += Step )
		{
			if( param - splineNodesInternal[i].posInSpline < splineNodesInternal[i].Length )
				return new SplineSegment( this, GetNode( i, 0 ), GetNode( i, Step ) );
		}
		
		return new SplineSegment( this, GetNode( MaxNodeIndex( ), 0 ), GetNode( MaxNodeIndex( ), Step ) );
	}
	
	/** 
	* This function converts a normalized spline parameter to the actual distance to the spline's start point.
	* @param param A normalized spline parameter ([0..1]).
    * @return Returns the actual distance from the start point to the point defined by param.
	*/
	public float ConvertNormalizedParameterToDistance( float param )
	{
		return Length * param;
	}
	
	/** 
	* This function converts an actual distance from the spline's start point to normalized spline parameter.
	* @param param A specific distance on the spline (must be less or equal to the spline length).
    * @return Returns a normalized spline parameter based on the distance from the splines start point.
	*/
	public float ConvertDistanceToNormalizedParameter( float param )
	{
		return (Length <= 0f) ? 0f : param/Length;
	}
	
	//Recalculate the spline parameter for constant-velocity interpolation
	private SegmentParameter RecalculateParameter( double param )
	{
		if( param <= 0 )
			return new SegmentParameter( 0, 0 );
		if( param > 1 )
			return new SegmentParameter( MaxNodeIndex( ), 1 );
		
		double invertedAccuracy = InvertedAccuracy;
		
		for( int i = lengthData.subSegmentPosition.Length - 1; i >= 0; i-- )
		{
			if( lengthData.subSegmentPosition[i] < param )
			{
				int floorIndex = (i - (i % (interpolationAccuracy)));
				
				int normalizedIndex = floorIndex * Step / interpolationAccuracy;
				double normalizedParam = invertedAccuracy * (i-floorIndex + (param - lengthData.subSegmentPosition[i]) / lengthData.subSegmentLength[i]);
				
				if( normalizedIndex >= ControlNodeCount - 1 )
					return new SegmentParameter( MaxNodeIndex( ), 1.0 );
				
				return new SegmentParameter( normalizedIndex, normalizedParam );
			}
		}
	
		return new SegmentParameter( MaxNodeIndex( ), 1 );
	}
	
	private SplineNode GetNode( int idxNode, int idxOffset )
	{
		idxNode += idxOffset;
		
		if( AutoClose )
			return splineNodesInternal[ (idxNode % splineNodesInternal.Count + splineNodesInternal.Count) % splineNodesInternal.Count ];
		else
			return splineNodesInternal[ Mathf.Clamp( idxNode, 0, splineNodesInternal.Count-1 ) ];
	}
	
	private void ReparameterizeCurve( )
	{
		if( lengthData == null )
			lengthData = new LengthData( );
		
		lengthData.Calculate( this );
	}
	
	private int MaxNodeIndex( )
	{
		return ControlNodeCount - Step - 1;
	}
	
	private int GetRelevantNodeCount( int nodeCount )
	{
		int relevantNodeCount = nodeCount;
		
		if( IsBezier )
		{
			if( nodeCount < 7 )
				relevantNodeCount -= (nodeCount) % 4;
			else
				relevantNodeCount -= (nodeCount - 4) % 3;
		}
		
		return relevantNodeCount;
	}
	
	private bool EnoughNodes( int nodeCount )
	{
		if( IsBezier )
			return !(nodeCount < 4 );
		else
			return !(nodeCount < 2);
	}
	
	private sealed class SegmentParameter
	{
		public double normalizedParam;
		public int normalizedIndex;
		
		public SegmentParameter( )
		{
			normalizedParam = 0;
			normalizedIndex = 0;
		}
		
		public SegmentParameter( int index, double param )
		{
			normalizedParam = param;
			normalizedIndex = index;
		}
	}
	
	private sealed class LengthData
	{
		public double[] subSegmentLength;
		public double[] subSegmentPosition;
		
		public double length;
		
		public void Calculate( Spline spline )
		{
			int subsegmentCount = spline.SegmentCount * spline.interpolationAccuracy;
			double invertedAccuracy = 1.0 / spline.interpolationAccuracy;
			
			subSegmentLength = new double[subsegmentCount];
			subSegmentPosition = new double[subsegmentCount];
		
			length = 0.0;
			
			for( int i = 0; i < subsegmentCount; i++ )
			{
				subSegmentLength[i] = 0.0;
				subSegmentPosition[i] = 0.0;
			}
			
			for( int i = 0; i < spline.SegmentCount; i++ )
			{
				for( int j = 0; j < spline.interpolationAccuracy; j++ )
				{
					int index = i * spline.interpolationAccuracy + j;
					
					subSegmentLength[index] = spline.GetSegmentLengthInternal( i * spline.Step, j*invertedAccuracy, (j+1)*invertedAccuracy, 0.2 * invertedAccuracy );
					
					length += subSegmentLength[index];
				}
			}
			
			for( int i = 0; i < spline.SegmentCount; i++ )
			{
				for( int j = 0; j < spline.interpolationAccuracy; j++ )
				{
					int index = i*spline.interpolationAccuracy+j;
					
					subSegmentLength[index] /= length;
					
					if( index < subSegmentPosition.Length-1 )
						subSegmentPosition[index+1] = subSegmentPosition[index] + subSegmentLength[index];
				}
			}
			
			SetupSplinePositions( spline );
		}
		
		private void SetupSplinePositions( Spline spline )
		{
			foreach( SplineNode node in spline.splineNodesInternal )
				node.Reset( );
			
			for( int i = 0; i < subSegmentLength.Length; i++ )
				spline.splineNodesInternal[((i - (i % spline.interpolationAccuracy))/spline.interpolationAccuracy) * spline.Step].length += subSegmentLength[i];
			
			for( int i = 0; i < spline.splineNodesInternal.Count-spline.Step; i+=spline.Step )
				spline.splineNodesInternal[i+spline.Step].posInSpline = spline.splineNodesInternal[i].posInSpline + spline.splineNodesInternal[i].Length;
			
			if( spline.IsBezier )
			{	
				for( int i = 0; i < spline.splineNodesInternal.Count-spline.Step; i+=spline.Step )
				{
					spline.splineNodesInternal[i+1].posInSpline = spline.splineNodesInternal[i].posInSpline;
					spline.splineNodesInternal[i+2].posInSpline = spline.splineNodesInternal[i].posInSpline;
					spline.splineNodesInternal[i+1].length = 0.0;
					spline.splineNodesInternal[i+2].length = 0.0;
				}
			}
			
			if( !spline.AutoClose )
				spline.splineNodesInternal[spline.splineNodesInternal.Count-1].posInSpline = 1.0;
		}
	}
	
	/**
	* @enum TangentMode
	* Specifies how tangents of control points should be calculated. Note that this will only affect Hermite-Splines.
	*/ 
	public enum TangentMode 
	{ 
		UseNormalizedTangents, ///< Use the normalized vector that connects the two adjacent control nodes as tangent (see UseTangents).
		UseTangents, ///< Use the vector that connects the two adjacent control nodes as tangent.
		UseNodeForwardVector ///< Use the forward vector which depends on the control node's rotation.
	}
	
	/**
	* @enum RotationMode
	* Specifies how rotations will be interpolated over the spline.
	*/ 
	public enum RotationMode 
	{ 
		None, ///< No rotation (Quaternion.identity).
		Node, ///< Interpolate the control nodes' orientation.
		Tangent ///< Use the tangent to calculate the rotation on the spline.
	}
	
	/**
	* @enum InterpolationMode
	* Specifies the type of spline interpolation that will be used.
	*/ 
	public enum InterpolationMode 
	{
		Hermite, ///< Hermite Spline
		Bezier, ///< Bézier Spline
		BSpline, ///< B-Spline
		Linear /// < Linear Interpolation
	}
}
