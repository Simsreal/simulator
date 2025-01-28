using System;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;

public class ZmqCommunicator : IDisposable
{
    private bool isRunning = false;
    private int messageCount = 0;

    public ZmqCommunicator(string address = "tcp://localhost:5556")
    {
        isRunning = true;
        Debug.Log($"[MOCK] ZMQ Communicator initialized at {address}");
    }

    public bool SendMessage(string message)
    {
        if (!isRunning) return false;

        messageCount++;
        Debug.Log($"[MOCK] Client sending: {message}");
        
        // Simulate receiving a response
        string fakeResponse = GenerateMockResponse(message);
        Debug.Log($"[MOCK] Server received: {message}");
        Debug.Log($"[MOCK] Server sending response: {fakeResponse}");
        Debug.Log($"[MOCK] Client received: {fakeResponse}");
        
        return true;
    }

    private string GenerateMockResponse(string originalMessage)
    {
        return $"Response #{messageCount}: Processed '{originalMessage}'";
    }

    public void Dispose()
    {
        if (isRunning)
        {
            isRunning = false;
            Debug.Log("[MOCK] ZMQ Communicator disposed");
        }
    }
}