using UnityEngine;
using System.Collections;
using FluffyUnderware.Curvy;

/* Instead of adding an additional script to look at a given transform, we just derive 
 * from the default FollowSpline component and override it's Refresh() method.
 * 
 * Note: In order to make the Preview working in the editor you'll need to create a stub editor script for it and place it in a folder named editor
 * FollowSplineAndLookAtInspector.cs:
 * 
    using FluffyUnderware.CurvyEditor;

    [CustomEditor(typeof(FollowSplineAndLookAt))]
    public class FollowSplineAndLookAtInspector : CurvyComponentInspector<FollowSplineAndLookAt> { }
 * 
  */
namespace FluffyUnderware.Curvy.Examples
{
    public class FollowSplineAndLookAt : FollowSpline
    {
        [Label(Tooltip = "The Transform to look at")]
        public Transform LookAtTarget;

        public override void Refresh()
        {
            base.Refresh();
            if (LookAtTarget)
                Transform.LookAt(LookAtTarget);
        }
    }
}