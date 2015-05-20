using System;
using UnityEngine;
public class HUD : MonoBehaviour
{
	public KartController vehicle;
	public Texture speedometer;
	public Texture speedoNeedle;
	private void OnGUI()
	{
		float num = 0.9f * (float)Screen.width - 128f;
		float num2 = 0.9f * (float)Screen.height;
		GUI.DrawTexture(new Rect(num - 128f, num2 - 128f, 256f, 128f), this.speedometer);
		float num3 = 50f;
		float num4 = Mathf.Max(0f, this.vehicle.MPH) / num3;
		float num5 = Mathf.Lerp(-80f, 80f, num4);
		float num6 = num - 16f;
		float num7 = num2 - 8f;
		GUIUtility.RotateAroundPivot(num5, new Vector2(num6, num7));
		GUI.DrawTexture(new Rect(num6 - 64f, num7 - 96f, 128f, 128f), this.speedoNeedle);
	}
}
