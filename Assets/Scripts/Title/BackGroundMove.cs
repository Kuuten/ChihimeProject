using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//
//  �w�i�Q����؂�ւ��čs�������ōs��
//
public class BackGroundMove : MonoBehaviour
{
    //  �X�N���[������X�s�[�h
    [SerializeField] private float scrollSpeed = 1.0f;
    //  �܂�Ԃ��n�_�p�I�u�W�F�N�g
    [SerializeField] private GameObject returnPoint;
    //  �ăX�^�[�g�n�_�p�I�u�W�F�N�g
    [SerializeField] private GameObject restartPoint;

    void Start()
    {
        
    }

    void Update()
    {
        this.transform.position += new Vector3(0,-scrollSpeed,0)*Time.deltaTime;

        if (this.transform.position.y <= returnPoint.transform.position.y)
        {
            this.transform.position = restartPoint.transform.position;
        }
    }
}
