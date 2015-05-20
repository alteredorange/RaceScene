using UnityEngine;
using UnityEditor;

using SplineEditorExtensions;
using System.Collections.Generic;

public partial class SplineEditor : Editor
{
	private SerializedProperty interpolationModeProp;
	private SerializedProperty rotationModeProp;
	private SerializedProperty tangentModeProp;
	private SerializedProperty accuracyProp;
	private SerializedProperty upVectorProp;
	private SerializedProperty autoCloseProp;
	private SerializedProperty tensionProp;
	
	private AutoUpdaterEditor autoUpdaterProp;
	
	private static readonly string performanceInfo = "Performance Hint: " +
				"Accuracy values above 15 are only reasonable if the segment length betweeen two spline nodes exceeds 10^4 game units, " +
				"or if you need high accuracy in a small scale of less than 10^(-4) game units.";
	
	private static readonly string editingInfo = "In order to insert spline nodes at particular positions on the curve, simply right-click " +
				"somewhere near the spline while pressing the " + (Application.platform == RuntimePlatform.OSXEditor ? "Command" : "Control") + " key.";
	
	public void OnEnable( )
	{
		interpolationModeProp = serializedObject.FindProperty( "interpolationMode" );
		rotationModeProp = serializedObject.FindProperty( "rotationMode" );
		tangentModeProp = serializedObject.FindProperty( "tangentMode" );
		accuracyProp = serializedObject.FindProperty( "interpolationAccuracy" );
		tensionProp = serializedObject.FindProperty( "tension" );
		upVectorProp = serializedObject.FindProperty( "tanUpVector" );
		autoCloseProp = serializedObject.FindProperty( "autoClose" );
		
		autoUpdaterProp = new AutoUpdaterEditor( serializedObject, "autoUpdate" );
	}
	
	public override void OnInspectorGUI( )
	{
		Spline currentSpline = target as Spline;
		
		serializedObject.Update( );
		
		autoUpdaterProp.DrawInspector( );
		
		EditorGUILayout.PropertyField( interpolationModeProp, new GUIContent( "   Interpolation Mode" ) );
		
		if( (Spline.InterpolationMode) interpolationModeProp.enumValueIndex == Spline.InterpolationMode.Hermite )
			EditorGUILayout.PropertyField( tangentModeProp, new GUIContent( "   Hermite Tangent Mode" ) );
		
		EditorGUILayout.PropertyField( rotationModeProp, new GUIContent( "   Rotation Mode" ) );
		EditorGUILayout.IntSlider( accuracyProp, 1, 30, new GUIContent( "   Interpolation Accuracy" ) );
		
		if( accuracyProp.intValue > 15 )
			EditorGUILayout.HelpBox( performanceInfo, MessageType.Info );
		
		EditorGUILayout.Space( );
		
		EditorGUILayout.PropertyField( tensionProp, new GUIContent( "   Curve Tension" ) );
		
		if( (Spline.RotationMode) rotationModeProp.enumValueIndex == Spline.RotationMode.Tangent ) 
		{
			EditorGUILayout.Space( );
			EditorGUILayout.PropertyField( upVectorProp, new GUIContent( "   Up-Vector (used for tangent-based rotation)" ), true );
		}
		
		if( (Spline.InterpolationMode) interpolationModeProp.enumValueIndex != Spline.InterpolationMode.Bezier )
		{
			EditorGUILayout.Space( );
			EditorGUILayout.PropertyField( autoCloseProp, new GUIContent( "   Auto Close" ), true );
		}
		
		DrawSplineNodeArray( currentSpline );
		
		EditorGUILayout.HelpBox( editingInfo, MessageType.Info );
		EditorGUILayout.Space( );
		
		if( serializedObject.ApplyModifiedProperties( ) )
			ApplyChanges( );
	}
	
	private bool showNodes = true;
	
	private void DrawSplineNodeArray( Spline currentSpline )
	{
		showNodes = EditorGUILayout.Foldout( showNodes, "Spline Nodes" );
		
		if( !showNodes )
			return;
		
		List<SplineNode> newNodes;
		
		if( currentSpline.splineNodesArray.Count == 0 )
		{
			newNodes = DropBox( "Drag game objects containing the SplineNode-component into this box to use them as control nodes for the spline." );
			
			if( newNodes.Count > 0 )
			{
				Undo.RegisterUndo( currentSpline, "Add Spline Nodes to " + currentSpline.gameObject.name );
				currentSpline.splineNodesArray.AddRange( newNodes );
			}
		}
		else
		{
			newNodes = DropBox( "Drag game objects containing the SplineNode-component into this box in order to insert new control nodes before the 1st spline node." );
			
			if( newNodes.Count > 0 )
			{
				Undo.RegisterUndo( currentSpline, "Add Spline Nodes to " + currentSpline.gameObject.name );
				currentSpline.splineNodesArray.InsertRange( 0, newNodes );
				return;
			}
			
			for( int i = 0; i < currentSpline.splineNodesArray.Count; i++ )
			{
				EditorGUILayout.BeginHorizontal( );
				GUILayout.Space( 15 );
				
				bool removePressed = GUILayout.Button( "Remove", EditorStyles.miniButtonLeft, GUILayout.Width( 50f ), GUILayout.Height( 16f ) );
				
				SplineNode newNode = EditorGUILayout.ObjectField( currentSpline.splineNodesArray[i], typeof( SplineNode ), true ) as SplineNode;
				
				if( newNode != currentSpline.splineNodesArray[i] )
				{
					Undo.RegisterUndo( currentSpline, "Change Spline Node in " + currentSpline.gameObject.name );
					currentSpline.splineNodesArray[i] = newNode;
				}
				
				EditorGUILayout.EndHorizontal( );
				
				if( removePressed )
				{
					Undo.RegisterUndo( currentSpline, "Remove Spline Node from " + currentSpline.gameObject.name );
					currentSpline.splineNodesArray.RemoveAt( i );	
					return;
				}
			}
			
			newNodes = DropBox( "Drag game objects containing the SplineNode-component into this box in order to append new control nodes to spline node." );
			
			if( newNodes.Count > 0 )
			{
				Undo.RegisterUndo( currentSpline, "Add Spline Nodes to " + currentSpline.gameObject.name );
				currentSpline.splineNodesArray.InsertRange( currentSpline.splineNodesArray.Count, newNodes );
			}
		}
		
		EditorGUILayout.Space( );
	}
	
	private List<SplineNode> DropBox( string caption )
	{
		EventType eventType = Event.current.type;
		
		//Draw drop box
		EditorGUILayout.BeginHorizontal( );
		GUILayout.Space( 15 );
		EditorGUILayout.HelpBox( caption, MessageType.None );
		
		Rect dropBoxRect = GUILayoutUtility.GetLastRect( );
		
		EditorGUILayout.EndHorizontal( );
		
		//if no drag n drop operation return empty collection
		if( eventType != EventType.DragUpdated && eventType != EventType.DragPerform )
			return new List<SplineNode>( );
		
		
		if( !dropBoxRect.Contains( Event.current.mousePosition ) )
			return new List<SplineNode>( );

		DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
		
		if( eventType != EventType.DragPerform )
			return new List<SplineNode>( );
		
		DragAndDrop.AcceptDrag( );
		
		List<SplineNode> splineNodes = new List<SplineNode>( );
		
		foreach( Object item in DragAndDrop.objectReferences )
		{
			GameObject gameObject = item as GameObject;
			
			if( gameObject == null )
				continue;
			
			SplineNode sNode = gameObject.GetComponent<SplineNode>( );
			
			if( sNode == null )
			{
				Debug.LogWarning( "The gameObject \"" + gameObject.name + "\" doesn't have a SplineNode component!", gameObject );
				
				continue;
			}
			
			splineNodes.Add( sNode );
		}
		
		return splineNodes;
	}
	
	private void ApplyChanges( )
	{
		foreach( Object targetObject in serializedObject.targetObjects )
			ApplyChangesToTarget( targetObject );
	}
	
	private void ApplyChangesToTarget( Object targetObject )
	{
		Spline spline = targetObject as Spline;
			
		spline.UpdateSpline( );
		
		SplineMesh splineMesh = spline.GetComponent<SplineMesh>( );
		
		if( splineMesh != null )
			splineMesh.UpdateMesh( );
	}
	
}
