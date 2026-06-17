using UnityEngine;

// ScriptableObject para definir cada tipo de item
[CreateAssetMenu(fileName = "Item_", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public int maxStackSize = 1; // Quantos itens podem estar no mesmo slot
    
    [TextArea(3, 5)]
    public string itemDescription;
}
