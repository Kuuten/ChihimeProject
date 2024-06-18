using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class TestTarget : MonoBehaviour
{
    float sec;

    void Update()
    {
        sec += Time.deltaTime;

        Vector3 pos = transform.position;
        pos.x = Mathf.Sin(sec) * 3f;

        transform.position = pos;
    }
}
