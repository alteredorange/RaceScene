// =====================================================================
// Copyright 2013-2014 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections;
using System;

namespace FluffyUnderware.Curvy
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class PCModuleSelectorAttribute : PropertyAttribute
    {
        public string MissingMessage = "Source missing";
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class NetworkNodeSelectorAttribute : PropertyAttribute
    {
        public string Tooltip;

        public NetworkNodeSelectorAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class NetworkEdgeSelectorAttribute : PropertyAttribute
    {
        public string Tooltip;

        public NetworkEdgeSelectorAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }
    }

    /// <summary>
    /// Keeps a float/int greater than 0
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class PositiveAttribute : PropertyAttribute
    {
        public string Tooltip;

        public PositiveAttribute() { }

        public PositiveAttribute(string tooltip)
        {
            this.Tooltip = tooltip;
        }
    }

    /// <summary>
    /// Keeps a float/int inside a certain range
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class RangeExAttribute : PropertyAttribute
    {
        public string Tooltip;
        public float Min;
        public float Max;

        public RangeExAttribute(float min, float max)
		{
			this.Min = min;
			this.Max = max;
		}

        public RangeExAttribute(float min, float max, string tooltip)
        {
            this.Min = min;
            this.Max = max;
            this.Tooltip = tooltip;
        }
    }

    /// <summary>
    /// Changes the label and/or tooltip
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class LabelAttribute : PropertyAttribute
    {
        public string Text=string.Empty;
        public string Tooltip;

        public LabelAttribute() { }
        public LabelAttribute(string text) : this(text,null) { }

        public LabelAttribute(string text, string tooltip)
        {
            this.Text = text;
            this.Tooltip = tooltip;
        }
    }

/*
    [AttributeUsage(AttributeTargets.Field,Inherited=true,AllowMultiple=false)]
    public class VectorAttribute : PropertyAttribute
    {
        public string Text=string.Empty;
        public string Tooltip;

        public VectorAttribute() { }
        public VectorAttribute(string text) : this(text,null) { }

        public VectorAttribute(string text, string tooltip)
        {
            this.Text = text;
            this.Tooltip = tooltip;
        }
    }*/
    
}