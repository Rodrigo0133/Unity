    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
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
        public bool spriteAnaoOlhaParaDireita = true;
        
        [Header("=== REFERÊNCIAS DO PÉ GRANDE ===")]
        [Tooltip("O GameObject do Pé Grande que vai ser ativado na fase 2")]
        public GameObject peGrandeGameObject; 
        [Tooltip("Animator do Pé Grande (opcional)")]
        public Animator animPeGrande;
        [Tooltip("Local de onde saem as pedras do Pé Grande (opcional)")]
        public Transform pontoDisparoPeGrande;
        public bool spritePeGrandeOlhaParaDireita = true;

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
        public bool EstaMorto => estaMorto;

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

        void Update()
        {
            ProcurarJogador();
            VirarObjetoParaJogador(gameObject, spriteAnaoOlhaParaDireita);

            if (peGrandeGameObject != null && peGrandeGameObject.activeInHierarchy)
            {
                VirarObjetoParaJogador(peGrandeGameObject, spritePeGrandeOlhaParaDireita);
            }
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
            GameDatabase.Instance.data.plets += 200;
            TrocaCenaBoss.CarregarProximaCena();
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
        // 1. Verificação de segurança crucial
        if (prefab == null)
        {
            Debug.LogWarning($"[Boss 2] Falta atribuir o prefab no Inspector para o ataque atual!");
            return;
        }
        
        if (jogador == null) return;

        // 2. Cria o objeto na cena
        GameObject proj = Instantiate(prefab, origem.position, Quaternion.identity);
        
        // 3. Calcula a direção para o jogador
        Vector3 direcao = (jogador.position - origem.position).normalized;
        
        // 4. Tenta pegar o script de todas as formas possíveis (no objeto ou nos filhos)
        ProjetilBoss pb = proj.GetComponent<ProjetilBoss>();
        if (pb == null)
        {
            pb = proj.GetComponentInChildren<ProjetilBoss>();
        }

        // 5. SE MESMO ASSIM NÃO ENCONTRAR (O plano de emergência para não travar o Boss!)
        if (pb == null)
        {
            Debug.LogWarning($"[Boss 2] O prefab {prefab.name} perdeu a referência do script temporariamente. A adicionar componente via código para salvar o jogo!");
            pb = proj.AddComponent<ProjetilBoss>();
        }
        
        // 6. Configura o projétil com toda a certeza de que ele existe
        pb.tipoMovimento = tipoMov;
        pb.velocidade = velocidade;
        pb.Configurar(direcao);
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

        private void VirarObjetoParaJogador(GameObject objeto, bool spriteOlhaParaDireita)
        {
            if (objeto == null || jogador == null) return;

            float direcao = jogador.position.x >= objeto.transform.position.x ? 1f : -1f;
            if (!spriteOlhaParaDireita) direcao *= -1f;

            Vector3 escala = objeto.transform.localScale;
            escala.x = Mathf.Abs(escala.x) * direcao;
            objeto.transform.localScale = escala;
        }
    }
