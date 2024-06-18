using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//---------------------------------------------------
//
//  �ʏ�e�̋����N���X
//
//---------------------------------------------------
public class NormalBullet : MonoBehaviour
{
    //  �e�̃X�s�[�h
    private Vector3 velocity = new Vector3(0,20,0);

    // ���ɓ���
    void Update()
    {
        transform.position += -velocity * Time.deltaTime;
    }

    //  �����蔻��
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //  �ǂɏՓ�
        if(collision.collider.CompareTag("WALL"))
        {
            Destroy(this.gameObject);
        }
    }
}
