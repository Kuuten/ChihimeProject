using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

//--------------------------------------------------------------
//
//  �v���C���[�̃V���b�g�Ǘ��N���X
//
//--------------------------------------------------------------
//  �V���b�g�̎��
public enum SHOT_TYPE
{
    NORMAL,
    DOUJI,              // �h�E�W
    DOUJI_BURST,        // �h�E�W���o�[�X�g
    TSUKUMO,            // �c�N��
    TSUKUMO_BURST,      // �c�N�����o�[�X�g
    KUCHINAWA,          // �N�`�i��
    KUCHINAWA_BURST,    // �N�`�i�����o�[�X�g
    KURAMA,             // �N���}
    KURAMA_BURST,       // �N���}���o�[�X�g
    WADATSUMI,          // ���_�c�~
    WADATSUMI_BURST,    // ���_�c�~���o�[�X�g
    HAKUMEN,            // �n�N����
    HAKUMEN_BURST,      // �n�N�������o�[�X�g

    TYPE_MAX
}

//  �m�[�}���e�̃��x�����X�g
enum eNormalShotLevel
{
    Lv1 = 1,
    Lv2,
    Lv3,

    LvMax
}

public class PlayerShotManager : MonoBehaviour
{
    //  GameManager
    [SerializeField] private GameManager gameManager;

    //  �e�̔��˓_
    [SerializeField]private Transform firePoint1;
    [SerializeField]private Transform firePoint2_L;
    [SerializeField]private Transform firePoint2_R;
    [SerializeField]private Transform firePoint3_L;
    [SerializeField]private Transform firePoint3_R;
    //  �e�̃v���n�u
    [SerializeField]private GameObject[] bulletPrefab;

    //  �e�̈ړ��x�N�g��
    private Vector3 velocity;
    //  �m�[�}���e�̃V���b�g�\�t���O
    private bool canShot;
    //  �m�[�}���e�̃V���b�gCD�̃J�E���g�p
    private float shotCount = 0;
    //  �m�[�}���e�̒e�����b���Ɍ��Ă邩
    private float shotInterval = 0.05f;
    //  �m�[�}���e�̃��x��
    private int normalShotLevel;
    //  �m�[�}���e�̈ړ���
    private const float normalSpeed = -20f; 
    //  �m�[�}���e�̍U����
    private float normalShotPower;

    //  �e�X�g�p
    public int gamestatus;
    InputAction test;
    bool b;

    //  ����
    InputAction shot;


    void Start()
    {
        // InputAction��Move��ݒ�
        PlayerInput playerInput = GetComponent<PlayerInput>();
        shot = playerInput.actions["Shot"];
        test = playerInput.actions["TestButton2"]; 

        normalShotPower = 1.0f;
        shotCount = 0;
        canShot = true;
        normalShotLevel = 1; // �ŏ��̓��x���P

        //  �e�X�g�p
        gamestatus = (int)eGameState.Zako;
        b = true;

        //  �e�̌����͂Ƃ肠�����ʏ�e�ɍ��킹��
        velocity = new Vector3(0,normalSpeed,0);   //  �ŏ��͉������֌���
    }

    void Update()
    {
        //  GameManager�����Ԃ��擾
        //gamestatus = gameManager.GetGameState();

        //  Enter�ŃU�R�{�X�؂�ւ�
        if(test.WasPressedThisFrame())
        {
            b = !b;
            Debug.Log("�؂�ւ��t���O:"+ b);

            if(b)gamestatus = (int)eGameState.Zako;
            else gamestatus = (int)eGameState.Boss;

            GameManager.Instance.SetGameState(gamestatus);
            
            Debug.Log("gamestatus:"+ gamestatus);
        }

        //  �Q�[���i�K�ʏ���
        switch(gamestatus)
        {
            case (int)eGameState.Zako:
                NormalShot(true);                    //  �ʏ�e
                break;
            case (int)eGameState.Boss:
                NormalShot(false);                   //  �ʏ�e
                break;
            case (int)eGameState.Event:
                break;
        }

    }

    //---------------------------------------------------------
    //  �v���p�e�B
    //---------------------------------------------------------
    public int GetNormalShotLevel(){ return normalShotLevel; }
    public void SetNormalShotLevel(int level)
    {
        Debug.Assert(normalShotLevel >= (int)eNormalShotLevel.Lv1 &&
            normalShotLevel <= (int)eNormalShotLevel.Lv3,
            "�ʏ�e���x���̐ݒ�l���͈͊O�ɂȂ��Ă��܂��I");
        if(normalShotLevel != level)normalShotLevel = level;
    }
    public float GetNormalShotPower(){ return normalShotPower; }
    public void SetNormalShotPower(float power) { normalShotPower = power; }

    //  �e�̈ړ��x�N�g���𔽓]����
    public Vector3 GetReverseVelocity(int state)
    {
        //  �U�R�풆�̓f�t�H���g�ݒ�ɂ���
        if( state == (int)eGameState.Zako )
        {
            //  �ړ��x�N�g���ݒ�
            velocity.y = normalSpeed;
            return velocity;
        }
        else if( state == (int)eGameState.Boss ) // �{�X�풆�Ȃ甽�]
        {
            //  �ړ��x�N�g���ݒ�
            velocity.y = 20f;
            return velocity;
        }
        else if( state == (int)eGameState.Event ) // ��b�C�x���g���Ȃ猂�ĂȂ�
        {
            velocity.y = 0.0f;
            canShot = false;
            shotCount = 0;
        }

        return Vector3.zero;
    }

    //-------------------------------------------
    //  �ʏ�e
    //-------------------------------------------
    private void NormalShot(bool flipY)
    {
        //  �ʏ�e������
        if (!canShot)
        {
            if (shotCount >= shotInterval)
            {
                canShot = true;
                shotCount = 0;
            }
            else shotCount += Time.deltaTime;
        }
        else
        {
            //  �e����
            if (shot.IsPressed())
            {
                //  �t���O���Z�b�g
                canShot = false;

                //  �I�u�W�F�N�g�ꎞ�i�[�p
                GameObject obj = null;

                //  �ʏ�e�̑��x�ݒ�p
                NormalBullet n = null;;

                //  Velocity�i�[�p
                Vector3 v = Vector3.zero;

                //  Y���]�p��SpriteRenderer
                SpriteRenderer sr = null;

                //  Y���]���̔��ˌ���Y���W�o�C�A�X
                const float biasY = 0.44f;

                switch(normalShotLevel)
                {
                    case 1: //  ���x���P
                        obj = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint1.position,
                        Quaternion.identity);

                        //  Y�𔽓]���邩�ǂ����ݒ肷��
                        sr = obj.GetComponent<SpriteRenderer>(); 
                        sr.flipY = flipY;

                        //  ���]���ɍ��W�𒲐�
                        if(!sr.flipY)
                        {
                            obj.transform.position = 
                                new Vector3(firePoint1.position.x,
                                firePoint1.position.y + biasY,
                                firePoint1.position.z);
                        }

                        //  �{�X�킩�ǂ�����Velocity���擾���Đݒ�
                        v = GetReverseVelocity(gamestatus);
                        velocity = v;
                        n = obj.GetComponent<NormalBullet>();
                        n.SetVelocity(velocity);

                        break;
                    case 2: //  ���x���Q
                        const int lv2BulletNum = 3; //  ��x�ɏo��e�̐�

                        //  �e�̐����̃��X�g���m��
                        List<Transform> firePointLv2= new List<Transform>(lv2BulletNum);
                        firePointLv2.Add(firePoint1.transform);
                        firePointLv2.Add(firePoint2_L.transform);
                        firePointLv2.Add(firePoint2_R.transform);

                        for(int i=0;i<firePointLv2.Count;i++)
                        {
                            obj = Instantiate(
                            bulletPrefab[(int)SHOT_TYPE.NORMAL],
                            firePointLv2[i].position,
                            Quaternion.identity);

                            //  Y�𔽓]���邩�ǂ����ݒ肷��
                            sr = obj.GetComponent<SpriteRenderer>(); 
                            sr.flipY = flipY;

                            //  ���]���ɍ��W�𒲐�
                            if(!sr.flipY)
                            {
                                obj.transform.position = 
                                    new Vector3(
                                        firePointLv2[i].position.x,
                                        firePointLv2[i].position.y + biasY,
                                        firePointLv2[i].position.z);
                            }

                            //  �{�X�킩�ǂ�����Velocity���擾���Đݒ�
                            v = GetReverseVelocity(gamestatus);
                            velocity = v;
                            n = obj.GetComponent<NormalBullet>();
                            n.SetVelocity(velocity);
                        }
                        break;
                    case 3: //  ���x���R
                        const int lv3BulletNum = 5; //  ��x�ɏo��e�̐�

                        //  �e�̐����̃��X�g���m��
                        List<Transform> firePointLv3 = new List<Transform>(lv3BulletNum);
                        firePointLv3.Add(firePoint1.transform);
                        firePointLv3.Add(firePoint2_L.transform);
                        firePointLv3.Add(firePoint2_R.transform);
                        firePointLv3.Add(firePoint3_L.transform);
                        firePointLv3.Add(firePoint3_R.transform);

                        for(int i=0;i<firePointLv3.Count;i++)
                        {
                            obj = Instantiate(
                            bulletPrefab[(int)SHOT_TYPE.NORMAL],
                            firePointLv3[i].position,
                            Quaternion.identity);

                            //  Y�𔽓]���邩�ǂ����ݒ肷��
                            sr = obj.GetComponent<SpriteRenderer>(); 
                            sr.flipY = flipY;

                            //  ���]���ɍ��W�𒲐�
                            if(!sr.flipY)
                            {
                                obj.transform.position = 
                                    new Vector3(firePointLv3[i].position.x,
                                    firePointLv3[i].position.y + biasY,
                                    firePointLv3[i].position.z);
                            }

                            //  �{�X�킩�ǂ�����Velocity���擾���Đݒ�
                            v = GetReverseVelocity(gamestatus);
                            velocity = v;
                            n = obj.GetComponent<NormalBullet>();
                            n.SetVelocity(velocity);
                        }
                        break;
                }

                
            }
        }

        //if(!canShot)
        //{
        //    if(shotCount >= shotInterval)
        //    {
        //        canShot = true;
        //        shotCount = 0;
        //    }
        //    else shotCount += Time.deltaTime;
        //}
        //else
        //{
        //    //  �e����
        //    if( shot.WasPressedThisFrame() ) 
        //    {
        //        canShot = false;

        //        //  �ʏ�e�̘A���Ō���������0��
        //        bulletCount = 0;
        //    }
        //}

        ////  10�t���[����1�񌂂�
        //shotIntervalCount++;

        //if(shotIntervalCount % bulletInterval2 == 0)
        //{
        //    //  �T�A�V���b�g
        //    if(bulletCount < MaxContinuousShot)
        //    {
        //        Instantiate( bulletPrefab[(int)SHOT_TYPE.NORMAL], firePoint1_L.position, bulletPrefab[(int)SHOT_TYPE.NORMAL].transform.rotation);
        //        bulletCount++;
        //        shotIntervalCount = 0;
        //    }
        //}
    }

    //---------------------------------------------------
    //  �ʏ�e�̃��x���A�b�v
    //---------------------------------------------------
    public void LevelupNormalShot()
    {
        if(normalShotLevel < (int)eNormalShotLevel.Lv3)normalShotLevel++;
    }


    //---------------------------------------------------
    //  �ʏ�e�̃��x���_�E��
    //---------------------------------------------------
    public void LeveldownNormalShot()
    {
        if(normalShotLevel > (int)eNormalShotLevel.Lv1)normalShotLevel--;
    }

}
