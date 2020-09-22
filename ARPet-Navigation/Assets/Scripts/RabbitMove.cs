using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitMove : MonoBehaviour
{
    public float speed = 0.2f;  // Units per second 

    private Vector3 startPos;
    private Vector3 forwardPos;
    private Vector3 toPos;

    void Start()
    {
        startPos = transform.position;
        toPos = startPos;
        forwardPos = transform.position + transform.forward * 1000000.0f;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, forwardPos, speed * Time.deltaTime);
    }
}
