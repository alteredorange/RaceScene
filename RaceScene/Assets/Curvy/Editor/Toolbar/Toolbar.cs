using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using FluffyUnderware.Curvy;



namespace FluffyUnderware.CurvyEditor
{
    /// <summary>
    /// SceneView toolbar class
    /// </summary>
    public class Toolbar
    {
        /// <summary>
        /// Spacing of button groups
        /// </summary>
        public static float SPACING = 10;
        
        public enum LabelMode : int
        {
            Text=1,
            Icon =2,
            Full = 15
        }

        public enum Orientation : int
        {
            Top = 1,
            Left =2,
            Right = 3,
            Bottom=4
        }

        /// <summary>
        /// Gets the current selection
        /// </summary>
        public static Object[] Selection { get { return _current; } }
        /// <summary>
        /// Gets the currently selected CurvySpline or null
        /// </summary>
        public static CurvySpline SelectedSpline { get { return (SelectedAny) ? _current[0] as CurvySpline : null; } }
        /// <summary>
        /// Gets the currently selected CurvySplines. The first item is the active object!
        /// </summary>
        public static List<CurvySpline> SelectedSplines
        {
            get
            {
                return getSelection<CurvySpline>();
            }
        }
        /// <summary>
        /// Gets the currently selected CurvySplineSegment or null
        /// </summary>
        public static CurvySplineSegment SelectedControlPoint { get { return (SelectedAny) ? _current[0] as CurvySplineSegment : null; } }
        /// <summary>
        /// Gets the currently selected CurvySplineSegments. The first item is the active object!
        /// </summary>
        public static List<CurvySplineSegment> SelectedControlPoints
        {
            get
            {
                return getSelection<CurvySplineSegment>();
            }
        }
        /// <summary>
        /// Gets the currently selected CurvySplineGroup or null
        /// </summary>
        public static CurvySplineGroup SelectedGroup { get { return (SelectedAny) ? _current[0] as CurvySplineGroup : null; } }
        /// <summary>
        /// Gets the currently selected CurvySplineGroups. The first item is the active object!
        /// </summary>
        public static List<CurvySplineGroup> SelectedGroups
        {
            get
            {
                return getSelection<CurvySplineGroup>();
            }
        }

        

        public static bool SelectedSingle { get { return _current.Length == 1; } }
        public static bool SelectedMultiple { get { return _current.Length > 1; } }
        public static bool SelectedAny { get { return _current.Length > 0; } }

        static List<ToolbarItem> _items = new List<ToolbarItem>();
        static Object[] _current;
        public static Vector2 ItemsMaxDimensions;
        static List<ToolbarItem> RenderedItems = new List<ToolbarItem>();
        static List<Rect> RenderedItemsRect = new List<Rect>();
        

        /// <summary>
        /// Add this to Editor.OnInspectorGUI()
        /// </summary>
        /// <param name="targets">Editor.targets</param>
        public static void RegisterTargets(params Object[] targets)
        {
            if (Event.current.type == EventType.Layout) {
                Release();
                _current = targets;
            } 
        }

        /// <summary>
        /// Renders the toolbar and processes events. Add this to OnSceneGUI()
        /// </summary>
        public static void Render()
        {
            if (_items.Count == 0)
                fetchToolbarItems();
            if (_items.Count > 0)
            {
                renderToolbar();
            }
            ProcessEvents();
        }

        /// <summary>
        /// Add this to Editor.OnSceneGUI() and (optionally) Editor.OnHierarchyWindowItemOnGUI
        /// </summary>
        public static void ProcessEvents()
        {
            Event ev = Event.current;
            if (ev.type == EventType.KeyDown || 
                ev.type == EventType.MouseDown) {
                for (int i = 0; i < _items.Count; i++) {
                    if (_items[i].KeyBinding != null &&
                        ItemIsAllowed(_items[i]) &&
                        _items[i].Active &&
                        _items[i].KeyBinding.IsTriggered(ev))
                    {
                        if (_items[i].Properties.Type != ToolbarItemType.Button)
                            _items[i].Enabled = !_items[i].Enabled;
                        _items[i].Action();
                    }
                }
            }
        }

        /// <summary>
        /// Add this to Editor.OnDisable()
        /// </summary>
        public static void Release()
        {
        }

        /// <summary>
        /// Clears all loaded ToolbarItems, forcing a reload!
        /// </summary>
        public static void Clear()
        {
            _items.Clear();
        }

        /// <summary>
        /// Gets a toolbar item by it's name
        /// </summary>
        /// <param name="name">item name as defined in the ToolbarItemInfo attribute</param>
        public static ToolbarItem GetItem(string name)
        {
            foreach (var item in _items)
                if (item.Properties.Name.Equals(name, System.StringComparison.CurrentCultureIgnoreCase))
                    return item;
            return null;
        }

        public static T GetItem<T>() where T:ToolbarItem
        {
            foreach (var item in _items)
                if (item is T)
                    return (T)item;
            return null;
        }

        /// <summary>
        /// Executes the Action() method of a ToolbarItem (only if the item is allowed for the current selection and Active)
        /// </summary>
        /// <remarks>This is useful to execute items from multiple places</remarks>
        /// <param name="name">item name as defined in the ToolbarItemInfo attribute</param>
        /// <returns>true if the ToolbarItem was allowed and it's Action() method called</returns>
        public static bool ExecuteItemAction (string name){
            var item = GetItem(name);
            if (item != null && ItemIsAllowed(item) && item.Active) {
                item.Action();
                return true;
            }
            return false;
        }

        

        #region ### Privates & Internals ###
        /*! \cond PRIVATE */
        /*! @name Internal Public
         *  Don't use them unless you know what you're doing!
         */
        //@{

        static List<T> getSelection<T>()  where T : Component
        {
            var L=new List<T>();
            foreach (Component O in _current)
                if (O!=null && O is T)
                {
                    if (UnityEditor.Selection.activeTransform==O.transform)
                        L.Insert(0, (T)O);
                    else
                        L.Add((T)O);
                }
            return L;
        }

        static internal void disableRadiogroupExcept(ToolbarItem item)
        {
            int group = item.Properties.Radiogroup;
            for (int i = 0; i < _items.Count; i++) {
                if (_items[i].Properties.Radiogroup == group &&
                    !_items[i].Equals(item)) {
                        _items[i].Enabled = false;
                }
            }
        }

        static void onSceneView(SceneView view)
        {
            if (_items.Count == 0)
                fetchToolbarItems();
            if (_items.Count>0)
                renderToolbar();
        }

        static void renderToolbar()
        {
            RenderedItems.Clear();
            RenderedItemsRect.Clear();

            Handles.BeginGUI();
            
            // Actually draw Toolbar
            int order = 0;
            Rect toolbarRect = getToolbarOrientedRect();
            Vector2 itemSize;
            // Pass I: Draw Toolbar Icons
            for (int i = 0; i < _items.Count; i++) {
                if (ItemIsAllowed(_items[i])) {
                    GUI.enabled = _items[i].Active;
                    GUIContent content = _items[i].GetContent(out itemSize);
                    // spacing?
                    if (_items[i].Properties.Order - order > 10)
                        Space(ref toolbarRect);
                    // In Icon Mode, ensure that ToolbarItems without Icons are wide enough to show their text
                    if (CurvyPreferences.ToolbarLabels == LabelMode.Icon)
                    {
                        toolbarRect.width = (content.image==null) ? ItemsMaxDimensions.x : itemSize.x;
                    }
                    toolbarRect.height = ItemsMaxDimensions.y;
                    // Render item and save Item+Rect, if we need to call OnItemGUI() afterwards
                    if (_items[i].OnGUI(toolbarRect, content))
                    {
                        RenderedItems.Add(_items[i]);
                        RenderedItemsRect.Add(toolbarRect);
                    }
                    if (i<_items.Count-1)
                        Advance(ref toolbarRect, itemSize);
                        
                    GUI.enabled = true;
                    order = _items[i].Properties.Order;
                }
            }
            // Pass II: Draw Item's OnItemGUI() if necessary
            for (int i = 0; i < RenderedItems.Count; i++)
            {
                Rect r=RenderedItemsRect[i];
                switch (CurvyPreferences.ToolbarOrientation)
                {
                    case Orientation.Left:
                        r.x = toolbarRect.x + ItemsMaxDimensions.x + 5;
                        break;
                    case Orientation.Right:
                        r.x = toolbarRect.x - 5;
                        break;
                    case Orientation.Top:
                        r.y = toolbarRect.y + ItemsMaxDimensions.y + 5;
                        break;
                    case Orientation.Bottom:
                        r.y = toolbarRect.y - ItemsMaxDimensions.y - 5;
                        break;
                }
                RenderedItems[i].OnItemGUI(r);
            }
            Handles.EndGUI();
            
        }

        static Rect getToolbarOrientedRect()
        {
            Rect r = new Rect(0, 0, ItemsMaxDimensions.x, ItemsMaxDimensions.y);
            switch (CurvyPreferences.ToolbarOrientation)
            {
                case Toolbar.Orientation.Bottom:
                    r.x = 5;r.y=SceneView.currentDrawingSceneView.position.height-ItemsMaxDimensions.y-30;
                    return r;
                case Toolbar.Orientation.Left:
                    r.x = 5;r.y=10;
                    return r;
                case Toolbar.Orientation.Right:
                    r.x=SceneView.currentDrawingSceneView.position.width-10-ItemsMaxDimensions.x;
                    r.y = 115;
                    return r;
                default: // Top
                    r.x=5; r.y = 10;
                    return r;
            }
        }

        static void Space(ref Rect itemRect) 
        {
            switch (CurvyPreferences.ToolbarOrientation)
            {
                case Toolbar.Orientation.Left:
                case Toolbar.Orientation.Right:
                        itemRect.y += SPACING;
                    break;
                case Toolbar.Orientation.Bottom:
                case Toolbar.Orientation.Top:
                        itemRect.x += SPACING;
                    break;
            }
        }

        // true if wrapping
        static void Advance(ref Rect itemRect, Vector2 itemSize)
        {
            switch (CurvyPreferences.ToolbarOrientation)
            {
                case Toolbar.Orientation.Left:
                    itemRect.y += ItemsMaxDimensions.y + 3;
                    if (itemRect.y + ItemsMaxDimensions.y > SceneView.currentDrawingSceneView.position.height - 30)
                    {
                        itemRect.y = 10;
                        itemRect.x += ItemsMaxDimensions.x+5;
                    }
                    break;
                case Toolbar.Orientation.Right:
                    itemRect.y += ItemsMaxDimensions.y + 3;
                    if (itemRect.y + ItemsMaxDimensions.y > SceneView.currentDrawingSceneView.position.height - 30)
                    {
                        itemRect.y = 10;
                        itemRect.x -= ItemsMaxDimensions.x+5;
                    }
                    break;
                case Toolbar.Orientation.Bottom:
                    itemRect.x += 3 + ItemsMaxDimensions.x;
                    if (itemRect.x + ItemsMaxDimensions.x > SceneView.currentDrawingSceneView.position.width)
                    {
                        itemRect.x = 5;
                        itemRect.y -= ItemsMaxDimensions.y+5;
                    }
                    break;
                case Toolbar.Orientation.Top:
                    itemRect.x += 3 + ItemsMaxDimensions.x;
                    if (itemRect.x + ItemsMaxDimensions.x > SceneView.currentDrawingSceneView.position.width)
                    {
                        itemRect.x = 5;
                        itemRect.y += ItemsMaxDimensions.y+5;
                    }
                    break;
            }
        }

        static void fetchToolbarItems()
        {
            ItemsMaxDimensions = Vector2.zero;
            foreach (System.Type T in Assembly.GetCallingAssembly().GetTypes()){
                if (T.IsSubclassOf(typeof(ToolbarItem))) {
                    object[] attribs=T.GetCustomAttributes(typeof(ToolbarItemInfo), false);
                    if (attribs.Length>0)
                        addItem(T, (ToolbarItemInfo)attribs[0]);
                }
            }
            _items.Sort();
        }

        static bool ItemIsAllowed(ToolbarItem item)
        {
            var flags = item.Properties.AppliesTo;

            if (SelectedAny && flags == ToolbarItemTarget.All)
                return true;

            if (SelectedSingle)
            {
                if (SelectedSpline && (flags & ToolbarItemTarget.Spline) != 0)
                    return true;
                if (SelectedControlPoint && (flags & ToolbarItemTarget.ControlPoint) != 0)
                    return true;
                if (SelectedGroup && (flags & ToolbarItemTarget.Group) != 0)
                    return true;
                
            }


            if (SelectedMultiple)
            {
                if (SelectedSplines.Count > 0 && (flags & ToolbarItemTarget.MultipleSplines) != 0)
                    return true;
                if (SelectedControlPoints.Count > 0 && (flags & ToolbarItemTarget.MultipleControlPoints) != 0)
                    return true;
                if (SelectedGroups.Count > 0 && (flags & ToolbarItemTarget.MultipleGroups) != 0)
                    return true;
                
            }

            return false;
        }

        static void addItem(System.Type type, ToolbarItemInfo properties)
        {
            ToolbarItem handler = (ToolbarItem)System.Activator.CreateInstance(type);
            if (handler!=null) {
                handler.Properties = properties;
                handler.Image = CurvyResource.Load(handler.Properties.Image);
                _items.Add(handler);
                // Get max. Content size or proper toolbar rendering
                Vector2 size;
                handler.GetContent(out size);
                ItemsMaxDimensions = Vector2.Max(ItemsMaxDimensions, size);
            }
        }

        //@}
        /*! \endcond */
        #endregion
       

    }
}
