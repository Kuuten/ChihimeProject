using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
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
    [SerializeField] GameObject bombFadePrefab;
    [SerializeField] GameObject CanvasObject;
    [SerializeField] GameObject MainCanvasObject;
    [SerializeField] GameObject bombCollision;
    [SerializeField] GameObject konBurstCollision;
    private GameObject FadeObj;
    [SerializeField] BombFade bombFade;

    //  ���o�[�X�g�Q�[�W�̒l�擾�p
    [SerializeField] Slider konBurstSlider;
    //  �h�E�W�̍��o�[�X�g�v���n�u
    [SerializeField] GameObject doujiKonburstPrefab;
    //  ���o�[�X�g�J�b�g�C���摜�v���n�u
    [SerializeField] GameObject[] konburstCutinPrefab;
    //  ���o�[�X�g�̈З�
    private float[] konburstShotPower = new float[(int)SHOT_TYPE.TYPE_MAX];


    //  BOMB�ɕ\�������e�L�X�g
    [SerializeField] private TextMeshProUGUI bombText;
    private int bombNum;
    private const int bombMaxNum = 9;
    private float bombPower = 50f; //  �{��1���̈З�

    InputAction inputBomb;
    bool bCanBomb;      //  �{���������ł��邩�ǂ���

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

        //  ���o�[�g�e���Ƃ̒e�̈З�
        konburstShotPower[(int)SHOT_TYPE.DOUJI]     = 100f;
        konburstShotPower[(int)SHOT_TYPE.TSUKUMO]   = 1f;
        konburstShotPower[(int)SHOT_TYPE.KUCHINAWA] = 5f;
        konburstShotPower[(int)SHOT_TYPE.KURAMA]    = 40f;
        konburstShotPower[(int)SHOT_TYPE.WADATSUMI] = 1f;   //  �n�[�g�񕜗�
        konburstShotPower[(int)SHOT_TYPE.HAKUMEN]   = 10f;
    }

    void Update()
    {
        //  GameManager�����Ԃ��擾
        int gamestatus = GameManager.Instance.GetGameState();

        //  �Q�[���i�K�ʏ���
        switch(gamestatus)
        {
            case (int)eGameState.Zako:
                BombAndKonBurstUpdate(true);    //  �{���E���o�[�X�g�̍X�V
                break;
            case (int)eGameState.Boss:
                BombAndKonBurstUpdate(false);   //  �{���E���o�[�X�g�̍X�V
                break;
            case (int)eGameState.Event:
                break;
        }

        //  �{���̃e�L�X�g���X�V
        bombText.text = $"{bombNum}";
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
        if( konBurstSlider.value >= 1.0f )
        {
            //  ���o�[�X�g�Q�[�W��_��Animator�ɐ؂�ւ�

            if(inputBomb.WasPressedThisFrame()) //  �{���{�^���������ꂽ�I
            {
                //  �{���𔭓��s�\�ɂ���
                bCanBomb = false;

                //  �Q�[�W��0�ɃZ�b�g
                konBurstSlider.value = 0.0f;

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
        GameObject FadeObj = Instantiate( bombFadePrefab );
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

        //  �t�F�[�h�I�u�W�F�N�g���폜
        Destroy( FadeObj );
        FadeObj = null;

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
        FadeObj = Instantiate( bombFadePrefab );
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
            (int)SFXList.SFX_CONCURST_CUTIN);

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

        //  �t�F�[�h�I�u�W�F�N�g���폜
        Destroy( FadeObj );
        FadeObj = null;

        //  ���o�[�X�g�̒e�����p�̓����蔻��I�u�W�F�N�g�𖳌���
        bombCollision.SetActive(false);

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
    //  �v���p�e�B
    //------------------------------------------------
    public int GetBombNum(){ return bombNum; }
    public int GetBombMaxNum(){ return bombMaxNum; }
    public void SetCanBomb(bool b){ bCanBomb = b; }
    public bool GetCanBomb(){ return bCanBomb; }
    public float GetBombPower(){ return bombPower; }
    public float GetKonburstShotPower(){ return konburstShotPower[(int)PlayerInfoManager.g_CONVERTSHOT]; }
}
