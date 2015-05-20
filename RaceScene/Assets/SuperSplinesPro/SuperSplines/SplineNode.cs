using UnityEngine;
using System.Collections;

/**
* @class SplineNode
*
* @brief This class represents a control node of the Spline.
*
* This class stores data about the position and orientation of the control node as well as information about 
* the spline parameter that is associated to the control node and the normalized distance to the next adjacent control node.
* For advanced use there is also a custom value that will be interpolated according to the interpolation mode that is used to calculate the spline.
*/ 
public class SplineNode : MonoBehaviour 
{
	public Transform Transform{ get{ return transform; } } 												///< Reference to the node's Transform in the scene.
	
	public Vector3 Position { get{ return Transform.position; } set{ Transform.position = value; } } 	///< Quick access to the control node's position.
	public Quaternion Rotation { get{ return Transform.rotation; } set{ Transform.rotation = value; } } ///< Quick access to the control node's orientation.
	
	public float PosInSpline{ get{ return (float) posInSpline; } } 										///< Normalized position on the spline (parameter from 0 to 1).
	public float Length{ get{ return (float) length; } } 												///< Normalized distance to the next adjacent node.
	
	public double posInSpline = 0f;
	public double length = 0f; 
	
	public float customValue = 0f;									///< A custom value that can be interpolated by the Spline-class, for advanced applications
	
	public Spline spline;											///< A reference to the spline that contains this control node
	
	public void Reset( )
	{
		posInSpline = 0f;
		length = 0f;
	}
}
