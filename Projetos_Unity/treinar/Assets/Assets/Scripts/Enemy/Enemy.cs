using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    protected Animator anim;
    protected Rigidbody2D rb;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

   public void Damage(float dmg)
    {
        health -= dmg;
        HurtSequence();
        if (health <= 0)
        {
            DeathSequence();
        }
    }
    protected virtual void DeathSequence()
    {

    }
    protected virtual void HurtSequence()
    {

    }
}
