using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Shop Configs")]
    public int slot3Cost = 100;
    
    // Sword Upgrade Costs & Damage values
    public int[] swordUpgradeCosts = new int[] { 0, 80, 150, 250 }; // levels 1 to 4 (cost to reach that level)
    public int[] swordDamageValues = new int[] { 0, 25, 50, 75, 100 }; // damage based on sword level

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool BuyAmulet(string id)
    {
        if (GameDatabase.Instance == null) return false;
        SaveData data = GameDatabase.Instance.data;

        AmuletInfo info = AmuletDatabase.GetById(id);
        if (info == null)
        {
            Debug.LogWarning("[Shop] Amuleto não encontrado: " + id);
            return false;
        }

        if (data.ownedAmulets.Contains(id))
        {
            Debug.Log("[Shop] Já possuis este amuleto!");
            return false;
        }

        if (data.plets >= info.cost)
        {
            data.plets -= info.cost;
            data.ownedAmulets.Add(id);
            GameDatabase.Instance.SaveGame();
            Debug.Log($"[Shop] Comprado: {info.name}! Restam {data.plets} Plets.");
            return true;
        }
        else
        {
            Debug.Log("[Shop] Plets insuficientes para comprar " + info.name);
            return false;
        }
    }

    public bool EquipAmulet(string id)
    {
        if (GameDatabase.Instance == null) return false;
        SaveData data = GameDatabase.Instance.data;

        if (!data.ownedAmulets.Contains(id))
        {
            Debug.LogWarning("[Shop] Não possuis este amuleto!");
            return false;
        }

        if (data.equippedAmulets.Contains(id))
        {
            // Unequip
            data.equippedAmulets.Remove(id);
            GameDatabase.Instance.SaveGame();
            ApplyAmuletEffectsToPlayer();
            Debug.Log("[Shop] Desequipado: " + id);
            return true;
        }

        int maxSlots = data.hasUnlockedThirdSlot ? 3 : 2;
        if (data.equippedAmulets.Count < maxSlots)
        {
            data.equippedAmulets.Add(id);
            GameDatabase.Instance.SaveGame();
            ApplyAmuletEffectsToPlayer();
            Debug.Log("[Shop] Equipado com sucesso: " + id);
            return true;
        }
        else
        {
            Debug.Log($"[Shop] Todos os {maxSlots} slots estão cheios!");
            return false;
        }
    }

    public bool BuyThirdSlot()
    {
        if (GameDatabase.Instance == null) return false;
        SaveData data = GameDatabase.Instance.data;

        if (data.hasUnlockedThirdSlot)
        {
            Debug.Log("[Shop] 3º slot já desbloqueado.");
            return false;
        }

        if (data.plets >= slot3Cost)
        {
            data.plets -= slot3Cost;
            data.hasUnlockedThirdSlot = true;
            GameDatabase.Instance.SaveGame();
            Debug.Log("[Shop] Desbloqueado 3º slot de amuleto!");
            return true;
        }
        else
        {
            Debug.Log("[Shop] Plets insuficientes para o 3º slot.");
            return false;
        }
    }

    public bool UpgradeSword()
    {
        if (GameDatabase.Instance == null) return false;
        SaveData data = GameDatabase.Instance.data;

        int nextLevel = data.swordLevel + 1;
        if (nextLevel > 4)
        {
            Debug.Log("[Shop] Espada já está no nível máximo!");
            return false;
        }

        int cost = swordUpgradeCosts[nextLevel - 1];
        if (data.plets >= cost)
        {
            data.plets -= cost;
            data.swordLevel = nextLevel;
            GameDatabase.Instance.SaveGame();
            ApplySwordEffectsToPlayer();
            Debug.Log($"[Shop] Espada melhorada para nível {nextLevel}! Dano base agora é: {swordDamageValues[nextLevel]}");
            return true;
        }
        else
        {
            Debug.Log($"[Shop] Plets insuficientes para melhorar a espada. Custo: {cost}");
            return false;
        }
    }

    public void ApplyAmuletEffectsToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                // Trigger health recalculation on PlayerMovement
                pm.Start(); 
            }
        }
    }

    public void ApplySwordEffectsToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                int currentLvl = GameDatabase.Instance.data.swordLevel;
                pm.damage = swordDamageValues[Mathf.Clamp(currentLvl, 1, 4)];
            }
        }
    }

    public void AddPlets(int amount)
    {
        if (GameDatabase.Instance == null) return;
        GameDatabase.Instance.data.plets += amount;
        GameDatabase.Instance.SaveGame();
    }
}
