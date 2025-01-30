// references
// https://mujoco.readthedocs.io/en/stable/APIreference/APItypes.html#mjtobj

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Mujoco;

public class MujocoControl : MonoBehaviour
{
    [DllImport("mujoco")]
    private static extern IntPtr mj_id2name(IntPtr m, int type, int id);

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

    private string GetObjectName(IntPtr mjModel, int type, int id)
    {
        IntPtr namePtr = mj_id2name(mjModel, type, id);
        if (namePtr != IntPtr.Zero)
        {
            return Marshal.PtrToStringAnsi(namePtr);
        }
        return null;
    }

    private unsafe RobotJointData getRobotJointData()
    {
        var mjData = MjScene.Instance.Data;
        var mjModel = MjScene.Instance.Model;
        RobotJointData jointStates = new RobotJointData();
        List<double> qpos = new List<double>();
        List<double> qvel = new List<double>();

        for (int i = 0; i < mjModel->nq; i++)
        {
            qpos.Add(mjData->qpos[i]);
        }

        for (int i = 0; i < mjModel->nv; i++)
        {
            qvel.Add(mjData->qvel[i]);
        }

        for (int i=0; i < mjModel->njnt; i++)
        {
            string name = GetObjectName((IntPtr)mjModel, 2, i);
            Debug.Log(name);
        }

        jointStates.qpos = qpos;
        jointStates.qvel = qvel;
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