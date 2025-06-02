using UnityEngine;
using System.Collections;

public class Director : MonoBehaviour
{
    public AgentController agentController; // Assign in Inspector or auto-find
    public Transform playerTransform; // Assign in Inspector or auto-find
    public Vector3 playerStartPosition; // Set in Inspector or auto-capture at Start
    public Quaternion playerStartRotation; // Set in Inspector or auto-capture at Start
    private bool winDetected = false;

    void Start()
    {
        if (agentController == null)
            agentController = Object.FindFirstObjectByType<AgentController>();
        if (playerTransform == null && agentController != null)
            playerTransform = agentController.transform;
        // Record the initial player position and rotation
        if (playerTransform != null)
            playerStartPosition = playerTransform.position;
        if (playerTransform != null)
            playerStartRotation = playerTransform.rotation;

        RandomizeGatePositions();
    }

    void Update()
    {
        if (!winDetected && agentController != null && agentController.status == 2)
        {
            winDetected = true;
            StartCoroutine(ResetAfterDelay(10f));
        }
    }

    IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetGame();
        winDetected = false;
    }

    void ResetGame()
    {
        // Reset position, rotation and state
        if (agentController != null)
            agentController.status = 0;
        if (playerTransform != null)
            playerTransform.position = playerStartPosition;
        if (playerTransform != null)
            playerTransform.rotation = playerStartRotation;

        // Try to reset the agent's hitpoint component if it exists
        HitPoints hitPoints = agentController.GetComponent<HitPoints>();
        if (hitPoints != null)
        {
            hitPoints.Reset();
        }

        // Randomize gate positions
        RandomizeGatePositions();
    }

    private static void RandomizeGatePositions()
    {
        for (int i = 0; i <= 5; i++)
        {
            string gateName = $"Gate{i}";
            GameObject gate = GameObject.Find(gateName);
            if (gate != null)
            {
                Vector3 pos = gate.transform.position;
                pos.x = Random.Range(-20f, 20f);
                gate.transform.position = pos;
            }
        }
    }
}
