using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class BossIrmaoKingCube : MonoBehaviour
{
    [Header("=== STATUS DO BOSS ===")]
    public float vidaMax = 300f;
    public float vidaAtual;
    public float tempoEntreAtaques = 3f;
    private bool estaMorto = false;
    private bool estaAtacando = false;
    public bool EstaMorto => estaMorto;

    [Header("=== BOSS PARADO ===")]
    public bool manterNoLugar = true;
    public bool spriteOlhaParaDireita = false;
    private Vector2 posicaoFixa;

    [Header("=== ATAQUE 1: RAJADA DE MOEDAS ===")]
    public GameObject prefabProjetilMoeda;
    public Transform pontoDisparo;
    public float velocidadeMoeda = 10f;
    public int quantidadeMoedas = 6;
    public float intervaloMoedas = 0.22f;

    [Header("=== ATAQUE 2: LEQUE DE CUBOS ===")]
    public GameObject prefabProjetilCubo;
    public int quantidadeCubosLeque = 7;
    public float anguloLeque = 75f;
    public float velocidadeCubo = 8f;

    [Header("=== ATAQUE 3: CHUVA DE OURO ===")]
    public GameObject prefabPoteOuro;
    public int quantidadeChuvaOuro = 5;
    public float larguraChuva = 7f;
    public float alturaChuva = 7f;
    public float tempoAvisoChuva = 0.8f;
    public float velocidadeQueda = 12f;

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

        StartCoroutine(CicloBatalha());
    }

    private void Update()
    {
        if (estaMorto) return;

        if (manterNoLugar)
        {
            transform.position = new Vector3(posicaoFixa.x, posicaoFixa.y, transform.position.z);
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        if (!estaAtacando && jogador != null)
        {
            VirarParaJogador();
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

    IEnumerator CicloBatalha()
    {
        yield return new WaitForSeconds(2f);

        while (!estaMorto)
        {
            if (jogador != null && jogador.gameObject.activeInHierarchy)
            {
                int ataqueAleatorio = Random.Range(1, 4);

                if (ataqueAleatorio == 1)
                    yield return StartCoroutine(ExecutarRajadaMoedas());
                else if (ataqueAleatorio == 2)
                    yield return StartCoroutine(ExecutarLequeCubos());
                else
                    yield return StartCoroutine(ExecutarChuvaOuro());
            }

            yield return new WaitForSeconds(tempoEntreAtaques);
        }
    }

    IEnumerator ExecutarRajadaMoedas()
    {
        estaAtacando = true;
        Debug.Log("[Boss Irmao King Cube] Rajada de moedas!");

        Transform origem = pontoDisparo != null ? pontoDisparo : transform;

        for (int i = 0; i < quantidadeMoedas; i++)
        {
            if (jogador == null) break;

            Vector3 direcao = (jogador.position - origem.position).normalized;
            direcao += new Vector3(Random.Range(-0.12f, 0.12f), Random.Range(-0.06f, 0.06f), 0f);
            CriarProjetil(prefabProjetilMoeda, "BossGoldProjectile", origem.position, direcao.normalized, velocidadeMoeda, Color.yellow, 0.28f);

            yield return new WaitForSeconds(intervaloMoedas);
        }

        estaAtacando = false;
    }

    IEnumerator ExecutarLequeCubos()
    {
        estaAtacando = true;
        Debug.Log("[Boss Irmao King Cube] Leque de cubos!");

        Transform origem = pontoDisparo != null ? pontoDisparo : transform;
        Vector3 direcaoBase = jogador != null ? (jogador.position - origem.position).normalized : Vector3.left;
        float anguloBase = Mathf.Atan2(direcaoBase.y, direcaoBase.x) * Mathf.Rad2Deg;
        int quantidade = Mathf.Max(1, quantidadeCubosLeque);

        for (int i = 0; i < quantidade; i++)
        {
            float t = quantidade == 1 ? 0.5f : i / (float)(quantidade - 1);
            float angulo = anguloBase + Mathf.Lerp(-anguloLeque * 0.5f, anguloLeque * 0.5f, t);
            Vector3 direcao = new Vector3(Mathf.Cos(angulo * Mathf.Deg2Rad), Mathf.Sin(angulo * Mathf.Deg2Rad), 0f);
            CriarProjetil(prefabProjetilCubo, "BossCubeProjectile", origem.position, direcao, velocidadeCubo, Color.cyan, 0.35f);
        }

        yield return new WaitForSeconds(0.45f);
        estaAtacando = false;
    }

    IEnumerator ExecutarChuvaOuro()
    {
        estaAtacando = true;
        Debug.Log("[Boss Irmao King Cube] Chuva de ouro!");

        float yBase = jogador != null ? jogador.position.y : transform.position.y;

        for (int i = 0; i < quantidadeChuvaOuro; i++)
        {
            Vector3 alvo = jogador != null ? jogador.position : transform.position;
            alvo.x += Random.Range(-larguraChuva * 0.5f, larguraChuva * 0.5f);
            alvo.y = yBase;

            GameObject aviso = CriarAviso(alvo, Color.yellow, 1.4f);
            yield return new WaitForSeconds(tempoAvisoChuva);

            Vector3 spawn = alvo + Vector3.up * alturaChuva;
            CriarProjetil(prefabPoteOuro, "BossFallingGold", spawn, Vector3.down, velocidadeQueda, new Color(1f, 0.65f, 0f), 0.45f);

            if (aviso != null) Destroy(aviso);
            yield return new WaitForSeconds(0.18f);
        }

        estaAtacando = false;
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

    public void TomarDano(float dano)
    {
        if (estaMorto) return;

        vidaAtual -= dano;
        Debug.Log($"[Boss Irmao King Cube] Tomou {dano} de dano! Vida atual: {vidaAtual}/{vidaMax}");

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    private void Morrer()
    {
        estaMorto = true;
        StopAllCoroutines();
        Debug.Log("[Boss Irmao King Cube] Derrotado com coragem!");

        GameDatabase.Instance.AddPlets(200);
        TrocaCenaBoss.CarregarProximaCena();
        Destroy(gameObject, 1f);
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
