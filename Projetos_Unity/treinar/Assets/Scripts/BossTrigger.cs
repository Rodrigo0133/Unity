using UnityEngine;
using System.Collections;

public class BossTrigger : MonoBehaviour
{
    [Header("Configuracoes")]
    public BossBarata boss;
    public BossAnaoMitologico bossAnao;
    public BossIrmaoKingCube bossIrmaoKingCube;
    public UltimoBoss ultimoBoss;
    public float tempoEspera = 2f;

    [Header("Bloqueio Inicial")]
    [Tooltip("Se ativado, esta porta fica solida ate todas as BarataInimigo morrerem.")]
    public bool bloquearAteMatarBaratas = true;

    private bool jaAtivado = false;
    private bool trocouCena = false;
    private bool tinhaBossBarata = false;
    private bool tinhaBossAnao = false;
    private bool tinhaBossIrmao = false;
    private bool tinhaUltimoBoss = false;
    private Collider2D meuCollider;

    void Start()
    {
        meuCollider = GetComponent<Collider2D>();

        tinhaBossBarata = boss != null;
        tinhaBossAnao = bossAnao != null;
        tinhaBossIrmao = bossIrmaoKingCube != null;
        tinhaUltimoBoss = ultimoBoss != null;

        if (bossAnao != null) bossAnao.gameObject.SetActive(false);
        if (bossIrmaoKingCube != null) bossIrmaoKingCube.gameObject.SetActive(false);
        if (ultimoBoss != null) ultimoBoss.gameObject.SetActive(false);

        if (bloquearAteMatarBaratas && meuCollider != null)
            meuCollider.isTrigger = false;
    }

    void Update()
    {
        if (jaAtivado)
        {
            if (!trocouCena && BossMorreu())
            {
                trocouCena = true;
                TrocaCenaBoss.CarregarProximaCena();
            }

            return;
        }

        if (!bloquearAteMatarBaratas || meuCollider == null || meuCollider.isTrigger)
            return;

        if (FindObjectsOfType<BarataInimigo>().Length == 0)
        {
            Debug.Log("[BossTrigger] Todas as baratas mortas. A entrada abriu.");
            meuCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (jaAtivado) return;

        if (other.CompareTag("Player") || other.name.Contains("Square") || other.name.Contains("Quadrado"))
        {
            jaAtivado = true;
            Debug.Log("[BossTrigger] Jogador entrou na sala. A luta vai comecar em " + tempoEspera + " segundos.");
            StartCoroutine(IniciarLutaRoutine());
        }
    }

    IEnumerator IniciarLutaRoutine()
    {
        yield return new WaitForSeconds(tempoEspera);

        if (boss != null)
            boss.IniciarBatalha();
        else if (bossAnao != null)
            bossAnao.gameObject.SetActive(true);
        else if (bossIrmaoKingCube != null)
            bossIrmaoKingCube.gameObject.SetActive(true);
        else if (ultimoBoss != null)
            ultimoBoss.gameObject.SetActive(true);
        else
            Debug.LogWarning("[BossTrigger] Boss nao referenciado no Inspector.");

        if (meuCollider != null)
        {
            meuCollider.enabled = true;
            meuCollider.isTrigger = false;
            Debug.Log("[BossTrigger] Porta trancada atras do jogador.");
        }
    }

    private bool BossMorreu()
    {
        if (tinhaBossBarata) return boss == null || boss.EstaMorto;
        if (tinhaBossAnao) return bossAnao == null || bossAnao.EstaMorto;
        if (tinhaBossIrmao) return bossIrmaoKingCube == null || bossIrmaoKingCube.EstaMorto;
        if (tinhaUltimoBoss) return ultimoBoss == null || ultimoBoss.EstaMorto;

        return false;
    }
}
