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
            InimigoIA_QuadradosVermelho enemy = other.GetComponent<InimigoIA_QuadradosVermelho>();
            if (enemy != null)
            {
                enemy.TakeDamage();
                alreadyHit.Add(other.gameObject);
            }
        }
    }
}
