using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    enum State
    {
        IDLE,
        RUNNING,
        JUMPING,
    }
    private State state;

    [Header("Movement Variables")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float groundDrag;
    private float horizontalInput;
    private bool changingDirection;

    [Header("Jump Variables")]
    [SerializeField] private float jumpPower;
    [SerializeField] private float airDrag;
    [SerializeField] private float fallMultiplier;
    [SerializeField] private float lowJumpMultiplier;
    [SerializeField] private float jumpBuffer;
    [SerializeField] private float onGroundBuffer;
    private float jumpBufferTimer;
    private float onGroundTimer;

    [Header("Components, Objects, and Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private BoxCollider2D hitbox;
    [SerializeField] private Rigidbody2D rb;

    [Header("Input")]
    public InputAction playerLeftRight;
    public InputAction playerJump;

    //Below two functions required for input system to work properly
    private void OnEnable()
    {
        playerLeftRight.Enable();
        playerJump.Enable();
    }

    private void OnDisable()
    {
        playerLeftRight.Disable();
        playerJump.Disable();
    }

    private void Update()
    {
        horizontalInput = playerLeftRight.ReadValue<float>();

        //Updates drag and fall speed
        AdjustDrag();
        AdjustFallSpeed();

        //jump buffer (stores jump for short amount of time so that player can press jump right before they hit the ground and still jump)
        if (playerJump.WasPressedThisFrame())
        {
            jumpBufferTimer = jumpBuffer;
        }
        else
        {
            if (jumpBufferTimer >= 0)
            {
                jumpBufferTimer -= Time.deltaTime;
            }
        }

        //Stores a buffer of time whenever player is on ground. Jump checks use this timer instead of OnGround() to allow the player
        //to jump for a short amount of time after leaving a platform (coyote time)
        if (OnGround())
        {
            onGroundTimer = onGroundBuffer;
        }
        else
        {
            if (onGroundTimer >= 0)
            {
                onGroundTimer -= Time.deltaTime;
            }
        }
    }

    private void FixedUpdate()
    {   
        //Player states
        switch (state)
        {
            case State.IDLE:
                //Checks if player is starting to jump
                if (jumpBufferTimer > 0 && onGroundTimer > 0)
                {
                    state = State.JUMPING;
                    jumpBufferTimer = 0;
                    Jump();
                    MovePlayer();
                }
                //Checks if player has started running
                else if (horizontalInput != 0)
                {
                    MovePlayer();
                    state = State.RUNNING;
                }
                break;

            case State.RUNNING:
                MovePlayer();
                //Checks if player is starting to jump
                if (jumpBufferTimer > 0 && onGroundTimer > 0)
                {
                    state = State.JUMPING;
                    jumpBufferTimer = 0;
                    Jump();
                }
                //Checks if player has stopped moving
                else if (horizontalInput == 0)
                {
                    state = State.IDLE;
                }
                break;

            case State.JUMPING:
                MovePlayer();
                //Checks if player is back on the ground
                if (OnGround())
                {
                    //Transitions to either idle or running depending on if player is moving
                    if (horizontalInput == 0)
                    {
                        state = State.IDLE;
                    }
                    else
                    {
                        state = State.RUNNING;
                    }
                }
                break;
        }
    }

    private void MovePlayer()
    {
        //adds force to the player until they have reached a max speed
        rb.AddForce(new Vector2(horizontalInput, 0f) * acceleration);

        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
        {
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
        }
    }

    private void Jump()
    {
        //Applies upward force to the player
        rb.drag = airDrag;
        rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
    }

    private void AdjustFallSpeed()
    {
        //makes player fall faster that way jump is less floaty
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = fallMultiplier;
        }
        //allows play to do short jumps by increasing gravity when they let go of jump
        else if (rb.velocity.y > 0 && playerJump.ReadValue<float>() == 0)
        {
            rb.gravityScale = lowJumpMultiplier;
        }
        //resets player gravity when not jumping
        else
        {
            rb.gravityScale = 1f;
        }
    }

    private void AdjustDrag()
    {
        //applies ground drag
        if (OnGround())
        {
            //checks if the player is currently trying to change directions by comparing player velocity to current input
            if ((rb.velocity.x > 0f && horizontalInput < 0f) || (rb.velocity.x < 0f && horizontalInput > 0f))
            {
                changingDirection = true;
            }
            else
            {
                changingDirection = false;
            }

            //applies drag when the player is trying to stop or change directions (stops the icy feel)
            if (Mathf.Abs(horizontalInput) < 0.8f || changingDirection)
            {
                rb.drag = groundDrag;
            }
            else
            {
                rb.drag = 0f;
            }
        }
        //applies air drag
        else
        {
            rb.drag = airDrag;
        }
    }

    private bool OnGround()
    {
        //checks if there are any groundLayer colliders below the player
        RaycastHit2D raycastHitGround = Physics2D.BoxCast(hitbox.bounds.center, hitbox.bounds.size, 0, Vector2.down, 0.05f, groundLayer);
        return raycastHitGround.collider != null;
    }
}
