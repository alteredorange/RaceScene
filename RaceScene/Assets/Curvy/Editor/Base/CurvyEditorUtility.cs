// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define OLD_UNDO
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.Curvy;

namespace FluffyUnderware.CurvyEditor
{

    public class CurvyEditorUtility
    {
        public const string BaseHelpURL = "http://docs.fluffyunderware.com/curvy160/";

        public static string HelpURL(string comp) { return HelpURL(comp, null); }
        public static string HelpURL(string comp, string anchor)
        {
            if (string.IsNullOrEmpty(anchor))
                return BaseHelpURL + comp + ".html";
            else
                return BaseHelpURL + comp+".html#"+comp+anchor;
        }

        public static void OnUndoRedoPerformed()
        {
            var item = Selection.activeGameObject;
            if (item != null)
            {
                CurvySpline spline = item.GetComponent<CurvySpline>();
                CurvySplineSegment cp = item.GetComponent<CurvySplineSegment>();
                if (!spline && cp)
                    spline = cp.Spline;
                if (spline)
                {
                    spline._RenameControlPointsByIndex();
                    spline.Refresh();
                }
            }
        }


        public static void UndoRegisterSplineControlPoints(CurvySpline spl, string name)
        {
#if !OLD_UNDO
            for (int i = 0; i < spl.ControlPointCount; i++)
                Undo.RecordObject(spl.ControlPoints[i].gameObject, name);
#endif
        }

        public static void UndoDeleteControlPoints(params CurvySplineSegment[] cps)
        {
            List<CurvySpline> RefreshSplines = new List<CurvySpline>();
            foreach (var cp in cps)
                if (!RefreshSplines.Contains(cp.Spline))
                    RefreshSplines.Add(cp.Spline);
#if OLD_UNDO
            Undo.RegisterSceneUndo("Delete Control Point(s)");
            for (int i = 0; i < cps.Length; i++)
                GameObject.DestroyImmediate(cps[i].gameObject);
#else

            for (int i = 0; i < cps.Length; i++)
            {
                cps[i].name = "CP" + string.Format("{0:0000}", cps[i].ControlPointIndex);
                Undo.DestroyObjectImmediate(cps[i].gameObject);
            }
#endif

            foreach (var spline in RefreshSplines)
                spline.RefreshImmediately();

        }

        /// <summary>
        /// Undo-complient version of CurvyUtility.SplitSpline()
        /// </summary>
        /// <param name="firstCP">the first Control Point of the new spline</param>
        /// <returns>the new spline</returns>
        public static CurvySpline UndoSplitSpline(CurvySplineSegment firstCP)
        {
#if OLD_UNDO
            Undo.RegisterSceneUndo("Split Spline");
            return CurvyUtility.SplitSpline(firstCP);
#else
            CurvySpline old = firstCP.Spline;
            CurvySpline spl = CurvySpline.Create(old);
            Undo.RegisterCreatedObjectUndo(spl.gameObject, "Split Spline");
            spl.name = old.name + "_parted";

            // Move CPs
            var affected = old.ControlPoints.GetRange(firstCP.ControlPointIndex, old.ControlPointCount - firstCP.ControlPointIndex);
            for (int i = 0; i < affected.Count; i++)
            {
                Undo.RecordObject(affected[i].gameObject, "Split Spline");
                Undo.SetTransformParent(affected[i].Transform, spl.Transform, "Split Spline");
                affected[i]._ReSettle();
                
            }
            old.ControlPoints.Clear();
            old.RefreshImmediately(true, true, false);
            spl._RenameControlPointsByIndex();
            spl.RefreshImmediately(true, true, false);
            return spl;
#endif
        }

        /// <summary>
        /// Undo-complient version of CurvyUtility.JoinSpline()
        /// </summary>
        /// <param name="sourceCP">a Control Point of the source spline</param>
        /// <param name="destCP">the Control Point of the destination spline</param>
        public static void UndoJoinSpline(CurvySplineSegment sourceCP, CurvySplineSegment destCP)
        {
#if !OLD_UNDO
            if (!sourceCP || !destCP)
                return;
            CurvySpline src = sourceCP.Spline;
            CurvySpline dst = destCP.Spline;

            if (src == dst)
                return;
            for (int i = 0; i < src.ControlPointCount; i++)
            {
                Undo.RecordObject(src.ControlPoints[i].gameObject, "Join Spline");
                Undo.SetTransformParent(src.ControlPoints[i].Transform, dst.Transform, "Join Spline");
                src.ControlPoints[i]._ReSettle();
            }

            dst.ControlPoints.InsertRange(destCP.ControlPointIndex + 1, src.ControlPoints);
            dst._RenameControlPointsByIndex();
            dst.RefreshImmediately(true, true, false);
            Undo.DestroyObjectImmediate(src.gameObject);
#endif
        }

        /// <summary>
        /// Gets the inspector dimensions if called from inside OnInspectorGUI()
        /// </summary>
        public static Rect InspectorDimensions
        {
            get
            {
                return new Rect(Screen.currentResolution.width - Screen.width, Screen.currentResolution.height - Screen.height, Screen.width, Screen.height);
            }
        }
        
    }

    [System.Serializable]
    public class EditorKeyDefinition
    {
        public string Name;
        public KeyCode Key;
        public bool Shift;
        public bool Control;
        public bool Alt;
        public bool Command;
        public bool MouseClick;

        public static EditorKeyDefinition Create(string ident, string description, KeyCode defKey, bool defShift, bool defControl, bool defAlt, bool defCommand, bool mouseclick)
        {
            EditorKeyDefinition def;
            if (!CurvyPreferences.KeyBindings.TryGetValue(ident,out def))
            {
                def = new EditorKeyDefinition(ident, description, defKey, defShift, defControl, defAlt, defCommand, mouseclick);
                CurvyPreferences.KeyBindings.Add(ident, def);
                CurvyPreferences.Save();
            }

            return def;
        }

        EditorKeyDefinition(string ident, string description, KeyCode defKey, bool defShift, bool defControl, bool defAlt, bool defCommand, bool mouseclick)
        {
            Name = description;
            Key = (KeyCode)EditorPrefs.GetInt(Name + "_Key", (int)defKey);
            Shift = EditorPrefs.GetBool(Name + "_Shift", defShift);
            Control = EditorPrefs.GetBool(Name + "_Control", defControl);
            Alt = EditorPrefs.GetBool(Name + "_Alt", defAlt);
            Command = EditorPrefs.GetBool(Name + "_Command", defCommand);
            MouseClick = mouseclick;
        }

        public EditorKeyDefinition(string prefsString)
        {
            string[] s = prefsString.Split(',');
            if (s.Length < 6) return;
            Name = s[0];
            Key = (KeyCode)System.Enum.Parse(typeof(KeyCode), s[1]);
            Shift = bool.Parse(s[2]);
            Control = bool.Parse(s[3]);
            Alt = bool.Parse(s[4]);
            Command = bool.Parse(s[5]);
            MouseClick = bool.Parse(s[6]);
        }

       
        public bool IsTriggered(Event ev)
        {
            return (ev.type==EventType.KeyDown &&
                    Key!=KeyCode.None &&
                    ev.keyCode == Key &&
                    ev.shift == Shift &&
                    ev.control == Control &&
                    ev.alt == Alt &&
                    ev.command == Command) ;
        }

        public void OnPreferencesGUI()
        {
            Key=(KeyCode)EditorGUILayout.EnumPopup(new GUIContent(Name), Key);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Shift = GUILayout.Toggle(Shift,"Shift");
            Control = GUILayout.Toggle(Control,"Ctrl");
            Alt = GUILayout.Toggle(Alt, "Alt");
            Command = GUILayout.Toggle(Command,"Cmd");
            EditorGUILayout.EndHorizontal();
        }

        public string ToTooltipString()
        {
            if (Key == KeyCode.None)
                return string.Empty;
            List<string> s = new List<string>();
            if (Control)
                s.Add("Ctrl");
            if (Alt)
                s.Add("Alt");
            if (Shift)
                s.Add("Shift");
            if (Command)
                s.Add("Cmd");
            string res = string.Join("-", s.ToArray());
            if (res.Length > 0)
                res += "-";

            return res + Key.ToString();
        }

        public string ToPrefsString()
        {
            return Name + "," + ((int)Key).ToString() + "," + Shift.ToString() + "," + Control.ToString() + "," + Alt.ToString() + "," + Command.ToString() + "," + MouseClick.ToString();
        }
    }

    /// <summary>
    /// Class for loading image resources
    /// </summary>
    public class CurvyResource
    {
        static Assembly _CurvyResourceAssembly;
        static Assembly CurvyResourceAssembly
        {
            get
            {
                var asms = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in asms)
                    if (asm.FullName.StartsWith("CurvyResources"))
                    {
                        _CurvyResourceAssembly = asm;
                        break;
                    }
                return _CurvyResourceAssembly;
            }
        }

        /// <summary>
        /// Load an image defined by a packed string either from the Resource DLL or the Editor/Resources folder
        /// </summary>
        /// <param name="packedstring">string formed name,width,height. E.g. "myIcon,16,16"</param>
        /// <returns>a Texture</returns>
        public static Texture2D Load(string packedstring)
        {
            if (!string.IsNullOrEmpty(packedstring)) {
                string[] s = packedstring.Split(',');
                if (s.Length == 3) {
                    try {
                        int w = int.Parse(s[1]);
                        int h = int.Parse(s[2]);
                        return CurvyResource.Load(s[0], w, h);
                    }
                    catch {}
                }
            }
            return null;
        }


        /// <summary>
        /// Load an image either from the Resource DLL or the Editor/Resources folder
        /// </summary>
        /// <param name="resourceName">name of the resource file (without extension)</param>
        /// <param name="width">width of the image</param>
        /// <param name="height">height of the image</param>
        /// <returns>a Texture</returns>
        public static Texture2D Load(string resourceName, int width, int height)
        {
            Texture2D texture = null;
            if (string.IsNullOrEmpty(System.IO.Path.GetExtension(resourceName)))
                resourceName += ".png";

            Assembly myAssembly = CurvyResourceAssembly;
            System.IO.Stream myStream = myAssembly.GetManifestResourceStream("Assets.Curvy.Editor.Resources." + resourceName);
            if (myStream != null) {
                texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
                texture.LoadImage(ReadToEnd(myStream));
            }

            if (texture != null)
                return texture;

            texture = (Texture2D)Resources.Load(System.IO.Path.GetFileNameWithoutExtension(resourceName));

            if (texture == null) {
                Debug.LogError("Missing Dll resource: " + resourceName);
            }

            return texture;
        }
        

        static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = stream.Position;
            stream.Position = 0;

            try {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0) {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length) {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1) {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            System.Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            System.Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead) {
                    buffer = new byte[totalBytesRead];
                    System.Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally {
                stream.Position = originalPosition;
            }
        }
    }

    public class CurvyGUI
    {
        #region ### Helpers ###

        static Stack<Color> mColorstack = new Stack<Color>();

        public static void PushColor(Color col)
        {
            mColorstack.Push(GUI.color);
            GUI.color = col;
        }

        public static void PopColor() 
        {
            GUI.color = mColorstack.Pop();
        }

        public static bool Click
        {
            get
            {
                return (Event.current.type == EventType.mouseDown && Event.current.button == 0);
            }
        }
        public static bool ContextClick
        {
            get
            {
                return (Event.current.type == EventType.mouseDown && Event.current.button == 1);
            }
        }

        public static bool InRegion() { return InRegion(GUILayoutUtility.GetLastRect()); }
        public static bool InRegion(Rect r)
        {
            return (r.Contains(Event.current.mousePosition));
        }

        /// <summary>
        /// Gets a GUI rect for content at worldposition. Also options to center the rect and offset it
        /// </summary>
        public static Rect GetRect(Vector3 worldPosition, GUIContent content, GUIStyle style, bool center, params float[] offset)
        {
            Rect r = HandleUtility.WorldPointToSizedRect(worldPosition, content, style);
            if (center)
            {
                r.x -= r.width / 2;
                r.y -= r.height / 2;
            }
            if (offset.Length >= 1)
                r.x += offset[0];
            if (offset.Length == 2)
                r.y += offset[1];
            return r;
        }

        

        public static Texture2D PixelTex(Color col)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            return tex;
        }

        #endregion

        #region ### Styles ###

        public static GUIStyle stToolbarWindow
        {
            get
            {
                if (mstToolbarWindow == null)
                {
                    mstToolbarWindow = new GUIStyle(GUI.skin.textArea);
                }
                return mstToolbarWindow;
            }
        }
        static GUIStyle mstToolbarWindow;

        public static GUIStyle stModuleBox
        {
            get
            {
                if (mstModuleBox == null)
                {
                    mstModuleBox = new GUIStyle(GUI.skin.box);
                    mstModuleBox.padding = new RectOffset(2, 0, 0, 4);
                }
                return mstModuleBox;
            }
        }
        static GUIStyle mstModuleBox;

        public static GUIStyle stModuleHeader
        {
            get
            {
                if (mstModuleHeader==null)
                {
                    mstModuleHeader = new GUIStyle(GUI.skin.button);
                    mstModuleHeader.alignment = TextAnchor.MiddleLeft;
                    mstModuleHeader.fontStyle = FontStyle.Bold;
                    mstModuleHeader.margin = new RectOffset(0, 0, 0, 10);
                    mstModuleHeader.padding.left = 34;
                }
                return mstModuleHeader;
            }
        }
        static GUIStyle mstModuleHeader;

        public static GUIStyle stFoldout
        {
            get
            {
                if (mstFoldout == null)
                {
                    mstFoldout = new GUIStyle(EditorStyles.foldout);
                    mstFoldout.fontStyle = FontStyle.Bold;
                }
                return mstFoldout;
            }
        }
        static GUIStyle mstFoldout;

        public static GUIStyle stToolbarItem
        {
            get
            {
                if (mstToolbarItem == null)
                {
                    mstToolbarItem = new GUIStyle(GUI.skin.button);
                    mstToolbarItem.alignment = TextAnchor.MiddleLeft;
                    mstToolbarItem.padding.top = 4;
                    mstToolbarItem.padding.bottom = 2;
                }
                return mstToolbarItem;
            }
        }
        static GUIStyle mstToolbarItem;

        public static GUIStyle stNetworkNodeLabel
        {
            get
            {
                if (mstNetworkNodeLabel == null)
                {
                    mstNetworkNodeLabel = new GUIStyle(GUI.skin.label);
                    mstNetworkNodeLabel.fontSize = 32;
                }
                return mstNetworkNodeLabel;
            }
        }
        static GUIStyle mstNetworkNodeLabel;

        public static GUIStyle stNetworkButton
        {
            get
            {
                if (mstNetworkButton == null)
                {
                    mstNetworkButton = new GUIStyle(GUI.skin.button);
                    mstNetworkButton.padding = new RectOffset(-1, -1, -1, -1);
                    mstNetworkButton.imagePosition = ImagePosition.ImageOnly;
                }
                return mstNetworkButton;
            }
        }
        static GUIStyle mstNetworkButton;

        public static GUIStyle stBorderlessButton
        {
            get
            {
                if (mstBorderlessButton == null)
                {
                    mstBorderlessButton = new GUIStyle(GUI.skin.label);
                    mstBorderlessButton.padding = new RectOffset(-1, 3, -1, -1);
                    mstBorderlessButton.imagePosition = ImagePosition.ImageOnly;
                }
                return mstBorderlessButton;
            }
        }
        static GUIStyle mstBorderlessButton;

        #endregion
        
        public static Texture2D IconSectionHelp
        {
            get
            {
                if (mIconSectionHelp == null)
                    mIconSectionHelp = CurvyResource.Load("sectionhelp", 16, 16);
                return mIconSectionHelp;
            }
        }
        static Texture2D mIconSectionHelp;

        #region ### GUI Controls ###

        public static bool Foldout(ref bool state, string text) { return Foldout(ref state, new GUIContent(text), null); }
        public static bool Foldout(ref bool state, string text, string helpURL) { return Foldout(ref state, new GUIContent(text), helpURL); }

        public static bool Foldout(ref bool state, GUIContent content, string helpURL)
        {
            Rect r=GUILayoutUtility.GetRect(content, stFoldout);
            int lvl = EditorGUI.indentLevel;
            EditorGUI.indentLevel = Mathf.Max(0, EditorGUI.indentLevel - 1);
            r = EditorGUI.IndentedRect(r);
            r.x += 3;
            r.width -= IconSectionHelp.width;
            //state = GUILayout.Toggle(state, content, stFoldout, GUILayout.ExpandWidth(true));
            state = GUI.Toggle(r,state, content, stFoldout);
            if (state && !string.IsNullOrEmpty(helpURL))
            {
                r.x=r.xMax;
                r.width = IconSectionHelp.width;
                if (GUI.Button(r,new GUIContent(IconSectionHelp, "Help"), stBorderlessButton))
                    Application.OpenURL(helpURL);
            }
            

            EditorGUI.indentLevel = lvl;

            return state;
        }

        #endregion

        #region ### SCENE GUI Controls ###

       

        #endregion

    }

   
}