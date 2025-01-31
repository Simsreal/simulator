using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Mujoco;

/// <summary>
/// Mujoco API reference:
/// https://mujoco.readthedocs.io/en/stable/APIreference/APItypes.html#mjtobj
/// </summary>
public class MujocoAPIProxy
{
    [DllImport("mujoco")]
    private static extern IntPtr mj_id2name(IntPtr m, int type, int id);
    private static readonly int JointType = 3; // mjtOBJ_JOINT
    private static readonly int GeomType = 5; // mjtOBJ_GEOM


    public unsafe string getRobotJointDataSerialized()
    {
        var mjData = MjScene.Instance.Data;
        var mjModel = MjScene.Instance.Model;
        Dictionary<string, RobotJointData> jointStates = new Dictionary<string, RobotJointData>();
        for (int i=0; i < mjModel->njnt; i++)
        {
            // int jnt_type = mjModel->jnt_type[i];
            string name = GetObjectName((IntPtr)mjModel, JointType, i);
            RobotJointData jointData = new RobotJointData();
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning($"No name found for joint ID {i}");
                continue;
            }
            Debug.Log($"Joint type: {JointType} name: {name}");
            jointStates[name] = jointData;
        }

        return JsonConvert.SerializeObject(jointStates);
    }

    public unsafe string getRobotGeomMappingSerialized() {
        RobotGeomIdNameMapping geomIdNameMapping = new RobotGeomIdNameMapping();
        Dictionary<int, string> idNameMapping = new Dictionary<int, string>();
        Dictionary<string, int> nameIdMapping = new Dictionary<string, int>();
        var mjModel = MjScene.Instance.Model;

        for (int i=0; i < mjModel->ngeom; i++) {
            string name = GetObjectName((IntPtr)mjModel, GeomType, i);
            idNameMapping[i] = name;
            nameIdMapping[name] = i;
        }
        geomIdNameMapping.geom_id_name_mapping = idNameMapping;
        geomIdNameMapping.geom_name_id_mapping = nameIdMapping;
        return JsonConvert.SerializeObject(geomIdNameMapping);
    }

    // public unsafe string
    public unsafe string getRobotJointMappingSerialized() {
        RobotJointIdNameMapping jointIdNameMapping = new RobotJointIdNameMapping();
        Dictionary<int, string> idNameMapping = new Dictionary<int, string>();
        Dictionary<string, int> nameIdMapping = new Dictionary<string, int>();

        var mjModel = MjScene.Instance.Model;
        for (int i=0; i < mjModel->njnt; i++) {
            string name = GetObjectName((IntPtr)mjModel, JointType, i);
            idNameMapping[i] = name;
            nameIdMapping[name] = i;
        }
        jointIdNameMapping.joint_id_name_mapping = idNameMapping;
        jointIdNameMapping.joint_name_id_mapping = nameIdMapping;
        return JsonConvert.SerializeObject(jointIdNameMapping);
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
