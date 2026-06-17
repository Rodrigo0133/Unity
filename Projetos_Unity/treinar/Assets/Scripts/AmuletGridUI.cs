using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmuletGridUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform gridParent;               // Parent object with GridLayoutGroup
    public GameObject amuletItemPrefab;        // Prefab containing a Button and TMP_Text fields

    private void OnEnable()
    {
        PopulateGrid();
    }

    /// <summary>
    /// Clears the existing grid and populates it with all owned amulets.
    /// </summary>
    private void PopulateGrid()
    {
        // Remove previous items
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        if (GameDatabase.Instance == null) return;
        List<string> owned = GameDatabase.Instance.data.ownedAmulets;

        foreach (string id in owned)
        {
            AmuletInfo info = AmuletDatabase.GetById(id);
            if (info == null) continue;

            GameObject go = Instantiate(amuletItemPrefab, gridParent);
            // Expect prefab hierarchy: Root has Button component; children "Icon", "Name" and "Cost"
            Button btn = go.GetComponent<Button>();
            TMP_Text nameTxt = go.transform.Find("Name")?.GetComponent<TMP_Text>();
            TMP_Text costTxt = go.transform.Find("Cost")?.GetComponent<TMP_Text>();
            Image iconImg = go.transform.Find("Icon")?.GetComponent<Image>();

            if (nameTxt != null) nameTxt.text = info.name;
            if (costTxt != null) costTxt.text = $"Cost: {info.cost}";
            // If you later add a Sprite field to AmuletInfo, you can assign it here:
            // if (iconImg != null && info.icon != null) iconImg.sprite = info.icon;

            string capturedId = id; // capture for the lambda
            btn.onClick.AddListener(() =>
            {
                // Toggle equip/unequip via ShopManager (EquipAmulet already handles both)
                ShopManager.Instance.EquipAmulet(capturedId);
                // Refresh UI elsewhere (slots, etc.)
                AmuletUI amuletUI = FindObjectOfType<AmuletUI>();
                if (amuletUI != null) amuletUI.AtualizarUI();
                // Re‑populate the grid to reflect the new state
                PopulateGrid();
            });
        }
    }
}
