using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] private float dmg;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.GetComponent<Enemy>().Damage(dmg);
    }
}
