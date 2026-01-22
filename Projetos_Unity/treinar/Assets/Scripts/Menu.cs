using UnityEngine;
using UnityEngine.SceneManagement;
public class Menu : MonoBehaviour
{
    [SerializeField] private string Nome_do_Level_do_jogo;
    [SerializeField]private GameObject PainelMenuInicial;
    [SerializeField]private GameObject painelOpcoes;
    public void Jogar()
    {
        SceneManager.LoadScene(Nome_do_Level_do_jogo);
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
