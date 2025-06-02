using UnityEngine;

public class TrapLifetime : MonoBehaviour
{
    private TrapSpawner spawner;
    private float lifetime;
    private bool destroyed = false;

    public void Init(TrapSpawner spawner, float lifetime)
    {
        this.spawner = spawner;
        this.lifetime = lifetime;

        // Destroy the trap after its lifetime
        Invoke(nameof(DestroySelf), lifetime);
    }

    void DestroySelf()
    {
        if (!destroyed)
        {
            destroyed = true;
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (!destroyed && spawner != null)
        {
            destroyed = true;
            spawner.OnTrapDestroyed();
        }
    }
}
