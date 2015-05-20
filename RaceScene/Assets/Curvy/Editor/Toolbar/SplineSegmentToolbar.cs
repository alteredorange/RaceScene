// =====================================================================
// Copyright 2014 FluffyUnderware
// All rights reserved
// =====================================================================
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define OLD_UNDO
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;
using FluffyUnderware.Curvy.Utils;

namespace FluffyUnderware.CurvyEditor.ToolbarItems
{
    #region ### Selections & Constraints ###
    [ToolbarItemInfo(Text = "Prev CP", Tooltip = "Select previous Control Point", Image="prevcp,24,24",
                     Order = 100, Type = ToolbarItemType.Button, AppliesTo = ToolbarItemTarget.ControlPoint)]
    public class TBCPGotoPrev : ToolbarItem
    {
        public TBCPGotoPrev()
        {
            KeyBinding = EditorKeyDefinition.Create("GotoPrevCP", "CP: Previous", KeyCode.T, true, false, false, false, false);
        }

        public override bool Active
        {
            get
            {
                return (Toolbar.SelectedControlPoint.PreviousControlPoint);
            }
        }

        public override void Action()
        {
            Selection.activeObject = Toolbar.SelectedControlPoint.PreviousControlPoint;
        }
    }

    [ToolbarItemInfo(Text="Next CP",Tooltip="Select next Control Point", Image="nextcp,24,24",
                     Order = 101, AppliesTo = ToolbarItemTarget.ControlPoint)]
    public class TBCPGotoNext : ToolbarItem
    {
        public TBCPGotoNext()
        {
            KeyBinding = EditorKeyDefinition.Create("GotoNextCP", "CP: Next", KeyCode.T, false, false, false, false, false);
        }

        public override bool Active
        {
            get
            {
                return (Toolbar.SelectedControlPoint.NextControlPoint);
            }
        }

        public override void Action()
        {
            Selection.activeObject = Toolbar.SelectedControlPoint.NextControlPoint;
        }
    }

    [ToolbarItemInfo(Text="Limit Len", Tooltip="Constraint max. Spline Length", Image="constraintlength,24,24",
                     Type=ToolbarItemType.Toggle,
                     Order = 180, AppliesTo=ToolbarItemTarget.ControlPoint|ToolbarItemTarget.MultipleControlPoints)]
    public class TBLengthConstraint : ToolbarItem
    {
        public float MaxSplineLength;

        public TBLengthConstraint()
        {
            KeyBinding = EditorKeyDefinition.Create("ConstraintLength", "Spline: Constraint Length", KeyCode.None, false, false, false, false, false);
        }

        public override void OnItemGUI(Rect itemRect)
        {
            SetSize(ref itemRect, 84,22);
            Background(itemRect, 84, 22);
            itemRect.width = 60;
            MaxSplineLength=EditorGUI.FloatField(itemRect,MaxSplineLength);
            itemRect.x+=62;
            itemRect.width = 22;
            if (GUI.Button(itemRect, "<"))
                MaxSplineLength = Toolbar.SelectedControlPoint.Spline.Length;
        }

        protected override void OnSceneGUI()
        {
            if (GUI.changed)
            {
                Toolbar.SelectedControlPoint.Spline.RefreshImmediately(true, true, false);

                if (Toolbar.SelectedControlPoint.Spline.Length > MaxSplineLength)
                {
                    RestorePos();
                    Toolbar.SelectedControlPoint.Spline.RefreshImmediately(true, true, false);
                }
                else
                    StorePos();
            }
            
        }


        Vector3[] storedPos = new Vector3[0];

        void StorePos()
        {
            storedPos = new Vector3[Selection.transforms.Length];
            for (int i = 0; i < storedPos.Length; i++)
                storedPos[i] = Selection.transforms[i].position;
        }
        void RestorePos()
        {
            for (int i = 0; i < storedPos.Length; i++)
                Selection.transforms[i].position = storedPos[i];
        }

    }

    [ToolbarItemInfo(Text="Smart Axis",Tooltip="Smart pick new Control Points", Image="smartaxis,24,24",
                     Type=ToolbarItemType.Toggle,
                     Order=181, AppliesTo=ToolbarItemTarget.ControlPoint)]
    public class TBCPSmartAxisConstraint : ToolbarItem
    {
        public Plane Axis;
        public Vector3 AxisCursorPoint;

        public TBCPSmartAxisConstraint()
        {
            KeyBinding = EditorKeyDefinition.Create("ToggleSmartAxis", "CP: Toggle Smart Axis", KeyCode.None, false, false, false, false, false);
        }

        protected override void OnSceneGUI()
        {
            var cpa = Toolbar.SelectedControlPoint;
            CurvySplineSegment cpb;
            CurvySplineSegment cpc;

            if (cpa.Spline.ControlPointCount < 3)
                return;
            if (cpa.ControlPointIndex==0)
            {
                cpb = cpa.NextControlPoint;
                cpc = cpb.NextControlPoint;
            }
            else if (cpa.ControlPointIndex == 1)
            {
                cpb = cpa.PreviousControlPoint;
                cpc = cpa.NextControlPoint;

            }
            else
            {
                cpb = cpa.PreviousControlPoint;
                cpc = cpb.PreviousControlPoint;
            }
            Axis = new Plane(cpa.Position, cpb.Position, cpc.Position);
            Vector3 up = cpa.Position - cpb.Position;
            if (Axis.normal == Vector3.zero)
                return;
            Handles.matrix = Matrix4x4.TRS(cpa.Position, Quaternion.LookRotation(Axis.normal,up), new Vector3(3, 3, 3));
            Handles.DrawSolidRectangleWithOutline(getVertices(), new Color(0.6f, 0.3f, 0.3f,0.1f), new Color(0.2f,0.2f,0.2f));
            Handles.matrix=Matrix4x4.identity;
            // Raycast a plane point to add CP at later on
            Ray R = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float pickDist;
            if (Axis.Raycast(R, out pickDist))
            {
                AxisCursorPoint = R.GetPoint(pickDist);
                Handles.color = new Color(CurvyPreferences.GizmoColor.r, CurvyPreferences.GizmoColor.g, CurvyPreferences.GizmoColor.b, 0.3f);
                Handles.SphereCap(0, AxisCursorPoint, Quaternion.identity, HandleUtility.GetHandleSize(AxisCursorPoint) * CurvyPreferences.GizmoControlPointSize*1.5f);
                SceneView.RepaintAll();
            }
            
            
        }

        Vector3[] getVertices()
        {
            
            Vector3[] v=new Vector3[4];
            v[0] = new Vector3(-1,1,0);
            v[1] = new Vector3(1,1,0);
            v[2] = new Vector3(1,-1,0);
            v[3] = new Vector3(-1,-1,0);
            return v;
        }
    }

    #endregion

    #region ### Add / Delete / ReOrder ###
    [ToolbarItemInfo(Text = "Insert before", Tooltip = "Insert CP before current", Image="insertbefore,24,24",
                     Order = 120, AppliesTo = ToolbarItemTarget.ControlPoint)]
    public class TBCPInsertBefore : ToolbarItem
    {

        TBCPSmartAxisConstraint Axis
        {
            get
            {
                var ax = Toolbar.GetItem<TBCPSmartAxisConstraint>();
                return (ax != null && ax.Enabled) ? ax : null;
            }
        }

        public TBCPInsertBefore()
        {
            KeyBinding = EditorKeyDefinition.Create ("InsertCPBefore", "CP: Insert Before", KeyCode.G, true, false, false, false, false);
        }

        public override bool Active
        {
            get
            {
                return (true);
            }
        }

        public override void Action()
        {
            var cp = Toolbar.SelectedControlPoint;

            var axis = Axis;
            if (axis != null)
            {
                var newCP = cp.Spline.Add(true,cp);
                newCP.Position = axis.AxisCursorPoint;
                Selection.activeObject = newCP;
            }
            else
                Selection.activeObject = cp.Spline.Add(true,cp);

            Undo.RegisterCreatedObjectUndo(Selection.activeGameObject, "Insert Control Point");
        }
    }

    [ToolbarItemInfo(Text = "Insert after", Tooltip = "Insert CP after current", Image="insertafter,24,24",
                     Order = 121, AppliesTo = ToolbarItemTarget.ControlPoint)]
    public class TBCPInsertAfter : ToolbarItem
    {
        TBCPSmartAxisConstraint Axis
        {
            get
            {
                var ax = Toolbar.GetItem<TBCPSmartAxisConstraint>();
                return (ax != null && ax.Enabled) ? ax : null;
            }
        }

        public TBCPInsertAfter()
        {
            KeyBinding = EditorKeyDefinition.Create("InsertCPAfter", "CP: Insert After", KeyCode.G, false, false, false, false, false);
        }

        public override bool Active
        {
            get
            {
                return (true);
            }
        }

        public override void Action()
        {
            var cp = Toolbar.SelectedControlPoint;
            
            var axis=Axis;
            if (axis != null)
            {
                var newCP = cp.Spline.Add(cp);
                newCP.Position = axis.AxisCursorPoint;
                Selection.activeObject = newCP;
            }
            else
                Selection.activeObject = cp.Spline.Add(cp);

            Undo.RegisterCreatedObjectUndo(Selection.activeGameObject, "Insert Control Point");
        }

        
    }

    [ToolbarItemInfo(Text = "Delete", Tooltip="Delete CP", Image="delete,24,24",
                     Order = 122, AppliesTo = ToolbarItemTarget.ControlPoint|ToolbarItemTarget.MultipleControlPoints)]
    public class TBCPDelete : ToolbarItem
    {
        public TBCPDelete()
        {
            KeyBinding = EditorKeyDefinition.Create("DeleteCP", "CP: Delete", KeyCode.H, false, false, false, false, false);
        }

        public override void Action()
        {
            var sel=Toolbar.SelectedControlPoint;
            if (sel)
                sel = sel.PreviousControlPoint;
            CurvyEditorUtility.UndoDeleteControlPoints(Toolbar.SelectedControlPoints.ToArray());
            Selection.activeObject = sel;
        }

    }

    [ToolbarItemInfo(Text="Set 1.",Tooltip="Set as first CP", Image="setfirstcp,24,24",
                     Order = 150, AppliesTo=ToolbarItemTarget.ControlPoint)]
    public class TBCPSetFirst : ToolbarItem
    {
        public TBCPSetFirst(){
            KeyBinding = EditorKeyDefinition.Create("SetFirst", "CP: Set as First", KeyCode.None, false, false, false, false, false);
        }

        public override bool Active
        {
            get
            {
                return Toolbar.SelectedControlPoint.ControlPointIndex > 0;
            }
        }

        public override void Action()
        {
#if OLD_UNDO
            Undo.RegisterSceneUndo("Set first CP");
#else
            CurvyEditorUtility.UndoRegisterSplineControlPoints(Toolbar.SelectedControlPoint.Spline,"Set first CP");
#endif
            CurvyUtility.SetFirstCP(Toolbar.SelectedControlPoint);
        }
    }
    #endregion

    #region ### Join / Split / Connection related ###
    [ToolbarItemInfo(Text="Split",Tooltip="Split Spline", Image="split,24,24",
                     Order = 151, AppliesTo=ToolbarItemTarget.ControlPoint)]
    public class TBCPSplit : ToolbarItem
    {
        public TBCPSplit()
        {
            KeyBinding = EditorKeyDefinition.Create("SplitSpline", "Spline: Split", KeyCode.None, false, false, false, false, false);
        }

        public override bool Active
        {
            get
            {
                return Toolbar.SelectedControlPoint.IsValidSegment && !Toolbar.SelectedControlPoint.IsFirstSegment;
            }
        }

        public override void Action()
        {
            var lastOld = Toolbar.SelectedControlPoint.PreviousControlPoint;
            CurvySpline newSpline = CurvyEditorUtility.UndoSplitSpline(Toolbar.SelectedControlPoint);

            if (lastOld && newSpline.ControlPointCount > 0)
            {
                Selection.activeGameObject = lastOld.gameObject; // to set the active connection to the other. Strange...
                Selection.objects = new GameObject[2] { lastOld.gameObject, newSpline.ControlPoints[0].gameObject }; ;
            }
        } 
    }

    [ToolbarItemInfo(Text="Join",Tooltip="Join Splines", Image="join,24,24",
                     Order = 152, AppliesTo=ToolbarItemTarget.MultipleControlPoints)]
    public class TBCPJoin : ToolbarItem
    {
        public TBCPJoin(){
            KeyBinding = EditorKeyDefinition.Create("JoinSplines", "Spline: Join", KeyCode.None, false, false, false, false, false);
        }

        public override bool Active
        {
            get
            {
                var sel=Toolbar.SelectedControlPoints;
                bool act=(sel.Count == 2 &&
                          sel[0].Spline!=sel[1].Spline);
                if (act)
                    Tooltip = string.Format("Insert {0} after {1}", sel[1].Spline.name, sel[0].name);
                else
                    Tooltip = null;
                return act;
            }
        }

        public override void Action()
        {
            var sel = Toolbar.SelectedControlPoints;
            var source = sel[1];
            var dest = sel[0];
#if OLD_UNDO
            Undo.RegisterSceneUndo("Join Spline");
            CurvyUtility.JoinSpline(source, dest);
#else
            CurvyEditorUtility.UndoJoinSpline(source, dest);
#endif
            Selection.activeObject = dest;
        }
    }

    [ToolbarItemInfo(Text="Connect",Tooltip="Connect Control Points", Image="connect,24,24",
                     Order=153, AppliesTo=ToolbarItemTarget.MultipleControlPoints)]
    public class TBCPConnect : ToolbarItem
    {
        public TBCPConnect() {
            KeyBinding = EditorKeyDefinition.Create("Connect", "CP: Connect", KeyCode.None, false, false, false, false, false);
        }

        public override bool Active
        {
            get
            {
                var sel = Toolbar.SelectedControlPoints;
                bool act = (sel.Count == 2 && // exactly 2 CPs
                            (sel[0].Spline!=sel[1].Spline || // either different splines or not next each other
                            Mathf.Abs(sel[0].ControlPointIndex-sel[1].ControlPointIndex)>1)
                            );
                if (act)
                    Tooltip = string.Format("Connect {0} to {1}", sel[1].ToString(), sel[0].ToString());
                return act;
            }
        }

        public override void Action()
        {
            var sel=Toolbar.SelectedControlPoints;
            var source = sel[1];
            var dest = sel[0];
#if OLD_UNDO
            Undo.RegisterSceneUndo("Connect Control Points");
#else
            Undo.RecordObject(source,"Connect Control Points");
            Undo.RecordObject(dest, "Connect Control Points");
#endif
            source.ConnectTo(dest);
            source.SyncConnections();
            source.Spline.RefreshImmediately();
            dest.Spline.RefreshImmediately();
            SceneView.RepaintAll();

            Selection.activeGameObject = null;
            Selection.activeObject = source.gameObject;
        }
    }

    #endregion
}