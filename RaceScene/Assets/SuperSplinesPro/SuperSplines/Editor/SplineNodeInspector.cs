using System;

using UnityEngine;
using UnityEditor;

using SplineEditorExtensions;

[CanEditMultipleObjects]
[CustomEditor(typeof(SplineNode))]
public class SplineNodeInspector : Editor
{
	private SerializedProperty posInSplineProp;
	private SerializedProperty lengthProp;
	private SerializedProperty customValueProp;
	
	private GUIStyle buttonGUIStyle;
	
	private LengthMode lMode = LengthMode.GameUnits;
	
	public void OnEnable( )
	{
		customValueProp = serializedObject.FindProperty( "customValue" );
		posInSplineProp = serializedObject.FindProperty( "posInSpline" );
		lengthProp = serializedObject.FindProperty( "length" );
	}
		
	public override void OnInspectorGUI( )
	{
		serializedObject.Update( );
		
		SplineNode targetNode = target as SplineNode;
		
		EditorGUILayout.LabelField( "   Name", target.name );
		
		if( targetNode.spline == null )
		{
			EditorGUILayout.HelpBox( "This SplineNode isn't used by any spline in the scene. Attach this node to a spline by dragging it onto a spline's Inspector window!", MessageType.Info );
			return;
		}
		
		int index = Array.IndexOf( targetNode.spline.SplineNodes, targetNode );
		
		EditorGUILayout.TextField( "   Index in Spline", index.ToString( ) );
		EditorGUILayout.Space( );
		
		lMode = (LengthMode) EditorGUILayout.EnumPopup( "   Length Mode", lMode );
		EditorGUILayout.Space( );
		
		EditorGUILayout.TextField( "   Position in Spline", (posInSplineProp.floatValue * ((lMode != LengthMode.GameUnits) ? 1 : ((SplineNode)target).spline.Length)).ToString( ) );
		EditorGUILayout.TextField( "   Distance to next node", (lengthProp.floatValue * ((lMode != LengthMode.GameUnits) ? 1 : ((SplineNode)target).spline.Length)).ToString( ) );
		EditorGUILayout.Space( );
		
		EditorGUILayout.PropertyField( customValueProp, new GUIContent( "   Custom Data" ) );
		EditorGUILayout.Space( );
		
		EditorGUILayout.BeginHorizontal( );
		EditorGUILayout.Space( );
		
		if( GUILayout.Button( "Previous Node", GetButtonGUIStyle( ), GUILayout.Width( 150f ), GUILayout.Height( 23f ) ) )
		{
			SplineNode[] splineNodes = targetNode.spline.SplineNodes;
			
			Selection.activeGameObject = splineNodes[ (index!=0 ? index : splineNodes.Length ) - 1].gameObject; 
		}
		
		GUILayout.Space( 10f );
		
		if( GUILayout.Button( "Next Node", GetButtonGUIStyle( ), GUILayout.Width( 150f ), GUILayout.Height( 23f ) ) )
		{
			SplineNode[] splineNodes = targetNode.spline.SplineNodes;
			
			Selection.activeGameObject = splineNodes[(index+1)%splineNodes.Length].gameObject; 
		}
			
		EditorGUILayout.Space( );
		EditorGUILayout.EndHorizontal( );
		
		EditorGUILayout.Space( );
		
		//Apply changes
		serializedObject.ApplyModifiedProperties( );
	}
	
	private GUIStyle GetButtonGUIStyle( )
	{
		if( buttonGUIStyle == null )
		{
			buttonGUIStyle = new GUIStyle( EditorStyles.miniButton );
			buttonGUIStyle.alignment = TextAnchor.MiddleCenter;
			buttonGUIStyle.wordWrap = true;
			buttonGUIStyle.border = new RectOffset( 5, 5, 5, 5 );
			buttonGUIStyle.contentOffset = - Vector2.up * 2f;
			buttonGUIStyle.fontSize = 12;
		}
		
		return buttonGUIStyle;
	}
	
	private enum LengthMode
	{
		Normalized,
		GameUnits
	}
}
