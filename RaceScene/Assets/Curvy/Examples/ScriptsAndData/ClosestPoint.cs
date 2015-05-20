using UnityEngine;
using System.Collections;
namespace FluffyUnderware.Curvy.Examples
{
    public class ClosestPoint : MonoBehaviour
    {
        public CurvySplineBase Target;
        public Transform TargetTransform;

        // Update is called once per frame
        void LateUpdate()
        {
            if (Target && Target.IsInitialized && TargetTransform)
            {
                float tf = Target.GetNearestPointTF(transform.position);
                TargetTransform.position = Target.Interpolate(tf);
            }
        }
    }
}