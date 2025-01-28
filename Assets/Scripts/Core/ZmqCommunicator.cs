using System;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;

public class ZmqCommunicator : IDisposable
{
    private PushSocket sender;
    private PullSocket receiver;
    private bool isRunning = false;
    private int messageCount = 0;

    public ZmqCommunicator(
        string sendAddress = "tcp://localhost:5556",
        string receiveAddress = "tcp://localhost:5557")
    {
        try
        {
            // Initialize NetMQ sockets
            sender = new PushSocket();
            receiver = new PullSocket();
            
            sender.Connect(sendAddress);
            receiver.Connect(receiveAddress);
            
            isRunning = true;
            Debug.Log($"ZMQ Communicator initialized - Sending to: {sendAddress}, Receiving from: {receiveAddress}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize ZMQ: {e.Message}");
            isRunning = false;
        }
    }

    public bool SendMessage(string message)
    {
        if (!isRunning) return false;

        try
        {
            messageCount++;
            Debug.Log($"Sending: {message}");
            sender.SendFrame(message);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in SendMessage: {e.Message}");
            return false;
        }
    }

    public string ReceiveMessage(bool nonBlocking = true)
    {
        if (!isRunning) return null;

        try
        {
            bool hasMore;
            if (nonBlocking)
            {
                if (receiver.TryReceiveFrameString(out string message, out hasMore))
                {
                    Debug.Log($"Received: {message}");
                    return message;
                }
                return null;
            }
            else
            {
                string message = receiver.ReceiveFrameString(out hasMore);
                Debug.Log($"Received: {message}");
                return message;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in ReceiveMessage: {e.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        if (isRunning)
        {
            sender?.Dispose();
            receiver?.Dispose();
            isRunning = false;
            Debug.Log("ZMQ Communicator disposed");
        }
    }
}