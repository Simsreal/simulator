using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using UnityEngine;
using Mujoco;

public class MujocoControl : MonoBehaviour
{
    private MujocoAPIProxy mujocoAPIProxy = new MujocoAPIProxy();
    private RobotProxy robotProxy;
    private string cameraName = "egocentric";

    private float lastSendTime = 0f;
    private Dictionary<string, string> robot_def;

    private CameraCapture egocentricView;
    public float sendInterval = 0.1f;
    public int egocentricViewWidth = 640;
    public int egocentricViewHeight = 480;
    protected string MJCFXMLPath;
    public string MjcfXmlRelativePath = "/MJCF/humanoid.xml";
    
    void Start()
    {
        MJCFXMLPath = Application.dataPath + MjcfXmlRelativePath;
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
        robot_def = XmlReaderUtility.ReadXmlAttributes(MJCFXMLPath);
        Debug.Log("Successfully initialized MujocoControl.");
    }

    public unsafe void Update()
    {
        if (Time.time - lastSendTime >= sendInterval)
        {
            RobotFrame frame = new RobotFrame();
            RobotData data = mujocoAPIProxy.GetData();

            frame.egocentric_view = egocentricView.CaptureViewBytes();
            frame.robot_state = JsonConvert.SerializeObject(data);
            frame.robot_mapping = JsonConvert.SerializeObject(mujocoAPIProxy.GetMapping());
            robotProxy.SendFrame(frame);

        }
    }
}