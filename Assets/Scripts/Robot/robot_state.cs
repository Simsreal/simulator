using System;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;

/// <summary>
/// serialized robot state.
/// </summary>
[Serializable]
public class RobotState
{
    public byte[] egocentric_view;
    public string robot_joint_data;
    public string robot_geom_mapping;
}

[Serializable]
public class RobotGeomIdNameMapping
{
    public Dictionary<int, string> geom_id_name_mapping;
    public Dictionary<string, int> geom_name_id_mapping;
}

[Serializable]
public class RobotJointData
{
    public double qpos;
    public double qvel;
    public double effort;
    public int qpos_adr;
    public int qvel_adr;
    public int effort_adr;
    public double xpos;
    public double axis;
    public double offset;
    public string parent_geoms;
    public string child_geoms;
}

