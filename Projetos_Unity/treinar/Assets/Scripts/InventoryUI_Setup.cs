using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private EquipmentManager equipmentManager; // reference to EquipmentManager
    private void Awake()
    {
        equipmentManager = FindObjectOfType<EquipmentManager>();
    }
    [SerializeField] private Transform slotsContainer; // Parent do Grid Layout Group
    [SerializeField] private GameObject slotPrefab; // Prefab do slot
    
    private InventorySlotUI[] slotUIs;

    private void Start()
    {
        if (inventoryManager == null)
            inventoryManager = GetComponent<InventoryManager>();
        
        CreateSlotUIs();
        
        // Subscreve aos eventos de mudança de inventário
        inventoryManager.onInventoryChanged.AddListener(RefreshSlot);
    }

    // Cria a UI para cada slot
    private void CreateSlotUIs()
    {
        var (width, height) = inventoryManager.GetGridDimensions();
        int totalSlots = inventoryManager.GetTotalSlots();
        
        // Configura o Grid Layout Group
        GridLayoutGroup gridLayout = slotsContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.constraintCount = width;
        }
        
        slotUIs = new InventorySlotUI[totalSlots];
        
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotsContainer);
            InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();
            
            if (slotUI != null)
            {
                slotUI.SetIndex(i);
                slotUI.SetInventoryManager(inventoryManager);
                slotUI.SetEquipmentManager(equipmentManager);
                slotUIs[i] = slotUI;
            }
        }
    }
    
    // Atualiza um slot específico
    public void RefreshSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slotUIs.Length)
        {
            InventorySlot slot = inventoryManager.GetSlot(slotIndex);
            slotUIs[slotIndex].UpdateSlotUI(slot);
        }
    }
    
    // Atualiza todos os slots
    public void RefreshAllSlots()
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            RefreshSlot(i);
        }
    }
    
    private void OnDestroy()
    {
        if (inventoryManager != null)
            inventoryManager.onInventoryChanged.RemoveListener(RefreshSlot);
    }
}
