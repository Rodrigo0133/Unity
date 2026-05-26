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
    private Vector3 respawnPosition;
    private bool isDead = false;

    private PlayerLives playerLives;


    public GameObject attackHitbox;
    public float attackDuration = 0.3f;
    public float attackCooldown = 0.6f; // Tempo entre ataques
    private float nextAttackTime = 0f;  // Controlador do tempo do próximo ataque permitido
    public int damage = 25;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        body.gravityScale = gravityScale;

        // --- CORREÇÃO DO BUG DE FICAR PRESO NA PAREDE ---
        // Cria um material de física sem atrito (Zero Friction) dinamicamente.
        PhysicsMaterial2D mat = new PhysicsMaterial2D("ZeroFriction");
        mat.friction = 0f;
        mat.bounciness = 0f;
        
        if (boxCollider != null) 
        {
            boxCollider.sharedMaterial = mat;
            // A MAGIA ESTÁ AQUI: Arredonda os cantos da hitbox verde para ela deslizar nos blocos
            // em vez de encravar nas fissuras afiadas do Tilemap!
            boxCollider.edgeRadius = 0.05f;
        }
        if (body != null) body.sharedMaterial = mat;
        // ------------------------------------------------

        currentLife = maxLife;
        respawnPosition = transform.position; // Guarda a posição exata onde colocaste o jogador na Unity

        playerLives = GetComponentInParent<PlayerLives>();

    }
        

    [System.Obsolete]
    private void Update()
    {
        if (isDead) return; // Impede movimento/ataques se estiver morto

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
            float velX = horizontalInput * speed;

            // Previne o bug das "costuras" (seams) do Tilemap.
            // Quando vais muito depressa (Shift) contra a parede, a hitbox encrava entre dois blocos.
            // Para resolver isto, anulamos a força horizontal contra a parede se já estivermos a encostar nela.
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
        {
            Jump();
        }
        
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
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
        if (isDead || isInvincible) return;

        currentLife -= damage;
        currentLife = Mathf.Max(currentLife, 0); // Não deixa ficar negativo

        Debug.Log($"[Jogador] Tomou {damage} de dano! Vida: {currentLife}/{maxLife}");

        if (currentLife <= 0)
        {
            StartCoroutine(RotinaRespawn());
        }
        else
        {
            // Invencibilidade temporária a cada hit para não ser metralhado
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    private System.Collections.IEnumerator RotinaRespawn()
    {
        isDead = true;
        Debug.Log("Jogador morreu! A reiniciar a fase...");
        
        // Para o jogador e desativa a física temporariamente
        body.linearVelocity = Vector2.zero;
        body.simulated = false; 

        // Oculta o sprite do jogador para parecer que desapareceu/morreu
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        yield return new WaitForSeconds(1.5f); // Atraso antes de reiniciar

        // Reinicia a cena atual (Game Over a sério)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private System.Collections.IEnumerator InvincibilityCoroutine()
    {   
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    public void HitSpikes(float bounceForce)
    {
        if (isDead || isInvincible) return;
        
        body.linearVelocity = new Vector2(body.linearVelocity.x, bounceForce);
        StartCoroutine(RotinaPiscarVermelho());
    }

    private System.Collections.IEnumerator RotinaPiscarVermelho()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.4f);
            sr.color = Color.white;
        }
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckEnemyBounce(other.gameObject, other.bounds);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckEnemyBounce(collision.gameObject, collision.collider.bounds);
    }

    private void CheckEnemyBounce(GameObject enemyObject, Bounds enemyBounds)
    {
        // Se bateu num inimigo
        if (enemyObject.CompareTag("Enemy"))
        {
            // Ignora Bosses (não permite pular em cima dos Bosses)
            if (enemyObject.GetComponent<BossBarata>() != null || enemyObject.GetComponent<BossAnaoMitologico>() != null)
                return;

            // Só podemos pular em cima se estivermos a cair (velocidade Y negativa)
            if (body.linearVelocity.y < 0.1f)
            {
                // Verifica se a base dos nossos pés está acima do centro do inimigo
                if (boxCollider.bounds.min.y >= enemyBounds.center.y)
                {
                    // Faz o "Pulo do Mario" apenas como impulso (sem dar dano)
                    body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);

                    // Fica temporariamente invencível para não levar dano do inimigo em que acabou de pisar
                    StartCoroutine(JumpInvincibility());
                }
            }
        }
    }

    private System.Collections.IEnumerator JumpInvincibility()
    {
        if (isInvincible) yield break; // Se já estiver invencível por outra razão, não mexe
        
        isInvincible = true;
        yield return new WaitForSeconds(0.15f);
        isInvincible = false;
    }
}
