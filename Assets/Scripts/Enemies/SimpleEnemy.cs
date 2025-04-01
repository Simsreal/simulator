using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public float attackDamage = 10f;
    public float knockbackForce = 15f;
    public float attackCooldown = 1f;
    public float moveSpeed = 10.0f;

    private Transform player;
    private NavMeshAgent agent;
    private bool canAttack = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
            transform.LookAt(player.position);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (canAttack && collision.gameObject.CompareTag("Player"))
        {
            // deal damage
            collision.gameObject.GetComponent<HitPoints>().DamageTaken(attackDamage);

            // knock back
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (collision.transform.position - transform.position).normalized;
                direction = new Vector3(direction.x, 0.5f, direction.z).normalized;
                rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
            }

            // cooling down
            StartCoroutine(AttackCooldown());
        }
    }

    private System.Collections.IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}