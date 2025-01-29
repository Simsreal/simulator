using UnityEngine;

public class CameraCapture : MonoBehaviour
{
    public Camera cameraToCapture;
    public RenderTexture renderTexture;
    public string cameraName = "egocentric"; // Expose cameraName as a public parameter

    // Add public fields for resolution
    public int captureWidth = 640;  // Default width
    public int captureHeight = 480; // Default height

    void Start()
    {
        if (cameraToCapture == null)
        {
            // Use the cameraName parameter to find the camera
            cameraToCapture = GameObject.Find(cameraName)?.GetComponent<Camera>();
            if (cameraToCapture == null)
            {
                Debug.LogError($"Camera named '{cameraName}' not found!");
                return;
            }
        }

        if (renderTexture == null)
        {
            // Use the specified captureWidth and captureHeight
            renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
        }

        cameraToCapture.targetTexture = renderTexture;
    }

    public Texture2D CaptureView()
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        cameraToCapture.Render();

        Texture2D image = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        image.Apply();

        RenderTexture.active = currentRT;
        return image;
    }
}