using UnityEngine;
using UnityEditor;
using System.Collections;
using FluffyUnderware.Curvy;

namespace FluffyUnderware.CurvyEditor
{

    public class CurvyMenu
    {
        #region ### Create Menu ###

        [MenuItem("GameObject/Create Other/Curvy/Spline", false, 0)]
        static void CreateCurvySpline()
        {
            CurvySpline spl = null;
            // If there's a Default Prefab, use that

            GameObject def = Resources.Load("DefaultCurvySpline") as GameObject;
            if (def)
            {
                GameObject splGO = PrefabUtility.InstantiatePrefab(def) as GameObject;
                if (splGO)
                {
                    PrefabUtility.DisconnectPrefabInstance(splGO);
                    splGO.name = "Curvy Spline";
                    spl = splGO.GetComponent<CurvySpline>();
                }
            }
            if (spl == null)
            {
                // Otherwise create a spline from scratch
                spl = CurvySpline.Create();
                spl.Interpolation = CurvyInterpolation.CatmulRom;
                spl.AutoEndTangents = true;
                spl.Closed = true;
                spl.Add(new Vector3(-2, -1, 0), new Vector3(0, 2, 0), new Vector3(2, -1, 0));

            }
            Selection.activeObject = spl;
            Undo.RegisterCreatedObjectUndo(spl.gameObject, "Create Spline");
        }


        [MenuItem("GameObject/Create Other/Curvy/Components/Align To Spline", false, 100)]
        static void CreateAlignToSpline()
        {
            CreateCurvyObject<AlignToSpline>("Align To Spline", true);
        }

        [MenuItem("GameObject/Create Other/Curvy/Components/Follow Spline", false, 101)]
        static void CreateFollowSpline()
        {
            CreateCurvyObject<FollowSpline>("Follow Spline", true);
        }

        

        #endregion

        #region ### CONTEXT Help ###

        [MenuItem("CONTEXT/CurvySpline/Help (CurvySpline)")]
        static void CTXCurvySpline() { Application.OpenURL(CurvyEditorUtility.HelpURL("curvyspline")); }

        [MenuItem("CONTEXT/CurvySplineSegment/Help (CurvySplineSegment)")]
        static void CTXCurvySplineSegment() { Application.OpenURL(CurvyEditorUtility.HelpURL("curvysplinesegment")); }

        [MenuItem("CONTEXT/CurvySplineGroup/Help (CurvySplineGroup)")]
        static void CTXCurvySplineGroup() { Application.OpenURL(CurvyEditorUtility.HelpURL("curvysplinegroup")); }

        [MenuItem("CONTEXT/AlignToSpline/Help (AlignToSpline)")]
        static void CTXAlignToSpline() { Application.OpenURL(CurvyEditorUtility.HelpURL("aligntospline")); }

        [MenuItem("CONTEXT/FollowSpline/Help (FollowSpline)")]
        static void CTXFollowSpline() { Application.OpenURL(CurvyEditorUtility.HelpURL("followspline")); }


        #endregion


        static T CreateCurvyObject<T>(string name, bool addToSelectedObjects) where T : MonoBehaviour
        {
            GameObject[] go = new GameObject[0];
            if (addToSelectedObjects)
                go = Selection.gameObjects;
            if (go.Length==0)
            {
                go = new GameObject[1] { new GameObject(name) };
                Undo.RegisterCreatedObjectUndo(go[0], "Create " + name);
            }
            T[] obj = new T[go.Length];
            for (int i=0;i<go.Length;i++)
            {
                obj[i] = go[i].AddComponent<T>();
                Undo.RegisterCreatedObjectUndo(obj[i], "Create " + name);
            }
            return obj[0];
        }
    }
}