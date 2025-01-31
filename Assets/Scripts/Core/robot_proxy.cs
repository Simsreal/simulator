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

    public void SendFrame(RobotFrame frame)
    {
        if (zmqCommunicator != null)
        {
            zmqCommunicator.SendFrame(frame);
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