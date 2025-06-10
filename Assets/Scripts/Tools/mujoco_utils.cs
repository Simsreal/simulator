using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Mujoco;
using static UnityEditor.PlayerSettings;
using Unity.VisualScripting;
using static Mujoco.MujocoLib;

/// <summary>
/// Mujoco API reference:
/// https://mujoco.readthedocs.io/en/stable/APIreference/APItypes.html#mjtobj
/// </summary>
/// 

public enum mjtObj_
{            // type of MujoCo object
    mjOBJ_UNKNOWN = 0,        // unknown object type
    mjOBJ_BODY,                     // body
    mjOBJ_XBODY,                    // body, used to access regular frame instead of i-frame
    mjOBJ_JOINT,                    // joint
    mjOBJ_DOF,                      // dof
    mjOBJ_GEOM,                     // geom
    mjOBJ_SITE,                     // site
    mjOBJ_CAMERA,                   // camera
    mjOBJ_LIGHT,                    // light
    mjOBJ_FLEX,                     // flex
    mjOBJ_MESH,                     // mesh
    mjOBJ_SKIN,                     // skin
    mjOBJ_HFIELD,                   // heightfield
    mjOBJ_TEXTURE,                  // texture
    mjOBJ_MATERIAL,                 // material for rendering
    mjOBJ_PAIR,                     // geom pair to include
    mjOBJ_EXCLUDE,                  // body pair to exclude
    mjOBJ_EQUALITY,                 // equality constraint
    mjOBJ_TENDON,                   // tendon
    mjOBJ_ACTUATOR,                 // actuator
    mjOBJ_SENSOR,                   // sensor
    mjOBJ_NUMERIC,                  // numeric
    mjOBJ_TEXT,                     // text
    mjOBJ_TUPLE,                    // tuple
    mjOBJ_KEY,                      // keyframe
    mjOBJ_PLUGIN,                   // plugin instance

    mjNOBJECT,                      // number of object types

    // meta elements, do not appear in mjModel
    mjOBJ_FRAME = 100       // frame
};

public class MujocoAPIProxy
{
    [DllImport("mujoco")]
    private static extern IntPtr mj_id2name(IntPtr m, int type, int id);

    public unsafe RobotMapping GetMapping()
    {
        var mjModel = MjScene.Instance.Model;
        RobotMapping mapping = new RobotMapping();
        mapping.joint_name_id_mapping = new Dictionary<string, int>();
        mapping.actuator_name_id_mapping = new Dictionary<string, int>();
        mapping.geom_name_id_mapping = new Dictionary<string, int>();

        for (int i = 0; i < mjModel->nu; i++)
        {
            string name = GetObjectName((IntPtr)mjModel, (int)mjtObj_.mjOBJ_ACTUATOR, i);
            mapping.actuator_name_id_mapping[name] = i;
        }

        for (int i = 0; i < mjModel->ngeom; i++)
        {
            string name = GetObjectName((IntPtr)mjModel, (int)mjtObj_.mjOBJ_GEOM, i);
            mapping.geom_name_id_mapping[name] = i;
        }

        for (int i = 0; i < mjModel->njnt; i++)
        {
            string name = GetObjectName((IntPtr)mjModel, (int)mjtObj_.mjOBJ_JOINT, i);
            mapping.joint_name_id_mapping[name] = i;
        }

        return mapping;
    }

    public unsafe RobotData GetData()
    {
        var mjData_ = MjScene.Instance.Data;
        var mjModel_ = MjScene.Instance.Model;
        RobotData d = new RobotData();

        d.narena = (int)mjData_->narena;
        d.nbuffer = (int)mjData_->nbuffer;
        d.nplugin = mjData_->nplugin;

        d.pstack = (int)mjData_->pstack;
        d.pbase = (int)mjData_->pbase;
        d.parena = (int)mjData_->parena;

        d.maxuse_stack = (int)mjData_->maxuse_stack;
        // d.maxuse_threadstack = (int)mjData_->maxuse_threadstack;
        d.maxuse_arena = (int)mjData_->maxuse_arena;
        d.maxuse_con = mjData_->maxuse_con;
        d.maxuse_efc = mjData_->maxuse_efc;

        d.ncon = mjData_->ncon;
        d.ne = mjData_->ne;
        d.nf = mjData_->nf;
        d.nl = mjData_->nl;
        d.nefc = mjData_->nefc;
        d.nJ = mjData_->nJ;
        // d.nA = mjData_->nA;
        d.nisland = mjData_->nisland;

        int nbody = mjModel_->nbody;
        int njnt = mjModel_->njnt;
        int ngeom = mjModel_->ngeom;
        int nsite = mjModel_->nsite;
        int ncam = mjModel_->ncam;
        int nlight = mjModel_->nlight;
        int nq = mjModel_->nq;
        int na = mjModel_->na;
        int nv = mjModel_->nv;
        int nu = mjModel_->nu;
        int nefc = mjData_->nefc;
        int nJ = mjData_->nJ;
        // int nA = mjData_->nA; // no such variable
        int nisland = mjData_->nisland;
        int ntendon = mjModel_->ntendon;

        d.time = mjData_->time;
        d.energy = InitDoubleArrayField(2, mjData_->energy);

        d.qpos = InitDoubleArrayField(nq, mjData_->qpos);

        d.qvel = InitDoubleArrayField(nv, mjData_->qvel);

        d.act = InitDoubleArrayField(na, mjData_->act);

        d.qacc_warmstart = InitDoubleArrayField(nv, mjData_->qacc_warmstart);

        d.plugin_state = InitDoubleArrayField(mjModel_->npluginstate, mjData_->plugin_state);

        d.qacc = InitDoubleArrayField(nv, mjData_->qacc);

        d.act_dot = InitDoubleArrayField(na, mjData_->act_dot);

        d.xpos = InitDoubleArrayField2(nbody, 3, mjData_->xpos);
        d.xquat = InitDoubleArrayField2(nbody, 4, mjData_->xquat);
        d.xmat = InitDoubleArrayField2(nbody, 9, mjData_->xmat);
        d.xipos = InitDoubleArrayField2(nbody, 3, mjData_->xipos);
        d.ximat = InitDoubleArrayField2(nbody, 9, mjData_->ximat);

        d.xanchor = InitDoubleArrayField2(njnt, 3, mjData_->xanchor);
        d.xaxis = InitDoubleArrayField2(njnt, 3, mjData_->xaxis);

        d.geom_xpos = InitDoubleArrayField2(ngeom, 3, mjData_->geom_xpos);
        d.geom_xmat = InitDoubleArrayField2(ngeom, 9, mjData_->geom_xmat);

        d.site_xpos = InitDoubleArrayField2(nsite, 3, mjData_->site_xpos);
        d.site_xmat = InitDoubleArrayField2(nsite, 9, mjData_->site_xmat);

        d.cam_xpos = InitDoubleArrayField2(ncam, 3, mjData_->cam_xpos);
        d.cam_xmat = InitDoubleArrayField2(ncam, 9, mjData_->cam_xmat);

        d.light_xpos = InitDoubleArrayField2(nlight, 3, mjData_->light_xpos);
        d.light_xdir = InitDoubleArrayField2(nlight, 3, mjData_->light_xdir);

        d.actuator_force = InitDoubleArrayField(nu, mjData_->actuator_force);
        d.qfrc_actuator = InitDoubleArrayField(nv, mjData_->qfrc_actuator);

        d.qfrc_smooth = InitDoubleArrayField(nv, mjData_->qfrc_smooth);
        d.qacc_smooth = InitDoubleArrayField(nv, mjData_->qacc_smooth);

        d.qfrc_constraint = InitDoubleArrayField(nv, mjData_->qfrc_constraint);

        d.qfrc_inverse = InitDoubleArrayField(nv, mjData_->qfrc_inverse);

        d.cacc = InitDoubleArrayField2(nbody, 6, mjData_->cacc);
        d.cfrc_int = InitDoubleArrayField2(nbody, 6, mjData_->cfrc_int);
        d.cfrc_ext = InitDoubleArrayField2(nbody, 6, mjData_->cfrc_ext);

        d.contact = new mjContact_[d.ncon];
        for (int i = 0; i < d.ncon; i++)
        {
            d.contact[i] = new mjContact_();
            d.contact[i].dist = mjData_->contact[i].dist;
            d.contact[i].pos = new double[3];
            for (int j = 0; j < 3; j++)
            {
                d.contact[i].pos[j] = mjData_->contact[i].pos[j];
            }
            d.contact[i].frame = new double[9];
            for (int j = 0; j < 9; j++)
            {
                d.contact[i].frame[j] = mjData_->contact[i].frame[j];
            }

            d.contact[i].dim = mjData_->contact[i].dim;
            d.contact[i].geom = new int[2];
            for (int j = 0; j < 2; j++)
            {
                d.contact[i].geom[j] = mjData_->contact[i].geom[j];
            }

            d.contact[i].flex = new int[2];
            for (int j = 0; j < 2; j++)
            {
                d.contact[i].flex[j] = mjData_->contact[i].flex[j];
            }

            d.contact[i].elem = new int[2];
            for (int j = 0; j < 2; j++)
            {
                d.contact[i].elem[j] = mjData_->contact[i].elem[j];
            }

            d.contact[i].vert = new int[2];
            for (int j = 0; j < 2; j++)
            {
                d.contact[i].vert[j] = mjData_->contact[i].vert[j];
            }

            d.contact[i].efc_address = mjData_->contact[i].efc_address;
        }

        //d.efc_type = InitIntArrayField(nefc, mjData_->efc_type);
        //d.efc_id = InitIntArrayField(nefc, mjData_->efc_id);
        //d.efc_J_rownnz = InitIntArrayField(nefc, mjData_->efc_J_rownnz);
        //d.efc_J_rowadr = InitIntArrayField(nefc, mjData_->efc_J_rowadr);
        //d.efc_J_rowsuper = InitIntArrayField(nefc, mjData_->efc_J_rowsuper);
        //d.efc_J_colind = InitIntArrayField(nJ, mjData_->efc_J_colind);
        //d.efc_JT_rownnz = InitIntArrayField(nv, mjData_->efc_JT_rownnz);
        //d.efc_JT_rowadr = InitIntArrayField(nv, mjData_->efc_JT_rowadr);
        //d.efc_JT_rowsuper = InitIntArrayField(nv, mjData_->efc_JT_rowsuper);
        //d.efc_JT_colind = InitIntArrayField(nJ, mjData_->efc_JT_colind);
        //d.efc_J = InitDoubleArrayField(nJ, mjData_->efc_J);
        //d.efc_JT = InitDoubleArrayField(nJ, mjData_->efc_JT);
        //d.efc_pos = InitDoubleArrayField(nefc, mjData_->efc_pos);
        //d.efc_margin = InitDoubleArrayField(nefc, mjData_->efc_margin);
        //d.efc_frictionloss = InitDoubleArrayField(nefc, mjData_->efc_frictionloss);
        //d.efc_diagApprox = InitDoubleArrayField(nefc, mjData_->efc_diagApprox);
        //d.efc_KBIP = InitDoubleArrayField2(nefc, 4, mjData_->efc_KBIP);
        //d.efc_D = InitDoubleArrayField(nefc, mjData_->efc_D);
        //d.efc_R = InitDoubleArrayField(nefc, mjData_->efc_R);
        //d.tendon_efcadr = InitIntArrayField(ntendon, mjData_->tendon_efcadr);

        //d.dof_island = InitIntArrayField(nv, mjData_->dof_island);
        //d.island_dofnum = InitIntArrayField(nisland, mjData_->island_dofnum);
        //d.island_dofadr = InitIntArrayField(nisland, mjData_->island_dofadr);
        //d.island_dofind = InitIntArrayField(nv, mjData_->island_dofind);
        //d.dof_islandind = InitIntArrayField(nv, mjData_->dof_islandind);
        //d.efc_island = InitIntArrayField(nefc, mjData_->efc_island);
        //d.island_efcnum = InitIntArrayField(nisland, mjData_->island_efcnum);
        //d.island_efcadr = InitIntArrayField(nisland, mjData_->island_efcadr);
        //d.island_efcind = InitIntArrayField(nefc, mjData_->island_efcind);

        //d.efc_AR_rownnz = InitIntArrayField(nefc, mjData_->efc_AR_rownnz);
        //d.efc_AR_rowadr = InitIntArrayField(nefc, mjData_->efc_AR_rowadr);

        d.efc_force = new double[mjData_->nefc];
        for (int i = 0; i < mjData_->nefc; i++)
        {
            d.efc_force[i] = mjData_->efc_force[i];
        }
        return d;
    }

    private unsafe double[,] InitDoubleArrayField2(int row, int ele, void* src)
    {
        var ret = new double[row, ele];
        if (src == null)
        {
            Debug.Log("src == null");
            return ret;
        }
        int byteLength = row * ele * sizeof(double);
        fixed (double* pDst = ret)
        {
            Buffer.MemoryCopy(src, pDst, byteLength, byteLength);
        }
        return ret;
    }

    private unsafe int[,] InitIntArrayField2(int row, int ele, void* src)
    {
        var ret = new int[row, ele];
        if (src == null) return ret;
        int byteLength = row * ele * sizeof(int);
        fixed (int* pDst = ret)
        {
            Buffer.MemoryCopy(src, pDst, byteLength, byteLength);
        }
        return ret;
    }

    private unsafe double[] InitDoubleArrayField(int ele, void* src)
    {
        var ret = new double[ele];
        if (src == null)
        {
            Debug.Log("src == null");
            return ret;
        }
        int byteLength = ele * sizeof(double);
        fixed (double* pDst = ret)
        {
            Buffer.MemoryCopy(src, pDst, byteLength, byteLength);
        }
        return ret;
    }

    private unsafe int[] InitIntArrayField(int ele, void* src)
    {
        var ret = new int[ele];
        if (src == null) return ret;
        int byteLength = ele * sizeof(int);
        fixed (int* pDst = ret)
        {
            Buffer.MemoryCopy(src, pDst, byteLength, byteLength);
        }
        return ret;
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
