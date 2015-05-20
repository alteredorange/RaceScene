// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Utils;
using System.Collections.Generic;

namespace FluffyUnderware.CurvyEditor
{
    public class CurvySplineExportWizard : EditorWindow
    {
        const int CLOSEDSHAPE = 0;
        const int VERTEXLINE = 1;

        // SOURCES
        public List<SplinePolyLine> Curves = new List<SplinePolyLine>();

        public string TriangulationMessage = string.Empty;

        bool refreshNow = true;
        public int Mode;
        public Material Mat;
        public Vector2 UVOffset = Vector2.zero;
        public Vector2 UVTiling = Vector2.one;
        
        public bool UV2;
        public string MeshName = "CurvyMeshExport";
        public bool[] FoldOuts = new bool[4] { true, true, true, true };

        public CurvySplineGizmos GizmoState;

        public GameObject previewGO;
        public MeshFilter previewMeshFilter;
        public MeshRenderer previewMeshRenderer;
        
        public Vector2 scroll;

        Texture2D mTexDelete;
        Texture2D mTexSave;
        Texture2D mTexGO;
        Texture2D mDefaultMatTexture;
        
        GUIStyle mRedLabel;

        Mesh previewMesh
        {
            get
            {
                return previewMeshFilter.sharedMesh;
            }
            set
            {
                previewMeshFilter.sharedMesh = value;
            }
        }

        static public void Create()
        {
            var win = GetWindow<CurvySplineExportWizard>(true, "Export Curvy Spline", true);
            win.Init(Selection.activeGameObject.GetComponent<CurvySplineBase>());
            win.minSize = new Vector2(400, 355);
            SceneView.onSceneGUIDelegate -= win.Preview;
            SceneView.onSceneGUIDelegate += win.Preview;
        }

        void OnEnable()
        {
            GizmoState = CurvySpline.Gizmos;
            CurvySpline.Gizmos = CurvySplineGizmos.Curve;
            mTexDelete = CurvyResource.Load("deletesmall,12,12");
            mTexSave = CurvyResource.Load("save,24,24");
            mTexGO = CurvyResource.Load("gameobject,24,24");
            mDefaultMatTexture = new Texture2D(32, 32);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    mDefaultMatTexture.SetPixel(x, y, Color.white);
                    mDefaultMatTexture.SetPixel(x + 16, y, Color.black);
                    mDefaultMatTexture.SetPixel(x, y + 16, Color.black);
                    mDefaultMatTexture.SetPixel(x + 16, y + 16, Color.white);
                }
            mDefaultMatTexture.Apply();
            if (!previewGO)
            {
                previewGO = new GameObject("ExportPreview");
                previewGO.hideFlags = HideFlags.HideAndDontSave;
                previewMeshRenderer = previewGO.AddComponent<MeshRenderer>();
                previewMeshFilter = previewGO.AddComponent<MeshFilter>();
                if (!Mat)
                {
                    Mat = new Material(Shader.Find("Diffuse"));
                    Mat.name = "Preview";
                    Mat.mainTexture = mDefaultMatTexture;
                }
                previewMeshRenderer.material = Mat;
            }
        }

        void OnDisable()
        {
            CurvySpline.Gizmos = GizmoState;
        }

        void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= Preview;
            foreach (var crv in Curves)
                UnhookSpline(crv.Spline);
            Curves.Clear();
            SceneView.RepaintAll();
            GameObject.DestroyImmediate(previewGO);
        }

        void OnFocus()
        {
            SceneView.onSceneGUIDelegate -= Preview;
            SceneView.onSceneGUIDelegate += Preview;
        }

        void Init(CurvySplineBase spline)
        {
            Curves.Add(new SplinePolyLine(spline));
            HookSpline(spline);
        }


        Mesh clonePreviewMesh()
        {
            Mesh msh = new Mesh();
            msh.vertices = previewMesh.vertices;
            msh.triangles = previewMesh.triangles;
            msh.uv = previewMesh.uv;
            msh.uv2 = previewMesh.uv2;
            msh.RecalculateNormals();
            msh.RecalculateBounds();
            return msh;
        }

        void OnSourceRefresh(CurvySplineBase spline)
        {
            refreshNow = true;
        }

        void HookSpline(CurvySplineBase spline)
        {
            if (!spline) return;
            spline.OnRefresh -= OnSourceRefresh;
            spline.OnRefresh += OnSourceRefresh;
        }

        void UnhookSpline(CurvySplineBase spline)
        {
            if (!spline) return;
            spline.OnRefresh -= OnSourceRefresh;
        }

        void OnGUI()
        {
            if (Curves.Count == 0)
                return;

            scroll = EditorGUILayout.BeginScrollView(scroll);

            Mode = GUILayout.SelectionGrid(Mode, new GUIContent[] 
                    {
                        new GUIContent("Closed Shape","Export a closed shape with triangles"),
                        new GUIContent("Vertex Line","Export a vertex line")
                    }, 2);

            if (!string.IsNullOrEmpty(TriangulationMessage) && !TriangulationMessage.Contains("Angle must be >0"))
                EditorGUILayout.HelpBox(TriangulationMessage, MessageType.Error);

            // OUTLINE
            if (CurvyGUI.Foldout(ref FoldOuts[0],"Outline Spline"))
            {
                CurveGUI(Curves[0], false);
            }
            if (Mode == CLOSEDSHAPE)
            {
                // HOLES
                if (CurvyGUI.Foldout(ref FoldOuts[1],"Holes"))
                {
                    for (int i = 1; i < Curves.Count; i++)
                    {
                        CurveGUI(Curves[i], true);
                    }
                    if (GUILayout.Button("Add Hole"))
                        Curves.Add(new SplinePolyLine(null));

                }
            }

            // TEXTURING
            if (CurvyGUI.Foldout(ref FoldOuts[2],"Texturing"))
            {
                Mat = (Material)EditorGUILayout.ObjectField("Material", Mat, typeof(Material), true);
                UVTiling = EditorGUILayout.Vector2Field("Tiling", UVTiling);
                UVOffset = EditorGUILayout.Vector2Field("Offset", UVOffset);
                
            }
            // EXPORT
            
            if (CurvyGUI.Foldout(ref FoldOuts[3],"Export"))
            {
                EditorGUILayout.HelpBox("Export is 2D (x/y) only!", MessageType.Info);
                MeshName = EditorGUILayout.TextField("Mesh Name", MeshName);
                UV2 = EditorGUILayout.Toggle("Add UV2", UV2);

                GUILayout.BeginHorizontal();

                if (GUILayout.Button(new GUIContent(mTexSave,"Save as Asset")))
                {
                    string path = EditorUtility.SaveFilePanelInProject("Save Mesh", MeshName + ".asset", "asset", "Choose a file location");
                    if (!string.IsNullOrEmpty(path))
                    {
                        Mesh msh = clonePreviewMesh();
                        if (msh)
                        {
                            msh.name = MeshName;
                            AssetDatabase.DeleteAsset(path);
                            AssetDatabase.CreateAsset(msh, path);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                            Debug.Log("Curvy Export: Mesh Asset saved!");
                        }
                    }
                }

                if (GUILayout.Button(new GUIContent(mTexGO,"Create GameObject")))
                {
                    Mesh msh = clonePreviewMesh();
                    if (msh)
                    {
                        msh.name = MeshName;
                        var go = new GameObject(MeshName, typeof(MeshRenderer), typeof(MeshFilter));
                        go.GetComponent<MeshFilter>().sharedMesh = msh;
                        go.GetComponent<MeshRenderer>().sharedMaterial = Mat;
                        Selection.activeGameObject = go;
                        Debug.Log("Curvy Export: GameObject created!");
                    }
                    else
                        Debug.LogWarning("Curvy Export: Unable to triangulate spline!");

                }
                GUILayout.EndHorizontal();

            }
            EditorGUILayout.EndScrollView();

            refreshNow = GUI.changed;

        }

        void CurveGUI(SplinePolyLine curve, bool canRemove)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            CurvySplineBase o = curve.Spline;
            curve.Spline = (CurvySplineBase)EditorGUILayout.ObjectField(new GUIContent("Spline", "Note: Curves from a SplineGroup needs to be connected!"), curve.Spline, typeof(CurvySplineBase), true);
            if (o != curve.Spline)
            {
                UnhookSpline(o);
            }
            HookSpline(curve.Spline);
            if (canRemove)
            {
                if (GUILayout.Button(new GUIContent(mTexDelete, "Remove"), GUILayout.ExpandWidth(false)))
                {
                    if (curve.Spline)
                        UnhookSpline(curve.Spline);
                    Curves.Remove(curve);
                    refreshNow = true;
                    GUIUtility.ExitGUI();
                }
            }
            GUILayout.EndHorizontal();

            curve.VertexMode = (SplinePolyLine.VertexCalculation)EditorGUILayout.EnumPopup(new GUIContent("Vertex Generation"), curve.VertexMode);

            switch (curve.VertexMode)
            {
                case SplinePolyLine.VertexCalculation.ByAngle:
                    curve.Angle = Mathf.Max(0,EditorGUILayout.FloatField(new GUIContent("Angle"), curve.Angle));
                    if (curve.Angle == 0)
                        EditorGUILayout.HelpBox("Angle must be >0", MessageType.Error);
                    curve.Distance = EditorGUILayout.FloatField(new GUIContent("Minimum Distance"), curve.Distance);
                    break;
            }
            GUILayout.EndVertical();

        }

        void Update()
        {
            if (Curves.Count == 0)
            {
                Close();
                return;
            }

            if (refreshNow)
            {
                previewMeshRenderer.sharedMaterial = Mat;
                refreshNow = false;
                Spline2Mesh s2m = new Spline2Mesh();
                s2m.Outline = Curves[0];
                s2m.VertexLineOnly = (Mode == VERTEXLINE);
                for (int i = 1; i < Curves.Count; i++)
                    s2m.Holes.Add(Curves[i]);
                
                s2m.UVOffset = UVOffset;
                s2m.UVTiling = UVTiling;
                s2m.UV2 = UV2;
                s2m.MeshName = MeshName;
                s2m.SetBounds(true, Vector3.one);
                Mesh m;
                s2m.Apply(out m);
                previewMesh = m;
                
                TriangulationMessage = s2m.Error;
                if (previewMesh)
                    if (previewMesh.triangles.Length > 0)
                        title = "Export Curvy Spline (" + previewMeshFilter.sharedMesh.vertexCount + " Vertices, " + previewMeshFilter.sharedMesh.triangles.Length / 3 + " Triangles)";
                    else
                        title = "Export Curvy Spline (" + previewMeshFilter.sharedMesh.vertexCount + " Vertices)";
                else
                    title = "Export Curvy Spline";

                SceneView.RepaintAll();
            }
        }

        void Preview(SceneView sceneView)
        {
            if (!previewMesh)
                return;
            float zOffset = Curves[0].Spline.Transform.position.z;
            previewGO.transform.position = new Vector3(previewGO.transform.position.x, previewGO.transform.position.y, zOffset);

            Vector3[] vts = previewMeshFilter.sharedMesh.vertices;
            int[] tris = previewMeshFilter.sharedMesh.triangles;
            Handles.color = Color.green;
            Handles.matrix = Matrix4x4.TRS(new Vector3(0, 0, zOffset), Quaternion.identity, Vector3.one);
            for (int i = 0; i < tris.Length; i += 3)
                Handles.DrawPolyLine(vts[tris[i]], vts[tris[i + 1]], vts[tris[i + 2]], vts[tris[i]]);

            Handles.color = Color.gray;
            for (int i = 0; i < vts.Length; i++)
                Handles.CubeCap(0, vts[i], Quaternion.identity, HandleUtility.GetHandleSize(vts[i]) * 0.07f);

        }

    }
}