using UnityEngine;
using UnityEditor;

using SplineEditorExtensions;

public partial class SplineEditor : Editor
{
	[MenuItem( "GameObject/Create Other/Spline/Hermite" )]
	static void CreateHermiteSpline( )
	{
		Undo.RegisterSceneUndo( "Create new spline" );
		
		GameObject gObject = new GameObject( );
		
		gObject.name = "New Spline";
		
		gObject.transform.localPosition = Vector3.zero;
		gObject.transform.localRotation = Quaternion.identity;
		gObject.transform.localScale = Vector3.one;
		
		Spline spline = gObject.AddComponent<Spline>( );
		
		spline.interpolationMode = Spline.InterpolationMode.Hermite;
		
		SetupChildren( spline );
		
		Selection.activeGameObject = gObject;
	}
	
	[MenuItem( "GameObject/Create Other/Spline/Bezier" )]
	static void CreateBezierSpline( )
	{
		Undo.RegisterSceneUndo( "Create new spline" );
		
		GameObject gObject = new GameObject( );
		
		gObject.name = "New Spline";
		
		gObject.transform.localPosition = Vector3.zero;
		gObject.transform.localRotation = Quaternion.identity;
		gObject.transform.localScale = Vector3.one;
		
		Spline spline = gObject.AddComponent<Spline>( );
		
		spline.interpolationMode = Spline.InterpolationMode.Bezier;
		
		SetupChildren( spline );
		
		Selection.activeGameObject = gObject;
	}
	
	[MenuItem( "GameObject/Create Other/Spline/B-Spline" )]
	static void CreateBSpline( )
	{
		Undo.RegisterSceneUndo( "Create new spline" );
		
		GameObject gObject = new GameObject( );
		
		gObject.name = "New Spline";
		
		gObject.transform.localPosition = Vector3.zero;
		gObject.transform.localRotation = Quaternion.identity;
		gObject.transform.localScale = Vector3.one;
		
		Spline spline = gObject.AddComponent<Spline>( );
		
		spline.interpolationMode = Spline.InterpolationMode.BSpline;
		
		SetupChildren( spline );
		
		Selection.activeGameObject = gObject;
	}
	
	private static void SetupChildren( Spline spline )
	{
		GameObject[] childs = new GameObject[4];
		
		for( int i = 0; i < childs.Length; i++ )
		{
			childs[i] = new GameObject( );
			childs[i].name = GetNodeName( i );
			childs[i].transform.parent = spline.transform;
			childs[i].transform.localPosition = -Vector3.forward * 1.5f + Vector3.forward * i;
			childs[i].transform.localRotation = Quaternion.identity;
			childs[i].transform.localScale = Vector3.one;
			
			SplineNode splineNode = childs[i].AddComponent<SplineNode>( );
			
			spline.splineNodesArray.Add( splineNode );
		}
	}
	
	private static string GetNodeName( int num )
	{
		string res = "";
		
		for( int i = 1; i<4; i++ )
			if( num < Mathf.Pow( 10, i ) )
				res += "0";
		
		return( res + num.ToString( ) );
	}
}
