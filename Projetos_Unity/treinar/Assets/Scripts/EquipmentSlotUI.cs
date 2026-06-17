using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentSlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI slotLabel;
    [SerializeField] private Color emptySlotColor = Color.gray;
    [SerializeField] private Color filledSlotColor = Color.white;
    
    private int slotIndex;
    private EquipmentManager equipmentManager;
    private Button slotButton;
    private Image slotImage;
    
    private void Awake()
    {
        slotButton = GetComponent<Button>();
        slotImage = GetComponent<Image>();
        
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnSlotClicked);
        }
    }
    
    public void Initialize(int index, EquipmentManager manager)
    {
        slotIndex = index;
        equipmentManager = manager;
        UpdateDisplay();
    }
    
    // Atualiza o visual do slot
    public void UpdateDisplay()
    {
        Item equippedItem = equipmentManager.GetEquippedItem(slotIndex);
        
        if (equippedItem == null)
        {
            itemIcon.sprite = null;
            itemIcon.color = new Color(1, 1, 1, 0);
            slotImage.color = emptySlotColor;
            if (slotLabel != null)
                slotLabel.text = $"Slot {slotIndex + 1}";
        }
        else
        {
            itemIcon.sprite = equippedItem.icon;
            itemIcon.color = Color.white;
            slotImage.color = filledSlotColor;
            if (slotLabel != null)
                slotLabel.text = equippedItem.itemName;
        }
    }
    
    // Chamado quando clica no slot
    private void OnSlotClicked()
    {
        Item equippedItem = equipmentManager.GetEquippedItem(slotIndex);
        
        if (equippedItem != null)
        {
            // Se já tem algo, mostra opção de desequipar
            Debug.Log($"Equipado: {equippedItem.itemName}");
            // Aqui podes adicionar um menu para desequipar ou trocar
        }
        else
        {
            Debug.Log($"Slot {slotIndex} vazio");
        }
    }
}
