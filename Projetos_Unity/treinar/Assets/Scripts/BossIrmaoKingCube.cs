using System.Collections;
using UnityEngine;

/// <summary>
/// 3º Boss: Irmão do King Cube
/// Descrição dos Ataques:
/// 
/// 1º ATAQUE - Giro em Rolamento:
/// O Boss encolhe-se e começa a girar intensamente, deslizando a alta velocidade de um canto
/// ao outro do mapa. O jogador é obrigado a saltar com timing correto para evitar ser atropelado.
/// 
/// 2º ATAQUE - Tiro de Dinheiro:
/// Com orgulho, o Boss atira moedas de ouro (projéteis) diretamente na direção do jogador, 
/// tentando atingi-lo à distância.
/// 
/// 3º ATAQUE - Grito de Afastamento (Scream & Knockback):
/// O Boss prepara-se, expande o peito e dá um grito ensurdecedor. O grito gera uma onda sonora 
/// circular visível (área AOE). Se o jogador for apanhado dentro da área, sofre dano e é 
/// violentamente empurrado para trás (Knockback).
/// </summary>
public class BossIrmaoKingCube : MonoBehaviour
{
    [Header("=== STATUS DO BOSS ===")]
    public float vidaMax = 300f;
    public float vidaAtual;
    public float tempoEntreAtaques = 3f;
    private bool estaMorto = false;
    private bool estaAtacando = false;

    [Header("=== ATAQUE 1: GIRO EM ROLAMENTO ===")]
    public float velocidadeGiro = 12f;
    public float tempoGiro = 2.5f;

    [Header("=== ATAQUE 2: LANÇAR MOEDAS ===")]
    public GameObject prefabProjetilMoeda; 
    public Transform pontoDisparo;
    public float velocidadeMoeda = 10f;
    public int quantidadeMoedas = 5;

    [Header("=== ATAQUE 3: GRITO SONORO ===")]
    public float raioGrito = 5f;
    public float forcaKnockback = 15f;
    public float danoGrito = 1f;

    private Transform jogador;
    private Rigidbody2D rb;
    private Vector2 posicaoOriginal;

    void Start()
    {
        vidaAtual = vidaMax;
        rb = GetComponent<Rigidbody2D>();
        posicaoOriginal = transform.position;

        // Auto tag e colliders
        gameObject.tag = "Enemy";

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jogador = p.transform;

        StartCoroutine(CicloBatalha());
    }

    private void Update()
    {
        if (estaMorto) return;

        // Sempre olhar na direção do jogador se não estiver a meio de um ataque de giro
        if (!estaAtacando && jogador != null)
        {
            if (jogador.position.x > transform.position.x)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    IEnumerator CicloBatalha()
    {
        yield return new WaitForSeconds(2f);

        while (!estaMorto)
        {
            if (jogador != null && jogador.gameObject.activeInHierarchy)
            {
                int ataqueAleatorio = Random.Range(1, 4);

                if (ataqueAleatorio == 1)
                {
                    yield return StartCoroutine(ExecutarAtaqueGiro());
                }
                else if (ataqueAleatorio == 2)
                {
                    yield return StartCoroutine(ExecutarAtaqueDinheiro());
                }
                else
                {
                    yield return StartCoroutine(ExecutarAtaqueGrito());
                }
            }

            yield return new WaitForSeconds(tempoEntreAtaques);
        }
    }

    // --- 1º ATAQUE: GIRO/ROLAMENTO ---
    IEnumerator ExecutarAtaqueGiro()
    {
        estaAtacando = true;
        Debug.Log("[Boss Irmão King Cube] Iniciando 1º Ataque: Giro de canto a canto!");

        // Guarda direção atual para avançar
        float direcao = (jogador != null && jogador.position.x > transform.position.x) ? 1f : -1f;

        float tempoDecorrido = 0f;
        while (tempoDecorrido < tempoGiro)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(direcao * velocidadeGiro, rb.linearVelocity.y);
            }
            else
            {
                transform.Translate(Vector3.right * direcao * velocidadeGiro * Time.deltaTime);
            }

            // Efeito visual de rotação
            transform.Rotate(0, 0, -direcao * 360f * Time.deltaTime);

            tempoDecorrido += Time.deltaTime;
            yield return null;
        }

        // Restaura rotação e para
        transform.rotation = Quaternion.identity;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        estaAtacando = false;
        yield return new WaitForSeconds(1f);
    }

    // --- 2º ATAQUE: ATIRAR DINHEIRO ---
    IEnumerator ExecutarAtaqueDinheiro()
    {
        estaAtacando = true;
        Debug.Log("[Boss Irmão King Cube] Iniciando 2º Ataque: Atirando Dinheiro!");

        Transform spawnPoint = pontoDisparo != null ? pontoDisparo : transform;

        for (int i = 0; i < quantidadeMoedas; i++)
        {
            if (jogador == null) break;

            GameObject moeda = null;
            if (prefabProjetilMoeda != null)
            {
                moeda = Instantiate(prefabProjetilMoeda, spawnPoint.position, Quaternion.identity);
            }
            else
            {
                // Fallback dinâmico: cria um projétil redondo amarelo
                moeda = new GameObject("BossGoldProjectile");
                moeda.transform.position = spawnPoint.position;
                moeda.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
                SpriteRenderer sr = moeda.AddComponent<SpriteRenderer>();
                sr.color = Color.yellow;
                sr.sprite = Resources.GetBuiltinResource<Sprite>("Knob.psd");
                CircleCollider2D cc = moeda.AddComponent<CircleCollider2D>();
                cc.isTrigger = true;
            }

            // Adiciona velocidade em linha reta
            Vector3 direcaoTiro = (jogador.position - spawnPoint.position).normalized;
            Rigidbody2D rbMoeda = moeda.GetComponent<Rigidbody2D>();
            if (rbMoeda == null) rbMoeda = moeda.AddComponent<Rigidbody2D>();
            rbMoeda.gravityScale = 0f;
            rbMoeda.linearVelocity = direcaoTiro * velocidadeMoeda;

            // Script de dano simples no projétil
            BossProjectileDamage damageScript = moeda.GetComponent<BossProjectileDamage>();
            if (damageScript == null) damageScript = moeda.AddComponent<BossProjectileDamage>();
            damageScript.dano = 1;

            Destroy(moeda, 4f);
            yield return new WaitForSeconds(0.3f);
        }

        estaAtacando = false;
    }

    // --- 3º ATAQUE: GRITO SONORO ---
    IEnumerator ExecutarAtaqueGrito()
    {
        estaAtacando = true;
        Debug.Log("[Boss Irmão King Cube] Iniciando 3º Ataque: Grito de Afastamento!");

        // Criar um indicador visual dinâmico do grito (círculo vermelho expandindo)
        GameObject indicador = new GameObject("ScreamWave");
        indicador.transform.position = transform.position;
        indicador.transform.localScale = Vector3.zero;
        SpriteRenderer sr = indicador.AddComponent<SpriteRenderer>();
        sr.color = new Color(1f, 0f, 0f, 0.4f); // Vermelho semi-transparente
        sr.sprite = Resources.GetBuiltinResource<Sprite>("Knob.psd");

        float tempoExpansao = 0.8f;
        float tempoDecorrido = 0f;

        // Verifica colisão do grito com o jogador
        bool jogadorAtingido = false;

        while (tempoDecorrido < tempoExpansao)
        {
            tempoDecorrido += Time.deltaTime;
            float escalaAtual = Mathf.Lerp(0f, raioGrito * 2f, tempoDecorrido / tempoExpansao);
            indicador.transform.localScale = new Vector3(escalaAtual, escalaAtual, 1f);

            if (jogador != null && !jogadorAtingido)
            {
                float dist = Vector2.Distance(transform.position, jogador.position);
                if (dist <= (escalaAtual / 2f))
                {
                    jogadorAtingido = true;
                    // Aplica dano e empurrão
                    PlayerMovement pm = jogador.GetComponent<PlayerMovement>();
                    if (pm != null)
                    {
                        pm.TakeDamage(danoGrito);
                    }

                    Rigidbody2D rbPlayer = jogador.GetComponent<Rigidbody2D>();
                    if (rbPlayer != null)
                    {
                        Vector2 direcaoKnock = (jogador.position - transform.position).normalized;
                        // Força horizontal com componente vertical para afastar no ar
                        rbPlayer.linearVelocity = new Vector2(direcaoKnock.x * forcaKnockback, 8f);
                    }
                    Debug.Log("[Boss Irmão King Cube] Jogador atingido pelo grito!");
                }
            }

            yield return null;
        }

        Destroy(indicador, 0.2f);
        estaAtacando = false;
    }

    public void TomarDano(float dano)
    {
        if (estaMorto) return;

        vidaAtual -= dano;
        Debug.Log($"[Boss Irmão King Cube] Tomou {dano} de dano! Vida atual: {vidaAtual}/{vidaMax}");

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    private void Morrer()
    {
        estaMorto = true;
        StopAllCoroutines();
        Debug.Log("[Boss Irmão King Cube] Derrotado com coragem!");

        // Dropa bastantes Plets como recompensa
        GameDatabase.Instance.data.plets += 200;

        Destroy(gameObject, 1f);
    }

    // Colisão direta simples
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement pm = collision.gameObject.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.TakeDamage(1);
            }
        }
    }
}

// Pequena classe auxiliar para dano de projéteis criados dinamicamente
public class BossProjectileDamage : MonoBehaviour
{
    public int dano = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null) pm.TakeDamage(dano);
            Destroy(gameObject);
        }
    }
}
