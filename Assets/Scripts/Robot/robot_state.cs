using System;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;

/// <summary>
/// serialized robot state.
/// </summary>
[Serializable]
public class RobotFrame
{
    public byte[] egocentric_view;
    public string robot_state;
}

[Serializable]
public class RobotState
{
    public List<double> qpos;
    public List<double> qvel;
    public Dictionary<string, RobotJointData> robot_joint_data;
    public RobotGeomMapping robot_geom_mapping;
    public RobotJointMapping robot_joint_mapping;
}

public class RobotGeomMapping
{
    // public Dictionary<int, string> geom_id_name_mapping;
    public Dictionary<string, int> geom_name_id_mapping;
}

public class RobotJointMapping
{
    // public Dictionary<int, string> joint_id_name_mapping;
    public Dictionary<string, int> joint_name_id_mapping;
}


public class RobotJointData
{
    public int id;
    public string name;
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

