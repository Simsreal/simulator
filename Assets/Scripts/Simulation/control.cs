using System.Collections.Generic;
using System;                       // IntPtr is in here
using System.Runtime.InteropServices;
using UnityEngine;
using Mujoco;

public class MujocoControl : MonoBehaviour
{
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


    private unsafe RobotJointData getRobotJointData()
    {
        var mjData = MjScene.Instance.Data;
        var mjModel = MjScene.Instance.Model;
        RobotJointData jointStates = new RobotJointData();
        List<double> qpos = new List<double>();

        for (int i = 0; i < mjModel->njnt; i++)
        {
            //
        }
        jointStates.qpos = qpos;
        return jointStates;
    }

    public unsafe void Update()
    {
        if (Time.time - lastSendTime >= sendInterval)
        {
            var mjData = MjScene.Instance.Data;
            var mjModel = MjScene.Instance.Model;
            RobotState state = new RobotState();
            state.egocentric_view = egocentricView.CaptureViewBytes();
            state.robot_joint_data = getRobotJointData();
            robotProxy.SendMessage(state);

        }
    }
}