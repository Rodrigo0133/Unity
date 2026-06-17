using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentSlotShop : MonoBehaviour
{
    [SerializeField] private EquipmentManager equipmentManager;
    [SerializeField] private int slotPrice = 100;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI slotCountText;
    
    // Referência ao sistema de moeda (adapta conforme o teu)
    private int playerMoney = 500; // Exemplo
    
    private void Start()
    {
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(BuySlot);
        }
        
        UpdateUI();
        
        // Subscreve para updates
        if (equipmentManager != null)
        {
            equipmentManager.onSlotsIncreased.AddListener(UpdateUI);
        }
    }
    
    public void BuySlot()
    {
        if (playerMoney >= slotPrice)
        {
            playerMoney -= slotPrice;
            equipmentManager.BuyEquipmentSlot();
            Debug.Log($"Novo slot de equipamento comprado! Slots: {equipmentManager.GetEquipmentSlotCount()}");
            UpdateUI();
        }
        else
        {
            Debug.Log($"Moeda insuficiente! Precisas de {slotPrice} mas tens {playerMoney}");
        }
    }
    
    // Atualiza o UI da loja
    public void UpdateUI()
    {
        if (priceText != null)
            priceText.text = $"Preço: {slotPrice}";
        
        if (slotCountText != null)
            slotCountText.text = $"Slots: {equipmentManager.GetEquipmentSlotCount()}";
        
        // Desativa o botão se não há moeda suficiente
        if (buyButton != null)
            buyButton.interactable = playerMoney >= slotPrice;
    }
    
    // Função para adicionar moeda (chama quando o jogador ganha moeda)
    public void AddMoney(int amount)
    {
        playerMoney += amount;
        UpdateUI();
    }
    
    // Função para obter moeda atual
    public int GetPlayerMoney()
    {
        return playerMoney;
    }
    
    // Define a moeda do jogador
    public void SetPlayerMoney(int amount)
    {
        playerMoney = amount;
        UpdateUI();
    }
}
