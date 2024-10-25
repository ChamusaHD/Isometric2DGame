using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Dead
    }

    private State state;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform target;

    [SerializeField] private Collider2D attackArea;

    [SerializeField] private float radius = 40.0f;
    [SerializeField] private float speed = 5.0f;

    [SerializeField] private FloatingHealthBar healthBar;
    [SerializeField] private float maxHp = 3f;
    private float currentHp;

    [SerializeField] private float attackRange = 0.2f;

    private Vector3 destination;

    [SerializeField] private LayerMask obstacleLayer;

    [SerializeField] private ParticleSystem particleSystemPrefab;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        healthBar = GetComponentInChildren<FloatingHealthBar>();
    }
    void Start()
    {
        state = State.Patrol;
        currentHp = maxHp;
        healthBar.UpdateHealthBar(currentHp, maxHp);
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        PickNewRandomDestination();
    }

    // Update is called once per frame
    void Update()
    {

        switch (state)
        {
            default:
            case State.Idle:
                StartCoroutine(StartPatrolCountdown());
                break;

            case State.Patrol:
                PatrolMovement();
                FindTarget();
                speed = 1f;
                break;

            case State.Chase:
                Chase();
                FindTarget();
                speed = 1.5f;
                break;

            case State.Attack:
                Attack();
                break;

            case State.Dead:
                Death();
                break;
        }
        print("Enemy current State: " + state);
    }
    private void PatrolMovement()
    {
        AvoidObstaclesAndMove(destination);

        // Check if the enemy is close enough to the destination
        if (Vector2.Distance(transform.position, destination) < 0.1f)
        {
            PickNewRandomDestination();
        }
        
        animator.SetBool("Patrol", true);
        animator.SetBool("Chase", false);
    }

    private void PickNewRandomDestination()
    {

        bool validPointFound = false;

        while (!validPointFound)
        {
            Vector2 randomPoint = Random.insideUnitCircle * radius;
            Vector2 potencialDestination = (Vector2)transform.position + randomPoint;

            Collider2D hitObstacle = Physics2D.OverlapCircle(potencialDestination, 0.5f, obstacleLayer);

            if(hitObstacle == null)
            {
                destination = potencialDestination;
                validPointFound = true;
            }
        }
    }
    private void AvoidObstaclesAndMove(Vector2 direction)
    {
        Vector2 directionToTarget = (direction - (Vector2)transform.position).normalized;

        int numRays = 3;                    
        float spreadAngle = 20f;          
        float rayAngleStep = spreadAngle / (numRays - 1); 
        float startAngle = -spreadAngle / 2;

        Vector2 avoidanceDirection = Vector2.zero;

        for (int i = 0; i < numRays; i++)
        {
            // Calculate the current ray's angle relative to the target direction
            float currentAngle = startAngle + (i * rayAngleStep);

            // Rotate the directionToTarget by the currentAngle to get the new ray direction
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * directionToTarget;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, 0.5f, obstacleLayer);

            Debug.DrawRay(transform.position, rayDirection * 0.5f, Color.red);

            if (hit.collider != null)
            {
                // If a ray hits an obstacle, calculate the avoidance direction by using the hit normal
                avoidanceDirection += Vector2.Perpendicular(hit.normal).normalized;
            }
        }

        if (avoidanceDirection != Vector2.zero)
        {
            // If there is any obstacle hit, move in the avoidance direction
            rb.velocity = avoidanceDirection.normalized * speed;
        }
        else
        {
            // If no obstacle, move towards the target
            rb.velocity = directionToTarget * speed;
        }
        //print(direction);
    }

    public void FindTarget()
    {
        if (Vector2.Distance(transform.position, target.transform.position) <= radius && !animator.GetCurrentAnimatorStateInfo(0).IsName("Stunned") && !target.GetComponent<PlayerController>().GetIsDead())
        {
            state = State.Chase;

            if (Vector2.Distance(transform.position, target.transform.position) < attackRange)
            {
                state = State.Attack;
            }
            else
            {
                state = State.Chase;
            }
        }
        else
        {
            state = State.Patrol;
        }
    }

    private void Chase()
    {
        AvoidObstaclesAndMove(target.position);
        animator.SetBool("Chase", true);
        animator.SetBool("Patrol", false);
    }
    private void Attack()
    {
        rb.velocity = Vector2.zero;
        animator.SetTrigger("Attack");
    }

    public void TakeDamage(float damageAmount)
    {
        currentHp -= damageAmount;
        healthBar.UpdateHealthBar(currentHp, maxHp);
        ParticleSystem ps = Instantiate(particleSystemPrefab, transform.position, Quaternion.identity, transform);
        ps.Play();

        if (currentHp > 0) // is alive
        {
            animator.SetTrigger("Stun");
            animator.ResetTrigger("Attack");
            animator.SetBool("Chase", false);
            rb.velocity = Vector2.zero;
            
        }
        else if(currentHp <= 0)
        {
            state = State.Dead;
            GetComponent<Collider2D>().enabled = false;
            Destroy(gameObject, 2f);
        }

        Debug.Log(gameObject.name + "Current hp: " + currentHp);
    }
    private void Death()
    {
        currentHp = 0;
        rb.velocity = Vector2.zero;
        animator.SetBool("Chase", false);
        animator.SetBool("Dead", true);       
    }
    private IEnumerator StartPatrolCountdown()
    {
        yield return new WaitForSeconds(2.0f);
        state = State.Patrol;
        PickNewRandomDestination();
    }
    public void SetVelocityToZero()
    {
        rb.velocity = Vector2.zero;
    }

    public void GetMeleeAttackForAnimation()
    {
        attackArea.GetComponent<AttackArea>().MeleeAttack();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

}
