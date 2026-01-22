using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallClouds : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float parallaxSpeed;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.left * parallaxSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position,endPoint.position)<0.1f)
        {
            transform.position = startPoint.position;
        }
    }
}
