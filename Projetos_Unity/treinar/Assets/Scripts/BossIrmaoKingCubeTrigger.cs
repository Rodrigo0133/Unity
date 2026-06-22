using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BossIrmaoKingCubeTrigger : MonoBehaviour
{
    [Header("Configuracoes")]
    public BossIrmaoKingCube boss;
    public float tempoEspera = 2f;

    [Header("Bloqueio Inicial")]
    [Tooltip("Se ativado, esta porta fica solida ate todas as BarataInimigo morrerem.")]
    public bool bloquearAteMatarBaratas = true;

    private bool jaAtivado = false;
    private bool trocouCena = false;
    private bool tinhaBoss = false;
    private Collider2D meuCollider;

    void Start()
    {
        meuCollider = GetComponent<Collider2D>();
        tinhaBoss = boss != null;

        if (boss != null)
            boss.gameObject.SetActive(false);

        if (bloquearAteMatarBaratas && meuCollider != null)
            meuCollider.isTrigger = false;
    }

    void Update()
    {
        if (!jaAtivado)
            VerificarBloqueioInicial();
        else if (!trocouCena && BossMorreu())
            CarregarProximaCena();
    }

    private void VerificarBloqueioInicial()
    {
        if (!bloquearAteMatarBaratas || meuCollider == null || meuCollider.isTrigger)
            return;

        if (FindObjectsOfType<BarataInimigo>().Length == 0)
        {
            Debug.Log("[BossIrmaoKingCubeTrigger] Todas as baratas morreram. A entrada abriu.");
            meuCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (jaAtivado) return;

        if (other.CompareTag("Player") || other.name.Contains("Square") || other.name.Contains("Quadrado"))
        {
            jaAtivado = true;
            Debug.Log("[BossIrmaoKingCubeTrigger] Jogador entrou na sala. A luta vai comecar.");
            StartCoroutine(IniciarLutaRoutine());
        }
    }

    private IEnumerator IniciarLutaRoutine()
    {
        yield return new WaitForSeconds(tempoEspera);

        if (boss != null)
            boss.gameObject.SetActive(true);
        else
            Debug.LogWarning("[BossIrmaoKingCubeTrigger] Boss nao referenciado no Inspector.");

        TrancarPorta();
    }

    private bool BossMorreu()
    {
        if (!tinhaBoss) return false;
        return boss == null || boss.EstaMorto;
    }

    private void TrancarPorta()
    {
        if (meuCollider != null)
        {
            meuCollider.enabled = true;
            meuCollider.isTrigger = false;
            Debug.Log("[BossIrmaoKingCubeTrigger] Porta trancada atras do jogador.");
        }
    }

    private void CarregarProximaCena()
    {
        trocouCena = true;
        TrocaCenaBoss.CarregarProximaCena();
    }
}
