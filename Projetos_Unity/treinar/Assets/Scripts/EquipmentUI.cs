using UnityEngine;

public class EquipmentUI : MonoBehaviour
{
    [SerializeField] private EquipmentManager equipmentManager;
    [SerializeField] private Transform equipmentSlotsContainer;
    [SerializeField] private GameObject equipmentSlotPrefab;
    
    private EquipmentSlotUI[] equipmentSlotUIs;
    
    private void Start()
    {
        if (equipmentManager == null)
            equipmentManager = GetComponent<EquipmentManager>();
        
        CreateEquipmentSlots();
        
        // Subscreve aos eventos
        equipmentManager.onEquipmentChanged.AddListener(RefreshEquipmentSlot);
        equipmentManager.onSlotsIncreased.AddListener(OnSlotsIncreased);
    }
    
    // Cria os slots de equipamento
    private void CreateEquipmentSlots()
    {
        int totalSlots = equipmentManager.GetEquipmentSlotCount();
        equipmentSlotUIs = new EquipmentSlotUI[totalSlots];
        
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject slotGO = Instantiate(equipmentSlotPrefab, equipmentSlotsContainer);
            EquipmentSlotUI slotUI = slotGO.GetComponent<EquipmentSlotUI>();
            
            if (slotUI != null)
            {
                slotUI.Initialize(i, equipmentManager);
                equipmentSlotUIs[i] = slotUI;
            }
        }
    }
    
    // Atualiza um slot de equipamento
    public void RefreshEquipmentSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < equipmentSlotUIs.Length)
        {
            equipmentSlotUIs[slotIndex].UpdateDisplay();
        }
    }
    
    // Chamado quando compra um novo slot
    private void OnSlotsIncreased(int newSlotCount)
    {
        // Cria o novo slot
        int newSlotIndex = newSlotCount - 1;
        EquipmentSlotUI[] newArray = new EquipmentSlotUI[newSlotCount];
        
        // Copia os slots antigos
        for (int i = 0; i < equipmentSlotUIs.Length; i++)
        {
            newArray[i] = equipmentSlotUIs[i];
        }
        
        // Cria o novo slot
        GameObject slotGO = Instantiate(equipmentSlotPrefab, equipmentSlotsContainer);
        EquipmentSlotUI slotUI = slotGO.GetComponent<EquipmentSlotUI>();
        if (slotUI != null)
        {
            slotUI.Initialize(newSlotIndex, equipmentManager);
            newArray[newSlotIndex] = slotUI;
        }
        
        equipmentSlotUIs = newArray;
        Debug.Log($"Novo slot de equipamento criado! Total: {newSlotCount}");
    }
    
    private void OnDestroy()
    {
        if (equipmentManager != null)
        {
            equipmentManager.onEquipmentChanged.RemoveListener(RefreshEquipmentSlot);
            equipmentManager.onSlotsIncreased.RemoveListener(OnSlotsIncreased);
        }
    }
}
