using System.Collections;
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

    [Header("Dash")]
    public float dashChargeTime;
    float dashCharge = 0f;
    float chargeTime = 0f;

    float accelerationRate;
    float decelerationRate;
    float gravity;
    float initialJumpSpeed;

    bool isGrounded = false;
    bool isWalled = false;
    bool isWalledLeft = false;
    bool isWalledRight = false;
    bool canWallJump = true;
    //public bool canJump = true;

    //public float jumpCooldown;
    //float jumpTimer;

    //This float exists for the third mechanic, as the player cannot move while charging up their dash.
    bool canMove = true;
    bool dashCharging = false;
    bool canDash = true;

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

        dash();
        MovementUpdate(playerInput);
        JumpUpdate();

        if (!isGrounded && canMove)
        {
            velocity.y += gravity * Time.deltaTime;
            if (velocity.y < -terminalSpeed)
                velocity.y = -terminalSpeed;
        }
        else
            velocity.y = 0;

        body.velocity = velocity;
    }

    private void dash()
    {
        if (Input.GetKeyDown(KeyCode.S) && canMove && canDash)
        {
            canMove = false;
            dashCharging = true;
        }
        if (Input.GetKeyUp(KeyCode.S) || chargeTime > dashChargeTime)
        {
            dashCharging = false;
            chargeTime = 0f;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 target = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y);
            body.AddForce(target.normalized * dashCharge);
            dashCharge = 0f;
            canMove = true;
            canWallJump = true;
            canDash = false;
        }

        if (dashCharging)
        {
            dashCharge += 30;
            chargeTime += Time.deltaTime;
        }

        if (!canDash && isGrounded)
            canDash = true;
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

        if (playerInput.x != 0 && canMove)
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
        if (isGrounded && Input.GetButton("Jump") /*&& canJump*/)
        {
            velocity.y = initialJumpSpeed;
            isGrounded = false;
            canWallJump = true;
            //canJump = false;
        }

        if (isWalled && Input.GetButton("Jump") && !isGrounded && canWallJump /*&& canJump*/)
        {
            velocity.y = initialJumpSpeed;
            canWallJump = false;
            if (isWalledRight)
            {
                velocity.x = walkSpeed * -1;
            }
            if (isWalledLeft)
            {
                velocity.x = walkSpeed;
            }
            //canJump = false;
        }

        /*if (!canJump)
            jumpTimer -= Time.deltaTime;
        if (jumpTimer <= 0)
        {
            canJump = true;
            jumpTimer = jumpCooldown;
        }*/
    }

    private void CheckForGround()
    {
        isGrounded = Physics2D.OverlapBox(transform.position + Vector3.down * groundCheckOffset, groundCheckSize, 0, groundCheckMask);
    }

    private void CheckForWall()
    {
        isWalled = Physics2D.OverlapBox(transform.position + Vector3.down * wallCheckOffset, wallCheckSize, 0, wallCheckMask);
        isWalledLeft = Physics2D.OverlapBox(transform.position + Vector3.left * leftRightOffset, leftRightCheckSize, 0, wallCheckMask);
        isWalledRight = Physics2D.OverlapBox(transform.position + Vector3.right * leftRightOffset, leftRightCheckSize, 0, wallCheckMask);
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
