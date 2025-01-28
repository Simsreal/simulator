using UnityEngine;
using Mujoco;

public class MujocoControl : MonoBehaviour
{
    private RobotProxy robotProxy;

    void Start()
    {
        Debug.Log("MujocoControl UP.");

        robotProxy = gameObject.GetComponent<RobotProxy>();
        if (robotProxy == null)
        {
            robotProxy = gameObject.AddComponent<RobotProxy>();
        }
    }

    public unsafe void Update()
    {
        var data = MjScene.Instance.Data;
        // Debug.Log(data->qpos[0]);
    }

}
