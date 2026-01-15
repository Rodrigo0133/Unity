using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower = 8f; 
    [SerializeField] private float wallSlideSpeed = 0.5f;
    [SerializeField] private float gravityScale = 1.5f;
    private Rigidbody2D body;
    private Animator anim;
    private bool grounded;
    private BoxCollider2D boxCollider;
    [SerializeField] private LayerMask groundlayer;
    [SerializeField] private LayerMask wallLayer;
    private float wallJumpCooldown;
    private float horizontalInput;
    private bool isWallSliding;
    [SerializeField] private Transform wallcheck;
    [SerializeField] private Rigidbody2D rb;    

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        rb = body; 
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        // garante um gravityScale inicial controlável
        if (body != null)
            body.gravityScale = gravityScale;
    }

    // Update is called once per frame
    [System.Obsolete]
    private void Update()
    {
         horizontalInput = Input.GetAxis("Horizontal");
      
        if (horizontalInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);
       
        anim.SetBool("Correr", horizontalInput != 0);
        anim.SetBool("Grounded", isGrounded()); 

       if(wallJumpCooldown > 0.2f)
        {
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
            if(onWall() && !isGrounded())
            {
                body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

                if (Input.GetKeyDown(KeyCode.Space))
                    Jump();

                if (wallJumpCooldown < 1f) 
                    wallJumpCooldown += Time.deltaTime;

                WallSlide();
            }
            else
            {
                
                body.gravityScale = gravityScale;
                if(Input.GetKeyDown(KeyCode.Space))
                    Jump();
                
                if (wallJumpCooldown < 1f) 
                    wallJumpCooldown += Time.deltaTime;

                WallSlide();
            }
        }
        else
        {
            wallJumpCooldown += Time.deltaTime;
        }
    }

    [System.Obsolete]
    private void Jump()
    {
        if (isGrounded())
        {
            body.velocity = new Vector2(body.velocity.x, jumpPower);
            anim.SetTrigger("jump");
        } else if (onWall() && !isGrounded())
        {
            if(horizontalInput == 0)
            {
                body.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * 10, 0);
                transform.localScale = new Vector3(-Mathf.Sign(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                body.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * 3, 6);
            }
         wallJumpCooldown = 0;
            
        }
    }

  /*  private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            grounded = true;
        }
    }
  */
    private bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundlayer);
        return raycastHit.collider != null;
    }
    private bool onWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center , boxCollider.bounds.size,0,new Vector2(transform.localScale.x,0),0.1f,wallLayer);
        return raycastHit.collider != null;
    }

    [System.Obsolete]

    private void WallSlide()
    {
        if (onWall() && !isGrounded() && horizontalInput != 0f)
        {
           isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }




}

