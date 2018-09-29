using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private GameObject slashHitBox;
    private PlayerAnimations animations;
    public ProjectileController[] projectiles;
    private playerCharge pCharge;

    #region Conditions
    private bool isGrounded;
    private bool isJumping;
    private bool touchingLeftWall;
    private bool touchingRightWall;
    #endregion

    private Vector3 fullHopPoint;
    #region Constants
    private const float MAXSPEEDX = 10f;
    private float ACCELERATIONX = 50f;
    [SerializeField]
    private float rayCastDownDist = 2.0f;
    private const float jumpForce = 500f;
    private const float fastFallForce = -50.0f;
    private const float jumpForceHeld = 500.0f;
    private const float fullHopJumpHeight = 1.2f;
    #endregion

    void Start()
    {
        animations = GetComponent<PlayerAnimations>();
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        pCharge = GetComponent<playerCharge>();
    }

    void Update()
    {
        if ( !isGrounded ) {
            ACCELERATIONX = 20f;
        } else if ( isGrounded ){
            ACCELERATIONX = 50f;
        }
        int layerMask = 1 << 2;
        layerMask = ~layerMask;
        CheckGrounded( layerMask );
        CheckWalled( layerMask );
    }

    private void CheckGrounded( int layerMask )
    {
        //int layerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
        //layerMask = ~layerMask;
        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * rayCastDownDist, Color.red);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3 (-0.15f,-1,0)) * rayCastDownDist, Color.red);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3 (0.15f,-1,0)) * rayCastDownDist, Color.red);
        Collider2D downCast = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector3.down), rayCastDownDist, layerMask).collider;
        Collider2D downleftCast = Physics2D.Raycast(transform.position, transform.TransformDirection(new Vector3 (-0.15f,-1,0)), rayCastDownDist, layerMask).collider;
        Collider2D downrightCast = Physics2D.Raycast(transform.position, transform.TransformDirection(new Vector3 (0.15f,-1,0)), rayCastDownDist, layerMask).collider; 
        if (downCast == null && downleftCast == null && downrightCast == null)
        {
            isGrounded = false;
        }
        else
        {
            isGrounded = true;
        }
    }

    private void CheckWalled( int layerMask ) {
        Vector3 leftRay = new Vector3(-1, 0, 0);
        Vector3 rightRay = new Vector3(1, 0, 0);

        Debug.DrawRay(transform.position, transform.TransformDirection(leftRay), Color.red);
        Debug.DrawRay(transform.position, transform.TransformDirection(rightRay) * rayCastDownDist, Color.green);
        
        Collider2D raycastColliderLeft = Physics2D.Raycast(transform.position, transform.TransformDirection(leftRay) * rayCastDownDist, rayCastDownDist, layerMask).collider;
        Collider2D raycastColliderRight = Physics2D.Raycast(transform.position, transform.TransformDirection(rightRay) * rayCastDownDist, rayCastDownDist, layerMask).collider;
        if (raycastColliderLeft != null)
        {
            touchingLeftWall = true;
            touchingRightWall = false;
        }
        else if ( raycastColliderRight != null ){
            touchingRightWall = true;
            touchingLeftWall = false;
        } else {
            touchingLeftWall = false;
            touchingRightWall = false;
        }
    }

    private Vector2 getForward() {
        return (spriteRenderer.flipX ? -1 : 1) * new Vector2(1,0);
    }

    public void Move(float speed)
    {
        if (Math.Abs(GetSpeedX()) > MAXSPEEDX)
        {
            if( isGrounded )
                rigidBody.velocity = new Vector2(MAXSPEEDX * Math.Sign(rigidBody.velocity.x), rigidBody.velocity.y);
        }
        else
        {
            rigidBody.AddForce(new Vector2(speed * ACCELERATIONX, 0));
        }

    }

    public void Stop()
    {
        rigidBody.velocity = new Vector2(0, rigidBody.velocity.y);
    }

    
    public void JumpPressed()
    {
        if( !isGrounded && touchingLeftWall )
        {
            //left wall jump
            wallJump("left");
        } else if( !isGrounded && touchingRightWall ) 
        {
            //right wall jump
            wallJump("right");
        }else if ( isJumping )
        {
            ContinueJump();
        } else if ( isGrounded )
        {
            Jump();
        }
    }
    public void Jump()
    {
        fullHopPoint = new Vector3(transform.position.x, transform.position.y + fullHopJumpHeight, transform.position.z);
        rigidBody.AddForce(new Vector2(0, jumpForce));
        isJumping = true;
    }
    public void ContinueJump()
    {
        rigidBody.AddForce(new Vector2(0, jumpForceHeld));
        isJumping = false;
    }
    public void wallJump( string dir )
    {
        isJumping = true;
        touchingLeftWall = false;
        touchingRightWall = false;
        if ( dir == "right" ) {
            rigidBody.AddForce(new Vector2(-600.0f, 650.0f));
        } else if ( dir == "left" ) {
            rigidBody.AddForce(new Vector2(600.0f, 650.0f));
        }
    }
    public void FastFall()
    {
        if (!isGrounded)
        {
            rigidBody.AddForce(new Vector2(0, fastFallForce));
        }
    }
    public void SlashStart()
    {
        slashHitBox.SetActive(true);
        //animations.SlashAnim();
    }
    public void SlashEnd()
    {
        slashHitBox.SetActive(false);
    }
    public bool IsGrounded()
    {
        return isGrounded;
    }
    public float GetSpeedX()
    {
        return rigidBody.velocity.x;
    }

    public void Shoot(){
        if (pCharge.charge < 5) return;     
        int p = (int) pCharge.charge / 20;

        ProjectileController projectile = Instantiate (
            projectiles[0],
            transform.position + new Vector3(0,0,-3),
            transform.rotation
        );
        projectile.Shoot(getForward());
        pCharge.charge = 0;
        Destroy(projectile.gameObject, 3);
    }

    public void FlipSlashHitBox()
    {
        // Uncomment for slash hitbox
        /*
        slashHitBox.transform.localPosition = new Vector2(-slashHitBox.transform.localPosition.x, slashHitBox.transform.localPosition.y);
        slashHitBox.transform.localScale = new Vector2(-slashHitBox.transform.localScale.x, slashHitBox.transform.localScale.y);
        */
    }

}
