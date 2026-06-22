using UnityEngine;
using UnityEngine.SceneManagement;

public class AbrirPortaQuandoBossMorrer : MonoBehaviour
{
    [Header("Boss")]
    public BossBarata bossBarata;
    public BossAnaoMitologico bossAnao;
    public BossIrmaoKingCube bossIrmaoKingCube;
    public UltimoBoss ultimoBoss;

    private bool trocouCena = false;

    private void Update()
    {
        if (trocouCena) return;

        if (BossMorreu())
            CarregarProximaCena();
    }

    private bool BossMorreu()
    {
        if (bossBarata != null) return bossBarata.EstaMorto;
        if (bossAnao != null) return bossAnao.EstaMorto;
        if (bossIrmaoKingCube != null) return bossIrmaoKingCube.EstaMorto;
        if (ultimoBoss != null) return ultimoBoss.EstaMorto;

        return false;
    }

    private void CarregarProximaCena()
    {
        trocouCena = true;
        TrocaCenaBoss.CarregarProximaCena();
    }
}
