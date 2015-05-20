using UnityEngine;
using UnityEditor;

namespace FluffyUnderware.CurvyEditor
{
    public class CurvyInspector<T> : Editor where T:MonoBehaviour
    {
        public virtual T Target
        {
            get
            {
                return target as T;
            }
        }
    }
}