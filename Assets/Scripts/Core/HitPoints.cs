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

        if (currentHitPoints <= 0)
        {
            currentHitPoints = 0;
            // TODO: trigger events
        }
    }
}
