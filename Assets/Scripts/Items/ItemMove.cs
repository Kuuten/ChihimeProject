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
    private GameObject player;
    private bool bField;
    private float speed;
    private float acceleration;

    void Start()
    {
        moneyManager = GameObject.Find("MoneyManager").GetComponent<MoneyManager>();
        Assert.IsTrue(moneyManager,"MoneyManager�I�u�W�F�N�g������܂���I");

        bField = false;
        speed = 1.0f;
        acceleration = 1.0f;
    }

    void Update()
    {


        //  �z���t�B�[���h�ɐG��Ȃ�����͏�ɗ����
        if(bField)MoveToPlayer();
        else Flowing();
    }

    //  ���o�[�g�e���Ƀv���C���[�ɉ������Ȃ���ړ�
    public void MoveToPlayer()
    {
        //  �A�C�e������v���C���[�ւ̃x�N�g��
        player = GameManager.Instance.GetPlayer();
        Vector3 vec = player.transform.position - this.transform.position;
        float distance = vec.magnitude;
        vec.Normalize();

        Vector3 pos = this.transform.position;
        const float d = 0.01f;   //  �߂Â�������臒l

        if(distance > d )
        {
            speed += acceleration;
            pos += vec * speed * Time.deltaTime;
        }

        this.transform.position = pos;
    }

    //  ��Ɍ��E�̒��S�i�{�X���j�ɃA�C�e���������
    private void Flowing()
    {
        this.transform.position += new Vector3(0, flowSpeed, 0);
    }

    //-------------------------------------------------------
    //  �z���t�B�[���h�Ɠ���������v���C���[�ɉ�������
    //-------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //  �^�O���z���t�B�[���h�ȊO�Ȃ�return
        if(!collision.CompareTag("PlayerField"))return;

        //  ����t�B�[���h�z��ON
        bField = true;
    }
}
