using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �G�̒e�N���X
//
//--------------------------------------------------------------
public class EnemyBullet : MonoBehaviour
{
    //  �O������e�̃x�N�g���ƈړ��X�s�[�h���擾���A�ړ����邾��

    private Vector3 velocity;
    private float speed;

    private int power;
    
    void Awake()
    {
        speed = 0f;
        power = 0;
        velocity = Vector3.zero;
    }

    void Update()
    {
        transform.position += speed * velocity * Time.deltaTime;
    }

    //-------------------------------------------------
    //  �����蔻��
    //-------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeadWall") ||
            collision.CompareTag("DeadWallBottom"))
        {
            Destroy(this.gameObject);
        }
    }


    //------------------------------------------------------
    //  �v���p�e�B
    //------------------------------------------------------
    public void SetVelocity(Vector3 v){ velocity = v; }
    public void SetSpeed(float s){ speed = s; }
    public void SetPower(int p){ power = p; }
    public int GetPower(){ return power; }
}
