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