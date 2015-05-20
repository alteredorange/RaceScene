using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy;

namespace FluffyUnderware.CurvyEditor
{
    [CustomEditor(typeof(AlignToSpline))]
    [CanEditMultipleObjects]
    public class AlignToSplineInspector : CurvyComponentInspector<AlignToSpline>
    {

        bool[] mFoldouts = new bool[2] { true, true };

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            if (CurvyGUI.Foldout(ref mFoldouts[0], "Source",CurvyEditorUtility.HelpURL("aligntospline","source")))
            {
                var ppSpline = serializedObject.FindProperty("m_Spline");
                EditorGUILayout.PropertyField(ppSpline);
                if (ppSpline.objectReferenceValue == null)
                    EditorGUILayout.HelpBox("No Source provided!", MessageType.Warning);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Dynamic"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("FastInterpolation"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UpdateIn"));
            }

            if (CurvyGUI.Foldout(ref mFoldouts[1], "Options", CurvyEditorUtility.HelpURL("aligntospline", "options")))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Mode"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Position"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SetOrientation"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Use2DOrientation"));
                
            }
            

            serializedObject.ApplyModifiedProperties();
        }
    }
}