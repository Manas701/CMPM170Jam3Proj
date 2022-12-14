using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public enum State
    {
        IDLE,
        RUNNING,
        JUMPING,
        IDLEHOLDING,
        RUNHOLDING,
        JUMPHOLDING,
        BEINGHELD,
        BEINGTHROWN,
        STUCK
    }
    public State state;

    // constants used for setting the facing of the player
    // impacts the horizontal throw direction of the grabbed player
    private const float left = -1f;
    private const float right = 1f;
    private float facing = right;

    [Header("Sticking Variables")]
    [SerializeField] private float horizontalWallThrow;
    [SerializeField] private float verticalWallThrow;

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

    [Header("Grabbing and Throwing Variables")]
    [SerializeField] private float grabbingPlayerJumpMultiplier;
    [SerializeField] private float horizontalThrowMultiplier;
    [SerializeField] private float verticalThrowMultiplier;
    [SerializeField] private float grabCD;
    [SerializeField] private float controlsBackCD;
    private float grabCDTimer = 0;
    private float controlsBackCDTimer = 0;
    private float prevGrabButtonState = 0;
    private bool canGrab = false;
    private float grabInput;

    [Header("Components, Objects, and Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask onlyGroundLayer;
    [SerializeField] private BoxCollider2D hitbox;
    [SerializeField] private BoxCollider2D grabHitbox;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject otherPlayer;
    [SerializeField] private PlayerMovement otherPlayerScript;
    [SerializeField] private Animator anim;

    [Header("Sound")]
    [SerializeField] private AudioSource audioPlayer;
    [SerializeField] private AudioClip foostepSFX;
    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private AudioClip throwSFX;
    [SerializeField] private AudioClip landSFX;
    private bool wasInAir;

    [Header("Input")]
    public InputAction playerLeftRight;
    public InputAction playerJump;
    public InputAction playerGrab;

    private void Start()
    {
        wasInAir = !OnGround();
    }

    //Below two functions required for input system to work properly
    private void OnEnable()
    {
        playerLeftRight.Enable();
        playerJump.Enable();
        playerGrab.Enable();
    }

    private void OnDisable()
    {
        playerLeftRight.Disable();
        playerJump.Disable();
        playerGrab.Disable();
    }

    private void Update()
    {
        //reads left/right input
        horizontalInput = playerLeftRight.ReadValue<float>();

        // these conditionals update player facing and accounts for
        // the BEINGHELD state, so if grabbed player is spamming directions
        // their facing won't be updated as it relies on the grabbing player's facing
        if(horizontalInput == 1 && !(state == State.BEINGHELD)){
            facing = right;
        }
        else if(horizontalInput == -1 && !(state == State.BEINGHELD)){
            facing = left;
        }

        // grab input
        grabInput = playerGrab.ReadValue<float>();

        if(state != State.BEINGHELD){
            AdjustDrag();
            AdjustFallSpeed();
        }

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
        //Also used for playing player landing noise
        if (OnGround())
        {
            onGroundTimer = onGroundBuffer;
            if (wasInAir)
            {
                audioPlayer.PlayOneShot(landSFX);
            }
            wasInAir = false;
        }
        else
        {
            wasInAir = true;
            if (onGroundTimer >= 0)
            {
                onGroundTimer -= Time.deltaTime;
            }
        }

        // timers for resetting grab and giving controls back cooldowns
        if(grabCDTimer > 0){
            grabCDTimer -= Time.deltaTime;
        }

        if(controlsBackCDTimer > 0)
        {
            controlsBackCDTimer -= Time.deltaTime;
        }

        //chanings which way sprite is facing based on input (doesnt change when player is stuck to a wall or being thrown)
        //Also plays footstep audio when not being held
        if (state != State.BEINGTHROWN && state != State.STUCK && state != State.BEINGHELD)
        {
            if (horizontalInput < -0.01f)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (horizontalInput > 0.01f)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        //sets players facing direction based on other player when being held
        else if (state == State.BEINGHELD)
        {
            transform.localScale = new Vector3(-facing, 1, 1);
        }
    }

    private void FixedUpdate()
    {   
        if(this.name == "Player1"){
            //Debug.Log("STATE " + state);
        }
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
                    anim.SetTrigger("Walking");
                }
                // grabbing
                else if(grabInput != 0 && canGrab)
                {
                    state = State.IDLEHOLDING;
                    otherPlayerScript.state = State.BEINGHELD;
                    otherPlayerScript.anim.SetTrigger("HeldIdle");
                    // set other player's gravity to zero so that their 
                    // position can be updated without gravity affecting it
                    otherPlayer.GetComponent<Rigidbody2D>().gravityScale = 0f;
                    // since the grabbed player is floating above the grabbing player
                    // increase the jump power, otherwise the grabbing player has a really small
                    // jump while holding the other player
                    // (can be modified by setting the GrabbingPlayerJumpMultiplier in the editor)
                    jumpPower *= grabbingPlayerJumpMultiplier;
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
                    anim.SetTrigger("Idle");
                }
                else if(grabInput != 0 && canGrab)
                {
                    state = State.RUNHOLDING;
                    otherPlayerScript.state = State.BEINGHELD;
                    otherPlayerScript.anim.SetTrigger("HeldIdle");
                    otherPlayer.GetComponent<Rigidbody2D>().gravityScale = 0f;
                    jumpPower *= grabbingPlayerJumpMultiplier;
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
                        anim.SetTrigger("Idle");
                    }
                    else
                    {
                        state = State.RUNNING;
                        anim.SetTrigger("Walking");
                    }
                }
                break;

            case State.IDLEHOLDING:
                // set the player who is being grabbed to just float above the grabbing player's head
                // DO NOT DIVIDE BY MORE THAN 14 OR THE BOX SINKING BUG WILL GET YOU
                otherPlayer.transform.position = new Vector2(this.transform.position.x, this.transform.position.y + hitbox.size.y + (hitbox.size.y / 13.5f));
                // also update the other player's facing direction to match the grabbing player's facing direction
                // so if grabbing player faces right the grabbed player also faces right
                otherPlayerScript.facing = facing;
                if (jumpBufferTimer > 0 && onGroundTimer > 0)
                {
                    state = State.JUMPHOLDING;
                    jumpBufferTimer = 0;
                    Jump();
                    MovePlayer();
                }
                else if (horizontalInput != 0)
                {
                    state = State.RUNHOLDING;
                    MovePlayer();
                    anim.SetTrigger("HoldingWalk");
                    otherPlayerScript.anim.SetTrigger("HeldWalk");
                }
                else if(grabInput != 0 && prevGrabButtonState != grabInput)
                {
                    ThrowPlayer();
                }
                break;

            case State.RUNHOLDING:
                MovePlayer();
                otherPlayer.transform.position = new Vector2(this.transform.position.x, this.transform.position.y + hitbox.size.y + (hitbox.size.y / 14));
                otherPlayerScript.facing = facing;
                if (jumpBufferTimer > 0 && onGroundTimer > 0)
                {
                    state = State.JUMPHOLDING;
                    jumpBufferTimer = 0;
                    Jump();
                    MovePlayer();
                }
                else if (horizontalInput == 0)
                {
                    state = State.IDLEHOLDING;
                    anim.SetTrigger("HoldingIdle");
                    otherPlayerScript.anim.SetTrigger("HeldIdle");
                }
                else if(grabInput != 0 && prevGrabButtonState != grabInput)
                {
                    ThrowPlayer();
                }
                break;

            case State.JUMPHOLDING:
                MovePlayer();
                otherPlayer.transform.position = new Vector2(this.transform.position.x, this.transform.position.y + hitbox.size.y + (hitbox.size.y / 14));
                otherPlayerScript.facing = facing;
                if (grabInput != 0 && prevGrabButtonState != grabInput)
                {
                    ThrowPlayer();
                }
                else if (rb.velocity.y < 0.1f && OnGround())
                {
                    if (horizontalInput == 0)
                    {
                        state = State.IDLEHOLDING;
                        anim.SetTrigger("HoldingIdle");
                        otherPlayerScript.anim.SetTrigger("HeldIdle");
                    }
                    else
                    {
                        state = State.RUNHOLDING;
                        anim.SetTrigger("HoldingWalk");
                        otherPlayerScript.anim.SetTrigger("HeldWalk");
                    }
                }              
                break;

            case State.BEINGHELD:
                break;

            case State.BEINGTHROWN:
                // if they hit a wall, stick the player
                if (OnWallLeft() || OnWallRight())
                {
                    rb.velocity = Vector2.zero;
                    rb.gravityScale = 0f;
                    state = State.STUCK;
                    anim.SetTrigger("WallStuck");
                }
        
                // if they hit the ground, return player to idle state
                else if(controlsBackCDTimer <= 0 || OnlyOnGround())
                {
                    state = State.IDLE;
                    anim.SetTrigger("Idle");
                }
                break;
                
            case State.STUCK:
                // stick player until they press down
                //rb.isKinematic = true;
                rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

                // once player presses down, slingshot them upwards
                if (grabInput != 0)
                {
                    //rb.isKinematic = false;
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                    rb.gravityScale = 1f;
                    rb.AddForce(new Vector2(horizontalWallThrow * (facing * -1), verticalWallThrow) * acceleration, ForceMode2D.Impulse);
                    canGrab = false;
                    state = State.JUMPING;
                    anim.SetTrigger("Jumping");
                    audioPlayer.PlayOneShot(jumpSFX);
                }
                break;
        }
        // check if the state of the grab button on the last update, this ensures that 
        // if the player picks another player up and holds the grab button they aren't
        // immediately thrown, this way grab button has the be repressed in order to throw
        if(prevGrabButtonState != grabInput)
        {
            prevGrabButtonState = grabInput;
        }

        
        if (state == State.RUNNING || state == State.RUNHOLDING)
        {
            if (!audioPlayer.isPlaying)
            {
                audioPlayer.PlayOneShot(foostepSFX);
            }
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
        audioPlayer.PlayOneShot(jumpSFX);

        //For animation triggers when holding/being held
        if (state == State.JUMPHOLDING)
        {
            anim.SetTrigger("HoldingJump");
            otherPlayerScript.anim.SetTrigger("HeldJump");
        } 
        else
        {
            anim.SetTrigger("Jumping");
        }
    }

    private void AdjustFallSpeed()
    {
        //makes player fall faster that way jump is less floaty
        if(OnGround()){
            rb.gravityScale = 1f;
        }
        else{
            if (rb.velocity.y < -0.1f)
            {
                rb.gravityScale = fallMultiplier;
            }
            //allows play to do short jumps by increasing gravity when they let go of jump
            else if (rb.velocity.y > 0.1f && playerJump.ReadValue<float>() == 0)
            {
                rb.gravityScale = lowJumpMultiplier;
            }
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
        //setting up arguments for the boxcast
        List<RaycastHit2D> colliderArray = new List<RaycastHit2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer);

        //collects all of the groundLayer colliders below the player
        int numColliders = Physics2D.BoxCast(hitbox.bounds.center, hitbox.bounds.size, 0f, Vector2.down, filter, colliderArray, 0.05f);
        //returns true if one of the colliders isnt a trigger (stops OnGround returning true on the players grab hitbox)
        for (int i = 0; i < numColliders; i++)
        {
            if (colliderArray[i].collider.isTrigger == false)
            {
                return true;
            }
        }
        return false;
    }

    private bool OnlyOnGround()
    {
        //checks if there are any groundLayer colliders below the player
        RaycastHit2D raycastHitGround = Physics2D.BoxCast(hitbox.bounds.center, hitbox.bounds.size, 0, Vector2.down, 0.05f, onlyGroundLayer);
        return raycastHitGround.collider != null;
    }

    private void ThrowPlayer(){
        state = State.IDLE;
        anim.SetTrigger("Idle");
        grabCDTimer = grabCD;
        otherPlayerScript.state = State.BEINGTHROWN;
        otherPlayerScript.anim.SetTrigger("Thrown");
        otherPlayerScript.controlsBackCDTimer = controlsBackCD;
        canGrab = false;
        otherPlayer.GetComponent<Rigidbody2D>().gravityScale = 1f;
        otherPlayer.GetComponent<Rigidbody2D>().AddForce(new Vector2(horizontalThrowMultiplier * facing, verticalThrowMultiplier) * acceleration, ForceMode2D.Impulse);
        jumpPower /= grabbingPlayerJumpMultiplier;
        audioPlayer.PlayOneShot(throwSFX);
    }

    // trigger enter and exit functions used for detecting
    // if the other player is inside the grab hitbox
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "Player")
        {
            if((state != State.BEINGHELD || state != State.IDLEHOLDING || state != State.RUNHOLDING || state != State.JUMPHOLDING)
                      && grabCDTimer <= 0 && controlsBackCDTimer <= 0 && otherPlayerScript.state != State.STUCK)
            {
                canGrab = true;
            }
        }
        if(col.gameObject.tag == "Platform" && state == State.BEINGTHROWN){
            this.gameObject.transform.parent = col.transform;
        }
        if(col.gameObject.tag == "Box" && state == State.BEINGTHROWN){
            state = State.IDLE;
            anim.SetTrigger("Idle");
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if(col.gameObject.tag == "Player")
        {
            canGrab = false;
        }
        if(col.gameObject.tag == "Platform" && state == State.JUMPING){
            this.gameObject.transform.parent = null;
        }
    }

    // checks if player hit wall on the right
    private bool OnWallRight()
    {
        RaycastHit2D raycastHitWall = Physics2D.BoxCast(hitbox.bounds.center, hitbox.bounds.size, 0, Vector2.right, 0.08f, groundLayer);
        return raycastHitWall.collider != null;
    }

    // checks if player hit wall on the left
    private bool OnWallLeft()
    {
        RaycastHit2D raycastHitWall = Physics2D.BoxCast(hitbox.bounds.center, hitbox.bounds.size, 0, Vector2.left, 0.08f, groundLayer);
        return raycastHitWall.collider != null;
    }
}
