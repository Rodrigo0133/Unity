using UnityEngine;
using UnityEngine.Events;

public class EquipmentManager : MonoBehaviour
{
    [SerializeField] private int initialSlots = 2;
    private int currentSlots;
    
    private Item[] equippedItems;
    
    // Eventos
    public UnityEvent<int> onEquipmentChanged = new UnityEvent<int>();
    public UnityEvent<int> onSlotsIncreased = new UnityEvent<int>();
    
    private void Awake()
    {
        currentSlots = initialSlots;
        equippedItems = new Item[currentSlots];
    }
    
    // Equipar um item
    public bool EquipItem(Item item, int slotIndex = -1)
    {
        // Se não especificar slot, encontra o primeiro vazio
        if (slotIndex == -1)
        {
            slotIndex = FindEmptyEquipmentSlot();
            if (slotIndex == -1)
                return false; // Sem slots vazios
        }
        
        if (slotIndex < 0 || slotIndex >= currentSlots)
            return false;
        
        // Se já tem algo equipado, desquipa
        Item previousItem = equippedItems[slotIndex];
        
        equippedItems[slotIndex] = item;
        onEquipmentChanged?.Invoke(slotIndex);
        
        return true;
    }
    
    // Desequipar um item
    public Item UnequipItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= currentSlots)
            return null;
        
        Item item = equippedItems[slotIndex];
        equippedItems[slotIndex] = null;
        onEquipmentChanged?.Invoke(slotIndex);
        
        return item;
    }
    
    // Encontra um slot de equipamento vazio
    public int FindEmptyEquipmentSlot()
    {
        for (int i = 0; i < currentSlots; i++)
        {
            if (equippedItems[i] == null)
                return i;
        }
        return -1;
    }
    
    // Retorna o item equipado num slot
    public Item GetEquippedItem(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < currentSlots)
            return equippedItems[slotIndex];
        return null;
    }
    
    // Retorna todos os itens equipados
    public Item[] GetAllEquippedItems()
    {
        return equippedItems;
    }
    
    // Retorna o número de slots de equipamento
    public int GetEquipmentSlotCount()
    {
        return currentSlots;
    }
    
    // Compra um novo slot de equipamento
    public void BuyEquipmentSlot()
    {
        currentSlots++;
        System.Array.Resize(ref equippedItems, currentSlots);
        onSlotsIncreased?.Invoke(currentSlots);
    }
    
    // Verifica quantos slots ainda estão vazios
    public int GetEmptySlotCount()
    {
        int count = 0;
        for (int i = 0; i < currentSlots; i++)
        {
            if (equippedItems[i] == null)
                count++;
        }
        return count;
    }
}
