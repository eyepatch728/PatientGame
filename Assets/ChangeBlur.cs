// Example script to change the BlurAmount of a material with the SpriteBlur Shader

using UnityEngine;

public class ChangeBlur : MonoBehaviour
{
    public Material spriteBlurMaterial;  // Reference to the material
    public float blurAmount = 0.5f;      // The desired blur amount

    void Start()
    {
        // Ensure the material is assigned
        if (spriteBlurMaterial != null)
        {
            // Set the blur amount using the property name ("BlurAmount") from the shader
            spriteBlurMaterial.SetFloat("_BlurAmount", blurAmount);
        }
    }

    // Method to change the blur amount at runtime
    public void UpdateBlurAmount(float newBlurAmount)
    {
        if (spriteBlurMaterial != null)
        {
            // Update the blur amount in the material
            spriteBlurMaterial.SetFloat("_BlurAmount", newBlurAmount);
        }
    }



    // Method to change the blur amount at runtime
   
}
