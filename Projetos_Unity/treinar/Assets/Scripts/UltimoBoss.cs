using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ÚLTIMO BOSS (FINAL BOSS) - DUAS ETAPAS DE BATALHA
/// 
/// --- DESCRIÇÃO DETALHADA DOS ATAQUES ---
/// 
/// === 1ª ETAPA ===
/// 1. Chuva de Bolas de Fogo:
///    O Boss atira bolas de fogo vertiginosamente para cima. Instantes depois, surgem indicadores 
///    de área (AOE) vermelhos no chão. As bolas de fogo caem do céu nestas áreas, dando dano 
///    se o jogador estiver nelas.
/// 2. Onda Repulsora de Curto Alcance:
///    Se o jogador permanecer perto do Boss por muito tempo (ex: mais de 2 segundos), o Boss 
///    liberta imediatamente uma onda de choque de curto alcance (AOE) que empurra o jogador 
///    e causa dano, impedindo que o jogador abuse de ataques corpo a corpo.
/// 
/// === 2ª ETAPA (Boss cresce e fica irritado) ===
/// 3. Lançamento de Correntes:
///    O Boss dispara correntes afiadas na horizontal que varrem o mapa rente ao chão. 
///    O jogador deve saltar para desviar.
/// 4. Invocação de Cabeça de Líder Supremo:
///    O Boss invoca aleatoriamente a cabeça flutuante de um dos seus 3 Líderes Supremos. 
///    A cabeça realiza um disparo carregado (como uma rajada de energia) na direção do jogador e desaparece.
/// 
/// === 2ª ETAPA - METADE DA VIDA (Fase Cansada e Enfurecida) ===
/// 5. Estado Cansado (50% Vida):
///    Ao chegar a metade da vida na Etapa 2, o Boss fica exausto e imóvel por alguns segundos (atordoado), 
///    ficando vulnerável.
/// 6. Modo Fúria Aceleração:
///    Após o cansaço, todos os ataques anteriores são desferidos com velocidade de cooldown acelerada em 30%.
/// 7. Novos Ataques Adicionados:
///    - Rajada de Bolas de Fogo Rápidas: Dispara consecutivamente várias bolas de fogo para o jogador desviar.
///    - Invocação de Lacaios & Imunidade: O Boss arremessa inimigos no mapa. Enquanto estes inimigos 
///      estiverem vivos, o Boss fica envolto num escudo de imunidade que anula qualquer dano recebido.
/// </summary>
public class UltimoBoss : MonoBehaviour
{
    [Header("=== STATUS DO BOSS ===")]
    public float vidaMax = 500f;
    public float vidaAtual;
    private bool estaMorto = false;
    private int etapaAtual = 1;

    [Header("=== TIMING E ESTADOS ===")]
    public float cooldownAtaques = 3f;
    private bool estaAtacando = false;
    private bool estaCansado = false;
    private bool enfurecido = false;

    [Header("=== ETAPA 1: CHUVA DE FOGO & REPEL ===")]
    public GameObject prefabBolaFogo;
    public float rangeRepel = 2.5f;
    private float tempoPertoDoBoss = 0f;

    [Header("=== ETAPA 2: CORRENTES E LÍDERES ===")]
    public float fatorCrescimento = 1.6f;
    public GameObject prefabCorrente;
    public GameObject[] prefabsCabecasLideres;

    [Header("=== ETAPA 2 ENFURECIDA: IMUNIDADE ===")]
    public GameObject prefabInimigoInvocado; // Inimigo comum a derrotar
    public GameObject visualEscudoImunidade;
    private List<GameObject> inimigosVivos = new List<GameObject>();
    private bool estaImune = false;

    private Transform jogador;
    private Rigidbody2D rb;

    void Start()
    {
        vidaAtual = vidaMax;
        rb = GetComponent<Rigidbody2D>();

        gameObject.tag = "Enemy";

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jogador = p.transform;

        if (visualEscudoImunidade != null) visualEscudoImunidade.SetActive(false);

        StartCoroutine(CicloBatalha());
    }

    void Update()
    {
        if (estaMorto) return;

        // Monitoriza a proximidade do jogador na Etapa 1
        if (etapaAtual == 1 && jogador != null)
        {
            float dist = Vector2.Distance(transform.position, jogador.position);
            if (dist <= rangeRepel)
            {
                tempoPertoDoBoss += Time.deltaTime;
                if (tempoPertoDoBoss >= 2f)
                {
                    // Força um ataque de repel imediato
                    tempoPertoDoBoss = 0f;
                    StartCoroutine(ExecutarAtaqueRepel());
                }
            }
            else
            {
                tempoPertoDoBoss = Mathf.Max(0f, tempoPertoDoBoss - Time.deltaTime);
            }
        }

        // Atualiza a imunidade baseado nos minions vivos
        if (inimigosVivos.Count > 0)
        {
            // Limpa referências nulas
            inimigosVivos.RemoveAll(item => item == null);
            if (inimigosVivos.Count > 0)
            {
                estaImune = true;
                if (visualEscudoImunidade != null) visualEscudoImunidade.SetActive(true);
            }
            else
            {
                estaImune = false;
                if (visualEscudoImunidade != null) visualEscudoImunidade.SetActive(false);
                Debug.Log("[Último Boss] Escudo quebrado! Minions derrotados.");
            }
        }
        else
        {
            estaImune = false;
            if (visualEscudoImunidade != null) visualEscudoImunidade.SetActive(false);
        }
    }

    IEnumerator CicloBatalha()
    {
        yield return new WaitForSeconds(2.5f);

        while (!estaMorto)
        {
            if (jogador != null && jogador.gameObject.activeInHierarchy && !estaAtacando && !estaCansado)
            {
                estaAtacando = true;
                
                // Reduz tempo entre ataques no modo enfurecido
                float cooldownReal = enfurecido ? cooldownAtaques * 0.7f : cooldownAtaques;

                if (etapaAtual == 1)
                {
                    // Apenas Chuva de Bolas de Fogo (e o repel acontece passivamente no Update)
                    yield return StartCoroutine(ExecutarChuvaDeFogo());
                }
                else
                {
                    // Etapa 2
                    List<int> ataquesDisponiveis = new List<int> { 1, 2 }; // 1=Correntes, 2=Cabeças
                    if (enfurecido)
                    {
                        ataquesDisponiveis.Add(3); // Rajada de Fogo
                        ataquesDisponiveis.Add(4); // Invocação de Minions
                    }

                    int ataqueEscolhido = ataquesDisponiveis[Random.Range(0, ataquesDisponiveis.Count)];

                    if (ataqueEscolhido == 1)
                        yield return StartCoroutine(ExecutarCorrenteHorizontal());
                    else if (ataqueEscolhido == 2)
                        yield return StartCoroutine(ExecutarInvocacaoLider());
                    else if (ataqueEscolhido == 3)
                        yield return StartCoroutine(ExecutarRajadaRapidaFogo());
                    else
                        yield return StartCoroutine(ExecutarInvocacaoMinions());
                }

                yield return new WaitForSeconds(cooldownReal);
                estaAtacando = false;
            }
            yield return null;
        }
    }

    // --- ATAQUES ETAPAS 1 & 2 ---

    IEnumerator ExecutarChuvaDeFogo()
    {
        Debug.Log("[Último Boss] Chuva de Bolas de Fogo!");
        
        // Posições de queda no chão
        int numBolas = enfurecido ? 5 : 3;
        for (int i = 0; i < numBolas; i++)
        {
            if (jogador == null) break;

            // Mira perto do jogador ou aleatório no mapa
            Vector3 posAlvo = jogador.position + new Vector3(Random.Range(-5f, 5f), -2f, 0f);
            posAlvo.y = transform.position.y - 3f; // Nível do chão aproximado

            // Cria aviso AOE na posição alvo
            GameObject aviso = new GameObject("AvisoAOE");
            aviso.transform.position = posAlvo;
            aviso.transform.localScale = Vector3.zero;
            SpriteRenderer sr = aviso.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.2f, 0f, 0.5f);
            sr.sprite = Resources.GetBuiltinResource<Sprite>("Knob.psd");

            // Efeito visual do aviso expandindo
            float tempoAviso = 1.2f;
            float decorrido = 0f;
            while (decorrido < tempoAviso)
            {
                decorrido += Time.deltaTime;
                float escala = Mathf.Lerp(0f, 2.5f, decorrido / tempoAviso);
                aviso.transform.localScale = new Vector3(escala, escala, 1f);
                yield return null;
            }

            // Cria bola de fogo vinda do céu
            GameObject fogo = Instantiate(prefabBolaFogo != null ? prefabBolaFogo : new GameObject("FallbackFireball"), posAlvo + Vector3.up * 8f, Quaternion.identity);
            if (prefabBolaFogo == null)
            {
                fogo.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                SpriteRenderer srf = fogo.AddComponent<SpriteRenderer>();
                srf.color = Color.red;
                srf.sprite = Resources.GetBuiltinResource<Sprite>("Knob.psd");
                fogo.AddComponent<CircleCollider2D>().isTrigger = true;
            }

            // Desce rapidamente
            Rigidbody2D rbf = fogo.GetComponent<Rigidbody2D>();
            if (rbf == null) rbf = fogo.AddComponent<Rigidbody2D>();
            rbf.gravityScale = 0f;
            rbf.linearVelocity = Vector2.down * 12f;

            // Adiciona dano
            BossProjectileDamage bpd = fogo.GetComponent<BossProjectileDamage>();
            if (bpd == null) bpd = fogo.AddComponent<BossProjectileDamage>();
            bpd.dano = 1;

            Destroy(aviso);
            Destroy(fogo, 3f);
            yield return new WaitForSeconds(0.4f);
        }
    }

    IEnumerator ExecutarAtaqueRepel()
    {
        estaAtacando = true;
        Debug.Log("[Último Boss] Ataque repulsor por proximidade prolongada!");

        GameObject wave = new GameObject("RepelShockwave");
        wave.transform.position = transform.position;
        wave.transform.localScale = Vector3.zero;
        SpriteRenderer sr = wave.AddComponent<SpriteRenderer>();
        sr.color = new Color(1f, 0.5f, 0f, 0.6f);
        sr.sprite = Resources.GetBuiltinResource<Sprite>("Knob.psd");

        float tempo = 0.5f;
        float decorrido = 0f;
        bool jogadorRepelido = false;

        while (decorrido < tempo)
        {
            decorrido += Time.deltaTime;
            float escala = Mathf.Lerp(0f, rangeRepel * 2.5f, decorrido / tempo);
            wave.transform.localScale = new Vector3(escala, escala, 1f);

            if (jogador != null && !jogadorRepelido)
            {
                float dist = Vector2.Distance(transform.position, jogador.position);
                if (dist <= (escala / 2f))
                {
                    jogadorRepelido = true;
                    PlayerMovement pm = jogador.GetComponent<PlayerMovement>();
                    if (pm != null) pm.TakeDamage(1);

                    Rigidbody2D rbPlayer = jogador.GetComponent<Rigidbody2D>();
                    if (rbPlayer != null)
                    {
                        Vector2 direcao = (jogador.position - transform.position).normalized;
                        rbPlayer.linearVelocity = new Vector2(direcao.x * 16f, 10f); // Forte empurrão
                    }
                }
            }
            yield return null;
        }

        Destroy(wave, 0.1f);
        estaAtacando = false;
    }

    IEnumerator ExecutarCorrenteHorizontal()
    {
        Debug.Log("[Último Boss] Ataque de Correntes horizontais!");
        float direcao = (jogador != null && jogador.position.x > transform.position.x) ? 1f : -1f;

        GameObject corrente = Instantiate(prefabCorrente != null ? prefabCorrente : new GameObject("FallbackChain"), transform.position + new Vector3(direcao * 2f, -1.5f, 0), Quaternion.identity);
        if (prefabCorrente == null)
        {
            corrente.transform.localScale = new Vector3(1.5f, 0.3f, 1f);
            SpriteRenderer sr = corrente.AddComponent<SpriteRenderer>();
            sr.color = Color.gray;
            sr.sprite = Resources.GetBuiltinResource<Sprite>("UIMask.psd");
            BoxCollider2D bc = corrente.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;
        }

        Rigidbody2D rbc = corrente.GetComponent<Rigidbody2D>();
        if (rbc == null) rbc = corrente.AddComponent<Rigidbody2D>();
        rbc.gravityScale = 0f;
        rbc.linearVelocity = new Vector2(direcao * 8f, 0f);

        BossProjectileDamage bpd = corrente.GetComponent<BossProjectileDamage>();
        if (bpd == null) bpd = corrente.AddComponent<BossProjectileDamage>();
        bpd.dano = 1;

        Destroy(corrente, 4f);
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator ExecutarInvocacaoLider()
    {
        Debug.Log("[Último Boss] Invocando Cabeça de Líder Supremo!");

        Vector3 posSpawn = transform.position + new Vector3(Random.Range(-3f, 3f), 4f, 0f);
        GameObject cabeca = null;

        if (prefabsCabecasLideres != null && prefabsCabecasLideres.Length > 0)
        {
            cabeca = Instantiate(prefabsCabecasLideres[Random.Range(0, prefabsCabecasLideres.Length)], posSpawn, Quaternion.identity);
        }
        else
        {
            cabeca = new GameObject("SupremeLeaderHead");
            cabeca.transform.position = posSpawn;
            cabeca.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            SpriteRenderer sr = cabeca.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.5f, 0f, 0.5f); // Cor Roxa mística
            sr.sprite = Resources.GetBuiltinResource<Sprite>("Knob.psd");
        }

        yield return new WaitForSeconds(1f); // Tempo de carregamento

        if (jogador != null && cabeca != null)
        {
            // Dispara um poder super rápido
            Vector3 dir = (jogador.position - cabeca.transform.position).normalized;
            GameObject spell = new GameObject("LeaderSpell");
            spell.transform.position = cabeca.transform.position;
            spell.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            SpriteRenderer srs = spell.AddComponent<SpriteRenderer>();
            srs.color = Color.magenta;
            srs.sprite = Resources.GetBuiltinResource<Sprite>("Knob.psd");

            Rigidbody2D rbs = spell.AddComponent<Rigidbody2D>();
            rbs.gravityScale = 0f;
            rbs.linearVelocity = dir * 14f;

            BossProjectileDamage bpd = spell.AddComponent<BossProjectileDamage>();
            bpd.dano = 1;

            Destroy(spell, 4f);
        }

        Destroy(cabeca, 0.5f);
        yield return new WaitForSeconds(0.6f);
    }

    IEnumerator ExecutarRajadaRapidaFogo()
    {
        Debug.Log("[Último Boss] Rajada rápida de bolas de fogo!");

        for (int i = 0; i < 6; i++)
        {
            if (jogador == null) break;

            Vector3 dir = (jogador.position - transform.position).normalized;
            GameObject fogo = Instantiate(prefabBolaFogo != null ? prefabBolaFogo : new GameObject("FallbackFireball"), transform.position + dir * 1.5f, Quaternion.identity);
            if (prefabBolaFogo == null)
            {
                fogo.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
                SpriteRenderer srf = fogo.AddComponent<SpriteRenderer>();
                srf.color = new Color(1f, 0.3f, 0f);
                srf.sprite = Resources.GetBuiltinResource<Sprite>("Knob.psd");
                fogo.AddComponent<CircleCollider2D>().isTrigger = true;
            }

            Rigidbody2D rbf = fogo.GetComponent<Rigidbody2D>();
            if (rbf == null) rbf = fogo.AddComponent<Rigidbody2D>();
            rbf.gravityScale = 0f;
            rbf.linearVelocity = dir * 13f;

            BossProjectileDamage bpd = fogo.GetComponent<BossProjectileDamage>();
            if (bpd == null) bpd = fogo.AddComponent<BossProjectileDamage>();
            bpd.dano = 1;

            Destroy(fogo, 3f);
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator ExecutarInvocacaoMinions()
    {
        Debug.Log("[Último Boss] Invocando lacaios para obter imunidade!");

        for (int i = 0; i < 2; i++)
        {
            Vector3 posSpawn = transform.position + new Vector3(Random.Range(-4f, 4f), 2f, 0f);
            GameObject inimigo = null;

            if (prefabInimigoInvocado != null)
            {
                inimigo = Instantiate(prefabInimigoInvocado, posSpawn, Quaternion.identity);
            }
            else
            {
                // Fallback: spawn a BarataInimigo programmatically if we find its component
                inimigo = new GameObject("InvocadoBarata");
                inimigo.transform.position = posSpawn;
                SpriteRenderer sr = inimigo.AddComponent<SpriteRenderer>();
                sr.color = Color.magenta;
                // Add script BarataInimigo which has its own AI perseguição
                BarataInimigo bi = inimigo.AddComponent<BarataInimigo>();
                bi.distanciaAtivacao = 15f;
                bi.velocidade = 3f;
                bi.vida = 1;
            }

            inimigosVivos.Add(inimigo);
        }

        yield return new WaitForSeconds(0.5f);
    }

    // --- RECEBER DANO ---

    public void TomarDano(float dano)
    {
        if (estaMorto) return;

        if (estaImune)
        {
            Debug.Log("[Último Boss] IMUNE! Derrota os inimigos primeiro.");
            return;
        }

        vidaAtual -= dano;
        Debug.Log($"[Último Boss] Tomou {dano} de dano! Vida atual: {vidaAtual}/{vidaMax}");

        // Verifica transição de etapas
        if (etapaAtual == 1 && vidaAtual <= (vidaMax / 2f))
        {
            TransitarParaEtapa2();
        }
        else if (etapaAtual == 2 && !estaCansado && !enfurecido && vidaAtual <= (vidaMax / 4f))
        {
            // Metade da vida da Etapa 2 (que é 25% da vida máxima total)
            StartCoroutine(ExecutarEstadoCansado());
        }

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    private void TransitarParaEtapa2()
    {
        etapaAtual = 2;
        Debug.Log("[Último Boss] TRANSITANDO PARA A ETAPA 2! O Boss cresce e fica irritado.");

        // O boss cresce
        transform.localScale = transform.localScale * fatorCrescimento;

        // Fica avermelhado de raiva
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(1f, 0.5f, 0.5f);
    }

    IEnumerator ExecutarEstadoCansado()
    {
        estaCansado = true;
        Debug.Log("[Último Boss] Ficou cansado! Parado por um tempo.");

        // Visual de cansado (cor mais escura ou acinzentada)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color original = sr != null ? sr.color : Color.white;
        if (sr != null) sr.color = Color.gray;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(4f); // Fica parado cansado por 4s

        if (sr != null) sr.color = new Color(1f, 0.3f, 0.3f); // Cor enfurecida total
        estaCansado = false;
        enfurecido = true;
        Debug.Log("[Último Boss] RECUPERADO E ENFURECIDO! Ataques mais rápidos e novos poderes desbloqueados.");
    }

    private void Morrer()
    {
        estaMorto = true;
        StopAllCoroutines();
        Debug.Log("[Último Boss] Derrotado! O mundo está salvo.");

        // Dropa montanhas de Plets
        PletCoin.Spawn(transform.position, Random.Range(30, 50));

        Destroy(gameObject, 1.5f);
    }

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
