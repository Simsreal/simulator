using System;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;

[Serializable]
public class RobotState
{
    public byte[] egocentric_view;
    public string robot_joint_data;
}

[Serializable]
public class RobotGeomMapping
{
    public List<string> names;
    public List<double> xpos;
    public List<double> xmat;
}

[Serializable]
public class RobotJointData
{
    public List<string> names;
    public List<double> qpos;
    public List<double> qvel;
    public List<double> effort;
    public List<int> qpos_adr;
    public List<int> qvel_adr;
    public List<int> effort_adr;
    public List<double> xpos;
    public List<double> axis;
    public List<double> offset;
    public List<string> parent_geoms;
    public List<string> child_geoms;
}

