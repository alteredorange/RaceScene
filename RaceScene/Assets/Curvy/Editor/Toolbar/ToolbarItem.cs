using UnityEngine;
using UnityEditor;
using System.Collections;

namespace FluffyUnderware.CurvyEditor
{
    /// <summary>
    /// Base ToolbarItem class
    /// </summary>
    public class ToolbarItem : System.IComparable
    {
        /// <summary>
        /// Gets the associated ToolbarItemInfo attribute
        /// </summary>
        public ToolbarItemInfo Properties;
        /// <summary>
        /// Key binding for this item (if any)
        /// </summary>
        public EditorKeyDefinition KeyBinding;
        /// <summary>
        /// Button Image
        /// </summary>
        public Texture Image
        {
            get
            {
                if (m_Image == null && Properties != null && !string.IsNullOrEmpty(Properties.Image))
                    m_Image = CurvyResource.Load(Properties.Image);
                return m_Image;
            }
            set
            {
                m_Image = value;
            }
        }
        Texture m_Image;
        /// <summary>
        /// Whether the item is currently checked
        /// </summary>
        /// <remarks>For normal buttons this is always false</remarks>
        public bool Enabled;
        
        /// <summary>
        /// Override this to define if the item's GUI should be rendered disabled
        /// </summary>
        /// <remarks>Set Tooltip here if you want dynamic tooltips!</remarks>
        public virtual bool Active { get { return true; } }
        /// <summary>
        /// Override this to add behaviour to the item
        /// </summary>
        public virtual void Action() { }
        /// <summary>
        /// Override this to add additional GUI elements that will be rendered when the item is enabled
        /// </summary>
        /// <remarks>This is executed inside a Handles.BeginGUI()/Handles.EndGUI() block</remarks>
        /// <param name="itemRect">a rect containing the start position</param>
        public virtual void OnItemGUI(Rect itemRect) { }
        /// <summary>
        /// Override this to add additional SceneView rendering when the item is enabled
        /// </summary>
        /// <remarks>This is executed inside the regular OnSceneGUI() block</remarks>
        protected virtual void OnSceneGUI() {}

        /// <summary>
        /// The item tooltip. By default it's the tooltip provided in the ToolbarItemInfo
        /// </summary>
        /// <remarks>Set to null to use the default tooltip</remarks>
        public string Tooltip
        {
            get { return (mTooltipOverride==null) ? Properties.Tooltip : mTooltipOverride; }
            set
            {
                mTooltipOverride = value;
            }
        }
        string mTooltipOverride=null;

        #region ### ItemGUI Helpers ###

        protected void SmallLineAdjust(ref Rect r, float lineHeight)
        {
            switch (CurvyPreferences.ToolbarOrientation)
            {
                case Toolbar.Orientation.Bottom:
                    r.y += Toolbar.ItemsMaxDimensions.y - lineHeight;
                    break;
                case Toolbar.Orientation.Left:
                case Toolbar.Orientation.Right:
                    r.y += Toolbar.ItemsMaxDimensions.y / 2 - lineHeight / 2;
                    break;
            }
        }

        /// <summary>
        /// Sets a rect to a minimum width & height
        /// </summary>
        protected void SetSize(ref Rect r, float width, float height)
        {
            r.width = width;
            r.height = height;
            if (CurvyPreferences.ToolbarOrientation == Toolbar.Orientation.Right)
                r.x -= r.width;
            SmallLineAdjust(ref r,height);
        }

        /// <summary>
        /// Advance the rect to a new line, respecting Toolbar Orientation
        /// </summary>
        protected void NewLine(ref Rect r) { NewLine(ref r, r.height); }
        /// <summary>
        /// Advance the rect to a new line, respecting Toolbar Orientation
        /// </summary>
        protected void NewLine(ref Rect r, float lastLineHeight)
        {
            switch (CurvyPreferences.ToolbarOrientation)
            {
                case Toolbar.Orientation.Bottom:
                    r.y -= lastLineHeight + 5;
                    break;
                case Toolbar.Orientation.Left:
                    r.x += r.width + 2;
                    break;
                case Toolbar.Orientation.Right:
                    r.x -= r.width + 2;
                    break;
                default: // Top
                    r.y += lastLineHeight + 5;
                    break;
            }
        }

        /// <summary>
        /// Advance the rect to a new line below (or above) the existing rect, depending on Toolbar Orientation
        /// </summary>
        protected void NewLineBelow(ref Rect r) { NewLineBelow(ref r, r.height); }
        /// <summary>
        /// Advance the rect to a new line below (or above) the existing rect, depending on Toolbar Orientation
        /// </summary>
        protected void NewLineBelow(ref Rect r, float lastLineHeight)
        {
            switch (CurvyPreferences.ToolbarOrientation)
            {
                case Toolbar.Orientation.Bottom:
                    r.y -= lastLineHeight;
                    break;
                default:
                    r.y += lastLineHeight;
                    break;
            }
        }

        /// <summary>
        /// Renders a background
        /// </summary>
        protected void Background(Rect pos, float width, float height)
        {
            pos.y -= 1;
            pos.x -= 1;

            if (CurvyPreferences.ToolbarOrientation == Toolbar.Orientation.Bottom)
                pos.y = pos.y + pos.height - height;
            
            pos.width = width + 2;
            pos.height = height + 2;
            GUI.Box(pos, "", CurvyGUI.stToolbarWindow);
        }
       
        #endregion

        #region ### Privates & Internals ###
        /*! \cond PRIVATE */
        /*! @name Internal Public
         *  Don't use them unless you know what you're doing!
         */
        //@{

        // returns true if OnItemGUI() needs to be called
        internal bool OnGUI(Rect itemRect, GUIContent content)
        {
            bool en;
                
            switch (Properties.Type){
                case ToolbarItemType.Button:
                    if (GUI.Button(itemRect, content, CurvyGUI.stToolbarItem))
                        Action();
                    return true;
                case ToolbarItemType.Toggle:
                    en = GUI.Toggle(itemRect, Enabled, content, CurvyGUI.stToolbarItem);
                        if (en != Enabled) {
                            Enabled = en;
                            Action();
                        }
                        if (Enabled)
                        {
                            Handles.EndGUI();
                            OnSceneGUI();
                            Handles.BeginGUI();
                            return true;
                        }
                        return false;
                case ToolbarItemType.Radio:
                        en = GUI.Toggle(itemRect,Enabled, content, CurvyGUI.stToolbarItem);
                        if (en!=Enabled) {
                            if (en) {
                                Enabled = true;
                                Toolbar.disableRadiogroupExcept(this);
                            }
                            if (Enabled)
                                Action();
                        }
                        return Enabled;
                case ToolbarItemType.RadioToggle:
                        en = GUI.Toggle(itemRect,Enabled, content, CurvyGUI.stToolbarItem);
                        if (en!=Enabled) {
                            Enabled = en;
                            if (Enabled) {
                                Toolbar.disableRadiogroupExcept(this);
                                Action();
                            }
                        }
                        if (Enabled) {
                            Handles.EndGUI();
                            OnSceneGUI();
                            Handles.BeginGUI();
                            return true;
                        }
                        return false;
            }
            return false;
        }

        

        internal GUIContent GetContent(out Vector2 contentSize)
        { 
            GUIContent c;
            
            // Apply Labelsettings from the preferences
            switch (CurvyPreferences.ToolbarLabels) {
                case Toolbar.LabelMode.Icon:
                    c = new GUIContent(Image, Tooltip);
                    break;
                case Toolbar.LabelMode.Text:
                    c = new GUIContent(Properties.Text, Tooltip);
                    break;
                default:
                    c=new GUIContent(Properties.Text,Image,Tooltip);
                    break;
            }
            // fallback for iconless items
            if (!c.image)
                c.text = Properties.Text;
            // Add Shortcut to tooltip
            if (KeyBinding != null) {
                string keytt = KeyBinding.ToTooltipString();
                if (!string.IsNullOrEmpty(keytt))
                    if (Tooltip.Length>0)
                        c.tooltip += "\n(" + KeyBinding.ToTooltipString() + ")";
                    else
                        c.tooltip=KeyBinding.ToTooltipString();
            }
            contentSize = CurvyGUI.stToolbarItem.CalcSize(c);
            return c;
        }

        //@}
        /*! \endcond */
        #endregion

        #region IComparable Member

        public int CompareTo(object obj)
        {
            ToolbarItem o=(ToolbarItem)obj;
            if (Properties!=null && o.Properties!=null)
                return (Properties.Order.CompareTo(o.Properties.Order));
            else
                return 0;
        }

        #endregion
    }

    /// <summary>
    /// Attribute to be used with ToolbarItem
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class,AllowMultiple=false)]
    public class ToolbarItemInfo : System.Attribute
    {
        /// <summary>
        /// Name identifier
        /// </summary>
        /// <remarks>The name is used to access the item from outside the toolbar, e.g. to execute it's action</remarks>
        public string Name = string.Empty;
        public string Text = string.Empty;
        public string Tooltip = string.Empty;
        public string Image = string.Empty;
        /// <summary>
        /// Order
        /// </summary>
        /// <remarks>A difference of 10 or more adds a space</remarks>
        public int Order = 100;
        /// <summary>
        /// Control Type
        /// </summary>
        public ToolbarItemType Type=ToolbarItemType.Button;
        /// <summary>
        /// Target Selection
        /// </summary>
        public ToolbarItemTarget AppliesTo=ToolbarItemTarget.All;
        /// <summary>
        /// Control Group (used by Radio and RadioToggle only)
        /// </summary>
        public int Radiogroup = 0;
    }
    /// <summary>
    /// Defines the type of a ToolbarItem
    /// </summary>
    public enum ToolbarItemType : int
    {
        /// <summary>
        /// A simple button
        /// </summary>
        Button = 1,
        /// <summary>
        /// A toggle button
        /// </summary>
        Toggle = 2,
        /// <summary>
        /// A Togglebutton being part of a radio group
        /// </summary>
        /// <remarks>Only one button of the group can be enabled</remarks>
        Radio = 3, // Always one of the group
        /// <summary>
        /// A Togglebutton being part of a radio group
        /// </summary>
        /// <remarks>Unlike Radio, all buttons of the radio group can be unchecked</remarks>
        RadioToggle = 4 // One or none of the group
    }

    /// <summary>
    /// Defines the allowed target selections of a ToolbarItem
    /// </summary>
    [System.Flags]
    public enum ToolbarItemTarget : int
    {
        /// <summary>
        /// A single CurvySpline
        /// </summary>
        Spline = 1,
        /// <summary>
        /// 2 or more CurvySplines
        /// </summary>
        MultipleSplines = 2,
        /// <summary>
        /// A single CurvySplineSegment
        /// </summary>
        ControlPoint = 4,
        /// <summary>
        /// 2 or more CurvySplineSegments
        /// </summary>
        MultipleControlPoints = 8,
        /// <summary>
        /// A single CurvySplineGroup
        /// </summary>
        Group = 16,
        /// <summary>
        /// 2 or more CurvySplineGroups
        /// </summary>
        MultipleGroups = 32,
        /// <summary>
        /// A single SplineNetwork
        /// </summary>
        SplineNetwork = 64,
        /// <summary>
        /// 2 or more SplineNetworks
        /// </summary>
        MultipleSplineNetworks = 128,
        /// <summary>
        /// Any selection
        /// </summary>
        All = 65535
    }

}
