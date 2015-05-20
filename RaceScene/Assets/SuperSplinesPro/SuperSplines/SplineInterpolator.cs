using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class Spline : MonoBehaviour 
{
	private Vector3 GetPositionInternal( SegmentParameter sParam )
	{
		SplineNode n0;
		SplineNode n1;
		SplineNode n2;
		SplineNode n3;
		
		GetAdjacentNodes( sParam, out n0, out n1, out n2, out n3 );
		
		Vector3 P0 = n0.Position; 
		Vector3 P1 = n1.Position;
		Vector3 P2 = n2.Position; 
		Vector3 P3 = n3.Position;
		
		RecalcVectors( n0, n1, ref P2, ref P3 );
			
		return InterpolatePosition( sParam.normalizedParam, P0, P1, P2, P3 );
	}
	
	private Vector3 GetTangentInternal( SegmentParameter sParam )
	{
		SplineNode n0;
		SplineNode n1;
		SplineNode n2;
		SplineNode n3;
		
		GetAdjacentNodes( sParam, out n0, out n1, out n2, out n3 );
		
		Vector3 P0 = n0.Position; 
		Vector3 P1 = n1.Position;
		Vector3 P2 = n2.Position; 
		Vector3 P3 = n3.Position;
		
		RecalcVectors( n0, n1, ref P2, ref P3 );
		
		return InterpolateTangent( sParam.normalizedParam, P0, P1, P2, P3 );
	}
	
	private double GetValueInternal( SegmentParameter sParam )
	{
		SplineNode n0;
		SplineNode n1;
		SplineNode n2;
		SplineNode n3;
		
		GetAdjacentNodes( sParam, out n0, out n1, out n2, out n3 );
		
		double P0 = n0.customValue; 
		double P1 = n1.customValue;
		double P2 = n2.customValue; 
		double P3 = n3.customValue;
		
		RecalcScalars( n0, n1, ref P2, ref P3 );
		
		return InterpolateValue( sParam.normalizedParam, P0, P1, P2, P3 );
	}
	
	private Quaternion GetRotationInternal( SegmentParameter sParam )
	{
		Quaternion Q0 = GetNode( sParam.normalizedIndex,-1 ).Transform.rotation;
		Quaternion Q1 = GetNode( sParam.normalizedIndex, 0 ).Transform.rotation;
		Quaternion Q2 = GetNode( sParam.normalizedIndex, 1 ).Transform.rotation;
		Quaternion Q3 = GetNode( sParam.normalizedIndex, 2 ).Transform.rotation;
		
		Quaternion T1 = GetSquadIntermediate( Q0, Q1, Q2 );
		Quaternion T2 = GetSquadIntermediate( Q1, Q2, Q3 );
		
		return GetQuatSquad( sParam.normalizedParam, Q1, Q2, T1, T2 );
	}
	
	private Vector3 InterpolatePosition( double t, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3 )
	{
		double t2 = t * t;
		double t3 = t2 * t;
		
		double b1; double b2;
		double b3; double b4;
		
		double[] matrix = GetMatrix( interpolationMode );
		
		b1 = matrix[ 0] * t3 + matrix[ 1] * t2 + matrix[ 2] * t + matrix[ 3];
		b2 = matrix[ 4] * t3 + matrix[ 5] * t2 + matrix[ 6] * t + matrix[ 7];
		b3 = matrix[ 8] * t3 + matrix[ 9] * t2 + matrix[10] * t + matrix[11];
		b4 = matrix[12] * t3 + matrix[13] * t2 + matrix[14] * t + matrix[15];
		
		return new Vector3( (float) (b1 * v0.x + b2 * v1.x + b3 * v2.x + b4 * v3.x), 
		                   	(float) (b1 * v0.y + b2 * v1.y + b3 * v2.y + b4 * v3.y), 
		                   	(float) (b1 * v0.z + b2 * v1.z + b3 * v2.z + b4 * v3.z) );
	}
	
	private Vector3 InterpolateTangent( double t, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3 )
	{
		double t2 = t * t;
		
		double b1; double b2;
		double b3; double b4;
		
		double[] matrix = GetMatrix( interpolationMode );
		
		t = t * 2.0;
		t2 = t2 * 3.0;
		
		b1 = matrix[ 0] * t2 + matrix[ 1] * t + matrix[ 2];
		b2 = matrix[ 4] * t2 + matrix[ 5] * t + matrix[ 6];
		b3 = matrix[ 8] * t2 + matrix[ 9] * t + matrix[10];
		b4 = matrix[12] * t2 + matrix[13] * t + matrix[14];
		
		return new Vector3( (float) (b1 * v0.x + b2 * v1.x + b3 * v2.x + b4 * v3.x), 
		                   	(float) (b1 * v0.y + b2 * v1.y + b3 * v2.y + b4 * v3.y), 
		                   	(float) (b1 * v0.z + b2 * v1.z + b3 * v2.z + b4 * v3.z) );
	}
	
	private double InterpolateValue( double t, double v0, double v1, double v2, double v3 )
	{
		double t2 = t * t;
		double t3 = t2 * t;
		
		double b1; double b2;
		double b3; double b4;
		
		double[] matrix = GetMatrix( interpolationMode );
		
		b1 = matrix[ 0] * t3 + matrix[ 1] * t2 + matrix[ 2] * t + matrix[ 3];
		b2 = matrix[ 4] * t3 + matrix[ 5] * t2 + matrix[ 6] * t + matrix[ 7];
		b3 = matrix[ 8] * t3 + matrix[ 9] * t2 + matrix[10] * t + matrix[11];
		b4 = matrix[12] * t3 + matrix[13] * t2 + matrix[14] * t + matrix[15];
		
		return b1 * v0 + b2 * v1 + b3 * v2 + b4 * v3;
	}
	
	private void GetAdjacentNodes( SegmentParameter sParam, out SplineNode node0, out SplineNode node1, out SplineNode node2, out SplineNode node3 )
	{
		switch( interpolationMode )
		{
		case InterpolationMode.BSpline:
			node0 = GetNode( sParam.normalizedIndex,-1 );
			node1 = GetNode( sParam.normalizedIndex, 0 );
			node2 = GetNode( sParam.normalizedIndex, 1 );
			node3 = GetNode( sParam.normalizedIndex, 2 );
			
			return;
			
		case InterpolationMode.Hermite:
			node0 = GetNode( sParam.normalizedIndex, 0 );
			node1 = GetNode( sParam.normalizedIndex, 1 );
			node2 = GetNode( sParam.normalizedIndex,-1 );
			node3 = GetNode( sParam.normalizedIndex, 2 );
			
			return;
			
		case InterpolationMode.Linear:
		case InterpolationMode.Bezier:
		default:
			node0 = GetNode( sParam.normalizedIndex, 0 );
			node1 = GetNode( sParam.normalizedIndex, 1 );
			node2 = GetNode( sParam.normalizedIndex, 2 );
			node3 = GetNode( sParam.normalizedIndex, 3 );
			
			return;
		}
	}
	
	private void RecalcVectors( SplineNode node0, SplineNode node1, ref Vector3 P2, ref Vector3 P3 )
	{
		if( interpolationMode != InterpolationMode.Hermite )
			return;
		
		if( tangentMode == TangentMode.UseNodeForwardVector )
		{
			P2 = node0.Transform.forward * tension;
			P3 = node1.Transform.forward * tension;
		}
		else
		{
			P2 = node1.Position - P2;
			P3 = P3 - node0.Position;
			
			if( tangentMode != TangentMode.UseTangents )
			{
				P2.Normalize( );
				P3.Normalize( );
			}
			
			P2 = P2 * tension;
			P3 = P3 * tension;
		}
	}
	
	private void RecalcScalars( SplineNode node0, SplineNode node1, ref double P2, ref double P3 )
	{
		if( interpolationMode != InterpolationMode.Hermite )
			return;
		
		P2 = node1.customValue - P2;
		P3 = P3 - node0.customValue;
			
		P2 = P2 * tension;
		P3 = P3 * tension;
	}
	
	//Approximate the length of a spline segment numerically
	private double GetSegmentLengthInternal( int idxFirstPoint, double startValue, double endValue, double step )
	{
		Vector3 currentPos;
		
		double pPosX; double pPosY; double pPosZ;
		double cPosX; double cPosY; double cPosZ;
		
		double length = 0;
		
		currentPos = GetPositionInternal( new SegmentParameter( idxFirstPoint, startValue ) );
		
		pPosX = currentPos.x;
		pPosY = currentPos.y;
		pPosZ = currentPos.z;
		
		for( double f = startValue + step; f < (endValue + step * 0.5); f += step )
		{
			currentPos = GetPositionInternal( new SegmentParameter( idxFirstPoint, f ) );
			
			cPosX = pPosX - currentPos.x;
			cPosY = pPosY - currentPos.y;
			cPosZ = pPosZ - currentPos.z;
			
			length += Math.Sqrt( cPosX*cPosX + cPosY*cPosY + cPosZ*cPosZ );
			
			pPosX = currentPos.x;
			pPosY = currentPos.y;
			pPosZ = currentPos.z;
		}
		
		return length;
	}
	
	//Returns a reference to the coefficient matrix that shall be used
	private double[] GetMatrix( InterpolationMode iMode )
	{
		switch( iMode )
		{
		case InterpolationMode.Bezier:
			return BezierMatrix;
		case InterpolationMode.BSpline:
			return BSplineMatrix;
		case InterpolationMode.Hermite:
			return HermiteMatrix;
		case InterpolationMode.Linear:
			return LinearMatrix;
			
		default:
			return LinearMatrix;
		}
	}
	
}
