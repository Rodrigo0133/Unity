using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    public Transform target;
    public float followSpeed = 5f;
    public float yOffset = 1f;

    private float currentShakeDuration = 0f;
    private float currentShakeMagnitude = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = new Vector3(
            target.position.x,
            target.position.y + yOffset,
            -10f
        );

        // Se estiver a tremer, adiciona ruído aleatório
        if (currentShakeDuration > 0)
        {
            Vector3 shakeOffset = Random.insideUnitCircle * currentShakeMagnitude;
            targetPosition += shakeOffset;
            
            // O tempo passa mesmo em Hit Stop, por isso usamos deltaTime não-afetado pela escala (unscaledDeltaTime)
            currentShakeDuration -= Time.unscaledDeltaTime;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.unscaledDeltaTime // Usa unscaled para a câmera não encravar no Hit Stop
        );
    }

    /// <summary>
    /// Faz o ecrã tremer durante um período de tempo.
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        currentShakeDuration = duration;
        currentShakeMagnitude = magnitude;
    }
}
