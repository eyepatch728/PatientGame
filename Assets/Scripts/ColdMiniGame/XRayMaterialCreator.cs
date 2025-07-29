using UnityEngine;

public class XRayMaterialCreator : MonoBehaviour
{
    [Header("X-Ray Effect Settings")]
    public Color xRayColor = new Color(0f, 0.8f, 1f, 1f);
    public Color outlineColor = Color.white;
    public float outlineWidth = 0.02f;
    public float intensity = 1.0f;
    public float noiseIntensity = 0.05f;
    public float scanLineSpeed = 2.0f;
    public int scanLineCount = 30;
    public float scanLineIntensity = 0.1f;

    [Header("References")]
    public Texture2D noiseTexture;

    private Material xRayMaterial;

    void Awake()
    {
        // Create material
        xRayMaterial = new Material(Shader.Find("XRay"));

        // Set material properties
        xRayMaterial.SetColor("_XRayColor", xRayColor);
        xRayMaterial.SetColor("_OutlineColor", outlineColor);
        xRayMaterial.SetFloat("_OutlineWidth", outlineWidth);
        xRayMaterial.SetFloat("_Intensity", intensity);

        if (noiseTexture != null)
        {
            xRayMaterial.SetTexture("_NoiseTexture", noiseTexture);
        }
        else
        {
            // Create a simple noise texture if none is provided
            noiseTexture = CreateNoiseTexture();
            xRayMaterial.SetTexture("_NoiseTexture", noiseTexture);
        }

        xRayMaterial.SetFloat("_NoiseIntensity", noiseIntensity);
        xRayMaterial.SetFloat("_ScanLineSpeed", scanLineSpeed);
        xRayMaterial.SetFloat("_ScanLineCount", scanLineCount);
        xRayMaterial.SetFloat("_ScanLineIntensity", scanLineIntensity);
    }

    public Material GetXRayMaterial()
    {
        return xRayMaterial;
    }

    private Texture2D CreateNoiseTexture()
    {
        int resolution = 256;
        Texture2D texture = new Texture2D(resolution, resolution);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float noise = Mathf.PerlinNoise(x / (float)resolution * 5f, y / (float)resolution * 5f);
                texture.SetPixel(x, y, new Color(noise, noise, noise, 1f));
            }
        }

        texture.Apply();
        return texture;
    }
}