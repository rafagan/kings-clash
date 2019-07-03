using UnityEngine;
using System.Collections;

[AddComponentMenu("Image Effects/Wall Vision Outline Effect")]
[ExecuteInEditMode]
public class WallVisionOutlineEffect : ImageEffectBase {

	public Shader occluderShader;
	public Color color = Color.white;
	public float glowStrength;
	public Texture patternTexture = null;
	public float patternScale = 1.0f;
	public float patternWeight = 1.0f;

	public LayerMask occluderLayer;
	public LayerMask wallVisionLayer;

	public AnimationCurve visibilityCurve = AnimationCurve.EaseInOut(0.0f, 1.0f, 1.0f, 0.0f);
	public Texture2D rampTexture;

	private RenderTexture _silhouetteTexture;
	private RenderTexture _occluderTexture;
	private GameObject _silhouetteCamGameObject;
	private GameObject _occluderCamGameObject;
		
	void CleanUpTextures() {
		if (_silhouetteTexture) {
			RenderTexture.ReleaseTemporary(_silhouetteTexture);
			_silhouetteTexture = null;
		}
		
		if (_occluderTexture) {
			RenderTexture.ReleaseTemporary(_occluderTexture);
			_occluderTexture = null;
		}
	}
	
	void OnPreRender () {
		//		if (!enabled || !gameObject.active)
		if (!enabled || !gameObject.activeInHierarchy)
			return;
		
		CleanUpTextures();
		
		_silhouetteTexture = RenderTexture.GetTemporary((int)GetComponent<Camera>().pixelWidth, (int)GetComponent<Camera>().pixelHeight, 16, RenderTextureFormat.ARGB32);
		_occluderTexture = RenderTexture.GetTemporary((int)GetComponent<Camera>().pixelWidth, (int)GetComponent<Camera>().pixelHeight, 16, RenderTextureFormat.ARGB32);
		
		if (!_silhouetteCamGameObject) {
			_silhouetteCamGameObject = new GameObject("SilhouetteCamera");
			_silhouetteCamGameObject.AddComponent<Camera>();
			_silhouetteCamGameObject.GetComponent<Camera>().enabled = false;
			_silhouetteCamGameObject.hideFlags = HideFlags.HideAndDontSave;
		}
		
		_silhouetteCamGameObject.GetComponent<Camera>().CopyFrom(GetComponent<Camera>());
		_silhouetteCamGameObject.GetComponent<Camera>().backgroundColor = new Color(0,0,0,0);
		_silhouetteCamGameObject.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
		_silhouetteCamGameObject.GetComponent<Camera>().cullingMask = wallVisionLayer;
		_silhouetteCamGameObject.GetComponent<Camera>().targetTexture = _silhouetteTexture;
		_silhouetteCamGameObject.GetComponent<Camera>().RenderWithShader(Shader.Find("Hidden/Camera-DepthNormalTexture"), null);
		_silhouetteCamGameObject.GetComponent<Camera>().depthTextureMode |= DepthTextureMode.DepthNormals;
		
		if (!_occluderCamGameObject) {
			_occluderCamGameObject = new GameObject("OccluderCamera");
			_occluderCamGameObject.AddComponent<Camera>();
			_occluderCamGameObject.GetComponent<Camera>().enabled = false;
			_occluderCamGameObject.hideFlags = HideFlags.HideAndDontSave;
		}
		
		_occluderCamGameObject.GetComponent<Camera>().CopyFrom(GetComponent<Camera>());
		_occluderCamGameObject.GetComponent<Camera>().backgroundColor = new Color(0,0,0,0);
		_occluderCamGameObject.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
		_occluderCamGameObject.GetComponent<Camera>().cullingMask = occluderLayer;
		_occluderCamGameObject.GetComponent<Camera>().targetTexture = _occluderTexture;
		_occluderCamGameObject.GetComponent<Camera>().RenderWithShader(occluderShader, null);
		_occluderCamGameObject.GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
		
		GetComponent<Camera>().depthTextureMode = DepthTextureMode.DepthNormals;
	}

	void OnRenderImage (RenderTexture source, RenderTexture destination) {
		
		material.SetColor("_Color", color);
		material.SetTexture("_Silhouette", _silhouetteTexture);
		material.SetTexture("_Occluder", _occluderTexture);
		material.SetTexture("_PatternTex", patternTexture);
		material.SetFloat("_PatternScale", patternScale);
		material.SetFloat("_PatternWeight", patternWeight);
		material.SetFloat("_GlowStrength", glowStrength);
		material.SetFloat("_Aspect", GetComponent<Camera>().aspect);
		material.SetTexture("_RampTex", rampTexture);
		Graphics.Blit(source, destination, material);
		
		CleanUpTextures();
	}
	
	new void OnDisable () {
		if (_silhouetteCamGameObject) {
			DestroyImmediate(_silhouetteCamGameObject);
		}
		
		if (_occluderCamGameObject) {
			DestroyImmediate(_occluderCamGameObject);
		}
		
		base.OnDisable();
	}
}
