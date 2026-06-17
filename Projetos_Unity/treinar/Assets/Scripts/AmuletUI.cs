using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmuletUI : MonoBehaviour
{
    [Header("=== ELEMENTOS DOS SLOTS EQUIPADOS ===")]
    public Image slot1Image;
    public Image slot2Image;
    public Image slot3Image;
    public GameObject cadeadoSlot3;

    [Header("=== TEXTOS DO INVENTÁRIO (OPCIONAL) ===")]
    public TMP_Text textoPlets;

    [Header("=== COMPONENTES DOS AMULETOS (LOJA/INVENTÁRIO) ===")]
    public Button botaoVitalidade;
    public TMP_Text textoVitalidade;

    public Button botaoFuria;
    public TMP_Text textoFuria;

    public Button botaoGanancia;
    public TMP_Text textoGanancia;

    public Button botaoRapidez;
    public TMP_Text textoRapidez;

    public Button botaoEscudo;
    public TMP_Text textoEscudo;

    public Button botaoSlot3;
    public TMP_Text textoSlot3;

    [Header("=== ELEMENTOS DA ESPADA ===")]
    public Button botaoEspada;
    public TMP_Text textoEspada;

    void OnEnable()
    {
        AtualizarUI();
    }

    public void AtualizarUI()
    {
        if (GameDatabase.Instance == null) return;
        SaveData data = GameDatabase.Instance.data;

        // 1. Atualiza dinheirinho
        if (textoPlets != null) textoPlets.text = "Plets: " + data.plets;

        // 2. Atualiza os slots equipados visuais
        AtualizarSlotEquipado(slot1Image, data.equippedAmulets.Count > 0 ? data.equippedAmulets[0] : null);
        AtualizarSlotEquipado(slot2Image, data.equippedAmulets.Count > 1 ? data.equippedAmulets[1] : null);
        
        if (data.hasUnlockedThirdSlot)
        {
            if (cadeadoSlot3 != null) cadeadoSlot3.SetActive(false);
            AtualizarSlotEquipado(slot3Image, data.equippedAmulets.Count > 2 ? data.equippedAmulets[2] : null);
        }
        else
        {
            if (cadeadoSlot3 != null) cadeadoSlot3.SetActive(true);
            if (slot3Image != null) slot3Image.color = Color.black; // Bloqueado
        }

        // 3. Atualiza estado dos botões da loja/inventário
        ConfigurarBotaoAmuleto("vitalidade", botaoVitalidade, textoVitalidade);
        ConfigurarBotaoAmuleto("furia", botaoFuria, textoFuria);
        ConfigurarBotaoAmuleto("ganancia", botaoGanancia, textoGanancia);
        ConfigurarBotaoAmuleto("rapidez", botaoRapidez, textoRapidez);
        ConfigurarBotaoAmuleto("escudo", botaoEscudo, textoEscudo);

        // Atualiza botão do 3º slot
        if (botaoSlot3 != null && textoSlot3 != null)
        {
            if (data.hasUnlockedThirdSlot)
            {
                textoSlot3.text = "3º Slot Desbloqueado";
                botaoSlot3.interactable = false;
            }
            else
            {
                textoSlot3.text = "Comprar 3º Slot (100 Plets)";
                botaoSlot3.interactable = data.plets >= 100;
            }
        }

        // Atualiza botão da Espada
        if (botaoEspada != null && textoEspada != null && ShopManager.Instance != null)
        {
            int nextLevel = data.swordLevel + 1;
            if (nextLevel > 4)
            {
                textoEspada.text = "Espada no Máx (Nível 4)";
                botaoEspada.interactable = false;
            }
            else
            {
                int custo = ShopManager.Instance.swordUpgradeCosts[nextLevel - 1];
                textoEspada.text = $"Melhorar Espada (Custo: {custo} Plets)";
                botaoEspada.interactable = data.plets >= custo;
            }
        }
    }

    private void AtualizarSlotEquipado(Image imgSlot, string amuletId)
    {
        if (imgSlot == null) return;

        if (string.IsNullOrEmpty(amuletId))
        {
            imgSlot.color = new Color(1, 1, 1, 0.2f); 
            imgSlot.sprite = null;
        }
        else
        {
            imgSlot.color = Color.white; 
        }
    }

    private void ConfigurarBotaoAmuleto(string id, Button btn, TMP_Text txt)
    {
        if (btn == null || txt == null) return;
        SaveData data = GameDatabase.Instance.data;
        AmuletInfo info = AmuletDatabase.GetById(id);

        if (info == null) return;

        if (data.ownedAmulets.Contains(id))
        {
            btn.interactable = true;
            if (data.equippedAmulets.Contains(id))
            {
                txt.text = "Desequipar";
            }
            else
            {
                txt.text = "Equipar";
                int maxSlots = data.hasUnlockedThirdSlot ? 3 : 2;
                if (data.equippedAmulets.Count >= maxSlots) btn.interactable = false;
            }
        }
        else 
        {
            txt.text = $"Comprar ({info.cost} Plets)";
            btn.interactable = data.plets >= info.cost;
        }
    }

    public void AcaoBotaoAmuleto(string id)
    {
        if (GameDatabase.Instance == null || ShopManager.Instance == null) return;
        SaveData data = GameDatabase.Instance.data;

        if (data.ownedAmulets.Contains(id))
        {
            ShopManager.Instance.EquipAmulet(id);
        }
        else
        {
            ShopManager.Instance.BuyAmulet(id);
        }
        AtualizarUI();
    }

    public void AcaoComprarSlot3()
    {
        if (ShopManager.Instance == null) return;
        ShopManager.Instance.BuyThirdSlot();
        AtualizarUI();
    }

    public void AcaoMelhorarEspada()
    {
        if (ShopManager.Instance == null) return;
        ShopManager.Instance.UpgradeSword();
        AtualizarUI();
    }
}
