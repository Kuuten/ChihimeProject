using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{

    private void Awake()
    {
        this.transform.rotation = Camera.main.transform.rotation;
    }

    private void Update()
    {
        
    }
}
