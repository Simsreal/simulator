using System.Collections.Generic;
using UnityEngine;

public class RayCaster : MonoBehaviour
{
    [Header("Ray Parameters")]
    public int rayCount = 12;
    public float maxDistance = 100f;
    public float spreadAngle = 120f;
    public Vector3 rayStartOffset = new Vector3(0, 1, 0);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public IList<RaycastHit> hits { get; private set; } = null;

    // Update is called once per frame
    void FixedUpdate()
    {
        hits = GetAllRaycastHits();

        //foreach (var hit in hits)
        //{
        //    if (hit.collider != null)
        //    {
        //        Debug.Log($"Hit: {hit.collider.name}, Distance: {hit.distance}");
        //    }
        //}
    }

    List<RaycastHit> GetAllRaycastHits()
    {
        List<RaycastHit> results = new List<RaycastHit>();
        Vector3 origin = transform.position + rayStartOffset;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = Mathf.Lerp(-spreadAngle / 2, spreadAngle / 2, (float)i / (rayCount - 1));
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;

            RaycastHit hit;
            bool isHit = Physics.Raycast(origin, dir, out hit, maxDistance);

            results.Add(isHit ? hit : new RaycastHit { distance = -1 });

            // Draw the ray in the editor for debugging
            Debug.DrawRay(origin, dir * (isHit ? hit.distance : maxDistance),
                         isHit ? Color.green : Color.yellow);
        }

        return results;
    }
}
