using UnityEngine;

public class AmuletDatabase : MonoBehaviour
{
    public static bool IsEquipped(string amuletName)
    {
        if (GameDatabase.Instance == null || GameDatabase.Instance.data == null)
            return false;

        return ContainsAmulet(GameDatabase.Instance.data.equippedAmulets, amuletName);
    }

    public static bool IsOwned(string amuletName)
    {
        if (GameDatabase.Instance == null || GameDatabase.Instance.data == null)
            return false;

        return ContainsAmulet(GameDatabase.Instance.data.ownedAmulets, amuletName);
    }

    private static bool ContainsAmulet(System.Collections.Generic.List<string> amulets, string amuletName)
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
}
