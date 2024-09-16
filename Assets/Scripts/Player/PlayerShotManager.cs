using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

//--------------------------------------------------------------
//
//  �v���C���[�̃V���b�g�Ǘ��N���X
//
//--------------------------------------------------------------
//  �V���b�g�̎��
public enum SHOT_TYPE
{
    DOUJI,              // �h�E�W
    TSUKUMO,            // �c�N��
    KUCHINAWA,          // �N�`�i��
    KURAMA,             // �N���}
    WADATSUMI,          // ���_�c�~
    HAKUMEN,            // �n�N����

    TYPE_MAX
}

//  �m�[�}���e�̃��x�����X�g
enum eNormalShotLevel
{
    Lv1 = 1,
    Lv2,
    Lv3
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
    //  �ʏ�e�̃v���n�u
    [SerializeField]private GameObject normalBulletPrefab;
    //  ���o�[�g�e�̃v���n�u
    [SerializeField]private GameObject[] convertBulletPrefab;
    //  �z���t�B�[���h�I�u�W�F�N�g
    [SerializeField]private GameObject fieldObject;
    //  �z���t�B�[���h�I�u�W�F�N�g�̃X�P�[��
    private Vector3 fieldObjectScale;

    //  �e�̈ړ��x�N�g��
    private Vector3 velocity;
    //  �m�[�}���e�̃V���b�g�\�t���O
    private bool canShot;
    //  �m�[�}���e�̃V���b�gCD�̃J�E���g�p
    private float shotCount = 0;
    //  �m�[�}���e�̒e�����b���Ɍ��Ă邩
    private float shotInterval = 0.05f;
    //  �m�[�}���e�̈ړ���
    private const float normalSpeed = -20f; 
    //  �m�[�}���e�̍U����
    private float normalShotPower;
    //  �m�[�}���e�̃��x��
    private int normalShotLevel;
    //  �m�[�}���e�̃��x���\���p�摜�̃v���n�u
    [SerializeField] private GameObject shotPowerLevelIcon;
    //  �m�[�}���e�̃V���b�g�p���[�A�C�R���̃��X�g
    private List<GameObject> shotPowerIconList = new List<GameObject>();
    //  �m�[�}���e�̃V���b�g�p���[�A�C�R���̐e�I�u�W�F�N�g�̈ʒu�擾�p
    [SerializeField] private GameObject shotPowerIconRootObj;

    //  ���o�[�g�e�̎��
    public enum CONVERT_TYPE
    {
        MIDDLE,
        FULL,

        MAX
    }

    //  ���o�[�g�e��SE��ID
    int[,] sfxId = new int[(int)SHOT_TYPE.TYPE_MAX,(int)CONVERT_TYPE.MAX]
        {
            //  �h�E�W
            { (int)SFXList.SFX_DOUJI_CONVERT_SHOT_MIDDLE,
              (int)SFXList.SFX_DOUJI_CONVERT_SHOT_FULL },
            //  �c�N��
            { (int)SFXList.SFX_DOUJI_CONVERT_SHOT_MIDDLE,
              (int)SFXList.SFX_DOUJI_CONVERT_SHOT_FULL },
            //  �N�`�i��
            { (int)SFXList.SFX_DOUJI_CONVERT_SHOT_MIDDLE, 
              (int)SFXList.SFX_DOUJI_CONVERT_SHOT_FULL },
            //  �N���}
            { (int)SFXList.SFX_DOUJI_CONVERT_SHOT_MIDDLE, 
              (int)SFXList.SFX_DOUJI_CONVERT_SHOT_FULL },
            //  ���_�c�~
            { (int)SFXList.SFX_DOUJI_CONVERT_SHOT_MIDDLE,
              (int)SFXList.SFX_DOUJI_CONVERT_SHOT_FULL },
            //  �n�N����
            { (int)SFXList.SFX_DOUJI_CONVERT_SHOT_MIDDLE,
              (int)SFXList.SFX_DOUJI_CONVERT_SHOT_FULL },
        };

    //  ���o�[�g�Q�[�W�p�̃X���C�_�[�P
    [SerializeField]private GameObject sliderObj1;
    //  ���o�[�g�Q�[�W�p�̃X���C�_�[�Q
    [SerializeField]private GameObject sliderObj2;
    //  ���o�[�g�Q�[�W�P�����܂�X�s�[�h
    private const float gauge1Speed = 2.0f;
    //  ���o�[�g�Q�[�W�P����������X�s�[�h
    private const float gauge1MinusSpeed = 1.0f;
    //  ���o�[�g�Q�[�W�Q�����܂�X�s�[�h
    private const float gauge2Speed = 2.0f;
    //  ���o�[�g�Q�[�W�P����������X�s�[�h
    private const float gauge2MinusSpeed = 1.0f;
    //  ���o�[�g�Q�[�W����p�X�e�b�v�ϐ�
    private int releaseStep;
    //  ���o�[�g�V���b�g�����Ă邩�ǂ���
    private bool canConvert;
    //  ���o�[�g�Q�[�W�̑����p�ϐ�
    private float gaugeValue;
    //  ���o�[�g�Q�[�W�̗��܂�
    enum ConvertState
    {
        None,               //  ��
        Restore,            //  �`���[�W��
        ReleaseRestore,     //  �s���S�����
        MidPower,           //  ���U��
        ReleaseMidPower,    //  ���U�������
        FullPower,          //  ���U��
        ReleaseFullPower    //  ���U�������
    }
    private ConvertState convertState;
    //  �R���o�[�g�e�����U�����ǂ���
    bool convertIsFullPower;
    //  ���o�[�g�e�̈З�(���U��)
    private float[] convertShotPowerHalf = new float[(int)SHOT_TYPE.TYPE_MAX];
    //  ���o�[�g�e�̈З�(���U��)
    private float[] convertShotPowerFull = new float[(int)SHOT_TYPE.TYPE_MAX];

    //  �e�X�g�p
    public int gamestatus;
    InputAction test;
    bool b;

    //  ����
    InputAction shot;
    InputAction shotConvert;


    void Start()
    {
        // InputAction��Shot��ݒ�
        PlayerInput playerInput = GetComponent<PlayerInput>();
        shot = playerInput.actions["Shot"];
        shotConvert = playerInput.actions["ConvertShot"];
        test = playerInput.actions["TestButton2"]; 

        normalShotPower = 1.0f;
        shotCount = 0;
        canShot = true;
        normalShotLevel = 1; // �ŏ��̓��x���P
        gaugeValue = 0.0f;
        convertState = ConvertState.None;
        fieldObjectScale = new Vector3(3f,3f,3f);
        releaseStep = 0;
        canConvert =true;
        convertIsFullPower = false;

        //  ���o�[�g�e���Ƃ̒e�̈З�
        convertShotPowerHalf[(int)SHOT_TYPE.DOUJI] = 10f;
        convertShotPowerHalf[(int)SHOT_TYPE.TSUKUMO] = 2f;
        convertShotPowerHalf[(int)SHOT_TYPE.KUCHINAWA] = 1f;
        convertShotPowerHalf[(int)SHOT_TYPE.KURAMA] = 7f;
        convertShotPowerHalf[(int)SHOT_TYPE.WADATSUMI] = 2f;
        convertShotPowerHalf[(int)SHOT_TYPE.HAKUMEN] = 5f;

        convertShotPowerFull[(int)SHOT_TYPE.DOUJI] = 30f;
        convertShotPowerFull[(int)SHOT_TYPE.TSUKUMO] = 5f;
        convertShotPowerFull[(int)SHOT_TYPE.KUCHINAWA] = 2f;
        convertShotPowerFull[(int)SHOT_TYPE.KURAMA] = 15f;
        convertShotPowerFull[(int)SHOT_TYPE.WADATSUMI] = 5f;
        convertShotPowerFull[(int)SHOT_TYPE.HAKUMEN] = 10f;

        //  �e�I�u�W�F�N�g�̎q�I�u�W�F�N�g�Ƃ��ă{���A�C�R���𐶐�
        for( int i=0; i<(int)eNormalShotLevel.Lv3;i++ )
        {
            GameObject obj = Instantiate(shotPowerLevelIcon);
            obj.GetComponent<RectTransform>().SetParent( shotPowerIconRootObj.transform);
            obj.GetComponent<RectTransform>().localScale = Vector3.one;
            obj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0,0,0);

            shotPowerIconList.Add( obj );   //  ���X�g�ɒǉ�
        }

        //  �e�X�g�p
        gamestatus = (int)eGameState.Zako;
        b = true;

        //  �e�̌����͂Ƃ肠�����ʏ�e�ɍ��킹��
        velocity = new Vector3(0,normalSpeed,0);   //  �ŏ��͉������֌���
    }

    void Update()
    {
        //  GameManager�����Ԃ��擾
        gamestatus = gameManager.GetGameState();

        //  �ʏ�e���x���A�C�R�����X�V
        UpdateShotPowerIcon();


        //  Enter�ŃU�R�{�X�؂�ւ�
        if (test.WasPressedThisFrame())
        {
            b = !b;
            Debug.Log("�؂�ւ��t���O:" + b);

            if (b)
            {
                gamestatus = (int)eGameState.Zako;
                gameManager.SetGameState(gamestatus);
            }
            else
            {
                gamestatus = (int)eGameState.Boss;
                gameManager.SetGameState(gamestatus);
            }

            GameManager.Instance.SetGameState(gamestatus);

            Debug.Log("gamestatus:" + gamestatus);
        }

        //  �Q�[���i�K�ʏ���
        switch (gamestatus)
        {
            case (int)eGameState.Zako:
                shot.Enable();
                shotConvert.Enable();
                NormalShot(true);                    //  �ʏ�e
                ConvertShot(true);                   //  ���o�[�g�e
                break;
            case (int)eGameState.Boss:
                shot.Enable();
                shotConvert.Enable();
                NormalShot(false);                   //  �ʏ�e
                ConvertShot(false);                  //  ���o�[�g�e
                break;
            case (int)eGameState.Event:
                shot.Disable();
                shotConvert.Disable();
                NormalShot(false);                   //  �ʏ�e
                ConvertShot(false);                  //  ���o�[�g�e
                break;
        }

    }

    //------------------------------------------------
    //  �ʏ�e�̃p���[�A�C�R���̐����X�V����
    //------------------------------------------------
    private void UpdateShotPowerIcon()
    {
        if(normalShotLevel <= 0)Debug.LogError("normalShotLevel��0�ȉ��̒l�������Ă��܂��I");

        //  1��S���\��
        for(int i=0;i<shotPowerIconList.Count;i++)
        {
            shotPowerIconList[i].gameObject.SetActive(true);
        }

        //  ��\������
        for(int i=shotPowerIconList.Count-1;i>normalShotLevel-1;i--)
        {
            shotPowerIconList[i].gameObject.SetActive(false);
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
    public float GetConvertShotPower()
    {
        if(convertIsFullPower)return convertShotPowerFull[(int)PlayerInfoManager.g_CONVERTSHOT];
        else return convertShotPowerHalf[(int)PlayerInfoManager.g_CONVERTSHOT];
    }

    //  �e�̈ړ��x�N�g���𔽓]����
    public Vector3 GetReverseVelocity(int state)
    {
        //  �U�R�풆�̓f�t�H���g�ݒ�ɂ���
        if( state == (int)eGameState.Zako )
        {
            //  �ړ��x�N�g���ݒ�
            velocity.x = 0f;
            velocity.y = normalSpeed;
            velocity.z = 0f;
            return velocity;
        }
        else if( state == (int)eGameState.Boss ) // �{�X�풆�Ȃ甽�]
        {
            //  �ړ��x�N�g���ݒ�
            velocity.x = 0f;
            velocity.y = 20f;
            velocity.z = 0f;
            return velocity;
        }
        else if( state == (int)eGameState.Event ) // ��b�C�x���g���Ȃ猂�ĂȂ�
        {
            velocity.x = 0f;
            velocity.y = 0.0f;
            velocity.z = 0f;
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

                //  �ʏ�eSE�Đ�
                SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.SFX_NORMAL_SHOT,
                    (int)SFXList.SFX_NORMAL_SHOT);

                switch(normalShotLevel)
                {
                    case 1: //  ���x���P
                        obj = Instantiate(
                        normalBulletPrefab,
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
                            normalBulletPrefab,
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
                        float Degree = 10;  //  �p�x

                        //  �e�̐����̃��X�g���m��
                        List<Transform> firePointLv3 = new List<Transform>(lv3BulletNum);
                        firePointLv3.Add(firePoint1.transform);
                        firePointLv3.Add(firePoint2_L.transform);
                        firePointLv3.Add(firePoint2_R.transform);
                        firePointLv3.Add(firePoint3_L.transform);
                        firePointLv3.Add(firePoint3_R.transform);

                        for(int i=0;i<firePointLv3.Count;i++)
                        {
                            //  �e�𐶐�
                            obj = Instantiate(
                                normalBulletPrefab,
                                firePointLv3[i].position,
                                Quaternion.identity);

                            //  Y�𔽓]���邩�ǂ����ݒ肷��
                            sr = obj.GetComponent<SpriteRenderer>();
                            sr.flipY = flipY;

                            //  ���]���ɍ��W�𒲐�
                            if (!sr.flipY)
                            {
                                obj.transform.position =
                                    new Vector3(firePointLv3[i].position.x,
                                    firePointLv3[i].position.y + biasY,
                                    firePointLv3[i].position.z);
                            }

                            //  �{�X�킩�ǂ�����Velocity���擾
                            v = GetReverseVelocity(gamestatus);
                            //  �[�̒e�����p�x������
                            if (i == firePointLv3.Count - 2)
                            {
                                if (gamestatus == (int)eGameState.Zako)
                                {
                                    v = Quaternion.Euler(0, 0, -Degree) * v;
                                    obj.transform.Rotate(0, 0, -Degree);
                                }
                                else
                                {
                                    v = Quaternion.Euler(0, 0, Degree) * v;
                                    obj.transform.Rotate(0, 0, Degree);
                                }

                            }
                            else if (i == firePointLv3.Count - 1)
                            {
                                if (gamestatus == (int)eGameState.Zako)
                                {
                                    v = Quaternion.Euler(0, 0, Degree) * v;
                                    obj.transform.Rotate(0, 0, Degree);
                                }
                                else
                                {
                                    v = Quaternion.Euler(0, 0, -Degree) * v;
                                    obj.transform.Rotate(0, 0, -Degree);
                                }
                            }
                            velocity = v;
                            n = obj.GetComponent<NormalBullet>();
                            n.SetVelocity(velocity);
                        }
                        break;
                }
            }
            //  �{�^���𗣂����u��
            if (shot.WasReleasedThisFrame())
            {
                //  �J�E���g�J�n
                
            }
        }
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
    
    //-------------------------------------------
    //  ���o�[�g�e
    //-------------------------------------------
    private void ConvertShot(bool flipY)
    {
        //  Slider�R���|�[�l���g���擾
        Slider slider1 = sliderObj1.GetComponent<Slider>();
        Slider slider2 = sliderObj2.GetComponent<Slider>();
        
        //  Y���]�p��SpriteRenderer
        SpriteRenderer sr = null;

        //  Velocity�i�[�p
        Vector3 v = Vector3.zero;

        //  ���o�[�X�g�p
        PlayerBombManager konburst = this.GetComponent<PlayerBombManager>();

        switch(convertState)
        {
            case ConvertState.None:         //  ��
                //  �Q�[�W�P�����Z�b�g
                slider1.value = 0.0f;
                sliderObj1.SetActive(false);

                //  �X���C�_�[�Q�����Z�b�g
                slider2.value = 0f;
                sliderObj2.SetActive(false);
                break;
            case ConvertState.Restore:      // ���ߓr��
                //  �X���C�_�[�P�L����
                sliderObj1.SetActive(true);

                //  �Q�[�W�P�̒l�𑝉�������
                if(1.0f <= slider1.value)
                {
                    slider1.value = 1.0f;
                    gaugeValue = 0.0f;

                    sliderObj2.SetActive(true);



                    //  ����SE�Đ�
                    SoundManager.Instance.PlaySFX(
                        (int)AudioChannel.SFX_CONVERT_SHOT,
                        (int)SFXList.SFX_CONVERT_SHOT_GAUGE2);
                    convertState = ConvertState.MidPower;
                }
                else
                {
                    gaugeValue += gauge1Speed * Time.deltaTime;
                    slider1.value = gaugeValue;
                }

                break;
            case ConvertState.ReleaseRestore:  //  �s���S���
                //  �Q�[�W�����Z�b�g
                slider1.value = 0.0f;
                sliderObj1.SetActive(false);

                //  �s��SE�Đ�
                SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.SFX,
                    (int)SFXList.SFX_CONVERT_SHOT_FAIL);

                //  ���Ƀ��Z�b�g
                gaugeValue = 0.0f;
                convertState = ConvertState.None;

                break;
            case ConvertState.MidPower:  // ���U��
                //  �Q�[�W�Q�̒l�𑝉�������
                if(1.0f <= slider2.value)
                {
                    slider2.value = 1.0f;

                    convertState = ConvertState.FullPower;
                }
                else
                {
                    gaugeValue += gauge2Speed * Time.deltaTime;
                    slider2.value = gaugeValue;
                }
                break;

            case ConvertState.ReleaseMidPower:  // ���U���̎��ɗ�����
                if( slider1.value <= 0.0f )
                {
                    canConvert = true; //  �R���o�[�g���߉\

                    //  �X���C�_�[�P�����Z�b�g
                    slider1.value = 0f;
                    sliderObj1.SetActive(false);
                    gaugeValue = 0.0f;

                    //  ���Ƀ��Z�b�g
                    convertState = ConvertState.None;
                }
                else
                {
                    gaugeValue -= gauge1MinusSpeed * Time.deltaTime;
                    slider1.value = gaugeValue;
                }

                break;
            case ConvertState.FullPower:        // ���U��
                break;
            case ConvertState.ReleaseFullPower:        // ���U���̎��ɗ�����
                switch(releaseStep)
                {
                    case 0:
                        if( slider2.value <= 0.0f )
                        {
                            //  �X���C�_�[�Q�����Z�b�g
                            slider2.value = 0f;
                            sliderObj2.SetActive(false);
                            gaugeValue = 1.0f;
                            releaseStep++;
                        }
                        else
                        {
                            gaugeValue -= gauge2MinusSpeed * Time.deltaTime;
                            slider2.value = gaugeValue;
                        }
                        break;
                    case 1:
                        if( slider1.value <= 0.0f )
                        {
                            canConvert = true; //  �R���o�[�g���߉\

                            //  �X���C�_�[�Q�����Z�b�g
                            slider1.value = 0f;
                            sliderObj2.SetActive(false);
                            gaugeValue = 0.0f;
                            releaseStep = 0;

                            //  ���Ƀ��Z�b�g
                            convertState = ConvertState.None;
                        }
                        else
                        {
                            gaugeValue -= gauge1MinusSpeed * Time.deltaTime;
                            slider1.value = gaugeValue;
                        }
                        break;
                }
                break;
        
        }


        if(canConvert)
        {
            //  �����Ă���ԃQ�[�W�𑝉�������
            if (shotConvert.IsPressed())
            {
                if(convertState == ConvertState.None)
                {
                    KonFieldAlphaAnimation(0.0f,0.784f,0.5f);
                    fieldObject.transform.DOScale(
                        new Vector3(6f,6f,6f),
                        0.5f);
                    if(convertState == ConvertState.None)
                    {
                        //  ����SE�Đ�
                        SoundManager.Instance.PlaySFX(
                            (int)AudioChannel.SFX_CONVERT_SHOT,
                            (int)SFXList.SFX_CONVERT_SHOT_GAUGE1);

                        convertState = ConvertState.Restore;
                    }
                }
            }

            // �{�^���𗣂���
            if (shotConvert.WasReleasedThisFrame())
            {
                KonFieldAlphaAnimation(0.784f,0.0f,0.5f);
                fieldObject.transform.DOScale(
                    new Vector3(3f,3f,3f),
                    0.5f);
                if(convertState == ConvertState.Restore)
                {
                    Debug.Log("���ߓr���ŗ������I");

                    convertState = ConvertState.ReleaseRestore;
                } 
                else if(convertState == ConvertState.MidPower)
                {
                    Debug.Log("���U���ŗ������I");

                    canConvert = false; //  �N�[���_�E��

                    //  �X���C�_�[�Q�����Z�b�g
                    slider2.value = 0f;
                    sliderObj2.SetActive(false);

                    //  ���U��SE�Đ�
                    SoundManager.Instance.PlaySFX(
                        (int)AudioChannel.SFX_CONVERT_SHOT,
                        sfxId[(int)PlayerInfoManager.g_CONVERTSHOT,(int)CONVERT_TYPE.MIDDLE]);

                    //  ���o�[�g�e����
                    GameObject obj = Instantiate(
                        convertBulletPrefab[(int)PlayerInfoManager.g_CONVERTSHOT],
                        this.transform.position,
                            Quaternion.identity);

                    //  �X�P�[���𔼕���
                    obj.transform.localScale = new Vector3(0.5f,0.5f,0.5f);

                    //  �З͂𒆍U���ɐݒ�
                    convertIsFullPower = false;

                    //  Y�𔽓]���邩�ǂ����ݒ肷��
                    sr = obj.GetComponent<SpriteRenderer>(); 
                    sr.flipY = flipY;

                    //  ���o�[�X�g�Q�[�W���������₷
                    konburst.PlusKonburstGauge(false);

                    convertState = ConvertState.ReleaseMidPower;
                }
                else if(convertState == ConvertState.FullPower)
                {
                    Debug.Log("���U���ŗ������I");

                    canConvert = false; //  �N�[���_�E��

                    //  ���U��SE�Đ�
                    SoundManager.Instance.PlaySFX(
                        (int)AudioChannel.SFX_CONVERT_SHOT,
                        sfxId[(int)PlayerInfoManager.g_CONVERTSHOT,(int)CONVERT_TYPE.FULL]);

                    //  ���o�[�g�e����
                    GameObject obj = Instantiate(
                        convertBulletPrefab[(int)PlayerInfoManager.g_CONVERTSHOT],
                        this.transform.position,
                            Quaternion.identity);

                    //  �З͂����U���ɐݒ�
                    convertIsFullPower = true;

                    //  Y�𔽓]���邩�ǂ����ݒ肷��
                    sr = obj.GetComponent<SpriteRenderer>(); 
                    sr.flipY = flipY;

                    //  ���o�[�X�g�Q�[�W�𑝂₷
                    konburst.PlusKonburstGauge(true);

                    convertState = ConvertState.ReleaseFullPower;
                }
            }
        }
    }

    //-----------------------------------------------------
    //  �z���t�B�[���h�̃A���t�@�A�j���[�V����
    //-----------------------------------------------------
    private void KonFieldAlphaAnimation(float start, float goal, float time)
    {
        //  ���[���Əo������
        SpriteRenderer  sr = fieldObject.GetComponent<SpriteRenderer>();
        sr.enabled = true;
        var c = sr.color;
        c.a = start; // �����l
        sr.color = c;

        DOTween.ToAlpha(
	        ()=> sr.color,
	        color => sr.color = color,
	        goal, // �ڕW�l
	        time // ���v����
        );
    }
}
