using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    // Last position & scene
    public string lastSceneName = "";
    public float posX = 0f;
    public float posY = 0f;
    public float posZ = 0f;

    // Currency and items
    public int plets = 0;
    public int swordLevel = 1; // 1 = Wooden, 2 = Iron, 3 = Golden, etc.
    public int keysCount = 0;
    public bool hasUnlockedThirdSlot = false;

    // Amulets
    public List<string> ownedAmulets = new List<string>();
    public List<string> equippedAmulets = new List<string>();

    // Settings
    public float masterVolume = 1f;
    public bool isFullscreen = true;
}

public class GameDatabase : MonoBehaviour
{
    public static GameDatabase Instance { get; private set; }

    [Header("Current Save State")]
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

    public void SaveGame()
    {
        // Try to update current player position if they exist in the scene
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
        Debug.Log("[GameDatabase] Jogo Guardado! JSON:\n" + json);
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("[GameDatabase] Jogo Carregado!");
        }
        else
        {
            data = new SaveData();
            // Default setup: start with no amulets and basic sword
            data.ownedAmulets = new List<string>();
            data.equippedAmulets = new List<string>();
            data.plets = 0;
            data.swordLevel = 1;
            Debug.Log("[GameDatabase] Sem save existente. Iniciando novo jogo.");
        }
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        data = new SaveData();
        Debug.Log("[GameDatabase] Save apagado.");
    }
}
