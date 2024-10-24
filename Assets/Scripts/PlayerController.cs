using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    static readonly string[] staticDirenctions = { "Stay_U", "Stay_L", "Stay_D", "Stay_R" };
    static readonly string[] moveDirenctions = { "Walk_U", "Walk_L", "Walk_D", "Walk_R" };
    static readonly string[] attackDirenctions = { "Slash_U", "Slash_L", "Slash_D", "Slash_R" };
    static readonly string[] hitDirenctions = { "Damage_U", "Damage_L", "Damage_D", "Damage_R" };
    static readonly string[] deadDirenctions = { "Dead_U", "Dead_L", "Dead_D", "Dead_R" };

    private PlayerInputActions playerControls;
    private InputAction move;
    private InputAction meleeAttack;
    private InputAction rangedAttack;

    private Vector2 inputVector = Vector2.zero;
    private int lastDirection;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 5f;


    [SerializeField] private Collider2D attackArea;
    [SerializeField] private bool isAttacking;
    [SerializeField] private bool isDead = false;
    [SerializeField] private bool isDamaged;

    [SerializeField] private float maxHealth = 5;
    private float currentHealth;

    // Start is called before the first frame update
    void Awake()
    {
        currentHealth = maxHealth;
        playerControls = new PlayerInputActions();
    }
    private void OnEnable()
    {
        move = playerControls.Player.Move;
        move.Enable();
        meleeAttack = playerControls.Player.MeleeAttack;
        meleeAttack.Enable(); 
        rangedAttack = playerControls.Player.RangedAttack;
        rangedAttack.Enable();
        
    }
    private void OnDisable()
    {
        move.Disable();
        meleeAttack.Disable();
        rangedAttack.Disable();
    }
    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            inputVector = move.ReadValue<Vector2>();
            inputVector = Vector2.ClampMagnitude(inputVector, 1f); // make sure that diagonal movement doesn't move faster

            if (meleeAttack.triggered)
            {
                isAttacking = true;
                //attackArea.enabled = isAttacking;
                attackArea.GetComponent<AttackArea>().MeleeAttack();
                animator.Play(attackDirenctions[lastDirection]);
            }
            if (rangedAttack.triggered)
            {
                TakeDamage(1);
            }
        }
        else
        {
            rb.MovePosition(transform.position);
        }
       
    }
    private void FixedUpdate()
    {
        if (!isAttacking)
        {
            Vector2 currentPos = rb.position;
            //inputVector = move.ReadValue<Vector2>();
            //inputVector = Vector2.ClampMagnitude(inputVector, 1f); // make sure that diagonal movement doesn't move faster
            Vector2 movement = inputVector * moveSpeed; //rb.velocity = moveDirection * moveSpeed;
            Vector2 newPos = currentPos + movement * Time.fixedDeltaTime;
            SetDirection(movement);
            rb.MovePosition(newPos);
        }
        
    }

    private void SetDirection(Vector2 direction)
    {
        string[] directionArray = null;

        if (!isDead)
        {
            
            //check if is moving
            if (direction.magnitude < 0.01f)
            {

                if (isAttacking)
                {
                    directionArray = attackDirenctions;
                }
                else
                {
                    //if the player is not moving, set the animation to stay
                    directionArray = staticDirenctions;

                    if (isDamaged)
                    {
                        directionArray = hitDirenctions;
                        lastDirection = DirectionToIndex(direction, 4);
                    }
                }
            }
            else
            {
                if (isAttacking)
                {
                    directionArray = attackDirenctions;
                    lastDirection = DirectionToIndex(direction, 4);
                }
                else
                {
                    directionArray = moveDirenctions;
                    lastDirection = DirectionToIndex(direction, 4);

                    if (isDamaged)
                    {
                        directionArray = hitDirenctions;
                        lastDirection = DirectionToIndex(direction, 4);
                    }
                }

                
            }

            
        }
        else
        {
            directionArray = deadDirenctions;
            lastDirection = DirectionToIndex(direction, 4);
        }
        

        animator.Play(directionArray[lastDirection]);
    }

    //converts a Vector2 to an index to a slice araound a circle
    //this goes in a counter-clockwise direction
    private int DirectionToIndex(Vector2 direction, int sliceCount)
    {
        //get the normalized direction
        Vector2 normalDirection = direction.normalized;

        //calculate how many degrees a slice is
        float step = 360 / sliceCount;

        //calculate how many degrees half a slice is
        //we need this to offset the pie, so that the north (UP) slice is aligned in the center
        float halfStep = step/2;

        //get the angle from -180 to 180 of the direction vector relative to the Up vector
        //this will return the angle between direction and North
        float angle = Vector2.SignedAngle(Vector2.up, normalDirection);

        //add the halfside offset
        angle += halfStep;

        //if the angle is negative, add 360 to wrap it around and make it positive
        if (angle < 0)
        {
            angle += 360;
        }

        //calculate the amount of steps required to reach this angle
        float stepcount = angle / step;

        //round it, and this is the answer
        return Mathf.FloorToInt(stepcount);
    }

    public void setIsAttackingToFalse()
    {
        isAttacking = false;
        attackArea.enabled = isAttacking;
    }
    public void SetIsDamagedToFalse()
    {
        isDamaged = false;
    }

    public void TakeDamage(int _damage)
    {
        currentHealth -= _damage;

        if (currentHealth > 0) // is alive
        {
            currentHealth -= _damage;
            isDamaged = true;
            //animator.Play(hitDirenctions[lastDirection]);
            Debug.Log(gameObject.name + "Current hp: " + currentHealth);
        }
        else
        {
            currentHealth = 0;
            rb.velocity = Vector2.zero;
            isDead = true;
            //animator.Play(deadDirenctions[lastDirection]);
           // Destroy(gameObject, 1.5f);
            //state = State.Dead;
        }
    }
}
