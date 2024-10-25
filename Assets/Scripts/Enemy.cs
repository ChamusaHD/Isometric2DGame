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

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform target;

    [SerializeField] private Collider2D attackArea;

    [SerializeField] private float radius = 40.0f;
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float maxHp = 3f;
    private float currentHp;

    [SerializeField] private float attackRange = 0.2f;

    private Vector2 moveDirection;

    // Destination of our current move
    private Vector3 destination;

    private State state;

    bool destinationReached = false;
    private float obstacleAvoidanceRange = 3f;
    [SerializeField] private LayerMask obstacleLayer;

    void Start()
    {
        state = State.Patrol;
        currentHp = maxHp;
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
                //FindTarget();
                break;

            case State.Dead:
                Death();
                break;
        }
        print("Enemy current State: " + state);
    }
    void FixedUpdate()
    {
        if (state == State.Attack)
        {
            // Stop all movement during the attack state
            rb.velocity = Vector2.zero;
        }

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
            // Generate a random point inside the patrol radius
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
        // Calculate the main direction towards the target
        Vector2 directionToTarget = (direction - (Vector2)transform.position).normalized;

        // Define how many rays to cast and the angle spread
        int numRays = 3;                    // Number of rays to cast
        float spreadAngle = 20f;             // Angle spread of the rays (in degrees)
        float rayAngleStep = spreadAngle / (numRays - 1);  // Step between rays
        float startAngle = -spreadAngle / 2; // Start angle of the first ray

        // Variable to store avoidance direction
        Vector2 avoidanceDirection = Vector2.zero;

        // Iterate through each ray
        for (int i = 0; i < numRays; i++)
        {
            // Calculate the current ray's angle relative to the target direction
            float currentAngle = startAngle + (i * rayAngleStep);

            // Rotate the directionToTarget by the currentAngle to get the new ray direction
            Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * directionToTarget;

            // Cast the ray
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, 0.5f, obstacleLayer);

            // Optional: Debug ray to visualize raycast in Scene view
            Debug.DrawRay(transform.position, rayDirection * 0.5f, Color.red);

            if (hit.collider != null)
            {
                // If a ray hits an obstacle, calculate the avoidance direction by using the hit normal
                avoidanceDirection += Vector2.Perpendicular(hit.normal).normalized;
            }
        }

        if (avoidanceDirection != Vector2.zero)
        {
            // If there is any obstacle hit, move in the avoidance direction (average of the hits)
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
        if (Vector2.Distance(transform.position, target.transform.position) <= radius && !animator.GetCurrentAnimatorStateInfo(0).IsName("Stunned")) //animator.GetCurrentAnimatorStateInfo(0).
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
        attackArea.GetComponent<AttackArea>().MeleeAttack();
        animator.SetTrigger("Attack");
    }

    public void TakeDamage(float _damage)
    {
        currentHp -= _damage;
        
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
        }

        Debug.Log(gameObject.name + "Current hp: " + currentHp);
    }
    private void Death()
    {
        currentHp = 0;
        rb.velocity = Vector2.zero;
        animator.SetBool("Chase", false);
        animator.SetBool("Dead", true);
        //Destroy(gameObject, 2f);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

}
