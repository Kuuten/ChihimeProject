using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  �v���C���[�̃{���E���o�[�X�g�Ǘ��N���X
//
//--------------------------------------------------------------
public class PlayerBombManager : MonoBehaviour
{
    [SerializeField] FadeIO Fade;
    [SerializeField] GameObject bombFadeObject;
    [SerializeField] GameObject CanvasObject;
    [SerializeField] GameObject MainCanvasObject;
    [SerializeField] GameObject bombCollision;
    [SerializeField] GameObject konBurstCollision;
    private GameObject FadeObj;
    [SerializeField] BombFade bombFade;

    //  �V���O���g���ȃC���X�^���X
    public static PlayerBombManager Instance
    {
        get; private set;
    }

    //  �h�E�W�̍��o�[�X�g�v���n�u
    [SerializeField] GameObject doujiKonburstPrefab;
    //  ���o�[�X�g�J�b�g�C���摜�v���n�u
    [SerializeField] GameObject[] konburstCutinPrefab;
    //  ���o�[�X�g�̈З�
    private float[] konburstShotPower = new float[(int)SHOT_TYPE.TYPE_MAX];
    //  ���o�[�X�g�Q�[�W�̃X���C�_�[
    [SerializeField] Slider konburstSlider;
    //  ���o�[�X�g�Q�[�WMAX���̃����v
    [SerializeField] GameObject konburstLamp;
    //  ���o�[�X�g�Q�[�W��1�񂠂���̑�����
    private const float konbrstPlusValue = 0.1f;    //  �b��l
    //  ���o�[�X�g�Q�[�W��Fill�I�u�W�F�N�g
    [SerializeField] GameObject koburstGaugeFill;
    //  ���o�[�X�g�Q�[�W��Fill�̃f�t�H���g�摜
    [SerializeField] Sprite koburstDefaultSprite;

    //  �{���A�C�R���̐e�I�u�W�F�N�g�̈ʒu�擾�p
    [SerializeField] private GameObject bombIconRootObj;
    //  �{���A�C�R���̃v���n�u
    [SerializeField] private GameObject bombIconPrefab;
    //  �{���A�C�R���I�u�W�F�N�g�̃��X�g
    private List<GameObject> bombIconList = new List<GameObject>();
    private int bombNum;
    private const int bombMaxNum = 5;
    private float bombPower = 20f; //  �{��1���̈З�

    InputAction inputBomb;
    bool bCanBomb;      //  �{���������ł��邩�ǂ���

    //  �U�R��I����̃{���R���W�����t���O
    bool isCalledOnce;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // InputAction��Move��ݒ�
        PlayerInput playerInput = GetComponent<PlayerInput>();
        inputBomb = playerInput.actions["Bomb"];

        //  PlayerInfoManager��������Z�b�g
        bombNum = PlayerInfoManager.g_BOMBNUM;

        //  �ŏ���null
        FadeObj = null;

        //  �ŏ��͔����ł���
        bCanBomb = true;

        //  �t���OOFF
        isCalledOnce =false;

        //  �ŏ��̓��C���{�[OFF
        koburstGaugeFill.GetComponent<Animator>().enabled = false;

        //  ���o�[�X�g���Ƃ̒e�̈З�
        konburstShotPower[(int)SHOT_TYPE.DOUJI]     = 100f;
        konburstShotPower[(int)SHOT_TYPE.TSUKUMO]   = 1f;
        konburstShotPower[(int)SHOT_TYPE.KUCHINAWA] = 5f;
        konburstShotPower[(int)SHOT_TYPE.KURAMA]    = 40f;
        konburstShotPower[(int)SHOT_TYPE.WADATSUMI] = 1f;   //  �n�[�g�񕜗�
        konburstShotPower[(int)SHOT_TYPE.HAKUMEN]   = 10f;

        //  �e�I�u�W�F�N�g�̎q�I�u�W�F�N�g�Ƃ��ă{���A�C�R���𐶐�
        for( int i=0; i<bombMaxNum;i++ )
        {
            GameObject obj = Instantiate(bombIconPrefab);
            obj.GetComponent<RectTransform>().SetParent( bombIconRootObj.transform);
            obj.GetComponent<RectTransform>().localScale = Vector3.one;
            obj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0,0,0);

            bombIconList.Add( obj );   //  ���X�g�ɒǉ�
        }
    }

    void Update()
    {
        //  �{���̃A�C�R�����X�V
        UpdateBombIcon();

        //  GameManager�����Ԃ��擾
        int gamestatus = GameManager.Instance.GetGameState();

        //  �Q�[���i�K�ʏ���
        switch(gamestatus)
        {
            case (int)eGameState.Zako:
                inputBomb.Enable();
                BombAndKonBurstUpdate(true);    //  �{���E���o�[�X�g�̍X�V
                break;
            case (int)eGameState.Boss:
                inputBomb.Enable();
                BombAndKonBurstUpdate(false);   //  �{���E���o�[�X�g�̍X�V
                break;
            case (int)eGameState.Event:
                inputBomb.Disable();

                //  1��{���R���W������ON�ɂ��Ēe������
                if (!isCalledOnce) {
                    isCalledOnce = true;
                    //  �{���R���W����ON�I
                    bombCollision.SetActive(true);

                    //  �Q�b��ɃR���W���������Z�b�g
                    StartCoroutine(ResetCollision());
                }
                break;
        }
    }

    //  �{�������Z
    public void AddBomb()
    {
        if(bombNum < bombMaxNum)
        {
            bombNum++;
        }
        else bombNum = bombMaxNum;
    }

    //  �{�������Z
    public void SubBomb()
    {
        if(bombNum > 0)
        {
            bombNum--;
        }
        else
        {
            bombNum = 0;
        }
    }

    //-------------------------------------------
    //  �{���E���o�[�X�g�𔭓�����
    //-------------------------------------------
    private void BombAndKonBurstUpdate(bool fripY)
    {
        //  ���o�[�X�g�Q�[�W��MAX��������
        if( konburstSlider.value >= 1.0f )
        {
            //  ���o�[�X�g�Q�[�W��_��Animator�ɐ؂�ւ�

            if(inputBomb.WasPressedThisFrame()) //  �{���{�^���������ꂽ�I
            {
                //  �{���𔭓��s�\�ɂ���
                bCanBomb = false;

                //  ���o�[�X�g�Q�[�W�����Z�b�g&�����vOFF
                ResetKonburstGauge();

                //  ���o�[�X�g�Q�[�W��ʏ�Animator�ɐ؂�ւ�

                //  ���o�[�X�g���o���J�n����
                if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.DOUJI)
                    StartCoroutine(DoujiKonBurst(fripY));
                else if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.TSUKUMO)
                    StartCoroutine(TsukumoKonBurst(fripY));
                else if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.KUCHINAWA)
                    StartCoroutine(KuchinawaKonBurst(fripY));
                else if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.KURAMA)
                    StartCoroutine(KuramaKonBurst(fripY));
                else if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.WADATSUMI)
                    StartCoroutine(WadatsumiKonBurst(fripY));
                else if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.HAKUMEN)
                    StartCoroutine(HakumenKonBurst(fripY));
            }
        }
        else // �{�������o�[�X�g�ł͂Ȃ���
        {
            if( bCanBomb )  //  �{���������\�Ȏ�
            {
                if(inputBomb.WasPressedThisFrame()) //  �{���{�^���������ꂽ�I
                {
                    bCanBomb = false;

                    Debug.Log("�{�����o�J�n�I");

                    //  �{����1���炷
                    SubBomb();

                    //  �{�����o���J�n����
                    StartCoroutine( BombAnimation() );
                }
            }
            else // ���������{���̎c�e���Ȃ���
            {
                if( inputBomb.WasPressedThisFrame() ) //  �{���{�^���������ꂽ�I
                {
                    //  �������Ȃ�
                }
            }
        }

        //  �{����0�Ȃ甭���ł��Ȃ�
        if(bombNum <= 0)bCanBomb = false; 
    }

    //-------------------------------------------
    //  �{�����o
    //-------------------------------------------
    private IEnumerator BombAnimation()
    {
        //  �{���̓����蔻��I�u�W�F�N�g��L����
        bombCollision.SetActive(true);

        //  �v���C���[�𖳓G�ɂ���
        PlayerHealth ph = this.GetComponent<PlayerHealth>();
        ph.SetSuperMode(true);

        //  �e��S�ď����p�̃t�F�[�h�𐶐�
        GameObject FadeObj = bombFadeObject;
        FadeObj.gameObject.transform.SetParent( CanvasObject.transform );
        FadeObj.gameObject.transform.SetAsFirstSibling();   //  �q�G�����L�[�̈�ԏ��
        FadeObj.GetComponent<RectTransform>().transform.localPosition = Vector3.zero;

        //  �����������L���b�I���Č���

        //  0.3�b�҂�
        yield return new WaitForSeconds(0.3f);

        //  ���������I�u�W�F�N�g�폜

        //  ��ʂ��z���C�g�Ńt�F�[�h�A�E�g����
        BombFade bombFade = FadeObj.GetComponent<BombFade>();
        yield return StartCoroutine(bombFade.StartFadeOut(FadeObj, 0.2f));

        //  0.2�b�҂�
        yield return new WaitForSeconds(0.2f);

        //  �����A�j���I�u�W�F�N�g�Đ�

        //  �Đ��I����҂�
        yield return new WaitForSeconds(1.5f);

        //  �����A�j���I�u�W�F�N�g�폜

        //  ��ʂ��z���C�g�Ńt�F�[�h�C������
        yield return StartCoroutine(bombFade.StartFadeIn(FadeObj, 0.2f));

        //  0.2�b�҂�
        yield return new WaitForSeconds(0.2f);

        //  �{���̓����蔻��I�u�W�F�N�g�𖳌���
        bombCollision.SetActive(false);

        //  �v���C���[�̖��G������
        ph.SetSuperMode(false);

        //  �{�������\�ɖ߂�
        bCanBomb = true;


        Debug.Log("�{�����o�I���I");

        yield return null;
    }

    //-------------------------------------------
    //  ���o�[�X�g�J�n���o
    //-------------------------------------------
    private IEnumerator StartingKonBurst(PlayerHealth ph)
    {
        //  ���o�[�X�g�̒e�����p�̓����蔻��I�u�W�F�N�g��L����
        konBurstCollision.SetActive(true);

        //  �v���C���[�𖳓G�ɂ���
        ph = this.GetComponent<PlayerHealth>();
        ph.SetSuperMode(true);

        //  �e��S�ď����p�̃t�F�[�h�𐶐�
        FadeObj = bombFadeObject;
        FadeObj.gameObject.transform.SetParent( CanvasObject.transform );
        FadeObj.gameObject.transform.SetAsFirstSibling();   //  �q�G�����L�[�̈�ԏ��
        FadeObj.GetComponent<RectTransform>().transform.localPosition = Vector3.zero;

        //  �J�b�g�C���Đ�
        GameObject obj =
            Instantiate(konburstCutinPrefab[(int)PlayerInfoManager.g_CONVERTSHOT]);
        obj.transform.SetParent( MainCanvasObject.transform, false );

        //  �J�b�g�C��SE�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_BOMB,
            (int)SFXList.SFX_KONBURST_CUTIN);

        //  ��ʂ��z���C�g�Ńt�F�[�h�A�E�g����
        bombFade = FadeObj.GetComponent<BombFade>();
        yield return StartCoroutine(bombFade.StartFadeOut(FadeObj, 0.2f));
    }

    //-------------------------------------------
    //  ���o�[�X�g�I�����o
    //-------------------------------------------
    private IEnumerator EndingKonBurst(PlayerHealth ph)
    {
        //  ��ʂ��z���C�g�Ńt�F�[�h�C������
        yield return StartCoroutine(bombFade.StartFadeIn(FadeObj, 0.2f));

         //  ���o�[�X�g�̒e�����p�̓����蔻��I�u�W�F�N�g��L����
        konBurstCollision.SetActive(false);

        //  �v���C���[�̖��G������
        ph.SetSuperMode(false);

        //  �{�������\�ɖ߂�
        bCanBomb = true;
    }

    //-------------------------------------------
    //  �h�E�W�̍��o�[�X�g���o
    //-------------------------------------------
    private IEnumerator DoujiKonBurst(bool fripY)
    {
        PlayerHealth ph = this.GetComponent<PlayerHealth>();

        //  ���o�[�X�g�J�n���o
        yield return StartCoroutine(StartingKonBurst(ph));

        //  ���o�[�X�g�I�u�W�F�N�g����
        GameObject obj = Instantiate( doujiKonburstPrefab,
                                    this.transform.position,
                                    Quaternion.identity );

        //  Y�𔽓]���邩�ǂ����ݒ肷��
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        sr.flipY = fripY;

        //  5�b�҂�
        yield return new WaitForSeconds(5);

        //  ���o�[�X�g�I�����o
        yield return StartCoroutine(EndingKonBurst(ph));

        Debug.Log("�h�E�W�̍��o�[�X�g���o�I���I");

        yield return null;
    }

    //-------------------------------------------
    //  �c�N���̍��o�[�X�g���o
    //-------------------------------------------
    private IEnumerator TsukumoKonBurst(bool fripY)
    {
        PlayerHealth ph = this.GetComponent<PlayerHealth>();

        //  ���o�[�X�g�J�n���o
        yield return StartCoroutine(StartingKonBurst(ph));

        //  ���o�[�X�g�I�u�W�F�N�g����
        GameObject obj = null;

        //  �З͂�ݒ�

        //  Y�𔽓]���邩�ǂ����ݒ肷��
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        sr.flipY = fripY;

        //  ���o�[�X�g�I�����o
        yield return StartCoroutine(EndingKonBurst(ph));

        Debug.Log("�c�N���̍��o�[�X�g���o�I���I");

        yield return null;
    }

    //-------------------------------------------
    //  �N�`�i���̍��o�[�X�g���o
    //-------------------------------------------
    private IEnumerator KuchinawaKonBurst(bool fripY)
    {
        PlayerHealth ph = this.GetComponent<PlayerHealth>();

        //  ���o�[�X�g�J�n���o
        yield return StartCoroutine(StartingKonBurst(ph));

        //  ���o�[�X�g�I�u�W�F�N�g����
        GameObject obj = null;

        //  �З͂�ݒ�

        //  Y�𔽓]���邩�ǂ����ݒ肷��
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        sr.flipY = fripY;

        //  ���o�[�X�g�I�����o
        yield return StartCoroutine(EndingKonBurst(ph));

        Debug.Log("�N�`�i���̍��o�[�X�g���o�I���I");

        yield return null;
    }

    //-------------------------------------------
    //  �N���}�̍��o�[�X�g���o
    //-------------------------------------------
    private IEnumerator KuramaKonBurst(bool fripY)
    {
         PlayerHealth ph = this.GetComponent<PlayerHealth>();

        //  ���o�[�X�g�J�n���o
        yield return StartCoroutine(StartingKonBurst(ph));

        //  ���o�[�X�g�I�u�W�F�N�g����
        GameObject obj = null;

        //  �З͂�ݒ�

        //  Y�𔽓]���邩�ǂ����ݒ肷��
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        sr.flipY = fripY;

        //  ���o�[�X�g�I�����o
        yield return StartCoroutine(EndingKonBurst(ph));

        Debug.Log("�N���}�̍��o�[�X�g���o�I���I");

        yield return null;
    }

    //-------------------------------------------
    //  ���_�c�~�̍��o�[�X�g���o
    //-------------------------------------------
    private IEnumerator WadatsumiKonBurst(bool fripY)
    {
        PlayerHealth ph = this.GetComponent<PlayerHealth>();

        //  ���o�[�X�g�J�n���o
        yield return StartCoroutine(StartingKonBurst(ph));

        //  ���o�[�X�g�I�u�W�F�N�g����
        GameObject obj = null;

        //  �З͂�ݒ�

        //  Y�𔽓]���邩�ǂ����ݒ肷��
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        sr.flipY = fripY;

        //  ���o�[�X�g�I�����o
        yield return StartCoroutine(EndingKonBurst(ph));

        Debug.Log("���_�c�~�̍��o�[�X�g���o�I���I");

        yield return null;
    }

    //-------------------------------------------
    //  �n�N�����̍��o�[�X�g���o
    //-------------------------------------------
    private IEnumerator HakumenKonBurst(bool fripY)
    {
        PlayerHealth ph = this.GetComponent<PlayerHealth>();

        //  ���o�[�X�g�J�n���o
        yield return StartCoroutine(StartingKonBurst(ph));

        //  ���o�[�X�g�I�u�W�F�N�g����
        GameObject obj = null;

        //  �З͂�ݒ�

        //  Y�𔽓]���邩�ǂ����ݒ肷��
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        sr.flipY = fripY;

        //  ���o�[�X�g�I�����o
        yield return StartCoroutine(EndingKonBurst(ph));

        Debug.Log("�n�N�����̍��o�[�X�g���o�I���I");

        yield return null;
    }

    //------------------------------------------------
    //  �{���A�C�R���̐����X�V����
    //------------------------------------------------
    private void UpdateBombIcon()
    {
        if(bombNum < 0)Debug.LogError("bombNum�Ƀ}�C�i�X�̒l�������Ă��܂��I");

        //  1��S���\��
        for(int i=0;i<bombIconList.Count;i++)
        {
            bombIconList[i].gameObject.SetActive(true);
        }

        //  ��\������
        for(int i=bombIconList.Count-1;i>bombNum-1;i--)
        {
            bombIconList[i].gameObject.SetActive(false);
        }
    }

    //------------------------------------------------
    //  �v���p�e�B
    //------------------------------------------------
    public int GetBombNum(){ return bombNum; }
    public int GetBombMaxNum(){ return bombMaxNum; }
    public void SetCanBomb(bool b){ bCanBomb = b; }
    public bool GetCanBomb(){ return bCanBomb; }
    public float GetBombPower(){ return bombPower; }
    public float GetKonburstShotPower(){ return konburstShotPower[(int)PlayerInfoManager.g_CONVERTSHOT]; }

    //------------------------------------------------
    //  ���o�[�X�g�Q�[�W�𑝂₷
    //------------------------------------------------
    public void PlusKonburstGauge(bool full)
    {
        if(full)konburstSlider.value += konbrstPlusValue;
        else konburstSlider.value += konbrstPlusValue / 2;
            
        if(konburstSlider.value >= 1.0f)
        {   
            konburstSlider.value = 1.0f;

            //  �Q�[�W�̃��C���{�[ON
            koburstGaugeFill.GetComponent<Animator>().enabled = true;

            //  MAX�����v��L���ɂ���
            SetLampActive(true);
        }
    }

    //------------------------------------------------
    //  ���o�[�X�g�Q�[�W�����Z�b�g����
    //------------------------------------------------
    public void ResetKonburstGauge()
    {
        konburstSlider.value = 0.0f;

        //  �Q�[�W�̃��C���{�[OFF
        koburstGaugeFill.GetComponent<Animator>().enabled = false;

        //  �Q�[�W�̉摜�����ɖ߂�
        koburstGaugeFill.GetComponent<Image>().sprite = koburstDefaultSprite;

        SetLampActive(false);
    }

    //------------------------------------------------
    //  MAX�����v��L���E�����ɂ���
    //------------------------------------------------
    public void SetLampActive(bool active)
    {
        if(konburstLamp.activeSelf != active)
        {
            konburstLamp.SetActive(active);
        }
    }
    //------------------------------------------------
    //  �Q�b��ɃR���W����OFF�ɂ���
    //------------------------------------------------
    private IEnumerator ResetCollision()
    {
        yield return new WaitForSeconds(2);

        //  �{���R���W����OFF
        bombCollision.SetActive(false);
    }

}
