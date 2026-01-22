using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateCutscene : MonoBehaviour
{
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
           
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
