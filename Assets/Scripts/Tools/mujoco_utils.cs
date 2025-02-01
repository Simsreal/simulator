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


    public unsafe List<double> getQpos()
    {
        var mjModel = MjScene.Instance.Model;
        var mjData = MjScene.Instance.Data;
        List<double> qpos = new List<double>();
        for (int i=0; i < mjModel->nq; i++)
        {
            qpos.Add(mjData->qpos[i]);
        }
        return qpos;
    }

    public unsafe List<double> getQvel()
    {
        var mjModel = MjScene.Instance.Model;
        var mjData = MjScene.Instance.Data;
        List<double> qvel = new List<double>();
        for (int i=0; i < mjModel->nv; i++)
        {
            qvel.Add(mjData->qvel[i]);
        }
        return qvel;
    }

    public unsafe List<double> getEfcForce() {
        var mjData = MjScene.Instance.Data;
        List<double> efc_force = new List<double>();
        for (int i=0; i < mjData->nefc; i++) {
            efc_force.Add(mjData->efc_force[i]);
        }
        return efc_force;
    }

    // public unsafe List<string> getBodyGeoms() {
        
    // }

    public unsafe Dictionary<string, RobotJointData> getJointData()
    {
        var mjData = MjScene.Instance.Data;
        var mjModel = MjScene.Instance.Model;
        Dictionary<string, RobotJointData> jointStates = new Dictionary<string, RobotJointData>();
        for (int i=0; i < mjModel->njnt; i++)
        {
            string name = GetObjectName((IntPtr)mjModel, JointType, i);
            RobotJointData jointData = new RobotJointData();
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning($"No name found for joint ID {i}");
                continue;
            }
            jointStates[name] = jointData;
        }

        return jointStates;
    }

    public unsafe RobotGeomMapping getGeomMapping() {
        RobotGeomMapping geomIdNameMapping = new RobotGeomMapping();
        Dictionary<string, int> nameIdMapping = new Dictionary<string, int>();
        var mjModel = MjScene.Instance.Model;

        for (int i=0; i < mjModel->ngeom; i++) {
            string name = GetObjectName((IntPtr)mjModel, GeomType, i);
            nameIdMapping[name] = i;
        }
        // geomIdNameMapping.geom_id_name_mapping = idNameMapping;
        geomIdNameMapping.geom_name_id_mapping = nameIdMapping;
        return geomIdNameMapping;
    }

    // public unsafe string
    public unsafe RobotJointMapping getJointMapping() {
        RobotJointMapping jointIdNameMapping = new RobotJointMapping();
        Dictionary<string, int> nameIdMapping = new Dictionary<string, int>();

        var mjModel = MjScene.Instance.Model;
        for (int i=0; i < mjModel->njnt; i++) {
            string name = GetObjectName((IntPtr)mjModel, JointType, i);
            nameIdMapping[name] = i;
        }
        jointIdNameMapping.joint_name_id_mapping = nameIdMapping;
        jointIdNameMapping.joint_name_id_mapping = nameIdMapping;
        return jointIdNameMapping;
    }

    public unsafe RobotContactList getContact() {
        const int coneHessianDim = 36;
        const int elemDim = 2;
        const int flexDim = 2;
        const int frameDim = 9;
        const int frictionDim = 5;
        const int geomDim = 2;
        const int posDim = 3;
        const int vertDim = 2;

        var mjData = MjScene.Instance.Data;
        RobotContactList contact_list = new RobotContactList();
        contact_list.contact = new List<RobotContact>();
        for (int i=0; i < mjData->ncon; i++) {
            RobotContact robot_contact = new RobotContact();

            robot_contact.H = new List<double>();
            robot_contact.elem = new List<int>();
            robot_contact.flex = new List<int>();
            robot_contact.frame = new List<double>();
            robot_contact.friction = new List<double>();
            robot_contact.geom = new List<int>();
            robot_contact.pos = new List<double>();
            robot_contact.vert = new List<int>();

            var contact_data = mjData->contact[i];

            for (int j=0; j < coneHessianDim; j++) {
                robot_contact.H.Add(contact_data.H[j]);
            }

            robot_contact.dim = contact_data.dim;
            
            robot_contact.distance = contact_data.dist;

            robot_contact.efc_address = contact_data.efc_address;

            for (int j=0; j < elemDim; j++) {
                robot_contact.elem.Add(contact_data.elem[j]);
            }

            robot_contact.exclude = contact_data.exclude;

            for (int j=0; j < flexDim; j++) {
                robot_contact.flex.Add(contact_data.flex[j]);
            }

            for (int j=0; j < frameDim; j++) {
                robot_contact.frame.Add(contact_data.frame[j]);
            }

            for (int j=0; j < frictionDim; j++) {
                robot_contact.friction.Add(contact_data.friction[j]);
            }

            for (int j=0; j < geomDim; j++) {
                robot_contact.geom.Add(contact_data.geom[j]);
            }

            robot_contact.geom1 = contact_data.geom1;

            robot_contact.geom2 = contact_data.geom2;

            robot_contact.includemargin = contact_data.includemargin;

            robot_contact.mu = contact_data.mu;

            for (int j=0; j < posDim; j++) {
                robot_contact.pos.Add(contact_data.pos[j]);
            }

            for (int j=0; j < vertDim; j++) {
                robot_contact.vert.Add(contact_data.vert[j]);
            }

            contact_list.contact.Add(robot_contact);

            
        }
        return contact_list;
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
