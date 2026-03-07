

using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Movimento : MonoBehaviour
{
    private Rigidbody jogador;
    public TextMeshProUGUI vida;
    public TextMeshProUGUI pontos;
    private int vidaAtual = 100;
    private int ponto = 0;
    public GameObject Perdeu;
    public GameObject Pontos;
    public TextMeshProUGUI Perder;
    bool jaPerdeu = false;
    public GameObject Creditos;
    public TextMeshProUGUI creditos;
  
    private float horizontalInput = 0f;
    public float forwardForce = 25f;
    public float lateralForce = 15f;
    public float maxLateralSpeed = 8f;
    public float VelocityCreds;

    void Start()
    {
        ponto = 0;
        vidaAtual = 100;
        vida.text = "Vida: 10";
        pontos.text = "Pontos: 0";
        jogador = GetComponent<Rigidbody>();
        Perdeu.SetActive(false);
        Pontos.SetActive(true);
    }

    void Update()
    {
    
        jogador.AddForce(Vector3.forward * forwardForce);

        
        float touchInput = 0f;

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == UnityEngine.TouchPhase.Began || t.phase == UnityEngine.TouchPhase.Moved || t.phase == UnityEngine.TouchPhase.Stationary)
            {
                Vector2 touchPos = t.position;
                touchInput = (touchPos.x < Screen.width / 2) ? -1f : 1f;
            }
        }
        
        else if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Input.mousePosition;
            touchInput = (mousePos.x < Screen.width / 2) ? -1f : 1f;
        }
        
        else
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed) jogador.AddForce(Vector3.left * lateralForce);
                else if (Keyboard.current.dKey.isPressed) jogador.AddForce(Vector3.right*lateralForce);
                else touchInput = 0f;
            }
        }

        horizontalInput = touchInput;

      
        Vector3 lateralForceVec = Vector3.right * horizontalInput * lateralForce;
        jogador.AddForce(lateralForceVec);

       

        Vector3 vel = jogador.linearVelocity;
        vel.x = Mathf.Clamp(vel.x, -maxLateralSpeed, maxLateralSpeed);
        jogador.linearVelocity = new Vector3(vel.x, vel.y, vel.z);

    
        if ((jogador.position.y < -2f || vidaAtual <= 0) && !jaPerdeu)
        {
            perdeu();
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
        else if (bateu.CompareTag("Parede"))
        {
            transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
            Destroy(bateu.gameObject);
            Vida();
        } else if (bateu.CompareTag("Portal"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        } else if (bateu.CompareTag("Final"))
        {
            Destroy(bateu.gameObject);
            Creditos.SetActive(true);
            Pontos.SetActive(false);
            Creditas();
        }
    }

    public void Vida()
    {
        vidaAtual--;
        vida.text = "Vida: " + vidaAtual;
    }
    public void perdeu()
    {
        if (jaPerdeu) return;

        jaPerdeu = true;

        Time.timeScale = 0f;

        int numeroInt = Random.Range(1, 5);
        switch (numeroInt)
        {
            case 1:
                Perder.text = "Caíste para um abismo infinito :(";
                break;
            case 2:
                Perder.text = "Caíste num amor profundo... sem escapatória! 💔🕳️";
                break;
            case 3:
                Perder.text = "Caíste num profundo buraco!";
                break;
            case 4:
                Perder.text = "Caíste numa armadilha de areia movediça!";
                break;
        }

        Pontos.SetActive(false);
        Perdeu.SetActive(true);
    }

    public void Voltaraojogo()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void MenudoJogo()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
    public void Creditas()
    {
        Time.timeScale = 0f;
        int i = 0;
        do
        {
            creditos.transform.position += new Vector3(0f, VelocityCreds * Time.deltaTime, 0f);
            
            i += 2;
        } while (i != 1);
    }
}
