using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �v���C���[�̃h�E�W���o�[�X�g�e�N���X
//
//--------------------------------------------------------------
public class KonburstDoujiBullet : MonoBehaviour
{
    private Vector3 velocity;
    private int gamestatus;
    private bool fripY;
    private const float priod = 5.0f;  //  �����i�b�j

    [SerializeField] private Vector3 positionStrength;
    [SerializeField] private Vector3 rotationStrength;

    void Start()
    {
        velocity = Vector3.zero;

        //  GameManager�����Ԃ��擾
        gamestatus = GameManager.Instance.GetGameState();

        //  �e�̌���
        switch(gamestatus)
        {
            case (int)eGameState.Zako:
                velocity = new Vector3(0,-20f,0);   //  �������֌���
                break;
            case (int)eGameState.Boss:
                velocity = new Vector3(0,20f,0);   //  ������֌���
                break;
            case (int)eGameState.Event:
                break;
        }

        //  ���݂̈ʒu����velocity���ړ�
        this.transform.DOMove(velocity,priod).SetRelative(true).SetEase(Ease.Linear);

        //  ���������������
        Destroy(gameObject, priod);

        //  �J������h�炷
        CameraShaker(priod);
    }

    void Update()
    {
        
    }

    //-------------------------------------------
    //  �v���p�e�B
    //-------------------------------------------
    public void SetVelocity(Vector3 v){ velocity = v; }
    public Vector3 GetVelocity(){ return velocity; }
    public void SetFripY(bool frip){ fripY = frip; }
    public bool GetFripY(){ return fripY; }

    //-------------------------------------------
    //  �J������h�炷
    //-------------------------------------------
    private void CameraShaker(float duration)
    {
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(duration, positionStrength);
        Camera.main.transform.DOShakeRotation(duration, rotationStrength);
    }
}
