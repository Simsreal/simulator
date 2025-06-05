using UnityEngine;

public class MovingGate : MonoBehaviour
{
    public Vector3 direction = Vector3.up;
    public float amplitude = 1.0f;
    public float frequency = 1.0f;

    private Vector3 initialPosition;
    private float timeOffset;

    public int seed = 42;

    private System.Random randomGenerator;

    void Start()
    {
        initialPosition = transform.position;

        if(seed == -1)
        {
            seed = (int)System.DateTime.Now.Ticks;
        }
        randomGenerator = new System.Random(seed);

        timeOffset = (float)randomGenerator.NextDouble() * 2 * Mathf.PI;
    }

    void Update()
    {
        float sineValue = Mathf.Sin(2 * Mathf.PI * frequency * (Time.time + timeOffset));

        Vector3 offset = direction.normalized * (sineValue * amplitude);

        transform.position = initialPosition + offset;
    }
}
