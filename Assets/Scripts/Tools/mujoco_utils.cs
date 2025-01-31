using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Mujoco;

public class MujocoUtils
{
    [DllImport("mujoco")]
    private static extern IntPtr mj_id2name(IntPtr m, int type, int id);


    public unsafe string getRobotJointData()
    {
        var mjData = MjScene.Instance.Data;
        var mjModel = MjScene.Instance.Model;
        Dictionary<string, RobotJointData> jointStates = new Dictionary<string, RobotJointData>();
        for (int i=0; i < mjModel->njnt; i++)
        {
            int jnt_type = mjModel->jnt_type[i];
            string name = GetObjectName((IntPtr)mjModel, jnt_type, i);
            RobotJointData jointData = new RobotJointData();
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning($"No name found for joint ID {i}");
                continue;
            }
            Debug.Log($"Joint {i} name: {name}");
            jointStates[name] = jointData;
        }

        return JsonConvert.SerializeObject(jointStates);
    }

    public unsafe RobotGeomMapping getRobotGeomMapping() {
        RobotGeomMapping geomMapping = new RobotGeomMapping();
        return geomMapping;
    }


    public string GetObjectName(IntPtr mjModel, int type, int id)
    {
        IntPtr namePtr = mj_id2name(mjModel, type, id);
        if (namePtr != IntPtr.Zero)
        {
            return Marshal.PtrToStringAnsi(namePtr);
        }
        return null;
    }
}