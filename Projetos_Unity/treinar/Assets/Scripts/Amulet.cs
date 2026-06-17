 using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AmuletInfo
{
    public string id;
    public string name;
    public string description;
    public int cost;

    public AmuletInfo(string id, string name, string description, int cost)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.cost = cost;
    }
}

public static class AmuletDatabase
{
    public static readonly List<AmuletInfo> AllAmulets = new List<AmuletInfo>()
    {
        new AmuletInfo("vitalidade", "Amuleto de Vitalidade", "+1 Coração de vida máxima.", 50),
        new AmuletInfo("furia", "Amuleto de Fúria", "+25% Dano quando vida está em 20% ou menos.", 75),
        new AmuletInfo("ganancia", "Amuleto de Ganância", "Inimigos derrotados dropam o dobro de Plets.", 60),
        new AmuletInfo("rapidez", "Amuleto de Rapidez", "+25% Velocidade de movimento.", 45),
        new AmuletInfo("escudo", "Amuleto de Escudo", "+1s de tempo de invencibilidade após tomar dano.", 55)
    };

    public static AmuletInfo GetById(string id)
    {
        return AllAmulets.Find(a => a.id.ToLower() == id.ToLower());
    }

    public static bool IsEquipped(string id)
    {
        if (GameDatabase.Instance == null || GameDatabase.Instance.data == null) return false;
        return GameDatabase.Instance.data.equippedAmulets.Contains(id);
    }
}
