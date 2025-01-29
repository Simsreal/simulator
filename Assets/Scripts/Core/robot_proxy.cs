using UnityEngine;

public class RobotProxy : MonoBehaviour
{
    private ZmqCommunicator zmqCommunicator;

    void Start()
    {
        zmqCommunicator = new ZmqCommunicator();
    }

    void Update()
    {
        
    }

    public void SendMessage(RobotState state)
    {
        if (zmqCommunicator != null)
        {
            zmqCommunicator.SendMessage(state);
        }
        else
        {
            Debug.LogError("ZmqCommunicator not initialized!");
        }
    }


    void OnDestroy()
    {
        zmqCommunicator?.Dispose();
    }
}