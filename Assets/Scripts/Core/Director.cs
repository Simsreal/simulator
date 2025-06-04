using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Director : MonoBehaviour
{
    public AgentController agentController; // Assign in Inspector or auto-find
    public Transform playerTransform; // Assign in Inspector or auto-find
    public Vector3 playerStartPosition; // Set in Inspector or auto-capture at Start
    public Quaternion playerStartRotation; // Set in Inspector or auto-capture at Start
    public GameObject foodPrefab; // Set in Inspector to the food prefab
    public int foodCount = 10; // Number of food items to spawn

    public Vector3 foodAreaCenter = Vector3.zero;
    public Vector2 foodAreaSize = new Vector2(40f, 40f);
    private List<GameObject> spawnedFood = new List<GameObject>();

    private bool resetDetected = false;

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
        SpawnFoodInArea();
    }

    void Update()
    {
        if (!resetDetected && agentController != null)
        {
            if (agentController.status == 2 || agentController.status == 3)
            {
                Debug.Log("Reset detected: Agent has won or died.");
                resetDetected = true;
                StartCoroutine(ResetAfterDelay(5f));
            }
        }
    }

    IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetGame();
        resetDetected = false;
    }

    void ResetGame()
    {
        Debug.Log("Resetting game state...");
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

        // Try to reset the agent's hunger component if it exists
        Hunger hunger = agentController.GetComponent<Hunger>();
        if (hunger != null)
        {
            hunger.Reset();
        }

        // Randomize gate positions
        RandomizeGatePositions();

        // Respawn food in the area
        SpawnFoodInArea();
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

    private void SpawnFoodInArea()
    {
        // Destroy previously spawned food items
        foreach (var food in spawnedFood)
        {
            if (food != null)
                Destroy(food);
        }
        spawnedFood.Clear();

        if (foodPrefab == null) return;

        for (int i = 0; i < foodCount; i++)
        {
            float x = Random.Range(foodAreaCenter.x - foodAreaSize.x / 2, foodAreaCenter.x + foodAreaSize.x / 2);
            float z = Random.Range(foodAreaCenter.z - foodAreaSize.y / 2, foodAreaCenter.z + foodAreaSize.y / 2);
            float y = foodAreaCenter.y;
            Vector3 pos = new Vector3(x, y, z);
            GameObject food = Instantiate(foodPrefab, pos, Quaternion.identity);
            spawnedFood.Add(food);
        }
    }
}
