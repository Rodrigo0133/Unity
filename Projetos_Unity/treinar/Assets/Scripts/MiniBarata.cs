using UnityEngine;

public class MiniBarata : MonoBehaviour
{
    [Header("Atributos")]
    public float velocidade = 2f;
    public int vida = 25;
    public int danoAoBossQuandoMorre = 1;
    public int danoAoJogador = 1;
    public bool spriteOlhaParaDireita = true;

    private Transform jogador;
    private BossBarata boss;

    void Start()
    {
        // AUTO-CONFIGURAÇÃO: Garante que a tag é "Enemy" para o sistema de ataque funcionar
        gameObject.tag = "Enemy";

        // Garante que tem um Collider2D (senão o ataque do jogador não a deteta)
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            Debug.Log("[MiniBarata] BoxCollider2D adicionado automaticamente!");
        }

        // Garante que tem um Rigidbody2D (necessário para colisões 2D funcionarem)
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            Debug.Log("[MiniBarata] Rigidbody2D adicionado automaticamente!");
        }

        // Tenta encontrar o boss na cena
        boss = FindObjectOfType<BossBarata>();

        // Tenta encontrar o jogador pela Tag
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            jogador = p.transform;
        }
        else
        {
            // Fallback: procura por um objeto com nome de quadrado (para testes rápidos)
            GameObject[] objetos = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in objetos)
            {
                string n = obj.name.ToLower();
                if ((n.Contains("square") || n.Contains("quadrado") || n.Contains("cube") || n.Contains("cubo")) && obj.transform != this.transform)
                {
                    if (boss == null || obj.transform != boss.transform)
                    {
                        jogador = obj.transform;
                        break;
                    }
                }
            }
        }
    }

    void Update()
    {
        if (jogador != null)
        {
            VirarParaJogador();

            // Persegue o jogador lentamente (2D)
            transform.position = Vector2.MoveTowards(transform.position, jogador.position, velocidade * Time.deltaTime);
        }
    }

    // Método compatível com Ataque.cs (mesmo nome que os outros inimigos)
    public void TakeDamage()
    {
        TomarDano(1);
    }

    public void TomarDano(int dano)
    {
        vida -= dano;
        Debug.Log($"[MiniBarata] Tomou {dano} de dano! Vida restante: {vida}");
        if (vida <= 0)
        {
            Morrer();
        }
    }

    private void Morrer()
    {
        Debug.Log("[MiniBarata] Morreu!");
        GameDatabase.Instance.AddPlets(20);
        Destroy(gameObject);
    }
    [Header("Cooldown de Ataque")]
    public float cooldownAtaque = 1.5f;
    private float proximoAtaque = 0f;

    // COLISÃO 2D - Quando a mini barata toca no jogador, dá-lhe dano (com cooldown)
    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < proximoAtaque) return; // Ainda em cooldown

        if (other.CompareTag("Player") || other.name.Contains("Square") || other.name.Contains("Quadrado"))
        {
            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.TakeDamage(danoAoJogador);
                proximoAtaque = Time.time + cooldownAtaque;
                Debug.Log($"[MiniBarata] Causou {danoAoJogador} de dano ao jogador! Próximo ataque em {cooldownAtaque}s");
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time < proximoAtaque) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name.Contains("Square"))
        {
            PlayerMovement pm = collision.gameObject.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.TakeDamage(danoAoJogador);
                proximoAtaque = Time.time + cooldownAtaque;
                Debug.Log($"[MiniBarata] Causou {danoAoJogador} de dano ao jogador! (Física)");
            }
        }
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
