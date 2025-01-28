using UnityEngine;

public class RobotProxy : MonoBehaviour
{
    private ZmqCommunicator zmqCommunicator;

    void Start()
    {
        zmqCommunicator = new ZmqCommunicator();
        TestConnection();
    }

    void TestConnection()
    {
        // Test multiple messages
        zmqCommunicator.SendMessage("Hello from Unity!");
        zmqCommunicator.SendMessage("Request robot position");
        zmqCommunicator.SendMessage("Move robot to (1,0,0)");
    }

    // Optional: Test periodic communication
    void Update()
    {
        if (Time.frameCount % 100 == 0)  // Every 100 frames
        {
            zmqCommunicator.SendMessage($"Periodic update at {Time.time}s");
        }
    }

    void OnDestroy()
    {
        zmqCommunicator?.Dispose();
    }
}