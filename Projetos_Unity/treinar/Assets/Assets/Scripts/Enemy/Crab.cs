using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crab : Enemy
{
    [SerializeField] private Collider2D coll;
    protected override void Start()
    {
        base.Start();
    }
    protected override void HurtSequence()
    {
        anim.SetTrigger("Hurt");
    }
    protected override void DeathSequence()
    {
        anim.SetTrigger("Death");
        coll.enabled = false;
        rb.isKinematic = true;
    }
    
}
