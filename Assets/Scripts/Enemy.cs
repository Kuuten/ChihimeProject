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
    //  �G�̈ړ��F�^���Ɉړ�����
    //  �G�𐶐��F�����H������
    //  �G�ɒe�����������甚������
    //  �G��Player���Ԃ������甚������

    [SerializeField] float moveSpeed = 1.0f;

    void Start()
    {
        
    }

    void Update()
    {
        transform.position += new Vector3(0,moveSpeed * Time.deltaTime,0);
    }
}
