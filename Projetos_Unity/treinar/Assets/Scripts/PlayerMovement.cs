using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower = 8f;
    [SerializeField] private float wallSlideSpeed = 1f;
    [SerializeField] private float gravityScale = 1.5f;
    [SerializeField] private float wallJumpForce = 6f;
    [SerializeField] private float SprintValor = 11f;
    public KeyCode Sprint = KeyCode.LeftShift;

    [Header("Vida")]
    [SerializeField] private int maxLife = 3;
    private float currentLife;

    [Header("Invencibilidade")]
    [SerializeField] private float invincibleTime = 1f;
    private bool isInvincible = false;

    private Rigidbody2D body;
    private BoxCollider2D boxCollider;

    [SerializeField] private LayerMask groundlayer;
    [SerializeField] private LayerMask groundlayer2;
    [SerializeField] private LayerMask wallLayer;

    private float horizontalInput;
    private float wallJumpCooldown;

    // Posição de respawn
    private Vector3 respawnPosition = new Vector3(-0.15f, -3.77f, 0f);

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        body.gravityScale = gravityScale;

        currentLife = maxLife;
        transform.position = respawnPosition;
    }

    [System.Obsolete]
    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        if (horizontalInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);
        if(Input.GetKey(Sprint))
        {
            SprintSpeed();
        }
        else
        {
            speed = 3f;
        }

        if (wallJumpCooldown > 0.2f)
        {
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
        }
        else
        {
            wallJumpCooldown += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space))
            Jump();

        WallSlide();
    }

    [System.Obsolete]
    private void Jump()
    {
        if (isGrounded() || isGrounded2())      
        {
            body.velocity = new Vector2(body.velocity.x, jumpPower);
        }
        else if ((onWall() && !isGrounded()) || (onWall() && !isGrounded2()))
        {
            body.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * wallJumpForce, jumpPower);
            wallJumpCooldown = 0;
        }
    }

    [System.Obsolete]
    private void WallSlide()
    {
        if (onWall() && !isGrounded() && horizontalInput != 0)
        {
            body.velocity = new Vector2(
                body.velocity.x,
                Mathf.Clamp(body.velocity.y, -wallSlideSpeed, float.MaxValue)
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
        if (isInvincible) return; // ignora dano se estiver invencível

        currentLife -= damage;
        if (currentLife <= 0)
        {
            Respawn();
        }
        else
        {
            Debug.Log($"Jogador recebeu {damage} de dano. Vida atual: {currentLife}");
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    [System.Obsolete]
    private void Respawn()
    {
        Debug.Log("Jogador morreu! Voltando para respawn...");
        currentLife = maxLife;
        body.velocity = Vector2.zero;
        transform.position = respawnPosition;
        StartCoroutine(InvincibilityCoroutine()); // invencível um pouco após respawn
    }

    private System.Collections.IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }
}
