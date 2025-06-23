using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;

[RequireComponent(typeof(ARCameraBackground))]
public class GradientDomainBlending : MonoBehaviour
{
    [Header("References")]
    public Camera virtualCamera;
    public RenderTexture unityContentTexture;
    public Material blendMaterial;

    [Header("Settings")]
    [Range(0, 1)] public float blendStrength = 0.5f;
    public bool clearEveryFrame = true;

    private ARCameraBackground arCameraBackground;
    private Texture2D cameraTexture;
    private bool frameReset;

    void Start()
    {
        arCameraBackground = GetComponent<ARCameraBackground>();
        InitializeTextures();
        StartCoroutine(FrameManagement());
    }

    void InitializeTextures()
    {
        // Create RenderTexture if not assigned
        if (unityContentTexture == null)
        {
            unityContentTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Bilinear,
                antiAliasing = 2
            };
        }

        // Configure virtual camera
        if (virtualCamera != null)
        {
            virtualCamera.targetTexture = unityContentTexture;
            virtualCamera.clearFlags = CameraClearFlags.SolidColor;
            virtualCamera.backgroundColor = Color.clear;
        }

        // Initialize blend material
        if (blendMaterial == null)
        {
            blendMaterial = new Material(Shader.Find("Custom/GradientDomainBlend"));
        }
    }

    IEnumerator FrameManagement()
    {
        while (true)
        {
            // Reset frame at end of frame
            frameReset = true;
            yield return new WaitForEndOfFrame();
            frameReset = false;

            if (clearEveryFrame)
            {
                ClearRenderTexture(unityContentTexture);
            }
        }
    }

    void ClearRenderTexture(RenderTexture rt)
    {
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = null;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (blendMaterial == null || unityContentTexture == null)
        {
            Graphics.Blit(src, dest);
            return;
        }

        // Update material properties
        blendMaterial.SetFloat("_BlendStrength", blendStrength);
        blendMaterial.SetFloat("_FrameReset", frameReset ? 1 : 0);
        blendMaterial.SetTexture("_MainTex", src);
        blendMaterial.SetTexture("_UnityTex", unityContentTexture);

        // Perform blending
        Graphics.Blit(src, dest, blendMaterial);

    }


    void OnDestroy()
    {
        if (unityContentTexture != null && unityContentTexture.IsCreated())
        {
            unityContentTexture.Release();
        }
    }
}