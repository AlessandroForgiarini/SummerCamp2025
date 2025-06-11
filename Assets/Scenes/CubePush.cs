using System;
using UnityEngine;

public class CubePush : MonoBehaviour
{
    private Renderer cubeRenderer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cubeRenderer = GetComponent<Renderer>();
    }

    private void OnCollisionEnter(Collision other)
    {
        cubeRenderer.material.color = Color.blue;
    }
}
