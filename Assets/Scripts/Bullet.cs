using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //  �e�̃X�s�[�h
    [SerializeField] private float bulletSpeed = -1.0f;

    // ���ɓ���
    void Update()
    {
        transform.position += new Vector3(0,-bulletSpeed,0) * Time.deltaTime;
    }
}
