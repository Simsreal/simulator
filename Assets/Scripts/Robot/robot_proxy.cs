using UnityEngine;

public class RobotProxy : MonoBehaviour
{
    private ZmqCommunicator zmqCommunicator;

    void Start()
    {
        zmqCommunicator = new ZmqCommunicator();
        TestConnection();
    }

    // Renamed to avoid confusion with Unity's SendMessage
    public void SendZmqMessage(string message)
    {
        if (zmqCommunicator != null)
        {
            zmqCommunicator.SendMessage(message);
        }
        else
        {
            Debug.LogError("ZmqCommunicator not initialized!");
        }
    }

    void TestConnection()
    {
        SendZmqMessage("Hello from Unity!");
    }

    void OnDestroy()
    {
        zmqCommunicator?.Dispose();
    }
}