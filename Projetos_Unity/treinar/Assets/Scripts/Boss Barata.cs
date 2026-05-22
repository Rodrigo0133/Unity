using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Boss Barata - Máquina de estados completa com 3 ataques, sistema de vida,
/// fase de fúria, e IA totalmente funcional.
/// 
/// COMO USAR:
///   1. Arrasta este script para o GameObject do Boss no Inspector.
///   2. Preenche os campos públicos marcados com [Header].
///   3. O campo "jogador" e "anim" são preenchidos automaticamente se não definidos.
///   4. Certifica-te que o Player tem a tag "Player".
///   5. Para causar dano ao boss, chama: boss.GetComponent<BossBarata>().TomarDano(valor);
/// </summary>
public class BossBarata : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════
    // ENUMS
    // ══════════════════════════════════════════════════════════════
    public enum BossState
    {
        Idle,
        PreparandoAtaque,
        Atacando,
        Cooldown,
        Morto
    }

    // ══════════════════════════════════════════════════════════════
    // CAMPOS PRIVADOS ESSENCIAIS (declarados aqui para não haver erros)
    // ══════════════════════════════════════════════════════════════
    private Animator anim;
    private Transform jogador;
    private bool estaMorto = false;
    private bool emFase2 = false;

    // ══════════════════════════════════════════════════════════════
    // INSPECTOR - STATUS
    // ══════════════════════════════════════════════════════════════
    [Header("═══ STATUS DO BOSS ═══")]
    [Tooltip("Vida máxima do Boss.")]
    public int maxVida = 35;

    [Tooltip("Vida atual do Boss (preenchido automaticamente no Start).")]
    public int vidaAtual;

    [Tooltip("Estado actual do Boss (útil para debug no Inspector).")]
    public BossState currentState = BossState.Idle;

    // ══════════════════════════════════════════════════════════════
    // INSPECTOR - REFERÊNCIAS
    // ══════════════════════════════════════════════════════════════
    [Header("═══ REFERÊNCIAS ═══")]
    [Tooltip("Animator do Boss. Preenchido automaticamente se deixado vazio.")]
    public Animator animatorExterno;

    [Tooltip("Transform do Jogador. Preenchido automaticamente pela tag 'Player' se deixado vazio.")]
    public Transform jogadorExterno;

    // ══════════════════════════════════════════════════════════════
    // INSPECTOR - IA / TIMING
    // ══════════════════════════════════════════════════════════════
    [Header("═══ IA / TIMING ═══")]
    [Tooltip("Tempo mínimo de espera entre ataques (Fase 1).")]
    public float tempoEntreAtaquesMin = 2f;

    [Tooltip("Tempo máximo de espera entre ataques (Fase 1).")]
    public float tempoEntreAtaquesMax = 4.5f;

    [Tooltip("Percentagem de vida (0-1) em que o boss entra na Fase 2 (modo fúria).")]
    [Range(0f, 1f)]
    public float limiarFase2 = 0.5f;

    // ══════════════════════════════════════════════════════════════
    // INSPECTOR - ATAQUE 1: EMBOSCADA SUBTERRÂNEA
    // ══════════════════════════════════════════════════════════════
    [Header("═══ ATAQUE 1: EMBOSCADA ═══")]
    [Tooltip("Prefab visual de aviso no chão (ex: círculo vermelho).")]
    public GameObject avisoDeBuracoPrefab;

    [Tooltip("Segundos que o aviso fica visível antes do boss emergir.")]
    public float tempoAvisoEmboscada = 2f;

    [Tooltip("Tempo da animação de entrar no chão.")]
    public float tempoAnimacaoEntrar = 1f;

    [Tooltip("Tempo da animação de sair do chão.")]
    public float tempoAnimacaoSair = 2f;

    [Tooltip("Raio do dano em área ao emergir.")]
    public float raioErupcao = 2.5f;

    [Tooltip("Dano causado pela erupção.")]
    public int danoErupcao = 15;

    // ══════════════════════════════════════════════════════════════
    // INSPECTOR - ATAQUE 2: ENXAME DE MINI BARATAS
    // ══════════════════════════════════════════════════════════════
    [Header("═══ ATAQUE 2: ENXAME ═══")]
    [Tooltip("Prefab da mini barata.")]
    public GameObject miniBarataPrefab;

    [Tooltip("Transform da boca/barriga por onde saem as baratinhas.")]
    public Transform pontoSpawnMiniBaratas;

    [Tooltip("Número de mini baratas invocadas.")]
    public int quantidadeDeMiniBaratas = 6;

    [Tooltip("Intervalo entre cada spawn de baratinha.")]
    public float tempoEntreSpawns = 0.3f;

    // ══════════════════════════════════════════════════════════════
    // INSPECTOR - ATAQUE 3: ONDA DE TERRA
    // ══════════════════════════════════════════════════════════════
    [Header("═══ ATAQUE 3: ONDA DE TERRA ═══")]
    [Tooltip("Prefab da onda de terra.")]
    public GameObject ondaDeTerraPrefab;

    [Tooltip("Transform na frente do boss onde a onda nasce.")]
    public Transform pontoSpawnOnda;

    // ══════════════════════════════════════════════════════════════
    // INSPECTOR - FEEDBACK / VFX / SFX
    // ══════════════════════════════════════════════════════════════
    [Header("═══ FEEDBACK VISUAL / SOM ═══")]
    [Tooltip("Prefab de partículas ao tomar dano (opcional).")]
    public GameObject vfxDano;

    [Tooltip("Prefab de efeito de morte (opcional).")]
    public GameObject vfxMorte;

    [Tooltip("AudioSource do Boss (opcional, para sons).")]
    public AudioSource audioSource;

    [Tooltip("Som ao tomar dano.")]
    public AudioClip somDano;

    [Tooltip("Som ao morrer.")]
    public AudioClip somMorte;

    // ══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════
    void Start()
    {
        vidaAtual = maxVida;

        // Resolve referências: usa as externas se definidas, senão busca automaticamente
        anim = (animatorExterno != null) ? animatorExterno : GetComponent<Animator>();

        if (jogadorExterno != null)
        {
            jogador = jogadorExterno;
        }
        else
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                jogador = p.transform;
            else
                Debug.LogWarning("[BossBarata] Nenhum GameObject com tag 'Player' encontrado na cena!");
        }

        StartCoroutine(CicloDeBatalha());
    }

    // ══════════════════════════════════════════════════════════════
    // SISTEMA DE VIDA E MORTE
    // ══════════════════════════════════════════════════════════════

    /// <summary>Causa dano ao Boss. Chama este método de outros scripts (projéteis, espada, etc.).</summary>
    public void TomarDano(int dano)
    {
        if (estaMorto) return;

        vidaAtual -= dano;
        vidaAtual = Mathf.Max(vidaAtual, 0); // Não passa de 0

        Debug.Log($"[BossBarata] Tomou {dano} de dano. Vida: {vidaAtual}/{maxVida}");

        // VFX de dano
        if (vfxDano != null)
            Instantiate(vfxDano, transform.position, Quaternion.identity);

        // SFX de dano
        if (audioSource != null && somDano != null)
            audioSource.PlayOneShot(somDano);

        // Trigger de animação de hit
        if (anim != null)
            anim.SetTrigger("TomarDano");

        // Verifica transição para Fase 2
        if (!emFase2 && vidaAtual <= maxVida * limiarFase2)
        {
            EntrarFase2();
        }

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    private void EntrarFase2()
    {
        emFase2 = true;
        Debug.Log("[BossBarata] FASE 2 ATIVADA! Boss em modo fúria.");

        // Reduz os tempos de espera para deixar o boss mais agressivo
        tempoEntreAtaquesMin = Mathf.Max(0.5f, tempoEntreAtaquesMin * 0.6f);
        tempoEntreAtaquesMax = Mathf.Max(1.5f, tempoEntreAtaquesMax * 0.6f);
        quantidadeDeMiniBaratas = Mathf.RoundToInt(quantidadeDeMiniBaratas * 1.5f);

        if (anim != null)
            anim.SetTrigger("Fase2");
    }

    private void Morrer()
    {
        if (estaMorto) return;
        estaMorto = true;

        currentState = BossState.Morto;
        Debug.Log("[BossBarata] Boss Barata foi derrotado!");

        StopAllCoroutines();

        if (anim != null)
            anim.SetTrigger("Morrer");

        // VFX de morte
        if (vfxMorte != null)
            Instantiate(vfxMorte, transform.position, Quaternion.identity);

        // SFX de morte
        if (audioSource != null && somMorte != null)
            audioSource.PlayOneShot(somMorte);

        // Desativa colisores
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        // Opcional: Destruir após 5 segundos
        // Destroy(gameObject, 5f);
    }

    // ══════════════════════════════════════════════════════════════
    // CICLO DE BATALHA (CÉREBRO DO BOSS)
    // ══════════════════════════════════════════════════════════════
    IEnumerator CicloDeBatalha()
    {
        // Pequena pausa inicial antes do boss começar a atacar
        yield return new WaitForSeconds(1f);

        while (!estaMorto)
        {
            // Espera aleatória entre ataques
            float espera = Random.Range(tempoEntreAtaquesMin, tempoEntreAtaquesMax);
            yield return new WaitForSeconds(espera);

            if (estaMorto) yield break;

            // Escolhe e ESPERA o ataque terminar antes de continuar
            int escolha = Random.Range(1, 4); // Escolhe 1, 2 ou 3
            switch (escolha)
            {
                case 1: yield return StartCoroutine(Ataque1_EmboscadaSubterranea()); break;
                case 2: yield return StartCoroutine(Ataque2_EnxameMiniBaratas());    break;
                case 3: yield return StartCoroutine(Ataque3_TsunamiDeTerra());       break;
            }
        }
    }

    // ══════════════════════════════════════════════════════════════
    // ATAQUE 1: EMBOSCADA SUBTERRÂNEA
    // ══════════════════════════════════════════════════════════════
    IEnumerator Ataque1_EmboscadaSubterranea()
    {
        currentState = BossState.Atacando;
        Debug.Log("[BossBarata] Ataque 1: Emboscada Subterrânea!");

        if (anim != null) anim.SetTrigger("AtkEmboscada_Entrar");
        yield return new WaitForSeconds(tempoAnimacaoEntrar);

        // Torna o boss visualmente invisível (se tiver MeshRenderer)
        SetVisibilidade(false);

        // Grava posição do jogador como alvo (null-safe)
        Vector3 posicaoAlvo = jogador != null
            ? new Vector3(jogador.position.x, transform.position.y, jogador.position.z)
            : transform.position;

        // Spawna aviso visual
        GameObject avisoAtual = null;
        if (avisoDeBuracoPrefab != null)
            avisoAtual = Instantiate(avisoDeBuracoPrefab, posicaoAlvo, Quaternion.identity);

        yield return new WaitForSeconds(tempoAvisoEmboscada);

        // Move o boss para o ponto alvo e destrói o aviso
        transform.position = posicaoAlvo;
        if (avisoAtual != null) Destroy(avisoAtual);

        // Reaparece
        SetVisibilidade(true);

        if (anim != null) anim.SetTrigger("AtkEmboscada_Sair");

        // Causa dano em área ao emergir
        CausarDanoEmAreaErupcao();

        yield return new WaitForSeconds(tempoAnimacaoSair);

        currentState = BossState.Cooldown;
    }

    // ══════════════════════════════════════════════════════════════
    // ATAQUE 2: ENXAME DE MINI BARATAS
    // ══════════════════════════════════════════════════════════════
    IEnumerator Ataque2_EnxameMiniBaratas()
    {
        currentState = BossState.Atacando;
        Debug.Log("[BossBarata] Ataque 2: Enxame de Mini Baratas!");

        if (anim != null) anim.SetTrigger("AtkEnxame_Carregar");
        yield return new WaitForSeconds(0.8f);

        for (int i = 0; i < quantidadeDeMiniBaratas; i++)
        {
            if (estaMorto) yield break;

            if (miniBarataPrefab != null && pontoSpawnMiniBaratas != null)
            {
                GameObject baratinha = Instantiate(
                    miniBarataPrefab,
                    pontoSpawnMiniBaratas.position,
                    pontoSpawnMiniBaratas.rotation
                );

                // Se a mini barata tiver um script com SetAlvo, chama aqui:
                // MiniBarataIA ia = baratinha.GetComponent<MiniBarataIA>();
                // if (ia != null && jogador != null) ia.SetAlvo(jogador);
            }

            yield return new WaitForSeconds(tempoEntreSpawns);
        }

        yield return new WaitForSeconds(1.5f);

        currentState = BossState.Cooldown;
    }

    // ══════════════════════════════════════════════════════════════
    // ATAQUE 3: ONDA DE TERRA (TSUNAMI)
    // ══════════════════════════════════════════════════════════════
    IEnumerator Ataque3_TsunamiDeTerra()
    {
        currentState = BossState.Atacando;
        Debug.Log("[BossBarata] Ataque 3: Tsunami de Terra!");

        if (anim != null) anim.SetTrigger("AtkTsunami_Carregar");
        yield return new WaitForSeconds(1.5f);

        if (anim != null) anim.SetTrigger("AtkTsunami_Lancar");
        yield return new WaitForSeconds(0.2f);

        if (ondaDeTerraPrefab != null && pontoSpawnOnda != null)
        {
            Instantiate(ondaDeTerraPrefab, pontoSpawnOnda.position, pontoSpawnOnda.rotation);
        }
        else
        {
            if (ondaDeTerraPrefab == null)
                Debug.LogWarning("[BossBarata] 'ondaDeTerraPrefab' não está definido!");
            if (pontoSpawnOnda == null)
                Debug.LogWarning("[BossBarata] 'pontoSpawnOnda' não está definido!");
        }

        yield return new WaitForSeconds(2f);

        currentState = BossState.Cooldown;
    }

    // ══════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// Causa dano em área ao redor do boss (chamado ao emergir da terra).
    /// Pode também ser chamado por um Animation Event no Animator.
    /// </summary>
    public void CausarDanoEmAreaErupcao()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, raioErupcao);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // Adapta ao teu script de vida do jogador:
                // PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                // if (ph != null) ph.TomarDano(danoErupcao);

                Debug.Log($"[BossBarata] Erupção causou {danoErupcao} de dano ao Jogador!");
            }
        }
    }

    /// <summary>
    /// Liga ou desliga todos os Renderers filhos do boss (para o efeito de entrar/sair do chão).
    /// </summary>
    private void SetVisibilidade(bool visivel)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = visivel;
    }

    // ══════════════════════════════════════════════════════════════
    // GIZMOS (visualização no Editor do Unity)
    // ══════════════════════════════════════════════════════════════
    void OnDrawGizmosSelected()
    {
        // Raio de erupção (Ataque 1)
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.4f);
        Gizmos.DrawSphere(transform.position, raioErupcao);

        // Ponto spawn onda
        if (pontoSpawnOnda != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(pontoSpawnOnda.position, 0.3f);
        }

        // Ponto spawn mini baratas
        if (pontoSpawnMiniBaratas != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pontoSpawnMiniBaratas.position, 0.2f);
        }
    }
}