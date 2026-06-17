using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameObject attackCollider;

    [Header("Movimento")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float jumpPower = 8f;
    [SerializeField] private float wallSlideSpeed = 1f;
    [SerializeField] private float gravityScale = 1.5f;
    [SerializeField] private float wallJumpForce = 6f;
    [SerializeField] private float SprintValor = 11f;
    public KeyCode Sprint = KeyCode.LeftShift;

    [Header("Vida")]
    public int maxLife = 3;
    public float currentLife;

    [Header("Invencibilidade")]
    [SerializeField] private float invincibleTime = 1f;
    private bool isInvincible = false;

    [Header("Portal")]
    [SerializeField] private string Nomedoproximolevel;
    [SerializeField] private LayerMask Portal;

    [Header("Colisões")]
    private Rigidbody2D body;
    private BoxCollider2D boxCollider;
    [SerializeField] private LayerMask groundlayer;
    [SerializeField] private LayerMask groundlayer2;
    [SerializeField] private LayerMask wallLayer;

    [SerializeField] private Animator animator;

    private float horizontalInput;
    private float wallJumpCooldown;
    private Vector3 respawnPosition;
    private bool isDead = false;

    public GameObject attackRange;
    public float attackCooldown = 0.6f;
    private float nextAttackTime = 0f;
    public int damage = 25;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        body.gravityScale = gravityScale;

        // Zero Friction + Edge Radius
        PhysicsMaterial2D mat = new PhysicsMaterial2D("ZeroFriction");
        mat.friction = 0f;
        mat.bounciness = 0f;

        if (boxCollider != null)
        {
            boxCollider.sharedMaterial = mat;
            boxCollider.edgeRadius = 0.05f;
        }
        if (body != null) body.sharedMaterial = mat;

        respawnPosition = transform.position;
    }

    private void Start()
    {
        maxLife = 3;
        if (AmuletDatabase.IsEquipped("vitalidade"))
            maxLife += 1;

        currentLife = Mathf.Clamp(currentLife, 0, maxLife);
        if (currentLife <= 0) currentLife = maxLife;

        AtualizarDanoEspada();
    }

    public void AtualizarDanoEspada()
    {
        if (GameDatabase.Instance == null)
        {
            damage = 25;
            return;
        }

        int level = GameDatabase.Instance.data.swordLevel;
        switch (level)
        {
            case 1: damage = 25; break;
            case 2: damage = 50; break;
            case 3: damage = 75; break;
            case 4: damage = 100; break;
            default: damage = 25; break;
        }
        Debug.Log($"[Espada] Nível {level} → Dano: {damage}");
    }

    private void Update()
    {
        if (isDead) return;

        horizontalInput = Input.GetAxis("Horizontal");

        if (horizontalInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        animator.SetBool("Grounded", isGrounded() || isGrounded2());
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));

        speed = Input.GetKey(Sprint) ? SprintValor : 3f;

        if (wallJumpCooldown > 0.2f)
        {
            float velX = horizontalInput * speed;

            if (onWall() && !isGrounded())
            {
                if (horizontalInput > 0 && transform.localScale.x > 0) velX = 0f;
                if (horizontalInput < 0 && transform.localScale.x < 0) velX = 0f;
            }

            body.linearVelocity = new Vector2(velX, body.linearVelocity.y);
        }
        else
        {
            wallJumpCooldown += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space))
            Jump();

        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            animator.SetTrigger("Attack");
            StartCoroutine(Attack());
        }

        if (inPortal())
        {
            SceneManager.LoadScene(Nomedoproximolevel);
        }
    }

    private void Jump()
    {
        if (isGrounded() || isGrounded2())
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);
        }
        else if (onWall() && !isGrounded())
        {
            body.linearVelocity = new Vector2(-Mathf.Sign(transform.localScale.x) * wallJumpForce, jumpPower);
            wallJumpCooldown = 0;
        }
    }

    private bool isGrounded() => Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, 0.1f, groundlayer);
    private bool isGrounded2() => Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, 0.1f, groundlayer2);
    private bool onWall() => Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);

    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible) return;

        currentLife -= damage;
        currentLife = Mathf.Max(currentLife, 0);

        if (currentLife <= 0)
            StartCoroutine(RotinaRespawn());
        else
            StartCoroutine(InvincibilityCoroutine());
    }

    // ==================== MÉTODO QUE FALTAVA ====================
    public void HitSpikes(float bounceForce)
    {
        if (isDead || isInvincible) return;

        body.linearVelocity = new Vector2(body.linearVelocity.x, bounceForce);
        StartCoroutine(RotinaPiscarVermelho());
    }

    private IEnumerator RotinaPiscarVermelho()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.4f);
            sr.color = Color.white;
        }
    }

    private IEnumerator RotinaRespawn()
    {
        isDead = true;
        body.linearVelocity = Vector2.zero;
        body.simulated = false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float finalInvTime = invincibleTime;
        if (AmuletDatabase.IsEquipped("escudo"))
            finalInvTime += 1f;

        yield return new WaitForSeconds(finalInvTime);
        isInvincible = false;
    }

    private IEnumerator Attack()
    {
        if (attackRange != null) attackRange.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        if (attackRange != null) attackRange.SetActive(false);
    }

    private bool inPortal()
    {
        return Physics2D.OverlapBox(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Portal) != null;
    }

    public void ActivateAttackCollider() => attackCollider?.SetActive(true);
    public void DeactivateAttackCollider() => attackCollider?.SetActive(false);

    public float GetCurrentDamage()
    {
        float finalDamage = damage;
        if (AmuletDatabase.IsEquipped("furia") && currentLife <= maxLife * 0.2f)
            finalDamage *= 1.25f;
        return finalDamage;
    }
}