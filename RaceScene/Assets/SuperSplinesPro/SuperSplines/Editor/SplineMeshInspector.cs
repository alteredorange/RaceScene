using UnityEngine;
using UnityEditor;

using SplineEditorExtensions;

[CanEditMultipleObjects]
[CustomEditor(typeof(SplineMesh))]
public class SplineMeshInspector : Editor
{
	private SerializedProperty splineProp;
	private SerializedProperty baseMeshProp;
	
	private AutoUpdaterEditor autoUpdateProp;
	
	private SerializedProperty segmentCountProp;
	private SerializedProperty splineSegmentProp;
	
	private SerializedProperty xyScaleProp;
	private SerializedProperty uvScaleProp;
	
	private SerializedProperty uvModeProp;
	
	public void OnEnable( )
	{
		splineProp = serializedObject.FindProperty( "spline" );
		baseMeshProp = serializedObject.FindProperty( "baseMesh" );
		segmentCountProp = serializedObject.FindProperty( "segmentCount" );
		xyScaleProp = serializedObject.FindProperty( "xyScale" );
		uvScaleProp = serializedObject.FindProperty( "uvScale" );
		uvModeProp = serializedObject.FindProperty( "uvMode" );
		splineSegmentProp = serializedObject.FindProperty( "splineSegment" );
		
		autoUpdateProp = new AutoUpdaterEditor( serializedObject, "autoUpdater" );
	}
		
	public override void OnInspectorGUI( )
	{
		serializedObject.Update( );
		
		EditorGUILayout.PropertyField( splineProp, new GUIContent( "   Spline" ) );
		
		if( splineProp.objectReferenceValue == null )
			EditorGUILayout.HelpBox( "Please select the spline that shall be used for mesh generation.", MessageType.Warning, false );
		
		EditorGUILayout.PropertyField( baseMeshProp, new GUIContent( "   Base Mesh" ) );
		
		if( baseMeshProp.objectReferenceValue == null )
			EditorGUILayout.HelpBox( "Please select a base mesh.", MessageType.Warning, false );
		
		EditorGUILayout.Space( );
		
		autoUpdateProp.DrawInspector( );
		
		EditorGUILayout.IntSlider( segmentCountProp, 1, MaxSegmentCount( ), new GUIContent( "   Segment Count" ) ); 
		
		bool splitMesh = EditorGUILayout.Toggle( "   Split Mesh", (splineSegmentProp.intValue != -1) );
		
		if( splitMesh && splineSegmentProp.intValue == -1 )
			splineSegmentProp.intValue = 0;
		else if( !splitMesh && splineSegmentProp.intValue != -1 )
			splineSegmentProp.intValue = -1;
		
		if( splitMesh )
			EditorGUILayout.IntSlider( splineSegmentProp, 0, MaxSplineSegment( ), new GUIContent( "   Spline Segment Index" ) ); 
		
		EditorGUILayout.Space( );
		
		EditorGUILayout.PropertyField( uvModeProp, new GUIContent( "   UV-Mode" ) );
		EditorGUILayout.PropertyField( uvScaleProp, new GUIContent( "UV Scale" ), true );
		EditorGUILayout.Space( );
		
		EditorGUILayout.PropertyField( xyScaleProp, new GUIContent( "Mesh Scale" ), true );
		EditorGUILayout.Space( );
		
		
		//Clamp values
		if( splitMesh )
			splineSegmentProp.intValue = Mathf.Clamp( splineSegmentProp.intValue, 0, 
				(splineProp.objectReferenceValue != null ) ? (splineProp.objectReferenceValue as Spline).SegmentCount-1 : 1 );
		
		
		//Apply changes
		if( serializedObject.ApplyModifiedProperties( ) )
			ApplyChanges( );
	}
	
	private void ApplyChanges( )
	{
		foreach( Object targetObject in serializedObject.targetObjects	 )
			(targetObject as SplineMesh).UpdateMesh( );
	}
	
	private int MaxSegmentCount( )
	{
		Mesh baseMesh = baseMeshProp.objectReferenceValue as Mesh;
		
		if( baseMesh != null )
			return (65000 - (65000 % baseMesh.vertexCount)) / baseMesh.vertexCount;
		else
			return 65000;
	}
	
	private int MaxSplineSegment( )
	{
		Spline spline = splineProp.objectReferenceValue as Spline;
		
		if( spline != null )
			return spline.SegmentCount-1;
		else
			return 0;
	}
}
