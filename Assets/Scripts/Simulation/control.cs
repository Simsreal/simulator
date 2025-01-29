using UnityEngine;
using Mujoco;

public class MujocoControl : MonoBehaviour
{
    private RobotProxy robotProxy;
    private string cameraName = "egocentric";
    private float sendInterval = 0.1f;
    private float lastSendTime = 0f;
    
    private CameraCapture cameraCapture;

    
    void Start()
    {
        Debug.Log("MujocoControl UP.");

        robotProxy = gameObject.GetComponent<RobotProxy>();
        if (robotProxy == null)
        {
            robotProxy = gameObject.AddComponent<RobotProxy>();
        }

        cameraCapture = gameObject.GetComponent<CameraCapture>();
        if (cameraCapture == null)
        {
            cameraCapture = gameObject.AddComponent<CameraCapture>();
        }
        else
        {
            cameraCapture.cameraName = cameraName;
        }
    }


    public unsafe void Update()
    {
        if (Time.time - lastSendTime >= sendInterval)
        {
            var data = MjScene.Instance.Data;
            string stateMessage = $"qpos: {data->qpos[0]}";
            
            if (cameraCapture != null)
            {
                Texture2D capturedImage = cameraCapture.CaptureView();
                int numPixelsToDebug = 5; // Number of pixels to debug
                for (int i = 0; i < numPixelsToDebug; i++)
                {
                    // Assuming you want to debug the first row of pixels
                    Color pixelColor = capturedImage.GetPixel(i, 0);
                    Debug.Log($"Pixel {i}: {pixelColor}");
                }
            }

        }
    }
}