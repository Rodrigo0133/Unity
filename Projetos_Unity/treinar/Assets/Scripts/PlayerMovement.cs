using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameObject attackCollider;

    [Header("Movimento")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower = 8f;
    [SerializeField] private float wallSlideSpeed = 1f;
    [SerializeField] private float gravityScale = 1.5f;
    [SerializeField] private float wallJumpForce = 6f;
    [SerializeField] private float SprintValor = 11f;
    public KeyCode Sprint = KeyCode.LeftShift;

    [Header("Vida")]
    public int maxLife;
    public float currentLife;

    [Header("Invencibilidade")]
    [SerializeField] private float invincibleTime = 1f;
    private bool isInvincible = false;

    [Header("Portal")]
    [SerializeField] private string Nomedoproximolevel;
    [SerializeField] private LayerMask Portal;

    [Header("Colis�es")]
    private Rigidbody2D body;
    private BoxCollider2D boxCollider;
    [SerializeField] private LayerMask groundlayer;
    [SerializeField] private LayerMask groundlayer2;
    [SerializeField] private LayerMask wallLayer;

    [SerializeField] private Animator animator;

    private float horizontalInput;
    private float wallJumpCooldown;
    private Vector3 respawnPosition = new Vector3(-0.15f, -3.77f, 0f);

    private PlayerLives playerLives;


    public GameObject attackHitbox;
    public float attackDuration = 0.3f;
    public int damage = 25;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        body.gravityScale = gravityScale;

        currentLife = maxLife;
        transform.position = respawnPosition;

        playerLives = GetComponentInParent<PlayerLives>();

    }
        

    [System.Obsolete]
    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        if (horizontalInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);
        animator.SetBool("Grounded", isGrounded() || isGrounded2());
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));



        if (Input.GetKey(Sprint))
        {
            SprintSpeed();
        }
        else
        {
            speed = 3f;
        }

        if (wallJumpCooldown > 0.2f)
        {
            body.linearVelocity = new Vector2(horizontalInput * speed, body.linearVelocity.y);
        }
        else
        {
            wallJumpCooldown += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
           
        }
        else {             
          
        }
        WallSlide();
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("Attack");
            StartCoroutine(Attack());

        }
        if (inPortal())
        {
            Debug.Log("Entrou no portal");
            SceneManager.LoadScene(Nomedoproximolevel);
        }

     
    }

    [System.Obsolete]
    private void Jump()
    {
        if (isGrounded() || isGrounded2())
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);
        }
        else if ((onWall() && !isGrounded()) || (onWall() && !isGrounded2()))
        {
            body.linearVelocity = new Vector2(-Mathf.Sign(transform.localScale.x) * wallJumpForce, jumpPower);
            wallJumpCooldown = 0;
        }
        
   
    }

    [System.Obsolete]
    private void WallSlide()
    {
        if (onWall() && !isGrounded() && horizontalInput != 0)
        {
            body.linearVelocity = new Vector2(
                body.linearVelocity.x,
                Mathf.Clamp(body.linearVelocity.y, -wallSlideSpeed, float.MaxValue)
            );
        }

      
    }

    public void SprintSpeed()
    {
        speed = SprintValor;
    }

    private bool isGrounded()
    {
        return Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
            0f,
            Vector2.down,
            0.1f,
            groundlayer
        );
    }
    private bool isGrounded2()
    {
        return Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
            0f,
            Vector2.down,
            0.1f,
            groundlayer2
        );
    }

    private bool onWall() => Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
            0f,
            new Vector2(transform.localScale.x, 0),
            0.1f,
            wallLayer);

    [System.Obsolete]
    public void TakeDamage(float damage)
    {
        currentLife -= damage;
        if (currentLife <= 0)
        {
            Respawn();
        }
        else
        {
           
        }
    }

    [System.Obsolete]
    private void Respawn()
    {
        Debug.Log("Jogador morreu! Voltando para respawn...");
        currentLife = maxLife;
        body.linearVelocity = Vector2.zero;
        transform.position = respawnPosition;
        StartCoroutine(InvincibilityCoroutine());

      
    }

    private System.Collections.IEnumerator InvincibilityCoroutine()
    {   
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }
    public void AttackReset()
    {
        animator.ResetTrigger("Attack");
    }
    private bool inPortal()
    {
        Collider2D hit = Physics2D.OverlapBox(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Portal);
        return hit != null;
    }

    public void ActivateAttackCollider()
    {
        attackCollider.SetActive(true);
    }

    public void DeactivateAttackCollider()
    {
        attackCollider.SetActive(false);
    }
  
    public GameObject attackRange; 

   

    IEnumerator Attack()
    {
        attackRange.SetActive(true);   
        yield return new WaitForSeconds(0.2f); 
        attackRange.SetActive(false);  
    }
}
