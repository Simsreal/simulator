using UnityEngine;
using System;

public class Hunger : MonoBehaviour
{
    [Header("Hunger Settings")]
    public float maxHunger = 100f;
    public float hungerDecreaseRate = 5f; // per second
    public float currentHunger;

    // Triggered when hunger reaches zero
    public event Action OnHungerDepleted;

    private bool depleted = false;

    void Start()
    {
        currentHunger = maxHunger;
        depleted = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHunger > 0f)
        {
            currentHunger -= hungerDecreaseRate * Time.deltaTime;
            if (currentHunger <= 0f)
            {
                currentHunger = 0f;
                if (!depleted)
                {
                    depleted = true;
                    OnHungerDepleted?.Invoke();
                }
            }
        }
    }

    public void Feed(float amount)
    {
        if (amount <= 0f) return;
        currentHunger += amount;
        if (currentHunger > maxHunger)
            currentHunger = maxHunger;
        if (currentHunger > 0f)
            depleted = false;
    }

    public void Reset()
    {
        currentHunger = maxHunger;
    }
}
