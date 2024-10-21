using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    static readonly string[] staticDirenctions = { "Stay_U", "Stay_L", "Stay_D", "Stay_R" };
    static readonly string[] moveDirenctions = { "Walk_U", "Walk_L", "Walk_D", "Walk_R" };

    private int lastDirection;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 5f;
    private PlayerInputActions playerControls;
    private InputAction move;
    private InputAction fire;

    private Vector2 inputVector = Vector2.zero;
    // Start is called before the first frame update
    void Awake()
    {
        playerControls = new PlayerInputActions();
    }
    private void OnEnable()
    {
        move = playerControls.Player.Move;
        move.Enable();
    }
    private void OnDisable()
    {
        move.Disable();
    }
    // Update is called once per frame
    void Update()
    {
        inputVector = move.ReadValue<Vector2>();
        inputVector = Vector2.ClampMagnitude(inputVector, 1f); // make sure that diagonal movement doesn't move faster
    }
    private void FixedUpdate()
    {
        Vector2 currentPos = rb.position;
        //inputVector = move.ReadValue<Vector2>();
        //inputVector = Vector2.ClampMagnitude(inputVector, 1f); // make sure that diagonal movement doesn't move faster
        Vector2 movement = inputVector * moveSpeed; //rb.velocity = moveDirection * moveSpeed;
        Vector2 newPos = currentPos + movement * Time.fixedDeltaTime;
        SetDirection(movement);
        rb.MovePosition(newPos);
    }

    private void SetDirection(Vector2 direction)
    {
        string[] directionArray = null;

        //measure the magnitude of the input vector
        if (direction.magnitude < 0.01f)
        {
            //if the player is not moving, set the animation to stay
            directionArray = staticDirenctions;
        }
        else
        {
            directionArray = moveDirenctions;
            lastDirection = DirectionToIndex(direction, 4);
        }
        animator.Play(directionArray[lastDirection]);
    }

    //this function converts a Vector2 to an index to a slice araound a circle
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
}
