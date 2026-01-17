using UnityEngine;
using UnityEngine.UI;

public class PlayerLives : MonoBehaviour
{
    public int vidasAtuais;

    public Image[] vidasUI; // Array das imagens

    void Start()
    {
        vidasAtuais = 3; // ou 3
        
    }


    public void PerderVida()
    {
        if (vidasAtuais > 0)
        {
            
            AtualizarVidasUI();
        }

        if (vidasAtuais <= 0)
        {
            Debug.Log("Player morreu");
            
        }
    }

    public void AtualizarVidasUI()
    {
        for (int i = 0; i < vidasUI.Length; i++)
        {
            if (i < vidasAtuais)
                vidasUI[i].enabled = true; // mostra o coração
            else
                vidasUI[i].enabled = false; // esconde
        }
    }
}
