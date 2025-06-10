using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;


/*
 * Quick Notes:
 * A player still may be able to clip up slopes if climbing at an angle.
 * 
 * Need to edit sliding on slopes
 */
public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCD;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    private bool hasAppliedForce = false;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Gravity Control")]
    public float gravityEnableAngle; // Angle at which gravity is enabled on slopes

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;

    private bool sliding;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        inAir,
        sliding
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        //exitingSlope = false;

        startYScale = transform.localScale.y;
        
    }

    // Update is called once per frame
    void Update()
    {
        // ground check:
        { 
        // Note: This assumes origin is from center 
        // grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.001f, whatIsGround);

        // If origin is from base use this:
        grounded = Physics.Raycast(transform.position, Vector3.down, 0.1f, whatIsGround);
        }
        MyInput();
        StateHandler();
        SpeedControl();
        

        // handle drag
        if (grounded)
        {
            rb.linearDamping = groundDrag;
            // Debug.Log("Is grounded");
        }
         else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();

        if (sliding)
        {
            SlidingMovement();
        }

    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCD);
        }
        // when to crouch
        if (Input.GetKey(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            if (!hasAppliedForce) // Downward force only applies on first crouchKey press and not continuous
            {
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse); // Moves the player towards the ground
                rb.AddForce(rb.linearVelocity, ForceMode.Impulse); // Preserve existing momentum

                hasAppliedForce = true;
                // Debug.Log("Downward Force Applied!");
            }
        }
        // stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            hasAppliedForce = false;

        }
        
        // when to slide
        if (Input.GetKey(crouchKey) && (horizontalInput != 0 || verticalInput != 0) && (state == MovementState.sprinting || rb.linearVelocity.magnitude > walkSpeed) && !sliding)
        {
            StartSlide();
        }
        
    }

    private void StateHandler()
    {
        
        // Mode - Sprinting
        if (grounded && Input.GetKey(sprintKey) && !Input.GetKey(crouchKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        // Mode - in Air
        else
            state = MovementState.inAir;

        // Mode - Crouching
        if (Input.GetKey(crouchKey) && !sliding)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // Mode - Sliding
        if(sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.linearVelocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;
            else
                desiredMoveSpeed = sprintSpeed;
        }

        // check if desiredMoveSpeed has changed drastically
        if(Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;

        Debug.Log("The current move state is:" + state);
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lep movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        // calculate movemenet direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            // Apply force along the slope direction
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            // Prevent unwanted upwards velocity while descending
            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
      
        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // Gravity should stay active when the slope angle is GREATER than the threshold
        float slopeAngle = OnSlope() ? Vector3.Angle(Vector3.up, slopeHit.normal) : 0f;
        rb.useGravity = !OnSlope() || slopeAngle >= gravityEnableAngle;

    }

    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // limit velcocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // Preserve existing momentum on jump
        Vector3 preservedVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Reset only Y velocity for consistent jump height
        rb.linearVelocity = new Vector3(preservedVelocity.x, jumpForce, preservedVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        // Debug.Log("Jumped");
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private bool OnSlope()
    {
        // Player Gizmo @ center
        /*
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, (float)(playerHeight * 0.5 + 0.3f))) // Detects slope
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }*/

        // Player Gizmo @ base
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 0.2f)) // Detects slope
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private void StartSlide()
    {
        sliding = true;

        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        if (!hasAppliedForce) // Downward force only applies on first crouchKey press and not continuous
        {
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse); // Moves the player towards the ground
            rb.AddForce(rb.linearVelocity, ForceMode.Impulse); // Preserve existing momentum

            hasAppliedForce = true;
            // Debug.Log("Downward Force Applied!");
        }

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // sliding normal
        if(!OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        // sliding down a slope
        else
        {
            rb.AddForce(GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        // Stop sliding when timer runs out or jump is pressed
        if (slideTimer <= 0 || Input.GetKeyDown(jumpKey))
            StopSlide();
    }

    private void StopSlide()
    {
        sliding = false;

        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }
}
