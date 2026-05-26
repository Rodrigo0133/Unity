using UnityEngine;
using System.Collections;

public class BossTrigger : MonoBehaviour
{
    [Header("Configurações")]
    public BossBarata boss;
    public float tempoEspera = 2f;
    
    [Header("Bloqueio Inicial")]
    [Tooltip("Se ativado, esta porta será sólida até todas as BarataInimigo morrerem.")]
    public bool bloquearAteMatarBaratas = true;

    private bool jaAtivado = false;
    private Collider2D meuCollider;

    void Start()
    {
        meuCollider = GetComponent<Collider2D>();
        
        // Se quisermos bloquear o caminho inicialmente, transformamos o trigger numa parede
        if (bloquearAteMatarBaratas && meuCollider != null)
        {
            meuCollider.isTrigger = false;
        }
    }

    void Update()
    {
        // Se já foi ativado, não precisamos de continuar a verificar
        if (jaAtivado || !bloquearAteMatarBaratas || meuCollider == null) return;

        // Enquanto o caminho estiver bloqueado (parede sólida), verificamos as baratas
        if (!meuCollider.isTrigger)
        {
            BarataInimigo[] baratas = FindObjectsOfType<BarataInimigo>();
            if (baratas.Length == 0)
            {
                Debug.Log("[BossTrigger] Todas as baratas mortas! A porta abriu.");
                meuCollider.isTrigger = true; // Transforma de novo em Trigger para o jogador poder passar!
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (jaAtivado) return;

        if (other.CompareTag("Player") || other.name.Contains("Square") || other.name.Contains("Quadrado"))
        {
            jaAtivado = true;
            Debug.Log("[BossTrigger] Jogador entrou na sala! A luta vai começar em " + tempoEspera + " segundos...");
            StartCoroutine(IniciarLutaRoutine());
        }
    }

    IEnumerator IniciarLutaRoutine()
    {
        yield return new WaitForSeconds(tempoEspera);
        
        if (boss != null)
        {
            boss.IniciarBatalha();
        }
        else
        {
            Debug.LogWarning("[BossTrigger] Boss não referenciado no Inspector!");
        }

        // Transforma a porta novamente numa parede sólida para impedir que o jogador fuja!
        if (meuCollider != null)
        {
            meuCollider.isTrigger = false;
            Debug.Log("[BossTrigger] Porta trancada atrás do jogador!");
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
