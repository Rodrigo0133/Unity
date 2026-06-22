using UnityEngine;
using UnityEngine.SceneManagement;

public static class TrocaCenaBoss
{
    private static bool estaATrocarCena = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ResetarTroca()
    {
        estaATrocarCena = false;
    }

    public static void CarregarProximaCena()
    {
        if (estaATrocarCena) return;

        int cenaAtual = SceneManager.GetActiveScene().buildIndex;
        int proximaCena = cenaAtual + 1;

        if (proximaCena >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("[TrocaCenaBoss] Nao existe proxima cena no Build Settings. Adiciona a cena seguinte em File > Build Settings.");
            return;
        }

        estaATrocarCena = true;
        SceneManager.LoadScene(proximaCena);
    }
}
