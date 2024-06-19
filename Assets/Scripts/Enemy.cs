using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �G�̊�{�N���X
//
//--------------------------------------------------------------
public class Enemy : MonoBehaviour
{

    //  �G�����E�ɗh�炷
    //  �X�R�A�̕\��
    //  �G��|�������ɃX�R�A��\��������
    //  ���X�^�[�g�̎���

    [SerializeField] float moveSpeed = 1.0f;
    [SerializeField] GameObject explosion;

    void Start()
    {
        
    }

    void Update()
    {
        transform.position += new Vector3(0,moveSpeed * Time.deltaTime,0);
    }

    //  �G�ɓ��������甚������
    //  �����蔻��̊�b�m���F
    //  �����蔻����s���ɂ́A
    //  �E���҂�Collider�����Ă���
    //  �E�ǂ��炩��RigidBody�����Ă���
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Instantiate(explosion, transform.position, transform.rotation);
        Destroy(this.gameObject);
        Destroy(collision.gameObject);
    }
}
