using System;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;

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
            var message = e.Socket.ReceiveFrameString();
            Debug.Log($"Received from Python: {message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error reading subscriber message: {ex.Message}");
        }
    }

    // Example of sending string messages:
    public void SendFrame(RobotFrame frame)
    {
        if (!isRunning)
        {
            Debug.LogWarning("ZmqCommunicator is not running.");
            return;
        }
        try
        {
            publisher.SendFrame(JsonUtility.ToJson(frame));
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in SendFrame: {e.Message}");
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