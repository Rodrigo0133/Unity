using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string lastSceneName = "";
    public float posX = 0f;
    public float posY = 0f;
    public float posZ = 0f;
    public bool hasPlayerPosition = false;

    public int plets = 0;
    public int swordLevel = 1;
    public int keysCount = 0;
    public bool hasUnlockedThirdSlot = false;

    public List<string> ownedAmulets = new List<string>();
    public List<string> equippedAmulets = new List<string>();

    public float masterVolume = 1f;
    public bool isFullscreen = true;
}

public class GameDatabase : MonoBehaviour
{
    public static GameDatabase Instance { get; private set; }

    public SaveData data = new SaveData();

    private const string SaveKey = "SaveDataKey";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddOwnedAmulet(string amuletName)
    {
        if (!ContainsAmulet(data.ownedAmulets, amuletName))
        {
            data.ownedAmulets.Add(amuletName);
            
        }
    }

    public void AddPlets(int amount)
    {
        if (amount <= 0)
        {
            data.plets += amount;
            SaveGame();
            return;
        }

        float multiplier = IsEquipped("Ganacia") ? 1.2f : 1f;
        data.plets += Mathf.RoundToInt(amount * multiplier);
        SaveGame();
    }

    public bool EquipAmulet(string amuletName)
    {
        if (!ContainsAmulet(data.ownedAmulets, amuletName))
        {
            
            return false;
        }
        int maxSlots = data.hasUnlockedThirdSlot ? 3 : 2;
        if (data.equippedAmulets.Count >= maxSlots)
        {
           
            return false;
        }

        if (!ContainsAmulet(data.equippedAmulets, amuletName))
        {
            data.equippedAmulets.Add(amuletName);
            SaveGame();
            return true;
        }
        return false;
    }

    public bool UnequipAmulet(string amuletName)
    {
        if (data.equippedAmulets.Remove(amuletName))
        {
            SaveGame();
            return true;
        }
        return false;
    }

    public bool HasAmulet(string amuletName)
    {
        return ContainsAmulet(data.ownedAmulets, amuletName);
    }

    public bool IsEquipped(string amuletName)
    {
        return ContainsAmulet(data.equippedAmulets, amuletName);
    }

    private bool ContainsAmulet(List<string> amulets, string amuletName)
    {
        if (amulets == null)
            return false;

        foreach (string amulet in amulets)
        {
            if (string.Equals(amulet, amuletName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.posX = player.transform.position.x;
            data.posY = player.transform.position.y;
            data.posZ = player.transform.position.z;
            data.hasPlayerPosition = true;
            data.lastSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }

        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
        }
        else
        {
            data = new SaveData();
        }

        if (data.ownedAmulets == null)
            data.ownedAmulets = new List<string>();

        if (data.equippedAmulets == null)
            data.equippedAmulets = new List<string>();
    }

    public string GetSceneToLoad(string defaultSceneName)
    {
        LoadGame();

        if (!string.IsNullOrWhiteSpace(data.lastSceneName))
            return data.lastSceneName;

        return defaultSceneName;
    }

    public static string GetSavedSceneToLoad(string defaultSceneName)
    {
        if (!PlayerPrefs.HasKey(SaveKey))
            return defaultSceneName;

        string json = PlayerPrefs.GetString(SaveKey);
        SaveData savedData = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();

        if (!string.IsNullOrWhiteSpace(savedData.lastSceneName))
            return savedData.lastSceneName;

        return defaultSceneName;
    }

    public bool TryGetSavedPlayerPosition(string sceneName, out Vector3 position)
    {
        position = Vector3.zero;

        if (!data.hasPlayerPosition || string.IsNullOrWhiteSpace(data.lastSceneName))
            return false;

        if (!string.Equals(data.lastSceneName, sceneName, System.StringComparison.Ordinal))
            return false;

        position = new Vector3(data.posX, data.posY, data.posZ);
        return true;
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        data = new SaveData();
    }
}
