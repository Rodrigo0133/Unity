using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class NPCTalk : MonoBehaviour
{
    [Header("Referências")]
    public GameObject avisoE;         // “[E] para falar”
    public TMP_Text textoNPC;         // Texto do NPC

    [Header("Diálogo")]
    public List<string> falas;        // Lista de falas
    public float tempoFala = 3f;      // Tempo que cada fala aparece

    [Header("Configurações do aviso")]
    public float tempoSemInteracao = 5f; // Tempo para sumir o aviso E sem interação

    private int indiceFala = 0;
    private bool jogadorPerto = false;
    private bool falando = false;
    private float contadorSemInteracao = 0f;

    void Start()
    {
        if (avisoE == null) Debug.LogWarning("avisoE não atribuído no Inspector.", this);
        if (textoNPC == null) Debug.LogWarning("textoNPC não atribuído no Inspector.", this);
        if (falas == null || falas.Count == 0) Debug.LogWarning("Lista 'falas' vazia ou não atribuída.", this);

        if (avisoE != null) avisoE.SetActive(false);
        if (textoNPC != null) textoNPC.gameObject.SetActive(false);
    }

    void Update()
    {
        if (jogadorPerto && !falando)
        {
            if (avisoE != null) avisoE.SetActive(true);

            // Contador de tempo sem apertar E
            contadorSemInteracao += Time.deltaTime;

            // Se apertar E, inicia diálogo
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Tecla E pressionada: tentando iniciar diálogo.", this);

                if (falas == null || falas.Count == 0)
                {
                    Debug.LogWarning("Não há falas para mostrar.", this);
                    return;
                }

                if (textoNPC == null)
                {
                    Debug.LogWarning("textoNPC não atribuído; não é possível mostrar a fala.", this);
                    return;
                }

                if (avisoE != null) avisoE.SetActive(false);
                contadorSemInteracao = 0f; // resetar contador
                StartCoroutine(MostrarFala());
            }

            // Esconde aviso se passar o tempo sem interação
            if (contadorSemInteracao >= tempoSemInteracao)
            {
                if (avisoE != null) avisoE.SetActive(false);
            }
        }
        else if (!jogadorPerto)
        {
            if (avisoE != null) avisoE.SetActive(false);
            if (textoNPC != null) textoNPC.gameObject.SetActive(false);
            falando = false;
            indiceFala = 0;
            contadorSemInteracao = 0f;
        }
    }

    IEnumerator MostrarFala()
    {
        falando = true;

        textoNPC.text = falas[indiceFala];
        textoNPC.gameObject.SetActive(true);

        yield return new WaitForSeconds(tempoFala);

        textoNPC.gameObject.SetActive(false);

        indiceFala++;

        if (indiceFala < falas.Count)
        {
            falando = false;
            if (avisoE != null) avisoE.SetActive(true);
            contadorSemInteracao = 0f; // reinicia contador para aviso reaparecer
        }
        else
        {
            falando = false;
            indiceFala = 0; // reinicia diálogo
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        jogadorPerto = true;
        contadorSemInteracao = 0f; // reset contador ao entrar
        Debug.Log("Jogador entrou no trigger do NPC.", this);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        jogadorPerto = false;
        Debug.Log("Jogador saiu do trigger do NPC.", this);
    }
}
