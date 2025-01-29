using UnityEngine;
using Mujoco;

public class MujocoControl : MonoBehaviour
{
    private RobotProxy robotProxy;
    private string cameraName = "egocentric";
    private float sendInterval = 0.1f;
    private float lastSendTime = 0f;
    
    private CameraCapture egocentricView;
    private int egocentricViewWidth = 640;
    private int egocentricViewHeight = 480;

    
    void Start()
    {
        Debug.Log("MujocoControl UP.");

        robotProxy = gameObject.GetComponent<RobotProxy>();
        if (robotProxy == null)
        {
            robotProxy = gameObject.AddComponent<RobotProxy>();
        }

        egocentricView = gameObject.GetComponent<CameraCapture>();
        if (egocentricView == null)
        {
            egocentricView = gameObject.AddComponent<CameraCapture>();
            egocentricView.cameraName = cameraName;
            egocentricView.captureWidth = egocentricViewWidth;
            egocentricView.captureHeight = egocentricViewHeight;
        }
    }


    public unsafe void Update()
    {
        if (Time.time - lastSendTime >= sendInterval)
        {
            var data = MjScene.Instance.Data;
            RobotState state = new RobotState();
            state.message = $"qpos: {data->qpos[0]}";
            
            if (egocentricView != null)
            {
                Texture2D capturedImage = egocentricView.CaptureView();
                byte[] bytes = capturedImage.EncodeToJPG();
                state.egocentric_view = bytes;
            }
            robotProxy.SendMessage(state);

        }
    }
}