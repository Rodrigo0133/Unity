using UnityEngine;
public class girar : MonoBehaviour
{
   public GameObject objeto;
    public float velocidade;
    void Update()
    {
        objeto.transform.Rotate(0, velocidade * Time.deltaTime, 0);
    }
}
