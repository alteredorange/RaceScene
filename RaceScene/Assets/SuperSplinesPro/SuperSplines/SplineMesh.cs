using UnityEngine;
using System;

using SplineUtilities;

/**
* @class SplineMesh
*
* @brief This class provides functions for generating curved meshes around a Spline.
*
* This class enables you to dynamically generate curved meshes like streets, rivers, tubes, ropes, tunnels, etc.
*/ 

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("SuperSplines/Spline Mesh")]
public class SplineMesh : MonoBehaviour
{
	public Spline spline;												///< Reference to the spline that defines the path.
	public AutomaticUpdater autoUpdater; 								///< Specifies when the mesh will be updated.
	
	public Mesh baseMesh; 												///< Reference to the base mesh that will be created around the spline.
	public int segmentCount = 50; 										///< Number of segments (base meshes) stringed together per generated mesh.
	
	public UVMode uvMode = UVMode.Normal; 								///< Defines how UV coordinates will be calculated.
	public Vector2 uvScale = Vector2.one; 								///< Affects the scale of texture coordinates on the streched mesh
	public Vector2 xyScale = Vector2.one; 								///< Mesh scale in the local directions around the spline.
	
	public int splineSegment = -1; 										///< Index of the spline segment that will be used as control path. If set to -1, the whole spline will be used.
	
	private Mesh bentMesh = null;
//	private bool autoDestroyMesh = true;
	
	public Mesh BentMesh{ get{ return ReturnMeshReference( ); } } 		///< Returns a reference to the generated mesh.
	public bool IsSubSegment{ get{ return (splineSegment != -1); } }	/// Returns true if the component only computes a part of the whole spline mesh.
	
	void Start( )
	{
		if( spline != null )
			spline.UpdateSpline( );
		
		UpdateMesh( );
	}
	
	void OnEnable( )
	{
		if( spline != null )
			spline.UpdateSpline( );
		
		UpdateMesh( );
	}
	
//	~SplineMesh( )
//	{
//		
//	}
	
	void LateUpdate( )
	{
		if( autoUpdater.Update( ) )
			UpdateMesh( );
	}
	
	/** 
	* This function updates the spline mesh. It is called automatically once in a while, if updateMode isn't set to DontUpdate.
	*/
	public void UpdateMesh( )
	{
		SetupMeshFilter( );
		
		bentMesh.Clear( );
		
		if( baseMesh == null || spline == null || segmentCount <= 0 )
			return;
		
		//Gather model data
		MeshData meshDataBase = new MeshData( baseMesh );
		MeshData meshDataNew = new MeshData( meshDataBase, segmentCount );
		
		SplineSegment currentSegment = null;
		
		if( IsSubSegment )
			currentSegment = spline.SplineSegments[splineSegment];
		
		float param0 = 0f;
		float param1 = 0f;
		
		for( int segmentIdx = 0; segmentIdx < segmentCount; segmentIdx++ )
		{
			if( !IsSubSegment )
			{
				param0 = (float) (segmentIdx) / segmentCount;
				param1 = (float) (segmentIdx+1) / segmentCount;
			}
			else
			{
				param0 = currentSegment.ConvertSegmentToSplineParamter( (float) (segmentIdx) / segmentCount );
				param1 = currentSegment.ConvertSegmentToSplineParamter( (float) (segmentIdx+1) / segmentCount );
			}
			
			GenerateBentMensh( segmentIdx, param0, param1, meshDataBase, meshDataNew );
		}
		
		meshDataNew.AssignToMesh( bentMesh );
	}

	private void GenerateBentMensh( int segmentIdx, float param0, float param1, MeshData meshDataBase, MeshData meshDataNew )
	{
		Vector3 pos0 = spline.transform.InverseTransformPoint(spline.GetPositionOnSpline( param0 ));
		Vector3 pos1 = spline.transform.InverseTransformPoint(spline.GetPositionOnSpline( param1 ));
		
		Quaternion rot0 = spline.GetOrientationOnSpline( param0 ) * Quaternion.Inverse( spline.transform.rotation );
		Quaternion rot1 = spline.GetOrientationOnSpline( param1 ) * Quaternion.Inverse( spline.transform.rotation );
		
		Quaternion targetRot = Quaternion.identity;
		
		Vector3 vertex = Vector3.zero;
		Vector2 uvCoord = Vector2.zero;
		
		float normalizedZPos = 0f;
		
		int newVertexIndex = meshDataBase.VertexCount * segmentIdx;
		
		for( int i = 0; i < meshDataBase.VertexCount; i++, newVertexIndex++ )
		{
			vertex = meshDataBase.vertices[i];
			uvCoord = meshDataBase.uvCoord[i];
			
			normalizedZPos = vertex.z + 0.5f;
			
			targetRot = Quaternion.Lerp( rot0, rot1, normalizedZPos );
			
			vertex.Scale( new Vector3( xyScale.x, xyScale.y, 0 ) );
			
			vertex = targetRot * vertex;
			vertex = vertex + Vector3.Lerp( pos0, pos1, normalizedZPos );
			
			meshDataNew.vertices[newVertexIndex] = vertex;
			
			if( meshDataBase.HasNormals )
				meshDataNew.normals[newVertexIndex] = targetRot * meshDataBase.normals[i];
			
			if( meshDataBase.HasTangents )
				meshDataNew.tangents[newVertexIndex] = targetRot * meshDataBase.tangents[i];
			
			if( uvMode == UVMode.Normal )
				uvCoord.y = Mathf.Lerp( param0, param1, normalizedZPos );
			else if( uvMode == UVMode.Swap )
				uvCoord.x = Mathf.Lerp( param0, param1, normalizedZPos );
			
			meshDataNew.uvCoord[newVertexIndex] = Vector2.Scale( uvCoord, uvScale );
		}
		
		for( int i = 0; i < meshDataBase.TriangleCount; i++ )
			meshDataNew.triangles[i + (segmentIdx * meshDataBase.TriangleCount)] = meshDataBase.triangles[i] + (meshDataBase.VertexCount * segmentIdx);
	}
		
	private void SetupMeshFilter( )
	{
		if( bentMesh == null )
		{
			bentMesh = new Mesh( );
		
			bentMesh.name = "BentMesh";
			bentMesh.hideFlags = HideFlags.HideAndDontSave;
		}
		
		MeshFilter meshFilter = GetComponent<MeshFilter>( );
		
		if( meshFilter.sharedMesh != bentMesh )
			meshFilter.sharedMesh = bentMesh;
		
		
		MeshCollider meshCollider = GetComponent<MeshCollider>( );
		
		if( meshCollider != null )
		{
			meshCollider.sharedMesh = null;
			meshCollider.sharedMesh = bentMesh;
		}
	}
	
	private Mesh ReturnMeshReference( )
	{
//		if( !autoDestroyMesh )
			return bentMesh;
		
//		throw new Exception( "The autodestroy function of this SplineMesh-component is enabled. " +
//			"In order to prevent 'MissingReferences' the acces the 'BentMesh'-property is blocked." +
//			"In case you need to access it, turn off autodestruction in the Inspector." );
	}
	
	private class MeshData
	{
		public Vector3[] vertices;
		public Vector2[] uvCoord;
		public Vector3[] normals;
		public Vector4[] tangents;
		
		public int[] triangles;
		
		public Bounds bounds;
		
		public bool HasNormals{ get{ return normals.Length > 0; } }
		public bool HasTangents{ get{ return tangents.Length > 0; } }
		
		public int VertexCount{ get{ return vertices.Length; } }
		public int TriangleCount{ get{ return triangles.Length; } }
		
		public MeshData( Mesh mesh )
		{
			vertices = mesh.vertices;
			normals = mesh.normals;
			tangents = mesh.tangents;
			uvCoord = mesh.uv;
			
			triangles = mesh.triangles;
			
			bounds = mesh.bounds;
		}
		
		public MeshData( MeshData mData, int segmentCount )
		{
			vertices = new Vector3[mData.vertices.Length * segmentCount];
			uvCoord = new Vector2[mData.uvCoord.Length * segmentCount];
			normals = new Vector3[mData.normals.Length * segmentCount];
			tangents = new Vector4[mData.tangents.Length * segmentCount];
			triangles = new int[mData.triangles.Length * segmentCount];
		}
		
		public void AssignToMesh( Mesh mesh )
		{
			mesh.vertices = vertices;
			mesh.uv = uvCoord;
			
			if( HasNormals )
				mesh.normals = normals;
			
			if( HasTangents )
				mesh.tangents = tangents;
			
			mesh.triangles = triangles;
		}
	}
	
	/**
	* @enum UVMode
	* Defines how the SplineMesh class will calculate UV-coordinates.
	*/ 
	public enum UVMode
	{
		Normal, ///< UV coordinates will be interpolated on the V-axis
		Swap, ///< UV coordinates will be interpolated on the U-axis
		DontInterpolate ///< UV coordinates will simply be copied from the base mesh and won't be altered.
	}
}
