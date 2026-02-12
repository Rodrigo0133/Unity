#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class Startup
{
    static Startup()    
    {
        EditorPrefs.SetInt("showCounts_sportcarcgb4", EditorPrefs.GetInt("showCounts_sportcarcgb4") + 1);

        if (EditorPrefs.GetInt("showCounts_sportcarcgb4") == 1)       
        {
            Application.OpenURL("https://assetstore.unity.com/packages/slug/354060");
            // System.IO.File.Delete("Assets/SportCar/Racing_Game.cs");
        }
    }     
}
#endif
