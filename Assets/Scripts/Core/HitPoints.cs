using System;
using UnityEngine;

public class HitPoints : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHitPoints = maxHitPoints;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Tooltip("Maximum Hit Points")]
    public float maxHitPoints = 100f;

    [Tooltip("Current Hit Points")]
    [SerializeField] private float currentHitPoints;

    public float HitPoint
    {
        get
        {
            return currentHitPoints;
        }
    }

    public void DamageTaken(float damage)
    {
        currentHitPoints -= damage;

        HitPointsChangedEvent(this);

        if (currentHitPoints <= 0)
        {
            currentHitPoints = 0;
            DeadEvent(this);
        }
    }

    public event Action<HitPoints> HitPointsChangedEvent;
    public event Action<HitPoints> DeadEvent;
}
