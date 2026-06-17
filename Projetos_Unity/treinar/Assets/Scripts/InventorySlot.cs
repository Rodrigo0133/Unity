using UnityEngine;

// Classe que representa um slot no inventário
[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int quantity;
    
    public InventorySlot()
    {
        item = null;
        quantity = 0;
    }
    
    public InventorySlot(Item newItem, int newQuantity)
    {
        item = newItem;
        quantity = newQuantity;
    }
    
    // Adiciona itens ao slot
    public bool AddItem(Item newItem, int quantityToAdd = 1)
    {
        if (item == null)
        {
            item = newItem;
            quantity = quantityToAdd;
            return true;
        }
        
        if (item == newItem && quantity < item.maxStackSize)
        {
            int availableSpace = item.maxStackSize - quantity;
            int actualAdd = Mathf.Min(quantityToAdd, availableSpace);
            quantity += actualAdd;
            return quantityToAdd <= availableSpace; // True se coube tudo
        }
        
        return false;
    }
    
    // Remove itens do slot
    public void RemoveItem(int quantityToRemove)
    {
        quantity -= quantityToRemove;
        if (quantity <= 0)
        {
            item = null;
            quantity = 0;
        }
    }
    
    // Verifica se o slot está vazio
    public bool IsEmpty()
    {
        return item == null || quantity <= 0;
    }
}
