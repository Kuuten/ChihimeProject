using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �v���C���[�̃{���̓����蔻��N���X
//
//--------------------------------------------------------------
public class BombCollision : MonoBehaviour
{
    

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    //-------------------------------------------------------
    //  �G�̒e�Ƃ̓����蔻��
    //-------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {   
        if(collision.CompareTag("EnemyBullet"))
        {
            //  �G�e�Ɠ���������G�e������
            Destroy(collision.gameObject);
        }
    }
}
