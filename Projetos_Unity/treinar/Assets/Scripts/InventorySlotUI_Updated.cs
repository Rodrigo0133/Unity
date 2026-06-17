using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Color emptySlotColor = Color.gray;
    [SerializeField] private Color filledSlotColor = Color.white;
    [SerializeField] private Color selectedSlotColor = Color.yellow;
    
    // Equipamento
    [SerializeField] private EquipmentManager equipmentManager;
    
    private int slotIndex;
    private InventoryManager inventoryManager;
    private Button slotButton;
    private Image slotImage;
    private bool isSelected = false;
    
    private void Awake()
    {
        slotButton = GetComponent<Button>();
        slotImage = GetComponent<Image>();
        
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnSlotClicked);
        }
    }
    
    public void SetIndex(int index)
    {
        slotIndex = index;
    }
    
    public void SetInventoryManager(InventoryManager manager)
    {
        inventoryManager = manager;
    }
    
    public void SetEquipmentManager(EquipmentManager manager)
    {
        equipmentManager = manager;
    }
    
    // Atualiza o visual do slot
    public void UpdateSlotUI(InventorySlot slot)
    {
        if (slot == null || slot.IsEmpty())
        {
            itemIcon.sprite = null;
            itemIcon.color = new Color(1, 1, 1, 0);
            quantityText.text = "";
            slotImage.color = emptySlotColor;
            isSelected = false;
        }
        else
        {
            itemIcon.sprite = slot.item.icon;
            itemIcon.color = Color.white;
            
            if (slot.quantity > 1)
                quantityText.text = slot.quantity.ToString();
            else
                quantityText.text = "";
            
            // Se não está selecionado, volta para cor normal
            if (!isSelected)
                slotImage.color = filledSlotColor;
        }
    }
    
    // Chamado quando clica no slot do inventário
    private void OnSlotClicked()
    {
        InventorySlot slot = inventoryManager.GetSlot(slotIndex);
        
        if (!slot.IsEmpty())
        {
            // Alterna seleção
            if (!isSelected)
            {
                SelectSlot();
            }
            else
            {
                // Se já está selecionado, tenta equipar
                EquipItem(slot.item);
                DeselectSlot();
            }
        }
    }
    
    private void SelectSlot()
    {
        isSelected = true;
        slotImage.color = selectedSlotColor;
        Debug.Log($"Slot {slotIndex} selecionado. Clica novamente para equipar!");
    }
    
    private void DeselectSlot()
    {
        isSelected = false;
        InventorySlot slot = inventoryManager.GetSlot(slotIndex);
        slotImage.color = slot.IsEmpty() ? emptySlotColor : filledSlotColor;
    }
    
    private void EquipItem(Item item)
    {
        if (equipmentManager == null)
        {
            Debug.LogWarning("EquipmentManager não configurado!");
            return;
        }
        
        // Tenta equipar no primeiro slot vazio
        if (equipmentManager.EquipItem(item))
        {
            Debug.Log($"Item '{item.itemName}' equipado!");
            // Remove do inventário
            inventoryManager.RemoveItem(item, 1);
        }
        else
        {
            Debug.Log("Nenhum slot de equipamento disponível!");
        }
    }
    
    // Função pública para desequipar
    public void UnequipToInventory(Item item)
    {
        if (inventoryManager != null)
        {
            inventoryManager.AddItem(item, 1);
        }
    }
}
