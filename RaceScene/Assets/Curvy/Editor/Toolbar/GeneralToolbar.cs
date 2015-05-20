// =====================================================================
// Copyright 2014 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy;

namespace FluffyUnderware.CurvyEditor.ToolbarItems
{

    [ToolbarItemInfo(Text="Options", Tooltip="General Options", Type=ToolbarItemType.Toggle, Image="prefs,24,24",
                     Order = 1, AppliesTo=ToolbarItemTarget.All)]
    public class TBOptions : ToolbarItem
    {
        Texture2D mTexAbout;
        Texture2D mTexPrefs;
        Texture2D mTexHelp;
        Texture2D mTexWWW;
        Texture2D mTexBugReport;

        public override void Action()
        {
          
        }

        public override void OnItemGUI(Rect itemRect)
        {
            if (mTexPrefs == null)
            {
                mTexPrefs = CurvyResource.Load("prefs,24,24");
                mTexHelp = CurvyResource.Load("help,24,24");
                mTexWWW = CurvyResource.Load("web,24,24");
                mTexBugReport = CurvyResource.Load("bugreport,24,24");
                mTexAbout = CurvyResource.Load("about,24,24");
            }

            SetSize(ref itemRect,34,34);

            
            if (GUI.Button(itemRect, new GUIContent(mTexPrefs,"Preferences")))
            {
                CurvyPreferences.Open();
                   Enabled = false;
            }
            NewLine(ref itemRect);
            if (GUI.Button(itemRect, new GUIContent(mTexHelp,"Online Manual")))
            {
                Application.OpenURL(CurvyEditorUtility.BaseHelpURL);
                Enabled = false;
            }
            NewLine(ref itemRect);
            if (GUI.Button(itemRect, new GUIContent(mTexWWW,"Curvy Website")))
            {
                Application.OpenURL("http://www.fluffyunderware.com/pages/unity-plugins/curvy.php");
                Enabled = false;
            }
            NewLine(ref itemRect);
            if (GUI.Button(itemRect, new GUIContent(mTexBugReport,"Report Bug")))
            {
                Application.OpenURL("mailto:bugreport@fluffyunderware.com?subject=[BUG] Curvy " + CurvySpline.Version + "&body=* Please give a brief description of the bug (please attach any screenshots or example projects that might be helpful) :%0A%0A* How to reproduce the bug:%0A%0A");
                Enabled = false;
            }
            NewLine(ref itemRect);
            if (GUI.Button(itemRect, new GUIContent(mTexAbout, "About Curvy")))
                EditorUtility.DisplayDialog("Curvy - Fast Spline Interpolation", "(c) 2013-2014 Fluffy Underware\r\rVersion: " + CurvySpline.Version, "OK");
            {
            }
            
            
             
        }

      
    }

    [ToolbarItemInfo(Text="View",Tooltip="Viewing Options",Image="viewsettings,24,24",
                     Order=2, AppliesTo=ToolbarItemTarget.All, Type=ToolbarItemType.Toggle)]
    public class TBViewSettings : ToolbarItem
    {
        CurvySplineGizmos Gizmos
        {
            get { return CurvySpline.Gizmos; }
            set
            {
                CurvyPreferences.Gizmos = value;
                CurvyPreferences.Save();
            }
        }

        public override void OnItemGUI(Rect itemRect)
        {
            bool b;
            bool v;
            
            SetSize(ref itemRect, 100,19);
            Background(itemRect, 100, 133);
            b = (Gizmos == 0);
            b = GUI.Toggle(itemRect, b, "None");
            if (b)
                Gizmos=0;
            // Curve
            NewLineBelow(ref itemRect);
            b=(Gizmos.HasFlag(CurvySplineGizmos.Curve));
            v = GUI.Toggle(itemRect, b, "Curve");
            if (b != v)
                Gizmos = Gizmos.Set<CurvySplineGizmos>(CurvySplineGizmos.Curve, v);
            // Approximation
            NewLineBelow(ref itemRect);
            b = (Gizmos.HasFlag(CurvySplineGizmos.Approximation));
            v = GUI.Toggle(itemRect, b, "Approximation");
            if (b != v)
                Gizmos = Gizmos.Set<CurvySplineGizmos>(CurvySplineGizmos.Approximation, v);
            // Orientation
            NewLineBelow(ref itemRect);
            b = (Gizmos.HasFlag(CurvySplineGizmos.Orientation));
            v = GUI.Toggle(itemRect, b, "Orientation");
            if (b != v)
                Gizmos = Gizmos.Set<CurvySplineGizmos>(CurvySplineGizmos.Orientation, v);
            // Tangents
            NewLineBelow(ref itemRect);
            b = (Gizmos.HasFlag(CurvySplineGizmos.Tangents));
            v = GUI.Toggle(itemRect, b, "Tangents");
            if (b != v)
                Gizmos = Gizmos.Set<CurvySplineGizmos>(CurvySplineGizmos.Tangents, v);
            // UserValues
            NewLineBelow(ref itemRect);
            b = (Gizmos.HasFlag(CurvySplineGizmos.UserValues));
            v = GUI.Toggle(itemRect, b, "User Values");
            if (b != v)
                Gizmos = Gizmos.Set<CurvySplineGizmos>(CurvySplineGizmos.UserValues, v);
            // Labels
            NewLineBelow(ref itemRect);
            b = (Gizmos.HasFlag(CurvySplineGizmos.Labels));
            v = GUI.Toggle(itemRect, b, "Labels");
            if (b != v)
                Gizmos = Gizmos.Set<CurvySplineGizmos>(CurvySplineGizmos.Labels, v);
            
        }
    }
}