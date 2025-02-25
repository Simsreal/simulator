using System;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;

/// <summary>
/// serialized robot state.
/// </summary>
/// 

// https://mujoco.readthedocs.io/en/stable/APIreference/APItypes.html#mjdata
[Serializable]
public class RobotData
{
    public int narena;            // size of the arena in bytes (inclusive of the stack)
    public int nbuffer;           // size of main buffer in bytes
    public int nplugin;           // number of plugin instances

    // stack pointer
    public int pstack;            // first available byte in stack
    public int pbase;             // value of pstack when mj_markStack was last called

    // arena pointer
    public int parena;            // first available byte in arena

    // // memory utilization statistics
    public int maxuse_stack;                       // maximum stack allocation in bytes
    // public int[] maxuse_threadstack;    // maximum stack allocation per thread in bytes
    public int maxuse_arena;                       // maximum arena allocation in bytes
    public int maxuse_con;                         // maximum number of contacts
    public int maxuse_efc;                         // maximum number of scalar constraints

    // // solver statistics
    // mjSolverStat solver[mjNISLAND * mjNSOLVER];  // solver statistics per island, per iteration
    // public int solver_nisland;               // number of islands processed by solver
    // public int solver_niter[mjNISLAND];      // number of solver iterations, per island
    // public int solver_nnz[mjNISLAND];        // number of nonzeros in Hessian or efc_AR, per island
    // mjtNum solver_fwdinv[2];             // forward-inverse comparison: qfrc, efc

    // // diagnostics
    // mjWarningStat warning[mjNWARNING];          // warning statistics
    // mjTimerStat timer[mjNTIMER];              // timer statistics

    // // variable sizes
    public int ncon;              // number of detected contacts
    public int ne;                // number of equality constraints
    public int nf;                // number of friction constraints
    public int nl;                // number of limit constraints
    public int nefc;              // number of constraints
    public int nJ;                // number of non-zeros in constraint Jacobian
    public int nA;                // number of non-zeros in constraint inverse inertia matrix
    public int nisland;           // number of detected constraint islands

    // // global properties
    public double time;              // simulation time
    public List<double> energy;         // potential, kinetic energy

    // //-------------------- end of info header

    // // buffers
    // void* buffer;            // main buffer; all pointers point in it            (nbuffer bytes)
    // void* arena;             // arena+stack buffer                               (narena bytes)

    //-------------------- main inputs and outputs of the computation

    // state
    public List<double> qpos;              // position                                         (nq x 1)
    public List<double> qvel;              // velocity                                         (nv x 1)
    public List<double> act;               // actuator activation                              (na x 1)
    public List<double> qacc_warmstart;    // acceleration used for warmstart                  (nv x 1)
    public List<double> plugin_state;      // plugin state                                     (npluginstate x 1)

    // // control
    // mjtNum* ctrl;              // control                                          (nu x 1)
    // mjtNum* qfrc_applied;      // applied generalized force                        (nv x 1)
    // mjtNum* xfrc_applied;      // applied Cartesian force/torque                   (nbody x 6)
    // mjtByte* eq_active;        // enable/disable constraints                       (neq x 1)

    // // mocap data
    // mjtNum* mocap_pos;         // positions of mocap bodies                        (nmocap x 3)
    // mjtNum* mocap_quat;        // orientations of mocap bodies                     (nmocap x 4)

    // // dynamics
    // mjtNum* qacc;              // acceleration                                     (nv x 1)
    // mjtNum* act_dot;           // time-derivative of actuator activation           (na x 1)

    // // user data
    // mjtNum* userdata;          // user data, not touched by engine                 (nuserdata x 1)

    // // sensors
    // mjtNum* sensordata;        // sensor data array                                (nsensordata x 1)

    // // plugins
    // public int* plugin;         // copy of m->plugin, required for deletion         (nplugin x 1)
    // uintptr_t* plugin_data;    // pointer to plugin-managed data structure         (nplugin x 1)

    // //-------------------- POSITION dependent

    // // computed by mj_fwdPosition/mj_kinematics
    // mjtNum* xpos;              // Cartesian position of body frame                 (nbody x 3)
    // mjtNum* xquat;             // Cartesian orientation of body frame              (nbody x 4)
    // mjtNum* xmat;              // Cartesian orientation of body frame              (nbody x 9)
    // mjtNum* xipos;             // Cartesian position of body com                   (nbody x 3)
    // mjtNum* ximat;             // Cartesian orientation of body inertia            (nbody x 9)
    // mjtNum* xanchor;           // Cartesian position of joint anchor               (njnt x 3)
    // mjtNum* xaxis;             // Cartesian joint axis                             (njnt x 3)
    // mjtNum* geom_xpos;         // Cartesian geom position                          (ngeom x 3)
    // mjtNum* geom_xmat;         // Cartesian geom orientation                       (ngeom x 9)
    // mjtNum* site_xpos;         // Cartesian site position                          (nsite x 3)
    // mjtNum* site_xmat;         // Cartesian site orientation                       (nsite x 9)
    // mjtNum* cam_xpos;          // Cartesian camera position                        (ncam x 3)
    // mjtNum* cam_xmat;          // Cartesian camera orientation                     (ncam x 9)
    // mjtNum* light_xpos;        // Cartesian light position                         (nlight x 3)
    // mjtNum* light_xdir;        // Cartesian light direction                        (nlight x 3)

    // // computed by mj_fwdPosition/mj_comPos
    // mjtNum* subtree_com;       // center of mass of each subtree                   (nbody x 3)
    // mjtNum* cdof;              // com-based motion axis of each dof (rot:lin)      (nv x 6)
    // mjtNum* cinert;            // com-based body inertia and mass                  (nbody x 10)

    // // computed by mj_fwdPosition/mj_flex
    // mjtNum* flexvert_xpos;     // Cartesian flex vertex positions                  (nflexvert x 3)
    // mjtNum* flexelem_aabb;     // flex element bounding boxes (center, size)       (nflexelem x 6)
    // public int* flexedge_J_rownnz; // number of non-zeros in Jacobian row              (nflexedge x 1)
    // public int* flexedge_J_rowadr; // row start address in colind array                (nflexedge x 1)
    // public int* flexedge_J_colind; // column indices in sparse Jacobian                (nflexedge x nv)
    // mjtNum* flexedge_J;        // flex edge Jacobian                               (nflexedge x nv)
    // mjtNum* flexedge_length;   // flex edge lengths                                (nflexedge x 1)

    // // computed by mj_fwdPosition/mj_tendon
    // public int* ten_wrapadr;       // start address of tendon's path                   (ntendon x 1)
    // public int* ten_wrapnum;       // number of wrap points in path                    (ntendon x 1)
    // public int* ten_J_rownnz;      // number of non-zeros in Jacobian row              (ntendon x 1)
    // public int* ten_J_rowadr;      // row start address in colind array                (ntendon x 1)
    // public int* ten_J_colind;      // column indices in sparse Jacobian                (ntendon x nv)
    // mjtNum* ten_J;             // tendon Jacobian                                  (ntendon x nv)
    // mjtNum* ten_length;        // tendon lengths                                   (ntendon x 1)
    // public int* wrap_obj;          // geom id; -1: site; -2: pulley                    (nwrap x 2)
    // mjtNum* wrap_xpos;         // Cartesian 3D points in all paths                 (nwrap x 6)

    // // computed by mj_fwdPosition/mj_transmission
    // mjtNum* actuator_length;   // actuator lengths                                 (nu x 1)
    // public int* moment_rownnz;     // number of non-zeros in actuator_moment row       (nu x 1)
    // public int* moment_rowadr;     // row start address in colind array                (nu x 1)
    // public int* moment_colind;     // column indices in sparse Jacobian                (nJmom x 1)
    // mjtNum* actuator_moment;   // actuator moments                                 (nJmom x 1)

    // // computed by mj_fwdPosition/mj_crb
    // mjtNum* crb;               // com-based composite inertia and mass             (nbody x 10)
    // mjtNum* qM;                // total inertia (sparse)                           (nM x 1)

    // // computed by mj_fwdPosition/mj_factorM
    // mjtNum* qLD;               // L'*D*L factorization of M (sparse)               (nM x 1)
    // mjtNum* qLDiagInv;         // 1/diag(D)                                        (nv x 1)

    // // computed by mj_collisionTree
    // mjtNum* bvh_aabb_dyn;     // global bounding box (center, size)               (nbvhdynamic x 6)
    // mjtByte* bvh_active;       // was bounding volume checked for collision        (nbvh x 1)

    // //-------------------- POSITION, VELOCITY dependent

    // // computed by mj_fwdVelocity
    // mjtNum* flexedge_velocity; // flex edge velocities                             (nflexedge x 1)
    // mjtNum* ten_velocity;      // tendon velocities                                (ntendon x 1)
    // mjtNum* actuator_velocity; // actuator velocities                              (nu x 1)

    // // computed by mj_fwdVelocity/mj_comVel
    // mjtNum* cvel;              // com-based velocity (rot:lin)                     (nbody x 6)
    // mjtNum* cdof_dot;          // time-derivative of cdof (rot:lin)                (nv x 6)

    // // computed by mj_fwdVelocity/mj_rne (without acceleration)
    // mjtNum* qfrc_bias;         // C(qpos,qvel)                                     (nv x 1)

    // // computed by mj_fwdVelocity/mj_passive
    // mjtNum* qfrc_spring;       // passive spring force                             (nv x 1)
    // mjtNum* qfrc_damper;       // passive damper force                             (nv x 1)
    // mjtNum* qfrc_gravcomp;     // passive gravity compensation force               (nv x 1)
    // mjtNum* qfrc_fluid;        // passive fluid force                              (nv x 1)
    // mjtNum* qfrc_passive;      // total passive force                              (nv x 1)

    // // computed by mj_sensorVel/mj_subtreeVel if needed
    // mjtNum* subtree_linvel;    // linear velocity of subtree com                   (nbody x 3)
    // mjtNum* subtree_angmom;    // angular momentum about subtree com               (nbody x 3)

    // // computed by mj_Euler or mj_implicit
    // mjtNum* qH;                // L'*D*L factorization of modified M               (nM x 1)
    // mjtNum* qHDiagInv;         // 1/diag(D) of modified M                          (nv x 1)

    // // computed by mj_resetData
    // public int* B_rownnz;          // body-dof: non-zeros in each row                  (nbody x 1)
    // public int* B_rowadr;          // body-dof: address of each row in B_colind        (nbody x 1)
    // public int* B_colind;          // body-dof: column indices of non-zeros            (nB x 1)
    // public int* C_rownnz;          // reduced dof-dof: non-zeros in each row           (nv x 1)
    // public int* C_rowadr;          // reduced dof-dof: address of each row in C_colind (nv x 1)
    // public int* C_colind;          // reduced dof-dof: column indices of non-zeros     (nC x 1)
    // public int* mapM2C;            // index mapping from M to C                        (nC x 1)
    // public int* D_rownnz;          // dof-dof: non-zeros in each row                   (nv x 1)
    // public int* D_rowadr;          // dof-dof: address of each row in D_colind         (nv x 1)
    // public int* D_diag;            // dof-dof: index of diagonal element               (nv x 1)
    // public int* D_colind;          // dof-dof: column indices of non-zeros             (nD x 1)
    // public int* mapM2D;            // index mapping from M to D                        (nD x 1)
    // public int* mapD2M;            // index mapping from D to M                        (nM x 1)

    // // computed by mj_implicit/mj_derivative
    // mjtNum* qDeriv;            // d (passive + actuator - bias) / d qvel           (nD x 1)

    // // computed by mj_implicit/mju_factorLUSparse
    // mjtNum* qLU;               // sparse LU of (qM - dt*qDeriv)                    (nD x 1)

    // //-------------------- POSITION, VELOCITY, CONTROL/ACCELERATION dependent

    // // computed by mj_fwdActuation
    // mjtNum* actuator_force;    // actuator force in actuation space                (nu x 1)
    // mjtNum* qfrc_actuator;     // actuator force                                   (nv x 1)

    // // computed by mj_fwdAcceleration
    // mjtNum* qfrc_smooth;       // net unconstrained force                          (nv x 1)
    // mjtNum* qacc_smooth;       // unconstrained acceleration                       (nv x 1)

    // // computed by mj_fwdConstraint/mj_inverse
    // mjtNum* qfrc_constraint;   // constraint force                                 (nv x 1)

    // // computed by mj_inverse
    // mjtNum* qfrc_inverse;      // net external force; should equal:                (nv x 1)
    //                            // qfrc_applied + J'*xfrc_applied + qfrc_actuator

    // // computed by mj_sensorAcc/mj_rnePostConstraint if needed; rotation:translation format
    // mjtNum* cacc;              // com-based acceleration                           (nbody x 6)
    // mjtNum* cfrc_int;          // com-based interaction force with parent          (nbody x 6)
    // mjtNum* cfrc_ext;          // com-based external force on body                 (nbody x 6)

    // //-------------------- arena-allocated: POSITION dependent

    // // computed by mj_collision
    // mjContact* contact;        // array of all detected contacts                   (ncon x 1)

    // // computed by mj_makeConstraint
    // public int* efc_type;          // constraint type (mjtConstraint)                  (nefc x 1)
    // public int* efc_id;            // id of object of specified type                   (nefc x 1)
    // public int* efc_J_rownnz;      // number of non-zeros in constraint Jacobian row   (nefc x 1)
    // public int* efc_J_rowadr;      // row start address in colind array                (nefc x 1)
    // public int* efc_J_rowsuper;    // number of subsequent rows in supernode           (nefc x 1)
    // public int* efc_J_colind;      // column indices in constraint Jacobian            (nJ x 1)
    // public int* efc_JT_rownnz;     // number of non-zeros in constraint Jacobian row T (nv x 1)
    // public int* efc_JT_rowadr;     // row start address in colind array              T (nv x 1)
    // public int* efc_JT_rowsuper;   // number of subsequent rows in supernode         T (nv x 1)
    // public int* efc_JT_colind;     // column indices in constraint Jacobian          T (nJ x 1)
    // mjtNum* efc_J;             // constraint Jacobian                              (nJ x 1)
    // mjtNum* efc_JT;            // constraint Jacobian transposed                   (nJ x 1)
    // mjtNum* efc_pos;           // constraint position (equality, contact)          (nefc x 1)
    // mjtNum* efc_margin;        // inclusion margin (contact)                       (nefc x 1)
    // mjtNum* efc_frictionloss;  // frictionloss (friction)                          (nefc x 1)
    // mjtNum* efc_diagApprox;    // approximation to diagonal of A                   (nefc x 1)
    // mjtNum* efc_KBIP;          // stiffness, damping, impedance, imp'              (nefc x 4)
    // mjtNum* efc_D;             // constraint mass                                  (nefc x 1)
    // mjtNum* efc_R;             // inverse constraint mass                          (nefc x 1)
    // public int* tendon_efcadr;     // first efc address involving tendon; -1: none     (ntendon x 1)

    // // computed by mj_island
    // public int* dof_island;        // island id of this dof; -1: none                  (nv x 1)
    // public int* island_dofnum;     // number of dofs in island                         (nisland x 1)
    // public int* island_dofadr;     // start address in island_dofind                   (nisland x 1)
    // public int* island_dofind;     // island dof indices; -1: none                     (nv x 1)
    // public int* dof_islandind;     // dof island indices; -1: none                     (nv x 1)
    // public int* efc_island;        // island id of this constraint                     (nefc x 1)
    // public int* island_efcnum;     // number of constraints in island                  (nisland x 1)
    // public int* island_efcadr;     // start address in island_efcind                   (nisland x 1)
    // public int* island_efcind;     // island constraint indices                        (nefc x 1)

    // // computed by mj_projectConstraint (PGS solver)
    // public int* efc_AR_rownnz;     // number of non-zeros in AR                        (nefc x 1)
    // public int* efc_AR_rowadr;     // row start address in colind array                (nefc x 1)
    // public int* efc_AR_colind;     // column indices in sparse AR                      (nA x 1)
    // mjtNum* efc_AR;            // J*inv(M)*J' + R                                  (nA x 1)

    // //-------------------- arena-allocated: POSITION, VELOCITY dependent

    // // computed by mj_fwdVelocity/mj_referenceConstraint
    // mjtNum* efc_vel;           // velocity in constraint space: J*qvel             (nefc x 1)
    // mjtNum* efc_aref;          // reference pseudo-acceleration                    (nefc x 1)

    // //-------------------- arena-allocated: POSITION, VELOCITY, CONTROL/ACCELERATION dependent

    // // computed by mj_fwdConstraint/mj_inverse
    // mjtNum* efc_b;             // linear cost term: J*qacc_smooth - aref           (nefc x 1)
    // mjtNum* efc_force;         // constraint force in constraint space             (nefc x 1)
    // public int* efc_state;         // constraint state (mjtConstraintState)            (nefc x 1)

    // // thread pool pointer
    // uintptr_t threadpool;
}

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

