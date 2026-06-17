using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public Text textoMoedas; // Arraste o seu TextoPletsHUD para aqui no Inspector

    void Update()
    {
        if (GameDatabase.Instance != null && textoMoedas != null)
        {
            // Lê dinamicamente o valor das moedas na base de dados
            textoMoedas.text = "Plets: " + GameDatabase.Instance.data.plets;
        }
    }
}
