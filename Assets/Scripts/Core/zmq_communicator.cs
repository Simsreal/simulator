using System;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using UnityEngine;
using Mujoco;

public class ZmqCommunicator : IDisposable
{
    private PublisherSocket publisher;
    private SubscriberSocket subscriber;
    private NetMQPoller poller;
    private volatile bool isRunning = false;

    public ZmqCommunicator(
        string pubAddress = "tcp://0.0.0.0:5556",
        string subAddress = "tcp://localhost:5557")
    {
        try
        {
            // A document says that is necessary
            AsyncIO.ForceDotNet.Force();

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
            subscriber.ReceiveReady += OnSubscriberMessageReceived;
            poller = new NetMQPoller { subscriber };
            poller.RunAsync();

            isRunning = true;

            Debug.Log($"ZMQ Communicator Initialized.\n" +
                      $"Publisher bound to {pubAddress}\n" +
                      $"Subscriber connected to {subAddress}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize ZMQ: {e.Message}");
            Dispose();
            isRunning = false;
        }
    }

    // Called by NetMQPoller every time the subscriber has a message.
    private unsafe void OnSubscriberMessageReceived(object sender, NetMQSocketEventArgs e)
    {
        if (!isRunning)
        {
            return;
        }

        try
        {
            var message = e.Socket.ReceiveFrameString();

            // Deserialize the JSON string into the Cmds class
            Cmds cmds = JsonConvert.DeserializeObject<Cmds>(message);

            var mjData = MjScene.Instance.Data;
            var mjModel = MjScene.Instance.Model;

            for (int i=0; i < mjModel->nu; i++) {
                mjData->ctrl[i] = cmds.torques[i];
            }   
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
            if (subscriber != null)
            {
                subscriber.ReceiveReady -= OnSubscriberMessageReceived;
            }

            poller?.Stop();
            poller?.Dispose();
            
            subscriber?.Close();
            subscriber?.Dispose();

            publisher?.Close();
            publisher?.Dispose();

            // Cleanup all things relate to NetMQ
            NetMQConfig.Cleanup();

            Debug.Log("ZMQ Communicator disposed.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error disposing ZMQ Communicator: {e.Message}");
        }

        isRunning = false;
    }
}