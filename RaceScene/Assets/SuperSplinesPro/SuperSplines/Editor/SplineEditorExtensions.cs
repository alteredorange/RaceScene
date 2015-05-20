using UnityEngine;
using UnityEditor;

using SplineUtilities;

namespace SplineEditorExtensions 
{
	public class AutoUpdaterEditor
	{
		private SerializedProperty updateModeProp;
		private SerializedProperty deltaFramesProp;
		private SerializedProperty deltaSecondsProp;
		
		public AutoUpdaterEditor( SerializedObject serializedObject, string propertyName )
		{
			updateModeProp = serializedObject.FindProperty( propertyName + ".mode" );
			deltaFramesProp = serializedObject.FindProperty( propertyName + ".deltaFrames" );
			deltaSecondsProp = serializedObject.FindProperty( propertyName + ".deltaSeconds" );
		}
		
		public void DrawInspector( )
		{
			EditorGUILayout.PropertyField( updateModeProp, new GUIContent ("   Update Mode" ) );
		
			switch( (AutomaticUpdater.UpdateMode) updateModeProp.enumValueIndex )
			{
			case AutomaticUpdater.UpdateMode.EveryXFrames:
				EditorGUILayout.PropertyField( deltaFramesProp, new GUIContent( "   Delta Frames" ) );
				EditorGUILayout.Space( );
				break;
				
			case AutomaticUpdater.UpdateMode.EveryXSeconds:	
				EditorGUILayout.PropertyField( deltaSecondsProp, new GUIContent( "   Delta Seconds" ) );
				EditorGUILayout.Space( );
				break;
			}
			
			//Clamp values
			deltaSecondsProp.floatValue = Mathf.Max( deltaSecondsProp.floatValue, 0.01f );
			deltaFramesProp.intValue = Mathf.Max( deltaFramesProp.intValue, 2 );
		}
	}
}
