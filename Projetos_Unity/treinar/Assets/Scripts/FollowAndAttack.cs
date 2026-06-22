using UnityEngine;

public class FollowAndAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("Prefab do ataque. Deve conter um PolygonCollider2D que define a área do ataque.")]
    [SerializeField] private GameObject attackPrefab = null;

    [Tooltip("Distância máxima à frente do personagem onde o ataque pode aparecer. Se o rato estiver mais perto, o ataque aparece exatamente onde o rato está.")]
    [SerializeField] private float offset = 1.0f;

    [Tooltip("Tempo entre ataques (segundos).")]
    [SerializeField] private float cooldown = 0.5f;

    [Tooltip("Tempo de vida do objeto de ataque instanciado (segundos).")]
    [SerializeField] private float lifetime = 0.2f;

    [Tooltip("Se verdadeiro, o ataque será parentado ao jogador. Se falso, ficará na cena raiz.")]
    [SerializeField] private bool parentAttack = false;

    private float nextAttackTime = 0f;

    private void Reset()
    {
        // Valores padrão úteis ao criar o componente
        offset = 1f;
        cooldown = 0.5f;
        lifetime = 0.2f;
        parentAttack = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            DoAttack();
        }
    }

    private void DoAttack()
    {
        if (attackPrefab == null)
        {
            Debug.LogWarning("[FollowAndAttack] attackPrefab não atribuído.");
            nextAttackTime = Time.time + cooldown;
            return;
        }

        // Pega posição do mouse no mundo (2D)
        Vector3 mouseScreen = Input.mousePosition;
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[FollowAndAttack] Camera.main é nula.");
            nextAttackTime = Time.time + cooldown;
            return;
        }

        Vector3 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);
        // Garantir z consistente com o jogo 2D / objeto
        Vector3 origin = transform.position;
        mouseWorld.z = origin.z;

        Vector2 dir = (Vector2)(mouseWorld - origin);
        float dist = dir.magnitude;

        Vector2 dirNorm = dir;
        if (dist <= 0.0001f)
        {
            // Rato exatamente sobre o personagem: manter spawn no mouse (que coincide com origin)
            dirNorm = Vector2.right; // fallback para cálculo de rotação
        }
        else
        {
            dirNorm = dir / dist;
        }

        Vector3 spawnPos;
        // Se houver offset positivo e o mouse estiver além desse alcance, limitar ao offset.
        if (offset > 0f && dist > offset)
        {
            spawnPos = origin + (Vector3)(dirNorm * offset);
        }
        else
        {
            // Caso contrário, spawn exatamente onde o rato está (mesmo se estiver sobre o personagem)
            spawnPos = mouseWorld;
        }

        float angle = Mathf.Atan2(dirNorm.y, dirNorm.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0f, 0f, angle);

        GameObject atk = Instantiate(attackPrefab, spawnPos, rot);
        if (parentAttack)
        {
            atk.transform.SetParent(transform, true);
        }

        // Verifica se o prefab tem PolygonCollider2D
        if (atk.GetComponent<PolygonCollider2D>() == null && atk.GetComponentInChildren<PolygonCollider2D>() == null)
        {
            Debug.LogWarning("[FollowAndAttack] O ataque instanciado não tem um PolygonCollider2D. A forma do ataque deve ser definida por um PolygonCollider2D.");
        }

        Ataque ataque = atk.GetComponent<Ataque>();
        if (ataque == null)
            ataque = atk.GetComponentInChildren<Ataque>(true);

        if (ataque != null)
            ataque.AtivarAtaque();
        // Destrói após lifetime
        if (lifetime > 0f)
        {
            Destroy(atk, lifetime);
        }

        nextAttackTime = Time.time + cooldown;
    }
}
