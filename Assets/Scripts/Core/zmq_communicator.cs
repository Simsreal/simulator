using System;
using System.Collections;
using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class ZmqCommunicator : IDisposable
{
    private PublisherSocket publisher;
    private SubscriberSocket subscriber;
    private NetMQPoller poller;
    private volatile bool isRunning = false;

    public event Action<Cmd> OnCmdReceived;

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
    private void OnSubscriberMessageReceived(object sender, NetMQSocketEventArgs e)
    {
        if (!isRunning)
        {
            return;
        }

        try
        {
            var message = e.Socket.ReceiveFrameString();

            // Deserialize the JSON string into the Cmds class
            Cmd cmd = JsonConvert.DeserializeObject<Cmd>(message);

            OnCmdReceived.Invoke(cmd);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error reading subscriber message: {ex.Message}");
        }
    }

    public void SendFrame(object frame)
    {
        if (!isRunning)
        {
            Debug.LogWarning("ZmqCommunicator is not running.");
            return;
        }
        try
        {
            string jsonFrame = JsonConvert.SerializeObject(frame);
            publisher.SendFrame(jsonFrame);
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