using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

//--------------------------------------------------------------
//
//  �h���b�v�A�C�e�����v���C���[�ɑS���
//
//--------------------------------------------------------------
public class ItemMove : MonoBehaviour
{
    [SerializeField] private Items itemType;
    private float flowSpeed = 0.005f;

    private SHOT_TYPE shotType;
    private MoneyManager moneyManager = null;
    [SerializeField] private GameObject player;

    void Start()
    {
        moneyManager = GameObject.Find("MoneyManager").GetComponent<MoneyManager>();
        Assert.IsTrue(moneyManager,"MoneyManager�I�u�W�F�N�g������܂���I");
    }

    void Update()
    {
        //  �z���t�B�[���h�ɐG��Ȃ�����͏�ɗ����
        Flowing();
    }

    //  ���o�[�g�e���Ƀv���C���[�ɉ������Ȃ���ړ�
    public void MoveToPlayer()
    {
        //  �A�C�e������v���C���[�ւ̃x�N�g��
        Vector3 vec = player.transform.position - this.transform.position;
        vec.Normalize();

        Vector3 pos = this.transform.position;
        float speed = 1.0f;
        pos += vec * speed * Time.deltaTime;

        this.transform.position = pos;
    }

    //  ��Ɍ��E�̒��S�i�{�X���j�ɃA�C�e���������
    private void Flowing()
    {
        this.transform.position += new Vector3(0, flowSpeed, 0);
    }
}
