using UnityEngine;
using System.Collections;
using FluffyUnderware.Curvy.Utils;

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Base Class for Curvy Controllers, working in the editor
    /// </summary>
    [ExecuteInEditMode]
    public class CurvyComponent : MonoBehaviour
    {
        public delegate void CurvyComponentEvent(CurvyComponent sender);

        /// <summary>
        /// Determines when to update
        /// </summary>
        [Label(Tooltip="Determines when to update")]
        public CurvyUpdateMethod UpdateIn = CurvyUpdateMethod.Update; // when to update?


        /// <summary>
        /// Gets the (cached) transform
        /// </summary>
        public Transform Transform
        {
            get
            {
                if (!mTransform)
                    mTransform = transform;
                return mTransform;
            }
        }
        Transform mTransform;

        /// <summary>
        /// Gets Time.deltaTime - even in the editor!
        /// </summary>
        public float DeltaTime
        {
            get { return CurvyUtility.DeltaTime; }
        }

        /// <summary>
        /// Called about 100 times a second when the component is selected
        /// </summary>
        public virtual void EditorUpdate()
        {
            CurvyUtility.SetEditorTiming();
        }

        /// <summary>
        /// Use this for initialization
        /// </summary>
        /// <returns></returns>
        public virtual bool Initialize() { return false; }
    }
}