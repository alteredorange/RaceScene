using UnityEngine;
using System.Collections;
using FluffyUnderware.Curvy;
namespace FluffyUnderware.Curvy.Examples
{
    public class FollowSplineExampleController : MonoBehaviour
    {
        public FollowSpline[] Controllers;
        int selection;

        void Start()
        {
            SetRelative();
        }

        void OnGUI()
        {
            if (GUILayout.Button("Reset"))
                foreach (var ctrl in Controllers)
                {
                    ctrl.Initialize();
                }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Movement Mode: ");
            int newsel = GUILayout.SelectionGrid(selection, new string[] { "Relative", "Absolute" }, 2);
            GUILayout.EndHorizontal();
            if (newsel != selection)
            {
                selection = newsel;
                switch (selection)
                {
                    case 0: SetRelative(); break;
                    case 1: SetAbsolute(); break;
                }
            }

        }

        void SetRelative()
        {
            foreach (var ctrl in Controllers)
            {
                ctrl.Mode = FollowSpline.FollowMode.Relative;
                ctrl.Speed = 0.2f;
            }

        }
        void SetAbsolute()
        {
            foreach (var ctrl in Controllers)
            {
                ctrl.Mode = FollowSpline.FollowMode.AbsoluteExtrapolate;
                ctrl.Speed = 4f;
            }
        }
    }
}