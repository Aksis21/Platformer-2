using Unity.VisualScripting;
using UnityEngine;

public enum FacingDirection
{
    left, right
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;

    [Header("Horizontal")]
    public float decelerationTime = 0.15f;
    public float accelerationTime = 0.25f;

    //Walk/Sprint variables created to store the max walk and sprint speed.
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    float maxSpeed;

    [Header("Vertical")]
    public float apexHeight = 3f;
    public float apexTime = 0.5f;
    public float terminalSpeed = 10f;

    [Header("Ground Checking")]
    public float groundCheckOffset = 0.5f;
    public Vector2 groundCheckSize = new(0.4f, 0.1f);
    public LayerMask groundCheckMask;

    [Header("Wall Checking")]
    public float wallCheckOffset;
    public Vector2 wallCheckSize = new(0.1f, 0.5f);
    public float leftRightOffset = 0.5f;
    public Vector2 leftRightCheckSize;
    public LayerMask wallCheckMask;

    float accelerationRate;
    float decelerationRate;
    float gravity;
    float initialJumpSpeed;

    bool isGrounded = false;
    bool isWalled = false;
    bool isWalledLeft = false;
    bool isWalledRight = false;

    FacingDirection currentDirection = FacingDirection.right;
    Vector2 velocity;

    public void Start()
    {
        //By default, max speed is set to walk speed.
        maxSpeed = walkSpeed;

        body.gravityScale = 0;

        accelerationRate = maxSpeed / accelerationTime;
        decelerationRate = maxSpeed / decelerationTime;

        gravity = -2 * apexHeight / (apexTime * apexTime);
        initialJumpSpeed = 2 * apexHeight / apexTime;
    }

    public void Update()
    {
        CheckForGround();
        CheckForWall();

        Vector2 playerInput = new Vector2();
        playerInput.x = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.LeftShift))
            maxSpeed = sprintSpeed;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            maxSpeed = walkSpeed;

        MovementUpdate(playerInput);
        JumpUpdate();

        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
            if (velocity.y < -terminalSpeed)
                velocity.y = -terminalSpeed;
        }
        else
            velocity.y = 0;

        body.velocity = velocity;
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        if (playerInput.x > 0)
        {
            currentDirection = FacingDirection.right;
        }
        if (playerInput.x < 0)
        {
            currentDirection = FacingDirection.left;
        }

        if (playerInput.x != 0)
        {
            velocity.x += accelerationRate * playerInput.x * Time.deltaTime;
            velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);
        }
        else
        {
            if (velocity.x > 0)
            {
                velocity.x -= decelerationRate * Time.deltaTime;
                velocity.x = Mathf.Max(velocity.x, 0);
            }
            else if (velocity.x < 0)
            {
                velocity.x += decelerationRate * Time.deltaTime;
                velocity.x = Mathf.Min(velocity.x, 0);
            }
        }
    }

    private void JumpUpdate()
    {
        if (isGrounded && Input.GetButton("Jump"))
        {
            velocity.y = initialJumpSpeed;
            isGrounded = false;
        }

        if (isWalled && Input.GetButton("Jump") && !isGrounded)
        {
            velocity.y = initialJumpSpeed;
            if(isWalledLeft)
            {
                velocity.x = maxSpeed;
            }
            if(isWalledRight)
            {
                velocity.x = -maxSpeed;
            }
        }
    }

    private void CheckForGround()
    {
        isGrounded = Physics2D.OverlapBox(transform.position + Vector3.down * groundCheckOffset, groundCheckSize, 0, groundCheckMask);
    }

    private void CheckForWall()
    {
        isWalled = Physics2D.OverlapBox(transform.position + Vector3.down * wallCheckOffset, wallCheckSize, 0, wallCheckMask);
        isWalledLeft = Physics2D.OverlapBox(transform.position + Vector3.left * leftRightOffset, leftRightCheckSize, 0, wallCheckMask);
        isWalledRight = Physics2D.OverlapBox(transform.position + Vector3.right * wallCheckOffset, leftRightCheckSize, 0, wallCheckMask);
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + Vector3.down * groundCheckOffset, groundCheckSize);
        Gizmos.DrawWireCube(transform.position + Vector3.down * wallCheckOffset, wallCheckSize);
        Gizmos.DrawWireCube(transform.position + Vector3.left * leftRightOffset, leftRightCheckSize);
        Gizmos.DrawWireCube(transform.position + Vector3.right * leftRightOffset, leftRightCheckSize);
    }

    public bool IsWalking()
    {
        return velocity.x != 0;
    }
    public bool IsGrounded()
    {
        return isGrounded;
    }

    public FacingDirection GetFacingDirection()
    {
        return currentDirection;
    }
}
