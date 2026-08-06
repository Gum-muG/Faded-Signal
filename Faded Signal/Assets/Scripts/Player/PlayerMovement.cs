using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] float moveSpeedMultiplier;
    [SerializeField] float groundDrag;
    [SerializeField] float jumpHeight;
    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;
    [SerializeField] float airborneControlMultiplier;
    [SerializeField] float gravityMultiplier;
    [SerializeField] float maxFallSpeed;
    [SerializeField] bool playerHasDoubleJump;

    float currentMoveSpeed;
    float jumpSpeed;
    float jumpGravity;
    bool readyToJump;
    bool isSprinting;
    bool sprintJumpActive;
    Vector3 moveDirection;
    Vector3 currentVelocity;
    Vector3 maxVelocity;
    float currentFallSpeed;



    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey;
    [SerializeField] KeyCode sprintKey;


    [Header("Ground Check")]
    [SerializeField] float playerHeight;
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] float groundRaycastWiggleRoom;
    bool grounded;

    [SerializeField] Transform playerOrientation;

    float horizontalInput;
    float verticalInput;


    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        ResetJump();
    }

    void Update()
    {
        //ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, (playerHeight/2) + groundRaycastWiggleRoom, whatIsGround);

        PlayerInput();
        UpdateMoveSpeed();

        //handle drag
        if (grounded)
        {
            //rb.linearDamping is just rb.drag, rb.drag is legacy feature though and is outdated
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }
    }

    void FixedUpdate()
    {
        MovePlayer();
        //when jumping
        ApplyJumpGravity();
        //when falling
        ApplyFallGravity();
        SpeedControl();
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //sprinting
        isSprinting = Input.GetKey(sprintKey) && grounded;

        //jumping
        if(Input.GetKey(jumpKey) && readyToJump)
        {
            if(!playerHasDoubleJump && !grounded)
            {
                return;
            }

            readyToJump = false;

            Jump();

            //allows player to continuously jump while holding down jump key
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void UpdateMoveSpeed()
    {
        if (grounded)
        {
            currentMoveSpeed = isSprinting ? sprintSpeed : walkSpeed;
        }
        else
        {
            currentMoveSpeed = sprintJumpActive ? sprintSpeed : walkSpeed;
        }
    }

    private void MovePlayer()
    {
        //calcuate movement dir
        moveDirection = (playerOrientation.forward * verticalInput) + playerOrientation.right * horizontalInput;
        moveDirection = moveDirection.normalized;
        
        //on ground
        if (grounded)
        {
            rb.AddForce(moveDirection * currentMoveSpeed * moveSpeedMultiplier, ForceMode.Force);
        }

        //in air
        else if (!grounded)
        {
            rb.AddForce(moveDirection * currentMoveSpeed * moveSpeedMultiplier * airborneControlMultiplier, ForceMode.Force);
        }

    }

    private void SpeedControl()
    {
        //horizontal speed cap

        currentVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        //limits max velocity to moveSpeed
        if(currentVelocity.magnitude > currentMoveSpeed)
        {
            //note for me, normalizing a vector keeps it's direction, but shifts its magnitude to 1. this is resetting currentVelocity magnitude to = moveSpeed, but keeping the direction.
            maxVelocity = currentVelocity.normalized * currentMoveSpeed;
            rb.linearVelocity = new Vector3(maxVelocity.x, rb.linearVelocity.y, maxVelocity.z);
        }

        //fall speed cap(terminal velocity)

        currentFallSpeed = rb.linearVelocity.y;
        if(currentFallSpeed < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -maxFallSpeed, rb.linearVelocity.z);
        }
    }

    //the calculation for jump height is (upwardVelocity^2 /(gravityMagnitude * 2)). I need custom gravity based on how high I want the player to jump(because I also want to control jump speed to make the player less floaty)
    private void CalculateJumpGravity()
    {   
        //the calculation for upwards velocity is the impulse force/divided by the mass of the rigidbody
        jumpSpeed = jumpForce/rb.mass;
        //the total downward acceleration needed to reach jumpHeight
        jumpGravity = -(jumpSpeed * jumpSpeed) / (jumpHeight * 2f);
    }

    private void ApplyJumpGravity()
    {
        if(!grounded && rb.linearVelocity.y > 0f)
        {
            float extraJumpGravity = jumpGravity - Physics.gravity.y;

            rb.AddForce(transform.up * extraJumpGravity, ForceMode.Acceleration);
        }
    }

    private void Jump()
    {
        sprintJumpActive = isSprinting;
        CalculateJumpGravity();
        //reset vertical(y) velocity to 0(ensures you always jump the same height)
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void ApplyFallGravity()
    {
        if(!grounded && rb.linearVelocity.y < 0f)
        {   
            //the "-1" is because this is adding to the force of gravity(AddForce), if we want to multiply gravity by 3, we should AddForce of gravity*2, because gravity is already applied once by unity.
            rb.AddForce(Physics.gravity * (gravityMultiplier-1f), ForceMode.Acceleration);
        }
    }

}
