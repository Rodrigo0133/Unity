using Unity.VisualScripting;
using UnityEngine;

public class Atack : MonoBehaviour
{
    public GameObject Espada;
    public Animation anim;
    
    void Start()
    {
        Espada.SetActive(false);
       
    }
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            atack();
        }
    }
    void atack()
    {
        Espada.SetActive (true);
        anim.Play();
        OnCollisionEnter();
        Espada.SetActive(false);

    }
}
