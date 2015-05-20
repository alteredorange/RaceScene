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

    [ToolbarItemInfo(Text = "First CP", Tooltip="Select first Control Point", Image="selectfirst,24,24",
                     Order=60, AppliesTo=ToolbarItemTarget.Spline)]
    public class TBSplineSelectFirstCP : ToolbarItem
    {

        public override bool Active
        {
            get
            {
                return (Toolbar.SelectedSpline.ControlPointCount > 0);
            }
        }

        public TBSplineSelectFirstCP()
        {
            KeyBinding = EditorKeyDefinition.Create("FirstCP", "Spline: Select first CP", KeyCode.T, false, false, false, false, false);
        }

        public override void Action()
        {
            Selection.activeObject = Toolbar.SelectedSpline.ControlPoints[0];
        }
    }

    [ToolbarItemInfo(Text="All CP's", Tooltip="Select all Control Points",  Image="selectall,24,24",
                     Order=61,AppliesTo=ToolbarItemTarget.Spline)]
    public class TBSplineSelectAllCP : ToolbarItem
    {
        public override bool Active
        {
            get
            {
                return (Toolbar.SelectedSpline.ControlPointCount > 0);
            }
        }

        public override void Action()
        {
            Object[] obj=new Object[Toolbar.SelectedSpline.ControlPointCount];
            for (int i=0;i<obj.Length;i++)
                obj[i]=Toolbar.SelectedSpline.ControlPoints[i].gameObject;
            Selection.objects = obj;
        }
    }

    [ToolbarItemInfo(Text = "Center Pivot", Tooltip = "Center Control Points around Spline transform", Image="centerpivot,24,24",
                     Order = 20, AppliesTo = ToolbarItemTarget.Spline|ToolbarItemTarget.MultipleSplines)]
    public class TBSplineCenterPivot : ToolbarItem 
    {
        public TBSplineCenterPivot()
        {
            KeyBinding = EditorKeyDefinition.Create("CenterPivot", "Spline: Center Pivot", KeyCode.None, false, false, false, false, false);
        }

        public override void Action()
        {
#if OLD_UNDO
            Undo.RegisterSceneUndo("Center Pivot");
            foreach (var spline in Toolbar.SelectedSplines)
                CurvyUtility.CenterPivot(spline);
#else
            foreach (var spline in Toolbar.SelectedSplines)
            {
                for (int i = 0; i < spline.ControlPointCount; i++)
                    Undo.RecordObject(spline.ControlPoints[i].transform, "Center Pivot");
                Undo.RecordObject(spline.Transform, "Center Pivot");
                CurvyUtility.CenterPivot(spline);
            }
#endif
            
        }
    }

    [ToolbarItemInfo(Text = "Flip Dir", Tooltip = "Flip Direction", Image="flip,24,24",
                     Order = 21, AppliesTo = ToolbarItemTarget.Spline | ToolbarItemTarget.MultipleSplines)]
    public class TBSplineFlipDirection : ToolbarItem 
    {
        public TBSplineFlipDirection()
        {
            KeyBinding = EditorKeyDefinition.Create("FlipDirection", "Spline: Flip Direction", KeyCode.None, false, false, false, false, false);
        }

        public override void Action()
        {
#if OLD_UNDO
            Undo.RegisterSceneUndo("Flip Spline(s)");
            foreach (var spline in Toolbar.SelectedSplines)
                CurvyUtility.FlipSpline(spline);
#else
            foreach (var spline in Toolbar.SelectedSplines){
                CurvyEditorUtility.UndoRegisterSplineControlPoints(spline,"Flip Spline(s)");
                CurvyUtility.FlipSpline(spline);
            }
#endif
            
        }
    }

    [ToolbarItemInfo(Text = "Align Wizard", Tooltip = "Align Objects to Spline", Image="alignwizard,24,24",
                     Order = 35, AppliesTo = ToolbarItemTarget.Spline)]
    public class TBSplineAlignWizard : ToolbarItem
    {
        public override void Action()
        {
            CurvySplineAlignWizard.Create();
        }
    }

    [ToolbarItemInfo(Text = "Export Wizard", Tooltip = "Export Shape", Image="exportwizard,24,24",
                     Order = 36, AppliesTo = ToolbarItemTarget.Spline)]
    public class TBSplineExportWizard : ToolbarItem
    {
        public override void Action()
        {
            CurvySplineExportWizard.Create();
        }
    }

    [ToolbarItemInfo(Text = "Group", Tooltip = "Create a Spline Group", Image="group,24,24",
                     Order = 80, AppliesTo = ToolbarItemTarget.Spline | ToolbarItemTarget.MultipleSplines)]
    public class TBSplineCreateGroup : ToolbarItem
    {

        public override void Action()
        {
            Selection.activeGameObject=CurvySplineGroup.Create(Toolbar.SelectedSplines.ToArray()).gameObject;
            Undo.RegisterCreatedObjectUndo(Selection.activeGameObject, "Create Spline Group");
        }
    }

    [ToolbarItemInfo(Text = "Clone-Bld", Tooltip = "Create a Clone Builder", Image = "clonebuilder,24,24",
                     Order = 81, AppliesTo = ToolbarItemTarget.Spline | ToolbarItemTarget.Group)]
    public class TBSplineCreateCloneBuilder : ToolbarItem
    {
        public override void Action()
        {
            var cb=SplinePathCloneBuilder.Create();
            cb.Spline = (CurvySplineBase)Toolbar.Selection[0];
            Selection.activeGameObject = cb.gameObject;
            Undo.RegisterCreatedObjectUndo(Selection.activeGameObject, "Create Clone Builder");
        }
    }

    [ToolbarItemInfo(Text = "Mesh-Bld", Tooltip = "Create a Mesh Builder", Image = "meshbuilder,24,24",
                     Order = 82, AppliesTo = ToolbarItemTarget.Spline | ToolbarItemTarget.Group)]
    public class TBSplineCreateMeshBuilder : ToolbarItem
    {
        public override void Action()
        {
            var mb = SplinePathMeshBuilder.Create();
            mb.Spline = (CurvySplineBase)Toolbar.Selection[0];
            Selection.activeGameObject = mb.gameObject;
            Undo.RegisterCreatedObjectUndo(Selection.activeGameObject, "Create Mesh Builder");
        }
    }

}