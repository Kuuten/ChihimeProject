using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.RuleTile.TilingRuleOutput;

//--------------------------------------------------------------
//
// 3D���j���[�̉�]�N���X
//
//--------------------------------------------------------------
public class Rotate3DMenu : MonoBehaviour
{
    bool bComplete;            //  ��]�����t���O
    Quaternion preRotation;    //  �O��̎p��
    float time;
    const float default_time = 0.5f;
    bool bClockwise;

    //  �V���O���g���ȃC���X�^���X
    public static Rotate3DMenu Instance
    {
        get; private set;
    }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        bComplete = true;
        bClockwise = true;
        preRotation = Quaternion.identity;
    }

    void Update()
    {

        //  ������-------------------------------
        var current = Keyboard.current;
        var enter = current.enterKey;

        if(enter.wasPressedThisFrame)
        {
            
        }
        //---------------------------------------

        //  ��]���������̎�
        if(!bComplete)
        {
            if(Timer()) //  �^�C�}�[�����������Ȃ�
            {
                bComplete = true;   //  ����

                //  preRotation����X����60�x��]��������]�ɕ␳
                if(bClockwise)
                {
                    Quaternion rot = Quaternion.AngleAxis(-60, Vector3.right);
                    Quaternion q = preRotation;
                    this.transform.rotation = q * rot;
                }
                else
                {
                    Quaternion rot = Quaternion.AngleAxis(60, Vector3.right);
                    Quaternion q = preRotation;
                    this.transform.rotation = q * rot;
                }

                //  ����̉�]��ۑ�
                preRotation = transform.rotation;
            }
            else
            {
                CompleteRotation(); //  ��]������
            }
        }


        
    }

    //-----------------------------------------------------------------
    //  �v���p�e�B
    //-----------------------------------------------------------------
    public bool GetComplete(){ return bComplete; }
    public void SetComplete(bool flag){ bComplete = flag; }
    public bool GeClockwise(){ return bClockwise; }
    public void SetClockwise(bool flag){ bClockwise = flag; }


    //  �w�肵�����o�[�g�����ʂɗ���悤�ɉ�]���Ă��̊�����m�点��
    public void CompleteRotation()
    {
        bComplete = false;
        
        float degree = 60;
        float duration = default_time;

        Quaternion result = Quaternion.identity;

        if(bClockwise)
        {
            Quaternion rot = Quaternion.AngleAxis(-degree, Vector3.right);
            Quaternion q = this.transform.rotation;
            result = q * rot;
        }
        else
        {
            Quaternion rot = Quaternion.AngleAxis(degree, Vector3.right);
            Quaternion q = this.transform.rotation;
            result = q * rot;
        }

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            result,
            (degree/duration)*Time.deltaTime);
    }

    //  �^�C�}�[
    private bool Timer()
    {
        if(time <= 0.0f)
        {
            time = 0.0f;
            return true;
        }
        else time -= Time.deltaTime;

        return false;
    }

    //  ���j���[����]������g���K�[
    public void TurnMenu(bool crockwise)
    {
        time = default_time;
        bComplete = false;
        bClockwise = crockwise;
    }
}
