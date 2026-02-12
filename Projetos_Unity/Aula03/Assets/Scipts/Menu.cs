using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{

    [SerializeField] private GameObject PainelMenuInicial;
    [SerializeField] private GameObject painelOpcoes;

    public void Jogar()
    {
        SceneManager.LoadScene("SampleScene");
    }
    public void Opções()
    {
        PainelMenuInicial.SetActive(false);
        painelOpcoes.SetActive(true);

    }
    public void FecharOpções()
    {
        painelOpcoes.SetActive(false);
        PainelMenuInicial.SetActive(true);
    }
    public void Sair()
    {
        Debug.Log("Sair do jogo");
        Application.Quit();
    }
}
