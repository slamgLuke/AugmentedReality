using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI; // required for UI Text

[RequireComponent(typeof(ARCameraManager))]
public class basicBlend : MonoBehaviour
{
    [SerializeField] ARCameraManager cameraManager;

    Texture2D m_Texture;


    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (cameraManager == null || !cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            return;

        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),
            outputFormat = TextureFormat.RGBA32,
            transformation = XRCpuImage.Transformation.MirrorY
        };

        int size = image.GetConvertedDataSize(conversionParams);

        // Use NativeArray<byte> safely without unsafe code
        using (var buffer = new NativeArray<byte>(size, Allocator.Temp))
        {
            image.Convert(conversionParams, buffer);

            if (m_Texture == null ||
                m_Texture.width != conversionParams.outputDimensions.x ||
                m_Texture.height != conversionParams.outputDimensions.y)
            {
                m_Texture = new Texture2D(
                    conversionParams.outputDimensions.x,
                    conversionParams.outputDimensions.y,
                    conversionParams.outputFormat,
                    false);
            }

            m_Texture.LoadRawTextureData(buffer);
            m_Texture.Apply();
        }

        image.Dispose();

        Color[] pixels = m_Texture.GetPixels();

        int width = m_Texture.width;
        int height = m_Texture.height;

        float average_brightness = 0;

        int samples = height * width;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                Color pixel = pixels[index];
                average_brightness += MathF.Sqrt(pixel.r * pixel.r + pixel.g * pixel.g + pixel.b * pixel.b);


            }
        }

        average_brightness /= samples;

        RenderSettings.ambientIntensity = average_brightness;
    }
}
