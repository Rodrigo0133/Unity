using UnityEngine;

public class AmuletDatabase : MonoBehaviour
{
    public static bool IsEquipped(string amuletName)
    {
        if (GameDatabase.Instance == null || GameDatabase.Instance.data == null)
            return false;

        return GameDatabase.Instance.data.equippedAmulets.Contains(amuletName);
    }

    public static bool IsOwned(string amuletName)
    {
        if (GameDatabase.Instance == null || GameDatabase.Instance.data == null)
            return false;

        return GameDatabase.Instance.data.ownedAmulets.Contains(amuletName);
    }
}