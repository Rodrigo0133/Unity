using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private float parallaxSpeed;

    private float spriteLength;
    private Vector3 startPos;
    void Start()
    {
        startPos = transform.position;
        spriteLength = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.left * parallaxSpeed * Time.deltaTime);

        if (transform.position.x < startPos.x - spriteLength)
        {
            transform.position = startPos;
        }
    }
}
