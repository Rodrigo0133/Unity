using UnityEngine;
using UnityEngine.Video;

public class InimigoIA_QuadradosVermelho : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 2f;
    public float distance = 0.4f;
    public bool isRight = true;
    public Transform GroundCheck;
    public LayerMask groundLayer;
    private float VIDA = 50f;

    [Header("Dano")]
    [SerializeField] public float dano;

    

    private bool virou = false;
    

    void Update()
    {
        
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        
        RaycastHit2D ground = Physics2D.Raycast(
            GroundCheck.position,
            Vector2.down,
            distance,
            groundLayer
        );

        if (ground.collider == null && !virou)
        {
           
            transform.eulerAngles = new Vector3(0, isRight ? -180 : 0, 0);
            isRight = !isRight;
            virou = true;
        }
        else if (ground.collider != null)
        {
            virou = false;
        }
    }

    [System.Obsolete]
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        float alturaPlayer = collision.transform.position.y;
        float alturaInimigo = transform.position.y;

        
        if (alturaPlayer > alturaInimigo + 0.3f)
        {
            
        }
        else
        {
            
            PlayerMovement player = collision.transform.GetComponentInParent<PlayerMovement>();
            PlayerLives playerlive = collision.transform.GetComponentInParent<PlayerLives>();
            if (player != null)
            {
                player.TakeDamage(dano);
                
            }
            else
            {
                Debug.LogError("PlayerMovement não encontrado no Player!");
            }
        }
    }

    void Morrer()
    {
        PletCoin.Spawn(transform.position, Random.Range(2, 4));
        Destroy(gameObject);
    }

   
    private void OnDrawGizmos()
    {
        if (GroundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(GroundCheck.position, GroundCheck.position + Vector3.down * distance);
        }
    }
    public void TakeDamage()
    {
        VIDA -= 1;
        if (VIDA <= 0)
        {
            Morrer();
        }
    }
}
