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
    public List<double> efc_force;
    public Dictionary<string, RobotJointData> joint_data;
    public GeomMapping geom_mapping;
    public RobotJointMapping joint_mapping;
    public List<RobotContact> contact_list;
    public ActuatorMapping actuator_mapping;
}

public class ActuatorMapping
{
    public Dictionary<string, int> actuator_name_id_mapping;
}

public class RobotContact
{
    public List<double> H;
    public int dim;
    public double distance;
    public int efc_address;
    public List<int> elem;
    public int exclude;
    public List<int> flex;
    public List<double> frame;
    public List<double> friction;
    public List<int> geom;
    public int geom1;
    public int geom2;
    public double includemargin;
    public double mu;
    public List<double> pos;

    public List<int> vert;

}

public class GeomMapping
{
    public Dictionary<string, int> geom_name_id_mapping;
}

public class RobotJointMapping
{
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

