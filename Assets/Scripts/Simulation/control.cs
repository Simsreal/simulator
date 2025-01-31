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
        robot_def = XmlReaderUtility.ReadXmlAttributes(MJCFXMLPath);
        Debug.Log("Successfully initialized MujocoControl.");
    }

    public unsafe void Update()
    {
        if (Time.time - lastSendTime >= sendInterval)
        {
            RobotState state = new RobotState();
            state.egocentric_view = egocentricView.CaptureViewBytes();
            state.robot_joint_data = mujocoAPIProxy.getRobotJointDataSerialized();
            state.robot_geom_mapping = mujocoAPIProxy.getRobotGeomMappingSerialized();
            state.robot_joint_mapping = mujocoAPIProxy.getRobotJointMappingSerialized();
            robotProxy.SendMessage(state);

        }
    }
}