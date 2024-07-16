using System.Collections;
using System.Collections.Generic;
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
    POWER,         // �h�E�W
    WIDE,          // �N���}
    PENETRATION,   // �N�`�i��
    HORMING,       // �c�N��
    SHIELD,        // ���_�c�~
    ALMIGHT,       // �n�N����

    TYPE_MAX
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

    //  �e�̕���
    private Quaternion shotRotation;
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

    public int gamestatus = 0;


    //  ����
    InputAction shot;

    void Start()
    {
        // InputAction��Move��ݒ�
        PlayerInput playerInput = GetComponent<PlayerInput>();
        shot = playerInput.actions["Shot"];

        shotCount = 0;
        canShot = true;
        normalShotLevel = 1; // �ŏ��̓��x���P
        //  �e�̌����͂Ƃ肠�����ʏ�e�ɍ��킹��
        shotRotation = bulletPrefab[(int)SHOT_TYPE.NORMAL].transform.rotation;
        velocity = new Vector3(0,normalSpeed,0);   //  �ŏ��͉������֌���
    }

    void Update()
    {
        //  GameManager�����Ԃ��擾
        int gamestatus = gameManager.GetGameState();

        //  �ʏ�e
        NormalShot(gamestatus);
        
    }

    //---------------------------------------------------------
    //  �v���p�e�B
    //---------------------------------------------------------

    //  �e�̉摜�̌����ƈړ��x�N�g���𔽓]����
    public void Reverse(int state)
    {
        //  �U�R�풆�̓f�t�H���g�ݒ�ɂ���
        if( state == (int)eGameState.Zako )
        {
            shotRotation = Quaternion.Euler(0,0,180);

            //  �ړ��x�N�g���ݒ�
            velocity = new Vector3(0, normalSpeed, 0);
        }
        else // �{�X�킩��b�C�x���g���Ȃ甽�]
        {
            //  �e�̈ړ��x�N�g�����������Ȃ�
            if(velocity.y == normalSpeed)
            {
                //  �摜�̌������]
                shotRotation = Quaternion.Euler(0,0,0);

                //  �ړ��x�N�g���ݒ�
                velocity.y = -normalSpeed;
            }
        }
    }

    //-------------------------------------------
    //  �ʏ�e
    //-------------------------------------------
    private void NormalShot(int state)
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

                //  �e�̉摜�̌������]
                Reverse(state);

                switch(normalShotLevel)
                {
                    case 1: //  ���x���P
                        NormalBullet n = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint1.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n.SetVelocity(velocity);
                        n.SetRotation(shotRotation);
                        
                        break;
                    case 2: //  ���x���Q
                        NormalBullet n2 = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint1.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n2.SetVelocity(velocity);
                        n2.SetRotation(shotRotation);

                        NormalBullet n3 = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint2_L.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n3.SetVelocity(velocity);
                        n3.SetRotation(shotRotation);

                        NormalBullet n4 = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint2_R.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n4.SetVelocity(velocity);
                        n4.SetRotation(shotRotation);
                        break;
                    case 3: //  ���x���R
                        NormalBullet n5 = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint1.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n5.SetVelocity(velocity);
                        n5.SetRotation(shotRotation);

                        NormalBullet n6 = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint2_L.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n6.SetVelocity(velocity);
                        n6.SetRotation(shotRotation);

                        NormalBullet n7 = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint2_R.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n7.SetVelocity(velocity);
                        n7.SetRotation(shotRotation);

                        NormalBullet n8 = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint3_L.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n8.SetVelocity(velocity);
                        n8.SetRotation(shotRotation);

                        NormalBullet n9 = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint3_R.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n9.SetVelocity(velocity);
                        n9.SetRotation(shotRotation);
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

}
