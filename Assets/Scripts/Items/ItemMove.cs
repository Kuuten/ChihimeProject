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
    private float flowSpeed = 0.005f;

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
        player = GameManager.Instance.GetPlayer();

        //  �v���C���[�̗̑͂��Ȃ���΃��^�[��
        if(player == null)return;

        //  �A�C�e������v���C���[�ւ̃x�N�g��
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

    //----------------------------------------------------------------
    //  �v���p�e�B
    //----------------------------------------------------------------
    public void SetFieldFlag(bool flag){ bField = flag; }
    public bool GetFieldFlag(){ return bField; }

    //-------------------------------------------------------
    //  �z���t�B�[���h�Ɠ���������v���C���[�ɉ�������
    //-------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("DeadWall")) //�@���ŕǂɓ���������
        {
            Debug.Log("���ŕǂɓ��������̂ŃA�C�e�������ł����܂�");

            //  �Q�[���I�u�W�F�N�g���폜
            Destroy(this.gameObject);
        }

        //  �^�O���z���t�B�[���h�Ȃ�z��ON
        if(collision.CompareTag("PlayerField"))
        {
            Debug.Log("����t�B�[���h�z��ON");

            //  ����t�B�[���h�z��ON
            bField = true;
        }
    }
}
