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
            RobotFrame frame = new RobotFrame();
            RobotState state = new RobotState();
            
            state.joint_data = mujocoAPIProxy.getJointData();
            state.geom_mapping = mujocoAPIProxy.getGeomMapping();
            state.joint_mapping = mujocoAPIProxy.getJointMapping();
            state.qpos = mujocoAPIProxy.getQpos();
            state.qvel = mujocoAPIProxy.getQvel();
            state.contact_list = mujocoAPIProxy.getContact();
            state.efc_force = mujocoAPIProxy.getEfcForce();

            frame.egocentric_view = egocentricView.CaptureViewBytes();
            frame.robot_state = JsonConvert.SerializeObject(state);
            robotProxy.SendFrame(frame);

        }
    }
}