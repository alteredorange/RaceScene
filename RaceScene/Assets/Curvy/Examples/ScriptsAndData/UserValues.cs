using UnityEngine;
using System.Collections;
using FluffyUnderware.Curvy;

/*
 * This script demonstrates the usage of UserValues:
 * 
 * Here we use the x value of the UserValue to scale the cube
 * 
 */


namespace FluffyUnderware.Curvy.Examples
{
    public class UserValues : MonoBehaviour {
    
        FollowSpline walkerScript;
        Material mMat;

	    // Use this for initialization
	    void Start () {
            walkerScript = GetComponent<FollowSpline>();
            mMat = GetComponent<Renderer>().material;
	    }
	
	    // Update is called once per frame
	    void Update () {
            if (walkerScript && walkerScript.Spline.IsInitialized) {
                // Scale is interpolated from the Control Point's scale
                transform.localScale = walkerScript.Spline.InterpolateScale(walkerScript.CurrentTF);
                // Color is stored as Vector3 in the UserValues array. We transform it back and set the material's color
                mMat.color = Vector3ToColor(walkerScript.Spline.InterpolateUserValue(walkerScript.CurrentTF, 0));
            }
	    }

        Color Vector3ToColor(Vector3 v)
        {
            return new Color(v.x, v.y, v.z);
        }
    }
}