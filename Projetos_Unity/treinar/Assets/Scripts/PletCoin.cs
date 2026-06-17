using UnityEngine;

public class PletCoin : MonoBehaviour
{
    public int value = 5;
    public float magnetRange = 5f;
    public float magnetSpeed = 8f;

    private Transform player;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // Apply a random initial pop force to look natural when dropped
        if (rb != null)
        {
            float forceX = Random.Range(-3f, 3f);
            float forceY = Random.Range(4f, 8f);
            rb.linearVelocity = new Vector2(forceX, forceY);
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= magnetRange)
        {
            // Disable gravity so it flies cleanly to the player
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
            }
            transform.position = Vector2.MoveTowards(transform.position, player.position, magnetSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectCoin();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CollectCoin();
        }
    }

    private void CollectCoin()
    {
        if (GameDatabase.Instance != null)
        {
            // Apply GoldenGreed amulet double coins modifier
            int finalValue = value;
            if (AmuletDatabase.IsEquipped("ganancia"))
            {
                finalValue *= 2;
            }

            GameDatabase.Instance.data.plets += finalValue;
            GameDatabase.Instance.SaveGame();
            Debug.Log($"[Plet] +{finalValue} Plets! Total: {GameDatabase.Instance.data.plets}");
        }

        // Play particle or sound here if any
        Destroy(gameObject);
    }

    public static void Spawn(Vector3 position, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject coin = new GameObject("PletCoin");
            coin.transform.position = position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
            coin.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
            coin.tag = "Collectible"; // or general tag

            SpriteRenderer sr = coin.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.85f, 0f); // Gold color
            // Use standard Unity UI sprite if possible, else it defaults to colored block
            sr.sprite = Resources.GetBuiltinResource<Sprite>("Knob.psd");

            CircleCollider2D col = coin.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            Rigidbody2D rb = coin.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1.5f;

            coin.AddComponent<PletCoin>();
        }
    }
}
