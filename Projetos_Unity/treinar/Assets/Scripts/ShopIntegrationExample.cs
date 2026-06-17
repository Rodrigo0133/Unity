using UnityEngine;

// Exemplo de como integrar com a tua loja existente
public class ShopIntegrationExample : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private EquipmentManager equipmentManager;
    [SerializeField] private EquipmentSlotShop equipmentSlotShop;
    
    // Variável para guardar a moeda do jogador
    private int playerMoney = 500;
    
    private void Start()
    {
        // Inicializa a loja com a moeda do jogador
        if (equipmentSlotShop != null)
        {
            equipmentSlotShop.SetPlayerMoney(playerMoney);
        }
    }
    
    private void Update()
    {
        // Exemplo: Pressiona E para adicionar moeda
        if (Input.GetKeyDown(KeyCode.E))
        {
            AddMoney(50);
            Debug.Log($"Adicionaste 50 moedas! Total: {playerMoney}");
        }
        
        // Exemplo: Pressiona Q para equipar um item de teste
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TestEquipItem();
        }
        
        // Exemplo: Pressiona R para desequipar
        if (Input.GetKeyDown(KeyCode.R))
        {
            TestUnequipItem();
        }
    }
    
    // Função para adicionar moeda
    public void AddMoney(int amount)
    {
        playerMoney += amount;
        
        if (equipmentSlotShop != null)
        {
            equipmentSlotShop.SetPlayerMoney(playerMoney);
        }
    }
    
    // Função para remover moeda (quando compra)
    public void RemoveMoney(int amount)
    {
        if (playerMoney >= amount)
        {
            playerMoney -= amount;
            
            if (equipmentSlotShop != null)
            {
                equipmentSlotShop.SetPlayerMoney(playerMoney);
            }
            
            return;
        }
        
        Debug.Log("Moeda insuficiente!");
    }
    
    // Obtém a moeda do jogador
    public int GetPlayerMoney()
    {
        return playerMoney;
    }
    
    // Exemplo: Equipar um item
    private void TestEquipItem()
    {
        // Se tens itens no inventário, tenta equipar o primeiro
        var allSlots = inventoryManager.GetAllSlots();
        
        foreach (var slot in allSlots)
        {
            if (!slot.IsEmpty())
            {
                if (equipmentManager.EquipItem(slot.item))
                {
                    Debug.Log($"Item '{slot.item.itemName}' equipado!");
                    inventoryManager.RemoveItem(slot.item, 1);
                }
                break;
            }
        }
    }
    
    // Exemplo: Desequipar um item
    private void TestUnequipItem()
    {
        Item unequippedItem = equipmentManager.UnequipItem(0);
        
        if (unequippedItem != null)
        {
            inventoryManager.AddItem(unequippedItem, 1);
            Debug.Log($"Item '{unequippedItem.itemName}' desequipado!");
        }
    }
    
    // Função para comprar um slot (chama esta quando o jogador clica)
    public void BuyEquipmentSlot()
    {
        // O EquipmentSlotShop já trata da moeda
        // Mas podes fazer verificações adicionais aqui
        
        if (playerMoney >= 100) // 100 é o preço padrão
        {
            // O EquipmentSlotShop.BuySlot() vai remover a moeda
            Debug.Log("Podes comprar um novo slot!");
        }
        else
        {
            Debug.Log("Moeda insuficiente para comprar um slot!");
        }
    }
}
