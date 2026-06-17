using UnityEngine;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private int gridWidth = 5;
    [SerializeField] private int gridHeight = 4;
    
    private InventorySlot[] slots;
    private int totalSlots;
    
    // Evento para notificar quando o inventário muda
    public UnityEvent<int> onInventoryChanged = new UnityEvent<int>();
    
    private void Awake()
    {
        totalSlots = gridWidth * gridHeight;
        slots = new InventorySlot[totalSlots];
        
        // Inicializa todos os slots como vazios
        for (int i = 0; i < totalSlots; i++)
        {
            slots[i] = new InventorySlot();
        }
    }
    
    // Tenta adicionar um item ao inventário
    public bool AddItem(Item itemToAdd, int quantity = 1)
    {
        // Primeiro, tenta adicionar a slots existentes do mesmo item
        for (int i = 0; i < totalSlots; i++)
        {
            if (slots[i].item == itemToAdd && !slots[i].IsEmpty())
            {
                if (slots[i].AddItem(itemToAdd, quantity))
                {
                    onInventoryChanged?.Invoke(i);
                    return true; // Coube tudo num slot existente
                }
                else
                {
                    // Coube parcialmente, adiciona o resto
                    quantity -= (itemToAdd.maxStackSize - slots[i].quantity);
                    onInventoryChanged?.Invoke(i);
                }
            }
        }
        
        // Se sobrou quantidade, tenta slots vazios
        while (quantity > 0)
        {
            int emptySlot = FindEmptySlot();
            if (emptySlot == -1)
                return false; // Inventário cheio
            
            int amountToAdd = Mathf.Min(quantity, itemToAdd.maxStackSize);
            slots[emptySlot].AddItem(itemToAdd, amountToAdd);
            quantity -= amountToAdd;
            onInventoryChanged?.Invoke(emptySlot);
        }
        
        return true;
    }
    
    // Remove um item do inventário
    public bool RemoveItem(Item itemToRemove, int quantity = 1)
    {
        int quantityToRemove = quantity;
        
        for (int i = 0; i < totalSlots; i++)
        {
            if (slots[i].item == itemToRemove && !slots[i].IsEmpty())
            {
                int amountToRemoveFromSlot = Mathf.Min(quantityToRemove, slots[i].quantity);
                slots[i].RemoveItem(amountToRemoveFromSlot);
                quantityToRemove -= amountToRemoveFromSlot;
                onInventoryChanged?.Invoke(i);
                
                if (quantityToRemove <= 0)
                    return true;
            }
        }
        
        return quantityToRemove <= 0;
    }
    
    // Encontra um slot vazio
    private int FindEmptySlot()
    {
        for (int i = 0; i < totalSlots; i++)
        {
            if (slots[i].IsEmpty())
                return i;
        }
        return -1;
    }
    
    // Retorna o slot num índice específico
    public InventorySlot GetSlot(int index)
    {
        if (index >= 0 && index < totalSlots)
            return slots[index];
        return null;
    }
    
    // Retorna todos os slots
    public InventorySlot[] GetAllSlots()
    {
        return slots;
    }
    
    // Retorna o total de slots
    public int GetTotalSlots()
    {
        return totalSlots;
    }
    
    // Retorna as dimensões da grid
    public (int width, int height) GetGridDimensions()
    {
        return (gridWidth, gridHeight);
    }
    
    // Move um item de um slot para outro
    public bool MoveItem(int fromSlotIndex, int toSlotIndex)
    {
        if (fromSlotIndex < 0 || fromSlotIndex >= totalSlots ||
            toSlotIndex < 0 || toSlotIndex >= totalSlots)
            return false;
        
        InventorySlot fromSlot = slots[fromSlotIndex];
        InventorySlot toSlot = slots[toSlotIndex];
        
        if (fromSlot.IsEmpty())
            return false;
        
        // Se o slot destino está vazio, move direto
        if (toSlot.IsEmpty())
        {
            toSlot.item = fromSlot.item;
            toSlot.quantity = fromSlot.quantity;
            fromSlot.item = null;
            fromSlot.quantity = 0;
            onInventoryChanged?.Invoke(fromSlotIndex);
            onInventoryChanged?.Invoke(toSlotIndex);
            return true;
        }
        
        // Se são o mesmo item e há espaço, tenta combinar
        if (toSlot.item == fromSlot.item && toSlot.quantity < fromSlot.item.maxStackSize)
        {
            int spaceAvailable = fromSlot.item.maxStackSize - toSlot.quantity;
            int amountToMove = Mathf.Min(spaceAvailable, fromSlot.quantity);
            
            toSlot.quantity += amountToMove;
            fromSlot.RemoveItem(amountToMove);
            onInventoryChanged?.Invoke(fromSlotIndex);
            onInventoryChanged?.Invoke(toSlotIndex);
            return true;
        }
        
        return false;
    }
}
