using System.Collections;
using UnityEngine;

public class GameFeelManager : MonoBehaviour
{
    public static GameFeelManager Instance { get; private set; }

    [Header("Hit Stop Configs")]
    [Tooltip("Tempo em segundos reais que o jogo congela ao dar um hit")]
    public float hitStopDuration = 0.05f;

    private bool isHitStopping = false;

    private void Awake()
    {
        // Singleton Pattern simples
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Congela o jogo temporariamente para dar sensação de impacto pesado.
    /// </summary>
    public void TriggerHitStop()
    {
        if (isHitStopping) return;
        StartCoroutine(HitStopRoutine());
    }

    private IEnumerator HitStopRoutine()
    {
        isHitStopping = true;
        
        // Congela o tempo do Unity
        Time.timeScale = 0f;
        
        // Espera no mundo real (como o timeScale é 0, não podemos usar WaitForSeconds)
        yield return new WaitForSecondsRealtime(hitStopDuration);
        
        // Restaura o tempo
        Time.timeScale = 1f;
        
        isHitStopping = false;
    }
}
