// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace FluffyUnderware.CurvyEditor
{
    [InitializeOnLoad]
    public class CurvyPreferences
    {
        static bool prefsLoaded = false;

        public static Color GizmoColor = Color.red;
        public static Color GizmoSelectionColor = Color.white;
        public static float GizmoControlPointSize = 0.15f;
        public static float GizmoOrientationLength = 1f;
        public static CurvySplineGizmos Gizmos = CurvySplineGizmos.Curve | CurvySplineGizmos.Orientation;
        public static Toolbar.LabelMode ToolbarLabels = Toolbar.LabelMode.Full;
        public static Toolbar.Orientation ToolbarOrientation = Toolbar.Orientation.Left;

        public static Dictionary<string, EditorKeyDefinition> KeyBindings = new Dictionary<string, EditorKeyDefinition>();

        static Vector2 scroll;
        static bool[] foldouts = new bool[4] { true, true, true, true };
        
        static CurvyPreferences()
        {
            Load();
            RemoveObsoleteEntries();
        }

        public static void Open()
        {
            var asm = Assembly.GetAssembly(typeof(EditorWindow));
            var T = asm.GetType("UnityEditor.PreferencesWindow");
            var M = T.GetMethod("ShowPreferencesWindow", BindingFlags.NonPublic | BindingFlags.Static);
            M.Invoke(null, null);
        }

        static void RemoveObsoleteEntries()
        {
            if (EditorPrefs.HasKey("CurvyCPNext_Key")) {
                RemoveObsoleteKeyBinding("CurvyCPNext");
                RemoveObsoleteKeyBinding("CurvyCPPrev");
                RemoveObsoleteKeyBinding("CurvyCPInsertAfter");
                RemoveObsoleteKeyBinding("CurvyCPInsertBefore");
                RemoveObsoleteKeyBinding("CurvyCPDelete");
                RemoveObsoleteKeyBinding("CurvyBezierToggleFree");
            }
        }

        static void RemoveObsoleteKeyBinding(string Name)
        {
            EditorPrefs.DeleteKey(Name + "_Key");
            EditorPrefs.DeleteKey(Name + "_Shift");
            EditorPrefs.DeleteKey(Name + "_Control");
            EditorPrefs.DeleteKey(Name + "_Alt");
            EditorPrefs.DeleteKey(Name + "_Command");
        }

        [PreferenceItem("Curvy")]
        public static void PreferencesGUI()
        {
            if (!prefsLoaded)
                Load();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foldouts[0]=EditorGUILayout.Foldout(foldouts[0],"Gizmos",CurvyGUI.stFoldout);
            if (foldouts[0]) {
                GizmoColor = EditorGUILayout.ColorField("Spline color", GizmoColor);
                GizmoSelectionColor = EditorGUILayout.ColorField("Spline Selection color", GizmoSelectionColor);
                GizmoControlPointSize = EditorGUILayout.FloatField("Control Point Size", GizmoControlPointSize);
                GizmoOrientationLength = EditorGUILayout.FloatField(new GUIContent("Orientation Length","Orientation gizmo size"), GizmoOrientationLength);
            }
            foldouts[1] = EditorGUILayout.Foldout(foldouts[1], "UI",CurvyGUI.stFoldout);
            if (foldouts[1])
            {
                ToolbarLabels = (Toolbar.LabelMode)EditorGUILayout.EnumPopup(new GUIContent("Toolbar Labels", "Defines Toolbar Display Mode"), ToolbarLabels);
                ToolbarOrientation = (Toolbar.Orientation)EditorGUILayout.EnumPopup(new GUIContent("Toolbar Orientation", "Defines Toolbar Position"), ToolbarOrientation);
            }
            foldouts[2] = EditorGUILayout.Foldout(foldouts[2], "Shortcuts",CurvyGUI.stFoldout);
            if (foldouts[2])
            {
                EditorGUILayout.HelpBox("Bindings take effect after Unity restart!", MessageType.Info);
                var bindings = new List<EditorKeyDefinition>(KeyBindings.Values);
                bindings.Sort((a, b) => string.Compare(a.Name, b.Name));
                foreach (EditorKeyDefinition binding in bindings)
                    binding.OnPreferencesGUI();
            }
            EditorGUILayout.EndScrollView();

            if (GUI.changed)
            {
                Save();
                Load();
                Toolbar.Clear();
            }
        }

        static void Load()
        {
            KeyBindings.Clear();
            if (!EditorPrefs.HasKey("Curvy_KeyBindings"))
                Save();
            GizmoColor = String2Color(EditorPrefs.GetString("Curvy_GizmoColor", "1;0;0;1"));
            GizmoSelectionColor = String2Color(EditorPrefs.GetString("Curvy_GizmoSelectionColor", "1;1;1;1"));
            GizmoControlPointSize = EditorPrefs.GetFloat("Curvy_ControlPointSize", 0.15f);
            GizmoOrientationLength = EditorPrefs.GetFloat("Curvy_OrientationLength", 1);
            Gizmos = (CurvySplineGizmos)EditorPrefs.GetInt("Curvy_Gizmos", (int)(CurvySplineGizmos.Curve | CurvySplineGizmos.Orientation));
            ToolbarLabels = (Toolbar.LabelMode)EditorPrefs.GetInt("Curvy_ToolbarLabels", (int)Toolbar.LabelMode.Icon);
            ToolbarOrientation = (Toolbar.Orientation)EditorPrefs.GetInt("Curvy_ToolbarOrientation", (int)Toolbar.Orientation.Top);
            string kbcol = EditorPrefs.GetString("Curvy_KeyBindings");
            string[] binds = kbcol.Split('|');
            if (binds.Length > 1)
            {
                try
                {
                    for (int i = 0; i < binds.Length; i += 2)
                    {
                        string key = binds[i];
                        KeyBindings.Add(key, new EditorKeyDefinition(binds[i + 1]));
                    }
                }
                catch
                {
                    Debug.LogError("Curvy Preferences: Error loading Key Bindings! Resetting to defaults!");
                    EditorPrefs.DeleteKey("Curvy_KeyBindings");
                }
            }
            prefsLoaded = true;
            SetSplineGizmoSettings();
        }

        public static void Save()
        {
            EditorPrefs.SetString("Curvy_GizmoColor", Color2String(GizmoColor));
            EditorPrefs.SetString("Curvy_GizmoSelectionColor", Color2String(GizmoSelectionColor));
            EditorPrefs.SetInt("Curvy_Gizmos", (int)Gizmos);
            EditorPrefs.SetFloat("Curvy_ControlPointSize", GizmoControlPointSize);
            EditorPrefs.SetFloat("Curvy_OrientationLength", GizmoOrientationLength);
            EditorPrefs.SetInt("Curvy_ToolbarLabels", (int)ToolbarLabels);
            EditorPrefs.SetInt("Curvy_ToolbarOrientation", (int)ToolbarOrientation);
            StringBuilder sb = new StringBuilder();
            foreach (string key in KeyBindings.Keys) {
                sb.Append(key + "|" + KeyBindings[key].ToPrefsString() + "|");
            }
            EditorPrefs.SetString("Curvy_KeyBindings", sb.ToString().TrimEnd('|'));
            SetSplineGizmoSettings();
        }

        public static void SetSplineGizmoSettings()
        {
            CurvySpline.GizmoColor = GizmoColor;
            CurvySpline.GizmoSelectionColor = GizmoSelectionColor;
            CurvySpline.GizmoControlPointSize = GizmoControlPointSize;
            CurvySpline.GizmoOrientationLength = GizmoOrientationLength;
            CurvySpline.Gizmos = Gizmos;
        }

        public static string Color2String(Color c)
        {
            return string.Format("{0};{1};{2};{3}", new object[] { c.r, c.g, c.b, c.a });
        }

        public static Color String2Color(string s)
        {
            s = s.Replace(',', '.');
            string[] array = s.Split(';');
            if (array.Length != 4)
                return Color.white;

            float r, g, b, a;
            float.TryParse(array[0], out r);
            float.TryParse(array[1], out g);
            float.TryParse(array[2], out b);
            float.TryParse(array[3], out a);
            return new Color(r, g, b, a);
        }
    }
    
    
}