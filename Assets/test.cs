using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    Tween tw;

    // Start is called before the first frame update
    void Start()
    {
        tw = transform.DOMoveX(5,5);
        Invoke("testMethod", 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void testMethod()
    {
      tw.Complete();  
    }
}
