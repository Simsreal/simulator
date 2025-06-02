using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Food Settings")]
    public float feedAmount = 30f;

    private bool hasFed = false; // need lock? unity main thread is single-threaded

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        TryFeed(collision.gameObject);
    }

    private void TryFeed(GameObject target)
    {
        Hunger hunger = target.GetComponent<Hunger>();
        if (hunger != null)
        {
            if (hasFed) return;
            hasFed = true;
            hunger.Feed(feedAmount);
            Destroy(gameObject);
        }
    }
}
