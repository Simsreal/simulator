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
        // Check for incoming messages
        string message = zmqCommunicator.ReceiveMessage();
        if (message != null)
        {
            // Handle received message
            Debug.Log($"Handling received message: {message}");
        }
    }

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

    void OnDestroy()
    {
        zmqCommunicator?.Dispose();
    }
}