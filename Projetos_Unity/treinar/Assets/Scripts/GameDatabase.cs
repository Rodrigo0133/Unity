using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string lastSceneName = "";
    public float posX = 0f, posY = 0f, posZ = 0f;

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
        if (!data.ownedAmulets.Contains(amuletName))
        {
            data.ownedAmulets.Add(amuletName);
            
        }
    }

    public bool EquipAmulet(string amuletName)
    {
        if (!data.ownedAmulets.Contains(amuletName))
        {
            
            return false;
        }
        int maxSlots = data.hasUnlockedThirdSlot ? 3 : 2;
        if (data.equippedAmulets.Count >= maxSlots)
        {
           
            return false;
        }

        if (!data.equippedAmulets.Contains(amuletName))
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
        return data.ownedAmulets.Contains(amuletName);
    }

    public bool IsEquipped(string amuletName)
    {
        return data.equippedAmulets.Contains(amuletName);
    }

    

    public void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.posX = player.transform.position.x;
            data.posY = player.transform.position.y;
            data.posZ = player.transform.position.z;
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
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        data = new SaveData();
    }
}