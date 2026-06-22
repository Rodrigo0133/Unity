using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ataque : MonoBehaviour
{
    private List<GameObject> alreadyHit = new List<GameObject>();
    private Collider2D ataqueCollider;
    private bool ataqueAtivo = false;

    private void Awake()
    {
        ataqueCollider = GetComponent<Collider2D>();
        DesativarAtaque();
    }

    private void OnEnable()
    {
        alreadyHit.Clear();
        ataqueAtivo = false;

        if (ataqueCollider == null)
            ataqueCollider = GetComponent<Collider2D>();

        if (ataqueCollider != null)
            ataqueCollider.enabled = false;
    }

    public void AtivarAtaque()
    {
        ataqueAtivo = true;
        alreadyHit.Clear();

        if (ataqueCollider == null)
            ataqueCollider = GetComponent<Collider2D>();

        if (ataqueCollider != null)
            ataqueCollider.enabled = true;
    }

    public void DesativarAtaque()
    {
        ataqueAtivo = false;
        alreadyHit.Clear();

        if (ataqueCollider == null)
            ataqueCollider = GetComponent<Collider2D>();

        if (ataqueCollider != null)
            ataqueCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!ataqueAtivo) return;
        TentarAcertar(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!ataqueAtivo) return;
        TentarAcertar(other);
    }

    private void TentarAcertar(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !alreadyHit.Contains(other.gameObject))
        {
            bool acertou = false;
            bool isBoss = false;
            float playerDamage = 25f;
            PlayerMovement pm = GetComponentInParent<PlayerMovement>();
            if (pm == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) pm = player.GetComponent<PlayerMovement>();
            }
            if (pm != null)
            {
                playerDamage = pm.GetCurrentDamage();
            }
            int playerDamageInt = Mathf.RoundToInt(playerDamage);
            MiniBarata miniBarata = other.GetComponent<MiniBarata>();
            if (miniBarata != null)
            {
                miniBarata.TomarDano(playerDamageInt);
                alreadyHit.Add(other.gameObject);
                Debug.Log($"[Ataque] Acertou na Mini Barata: {other.gameObject.name} | Vida restante: {miniBarata.vida}");
                acertou = true;
            }
            BarataInimigo barataInimigo = other.GetComponent<BarataInimigo>();
            if (barataInimigo != null)
            {
                barataInimigo.TomarDano(playerDamageInt);
                alreadyHit.Add(other.gameObject);
                Debug.Log($"[Ataque] Acertou na Barata Inimigo: {other.gameObject.name} | Vida restante: {barataInimigo.vida}");
                acertou = true;
            }

            BossBarata boss = other.GetComponent<BossBarata>();
            if (boss != null)
            {
                boss.TomarDano(playerDamageInt);
                alreadyHit.Add(other.gameObject);
                Debug.Log($"[Ataque] Acertou no Boss Barata! Vida restante: {boss.vidaAtual}/{boss.maxVida}");
                acertou = true;
                isBoss = true;
            }


            BossAnaoMitologico bossAnao = other.GetComponent<BossAnaoMitologico>();
            if (bossAnao != null)
            {
                bossAnao.TomarDano(playerDamageInt);
                alreadyHit.Add(other.gameObject);
                Debug.Log($"[Ataque] Acertou no Boss Anão!");
                acertou = true;
                isBoss = true;
            }

            BossIrmaoKingCube bossIrmao = other.GetComponent<BossIrmaoKingCube>();
            if (bossIrmao != null)
            {
                bossIrmao.TomarDano(playerDamage);
                alreadyHit.Add(other.gameObject);
                Debug.Log($"[Ataque] Acertou no Boss Irmão King Cube!");
                acertou = true;
                isBoss = true;
            }
            UltimoBoss ultimoBoss = other.GetComponent<UltimoBoss>();
            if (ultimoBoss != null)
            {
                ultimoBoss.TomarDano(playerDamage);
                alreadyHit.Add(other.gameObject);
                Debug.Log($"[Ataque] Acertou no Último Boss!");
                acertou = true;
                isBoss = true;
            }

            if (acertou)
            {
                if (CameraFollow.Instance != null)
                {
                    CameraFollow.Instance.Shake(0.1f, 0.3f);
                }

                if (!isBoss)
                {
                    StartCoroutine(EfeitoImpactoInimigo(other.gameObject));
                }
            }
        }
    }

  
    private IEnumerator EfeitoImpactoInimigo(GameObject inimigo)
    {
        if (inimigo == null) yield break;

        
        float direcaoRecuo = 1f;
        PlayerMovement pm = GetComponentInParent<PlayerMovement>();
        if (pm == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) direcaoRecuo = player.transform.localScale.x > 0 ? 1f : -1f;
        }
        else
        {
            direcaoRecuo = pm.transform.localScale.x > 0 ? 1f : -1f;
        }
        Animator anim = inimigo.GetComponentInChildren<Animator>();
        if (anim != null) anim.speed = 0f;
        Rigidbody2D rb = inimigo.GetComponent<Rigidbody2D>();
        if (rb != null && rb.bodyType != RigidbodyType2D.Static)
        {
            rb.linearVelocity = new Vector2(direcaoRecuo * 5f, rb.linearVelocity.y);
            yield return new WaitForSeconds(0.15f);
            
            if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); 
        }
        else
        {
            float tempo = 0f;
            while(tempo < 0.15f)
            {
                if (inimigo == null) break;
                inimigo.transform.position += new Vector3(direcaoRecuo * 2f * Time.deltaTime, 0, 0);
                tempo += Time.deltaTime;
                yield return null;
            }
        }
        if (inimigo != null && anim != null)
        {
            anim.speed = 1f;
        }
    }
}
