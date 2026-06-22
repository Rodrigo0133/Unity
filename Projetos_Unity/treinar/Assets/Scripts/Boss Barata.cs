using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    
    private Animator anim;
    private Transform jogador;
    private bool estaMorto = false;
    private bool emFase2 = false;
    private bool invencivel = false;
    private bool batalhaIniciada = false;
    public bool EstaMorto => estaMorto;

    
    [Header("═══ STATUS DO BOSS ═══")]
    [Tooltip("Se verdadeiro, o boss só começa a atacar quando IniciarBatalha() for chamado.")]
    public bool esperarTriggerParaComecar = true;

    [Tooltip("Vida máxima do Boss.")]
    public int maxVida = 300;

    [Tooltip("Vida atual do Boss (preenchido automaticamente no Start).")]
    public int vidaAtual;

    [Tooltip("Estado actual do Boss (útil para debug no Inspector).")]
    public BossState currentState = BossState.Idle;

    
    [Header("═══ REFERÊNCIAS ═══")]
    [Tooltip("Animator do Boss. Preenchido automaticamente se deixado vazio.")]
    public Animator animatorExterno;

    [Tooltip("Transform do Jogador. Preenchido automaticamente pela tag 'Player' se deixado vazio.")]
    public Transform jogadorExterno;
    public bool spriteOlhaParaDireita = true;

   
    [Header("═══ IA / TIMING ═══")]
    [Tooltip("Tempo mínimo de espera entre ataques (Fase 1).")]
    public float tempoEntreAtaquesMin = 2f;

    [Tooltip("Tempo máximo de espera entre ataques (Fase 1).")]
    public float tempoEntreAtaquesMax = 4.5f;

    [Tooltip("Percentagem de vida (0-1) em que o boss entra na Fase 2 (modo fúria).")]
    [Range(0f, 1f)]
    public float limiarFase2 = 0.25f;

    
    [Header("═══ ATAQUE 1: EMBOSCADA ═══")]
    [Tooltip("Prefab visual de aviso no chão (ex: círculo vermelho).")]
    public GameObject avisoDeBuracoPrefab;

    [Tooltip("Tempo (em segundos) que o aviso segue o jogador.")]
    public float tempoAvisoSegueJogador = 3f;

    [Tooltip("Tempo (em segundos) que o aviso fica parado no chão antes de dar dano.")]
    public float tempoAvisoParado = 1f;

    [Tooltip("Tempo da animação de entrar no chão.")]
    public float tempoAnimacaoEntrar = 1f;

    [Tooltip("Tempo da animação de sair do chão.")]
    public float tempoAnimacaoSair = 2f;

    [Tooltip("Raio do dano em área ao emergir.")]
    public float raioErupcao = 2.5f;

    [Tooltip("Dano causado pela erupção.")]
    public int danoErupcao = 1;

    
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
    // INSPECTOR - DEBUG / TESTES
    // ══════════════════════════════════════════════════════════════
    [Header("═══ DEBUG / TESTES ═══")]
    [Tooltip("Se verdadeiro, o boss perde vida sozinho com o tempo. Útil para testar sem ter um jogador pronto.")]
    public bool bossMorreSozinho = true;

    [Tooltip("Podes pressionar a barra de ESPAÇO para dar dano ao boss manualmente.")]
    public bool danoPeloEspaco = true;

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

        // AUTO-CONFIGURAÇÃO 2D: Garante que o Boss tem tudo o que precisa
        gameObject.tag = "Enemy";

        // Garante que tem um Collider2D para a espada do jogador o detetar
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            Debug.Log("[BossBarata] BoxCollider2D adicionado automaticamente!");
        }

        // Garante que tem um Rigidbody2D (necessário para colisões 2D funcionarem)
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // Não cai nem é empurrado
            Debug.Log("[BossBarata] Rigidbody2D adicionado automaticamente!");
        }

        // Resolve referências: usa as externas se definidas, senão busca automaticamente
        anim = (animatorExterno != null) ? animatorExterno : GetComponent<Animator>();

        // Desativar root motion para evitar que a barata ande sozinha para lados aleatórios por causa da animação
        if (anim != null) anim.applyRootMotion = false;

        if (jogadorExterno != null)
        {
            jogador = jogadorExterno;
        }
        else
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                jogador = p.transform;
            }
            else
            {
                // FALLBACK: Para testes rápidos sem configurar o Player
                GameObject[] objetos = FindObjectsOfType<GameObject>();
                foreach (GameObject obj in objetos)
                {
                    string n = obj.name.ToLower();
                    // Ignora o próprio boss
                    if ((n.Contains("square") || n.Contains("quadrado") || n.Contains("cube") || n.Contains("cubo")) && obj.transform != this.transform)
                    {
                        jogador = obj.transform;
                        Debug.Log($"[BossBarata] Jogador definido automaticamente como '{obj.name}'!");
                        break;
                    }
                }

                if (jogador == null)
                {
                    Debug.LogWarning("[BossBarata] Nenhum jogador encontrado. O boss vai atacar a própria posição.");
                }
            }
        }

        if (!esperarTriggerParaComecar)
        {
            IniciarBatalha();
        }
    }

    public void IniciarBatalha()
    {
        if (batalhaIniciada || estaMorto) return;
        batalhaIniciada = true;
        
        Debug.Log("[BossBarata] Batalha Iniciada!");
        StartCoroutine(CicloDeBatalha());

        if (bossMorreSozinho)
        {
            StartCoroutine(RotinaMorteSozinho());
        }
    }

    void Update()
    {
        VirarParaJogador();

        // Debug para facilitar testes (dar dano com a tecla Espaço)
        if (danoPeloEspaco && Input.GetKeyDown(KeyCode.Space))
        {
            TomarDano(5);
        }
    }

    IEnumerator RotinaMorteSozinho()
    {
        while (!estaMorto)
        {
            yield return new WaitForSeconds(3f); // Tira 5 de vida a cada 3 segundos
            if (!estaMorto)
            {
                Debug.Log("[BossBarata] TESTE: O boss perdeu vida sozinho!");
                TomarDano(5);
            }
        }
    }

    private void TocarAnimacao(string triggerName)
    {
        // Só tenta tocar animação se o Animator existir e tiver um Controller válido (evita erros se for só um quadrado)
        if (anim != null && anim.runtimeAnimatorController != null && anim.gameObject.activeInHierarchy)
        {
            anim.SetTrigger(triggerName);
        }
    }

    private void VirarParaJogador()
    {
        if (estaMorto || jogador == null) return;

        float direcao = jogador.position.x >= transform.position.x ? 1f : -1f;
        if (!spriteOlhaParaDireita) direcao *= -1f;

        Vector3 escala = transform.localScale;
        escala.x = Mathf.Abs(escala.x) * direcao;
        transform.localScale = escala;
    }

    // ══════════════════════════════════════════════════════════════
    // SISTEMA DE VIDA E MORTE
    // ══════════════════════════════════════════════════════════════

    /// <summary>Causa dano ao Boss. Chama este método de outros scripts (projéteis, espada, etc.).</summary>
    public void TomarDano(int dano)
    {
        if (estaMorto || invencivel) return;

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
        TocarAnimacao("TomarDano");

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

        TocarAnimacao("Fase2");
    }

    private void Morrer()
    {
        if (estaMorto) return;
        estaMorto = true;

        currentState = BossState.Morto;
        Debug.Log("[BossBarata] Boss Barata foi derrotado!");

        StopAllCoroutines();
        GameDatabase.Instance.data.plets += 200;
        TrocaCenaBoss.CarregarProximaCena();
        TocarAnimacao("Morrer");

        // VFX de morte
        if (vfxMorte != null)
            Instantiate(vfxMorte, transform.position, Quaternion.identity);

        // SFX de morte
        if (audioSource != null && somMorte != null)
            audioSource.PlayOneShot(somMorte);
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;
        
        Destroy(gameObject, 2f);
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
            int escolha = Random.Range(1, 3); // Escolhe 1 ou 2
            switch (escolha)
            {
                case 1: yield return StartCoroutine(Ataque1_EmboscadaSubterranea()); break;
                case 2: yield return StartCoroutine(Ataque2_EnxameMiniBaratas());    break;
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

        TocarAnimacao("AtkEmboscada_Entrar");
        yield return new WaitForSeconds(tempoAnimacaoEntrar);

        // Torna o boss visualmente invisível (se tiver MeshRenderer) e invencível
        SetVisibilidade(false);
        invencivel = true;

        // Calcula a altura do chão baseada nos pés do Boss antes de ele desaparecer
        float nivelDoChaoY = transform.position.y;
        Collider2D colBoss = GetComponent<Collider2D>();
        if (colBoss != null)
        {
            nivelDoChaoY = colBoss.bounds.min.y;
        }

        // Posição alvo inicial foca no X do jogador e no Y do chão
        Vector3 posicaoAlvo = new Vector3(transform.position.x, nivelDoChaoY, 0f);
        if (jogador != null)
        {
            posicaoAlvo = new Vector3(jogador.position.x, nivelDoChaoY, 0f);
        }

        // Spawna aviso visual no chão (debaixo do jogador)
        GameObject avisoAtual = null;
        if (avisoDeBuracoPrefab != null)
            avisoAtual = Instantiate(avisoDeBuracoPrefab, posicaoAlvo, Quaternion.identity);

        // FASE 1: O círculo segue apenas o Eixo X do jogador, mantendo-se preso ao chão
        float tempoRestante = tempoAvisoSegueJogador;
        while (tempoRestante > 0)
        {
            tempoRestante -= Time.deltaTime;
            
            // Segue o jogador apenas na horizontal (X)
            if (jogador != null && avisoAtual != null)
            {
                avisoAtual.transform.position = new Vector3(jogador.position.x, nivelDoChaoY, 0f);
            }
            
            yield return null; // Espera pela próxima frame
        }

        // Atualiza a posição final para onde o círculo parou
        if (avisoAtual != null)
            posicaoAlvo = avisoAtual.transform.position;
        else if (jogador != null)
            posicaoAlvo = jogador.position;

        // FASE 2: O círculo para e avisa por mais Y segundos antes do ataque
        yield return new WaitForSeconds(tempoAvisoParado);

        // Destrói o aviso, mas O BOSS FICA NO MESMO LUGAR (não se move para posicaoAlvo)
        if (avisoAtual != null) Destroy(avisoAtual);

        // Reaparece (onde sempre esteve) e volta a poder levar dano
        SetVisibilidade(true);
        invencivel = false;

        TocarAnimacao("AtkEmboscada_Sair");

        // Causa dano em área no ponto EXATO onde o aviso parou
        CausarDanoEmAreaErupcao(posicaoAlvo);

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

        TocarAnimacao("AtkEnxame_Carregar");
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
    // HELPERS
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// Causa dano em área 2D na posição indicada.
    /// </summary>
    public void CausarDanoEmAreaErupcao(Vector2 posicaoDano)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(posicaoDano, raioErupcao);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerMovement pm = hit.GetComponent<PlayerMovement>();
                if (pm != null)
                {
                    pm.TakeDamage(danoErupcao);
                    Debug.Log($"[BossBarata] Erupção causou {danoErupcao} de dano ao Jogador!");
                }
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

        // Ponto spawn mini baratas
        if (pontoSpawnMiniBaratas != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pontoSpawnMiniBaratas.position, 0.2f);
        }
    }
}
