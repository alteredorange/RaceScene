// =====================================================================
// Copyright 2014 FluffyUnderware
// All rights reserved
// =====================================================================
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define OLD_UNDO
#endif
using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.CurvyEditor;
using FluffyUnderware.Curvy;

namespace FluffyUnderware.CurvyEditor
{
    [CustomEditor(typeof(CurvySplineSegment)), CanEditMultipleObjects]
    public class CurvySplineSegmentInspector : CurvyInspector<CurvySplineSegment>
    {
        public static float SmoothingOffset = 0.3f;
        bool[] mFoldouts = new bool[5] { true, true, true,true,true };

        bool mValid;
        SerializedProperty tSmoothTangent;
        SerializedProperty tSyncStartEnd;
        SerializedProperty tT0;
        SerializedProperty tB0;
        SerializedProperty tC0;
        SerializedProperty tT1;
        SerializedProperty tB1;
        SerializedProperty tC1;
        SerializedProperty tOT;
        SerializedProperty tOB;
        SerializedProperty tOC;
        SerializedProperty tHandleIn;
        SerializedProperty tHandleOut;
        SerializedProperty tFreeHandles;
        SerializedProperty tHandleScale;
        
        Texture2D mTexDelete;
        Texture2D mTexSelect;
        Texture2D mTexAddUserData;
        Texture2D mTexRemoveUserData;


        bool IsActive
        {
            get { return Target.Transform == Selection.activeTransform; }
        }

        bool SceneIsSelected
        {
            get
            {
                return SceneView.focusedWindow == SceneView.currentDrawingSceneView;
            }
        }

        void OnEnable()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
            tSmoothTangent = serializedObject.FindProperty("SmoothEdgeTangent");
            tSyncStartEnd = serializedObject.FindProperty("SynchronizeTCB");
            tT0 = serializedObject.FindProperty("StartTension");
            tC0 = serializedObject.FindProperty("StartContinuity");
            tB0 = serializedObject.FindProperty("StartBias");
            tT1 = serializedObject.FindProperty("EndTension");
            tC1 = serializedObject.FindProperty("EndContinuity");
            tB1 = serializedObject.FindProperty("EndBias");
            tOT = serializedObject.FindProperty("OverrideGlobalTension");
            tOC = serializedObject.FindProperty("OverrideGlobalContinuity");
            tOB = serializedObject.FindProperty("OverrideGlobalBias");
            tHandleIn = serializedObject.FindProperty("HandleIn");
            tHandleOut = serializedObject.FindProperty("HandleOut");
            tFreeHandles = serializedObject.FindProperty("FreeHandles");
            tHandleScale = serializedObject.FindProperty("HandleScale");

            mTexDelete = CurvyResource.Load("deletesmall,12,12");
            mTexSelect = CurvyResource.Load("selectsmall,12,12");
            mTexAddUserData = CurvyResource.Load("addsmall,16,16");
            mTexRemoveUserData = CurvyResource.Load("removesmall,16,16");
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
            if (Tools.current == Tool.None)
                Tools.current = Tool.Move;
        }
        
        void OnSceneGUI()
        {

            Handles.ArrowCap(0, Target.Position, Quaternion.LookRotation(Target.Transform.up), HandleUtility.GetHandleSize(Target.Position));

            if (IsActive)
            {
                // Bezier-Handles
                if (Target.Spline.Interpolation == CurvyInterpolation.Bezier)
                {
                    Handles.color = Color.grey;
                    Vector3 handleOut = Target.HandleOutPosition;
                    Vector3 handleIn = Target.HandleInPosition;

                    EditorGUI.BeginChangeCheck();
                    Vector3 pOut = Handles.PositionHandle(Target.HandleOutPosition, Tools.handleRotation);
                    
                    Handles.CubeCap(0, pOut, Quaternion.identity, HandleUtility.GetHandleSize(Target.HandleOutPosition) * 0.1f);
                    Handles.DrawLine(pOut, Target.Position);

                    Vector3 pIn = Handles.PositionHandle(Target.HandleInPosition, Tools.handleRotation);
                    Handles.CubeCap(0, pIn, Quaternion.identity, HandleUtility.GetHandleSize(Target.HandleInPosition) * 0.1f);
                    Handles.DrawLine(pIn, Target.Position);

                    bool chgOut=Target.HandleOutPosition != pOut;
                    bool chgIn=Target.HandleInPosition != pIn;

                    if ( chgOut || chgIn)
                    {
                        
#if OLD_UNDO
                        Undo.RegisterUndo(Target, "Move Handle");
#else
                        Undo.ClearUndo(Target);
                        Undo.RecordObject(Target,"Move Handle");
#endif
                        if (chgOut)
                            Target.HandleOutPosition = pOut;

                        if (chgIn)
                            Target.HandleInPosition = pIn;
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        Target.Spline.RefreshImmediately(true, true, false);
                        var lcons = Toolbar.GetItem<CurvyEditor.ToolbarItems.TBLengthConstraint>();
                        if (lcons.Enabled && Target.Spline.Length > lcons.MaxSplineLength)
                        {
                            Target.HandleOutPosition = handleOut;
                            Target.HandleInPosition = handleIn;
                            Target.Spline.RefreshImmediately(true, true, false);
                        }
                    }
                }

            }
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

            serializedObject.UpdateIfDirtyOrScript();
            if (Event.current.type == EventType.Layout)
                mValid = Target.IsValidSegment;

            if (mValid && (Target.Spline.Closed || !Target.IsFirstSegment))
                EditorGUILayout.PropertyField(tSmoothTangent, new GUIContent("Smooth Tangent", "Smooth end tangent?"));
            
            if (Target.Spline.Interpolation == CurvyInterpolation.Bezier && CurvyGUI.Foldout(ref mFoldouts[0], "Bezier Handles",CurvyEditorUtility.HelpURL("curvysplinesegment","bezier")))
            {
                EditorGUILayout.PropertyField(tFreeHandles, new GUIContent("Free Move", "Move Handles individually?"));
                EditorGUILayout.Slider(tHandleScale, 0, 10, new GUIContent("Scale", "Handle Scaling"));

                Vector3 v = tHandleIn.vector3Value;
                EditorGUILayout.PropertyField(tHandleIn);
                if (v != tHandleIn.vector3Value && !tFreeHandles.boolValue)
                    tHandleOut.vector3Value = -tHandleIn.vector3Value;
                v = tHandleIn.vector3Value;
                EditorGUILayout.PropertyField(tHandleOut);
                if (v != tHandleOut.vector3Value && !tFreeHandles.boolValue)
                    tHandleIn.vector3Value = -tHandleOut.vector3Value;

                EditorGUILayout.LabelField("Smooth Handles", EditorStyles.boldLabel);

                SmoothingOffset = EditorGUILayout.Slider(new GUIContent("Offset", "Smoothing Offset"), SmoothingOffset, 0.1f, 1f);
                if (GUILayout.Button(new GUIContent("Smooth", "Set Handles by Catmul-Rom")))
                {
#if OLD_UNDO
                Undo.RegisterUndo(targets, "Smooth Bezier Handles");
#else
                    Undo.RecordObjects(targets, "Smooth Bezier Handles");
#endif
                    foreach (CurvySplineSegment tgt in targets)
                    {
                        CurvyUtility.InterpolateBezierHandles(CurvyInterpolation.CatmulRom, SmoothingOffset, tgt.FreeHandles, tgt);
                    }
                    Target.Spline.RefreshImmediately(true, true, false);
                    SceneView.RepaintAll();
                }
                
            }


            if (mValid && Target.Spline.Interpolation == CurvyInterpolation.TCB && CurvyGUI.Foldout(ref mFoldouts[0], "TCB Settings",CurvyEditorUtility.HelpURL("curvysplinesegment","tcb")))
            {
                EditorGUILayout.PropertyField(tSyncStartEnd, new GUIContent("Synchronize TCB", "Synchronize Start and End Values"));
                EditorGUILayout.PropertyField(tOT, new GUIContent("Local Tension", "Override Spline Tension?"));
                if (tOT.boolValue)
                {
                    EditorGUILayout.PropertyField(tT0, Target.SynchronizeTCB ? new GUIContent("Tension", "Tension") : new GUIContent("Start Tension", "Start Tension"));
                    if (!Target.SynchronizeTCB)
                        EditorGUILayout.PropertyField(tT1, new GUIContent("End Tension", "End Tension"));
                    else
                        tT1.floatValue = tT0.floatValue;
                }
                EditorGUILayout.PropertyField(tOC, new GUIContent("Local Continuity", "Override Spline Continuity?"));
                if (tOC.boolValue)
                {
                    EditorGUILayout.PropertyField(tC0, Target.SynchronizeTCB ? new GUIContent("Continuity", "Continuity") : new GUIContent("Start Continuity", "Start Continuity"));
                    if (!Target.SynchronizeTCB)
                        EditorGUILayout.PropertyField(tC1, new GUIContent("End Continuity", "End Continuity"));
                    else
                        tC1.floatValue = tC0.floatValue;
                }
                EditorGUILayout.PropertyField(tOB, new GUIContent("Local Bias", "Override Spline Bias?"));
                if (tOB.boolValue)
                {
                    EditorGUILayout.PropertyField(tB0, Target.SynchronizeTCB ? new GUIContent("Bias", "Bias") : new GUIContent("Start Bias", "Start Bias"));
                    if (!Target.SynchronizeTCB)
                        EditorGUILayout.PropertyField(tB1, new GUIContent("End Bias", "End Bias"));
                    else
                        tB1.floatValue = tB0.floatValue;
                }

                if (tOT.boolValue || tOC.boolValue || tOB.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("Set Catmul"))
                    {
                        tT0.floatValue = 0; tC0.floatValue = 0; tB0.floatValue = 0;
                        tT1.floatValue = 0; tC1.floatValue = 0; tB1.floatValue = 0;
                    }
                    if (GUILayout.Button("Set Cubic"))
                    {
                        tT0.floatValue = -1; tC0.floatValue = 0; tB0.floatValue = 0;
                        tT1.floatValue = -1; tC1.floatValue = 0; tB1.floatValue = 0;
                    }
                    if (GUILayout.Button("Set Linear"))
                    {
                        tT0.floatValue = 0; tC0.floatValue = -1; tB0.floatValue = 0;
                        tT1.floatValue = 0; tC1.floatValue = -1; tB1.floatValue = 0;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (CurvyGUI.Foldout(ref mFoldouts[1], "User Values",CurvyEditorUtility.HelpURL("curvysplinesegment","uservalues")))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent(mTexAddUserData, "Add User Value slot"), GUILayout.ExpandWidth(false), GUILayout.Height(mTexAddUserData.height)))
                {
                    Target.Spline.UserValueSize++;
                    Target.Spline.Refresh();
                    EditorUtility.SetDirty(Target.Spline);
                }


                if (GUILayout.Button(new GUIContent(mTexRemoveUserData, "Remove User Value slot"), GUILayout.ExpandWidth(false), GUILayout.Height(mTexRemoveUserData.height)))
                {
                    Target.Spline.UserValueSize--;
                    Target.Spline.Refresh();
                    EditorUtility.SetDirty(Target.Spline);
                }

                EditorGUILayout.EndHorizontal();

                if (Target.UserValues != null && Target.UserValues.Length > 0)
                    ArrayGUI(serializedObject, "UserValues", false);
            }


            if ((Target.Connection != null || Target.ConnectedBy.Count > 0) && CurvyGUI.Foldout(ref mFoldouts[2], "Connections", CurvyEditorUtility.HelpURL("curvysplinesegment", "connections")))
                ConnectionGUI();

            //EditorGUILayout.PropertyField(serializedObject.FindProperty("Node"));


            if ((serializedObject.targetObject && serializedObject.ApplyModifiedProperties()))
            {
                Target.Spline.Refresh(true, true, false);
                SceneView.RepaintAll();
            }

            if (mValid && CurvyGUI.Foldout(ref mFoldouts[3],"Segment Info"))
            {
                EditorGUILayout.LabelField(new GUIContent("Distance: " + Target.Distance,"Distance from first Control Point"));
                EditorGUILayout.LabelField(new GUIContent("Length: " + Target.Length,"Length of this segment"));
            }
            if (CurvyGUI.Foldout(ref mFoldouts[4],"Spline Info"))
                EditorGUILayout.LabelField("Total Length: " + Target.Spline.Length);
            
        }

        void ConnectionGUI()
        {
            if (Target.Connection != null)
                ConnectionDetailsGUI(Target);


            for (int i = 0; i < Target.ConnectedBy.Count; i++)
            {
                ConnectionDetailsGUI(Target.ConnectedBy[i]);
            }
        }

        void ConnectionDetailsGUI(CurvySplineSegment seg)
        {
            if (!seg || seg.Connection == null) return;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(mTexSelect, "Select Other"), GUILayout.ExpandWidth(false)))
            {
                Selection.activeObject = (seg == Target) ? seg.Connection.Other : seg;
            }
            EditorGUILayout.LabelField(seg.Connection.ToString());
            if (GUILayout.Button(new GUIContent(mTexDelete, "Delete Connection"), GUILayout.ExpandWidth(false)))
            {
#if OLD_UNDO
                Undo.RegisterUndo(seg.Connection.Owner, "Delete Connection");
                Undo.RegisterUndo(seg.Connection.Other, "Delete Connection");
#else
                Undo.RecordObject(seg.Connection.Owner, "Delete Connection");
                Undo.RecordObject(seg.Connection.Other, "Delete Connection");
#endif
                seg.ConnectTo(null);
                SceneView.RepaintAll();
                return;
            }
            EditorGUILayout.EndHorizontal();
            seg.Connection.Heading = (CurvyConnection.HeadingMode)EditorGUILayout.EnumPopup(new GUIContent("Heading", "Heading Mode"), seg.Connection.Heading);
            seg.Connection.Sync = (CurvyConnection.SyncMode)EditorGUILayout.EnumPopup(new GUIContent("Synchronization", "Synchronization Mode"), seg.Connection.Sync);
            seg.Connection.Tags = EditorGUILayout.TextField(new GUIContent("Tags", "Identifier tags (space separated)"), seg.Connection.Tags);
            
            if (GUI.changed)
            {
                seg.SyncConnections();
                if (seg.Connection.Active)
                    seg.Connection.Other.Spline.Refresh();
            }
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        }

        #region ### Helpers ###

        void ArrayGUI(SerializedObject obj, string name, bool resizeable)
        {
            int size = obj.FindProperty(name + ".Array.size").intValue;
            int newSize = size;
            if (resizeable)
            {
                newSize = EditorGUILayout.IntField(" Size", size);
                if (newSize != size)
                    obj.FindProperty(name + ".Array.size").intValue = newSize;
            }
            
            for (int i = 0; i < newSize; i++)
            {
                var prop = obj.FindProperty(string.Format("{0}.Array.data[{1}]", new object[] { name, i }));
                EditorGUILayout.PropertyField(prop, true);
            }
        }

        #endregion
    }
}