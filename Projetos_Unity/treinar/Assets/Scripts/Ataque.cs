using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ataque : MonoBehaviour
{
    private List<GameObject> alreadyHit = new List<GameObject>();

    private void OnEnable()
    {
        alreadyHit.Clear(); 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !alreadyHit.Contains(other.gameObject))
        {
            bool acertou = false;
            bool isBoss = false;

            // Tenta dar dano ao inimigo quadrado vermelho
            InimigoIA_QuadradosVermelho enemy = other.GetComponent<InimigoIA_QuadradosVermelho>();
            if (enemy != null)
            {
                enemy.TakeDamage();
                alreadyHit.Add(other.gameObject);
                Debug.Log($"[Ataque] Acertou no inimigo: {other.gameObject.name}");
                acertou = true;
            }

            // Tenta dar dano à mini barata
            MiniBarata miniBarata = other.GetComponent<MiniBarata>();
            if (miniBarata != null)
            {
                miniBarata.TakeDamage();
                alreadyHit.Add(other.gameObject);
                Debug.Log($"[Ataque] Acertou na Mini Barata: {other.gameObject.name} | Vida restante: {miniBarata.vida}");
                acertou = true;
            }

            // Tenta dar dano à Barata Inimigo (nova do mapa)
            BarataInimigo barataInimigo = other.GetComponent<BarataInimigo>();
            if (barataInimigo != null)
            {
                barataInimigo.TakeDamage();
                alreadyHit.Add(other.gameObject);
                Debug.Log($"[Ataque] Acertou na Barata Inimigo: {other.gameObject.name} | Vida restante: {barataInimigo.vida}");
                acertou = true;
            }

            // Get dynamic player damage
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

            // Tenta dar dano ao Boss Barata
            BossBarata boss = other.GetComponent<BossBarata>();
            if (boss != null)
            {
                boss.TomarDano((int)playerDamage);
                alreadyHit.Add(other.gameObject);
                Debug.Log($"[Ataque] Acertou no Boss Barata! Vida restante: {boss.vidaAtual}/{boss.maxVida}");
                acertou = true;
                isBoss = true;
            }

            // Tenta dar dano ao Novo Boss (Anão Mitológico)
            BossAnaoMitologico bossAnao = other.GetComponent<BossAnaoMitologico>();
            if (bossAnao != null)
            {
                bossAnao.TomarDano((int)playerDamage);
                alreadyHit.Add(other.gameObject);
                Debug.Log($"[Ataque] Acertou no Boss Anão!");
                acertou = true;
                isBoss = true;
            }

            // Tenta dar dano ao Irmão do King Cube (3º Boss)
            BossIrmaoKingCube bossIrmao = other.GetComponent<BossIrmaoKingCube>();
            if (bossIrmao != null)
            {
                bossIrmao.TomarDano(playerDamage);
                alreadyHit.Add(other.gameObject);
                Debug.Log($"[Ataque] Acertou no Boss Irmão King Cube!");
                acertou = true;
                isBoss = true;
            }

            // Tenta dar dano ao Último Boss (Final Boss)
            UltimoBoss ultimoBoss = other.GetComponent<UltimoBoss>();
            if (ultimoBoss != null)
            {
                ultimoBoss.TomarDano(playerDamage);
                alreadyHit.Add(other.gameObject);
                Debug.Log($"[Ataque] Acertou no Último Boss!");
                acertou = true;
                isBoss = true;
            }

            // Se acertou num inimigo com sucesso, afeta o inimigo (Para e empurra para trás)
            if (acertou)
            {
                // 1. Camera Shake (mantém o impacto visual no ecrã)
                if (CameraFollow.Instance != null)
                {
                    CameraFollow.Instance.Shake(0.1f, 0.3f);
                }

                // 2. Para e empurra o inimigo específico (APENAS se não for boss)
                if (!isBoss)
                {
                    StartCoroutine(EfeitoImpactoInimigo(other.gameObject));
                }
            }
        }
    }

    /// <summary>
    /// Congela o inimigo temporariamente e empurra-o para trás.
    /// Não afeta o jogador nem o resto do jogo.
    /// </summary>
    private IEnumerator EfeitoImpactoInimigo(GameObject inimigo)
    {
        if (inimigo == null) yield break;

        // Descobre a direção do recuo baseando-se em onde o jogador está a olhar.
        // Assumimos que o script Ataque é filho direto ou indireto do PlayerMovement.
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

        // 1. Pausa o Animator (simula o "congelamento" do golpe no inimigo)
        Animator anim = inimigo.GetComponentInChildren<Animator>();
        if (anim != null) anim.speed = 0f;

        // 2. Tenta dar um pequeno empurrão para trás (Knockback)
        Rigidbody2D rb = inimigo.GetComponent<Rigidbody2D>();
        
        // Se tiver Rigidbody2D, empurramos pela física
        if (rb != null && rb.bodyType != RigidbodyType2D.Static)
        {
            // Substitui a velocidade atual pela força do empurrão
            rb.linearVelocity = new Vector2(direcaoRecuo * 5f, rb.linearVelocity.y);
            
            // Espera o tempo do "Hit Stun" (inimigo parado/empurrado)
            yield return new WaitForSeconds(0.15f);
            
            if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // Trava o recuo
        }
        else
        {
            // Se não tiver Rigidbody (ex: inimigos antigos ou bosses parados),
            // empurra modificando a posição suavemente no Transform
            float tempo = 0f;
            while(tempo < 0.15f)
            {
                if (inimigo == null) break;
                inimigo.transform.position += new Vector3(direcaoRecuo * 2f * Time.deltaTime, 0, 0);
                tempo += Time.deltaTime;
                yield return null;
            }
        }

        // 3. Restaura a animação do inimigo para voltar a andar
        if (inimigo != null && anim != null)
        {
            anim.speed = 1f;
        }
    }
}
