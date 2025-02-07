using System.IO;
using UnityEngine;

public class RobotProxy : MonoBehaviour
{
    private ZmqCommunicator zmqCommunicator;

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

        zmqCommunicator = new ZmqCommunicator(config.pubAddress, config.subAddress);
    }

    void Update()
    {
        
    }

    public void SendFrame(RobotFrame frame)
    {
        if (zmqCommunicator != null)
        {
            zmqCommunicator.SendFrame(frame);
        }
        else
        {
            Debug.LogError("ZmqCommunicator not initialized!");
        }
    }


    void OnDestroy()
    {
        zmqCommunicator?.Dispose();
    }
}