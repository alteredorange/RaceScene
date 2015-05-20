using UnityEngine;
using System.Collections;
using FluffyUnderware.Curvy;

namespace FluffyUnderware.Curvy.Examples
{
    public class MouseAddControlPoint : MonoBehaviour
    {
        public bool RemoveUnusedSegments = true;
        CurvySpline mSpline;
        FollowSpline Walker;

        // Use this for initialization
        IEnumerator Start()
        {
            mSpline = GetComponent<CurvySpline>();
            Walker = GameObject.FindObjectOfType(typeof(FollowSpline)) as FollowSpline;
            while (!mSpline.IsInitialized)
                yield return null;
        }

        // Update is called once per frame
        void Update()
        {
            // Add Control Point by mouseclick
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 p = Input.mousePosition;
                p.z = 10;
                p = Camera.main.ScreenToWorldPoint(p);
                mSpline.Add(p);
                // remove segments we've already passed
                if (RemoveUnusedSegments)
                {
                    var current = Walker.CurrentSegment;
                    if (current)
                    {
                        // If we're not on the first segment, we can savely remove some Control Points!
                        // NOTE: While we usually would have used current.SegmentIndex to make it work with disabled AutoEndTangents (where the first CP isn't the first segment),
                        // we use ControlPointIndex here for single good reason:  
                        // We don't want to refresh the spline each time we delete a segment, but just at the end of the deletion process. Deleting a ControlPoint resets the segment hierarchy
                        // (it will be repopulated during Refresh), so we can't use SegmentIndex nor mSpline[n] to access the first segment in the deletion loop.
                        int curIdx = current.ControlPointIndex;
                        int toDelete = curIdx-2; // keep two extra segments to prevent curve change of active segment

                        if (toDelete>0) {
                            for (int i=0;i<toDelete;i++)
                            mSpline.Delete(mSpline.ControlPoints[0], false); // don't refresh! Warning: This resets Segment info of the spline!
                        
                            mSpline.RefreshImmediately(); // now refresh the spline
                        }
                        
                    }

                }
            }
        }


    }
}