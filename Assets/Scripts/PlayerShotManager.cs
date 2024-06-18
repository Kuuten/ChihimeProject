using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �v���C���[�̃V���b�g�Ǘ��N���X
//
//--------------------------------------------------------------
public class PlayerShotManager : MonoBehaviour
{
    //  �e�̔��˓_
    [SerializeField]private Transform firePoint;
    //  �e�̃v���n�u
    [SerializeField]private GameObject[] bulletPrefab;

    //  �ʏ�V���b�g�\�t���O
    private bool canShot;
    //  �V���b�g�Ԋu�̃J�E���g�p
    private float shotCount = 0;
    //  �e�����b���Ɍ��Ă邩
    private float shotInterval = 1;
    //  �e�̊Ԋu(�b)
    private float bulletInterval = 0.1f;

    //  �R���[�`��
    private IEnumerator shotCoroutine;

    //  �V���b�g�̎��
    enum SHOT_TYPE
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

    void Start()
    {
        shotCount = 0;
        canShot = true;

        //�@�ŏ��͒ʏ�e   
        shotCoroutine = NormalShot();
    }

    void Update()
    {
        if(!canShot)
        {
            if(shotCount >= shotInterval)
            {
                canShot = true;
                shotCount = 0;
            }
            else shotCount += Time.deltaTime;
        }
        else
        {
            //  �e����
            if( Input.GetKeyDown(KeyCode.Z)) 
            {
                canShot = false;

                shotCoroutine = NormalShot();

                StartCoroutine(shotCoroutine);
            }
        }
    }

    //---------------------------------------------------------
    //  �v���p�e�B
    //---------------------------------------------------------
    private void SetShotInterval(float interval)
    {
        shotInterval = interval;
    }
    private void SetBulletInterva(float interval)
    {
        bulletInterval = interval;
    }
    private float GetBulletInterval()
    {
        return bulletInterval;
    }
    private void SetCanShot(bool shot)
    {
        canShot = shot;
    }
    private bool GetCanShot()
    {
        return canShot;
    }

    //-------------------------------------------
    //  �ʏ�e�R���[�`��
    //-------------------------------------------
    private IEnumerator NormalShot()
    {
        Instantiate( bulletPrefab[(int)SHOT_TYPE.HORMING], firePoint.position, bulletPrefab[(int)SHOT_TYPE.NORMAL].transform.rotation);

        yield return new WaitForSeconds(bulletInterval);

        Instantiate( bulletPrefab[(int)SHOT_TYPE.HORMING], firePoint.position, bulletPrefab[(int)SHOT_TYPE.NORMAL].transform.rotation);

        yield return new WaitForSeconds(bulletInterval);

        Instantiate( bulletPrefab[(int)SHOT_TYPE.HORMING], firePoint.position, bulletPrefab[(int)SHOT_TYPE.NORMAL].transform.rotation);

        yield return new WaitForSeconds(bulletInterval);

        Instantiate( bulletPrefab[(int)SHOT_TYPE.HORMING], firePoint.position, bulletPrefab[(int)SHOT_TYPE.NORMAL].transform.rotation);

        yield return null;
    }

}
