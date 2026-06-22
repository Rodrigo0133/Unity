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
    public const int MaxSwordLevel = 4;

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
        AtualizarEfeitosAmuletos();
        RestaurarPosicaoGuardada();

        AtualizarDanoEspada();
        DesativarObjetoDeAtaque(attackRange);
        if (attackCollider != attackRange)
            DesativarObjetoDeAtaque(attackCollider);
    }

    public void AtualizarDanoEspada()
    {
        if (GameDatabase.Instance == null)
        {
            damage = 25;
            return;
        }

        int level = Mathf.Clamp(GameDatabase.Instance.data.swordLevel, 1, MaxSwordLevel);
        GameDatabase.Instance.data.swordLevel = level;
        damage = GetSwordDamage(level);
        Debug.Log($"[Espada] Nível {level} → Dano: {damage}");
    }

    public static int GetSwordDamage(int level)
    {
        switch (Mathf.Clamp(level, 1, MaxSwordLevel))
        {
            case 1: return 25;
            case 2: return 50;
            case 3: return 75;
            case 4: return 100;
            default: return 25;
        }
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

        float velocidadeBase = Input.GetKey(Sprint) ? SprintValor : 3f;
        speed = velocidadeBase * GetSpeedMultiplier();

        if (wallJumpCooldown > 0.2f)
        {
            float velX = horizontalInput * speed;
            int wallDirection = GetWallDirection();
            bool grounded = isGrounded() || isGrounded2();

            if (wallDirection != 0 && !grounded)
            {
                if (Mathf.Sign(horizontalInput) == wallDirection)
                    velX = 0f;

                if (body.linearVelocity.y < -wallSlideSpeed)
                    body.linearVelocity = new Vector2(body.linearVelocity.x, -wallSlideSpeed);
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
        else if (onWall())
        {
            int wallDirection = GetWallDirection();
            if (wallDirection == 0)
                wallDirection = transform.localScale.x > 0 ? 1 : -1;

            body.linearVelocity = new Vector2(-wallDirection * wallJumpForce, jumpPower);
            wallJumpCooldown = 0;
        }
    }

    private bool isGrounded() => Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, 0.1f, groundlayer);
    private bool isGrounded2() => Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, 0.1f, groundlayer2);
    private bool onWall() => GetWallDirection() != 0;

    private int GetWallDirection()
    {
        LayerMask activeWallLayer = wallLayer.value != 0 ? wallLayer : groundlayer | groundlayer2;
        bool touchingRight = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.right, 0.1f, activeWallLayer);
        bool touchingLeft = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.left, 0.1f, activeWallLayer);

        if (touchingRight && !touchingLeft) return 1;
        if (touchingLeft && !touchingRight) return -1;
        if (touchingRight && touchingLeft) return transform.localScale.x > 0 ? 1 : -1;
        return 0;
    }

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
        if (AmuletDatabase.IsEquipped("Escudo"))
            finalInvTime += 1f;

        yield return new WaitForSeconds(finalInvTime);
        isInvincible = false;
    }

    private IEnumerator Attack()
    {
        AtivarObjetoDeAtaque(attackRange);
        if (attackCollider != attackRange)
            AtivarObjetoDeAtaque(attackCollider);

        yield return new WaitForSeconds(0.2f);

        DesativarObjetoDeAtaque(attackRange);
        if (attackCollider != attackRange)
            DesativarObjetoDeAtaque(attackCollider);
    }

    private bool inPortal()
    {
        return Physics2D.OverlapBox(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Portal) != null;
    }

    public void ActivateAttackCollider() => AtivarObjetoDeAtaque(attackCollider);
    public void DeactivateAttackCollider() => DesativarObjetoDeAtaque(attackCollider);

    private void AtivarObjetoDeAtaque(GameObject objetoAtaque)
    {
        if (objetoAtaque == null) return;

        objetoAtaque.SetActive(true);
        Ataque ataque = objetoAtaque.GetComponent<Ataque>();
        if (ataque == null)
            ataque = objetoAtaque.GetComponentInChildren<Ataque>(true);

        if (ataque != null)
            ataque.AtivarAtaque();
    }

    private void DesativarObjetoDeAtaque(GameObject objetoAtaque)
    {
        if (objetoAtaque == null) return;

        Ataque ataque = objetoAtaque.GetComponent<Ataque>();
        if (ataque == null)
            ataque = objetoAtaque.GetComponentInChildren<Ataque>(true);

        if (ataque != null)
            ataque.DesativarAtaque();

        objetoAtaque.SetActive(false);
    }

    public float GetCurrentDamage()
    {
        float finalDamage = damage;
        if (AmuletDatabase.IsEquipped("Furia"))
            finalDamage *= 1.10f;
        return finalDamage;
    }

    public void AtualizarEfeitosAmuletos()
    {
        int vidaMaximaAnterior = maxLife;

        maxLife = 3;
        if (AmuletDatabase.IsEquipped("Vitalidade"))
            maxLife += 1;

        if (currentLife <= 0)
        {
            currentLife = maxLife;
        }
        else
        {
            if (maxLife > vidaMaximaAnterior && currentLife >= vidaMaximaAnterior)
                currentLife = maxLife;

            currentLife = Mathf.Clamp(currentLife, 0, maxLife);
        }
    }

    private float GetSpeedMultiplier()
    {
        return AmuletDatabase.IsEquipped("Rapidez") ? 1.10f : 1f;
    }

    private void RestaurarPosicaoGuardada()
    {
        if (GameDatabase.Instance == null)
            return;

        Vector3 posicaoGuardada;
        string cenaAtual = SceneManager.GetActiveScene().name;

        if (GameDatabase.Instance.TryGetSavedPlayerPosition(cenaAtual, out posicaoGuardada))
            transform.position = posicaoGuardada;
    }
}
