using UnityEngine;
using System;

namespace SplineUtilities
{
	/**
	 * @class AutomaticUpdater
	 *
	 * @brief A helper class that is used by the Spline class and the SplineMesh class.
	 *
	 * This class specifies when a spline or a spline mesh will be updated. 
	 */ 
	[Serializable]
	public class AutomaticUpdater
	{
		public UpdateMode mode;							///< Specifies whether an object will be updated after seconds, frames or won't be updated at all.
		
		public float deltaSeconds = 0.1f; 				///< Specifies after how much time the spline will be updated (see UpdateMode).
		public int deltaFrames = 2; 					///< Specifies after how many frames the spline will be updated (see UpdateMode).
		
		private float passedTime;
		
		/** 
		* This returns true when an object needs to be updated. 
		*/
		public bool Update( )
		{
			switch( mode )
			{
			case UpdateMode.EveryFrame:
				return true;
				
			case UpdateMode.EveryXFrames:
				if( Time.frameCount % deltaFrames == 0 )
					return true;
				
				return false;
				
			case UpdateMode.EveryXSeconds:
				passedTime += Time.deltaTime;
				
				if( passedTime >= deltaSeconds )
				{
					passedTime = 0f;
					return true;
				}
				
				return false;
			}
			
			return false;
		}
		
		/**
		 * @enum UpdateMode
		 * Specifies when to update and recalculate a spline or a spline mesh.
		 */ 
		public enum UpdateMode
		{
			DontUpdate, 	///< Keeps the spline / spline mesh static. It will only be updated when the component becomes enabled (OnEnable( )).
			EveryFrame, 	///< Updates the spline / spline mesh every frame.
			EveryXFrames, 	///< Updates the spline / spline mesh every x frames.
			EveryXSeconds 	///< Updates the spline / spline mesh every x seconds.
		}
	}
	
	/**
	 * @class SplineUtils
	 *
	 * @brief A helper class that contains useful functions for general calculations related to splines.
	 */ 
	public static class SplineUtils
	{
		/** 
		* This function 'normalizes' an arbitrary value to a particular interval.
		* @param v An arbitrary value.
		* @param start Lower bound of the target interval.
		* @param end Upper bound of the target interval.
		* @param wMode Specifies how the value v will be normalized to the interval given by start and end.
	    * @return Returns a parameter that is within the bounds of the interval [start, end].
		*/
		public static float WrapValue( float v, float start, float end, WrapMode wMode )
		{
			switch( wMode )
			{
			case WrapMode.Clamp:
			case WrapMode.ClampForever:
				return Mathf.Clamp( v, start, end );
			case WrapMode.Default:
			case WrapMode.Loop:
				return Mathf.Repeat( v, end - start ) + start;
			case WrapMode.PingPong:
				return Mathf.PingPong( v, end - start ) + start;
			default:
				return v;
			}
		}
	}
}