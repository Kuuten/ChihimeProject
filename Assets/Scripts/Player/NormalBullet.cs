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
    //  �e�̎���
    [SerializeField] private float lifetime = 3;

    // ���ɓ���
    void Update()
    {
        transform.position += -velocity * Time.deltaTime;

        lifetime -= Time.deltaTime;
        if(lifetime <= 0f)Destroy(this.gameObject);
    }

    ////  �����蔻��
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    //  �ǂɏՓ�
    //    if(collision.CompareTag("ENEMY"))
    //    {
    //        Destroy(this.gameObject);
    //    }
    //}
}
