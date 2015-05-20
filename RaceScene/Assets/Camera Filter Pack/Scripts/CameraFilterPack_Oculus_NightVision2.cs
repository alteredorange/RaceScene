////////////////////////////////////////////////////////////////////////////////////
//  CAMERA FILTER PACK - by VETASOFT 2014 //////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu ("Camera Filter Pack/Oculus/NightVision2")]
public class CameraFilterPack_Oculus_NightVision2 : MonoBehaviour {
	#region Variables
	public Shader SCShader;
	private float TimeX = 1.0f;
	private Vector4 ScreenResolution;
	private Material SCMaterial;
	[Range(0,1)]
	public float BinocularSize = 0.5f;
	[Range(0,1)]
	public float BinocularDistance = 0.5f;
	[Range(0,1)]
	public float Greenness = 0.4f;

	public static float ChangeBinocularSize ;
	public static float ChangeBinocularDistance;
	public static float ChangeGreenness;

	#endregion
	
	#region Properties
	Material material
	{
		get
		{
			if(SCMaterial == null)
			{
				SCMaterial = new Material(SCShader);
				SCMaterial.hideFlags = HideFlags.HideAndDontSave;	
			}
			return SCMaterial;
		}
	}
	#endregion
	void Start () 
	{
		ChangeBinocularSize = BinocularSize;
		ChangeBinocularDistance = BinocularDistance;
		ChangeGreenness = Greenness;

		SCShader = Shader.Find("CameraFilterPack/Oculus_NightVision2");

		if(!SystemInfo.supportsImageEffects)
		{
			enabled = false;
			return;
		}
	}
	
	void OnRenderImage (RenderTexture sourceTexture, RenderTexture destTexture)
	{
		if(SCShader != null)
		{
			TimeX+=Time.deltaTime;
			if (TimeX>100)  TimeX=0;
			material.SetFloat("_TimeX", TimeX);
			material.SetFloat("_BinocularSize", BinocularSize);
			material.SetFloat("_BinocularDistance", BinocularDistance);
			material.SetFloat("_Greenness", Greenness);
			material.SetVector("_ScreenResolution",new Vector2(Screen.width,Screen.height));
			Graphics.Blit(sourceTexture, destTexture, material);
		}
		else
		{
			Graphics.Blit(sourceTexture, destTexture);	
		}
		
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Application.isPlaying)
		{
			BinocularSize = ChangeBinocularSize;
			BinocularDistance = ChangeBinocularDistance;
			Greenness = ChangeGreenness;
		}
		#if UNITY_EDITOR
		if (Application.isPlaying!=true)
		{
			SCShader = Shader.Find("CameraFilterPack/Oculus_NightVision2");

		}
		#endif

	}
	
	void OnDisable ()
	{
		if(SCMaterial)
		{
			DestroyImmediate(SCMaterial);	
		}
		
	}
	
	
}