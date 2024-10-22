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
        Attack
    }

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform target;

    [SerializeField] private float radius = 40.0f;
    [SerializeField] private float speed = 5.0f;

    private float attackRannge = 0.5f;

    private Vector2 moveDirection;

    // Destination of our current move
    private Vector2 destination;

    private State state;

    bool destinationReached = false;

    void Start()
    {
        state = State.Patrol;
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        //PickNewRandomDestination();
    }

    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            switch (state)
            {
                default:
                case State.Idle:

                    StartCoroutine(StartPatrolCountdown());
                    break;

                case State.Patrol:

                    PatrolMovement();
                    break;

                case State.Chase:
                    Chase();
                    
                    break;

                case State.Attack:
                    Attack();
                    break;
            }
            FindTarget();
            print(state);
        }

    }
    private void PatrolMovement()
    {
        
        // Calculate the direction towards the destination
        Vector2 direction = (destination - (Vector2)transform.position);

        // Check if the enemy is close enough to the destination
        if (Vector2.Distance(transform.position, destination) < 0.1f)
        {
            print("Destination reached!!!!");
            PickNewRandomDestination();
            if (!destinationReached)
            {
                destinationReached = true;
                // Reached destination, pick a new random destination
                print("Destination reached");
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
        if (target)
        {
            if (Vector2.Distance(transform.position, target.transform.position) < radius)
            {
                state = State.Chase;

                if (Vector2.Distance(transform.position, target.transform.position) < attackRannge)
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
    }

    private void Chase()
    {
        rb.velocity = (target.position - transform.position).normalized * speed;
        animator.SetBool("Chase", true);
        animator.SetBool("Patrol", false);
    }
    private void Attack()
    {
        Debug.Log("Attacking");
        rb.velocity = Vector2.zero;
        animator.SetTrigger("Attack");
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
