using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==============================================================================
// CLASSES AUXILIARES (Tudo no mesmo ficheiro para simplificar)
// ==============================================================================

/// <summary>
/// Script para as moedas, potes de ouro e pedras.
/// Colocar este script no Prefab do projétil (que deve ter um Collider2D como Trigger).
/// </summary>
public class ProjetilBoss : MonoBehaviour
{
    public float velocidade = 5f;
    public float tempoDeVida = 5f;
    private Vector3 direcao;
    
    // Todos os ataques tiram 1 de dano, como pedido
    private int dano = 1;

    void Start()
    {
        Destroy(gameObject, tempoDeVida);
    }

    void Update()
    {
        if (direcao != Vector3.zero)
        {
            transform.Translate(direcao.normalized * velocidade * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            PlayerMovement pm = col.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.TakeDamage(dano);
            }
            Destroy(gameObject);
        }
        else if (col.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }

    public void Configurar(Vector3 dir)
    {
        direcao = dir;
    }
}

/// <summary>
/// Script para o aviso do espinho (Pisão do Pé Grande).
/// Colocar no Prefab de Aviso (ex: um círculo vermelho no chão).
/// </summary>
public class AvisoEspinhoBoss : MonoBehaviour
{
    public float tempoDeAviso = 1.2f;
    public GameObject prefabEspinhoReal; // O prefab que tem o script Espinho.cs já existente

    void Start()
    {
        StartCoroutine(SpawnEspinho());
    }

    IEnumerator SpawnEspinho()
    {
        yield return new WaitForSeconds(tempoDeAviso);

        if (prefabEspinhoReal != null)
        {
            Instantiate(prefabEspinhoReal, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning("[Boss 2] Prefab do Espinho Real não atribuído no AvisoEspinhoBoss!");
        }
        Destroy(gameObject);
    }
}

// ==============================================================================
// CLASSE PRINCIPAL DO BOSS (ANÃO + PÉ GRANDE)
// ==============================================================================
public class BossAnaoMitologico : MonoBehaviour
{
    public enum EstadoTurno
    {
        Anao,
        PeGrande
    }

    [Header("=== REFERÊNCIAS DO ANÃO ===")]
    [Tooltip("Animator do Anão (opcional)")]
    public Animator animAnao;
    [Tooltip("Local de onde saem os tiros do Anão (opcional, usa o centro se vazio)")]
    public Transform pontoDisparoAnao;
    
    [Header("=== REFERÊNCIAS DO PÉ GRANDE ===")]
    [Tooltip("O GameObject do Pé Grande que vai ser ativado na fase 2")]
    public GameObject peGrandeGameObject; 
    [Tooltip("Animator do Pé Grande (opcional)")]
    public Animator animPeGrande;
    [Tooltip("Local de onde saem as pedras do Pé Grande (opcional)")]
    public Transform pontoDisparoPeGrande;

    [Header("=== PREFABS DOS ATAQUES ===")]
    public GameObject prefabMoeda;
    public GameObject prefabPoteOuro;
    public GameObject prefabPedra;
    public GameObject prefabAvisoEspinho; // Onde colocas o script AvisoEspinhoBoss

    [Header("=== STATUS ===")]
    public int vidaMax = 20;
    [Tooltip("Podes ver a vida atual aqui durante o jogo")]
    public int vidaAtual; 
    private bool fase2Ativa = false;
    private bool estaMorto = false;

    [Header("=== TIMING ===")]
    public float tempoEntreAtaques = 2.5f;

    private Transform jogador;
    private EstadoTurno turnoAtual = EstadoTurno.Anao; 

    void Start()
    {
        vidaAtual = vidaMax;
        
        // Garante que o Pé Grande começa desativado
        if (peGrandeGameObject != null) peGrandeGameObject.SetActive(false);

        ProcurarJogador();

        // Inicia o ciclo de batalha
        StartCoroutine(CicloDeBatalha());
    }

    private void ProcurarJogador()
    {
        if (jogador == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) jogador = p.transform;
        }
    }

    // ----------------------------------------------------
    // SISTEMA DE VIDA
    // ----------------------------------------------------
    public void TomarDano(int dano)
    {
        if (estaMorto) return;

        vidaAtual -= dano;
        vidaAtual = Mathf.Max(vidaAtual, 0);

        Debug.Log("[Boss 2] Anão levou dano! Vida: " + vidaAtual);

        if (animAnao != null) animAnao.SetTrigger("Hit");

        if (vidaAtual <= (vidaMax / 2) && !fase2Ativa)
        {
            AtivarFase2();
        }

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    private void AtivarFase2()
    {
        fase2Ativa = true;
        Debug.Log("[Boss 2] FASE 2 INICIADA! Pé Grande Invocado!");

        if (peGrandeGameObject != null)
        {
            peGrandeGameObject.SetActive(true);
        }
    }

    private void Morrer()
    {
        estaMorto = true;
        StopAllCoroutines();

        Debug.Log("[Boss 2] Anão Mitológico Derrotado!");

        if (animAnao != null) animAnao.SetTrigger("Morrer");
        if (animPeGrande != null) animPeGrande.SetTrigger("Morrer");

        // Desativa tudo passados uns segundos
        Destroy(gameObject, 2f);
        if (peGrandeGameObject != null) Destroy(peGrandeGameObject, 2f);
    }

    // ----------------------------------------------------
    // CÉREBRO DA BATALHA
    // ----------------------------------------------------
    IEnumerator CicloDeBatalha()
    {
        yield return new WaitForSeconds(1.5f);

        while (!estaMorto)
        {
            // Tenta encontrar o jogador a cada ciclo caso tenha morrido/reaparecido
            ProcurarJogador(); 

            if (jogador != null && jogador.gameObject.activeInHierarchy)
            {
                if (!fase2Ativa)
                {
                    // FASE 1: Só o Anão ataca
                    yield return StartCoroutine(AtacarAnao());
                }
                else
                {
                    // FASE 2: Alterna entre Anão e Pé Grande
                    if (turnoAtual == EstadoTurno.Anao)
                    {
                        yield return StartCoroutine(AtacarAnao());
                        turnoAtual = EstadoTurno.PeGrande;
                    }
                    else
                    {
                        if (peGrandeGameObject != null && peGrandeGameObject.activeInHierarchy)
                        {
                            yield return StartCoroutine(AtacarPeGrande());
                        }
                        turnoAtual = EstadoTurno.Anao;
                    }
                }
            }

            yield return new WaitForSeconds(tempoEntreAtaques);
        }
    }

    // ----------------------------------------------------
    // ATAQUES DO ANÃO
    // ----------------------------------------------------
    IEnumerator AtacarAnao()
    {
        if (jogador == null) yield break;

        // Escolhe um ataque (1 = Moedas, 2 = Pote de Ouro)
        int escolha = Random.Range(1, 3);

        Transform origem = pontoDisparoAnao != null ? pontoDisparoAnao : transform;

        if (escolha == 1)
        {
            // ATAQUE: Disparar Moedas (ex: 3 moedas seguidas)
            if (animAnao != null) animAnao.SetTrigger("AtacarMoedas");
            
            for (int i = 0; i < 3; i++)
            {
                DispararProjetil(prefabMoeda, origem, 8f);
                yield return new WaitForSeconds(0.3f);
            }
        }
        else
        {
            // ATAQUE: Disparar Pote de Ouro (1 projétil)
            if (animAnao != null) animAnao.SetTrigger("AtacarPote");
            
            DispararProjetil(prefabPoteOuro, origem, 6f);
            yield return new WaitForSeconds(0.5f);
        }
    }

    // ----------------------------------------------------
    // ATAQUES DO PÉ GRANDE
    // ----------------------------------------------------
    IEnumerator AtacarPeGrande()
    {
        if (jogador == null) yield break;

        // Escolhe um ataque (1 = Pisão Espinhos, 2 = Pedras)
        int escolha = Random.Range(1, 3);

        Transform origem = pontoDisparoPeGrande != null ? pontoDisparoPeGrande : peGrandeGameObject.transform;

        if (escolha == 1)
        {
            // ATAQUE: Pisão (Aviso + Espinhos)
            if (animPeGrande != null) animPeGrande.SetTrigger("Pisao");
            yield return new WaitForSeconds(0.5f); // Tempo até o pé bater no chão

            Vector3 posJogador = ObterPosicaoPesJogador();
            
            if (prefabAvisoEspinho != null)
            {
                // Espinho debaixo do jogador
                Instantiate(prefabAvisoEspinho, posJogador, Quaternion.identity);
                
                // Mais 1 aviso aleatório ao lado
                Vector3 posAleatoria = new Vector3(posJogador.x + Random.Range(-4f, 4f), posJogador.y, 0f);
                Instantiate(prefabAvisoEspinho, posAleatoria, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("[Boss 2] Prefab Aviso Espinho não está atribuído no Inspector!");
            }
        }
        else
        {
            // ATAQUE: Lançar Pedras
            if (animPeGrande != null) animPeGrande.SetTrigger("LancarPedra");
            
            for (int i = 0; i < 2; i++)
            {
                DispararProjetil(prefabPedra, origem, 7f);
                yield return new WaitForSeconds(0.4f);
            }
        }
    }

    // ----------------------------------------------------
    // HELPERS
    // ----------------------------------------------------
    private void DispararProjetil(GameObject prefab, Transform origem, float velocidade)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[Boss 2] Falta atribuir um prefab de projétil no Inspector!");
            return;
        }
        
        if (jogador == null) return;

        GameObject proj = Instantiate(prefab, origem.position, Quaternion.identity);
        
        // Calcula a direção para o jogador
        Vector3 direcao = (jogador.position - origem.position).normalized;
        
        ProjetilBoss pb = proj.GetComponent<ProjetilBoss>();
        if (pb != null)
        {
            pb.velocidade = velocidade;
            pb.Configurar(direcao);
        }
        else
        {
            Debug.LogWarning("[Boss 2] O prefab " + prefab.name + " não tem o script ProjetilBoss anexado!");
        }
    }

    private Vector3 ObterPosicaoPesJogador()
    {
        if (jogador == null) return transform.position;

        Collider2D col = jogador.GetComponent<Collider2D>();
        if (col != null)
        {
            // O pé do jogador costuma ser a base do collider
            return new Vector3(jogador.position.x, col.bounds.min.y, 0f);
        }
        
        return jogador.position;
    }
}
