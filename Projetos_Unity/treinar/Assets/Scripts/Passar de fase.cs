using UnityEngine;
using UnityEngine.SceneManagement;
public class Passardefase : MonoBehaviour
{

    public void AvancarParaProximaFase()
    {
       
        int indexCenaAtual = SceneManager.GetActiveScene().buildIndex;
        
        
        SceneManager.LoadScene(indexCenaAtual + 1);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se quem colidiu foi o jogador
        if (collision.CompareTag("Player"))
        {
            AvancarParaProximaFase();
        }
    }
}
