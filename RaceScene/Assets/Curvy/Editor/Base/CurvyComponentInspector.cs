using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Utils;

namespace FluffyUnderware.CurvyEditor
{
    /// <summary>
    /// Base Editor class for components inherited from CurvyComponent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [CustomEditor(typeof(CurvyComponent))]
    public class CurvyComponentInspector<T> : CurvyInspector<T> where T:CurvyComponent
    {
        bool mRunningInEditor;
        bool mPreviewFoldout;

        Texture2D mTexPlay;
        Texture2D mTexStop;

        

        protected virtual void OnEnable()
        {
            mTexPlay = CurvyResource.Load("play,24,24");
            mTexStop = CurvyResource.Load("stop,24,24");
        }

        protected virtual void OnDisable()
        {
            StopPreview();
        }

        /// <summary>
        /// Start editor preview
        /// </summary>
        public virtual void StartPreview()
        {
            if (mRunningInEditor)
                Target.Initialize();
            else
            {
                EditorApplication.update -= Target.EditorUpdate;
                EditorApplication.update += Target.EditorUpdate;
                mRunningInEditor = true;
            }
        }

        /// <summary>
        /// Stop editor preview
        /// </summary>
        public virtual void StopPreview()
        {
            EditorApplication.update -= Target.EditorUpdate;
            mRunningInEditor = false;
            Target.Initialize();
        }

        protected void IterateProperties()
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                if (iterator.name != "m_Script" && iterator.name != "InspectorFoldout")
                    EditorGUILayout.PropertyField(iterator, true);
                enterChildren = false;
            }
        }

        /// <summary>
        /// Show the preview buttons
        /// </summary>
        protected void ShowPreviewButtons()
        {
            if (CurvyGUI.Foldout(ref mPreviewFoldout, "Preview"))
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Toggle(mRunningInEditor, new GUIContent(mTexPlay, "Play/Replay in Editor"), GUI.skin.button) != mRunningInEditor)
                    StartPreview();
                if (GUILayout.Button(new GUIContent(mTexStop, "Stop")))
                    StopPreview();
                GUILayout.EndHorizontal();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();
            IterateProperties();
            serializedObject.ApplyModifiedProperties();
            ShowPreviewButtons();
        }
    }

}