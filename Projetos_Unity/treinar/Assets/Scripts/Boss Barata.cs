using System.Collections;
using UnityEngine;

public class BossBarata : MonoBehaviour
{
    // --- ESTADOS DO BOSS ---
    // Uma máquina de estados simples para saber o que o boss está fazendo
    public enum BossState
    {
        Idle,              // Boss parado/andando, esperando para atacar
        PreparandoAtaque,  // Boss decidiu qual ataque fazer
        Atacando,          // Boss executando o ataque (não pode ser interrompido)
        Cooldown           // Tempo de recuperação após um ataque
    }

    [Header("Status do Boss")]
    [Tooltip("Vida máxima do Boss. Você mencionou 25, ajustei para 35 para ser um desafio justo.")]
    public int maxVida = 35;
    [Tooltip("Vida atual do Boss.")]
    public int vidaAtual;
    private bool estaMorto = false;

    [Header("Mecânicas Gerais / IA")]
    [Tooltip("Estado atual do boss.")]
    public BossState currentState = BossState.Idle;
    
    [Tooltip("Tempo mínimo que o boss espera antes de lançar outro ataque aleatório.")]
    public float tempoEntreAtaquesMin = 2f;
    [Tooltip("Tempo máximo que o boss espera antes de lançar outro ataque aleatório.")]
    public float tempoEntreAtaquesMax = 4.5f;

    [Header("Referências Principais")]
    public Animator anim;       // Controlador de animações
    public Transform jogador;   // Referência ao alvo (player)

    [Header("Ataque 1: Emboscada Subterrânea")]
    [Tooltip("O lugar central para onde o boss sempre volta após atacar.")]
    public Transform pontoInicialFixo;
    [Tooltip("O prefab visual (ex: círculo vermelho, buraco abrindo) para avisar o jogador.")]
    public GameObject avisoDeBuracoPrefab;
    [Tooltip("Quanto tempo o aviso aparece antes da barata pular para fora e dar dano.")]
    public float tempoAvisoEmboscada = 2f;

    [Header("Ataque 2: Enxame de Mini Baratas")]
    [Tooltip("O prefab da mini barata inimiga.")]
    public GameObject miniBarataPrefab;
    [Tooltip("Posição da boca ou barriga do boss por onde saem as baratinhas.")]
    public Transform pontoSpawnMiniBaratas;
    [Tooltip("Quantas mini baratas saem neste ataque.")]
    public int quantidadeDeMiniBaratas = 6;
    [Tooltip("Intervalo de tempo entre o spawn de cada mini barata.")]
    public float tempoEntreSpawns = 0.3f;

    [Header("Ataque 3: Onda de Terra (Tsunami)")]
    [Tooltip("O prefab do ataque em área da onda de terra que se move pelo chão.")]
    public GameObject ondaDeTerraPrefab;
    [Tooltip("Posição na frente do boss onde a onda de terra começa.")]
    public Transform pontoSpawnOnda;

    void Start()
    {
        // Inicializa a vida do boss
        vidaAtual = maxVida;

        // Se as referências não foram preenchidas no Inspector, tentamos achar
        if (anim == null) anim = GetComponent<Animator>();
        if (jogador == null) 
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) jogador = p.transform;
        }

        // Inicia a "Inteligência" do Boss, rodando os ataques em loop infinito
        StartCoroutine(CicloDeBatalha());
    }

    // ==========================================
    // SISTEMA DE VIDA E MORTE
    // ==========================================
    public void TomarDano(int dano)
    {
        if (estaMorto) return;

        vidaAtual -= dano;
        Debug.Log("Boss Barata tomou " + dano + " de dano! Vida restante: " + vidaAtual);

        // Opcional: Tocar uma animação de Hit ou piscar o boss
        // anim.SetTrigger("TomarDano");

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    void Morrer()
    {
        estaMorto = true;
        Debug.Log("O Boss Barata foi derrotado!");

        // Para a IA e todos os ataques que estiverem acontecendo
        StopAllCoroutines();

        // Toca animação de morte (Crie esse Trigger no Animator)
        anim.SetTrigger("Morrer");

        // Opcional: Desativa colisor para o jogador poder passar pelo corpo
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Opcional: Destrói o boss da cena após um tempo ou exibe tela de vitória
        // Destroy(gameObject, 5f);
    }

    // Corotina responsável pelo cérebro do boss, escolhendo o que fazer e quando
    IEnumerator CicloDeBatalha()
    {
        // Fica rodando pra sempre enquanto o boss estiver vivo
        while (!estaMorto)
        {
            // O boss só ataca se estiver "Idle" ou em "Cooldown"
            if (currentState == BossState.Idle || currentState == BossState.Cooldown)
            {
                // O Boss "pensa" por um tempo aleatório
                float tempoDeEspera = Random.Range(tempoEntreAtaquesMin, tempoEntreAtaquesMax);
                yield return new WaitForSeconds(tempoDeEspera);

                // Troca para preparando e escolhe o ataque
                currentState = BossState.PreparandoAtaque;
                EscolherAtaqueAleatorio();
            }

            // Espera até o próximo frame para checar de novo
            yield return null;
        }
    }

    void EscolherAtaqueAleatorio()
    {
        if (estaMorto) return;

        // Em um jogo grande, podemos ter um Random.Range, ou até mesmo um sistema de pesos
        // Ex: Se o jogador está muito longe, aumenta a chance da Onda de Terra.
        // Aqui usaremos 1, 2 ou 3 aleatoriamente.
        int ataqueEscolhido = Random.Range(1, 4); // Escolhe 1, 2 ou 3

        switch (ataqueEscolhido)
        {
            case 1:
                StartCoroutine(Ataque1_EmboscadaSubterranea());
                break;
            case 2:
                StartCoroutine(Ataque2_EnxameMiniBaratas());
                break;
            case 3:
                StartCoroutine(Ataque3_TsunamiDeTerra());
                break;
        }
    }

    // ==========================================
    // ATAQUE 1: EMBOSCADA SUBTERRÂNEA
    // ==========================================
    IEnumerator Ataque1_EmboscadaSubterranea()
    {
        currentState = BossState.Atacando;

        // 1. Toca animação de escavar/entrar no chão
        anim.SetTrigger("AtkEmboscada_Entrar");

        // (Opcional: Esperar a animação terminar e então sumir visualmente o modelo 3D do boss)
        yield return new WaitForSeconds(1.0f); // Tempo fictício da animação de cavar
        
        // --- Boss fica invisível e invulnerável debaixo da terra ---
        // Exemplo: GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;

        // 2. Grava a posição atual do jogador para ser o alvo da emboscada
        Vector3 posicaoAlvo = jogador.position;
        posicaoAlvo.y = transform.position.y; // Mantém a mesma altura no eixo Y

        // 3. Surge a marca/aviso no chão onde o jogador estava
        GameObject avisoAtual = null;
        if (avisoDeBuracoPrefab != null)
        {
            avisoAtual = Instantiate(avisoDeBuracoPrefab, posicaoAlvo, Quaternion.identity);
        }

        // 4. Aguarda alguns segundos para o jogador ver a marca e ter tempo de desviar
        yield return new WaitForSeconds(tempoAvisoEmboscada);

        // 5. Move o Boss invisível para aquele ponto antes dele emergir
        transform.position = posicaoAlvo;

        // 6. Destrói o sinal visual, pois o boss está saindo
        if (avisoAtual != null) Destroy(avisoAtual);

        // --- Retorna a visibilidade do Boss ---
        
        // 7. Toca animação de sair do chão (Essa animação deve ter um "Animation Event" chamando a função que causa dano em área, ou usar um colisor na frente)
        anim.SetTrigger("AtkEmboscada_Sair");

        // Aguarda a animação acabar para o boss se recuperar
        yield return new WaitForSeconds(2.0f); 

        // 8. Após o ataque, o boss volta para a posição inicial
        yield return StartCoroutine(RetornarParaPosicaoInicial());

        currentState = BossState.Cooldown;
    }

    // ==========================================
    // ATAQUE 2: ENXAME DE MINI BARATAS
    // ==========================================
    IEnumerator Ataque2_EnxameMiniBaratas()
    {
        currentState = BossState.Atacando;

        // 1. Toca animação de gritar ou cuspir as baratinhas
        anim.SetTrigger("AtkEnxame_Carregar");

        // Espera o exato momento na animação que a boca abre (ajuste esse tempo)
        yield return new WaitForSeconds(0.8f);

        // 2. Faz um loop para cuspir as baratas uma por uma
        for (int i = 0; i < quantidadeDeMiniBaratas; i++)
        {
            if (miniBarataPrefab != null && pontoSpawnMiniBaratas != null)
            {
                // Cria a baratinha
                GameObject baratinha = Instantiate(miniBarataPrefab, pontoSpawnMiniBaratas.position, pontoSpawnMiniBaratas.rotation);
                
                // O script da baratinha em si deve ser responsável por correr reto e dar dano/destruir.
                // Exemplo: baratinha.GetComponent<MiniBarataIA>().DefinirAlvo(jogador);
            }
            
            // Pausa entre o spawn de uma barata e outra para não saírem coladas
            yield return new WaitForSeconds(tempoEntreSpawns);
        }

        // Tempo final de recuperação do ataque
        yield return new WaitForSeconds(1.5f);

        // Volta para a posição inicial após o ataque
        yield return StartCoroutine(RetornarParaPosicaoInicial());

        currentState = BossState.Cooldown;
    }

    // ==========================================
    // ATAQUE 3: ONDA DE TERRA (TSUNAMI)
    // ==========================================
    IEnumerator Ataque3_TsunamiDeTerra()
    {
        currentState = BossState.Atacando;

        // 1. O Boss levanta as garras ou bate o pé (animação de aviso do ataque carregado)
        anim.SetTrigger("AtkTsunami_Carregar");

        // Tempo que ele fica carregando e o jogador já entende "Opa, vem um ataque grande, vou preparar o salto"
        yield return new WaitForSeconds(1.5f);

        // 2. Animação dele descendo as garras/patas no chão com força
        anim.SetTrigger("AtkTsunami_Lancar");

        // Pequeno atraso para a animação bater no chão antes do poder sair
        yield return new WaitForSeconds(0.2f);

        // 3. Spawna a Onda de Terra ("Tsunami")
        if (ondaDeTerraPrefab != null && pontoSpawnOnda != null)
        {
            // Cria o tsunami apontado para onde o boss está olhando
            GameObject tsunami = Instantiate(ondaDeTerraPrefab, pontoSpawnOnda.position, pontoSpawnOnda.rotation);
            
            // O script da OndaDeTerraPrefab cuidará de mover ela para frente e destruir após um tempo
        }

        // Tempo de recuperação pós-ataque
        yield return new WaitForSeconds(2.0f);

        // Volta para a posição inicial após o ataque
        yield return StartCoroutine(RetornarParaPosicaoInicial());

        currentState = BossState.Cooldown;
    }

    // ==========================================
    // RETORNAR PARA A POSIÇÃO INICIAL
    // ==========================================
    IEnumerator RetornarParaPosicaoInicial()
    {
        if (pontoInicialFixo == null) yield break;

        // Se já estiver muito perto da posição inicial, não precisa andar
        if (Vector3.Distance(transform.position, pontoInicialFixo.position) < 0.5f)
        {
            yield break;
        }

        // Opcional: Se tiver uma animação de andar/correr, você pode ativar aqui
        // anim.SetBool("Andando", true);

        // Move o boss suavemente de volta para o ponto inicial
        while (Vector3.Distance(transform.position, pontoInicialFixo.position) > 0.1f)
        {
            // Vira para o ponto inicial
            Vector3 direcao = pontoInicialFixo.position - transform.position;
            direcao.y = 0;
            if (direcao != Vector3.zero)
            {
                Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, Time.deltaTime * 5f);
            }

            // Move para o ponto inicial a uma velocidade de 5 (ajuste se precisar)
            transform.position = Vector3.MoveTowards(transform.position, pontoInicialFixo.position, Time.deltaTime * 5f);

            // Espera até o próximo frame
            yield return null;
        }

        // Garante que ele pare exatamente no ponto correto
        transform.position = pontoInicialFixo.position;

        // Opcional: Desativa animação de andar
        // anim.SetBool("Andando", false);
    }

    void Update()
    {
        if (estaMorto) return;

        // Fora dos ataques principais e se ele não está cavando, o Boss vira para encarar o jogador
        // (Ajuste ou remova se preferir que ele vire por Root Motion nas animações)
        if (jogador != null && currentState != BossState.Atacando)
        {
            Vector3 direcaoParaJogador = jogador.position - transform.position;
            direcaoParaJogador.y = 0; // Previne que o Boss incline para baixo/cima
            
            if (direcaoParaJogador != Vector3.zero)
            {
                Quaternion rotacaoAlvo = Quaternion.LookRotation(direcaoParaJogador);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, Time.deltaTime * 3f);
            }
        }
    }

    // --- FUNÇÃO PARA SER CHAMADA POR EVENTOS DE ANIMAÇÃO (OPCIONAL) ---
    // Você pode chamar isso direto da animação "AtkEmboscada_Sair" se quiser o dano em área em um frame específico!
    public void CausarDanoEmAreaErupcao()
    {
        // Lógica de Physics.OverlapSphere para achar o jogador e dar dano
        /*
        Collider[] hits = Physics.OverlapSphere(transform.position, 5f);
        foreach(var hit in hits) {
            if(hit.CompareTag("Player")) {
                hit.GetComponent<PlayerHealth>().TomarDano(50);
            }
        }
        */
    }
}
