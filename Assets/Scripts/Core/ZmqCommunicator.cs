using System;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;

// Example usage of NetMQPoller for async receiving:
public class ZmqCommunicator : IDisposable
{
    private PublisherSocket publisher;
    private SubscriberSocket subscriber;
    private NetMQPoller poller;
    private bool isRunning = false;

    public ZmqCommunicator(
        string pubAddress = "tcp://*:5556",
        string subAddress = "tcp://localhost:5557")
    {
        try
        {
            // 1) Publisher for sending images/text from Unity.
            publisher = new PublisherSocket();
            // Bind so Python subscribers can connect.
            publisher.Bind(pubAddress);

            // 2) Subscriber for receiving messages from Python.
            subscriber = new SubscriberSocket();
            // Connect to Python’s publisher address.
            subscriber.Connect(subAddress);
            // Subscribe to all topics (empty string).
            subscriber.Subscribe("");

            // Use a NetMQPoller to handle incoming subscriber messages asynchronously.
            poller = new NetMQPoller { subscriber };
            subscriber.ReceiveReady += OnSubscriberMessageReceived;
            poller.RunAsync();

            isRunning = true;

            Debug.Log($"ZMQ Communicator Initialized.\n" +
                      $"Publisher bound to {pubAddress}\n" +
                      $"Subscriber connected to {subAddress}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize ZMQ: {e.Message}");
            isRunning = false;
        }
    }

    // Called by NetMQPoller every time the subscriber has a message.
    private void OnSubscriberMessageReceived(object sender, NetMQSocketEventArgs e)
    {
        try
        {
            // The first frame is the topic (if you use them), or the entire message.
            // By default, we subscribed to "", so we’ll receive everything.
            var message = e.Socket.ReceiveFrameString();
            Debug.Log($"Received from Python: {message}");

            // Process or store the message as needed...
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error reading subscriber message: {ex.Message}");
        }
    }

    // Example of sending string messages:
    public void SendMessage(RobotState state)
    {
        if (!isRunning)
        {
            Debug.LogWarning("ZmqCommunicator is not running.");
            return;
        }
        try
        {
            publisher.SendFrame(JsonUtility.ToJson(state));
            // Debug.Log($"Published message: {state.message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in SendMessage: {e.Message}");
        }
    }

    // Example of sending images:
    public void SendImage(Texture2D image)
    {
        if (!isRunning)
        {
            Debug.LogWarning("ZmqCommunicator is not running.");
            return;
        }

        try
        {
            byte[] imageBytes = image.EncodeToPNG();
            publisher.SendFrame(imageBytes);
            Debug.Log("Published image.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in SendImage: {e.Message}");
        }
    }

    public void Dispose()
    {
        if (!isRunning) return;

        try
        {
            poller?.Stop();
            poller?.Dispose();
            
            subscriber?.Close();
            subscriber?.Dispose();

            publisher?.Close();
            publisher?.Dispose();

            Debug.Log("ZMQ Communicator disposed.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error disposing ZMQ Communicator: {e.Message}");
        }

        isRunning = false;
    }
}