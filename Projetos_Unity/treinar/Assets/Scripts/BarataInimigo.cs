using UnityEngine;

public class BarataInimigo : MonoBehaviour
{
    [Header("Atributos")]
    public float velocidade = 2f;
    public int vida = 50;
    public int danoAoJogador = 1;
    public float distanciaAtivacao; // Distância para começar a seguir o jogador

    private Transform jogador;
    private Vector2 posicaoInicial;
    private bool estaMorta = false;

    [Header("Cooldown de Ataque")]
    public float cooldownAtaque = 1.5f;
    private float proximoAtaque = 0f;

    void Start()
    {
        // Guarda a posição inicial para voltar quando o jogador sair do range
        posicaoInicial = transform.position;

        // AUTO-CONFIGURAÇÃO: Garante que a tag é "Enemy" para o sistema de ataque funcionar
        gameObject.tag = "Enemy";

        // Garante que tem um Collider2D (senão o ataque do jogador não a deteta)
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            Debug.Log("[BarataInimigo] BoxCollider2D adicionado automaticamente!");
        }

        // Garante que tem um Rigidbody2D (necessário para colisões 2D funcionarem)
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            Debug.Log("[BarataInimigo] Rigidbody2D adicionado automaticamente!");
        }

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
                    jogador = obj.transform;
                    break;
                }
            }
        }
    }

    void Update()
    {
        if (estaMorta) return;

        if (jogador != null)
        {
            float distancia = Vector2.Distance(transform.position, jogador.position);

            if (distancia <= distanciaAtivacao)
            {
                // Entrou no range, persegue o jogador
                transform.position = Vector2.MoveTowards(transform.position, jogador.position, velocidade * Time.deltaTime);
            }
            else
            {
                // Saiu do range, volta para a posição inicial
                if (Vector2.Distance(transform.position, posicaoInicial) > 0.05f)
                {
                    transform.position = Vector2.MoveTowards(transform.position, posicaoInicial, velocidade * Time.deltaTime);
                }
            }
        }
    }

    // Método compatível com Ataque.cs (mesmo nome que os outros inimigos)
    public void TakeDamage()
    {
        TomarDano(1);
    }

    public void TomarDano(int dano)
    {
        if (estaMorta) return;

        vida -= dano;
        Debug.Log($"[BarataInimigo] Tomou {dano} de dano! Vida restante: {vida}");
        if (vida <= 0)
        {
            Morrer();
        }
    }

    private void Morrer()
    {
        if (estaMorta) return;
        estaMorta = true;

        Debug.Log("[BarataInimigo] Morreu!");
        StartCoroutine(RotinaMorte());
    }

    private System.Collections.IEnumerator RotinaMorte()
    {
        // Desativa colisões para não dar mais dano ao jogador nem bloquear
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        // Fica vermelha
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.red;

        // Dá um pulinho para cima
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic; // Permite que a física da gravidade atue
            rb.gravityScale = 2f;
            rb.linearVelocity = new Vector2(0f, 6f); // Força do pulo
        }

        // Spawn coins upon death
        PletCoin.Spawn(transform.position, Random.Range(2, 5));

        // Espera 1.5 segundo para vermos o pulo cair
        yield return new WaitForSeconds(1.5f);

        // Deleta o objeto
        Destroy(gameObject);
    }

    // COLISÃO 2D - Quando a barata toca no jogador, dá-lhe dano (com cooldown)
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
                Debug.Log($"[BarataInimigo] Causou {danoAoJogador} de dano ao jogador! Próximo ataque em {cooldownAtaque}s");
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
                Debug.Log($"[BarataInimigo] Causou {danoAoJogador} de dano ao jogador! (Física)");
            }
        }
    }
}
