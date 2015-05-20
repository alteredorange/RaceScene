// =====================================================================
// Copyright 2014 FluffyUnderware
// All rights reserved
// =====================================================================
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define OLD_UNDO
#endif
using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Utils;

namespace FluffyUnderware.CurvyEditor
{
    [CustomEditor(typeof(CurvySpline)), CanEditMultipleObjects]
    public class CurvySplineInspector : CurvyInspector<CurvySpline>
    {

        SerializedProperty tInterpolation;
        SerializedProperty tClosed;
        SerializedProperty tAutoEndTangents;
        SerializedProperty tInitialUp;
        SerializedProperty tOrientation;
        SerializedProperty tSetCPRotation;
        SerializedProperty tSwirl;
        SerializedProperty tSwirlTurns;
        SerializedProperty tGranularity;
        SerializedProperty tAutoRefresh;
        SerializedProperty tAutoRefreshLength;
        SerializedProperty tAutoRefreshOrientation;
        SerializedProperty tT;
        SerializedProperty tC;
        SerializedProperty tB;

        Texture2D mTexSelect;

        GUIStyle mUserValuesLabel;

        
        bool[] mFoldouts = new bool[4] { true, true, true, true };

        

        void OnEnable()
        {
            

            mTexSelect = CurvyResource.Load("selectsmall,12,12");

            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;

            tInterpolation = serializedObject.FindProperty("Interpolation");
            tClosed = serializedObject.FindProperty("Closed");
            tAutoEndTangents = serializedObject.FindProperty("AutoEndTangents");
            tInitialUp = serializedObject.FindProperty("InitialUpVector");
            tOrientation = serializedObject.FindProperty("Orientation");
            tSetCPRotation = serializedObject.FindProperty("SetControlPointRotation");
            tSwirl = serializedObject.FindProperty("Swirl");
            tSwirlTurns = serializedObject.FindProperty("SwirlTurns");
            tGranularity = serializedObject.FindProperty("Granularity");
            tAutoRefresh = serializedObject.FindProperty("AutoRefresh");
            tAutoRefreshLength = serializedObject.FindProperty("AutoRefreshLength");
            tAutoRefreshOrientation = serializedObject.FindProperty("AutoRefreshOrientation");
            tT = serializedObject.FindProperty("Tension");
            tC = serializedObject.FindProperty("Continuity");
            tB = serializedObject.FindProperty("Bias");

            Target.Refresh(true, true, false);
            SceneView.RepaintAll();
#if !OLD_UNDO
            Undo.undoRedoPerformed -= CurvyEditorUtility.OnUndoRedoPerformed;
            Undo.undoRedoPerformed += CurvyEditorUtility.OnUndoRedoPerformed;
#endif
        }

        void OnDisable()
        {
#if !OLD_UNDO
            Undo.undoRedoPerformed -= CurvyEditorUtility.OnUndoRedoPerformed;
#endif
            Toolbar.Release();
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
        }

        void OnSceneGUI()
        {
            Handles.BeginGUI();

            mUserValuesLabel = new GUIStyle(EditorStyles.boldLabel);
            mUserValuesLabel.normal.textColor = Color.green;

            if (Target.UserValueSize > 0 && CurvySpline.Gizmos.HasFlag(CurvySplineGizmos.UserValues))
                foreach (CurvySplineSegment cp in Target.ControlPoints)
                {
                    string uvalues = "";
                    for (int i = 0; i < cp.UserValues.Length; i++)
                        uvalues += string.Format("{0}: {1}\n", new object[] { i, cp.UserValues[i] });
                    Handles.Label(cp.Transform.position + new Vector3(0, -0.2f, 0), uvalues, mUserValuesLabel);
                }

            if (CurvySpline.Gizmos.HasFlag(CurvySplineGizmos.Labels))
            {
                GUIStyle stLabel = new GUIStyle(EditorStyles.boldLabel);
                stLabel.normal.textColor = Color.white;
                Handles.Label(Target.Transform.position - new Vector3(-0.5f, 0.2f, 0), Target.name, stLabel);
                foreach (CurvySplineSegment cp in Target.ControlPoints)
                    Handles.Label(cp.Position + new Vector3(-0.5f, HandleUtility.GetHandleSize(cp.Position) * 0.35f, 0), cp.name, stLabel);
            }

            Handles.EndGUI();
            Toolbar.Render();
        }

        void OnHierarchyWindowItemOnGUI(int instanceid, Rect selectionrect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceid) as GameObject;
            if (obj)
            {
                var cp = obj.GetComponent<CurvySplineSegment>();
                if (cp)
                {
                    CurvyConnection con = cp.ConnectionAny;
                    if (con != null)
                    {
                        Color c = GUI.color;
                        switch (con.Sync)
                        {
                            case CurvyConnection.SyncMode.NoSync: GUI.color = new Color(0, 0, 0); break;
                            case CurvyConnection.SyncMode.SyncPosAndRot: GUI.color = new Color(1, 1, 1); break;
                            case CurvyConnection.SyncMode.SyncRot: GUI.color = new Color(1, 1, 0); break;
                            case CurvyConnection.SyncMode.SyncPos: GUI.color = CurvySpline.GizmoColor; break;
                        }
                        GUI.DrawTexture(new Rect(selectionrect.xMax - 14, selectionrect.yMin + 4, 10, 10), mTexSelect);
                        GUI.color = c;
                    }
                }
            }
            if (targets.Length == 1 && Target && Target.gameObject.GetInstanceID() == instanceid)
            {
                // Shortcut-Keys
                Toolbar.ProcessEvents();
            }
        }
        
        public override void OnInspectorGUI()
        {
            Toolbar.RegisterTargets(targets);

            EditorGUILayout.PropertyField(tInterpolation, new GUIContent("Interpolation", "Interpolation Method"));
            EditorGUILayout.PropertyField(tClosed, new GUIContent("Close Spline", "Close spline?"));
            GUI.enabled = !tClosed.boolValue && tInterpolation.enumNames[tInterpolation.enumValueIndex] != "Linear" && tInterpolation.enumNames[tInterpolation.enumValueIndex] != "Bezier";
            EditorGUILayout.PropertyField(tAutoEndTangents, new GUIContent("Auto End Tangents", "Handle End Control Points automatically?"));
            GUI.enabled = true;
            EditorGUILayout.PropertyField(tGranularity, new GUIContent("Granularity", "Approximation resolution"));
            tGranularity.intValue = Mathf.Max(1, tGranularity.intValue);
            
            if (CurvyGUI.Foldout(ref mFoldouts[0], "Orientation",CurvyEditorUtility.HelpURL("curvyspline","orientation")))
            {
                EditorGUILayout.PropertyField(tOrientation, new GUIContent("Orientation", "How the Up-Vector should be calculated"));
                GUI.enabled = Target.Interpolation != CurvyInterpolation.Bezier;
                EditorGUILayout.PropertyField(tSetCPRotation, new GUIContent("Set CP Rotation", "Rotate CP to match calculated Up-Vector3"));
                GUI.enabled = true;
                if (tOrientation.enumNames[tOrientation.enumValueIndex] == "Tangent")
                {
                    EditorGUILayout.PropertyField(tInitialUp, new GUIContent("Initial Up-Vector", "How the first Up-Vector should be determined"));
                    EditorGUILayout.PropertyField(tSwirl, new GUIContent("Swirl", "Orientation swirl mode"));
                    if (tSwirl.enumNames[tSwirl.enumValueIndex] != "None")
                        EditorGUILayout.PropertyField(tSwirlTurns, new GUIContent("Turns", "Swirl turns"));
                }
            }
            if (CurvyGUI.Foldout(ref mFoldouts[1], "Updates", CurvyEditorUtility.HelpURL("curvyspline", "updates")))
            {
                EditorGUILayout.PropertyField(tAutoRefresh, new GUIContent("Auto Refresh", "Refresh when Control Point position change?"));
                EditorGUILayout.PropertyField(tAutoRefreshLength, new GUIContent("Auto Refresh Length", "Recalculate Length on Refresh?"));
                EditorGUILayout.PropertyField(tAutoRefreshOrientation, new GUIContent("Auto Refresh Orientation", "Recalculate tangent normals and Up-Vectors on Refresh?"));
            }
            if (tInterpolation.enumNames[tInterpolation.enumValueIndex] == "TCB" && CurvyGUI.Foldout(ref mFoldouts[2],"TCB Settings",CurvyEditorUtility.HelpURL("curvyspline","tcb")))
            {
                EditorGUILayout.PropertyField(tT, new GUIContent("Tension", "Tension for TCB-Spline"));
                EditorGUILayout.PropertyField(tC, new GUIContent("Continuity", "Continuity for TCB-Spline"));
                EditorGUILayout.PropertyField(tB, new GUIContent("Bias", "Bias for TCB-Spline"));

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Set Catmul", "Set TCB to match Catmul Rom")))
                {
                    tT.floatValue = 0; tC.floatValue = 0; tB.floatValue = 0;
                }
                if (GUILayout.Button(new GUIContent("Set Cubic", "Set TCB to match Simple Cubic")))
                {
                    tT.floatValue = -1; tC.floatValue = 0; tB.floatValue = 0;
                }
                if (GUILayout.Button(new GUIContent("Set Linear", "Set TCB to match Linear")))
                {
                    tT.floatValue = 0; tC.floatValue = -1; tB.floatValue = 0;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            
            
            if (serializedObject.targetObject && serializedObject.ApplyModifiedProperties())
            {
                Target.Refresh(true, true, false);
                SceneView.RepaintAll();
            }

            if (CurvyGUI.Foldout(ref mFoldouts[3], "Spline Info"))
            {
                EditorGUILayout.LabelField("Control Points: " + Target.ControlPointCount);
                EditorGUILayout.LabelField("Segments: " + Target.Count);
                EditorGUILayout.LabelField("Total Length: " + Target.Length);
                EditorGUILayout.LabelField(new GUIContent("User Value Size: " + Target.UserValueSize, "Size of User Value array"));
            }
            Repaint();
        }




    }


}