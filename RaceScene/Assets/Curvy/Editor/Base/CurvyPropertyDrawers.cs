// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define OLD_UNITY
#endif
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using System;
using System.Collections.Generic;
using FluffyUnderware.CurvyEditor;
using FluffyUnderware.Curvy;

namespace FluffyUnderware.CurvyEditor 
{
   
    [CustomPropertyDrawer(typeof(PositiveAttribute))]
    public class PositivePropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            label = EditorGUI.BeginProperty(position, label, property);
            label.tooltip = ((PositiveAttribute)attribute).Tooltip;
            
            position.x += 5;
            position.width -= 10;
            
            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                    property.floatValue = Mathf.Max(0f, EditorGUI.FloatField(position, label, property.floatValue));
                    break;
                case SerializedPropertyType.Integer:
                    property.intValue = Mathf.Max(0, EditorGUI.IntField(position, label, property.intValue));
                    break;
            }
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(RangeExAttribute))]
    internal sealed class RangeExDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RangeExAttribute rangeAttribute = (RangeExAttribute)base.attribute;

            label = EditorGUI.BeginProperty(position, label, property);
            label.tooltip = rangeAttribute.Tooltip;

            position.x += 5;
            position.width -= 10;

            if (property.propertyType == SerializedPropertyType.Float)
            {
                EditorGUI.Slider(position, property, rangeAttribute.Min, rangeAttribute.Max, label);
            }
            else
            {
                if (property.propertyType == SerializedPropertyType.Integer)
                {
                    EditorGUI.IntSlider(position, property, (int)rangeAttribute.Min, (int)rangeAttribute.Max, label);
                }
                else
                {
                    EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
                }
            }
            EditorGUI.EndProperty();
        }
    }

#if OLD_UNITY  // 4.2 and older Label Drawer 
    [CustomPropertyDrawer(typeof(LabelAttribute))]
    public class LabelDrawer : PropertyDrawer
    {
        static Dictionary<string, PropertyDrawer> _dictionary = new Dictionary<string, PropertyDrawer>();
        static string _currentSection;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_dictionary.Count == 0)
                _dictionary = typeof(PropertyDrawer).GetField("s_PropertyDrawers", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null) as Dictionary<string, PropertyDrawer>;

            
            var L = (LabelAttribute)attribute;

            if (!string.IsNullOrEmpty(L.Text))
                label.text = L.Text;
            label.tooltip = L.Tooltip;

            // Save settings
            
            int previousIndentLevel = EditorGUI.indentLevel;

            int indentMod = previousIndentLevel - property.depth;

            foreach (var entry in _dictionary)
            {
                if (entry.Value == this)
                {
                    _dictionary[entry.Key] = null;
                    
                    
                        position.width -= 5;
                        if (position.x == 0)
                        {
                            position.x += 4;
                            position.width -= 4;
                        }
                    EditorGUI.indentLevel = property.depth + indentMod;
                    EditorGUI.PropertyField(position, property, label, true);
                    
                    _dictionary[entry.Key] = this;
                    return;
                }
            }
        }
    }
#else // 4.3+ LabelDrawer

    [CustomPropertyDrawer(typeof(LabelAttribute))]
    public class LabelDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var L = (LabelAttribute)attribute;

            if (!string.IsNullOrEmpty(L.Text))
                label.text = L.Text;
            label.tooltip = L.Tooltip;

            // Save settings
            bool wasEnabled = GUI.enabled;
            int previousIndentLevel = EditorGUI.indentLevel;

            int indentMod = previousIndentLevel - property.depth;
            position.height = 16f;

            SerializedProperty serializedProperty = property.Copy();
            SerializedProperty endProperty = serializedProperty.GetEndProperty();
            EditorGUI.indentLevel = serializedProperty.depth + indentMod;
            bool enterChildren = EditorGUI.PropertyField(position, serializedProperty, label) && serializedProperty.hasVisibleChildren;

            position.y += EditorGUI.GetPropertyHeight(serializedProperty, label, false);
            position.y += 2;

            while (serializedProperty.NextVisible(enterChildren) && !SerializedProperty.EqualContents(serializedProperty, endProperty))
            {
                EditorGUI.indentLevel = serializedProperty.depth + indentMod;

                EditorGUI.BeginChangeCheck();
                enterChildren = (serializedProperty.hasVisibleChildren && EditorGUI.PropertyField(position, serializedProperty));
                if (EditorGUI.EndChangeCheck()) break;

                position.y += 2;
                position.y += EditorGUI.GetPropertyHeight(serializedProperty);
            }

            // Restore settings
            GUI.enabled = wasEnabled;
            EditorGUI.indentLevel = previousIndentLevel;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
    }
#endif

    [CustomPropertyDrawer(typeof(CurvyVector))]
    [CustomPropertyDrawer(typeof(CurvyVectorRelative))]
    public class CurvyVectorPropertyDrawer : PropertyDrawerEx
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (property.isExpanded) ? 53 : 17;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            mControlRect = position;
            mControlRect.height = 17;

            label = EditorGUI.BeginProperty(position, label, property);

            if (ShowFoldout(mControlRect, property, label))
            {


                var ppPos = property.FindPropertyRelative("m_Position");
                var ppDir = property.FindPropertyRelative("m_Direction");
                float max = 1;
                var vec = GetPropertyTargetField(property) as CurvyVector;
                if (vec != null)
                    max = (vec.MaxDistance == -1) ? 1 : vec.MaxDistance;
                EditorGUI.Slider(mControlRect, ppPos, 0, max);
                mControlRect.y += mControlRect.height;

                Rect r = EditorGUI.PrefixLabel(mControlRect, 0, new GUIContent("Direction"));
                r.width /= 2;
                bool v = GUI.Toggle(r, (ppDir.intValue < 0), "Backward", GUI.skin.button);
                if (v && ppDir.intValue > 0)
                    ppDir.intValue = -1;
                r.x += r.width;
                v = GUI.Toggle(r, (ppDir.intValue > 0), "Forward", GUI.skin.button);
                if (v && ppDir.intValue < 0)
                    ppDir.intValue = 1;
                EndFoldout();
            }

            EditorGUI.EndProperty();
        }


    }

    public class PropertyDrawerEx : PropertyDrawer
    {
        protected Rect mControlRect;

        protected bool ShowFoldout(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true, CurvyGUI.stFoldout);
            EditorGUI.EndProperty();
            if (property.isExpanded)
                mControlRect.y += mControlRect.height;
            EditorGUI.indentLevel++;
            mControlRect.width -= 5;

            return property.isExpanded;
        }

        protected void EndFoldout()
        {
            EditorGUI.indentLevel--;
            mControlRect.width += 5;
        }

        protected void PropertyField(GUIContent label, SerializedProperty property)
        {
            EditorGUI.PropertyField(mControlRect, property, label, false);
            mControlRect.y += EditorGUI.GetPropertyHeight(property) + 1;
        }

        protected object GetPropertyTargetField(SerializedProperty property)
        {
            var instance = property.serializedObject.targetObject;
            var t = instance.GetType();
            FieldInfo fi = null;
            while (fi == null && t != null)
            {
                fi = t.GetField(property.propertyPath, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (fi == null)
                    t = t.BaseType;
            }
            return (fi != null) ? fi.GetValue(instance) : null;
        }

    }

  
   
}