using UnityEngine;

[ExecuteInEditMode] // Allows the script to run in the editor
[RequireComponent(typeof(Camera))] // Ensures a camera is attached
public class Raymarcher : MonoBehaviour
{
    // Assign the material you create from the shader in the Inspector.
    public Material raymarchMaterial;

    // This method is called by Unity after the camera is done rendering.
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (raymarchMaterial == null)
        {
            // If no material is assigned, just pass the image through without effect.
            Graphics.Blit(source, destination);
            return;
        }

        // Use Graphics.Blit to copy the source texture to the destination,
        // applying the raymarchMaterial in the process.
        Graphics.Blit(source, destination, raymarchMaterial);
    }
}