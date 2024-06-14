using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //  弾のスピード
    [SerializeField] private float bulletSpeed = -1.0f;

    // 下に動く
    void Update()
    {
        transform.position += new Vector3(0,-bulletSpeed,0) * Time.deltaTime;
    }
}
