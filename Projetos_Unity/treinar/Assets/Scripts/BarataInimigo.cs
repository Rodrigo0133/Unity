using System.Collections;
using UnityEngine;

public class BarataInimigo : MonoBehaviour
{
    [Header("Atributos")]
    public float velocidade = 2f;
    public int vida = 50;
    public int danoAoJogador = 1;
    public float distanciaAtivacao = 8f; // Ajuste conforme necessário
    public bool spriteOlhaParaDireita = true;

    private Transform jogador;
    private Vector2 posicaoInicial;
    private bool estaMorta = false;

    [Header("Cooldown de Ataque")]
    public float cooldownAtaque = 1.5f;
    private float proximoAtaque = 0f;

    void Start()
    {
        posicaoInicial = transform.position;
        gameObject.tag = "Enemy";

        // Auto-configuração de componentes
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Encontra o jogador
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            jogador = p.transform;
    }

    void Update()
    {
        if (estaMorta || jogador == null) return;

        float distancia = Vector2.Distance(transform.position, jogador.position);

        if (distancia <= distanciaAtivacao)
        {
            VirarParaJogador();
            transform.position = Vector2.MoveTowards(transform.position, jogador.position, velocidade * Time.deltaTime);
        }
        else if (Vector2.Distance(transform.position, posicaoInicial) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(transform.position, posicaoInicial, velocidade * Time.deltaTime);
        }
    }

    public void TakeDamage()
    {
        TomarDano(1);
    }

    public void TomarDano(int dano)
    {
        if (estaMorta) return;

        vida -= dano;

        if (vida <= 0)
            Morrer();
    }

    private void Morrer()
    {
        if (estaMorta) return;
        estaMorta = true;

        // Recompensa
        if (GameDatabase.Instance != null && GameDatabase.Instance.data != null)
        {
            GameDatabase.Instance.data.plets += 60; // 20 + 40
            GameDatabase.Instance.SaveGame();
        }

        StartCoroutine(RotinaMorte());
    }

    private IEnumerator RotinaMorte()
    {
        // Desativa colisões
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        // Efeito visual
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.red;

        // Pulo de morte
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2f;
            rb.linearVelocity = new Vector2(0f, 6f);
        }

        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TentarDarDano(other.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TentarDarDano(other.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TentarDarDano(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TentarDarDano(collision.gameObject);
    }

    private void TentarDarDano(GameObject alvo)
    {
        if (estaMorta || Time.time < proximoAtaque) return;

        PlayerMovement pm = alvo.GetComponent<PlayerMovement>();
        if (pm == null)
            pm = alvo.GetComponentInParent<PlayerMovement>();

        if (pm == null || !pm.CompareTag("Player")) return;

        pm.TakeDamage(danoAoJogador);
        proximoAtaque = Time.time + cooldownAtaque;
    }

    private void VirarParaJogador()
    {
        if (jogador == null) return;

        float direcao = jogador.position.x >= transform.position.x ? 1f : -1f;
        if (!spriteOlhaParaDireita) direcao *= -1f;

        Vector3 escala = transform.localScale;
        escala.x = Mathf.Abs(escala.x) * direcao;
        transform.localScale = escala;
    }
}
