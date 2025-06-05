using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;
using NUnit.Framework.Internal.Commands;
using Newtonsoft.Json;

public class AgentController : MonoBehaviour
{
    private const int commandStaleThresholdS = 2;
    private ZmqCommunicator zmqCommunicator;
    private Queue<Cmd> commandQueue;
    private Rigidbody controlledObject;

    public float maxSpeed = 10.0f;
    public float acceleration = 1.0f;
    public int status = 0; // 0 - normal, 1 - fell down, 2 - won, await reset, 3 - dead, await reset

    private float lastStatusSendTime = 0f;
    private float statusSendInterval = 0.05f; // 20Hz instead of every frame

    // Add coroutine reference for proper cleanup
    private Coroutine processCommandsCoroutine;
    private bool isShuttingDown = false;

    public void ResetStatus()
    {
        controlledObject = GetComponent<Rigidbody>();
        if (controlledObject == null)
        {
            Debug.LogError("Rigidbody component is missing on the AgentController GameObject.");
        }
        else
        {
            controlledObject.linearVelocity = Vector3.zero;
            controlledObject.angularVelocity = Vector3.zero;
        }
        status = 0;
        latestCmd = null;
    }

    void Start()
    {
        isShuttingDown = false;

        string configPath = Path.Combine(Application.persistentDataPath, "zmq_config.json");
        Debug.Log("Loading zmq config from " + configPath);
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
        commandQueue = new Queue<Cmd>();

        try
        {
            zmqCommunicator = new ZmqCommunicator(config.pubAddress, config.subAddress);
            zmqCommunicator.OnCmdReceived += OnCmdIn;

            processCommandsCoroutine = StartCoroutine(ProcessCommands());

            Hunger hunger = GetComponent<Hunger>();
            if (hunger != null)
            {
                hunger.OnHungerDepleted += OnHungerDepletedHandler;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize ZMQ: {e.Message}");
        }
    }

    private void OnHungerDepletedHandler(Hunger sender)
    {
        if (status == 2) // If already won, ignore hunger depletion
            return;
        status = 3; // dead, await reset
    }

    private void OnCmdIn(Cmd cmd)
    {
        if (isShuttingDown) return;

        Debug.Log($"[DEBUG] Command received: Movement={cmd?.Action?.Movement}, Timestamp={cmd?.TimestampS}, Confidence={cmd?.Action?.Confidence}");
        lock (commandQueue)
        {
            if (commandQueue.Count > 5) // Keep max 5 commands
            {
                commandQueue.Clear();
            }
            commandQueue.Enqueue(cmd);
        }
    }

    private Cmd latestCmd = null;
    private long lastCommandTimestamp = 0; // Timestamp of the last command processed

    private IEnumerator ProcessCommands()
    {
        while (!isShuttingDown)
        {
            Cmd cmd = null;
            lock (commandQueue)
            {
                if (commandQueue.Count == 0)
                {
                    cmd = null;
                }
                else
                {
                    cmd = commandQueue.Dequeue();
                }
            }

            if (cmd == null)
            {
                yield return new WaitForSeconds(0.01f); // Small delay instead of null
                continue;
            }

            Debug.Log($"Processing command: {cmd.Action.Movement} at {cmd.TimestampS}");

            // Check if the command is too old
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - cmd.TimestampS > commandStaleThresholdS)
            {
                Debug.Log($"Stale command: {cmd.Action.Movement} at {DateTimeOffset.UtcNow.ToUnixTimeSeconds() - cmd.TimestampS}s before (older than {commandStaleThresholdS} s), processing anyway.");
            }
            if (cmd.TimestampS < lastCommandTimestamp)
            {
                Debug.Log($"Ignoring command: {cmd.Action.Movement} at {cmd.TimestampS} (earlier than the last command at {lastCommandTimestamp})");
                continue;
            }

            latestCmd = cmd;
            yield return null; // Yield control back to Unity
        }
    }

    private void ApplyCommand(Cmd cmd)
    {
        //Debug.Log($"Applying command: {cmd.Action.Movement} at {cmd.TimestampS}");
        lastCommandTimestamp = cmd.TimestampS;
        if (status == 1)
        {
            if (cmd.Action.Movement == "standup")
            {
                status = 0; // Reset status to normal
            }
            return; // Ignore commands while in "fell down" state
        }
        if (status == 2 || status == 3)
        {
            // await reset
            return; // Ignore commands while in "won"/"dead" state
        }

        if (controlledObject == null)
        {
            Debug.LogWarning("Controlled object is null, cannot apply command.");
            return;
        }

        // Move
        Vector3 targetDirection = Vector3.zero;
        switch (cmd.Action.Movement)
        {
            case "moveforward": // forward
                targetDirection = transform.forward;
                break;
            case "movebackward": // backward
                targetDirection = -transform.forward;
                break;
            case "moveleft": // left
                targetDirection = -transform.right;
                break;
            case "moveright": // right
                targetDirection = transform.right;
                break;
            case "lookleft": // turn left
                controlledObject.MoveRotation(
                    controlledObject.rotation * Quaternion.Euler(0, -90f * Time.deltaTime, 0)
                );
                break;
            case "lookright": // turn right
                controlledObject.MoveRotation(
                    controlledObject.rotation * Quaternion.Euler(0, 90f * Time.deltaTime, 0)
                );
                break;
        }

        if (targetDirection != Vector3.zero)
        {
            float vel = controlledObject.linearVelocity.magnitude;
            if (vel < maxSpeed)
            {
                controlledObject.AddForce(targetDirection.normalized * acceleration, ForceMode.Impulse);
            }
        }
    }

    private class LineOfSight
    {
        public float Distance { get; set; }
        public int Type { get; set; }
    }

    private class Status
    {
        [JsonProperty("x")]
        public float X { get; set; }
        [JsonProperty("y")]
        public float Y { get; set; }
        [JsonProperty("z")]
        public float Z { get; set; }
        //public float Orientation { get; set; }
        //public byte[] Frame { get; set; }

        [JsonProperty("line_of_sight")]
        public List<LineOfSight> LineOfSight { get; } = new List<LineOfSight>();

        // May be used for future
        [JsonProperty("hit_point")]
        public int HitPoint { get; set; }

        [JsonProperty("state")]
        public int State { get; set; }
        [JsonProperty("hunger")]
        public float Hunger { get; set; }
    }

    private void FixedUpdate()
    {
        if (!isShuttingDown && latestCmd != null)
        {
            ApplyCommand(latestCmd);
        }
    }

    void Update()
    {
        if (isShuttingDown) return;

        if (Time.time - lastStatusSendTime < statusSendInterval)
        {
            return;
        }
        lastStatusSendTime = Time.time;

        Status s = new Status();
        s.HitPoint = (int)GetComponent<HitPoints>().HitPoint;

        s.X = transform.position.x;
        s.Y = transform.position.y;
        s.Z = transform.position.z;
        s.State = status;
        s.Hunger = GetComponent<Hunger>().currentHunger;

        IList<RaycastHit> hits = GetComponent<RayCaster>().hits;
        foreach (var hit in hits)
        {
            if (hit.collider)
            {
                if (hit.collider.CompareTag("Obstacle"))
                {
                    s.LineOfSight.Add(new LineOfSight
                    {
                        Distance = hit.distance,
                        Type = 1
                    });
                }
                else if (hit.collider.CompareTag("Enemy"))
                {
                    s.LineOfSight.Add(new LineOfSight
                    {
                        Distance = hit.distance,
                        Type = 2
                    });
                }
                else if (hit.collider.CompareTag("Trap"))
                {
                    s.LineOfSight.Add(new LineOfSight
                    {
                        Distance = hit.distance,
                        Type = 3
                    });
                }
                else if (hit.collider.CompareTag("Goal"))
                {
                    s.LineOfSight.Add(new LineOfSight
                    {
                        Distance = hit.distance,
                        Type = 4
                    });
                }
                else if (hit.collider.CompareTag("People"))
                {
                    s.LineOfSight.Add(new LineOfSight
                    {
                        Distance = hit.distance,
                        Type = 5
                    });
                }
                else if (hit.collider.CompareTag("Food"))
                {
                    s.LineOfSight.Add(new LineOfSight
                    {
                        Distance = hit.distance,
                        Type = 6
                    });
                }
            }
            else
            {
                s.LineOfSight.Add(new LineOfSight
                {
                    Distance = -1,
                    Type = 0
                });
            }
        }

        try
        {
            zmqCommunicator?.SendFrame(s);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to send frame: {e.Message}");
        }

        // Input handling (existing code)
        if (Input.GetKey(KeyCode.W))
        {
            Cmd cmd = new Cmd
            {
                TimestampS = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Action = new Action { Movement = "moveforward" }
            };
            OnCmdIn(cmd);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Cmd cmd = new Cmd
            {
                TimestampS = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Action = new Action { Movement = "movebackward" }
            };
            OnCmdIn(cmd);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Cmd cmd = new Cmd
            {
                TimestampS = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Action = new Action { Movement = "moveleft" }
            };
            OnCmdIn(cmd);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Cmd cmd = new Cmd
            {
                TimestampS = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Action = new Action { Movement = "moveright" }
            };
            OnCmdIn(cmd);
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            Cmd cmd = new Cmd
            {
                TimestampS = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Action = new Action { Movement = "lookleft" }
            };
            OnCmdIn(cmd);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            Cmd cmd = new Cmd
            {
                TimestampS = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Action = new Action { Movement = "lookright" }
            };
            OnCmdIn(cmd);
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            Cmd cmd = new Cmd
            {
                TimestampS = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Action = new Action { Movement = "standup" }
            };
            OnCmdIn(cmd);
        }

        // Add this at the end of the Update() method, after the existing input handling
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC pressed - stopping simulation manually");
            StopSimulation();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Trap"))
        {
            status = 1; // falling down
        }
        else if (collision.collider.CompareTag("Goal"))
        {
            status = 2; // win
        }
    }

    // Keep only these cleanup methods:
    void OnApplicationQuit()
    {
        CleanupZMQ();
    }

    void OnDestroy()
    {
        CleanupZMQ();
    }

    // Add a manual stop method that you can call when you actually want to stop
    public void StopSimulation()
    {
        CleanupZMQ();
    }

    private void CleanupZMQ()
    {
        if (isShuttingDown) return;

        isShuttingDown = true;
        Debug.Log("Starting ZMQ cleanup...");

        // Stop the coroutine
        if (processCommandsCoroutine != null)
        {
            StopCoroutine(processCommandsCoroutine);
            processCommandsCoroutine = null;
        }

        // Remove event handlers
        if (zmqCommunicator != null)
        {
            zmqCommunicator.OnCmdReceived -= OnCmdIn;
        }

        // Remove hunger handler
        Hunger hunger = GetComponent<Hunger>();
        if (hunger != null)
        {
            hunger.OnHungerDepleted -= OnHungerDepletedHandler;
        }

        // Dispose ZMQ communicator
        try
        {
            zmqCommunicator?.Dispose();
            zmqCommunicator = null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error during ZMQ disposal: {e.Message}");
        }

        Debug.Log("ZMQ cleanup completed.");
    }
}