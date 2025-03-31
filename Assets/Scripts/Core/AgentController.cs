using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;
using NUnit.Framework.Internal.Commands;

public class AgentController : MonoBehaviour
{
    private ZmqCommunicator zmqCommunicator;
    private Queue<Cmd> commandQueue;
    private Rigidbody controlledObject;
    private Transform transform;

    public Camera cameraToCapture;
    public RenderTexture renderTexture;
    public string cameraName = "egocentric"; // Expose cameraName as a public parameter

    // Add public fields for resolution
    public int captureWidth = 640;  // Default width
    public int captureHeight = 480; // Default height

    //public float maxSpeed = 1.0f;
    public float acceleration = 1.0f;

    void Start()
    {
        string configPath = Path.Combine(Application.persistentDataPath, "zmq_config.json");
        Debug.Log("Loading zmq config from " +  configPath);
        ZmqConfig config = new ZmqConfig
        {
            pubAddress = "tcp://0.0.0.0:5556",
            subAddress = "tcp://127.0.0.1:5557"
        };
        if (!File.Exists(configPath))
        {
            TextAsset configFile = Resources.Load<TextAsset>("zmqConfig");
            if (configFile != null)
            {
                Debug.Log("Loading zmq config from text asset zmqConfig.");
                config = JsonUtility.FromJson<ZmqConfig>(configFile.text);
            }
            Debug.Log("Creating default config file at " + configPath);
            File.WriteAllText(configPath, JsonUtility.ToJson(config, true));
        }
        else
        {
            string text = File.ReadAllText(configPath);

            config = JsonUtility.FromJson<ZmqConfig>(text);
        }

        if (string.IsNullOrEmpty(config.pubAddress))
        {
            Debug.LogWarning("Empty pub address! Reset to default.");
            config.pubAddress = "tcp://0.0.0.0:5556";
        }
        if (string.IsNullOrEmpty(config.subAddress))
        {
            Debug.LogWarning("Empty sub address! Reset to default.");
            config.subAddress = "tcp://127.0.0.1:5557";
        }

        controlledObject = GetComponent<Rigidbody>();
        transform = GetComponent<Transform>();

        if (cameraToCapture == null)
        {
            // Use the cameraName parameter to find the camera
            cameraToCapture = GameObject.Find(cameraName)?.GetComponent<Camera>();
            if (cameraToCapture == null)
            {
                Debug.LogError($"Camera named '{cameraName}' not found!");
                return;
            }
        }

        if (renderTexture == null)
        {
            // Use the specified captureWidth and captureHeight
            renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
        }

        cameraToCapture.targetTexture = renderTexture;

        commandQueue = new Queue<Cmd>();

        zmqCommunicator = new ZmqCommunicator(config.pubAddress, config.subAddress);
        zmqCommunicator.OnCmdReceived += OnCmdIn;

        StartCoroutine(ProcessCommands());
    }
    private void OnCmdIn(Cmd cmd)
    {
        lock (commandQueue)
        {
            commandQueue.Enqueue(cmd);
        }
    }
    private IEnumerator ProcessCommands()
    {
        while (true)
        {
            Cmd cmd = null;
            lock (commandQueue)
            {
                if (commandQueue.Count == 0)
                {
                    cmd = null;
                    yield return null;
                }
                else
                {
                    cmd = commandQueue.Dequeue();
                }
                //_commandQueue.Clear(); // Ignore all following commands
            }
            if (cmd == null) continue;

            if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - cmd.TimestampMs > 200) continue;

            ApplyCommand(cmd);
        }
    }
    private void ApplyCommand(Cmd cmd)
    {
        if (controlledObject == null) return;

        // Move
        Vector3 targetDirection = new Vector3(cmd.X, cmd.Y, 0);
        targetDirection.Normalize();

        controlledObject.AddForce(targetDirection * acceleration, ForceMode.Acceleration);

        // Rotate
        Quaternion targetRotation = Quaternion.Euler(0, 0, -cmd.Orientation); // Z-Axis Rotation
        controlledObject.rotation = Quaternion.Slerp(
            controlledObject.rotation,
            targetRotation,
            Time.deltaTime * 5f
        );
    }

    private class Status
    {
        //public float X {  get; set; }
        //public float Y { get; set; }
        //public float Z { get; set; }
        //public float Orientation { get; set; }
        public byte[] Frame { get; set; }
        public int HitPoint { get; set; }
    }

    void Update()
    {
        Status s = new Status();
        s.Frame = CaptureViewBytes() ?? new byte[0];
        s.HitPoint = (int)GetComponent<HitPoints>().HitPoint;
        zmqCommunicator.SendFrame( s );
    }

    public Texture2D CaptureView()
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        cameraToCapture.Render();

        Texture2D image = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        image.Apply();

        RenderTexture.active = currentRT;
        return image;
    }

    public byte[] CaptureViewBytes()
    {
        Texture2D image = CaptureView();
        byte[] bytes = image.EncodeToJPG();
        UnityEngine.Object.Destroy(image);
        return bytes;
    }


    void OnDestroy()
    {
        zmqCommunicator?.Dispose();
    }
}