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
/// 

public enum mjtObj_ {            // type of MujoCo object
  mjOBJ_UNKNOWN       = 0,        // unknown object type
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
  mjOBJ_FRAME         = 100       // frame
};

public class MujocoAPIProxy
{
    [DllImport("mujoco")]
    private static extern IntPtr mj_id2name(IntPtr m, int type, int id);

    public unsafe RobotMapping GetMapping() {
        var mjModel = MjScene.Instance.Model;
        RobotMapping mapping = new RobotMapping();
        mapping.joint_name_id_mapping = new Dictionary<string, int>();
        mapping.actuator_name_id_mapping = new Dictionary<string, int>();
        mapping.geom_name_id_mapping = new Dictionary<string, int>();

        for (int i=0; i < mjModel->nu; i++) {
            string name = GetObjectName((IntPtr)mjModel, (int)mjtObj_.mjOBJ_ACTUATOR, i);
            mapping.actuator_name_id_mapping[name] = i;
        }

        for (int i=0; i < mjModel->ngeom; i++) {
            string name = GetObjectName((IntPtr)mjModel, (int)mjtObj_.mjOBJ_GEOM, i);
            mapping.geom_name_id_mapping[name] = i;
        }

        for (int i=0; i < mjModel->njnt; i++) {
            string name = GetObjectName((IntPtr)mjModel, (int)mjtObj_.mjOBJ_JOINT, i);
            mapping.joint_name_id_mapping[name] = i;
        }

        return mapping;
    }

    public unsafe RobotData GetData() {
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

        d.time = mjData_->time;
        d.energy = new double[2];
        for (int i=0; i < 2; i++) {
            d.energy[i] = mjData_->energy[i];
        }
        
        d.qpos = new double[mjModel_->nq];
        for (int i=0; i < mjModel_->nq; i++) {
            d.qpos[i] = mjData_->qpos[i];
        }

        d.qvel = new double[mjModel_->nv];
        for (int i=0; i < mjModel_->nv; i++) {
            d.qvel[i] = mjData_->qvel[i];
        }

        d.act = new double[mjModel_->na];
        for (int i=0; i < mjModel_->na; i++) {
            d.act[i] = mjData_->act[i];
        }

        d.qacc_warmstart = new double[mjModel_->nv];
        for (int i=0; i < mjModel_->nv; i++) {
            d.qacc_warmstart[i] = mjData_->qacc_warmstart[i];
        }

        d.plugin_state = new double[mjModel_->npluginstate];
        for (int i=0; i < mjModel_->npluginstate; i++) {
            d.plugin_state[i] = mjData_->plugin_state[i];
        }

        d.qacc = new double[mjModel_->nv];
        for (int i=0; i < mjModel_->nv; i++) {
            d.qacc[i] = mjData_->qacc[i];
        }

        d.act_dot = new double[mjModel_->na];
        for (int i=0; i < mjModel_->na; i++) {
            d.act_dot[i] = mjData_->act_dot[i];
        }

        d.xpos = new double[mjModel_->nbody, 3];
        for (int i=0; i < mjModel_->nbody; i++) {
            for (int j=0; j < 3; j++) {
                d.xpos[i, j] = mjData_->xpos[i * 3 + j];
            }
        }

        d.xquat = new double[mjModel_->nbody, 4];
        for (int i=0; i < mjModel_->nbody; i++) {
            for (int j=0; j < 4; j++) {
                d.xquat[i, j] = mjData_->xquat[i * 4 + j];
            }
        }

        d.xmat = new double[mjModel_->nbody, 9];
        for (int i=0; i < mjModel_->nbody; i++) {
            for (int j=0; j < 9; j++) {
                d.xmat[i, j] = mjData_->xmat[i * 9 + j];
            }
        }

        d.ximat = new double[mjModel_->nbody, 9];
        for (int i=0; i < mjModel_->nbody; i++) {
            for (int j=0; j < 9; j++) {
                d.ximat[i, j] = mjData_->ximat[i * 9 + j];
            }
        }

        d.xanchor = new double[mjModel_->njnt, 3];
        for (int i=0; i < mjModel_->njnt; i++) {
            for (int j=0; j < 3; j++) {
                d.xanchor[i, j] = mjData_->xanchor[i * 3 + j];
            }
        }

        d.xaxis = new double[mjModel_->njnt, 3];
        for (int i=0; i < mjModel_->njnt; i++) {
            for (int j=0; j < 3; j++) {
                d.xaxis[i, j] = mjData_->xaxis[i * 3 + j];
            }
        }

        d.geom_xpos = new double[mjModel_->ngeom, 3];
        for (int i=0; i < mjModel_->ngeom; i++) {
            for (int j=0; j < 3; j++) {
                d.geom_xpos[i, j] = mjData_->geom_xpos[i * 3 + j];
            }
        }

        d.geom_xmat = new double[mjModel_->ngeom, 9];
        for (int i=0; i < mjModel_->ngeom; i++) {
            for (int j=0; j < 9; j++) {
                d.geom_xmat[i, j] = mjData_->geom_xmat[i * 9 + j];
            }
        }

        d.site_xpos = new double[mjModel_->nsite, 3];
        for (int i=0; i < mjModel_->nsite; i++) {
            for (int j=0; j < 3; j++) {
                d.site_xpos[i, j] = mjData_->site_xpos[i * 3 + j];
            }
        }

        d.site_xmat = new double[mjModel_->nsite, 9];
        for (int i=0; i < mjModel_->nsite; i++) {
            for (int j=0; j < 9; j++) {
                d.site_xmat[i, j] = mjData_->site_xmat[i * 9 + j];
            }
        }

        d.cam_xpos = new double[mjModel_->ncam, 3];
        for (int i=0; i < mjModel_->ncam; i++) {
            for (int j=0; j < 3; j++) {
                d.cam_xpos[i, j] = mjData_->cam_xpos[i * 3 + j];
            }
        }

        d.cam_xmat = new double[mjModel_->ncam, 9];
        for (int i=0; i < mjModel_->ncam; i++) {
            for (int j=0; j < 9; j++) {
                d.cam_xmat[i, j] = mjData_->cam_xmat[i * 9 + j];
            }
        }

        d.light_xpos = new double[mjModel_->nlight, 3];
        for (int i=0; i < mjModel_->nlight; i++) {
            for (int j=0; j < 3; j++) {
                d.light_xpos[i, j] = mjData_->light_xpos[i * 3 + j];
            }
        }

        d.light_xdir = new double[mjModel_->nlight, 3];
        for (int i=0; i < mjModel_->nlight; i++) {
            for (int j=0; j < 3; j++) {
                d.light_xdir[i, j] = mjData_->light_xdir[i * 3 + j];
            }
        }

        d.contact = new mjContact_[d.ncon];
        for (int i=0; i < d.ncon; i++) {
            d.contact[i] = new mjContact_();
            d.contact[i].dist = mjData_->contact[i].dist;
            d.contact[i].pos = new double[3];
            for (int j=0; j < 3; j++) {
                d.contact[i].pos[j] = mjData_->contact[i].pos[j];
            }
            d.contact[i].frame = new double[9];
            for (int j=0; j < 9; j++) {
                d.contact[i].frame[j] = mjData_->contact[i].frame[j];
            }

            d.contact[i].dim = mjData_->contact[i].dim;
            d.contact[i].geom = new int[2];
            for (int j=0; j < 2; j++) {
                d.contact[i].geom[j] = mjData_->contact[i].geom[j];
            }

            d.contact[i].flex = new int[2];
            for (int j=0; j < 2; j++) {
                d.contact[i].flex[j] = mjData_->contact[i].flex[j];
            }

            d.contact[i].elem = new int[2];
            for (int j=0; j < 2; j++) {
                d.contact[i].elem[j] = mjData_->contact[i].elem[j];
            }

            d.contact[i].vert = new int[2];
            for (int j=0; j < 2; j++) {
                d.contact[i].vert[j] = mjData_->contact[i].vert[j];
            }

            d.contact[i].efc_address = mjData_->contact[i].efc_address;

        }

        d.efc_force = new double[mjData_->nefc];
        for (int i=0; i < mjData_->nefc; i++) {
            d.efc_force[i] = mjData_->efc_force[i];
        }
        return d;
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
