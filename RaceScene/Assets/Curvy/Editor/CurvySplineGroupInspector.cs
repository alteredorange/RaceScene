// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;

namespace FluffyUnderware.CurvyEditor
{
    [CustomEditor(typeof(CurvySplineGroup)), CanEditMultipleObjects]
    public class CurvySplineGroupInspector : Editor
    {

        public CurvySplineGroup Target { get { return target as CurvySplineGroup; } }

        GUIStyle mLargeFont;

        SerializedProperty tSplines;


        [MenuItem("GameObject/Create Other/Curvy/Spline Group", false, 1)]
        static void CreateCurvySplineGroup()
        {
            CurvySplineGroup grp = CurvySplineGroup.Create();
            Selection.activeObject = grp;
        }

        void OnEnable()
        {
            mLargeFont = new GUIStyle();
            mLargeFont.normal.textColor = new Color(0.8f, 0.8f, 1, 0.5f);
            mLargeFont.fontSize = 60;

            Target.Refresh();
            tSplines = serializedObject.FindProperty("Splines");
        }

        void OnDisable()
        {
            Toolbar.Release();
        }

        void OnSceneGUI()
        {
            Target._RemoveEmptySplines();
            for (int i = 0; i < Target.Count; i++)
                Handles.Label(Target[i].Transform.position, i.ToString(), mLargeFont);
            
            Toolbar.Render();
        }

        public override void OnInspectorGUI()
        {
            Toolbar.RegisterTargets(targets);

            EditorGUILayout.PropertyField(tSplines, new GUIContent("Splines", "Splines in the Group"), true);

            EditorGUILayout.LabelField("Group Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Total Length: " + Target.Length);

            if (serializedObject.targetObject && serializedObject.ApplyModifiedProperties())
            {
                Target.RefreshImmediately(true, true, false);
                SceneView.RepaintAll();
            }
        }


    }
}