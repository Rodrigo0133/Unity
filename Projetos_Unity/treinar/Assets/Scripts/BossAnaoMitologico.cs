using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TipoMovimentoProjetil
{
    LinhaReta,
    LancarEPerseguir,
    ArcoParabolico
}

public class ProjetilBoss : MonoBehaviour
{
    public TipoMovimentoProjetil tipoMovimento = TipoMovimentoProjetil.LinhaReta;

    [Header("=== CONFIGURAÇÃO LANÇAR E PERSEGUIR ===")]
    public float tempoSubida = 0.6f;
    public float velocidadeSubida = 10f;
    public float velocidadePerseguicao = 12f;

    [Header("=== CONFIGURAÇÃO ARCO PARABÓLICO ===")]
    public float gravidadeSimulada = 15f;

    public float velocidade = 5f;
    public float tempoDeVida = 5f;
    private Vector3 direcao;
    
    // Todos os ataques tiram 1 de dano, como pedido
    private int dano = 1;

    // Controle interno do LancarEPerseguir
    private float tempoPassado = 0f;
    private bool emPerseguicao = false;
    private Vector3 direcaoSubida;
    private Vector3 direcaoPerseguicao;
    private Transform jogador;

    // Controle interno do Arco Parabólico
    private Vector3 posicaoInicial;
    private Vector3 posicaoAlvo;
    private float tempoTotalTrajeto;
    private float tempoTrajetoPercorrido = 0f;
    private float velocidadeHorizontal;
    private float velocidadeVerticalInicial;
    private bool arcoInicializado = false;

    void Start()
    {
        Destroy(gameObject, tempoDeVida);
        
        // Localiza o jogador
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jogador = p.transform;

        // Direção de subida com um pequeno espalhamento horizontal para o efeito chafariz
        direcaoSubida = new Vector3(Random.Range(-0.35f, 0.35f), 1f, 0f).normalized;
    }

    void Update()
    {
        if (tipoMovimento == TipoMovimentoProjetil.LinhaReta)
        {
            if (direcao != Vector3.zero)
            {
                transform.Translate(direcao.normalized * velocidade * Time.deltaTime, Space.World);
            }
        }
        else if (tipoMovimento == TipoMovimentoProjetil.LancarEPerseguir)
        {
            tempoPassado += Time.deltaTime;

            if (!emPerseguicao)
            {
                // Fase 1: Atira para o ar (desacelera gradualmente até parar no topo)
                float t = Mathf.Clamp01(tempoPassado / tempoSubida);
                float velAtual = Mathf.Lerp(velocidadeSubida, 0f, t);
                transform.Translate(direcaoSubida * velAtual * Time.deltaTime, Space.World);

                if (tempoPassado >= tempoSubida)
                {
                    emPerseguicao = true;
                    // Mira no jogador
                    if (jogador != null)
                    {
                        direcaoPerseguicao = (jogador.position - transform.position).normalized;
                    }
                    else
                    {
                        direcaoPerseguicao = Vector3.down;
                    }
                }
            }
            else
            {
                // Fase 2: Ataca o jogador
                transform.Translate(direcaoPerseguicao * velocidadePerseguicao * Time.deltaTime, Space.World);
            }
        }
        else if (tipoMovimento == TipoMovimentoProjetil.ArcoParabolico)
        {
            if (!arcoInicializado)
            {
                InicializarArco();
            }

            tempoTrajetoPercorrido += Time.deltaTime;
            float t = tempoTrajetoPercorrido;

            // Equações de física paramétrica para trajetória parabólica exata
            float novoX = posicaoInicial.x + velocidadeHorizontal * t;
            float novoY = posicaoInicial.y + (velocidadeVerticalInicial * t) - (0.5f * gravidadeSimulada * t * t);

            transform.position = new Vector3(novoX, novoY, transform.position.z);

            // Rotaciona o projétil para olhar na direção da trajetória atual
            float velYAtual = velocidadeVerticalInicial - gravidadeSimulada * t;
            Vector3 vetorVelocidade = new Vector3(velocidadeHorizontal, velYAtual, 0f);
            if (vetorVelocidade != Vector3.zero)
            {
                float angulo = Mathf.Atan2(vetorVelocidade.y, vetorVelocidade.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);
            }
        }
    }

    private void InicializarArco()
    {
        posicaoInicial = transform.position;
        if (jogador != null)
        {
            posicaoAlvo = jogador.position;
        }
        else
        {
            posicaoAlvo = posicaoInicial + Vector3.down * 3f;
        }

        float distanciaX = posicaoAlvo.x - posicaoInicial.x;
        // Evita divisões por zero e garante tempo mínimo de trajeto
        tempoTotalTrajeto = Mathf.Max(0.5f, Mathf.Abs(distanciaX) / velocidade);

        velocidadeHorizontal = distanciaX / tempoTotalTrajeto;
        
        // Vy0 = (y1 - y0) / T + 0.5 * g * T
        float diferencaY = posicaoAlvo.y - posicaoInicial.y;
        velocidadeVerticalInicial = (diferencaY / tempoTotalTrajeto) + (0.5f * gravidadeSimulada * tempoTotalTrajeto);

        arcoInicializado = true;
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
    public int vidaMax = 400;
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
                // Dispara moedas no ar (Lançar e Perseguir)
                DispararProjetil(prefabMoeda, origem, 8f, TipoMovimentoProjetil.LancarEPerseguir);
                yield return new WaitForSeconds(0.3f);
            }
        }
        else
        {
            // ATAQUE: Disparar Pote de Ouro (1 projétil)
            if (animAnao != null) animAnao.SetTrigger("AtacarPote");
            
            DispararProjetil(prefabPoteOuro, origem, 6f, TipoMovimentoProjetil.LinhaReta);
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
                DispararProjetil(prefabPedra, origem, 7f, TipoMovimentoProjetil.LinhaReta);
                yield return new WaitForSeconds(0.4f);
            }
        }
    }

    // ----------------------------------------------------
    // HELPERS
    // ----------------------------------------------------
    private void DispararProjetil(GameObject prefab, Transform origem, float velocidade, TipoMovimentoProjetil tipoMov = TipoMovimentoProjetil.LinhaReta)
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
            pb.tipoMovimento = tipoMov;
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
