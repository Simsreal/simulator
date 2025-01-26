using UnityEngine;
using Mujoco;

public class MujocoControl : MonoBehaviour
{
    // private MjScene scene;

    void Start()
    {
        Debug.Log("MujocoControl UP.");
    }

    public unsafe void Update()
    {
        var data = MjScene.Instance.Data;
        Debug.Log(data->qpos[0]);
    }

}
