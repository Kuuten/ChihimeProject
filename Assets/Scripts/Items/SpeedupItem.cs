using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//--------------------------------------------------------------
//
//  �X�s�[�h�A�b�v�A�C�e���N���X
//
//--------------------------------------------------------------
public class SpeedupItem : MonoBehaviour
{
    //  �v���C���[�̃N���X���擾
    //[SerializeField] private 

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    //-------------------------------------------------------
    //  �v���C���[�Ɠ���������p���[�A�b�v����
    //-------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //  �^�O���v���C���[�ȊO�Ȃ�return
        if(!collision.CompareTag("Player"))return;

        //  �X�s�[�h�A�b�v
        Debug.Log("���@���X�s�[�h�A�b�v���܂����I");
        
        //  �A�C�e��������
        Destroy(this.gameObject);


    }
}
