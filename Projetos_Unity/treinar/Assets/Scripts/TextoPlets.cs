using TMPro;
using UnityEngine;

public class TextoPlets : MonoBehaviour
{
    public TMP_Text textoPlets;
    public string prefixo = "";
    public string sufixo = " plets";

    private int ultimoValor = int.MinValue;

    private void Awake()
    {
        if (textoPlets == null)
            textoPlets = GetComponent<TMP_Text>();

        AtualizarTexto(true);
    }

    private void OnEnable()
    {
        AtualizarTexto(true);
    }

    private void Update()
    {
        AtualizarTexto();
    }

    public void AtualizarTexto(bool forcar = false)
    {
        int totalPlets = 0;

        if (GameDatabase.Instance != null && GameDatabase.Instance.data != null)
            totalPlets = GameDatabase.Instance.data.plets;

        if (!forcar && totalPlets == ultimoValor)
            return;

        ultimoValor = totalPlets;

        if (textoPlets != null)
            textoPlets.text = prefixo + totalPlets + sufixo;
    }
}
