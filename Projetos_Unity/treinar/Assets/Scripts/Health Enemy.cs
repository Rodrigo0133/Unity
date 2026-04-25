using UnityEngine;

public class HealthEnemy : MonoBehaviour
{


    public int maxVida = 100;
    private int vidaAtual;

    void Start()
    {
        vidaAtual = maxVida;
    }

    public void TomarDano(int dano)
    {
        vidaAtual -= dano;
        Debug.Log(gameObject.name + " tomou " + dano + " de dano! Vida: " + vidaAtual);

        if (vidaAtual <= 0)
        {
            Destroy(gameObject); 
        }
    }
}
