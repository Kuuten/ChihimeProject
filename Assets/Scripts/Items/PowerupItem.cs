using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//--------------------------------------------------------------
//
//  �p���[�A�b�v�A�C�e���N���X
//
//--------------------------------------------------------------
public class PowerupItem : MonoBehaviour
{
    //  �v���C���[�̃N���X���擾
    private GameObject player;

    void Start()
    {
        player = GameManager.Instance.GetPlayer();
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

        //  �p���[�A�b�v
        Debug.Log("�e���p���[�A�b�v���܂����I");
        
        //  �A�C�e��������
        Destroy(this.gameObject);


    }
}
