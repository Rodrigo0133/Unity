using UnityEngine;

public class Faladaloja : MonoBehaviour
{
    public GameObject Abrirloja;
    private bool jogadorPerto = false;
    private inventarioeloja inventarioeloja;

    void Start()
    {
        jogadorPerto = false;
        Abrirloja.SetActive(false);
        inventarioeloja = FindObjectOfType<inventarioeloja>();
    }

    void Update()
    {
        if (jogadorPerto && Input.GetKeyDown(KeyCode.E))
        {
            inventarioeloja.Abrirloja();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        jogadorPerto = true;
        Abrirloja.SetActive(true);

        Debug.Log("Jogador entrou no trigger do NPC.", this);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        jogadorPerto = false;
        inventarioeloja.Sairdomenu();
        Abrirloja.SetActive(false);
        Debug.Log("Jogador saiu do trigger do NPC.", this);
    }
}