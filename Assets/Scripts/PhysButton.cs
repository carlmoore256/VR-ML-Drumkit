using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysButton : MonoBehaviour
{
    Renderer rend;
    Color origColor;
    Color selectedColor;
    void Start()
    {
        rend = GetComponent<Renderer>();
        origColor = rend.material.color;
    }

    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "controller")
        {
            rend.material.color = Color.red;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "controller")
        {
            rend.material.color = origColor;
        }
    }

    // IEnumerator ColorLerp(Renderer r)
    // {
        
    // }
}
