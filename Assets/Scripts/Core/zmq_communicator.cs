using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
    private volatile bool isDisposing = false;
    private readonly object disposeLock = new object();

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

            // Set socket options for better performance and cleanup
            publisher.Options.Linger = TimeSpan.FromMilliseconds(100);

            // Bind so Python subscribers can connect.
            publisher.Bind(pubAddress);

            // 2) Subscriber for receiving messages from Python.
            subscriber = new SubscriberSocket();

            // Set socket options for better performance and cleanup
            subscriber.Options.Linger = TimeSpan.FromMilliseconds(100);

            // Connect to Python's publisher address.
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
        if (!isRunning || isDisposing)
        {
            return;
        }

        try
        {
            // Use non-blocking receive with timeout
            if (e.Socket.TryReceiveFrameString(TimeSpan.FromMilliseconds(10), out string message))
            {
                // Deserialize the JSON string into the Cmds class
                Cmd cmd = JsonConvert.DeserializeObject<Cmd>(message);
                OnCmdReceived?.Invoke(cmd);
            }
        }
        catch (Exception ex)
        {
            if (!isDisposing)
            {
                Debug.LogError($"Error reading subscriber message: {ex.Message}");
            }
        }
    }

    public void SendFrame(object frame)
    {
        if (!isRunning || isDisposing)
        {
            return;
        }

        try
        {
            string jsonFrame = JsonConvert.SerializeObject(frame);

            // Use non-blocking send with timeout
            if (!publisher.TrySendFrame(TimeSpan.FromMilliseconds(10), jsonFrame))
            {
                Debug.LogWarning("Failed to send frame - timeout");
            }
        }
        catch (Exception e)
        {
            if (!isDisposing)
            {
                Debug.LogError($"Error in SendFrame: {e.Message}");
            }
        }
    }

    public void Dispose()
    {
        lock (disposeLock)
        {
            if (!isRunning || isDisposing) return;

            isDisposing = true;
            isRunning = false;
        }

        try
        {
            Debug.Log("Starting ZMQ Communicator disposal...");

            // Stop the poller first with timeout
            if (poller != null && poller.IsRunning)
            {
                poller.Stop();

                // Wait for poller to stop with timeout
                var stopTimeout = DateTime.Now.AddMilliseconds(500);
                while (poller.IsRunning && DateTime.Now < stopTimeout)
                {
                    Thread.Sleep(10);
                }

                if (poller.IsRunning)
                {
                    Debug.LogWarning("Poller did not stop within timeout");
                }
            }

            // Unsubscribe from events
            if (subscriber != null)
            {
                try
                {
                    subscriber.ReceiveReady -= OnSubscriberMessageReceived;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error unsubscribing from events: {ex.Message}");
                }
            }

            // Dispose poller
            try
            {
                poller?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error disposing poller: {ex.Message}");
            }

            // Close and dispose subscriber
            try
            {
                subscriber?.Close();
                subscriber?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error disposing subscriber: {ex.Message}");
            }

            // Close and dispose publisher
            try
            {
                publisher?.Close();
                publisher?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error disposing publisher: {ex.Message}");
            }

            // Cleanup all things relate to NetMQ with timeout
            try
            {
                var cleanupTask = System.Threading.Tasks.Task.Run(() => NetMQConfig.Cleanup());
                if (!cleanupTask.Wait(1000)) // 1 second timeout
                {
                    Debug.LogWarning("NetMQ cleanup timed out");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error during NetMQ cleanup: {ex.Message}");
            }

            Debug.Log("ZMQ Communicator disposed successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error disposing ZMQ Communicator: {e.Message}");
        }
        finally
        {
            isDisposing = false;
        }
    }

    // Add finalizer as safety net
    ~ZmqCommunicator()
    {
        Dispose();
    }
}