using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;


public class Movimento : MonoBehaviour
{
    public Rigidbody jogador;
    public TextMeshProUGUI vida;
    public TextMeshProUGUI pontos;
    private int vidaAtual = 10;
    private int ponto = 0;
    void Start()
    {
        ponto = 0;
        vidaAtual = 10;
        vida.text = "Vida: 10";
        pontos.text = "Pontos: 0";
    }

    
    void Update()
    {

        jogador.AddForce(0, 0, 25);
        if(Keyboard.current.aKey.isPressed)
        {
            jogador.AddForce(-15, 0, 0);
        }
        if(Keyboard.current.dKey.isPressed)
        {
            jogador.AddForce(15, 0, 0);
        }
    }

  public void OnCollisionEnter(Collision bateu)
    {
        if (bateu.collider.tag == "Parede")
        {
           transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
            Destroy(bateu.gameObject);
            Vida();
        }
    }
    public void OnTriggerEnter(Collider bateu)
    {
        if (bateu.CompareTag("Pontos"))
        {
            ponto += 10;
            pontos.text = "Pontos: " + ponto;
            Destroy(bateu.gameObject);
        }
    }
    public void Vida()
    {
        vidaAtual--;
        vida.text = "Vida: " + vidaAtual;
    }

}
