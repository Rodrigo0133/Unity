using UnityEngine;

public class Espinho : MonoBehaviour
{
    [Header("Configurações do Espinho")]
    [Tooltip("Quantidade de dano que o espinho causa ao jogador.")]
    public int dano = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se quem bateu foi o jogador
        if (collision.CompareTag("Player"))
        {
            // Pega o script do jogador localmente
            PlayerMovement scriptJogador = collision.GetComponent<PlayerMovement>();
            
            // Segurança: verifica se o script realmente existe antes de dar dano
            if (scriptJogador != null)
            {
                scriptJogador.TakeDamage(dano);
            }
        }
    }
}
