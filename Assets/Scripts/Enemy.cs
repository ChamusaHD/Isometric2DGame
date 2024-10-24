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
    private Vector2 destination;

    private State state;

    bool destinationReached = false;

    void Start()
    {
        state = State.Patrol;
        currentHp = maxHp;
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        //PickNewRandomDestination();
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
                break;

            case State.Chase:
                Chase();
                FindTarget();
                break;

            case State.Attack:
                Attack();
                FindTarget();
                break;

            case State.Dead:
                Death();
                break;
        }
        print("Enemy current State: " + state);
    }
    private void PatrolMovement()
    {
        
        // Calculate the direction towards the destination
        Vector2 direction = (destination - (Vector2)transform.position);

        // Check if the enemy is close enough to the destination
        if (Vector2.Distance(transform.position, destination) < 0.1f)
        {
           // print("Destination reached!!!!");
            PickNewRandomDestination();
            if (!destinationReached)
            {
                destinationReached = true;
                // Reached destination, pick a new random destination
               // print("Destination reached");
            }
           
        }
        else
        {
            rb.velocity = direction.normalized * speed;
        }
        animator.SetBool("Patrol", true);
        animator.SetBool("Chase", false);
    }

    private void PickNewRandomDestination()
    {
        Vector3 newDestination;

        // Generate a random point inside the patrol radius
        Vector2 randomPoint = Random.insideUnitCircle * radius;
        newDestination = (Vector2)transform.position + randomPoint;

       /* // Check for obstacles using a raycast
        RaycastHit2D hit = Physics2D.Raycast(transform.position, newDestination - transform.position, Vector2.Distance(transform.position, newDestination));
        // Debugging raycast (optional, you can remove this in production)
        Debug.DrawLine(transform.position, newDestination, Color.red, 1f);

        // If we hit an obstacle, we will discard this destination
        if (hit.collider != null)
        {
            PickNewRandomDestination(); //Try again
        }*/

        // Set the new destination if it's valid
        destination = newDestination;
    }

    private void FindTarget()
    {
        if (target != null)
        {
            if (Vector2.Distance(transform.position, target.transform.position) < radius)
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
                PickNewRandomDestination();
            }
        }
        else
        {
            state = State.Patrol;
            PickNewRandomDestination();
        }
    }

    private void Chase()
    {
        if (target != null)
        {
            rb.velocity = (target.position - transform.position).normalized * speed;
            animator.SetBool("Chase", true);
            animator.SetBool("Patrol", false);
        }
        else
        {
            state = State.Patrol;
            PickNewRandomDestination();
        }
        
    }
    private void Attack()
    {
       // Debug.Log("Attacking");
        rb.velocity = Vector2.zero;
        attackArea.GetComponent<AttackArea>().MeleeAttack();
        animator.SetTrigger("Attack");
    }

    public void TakeDamage(int _damage)
    {
        currentHp -= _damage;
        
        if (currentHp > 0) // is alive
        {
            animator.SetTrigger("Stun");
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
        animator.SetBool("Dead", true);
        Destroy(gameObject, 2f);
    }
    private IEnumerator StartPatrolCountdown()
    {
        yield return new WaitForSeconds(2.0f);
        state = State.Patrol;
        PickNewRandomDestination();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

}
