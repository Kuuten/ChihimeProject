using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �N�`�i���̃r�[���N���X
//
//--------------------------------------------------------------
public class KuchinawaPhase3Bullet : MonoBehaviour
{
    //  VFX�Ȃ̂ňЗ͂̂ݐݒ�
    private int power;
    void Awake()
    {
        power = 1;  // �b��  �����BossKuchinawa�Őݒ肷�邱�ƁI
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    //------------------------------------------------------
    //  �v���p�e�B
    //------------------------------------------------------
    public int GetPower(){ return power; }
}
