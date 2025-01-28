using UnityEngine;
using Mujoco;

public class MujocoControl : MonoBehaviour
{
    private RobotProxy robotProxy;
    private float sendInterval = 0.1f;
    private float lastSendTime = 0f;

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
        if (Time.time - lastSendTime >= sendInterval)
        {
            var data = MjScene.Instance.Data;
            string stateMessage = $"qpos: {data->qpos[0]}";
            
            // Use the renamed method
            robotProxy.SendZmqMessage(stateMessage);
            
            lastSendTime = Time.time;
        }
    }
}