using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Mujoco;

public class MujocoUtils
{
    [DllImport("mujoco")]
    private static extern IntPtr mj_id2name(IntPtr m, int type, int id);

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