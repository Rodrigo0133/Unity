using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLives : MonoBehaviour
{
    public Image[] coracao;
    public Sprite cheio;
    public Sprite vazio;
    private PlayerMovement Script_P;
    void Awake()
    {
        Script_P = FindObjectOfType<PlayerMovement>();
    }
    private void Update()
    {
        if (Script_P == null) return; // Protege contra null
        HealthLogick(Script_P.currentLife,Script_P.maxLife);

    }
    public void HealthLogick(float life,float Maxlife)
    {
        if(life > Maxlife)
        {   
            life = Maxlife;
        }
        for (int i = 0; i < coracao.Length; i++)
        {
            if (coracao[i] == null) continue; // Ignora slots vazios

            if(i < life)
            {
                coracao[i].sprite = cheio;
            }
            else
            {
                coracao[i].sprite = vazio;  
            }
            if (i < Maxlife)
            {
                coracao[i].enabled = true;
            }
            else
            {
                coracao[i].enabled = false;
            }
        }
    }

  
}