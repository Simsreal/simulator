using System.Collections.Generic;
using UnityEngine;
using Mujoco;

public class MujocoControl : MonoBehaviour
{
    private RobotProxy robotProxy;
    private string cameraName = "egocentric";

    private float lastSendTime = 0f;
    private Dictionary<string, string> xmlData;

    private CameraCapture egocentricView;
    public float sendInterval = 0.1f;
    public int egocentricViewWidth = 640;
    public int egocentricViewHeight = 480;
    public string MJCFXMLPath = Application.dataPath + "/Assets/MJCF/humanoid.xml";

    
    void Start()
    {
        Debug.Log(MJCFXMLPath);
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
        xmlData = XmlReaderUtility.ReadXmlAttributes(MJCFXMLPath);
        Debug.Log("Successfully initialized MujocoControl.");
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
                // no need to destroy the texture here, as it is destroyed by the camera capture component
                byte[] bytes = egocentricView.CaptureViewBytes();
                state.egocentric_view = bytes;
            }
            robotProxy.SendMessage(state);

        }
    }
}