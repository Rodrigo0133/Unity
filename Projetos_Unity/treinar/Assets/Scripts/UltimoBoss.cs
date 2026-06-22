using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class UltimoBoss : MonoBehaviour
{
    [Header("=== STATUS DO BOSS ===")]
    public float vidaMax = 500f;
    public float vidaAtual;
    private bool estaMorto = false;
    public bool EstaMorto => estaMorto;
    private int etapaAtual = 1;

    [Header("=== BOSS PARADO ===")]
    public bool manterNoLugar = true;
    public bool spriteOlhaParaDireita = true;
    public float cooldownAtaques = 3f;
    private bool estaAtacando = false;
    private bool estaCansado = false;
    private bool enfurecido = false;
    private Vector2 posicaoFixa;

    [Header("=== PREFABS DOS ATAQUES ===")]
    public GameObject prefabBolaFogo;
    public GameObject prefabCorrente;
    public GameObject[] prefabsCabecasLideres;
    public GameObject prefabInimigoInvocado;
    public GameObject visualEscudoImunidade;

    [Header("=== ATAQUE: CHUVA DE METEOROS ===")]
    public int quantidadeMeteoros = 4;
    public float larguraChuva = 9f;
    public float alturaChuva = 8f;
    public float tempoAvisoMeteoro = 0.8f;
    public float velocidadeMeteoro = 13f;

    [Header("=== ATAQUE: RAJADA DIRETA ===")]
    public int quantidadeRajada = 6;
    public float velocidadeRajada = 12f;
    public float intervaloRajada = 0.18f;

    [Header("=== ATAQUE: LEQUE DE ENERGIA ===")]
    public int quantidadeLeque = 9;
    public float anguloLeque = 100f;
    public float velocidadeLeque = 9f;

    [Header("=== ATAQUE: PILARES NO CHAO ===")]
    public int quantidadePilares = 4;
    public float raioPilar = 1.4f;
    public float tempoAvisoPilar = 1f;
    public float danoPilar = 1f;

    [Header("=== ETAPA 2 ===")]
    public float fatorCrescimento = 1.6f;
    public int quantidadeEspiral = 16;
    public float velocidadeEspiral = 8f;
    public int quantidadeCabecas = 2;

    private readonly List<GameObject> inimigosVivos = new List<GameObject>();
    private bool estaImune = false;
    private Transform jogador;
    private Rigidbody2D rb;

    void Start()
    {
        vidaAtual = vidaMax;
        rb = GetComponent<Rigidbody2D>();
        posicaoFixa = transform.position;

        gameObject.tag = "Enemy";
        FixarBossNoLugar();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jogador = p.transform;

        if (visualEscudoImunidade != null) visualEscudoImunidade.SetActive(false);

        StartCoroutine(CicloBatalha());
    }

    void Update()
    {
        if (estaMorto) return;

        if (manterNoLugar)
        {
            transform.position = new Vector3(posicaoFixa.x, posicaoFixa.y, transform.position.z);
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        VirarParaJogador();
        AtualizarImunidade();
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

    IEnumerator CicloBatalha()
    {
        yield return new WaitForSeconds(2.5f);

        while (!estaMorto)
        {
            if (jogador != null && jogador.gameObject.activeInHierarchy && !estaAtacando && !estaCansado)
            {
                estaAtacando = true;

                float cooldownReal = enfurecido ? cooldownAtaques * 0.7f : cooldownAtaques;

                if (etapaAtual == 1)
                {
                    int ataque = Random.Range(1, 4);
                    if (ataque == 1)
                        yield return StartCoroutine(ExecutarChuvaMeteoros());
                    else if (ataque == 2)
                        yield return StartCoroutine(ExecutarRajadaDireta());
                    else
                        yield return StartCoroutine(ExecutarLequeEnergia());
                }
                else
                {
                    List<int> ataquesDisponiveis = new List<int> { 1, 2, 3, 4 };
                    if (enfurecido)
                    {
                        ataquesDisponiveis.Add(5);
                        ataquesDisponiveis.Add(6);
                    }

                    int ataque = ataquesDisponiveis[Random.Range(0, ataquesDisponiveis.Count)];

                    if (ataque == 1)
                        yield return StartCoroutine(ExecutarChuvaMeteoros());
                    else if (ataque == 2)
                        yield return StartCoroutine(ExecutarPilaresNoChao());
                    else if (ataque == 3)
                        yield return StartCoroutine(ExecutarEspiralEnergia());
                    else if (ataque == 4)
                        yield return StartCoroutine(ExecutarInvocacaoCabecas());
                    else if (ataque == 5)
                        yield return StartCoroutine(ExecutarRajadaDireta());
                    else
                        yield return StartCoroutine(ExecutarInvocacaoMinionsParados());
                }

                yield return new WaitForSeconds(cooldownReal);
                estaAtacando = false;
            }

            yield return null;
        }
    }

    IEnumerator ExecutarChuvaMeteoros()
    {
        Debug.Log("[Ultimo Boss] Chuva de meteoros!");

        int quantidade = enfurecido ? quantidadeMeteoros + 2 : quantidadeMeteoros;
        float yBase = jogador != null ? jogador.position.y : transform.position.y;

        for (int i = 0; i < quantidade; i++)
        {
            Vector3 alvo = jogador != null ? jogador.position : transform.position;
            alvo.x += Random.Range(-larguraChuva * 0.5f, larguraChuva * 0.5f);
            alvo.y = yBase;

            GameObject aviso = CriarAviso(alvo, new Color(1f, 0.2f, 0f), 1.7f);
            yield return new WaitForSeconds(tempoAvisoMeteoro);

            CriarProjetil(prefabBolaFogo, "BossMeteor", alvo + Vector3.up * alturaChuva, Vector3.down, velocidadeMeteoro, Color.red, 0.45f);

            if (aviso != null) Destroy(aviso);
            yield return new WaitForSeconds(0.15f);
        }
    }

    IEnumerator ExecutarRajadaDireta()
    {
        Debug.Log("[Ultimo Boss] Rajada direta!");

        for (int i = 0; i < quantidadeRajada; i++)
        {
            if (jogador == null) break;

            Vector3 origem = transform.position + Vector3.up * 0.6f;
            Vector3 direcao = (jogador.position - origem).normalized;
            direcao += new Vector3(Random.Range(-0.08f, 0.08f), Random.Range(-0.08f, 0.08f), 0f);

            CriarProjetil(prefabBolaFogo, "BossFireShot", origem, direcao.normalized, velocidadeRajada, new Color(1f, 0.35f, 0f), 0.36f);
            yield return new WaitForSeconds(intervaloRajada);
        }
    }

    IEnumerator ExecutarLequeEnergia()
    {
        Debug.Log("[Ultimo Boss] Leque de energia!");

        Vector3 origem = transform.position + Vector3.up * 0.4f;
        Vector3 direcaoBase = jogador != null ? (jogador.position - origem).normalized : Vector3.left;
        float anguloBase = Mathf.Atan2(direcaoBase.y, direcaoBase.x) * Mathf.Rad2Deg;
        int quantidade = Mathf.Max(1, quantidadeLeque);

        for (int i = 0; i < quantidade; i++)
        {
            float t = quantidade == 1 ? 0.5f : i / (float)(quantidade - 1);
            float angulo = anguloBase + Mathf.Lerp(-anguloLeque * 0.5f, anguloLeque * 0.5f, t);
            Vector3 direcao = new Vector3(Mathf.Cos(angulo * Mathf.Deg2Rad), Mathf.Sin(angulo * Mathf.Deg2Rad), 0f);
            CriarProjetil(prefabCorrente, "BossEnergyFan", origem, direcao, velocidadeLeque, Color.magenta, 0.32f);
        }

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator ExecutarPilaresNoChao()
    {
        Debug.Log("[Ultimo Boss] Pilares no chao!");

        for (int i = 0; i < quantidadePilares; i++)
        {
            Vector3 alvo = jogador != null ? jogador.position : transform.position;
            alvo.x += Random.Range(-4f, 4f);

            GameObject aviso = CriarAviso(alvo, Color.magenta, raioPilar * 2f);
            yield return new WaitForSeconds(tempoAvisoPilar);

            GameObject pilar = CriarAviso(alvo, new Color(0.7f, 0f, 1f), raioPilar * 2.2f);
            CausarDanoEmArea(alvo, raioPilar, danoPilar);

            if (aviso != null) Destroy(aviso);
            if (pilar != null) Destroy(pilar, 0.25f);
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator ExecutarEspiralEnergia()
    {
        Debug.Log("[Ultimo Boss] Espiral de energia!");

        Vector3 origem = transform.position + Vector3.up * 0.5f;
        int quantidade = enfurecido ? quantidadeEspiral + 8 : quantidadeEspiral;

        for (int i = 0; i < quantidade; i++)
        {
            float angulo = (360f / quantidade) * i;
            Vector3 direcao = new Vector3(Mathf.Cos(angulo * Mathf.Deg2Rad), Mathf.Sin(angulo * Mathf.Deg2Rad), 0f);
            CriarProjetil(prefabCorrente, "BossSpiralShot", origem, direcao, velocidadeEspiral, new Color(0.35f, 0.9f, 1f), 0.3f);
            yield return new WaitForSeconds(0.035f);
        }

        yield return new WaitForSeconds(0.35f);
    }

    IEnumerator ExecutarInvocacaoCabecas()
    {
        Debug.Log("[Ultimo Boss] Cabecas supremas a disparar!");

        for (int i = 0; i < quantidadeCabecas; i++)
        {
            Vector3 posSpawn = transform.position + new Vector3(Random.Range(-3.5f, 3.5f), 3.5f, 0f);
            GameObject cabeca = CriarCabeca(posSpawn);

            yield return new WaitForSeconds(0.55f);

            if (jogador != null && cabeca != null)
            {
                Vector3 direcao = (jogador.position - cabeca.transform.position).normalized;
                CriarProjetil(null, "LeaderSpell", cabeca.transform.position, direcao, 14f, Color.magenta, 0.35f);
            }

            if (cabeca != null) Destroy(cabeca, 0.4f);
            yield return new WaitForSeconds(0.25f);
        }
    }

    IEnumerator ExecutarInvocacaoMinionsParados()
    {
        Debug.Log("[Ultimo Boss] Totens de imunidade!");

        for (int i = 0; i < 2; i++)
        {
            Vector3 posSpawn = transform.position + new Vector3(Random.Range(-4f, 4f), 1.5f, 0f);
            GameObject inimigo = prefabInimigoInvocado != null ? Instantiate(prefabInimigoInvocado, posSpawn, Quaternion.identity) : CriarTotemFallback(posSpawn);
            CongelarObjetoInvocado(inimigo);
            inimigosVivos.Add(inimigo);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private void AtualizarImunidade()
    {
        inimigosVivos.RemoveAll(item => item == null);
        estaImune = inimigosVivos.Count > 0;

        if (visualEscudoImunidade != null)
            visualEscudoImunidade.SetActive(estaImune);
    }

    private void FixarBossNoLugar()
    {
        if (rb == null) return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private GameObject CriarProjetil(GameObject prefab, string nomeFallback, Vector3 posicao, Vector3 direcao, float velocidade, Color corFallback, float escalaFallback)
    {
        GameObject projetil = prefab != null ? Instantiate(prefab, posicao, Quaternion.identity) : CriarProjetilFallback(nomeFallback, posicao, corFallback, escalaFallback);

        Rigidbody2D rbProjetil = projetil.GetComponent<Rigidbody2D>();
        if (rbProjetil == null) rbProjetil = projetil.AddComponent<Rigidbody2D>();
        rbProjetil.gravityScale = 0f;
        rbProjetil.linearVelocity = direcao.normalized * velocidade;

        BossProjectileDamage dano = projetil.GetComponent<BossProjectileDamage>();
        if (dano == null) dano = projetil.AddComponent<BossProjectileDamage>();
        dano.dano = 1;

        Destroy(projetil, 5f);
        return projetil;
    }

    private GameObject CriarProjetilFallback(string nome, Vector3 posicao, Color cor, float escala)
    {
        GameObject projetil = new GameObject(nome);
        projetil.transform.position = posicao;
        projetil.transform.localScale = new Vector3(escala, escala, 1f);

        SpriteRenderer sr = projetil.AddComponent<SpriteRenderer>();
        sr.color = cor;
        sr.sprite = Resources.GetBuiltinResource<Sprite>("Knob.psd");

        CircleCollider2D cc = projetil.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;

        return projetil;
    }

    private GameObject CriarAviso(Vector3 posicao, Color cor, float escala)
    {
        GameObject aviso = new GameObject("AvisoAtaqueBoss");
        aviso.transform.position = posicao;
        aviso.transform.localScale = new Vector3(escala, escala, 1f);

        SpriteRenderer sr = aviso.AddComponent<SpriteRenderer>();
        sr.color = new Color(cor.r, cor.g, cor.b, 0.45f);
        sr.sprite = Resources.GetBuiltinResource<Sprite>("Knob.psd");

        return aviso;
    }

    private GameObject CriarCabeca(Vector3 posicao)
    {
        if (prefabsCabecasLideres != null && prefabsCabecasLideres.Length > 0)
        {
            return Instantiate(prefabsCabecasLideres[Random.Range(0, prefabsCabecasLideres.Length)], posicao, Quaternion.identity);
        }

        GameObject cabeca = new GameObject("SupremeLeaderHead");
        cabeca.transform.position = posicao;
        cabeca.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        SpriteRenderer sr = cabeca.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.5f, 0f, 0.5f);
        sr.sprite = Resources.GetBuiltinResource<Sprite>("Knob.psd");

        return cabeca;
    }

    private GameObject CriarTotemFallback(Vector3 posicao)
    {
        GameObject totem = new GameObject("TotemImunidadeBoss");
        totem.transform.position = posicao;
        totem.transform.localScale = new Vector3(0.8f, 1.2f, 1f);
        totem.tag = "Enemy";

        SpriteRenderer sr = totem.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.4f, 0f, 0.9f);
        sr.sprite = Resources.GetBuiltinResource<Sprite>("UIMask.psd");

        BoxCollider2D col = totem.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        Destroy(totem, 8f);
        return totem;
    }

    private void CongelarObjetoInvocado(GameObject objeto)
    {
        if (objeto == null) return;

        Rigidbody2D rbObjeto = objeto.GetComponent<Rigidbody2D>();
        if (rbObjeto == null) rbObjeto = objeto.AddComponent<Rigidbody2D>();

        rbObjeto.linearVelocity = Vector2.zero;
        rbObjeto.angularVelocity = 0f;
        rbObjeto.gravityScale = 0f;
        rbObjeto.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void CausarDanoEmArea(Vector2 posicao, float raio, float dano)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(posicao, raio);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerMovement pm = hit.GetComponent<PlayerMovement>();
                if (pm != null) pm.TakeDamage(dano);
            }
        }
    }

    public void TomarDano(float dano)
    {
        if (estaMorto) return;

        if (estaImune)
        {
            Debug.Log("[Ultimo Boss] IMUNE! Destroi os totens/inimigos primeiro.");
            return;
        }

        vidaAtual -= dano;
        Debug.Log($"[Ultimo Boss] Tomou {dano} de dano! Vida atual: {vidaAtual}/{vidaMax}");

        if (etapaAtual == 1 && vidaAtual <= (vidaMax / 2f))
        {
            TransitarParaEtapa2();
        }
        else if (etapaAtual == 2 && !estaCansado && !enfurecido && vidaAtual <= (vidaMax / 4f))
        {
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
        Debug.Log("[Ultimo Boss] Etapa 2! Ataques parados mais agressivos.");

        transform.localScale = transform.localScale * fatorCrescimento;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(1f, 0.5f, 0.5f);
    }

    IEnumerator ExecutarEstadoCansado()
    {
        estaCansado = true;
        Debug.Log("[Ultimo Boss] Ficou cansado por um tempo.");

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color original = sr != null ? sr.color : Color.white;
        if (sr != null) sr.color = Color.gray;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(4f);

        if (sr != null) sr.color = new Color(1f, 0.3f, 0.3f);
        estaCansado = false;
        enfurecido = true;
        Debug.Log("[Ultimo Boss] Recuperado e enfurecido!");
    }

    private void Morrer()
    {
        estaMorto = true;
        StopAllCoroutines();
        Debug.Log("[Ultimo Boss] Derrotado! O mundo esta salvo.");

        GameDatabase.Instance.AddPlets(400);
        TrocaCenaBoss.CarregarProximaCena();
        
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
