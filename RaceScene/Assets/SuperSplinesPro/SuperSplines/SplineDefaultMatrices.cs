using UnityEngine;

using System;
using System.Collections.Generic;

public partial class Spline : MonoBehaviour
{
	public static readonly double[] HermiteMatrix = new double[] {
		 2.0, -3.0,  0.0,  1.0,
		-2.0,  3.0,  0.0,  0.0,
		 1.0, -2.0,  1.0,  0.0,
		 1.0, -1.0,  0.0,  0.0
	}; ///< The coefficients matrix for hermite splines
	
	public static readonly double[] BezierMatrix = new double[] {
		-1.0,  3.0, -3.0,  1.0,
		 3.0, -6.0,  3.0,  0.0,
		-3.0,  3.0,  0.0,  0.0,
		 1.0,  0.0,  0.0,  0.0
	}; ///< The coefficients matrix for bezier splines
	
	public static readonly double[] BSplineMatrix = new double[] {
		-1.0/6.0,   3.0/6.0, - 3.0/6.0,  1.0/6.0,
		 3.0/6.0, - 6.0/6.0,   0.0/6.0,  4.0/6.0,
		-3.0/6.0,   3.0/6.0,   3.0/6.0,  1.0/6.0,
		 1.0/6.0,   0.0/6.0,   0.0/6.0,  0.0/6.0
	}; ///< The coefficients matrix for b-splines
	
	public static readonly double[] LinearMatrix = new double[] {
		0.0,   0.0, - 1.0,  1.0,
		0.0,   0.0,   1.0,  0.0,
		0.0,   0.0,   0.0,  0.0,
		0.0,   0.0,   0.0,  0.0
	}; ///< The coefficients matrix for linear paths
}
