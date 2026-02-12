using UnityEngine;

public class seguir : MonoBehaviour
{
    //  serve para copiar a posição do objeto que por
    public Transform esfera;
    //Vector3 = (X,Y,Z)
    public Vector3 distancia_para_esfera;
    void Update()
    {
        //  serve para que a posição da camera seja a mesma do item/Objeto 
        transform.position = esfera.position+distancia_para_esfera;
    }
}
