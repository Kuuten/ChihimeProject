using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static PlayerInfoManager;

//--------------------------------------------------------------
//
//  �C�x���g�V�[���Ǘ��N���X
//
//--------------------------------------------------------------

//  �{�X��O��ɓ����b�V�[�����Ǘ�����B
//  �����ŃR���[�`����֐����쐬���čĐ�����B

//  �C�x���g�̎��
public enum EventType
{
    Ev01,    //  �C�x���g01�F�h�E�W�퓬�O
    Ev02,    //  �C�x���g02�F�h�E�W�퓬��
    Ev03,    //  �C�x���g03�F�c�N���퓬�O
    Ev04,    //  �C�x���g04�F�c�N���퓬��
    Ev05,    //  �C�x���g05�F�N�`�i���퓬�O
    Ev06,    //  �C�x���g06�F�N�`�i���퓬��
    Ev07,    //  �C�x���g07�F�N���}�퓬�O
    Ev08,    //  �C�x���g08�F�N���}�퓬��
    Ev09,    //  �C�x���g09�F���_�c�~�퓬�O
    Ev10,    //  �C�x���g10�F���_�c�~�퓬��
    Ev11,    //  �C�x���g11�F�n�N�����퓬�O
    Ev12,    //  �C�x���g12�F�n�N�����퓬��

    Max
}

public class EventSceneManager : MonoBehaviour
{
    //  �V���O���g���ȃC���X�^���X
    public static EventSceneManager Instance
    {
        get; private set;
    }

    //  �C�x���g�V�[���p�w�i�I�u�W�F�N�g
    [SerializeField] private  GameObject eventCanvas; 

    //  ����
    private InputAction textNext;

    //  �����ւ��p�̊�O���t�B�b�N�v���n�u
    [SerializeField] private  Sprite[] faceSprite;

    //  �t���[���I�u�W�F�N�g
    [SerializeField] private  GameObject[] frameObject;

    //  ��O���t�B�b�N�I�u�W�F�N�g
    [SerializeField] private  Image[] faceImage;

    //  �e�L�X�g�I�u�W�F�N�g
    [SerializeField] private  GameObject[] TextObject;

    //  �t���[��
    public enum Frame
    {
        TOP,    //  ��
        BOTTOM, //  ��

        Max
    }

    //  �����ւ���O���t�B�b�N�̎��
    public enum FaceType
    {
        CHIHIME_NORMAL,     //  ��P�E�ʏ�
        CHIHIME_EXCITE,     //  ��P�E><�B
        CHIHIME_SURPRISED,  //  ��P�E����Ƃ�

        DOUJI_NORMAL,       //  �h�E�W�E�ʏ�
        DOUJI_ANGRY,        //  �h�E�W�E�{��
        DOUJI_SURPRISED,    //  �h�E�W�E����

        DODOME_NORMAL,      //  �S�X�ځE�ʏ�

        HONEG_NORMAL,       //  ��G�E�ʏ�

        Max
    }

    //  �e�L�X�g���[�h�p�e�L�X�g�A�Z�b�g
    [SerializeField] TextAsset[] TextFile;

    //  ���ۂɃ��[�h����e�L�X�g�A�Z�b�g
    private TextAsset textFile;

    //  �s���Ƃɕۑ����郊�X�g
    List<string> TextData = new List<string>();

    //  ���X�g�ԍ��𑗂�p
    int textNum = 0;

    //  ���b�Z�[�W�e�L�X�g
    string gameText = string.Empty;

    //  �C�x���g�i�[�p
    private IEnumerator[] eventFunc;

    //  �{�X�̃v���n�u
    [SerializeField] private GameObject[] BossPrefab;

    //  �{�X�I�u�W�F�N�g�i�[�p
    private GameObject BossObject;

    //  �{�X��J�n�t���O
    private bool startBoss;

    //  ���ʕ\���J�n�t���O
    private bool startResult;

    //  �{�X�̏�C�I�u�W�F�N�g
    [SerializeField] private GameObject bossFogObjectL;
    [SerializeField] private GameObject bossFogObjectR;

    //  �{�X��O�Ƀ{�X�̖��O��\������e�L�X�g
     [SerializeField] private GameObject bossNameText;

    //  �{�X��i��j�p�̔w�i�I�u�W�F�N�g
    [SerializeField] private GameObject[] bossBackGroundObj;

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
        //  �{�X��J�n�t���O��������
        startBoss = false;
        //  ���ʕ\���J�n�t���O��������
        startResult = false;
    }

    //------------------------------------------------------------
    //  �L�������ꂽ���ɌĂ΂��(Start�����O)
    //------------------------------------------------------------
    void OnEnable()
    {
        //  �w�i�I�u�W�F�N�gON
        eventCanvas.SetActive(true);

        //  ��U�S�Ĕ�\��
        frameObject[(int)Frame.TOP].SetActive(false);
        frameObject[(int)Frame.BOTTOM].SetActive(false);
        bossNameText.SetActive(false);

        // InputAction��Shot��ݒ�
        PlayerInput playerInput = GameManager.Instance.GetPlayer().GetComponent<PlayerInput>();
        textNext = playerInput.actions["TextNext"];

        //  �C�x���g�֐���ݒ�
        eventFunc = new IEnumerator[(int)EventType.Max]
            {
                Event01(),
                Event02(),
                Event01(),
                Event01(),
                Event01(),
                Event01(),
                Event01(),
                Event01(),
                Event01(),
                Event01(),
                Event01(),
                Event01(),
            };


        Debug.Log("***�{�X��J�n�t���O*** " + startBoss);

        //  �ǂݍ��ރe�L�X�g�t�@�C����I��
        if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage01)
        {
            if(!startBoss)textFile = TextFile[(int)EventType.Ev01];
            else textFile = TextFile[(int)EventType.Ev02];
        }
        else  if( PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage02 )
        {
            if(!startBoss)textFile = TextFile[(int)EventType.Ev03];
            else textFile = TextFile[(int)EventType.Ev04];
        }
        else  if( PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage03 )
        {
            if(!startBoss)textFile = TextFile[(int)EventType.Ev04];
            else textFile = TextFile[(int)EventType.Ev05];
        }
        else  if( PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage04 )
        {
            if(!startBoss)textFile = TextFile[(int)EventType.Ev06];
            else textFile = TextFile[(int)EventType.Ev07];
        }
        else  if( PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage05 )
        {
            if(!startBoss)textFile = TextFile[(int)EventType.Ev08];
            else textFile = TextFile[(int)EventType.Ev09];
        }
        else  if( PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage06 )
        {
            if(!startBoss)textFile = TextFile[(int)EventType.Ev10];
            else textFile = TextFile[(int)EventType.Ev11];
        }

        //  �e�L�X�g�ǂݍ��݁��s���Ƃɕۑ�
        StringReader reader = new StringReader(textFile.text);
        TextData.Clear();
        textNum = 0;
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            TextData.Add(line);
        }

        //  �e�L�X�g��������
        gameText = null;
        gameText = TextData[0].ToString();

        //  �C�x���g���J�n
        if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage01)
        {
            if(!startBoss)StartCoroutine(eventFunc[(int)EventType.Ev01]);
            else StartCoroutine(eventFunc[(int)EventType.Ev02]);
        }
        else  if( PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage02 )
        {
            if(!startBoss)StartCoroutine(eventFunc[(int)EventType.Ev03]);
            else StartCoroutine(eventFunc[(int)EventType.Ev04]);
        }
        else  if( PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage03 )
        {
            if(!startBoss)StartCoroutine(eventFunc[(int)EventType.Ev05]);
            else StartCoroutine(eventFunc[(int)EventType.Ev06]);
        }
        else  if( PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage04 )
        {
            if(!startBoss)StartCoroutine(eventFunc[(int)EventType.Ev07]);
            else StartCoroutine(eventFunc[(int)EventType.Ev08]);
        }
        else  if( PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage05 )
        {
            if(!startBoss)StartCoroutine(eventFunc[(int)EventType.Ev09]);
            else StartCoroutine(eventFunc[(int)EventType.Ev10]);
        }
        else  if( PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage06 )
        {
            if(!startBoss)StartCoroutine(eventFunc[(int)EventType.Ev11]);
            else StartCoroutine(eventFunc[(int)EventType.Ev12]);
        }

        Debug.Log("***�C�x���g���[�h�ɂȂ�܂����B***");
    }

    void Update()
    {
        eventCanvas.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(0,0);

        if (TextData[textNum] != "ENDTEXT")
        {
            if (textNext.WasPressedThisFrame())
            {
                textNum++; //�s�����i���j�ɂ���
            }
            gameText = TextData[textNum];
        }
    }

    //-----------------------------------------------------------------
    //  �v���p�e�B
    //-----------------------------------------------------------------
    public bool GetStartBoss(){ return startBoss; }
    public void SetStartBoss(bool b){ startBoss = b; }
    public bool GetStartResult(){ return startResult; }
    public void SetStartResult(bool b){ startResult = b; }
    public GameObject GetBossObject(){ return BossObject; }
    public GameObject GetFogObjectL(){ return bossFogObjectL; }
    public GameObject GetFogObjectR(){ return bossFogObjectR; }

    //-------------------------------------------------------------
    //  ��UI�ƃe�L�X�g��ύX����
    //-------------------------------------------------------------
    private IEnumerator ChangeFaceAndText(Frame frame,FaceType type,string str)
    {
        //  ��UI���v���n�u���w�肵�ĕύX
        faceImage[(int)frame].sprite = faceSprite[(int)type];

        //  �e�L�X�g�Ɉ������w��
        TextObject[(int)frame].GetComponent<TextMeshProUGUI>().text = Regex.Unescape(str);

        yield return null;
    }

    //-------------------------------------------------------------
    //  �{�X�𐶐����Ďw��̍��W�Ɉړ�������
    //-------------------------------------------------------------
    private IEnumerator CreateBossAndMove(BossType type,Vector2 target)
    {
        float duration = 1.0f;

        //  �{�X�𐶐�����
        Vector2 pos = new Vector2(-1,11);   //  �����ʒu
        BossObject = Instantiate(BossPrefab[(int)type],pos,Quaternion.identity);

        //  �{�X���Z�b�g
        EnemyManager.Instance.SetBoss(type, ePowerupItems.PowerUp);

        //  BossDouji�R���|�[�l���g�𖳌���
        BossObject.GetComponent<BossDouji>().enabled = false;
        BossObject.GetComponent<BoxCollider2D>().enabled = false;

        //  �ڕW���W�Ɍ������Ĉړ��J�n
        BossObject.GetComponent<RectTransform>().DOAnchorPos(target,duration);

        //  �R�b�҂�
        yield return new WaitForSeconds(duration);
    }

    //-------------------------------------------------------------
    //  �{�X�̖��O���A���t�@�A�j��������
    //-------------------------------------------------------------
    private IEnumerator AlphaAnimationBossName()
    {
        float duration = 5.0f;  //  �t�F�[�h�C���ɂ����鎞��
        float duration2 = 2.0f; //  �t�F�[�h�A�E�g�ɂ����鎞��

        //  �\��������e��\��
        string douji     = "�����V �h�E�W";
        string tsukumo   = "�����V �c�N��";
        string kuchinawa = "�֖��V �N�`�i��";
        string kurama    = "�����V �N���}";
        string wadatsumi = "�C���V ���_�c�~";
        string hakumen   = "�����V �n�N����";
        string name = "";

        //  ���̃X�e�[�W�ɂ���ă{�X�̖��O��ݒ�
        if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage01)
        {
            name = douji;
        }
        else if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage02)
        {
            name = tsukumo;
        }
        else if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage03)
        {
            name = kuchinawa;
        }
        else if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage04)
        {
            name = kurama;
        }
        else if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage05)
        {
            name = wadatsumi;
        }
        else if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage06)
        {
            name = hakumen;
        }

        //  �e�L�X�g��L����
        bossNameText.SetActive(true);

        //  �{�X�̖��O��ݒ�
        bossNameText.GetComponent<TextMeshProUGUI>().text = name;

        //  �e�L�X�g�̃A���t�@��0�ɂ���
        bossNameText.GetComponent<TextMeshProUGUI>().DOFade(0f,0f);

        yield return null;

        //  ������Ԃ���������o������
        bossNameText.GetComponent<TextMeshProUGUI>().DOFade(1f,duration)
            .OnComplete(()=>
            bossNameText.GetComponent<TextMeshProUGUI>().DOFade(0f, duration2).SetEase(Ease.Linear).SetEase(Ease.Linear))
            .SetEase(Ease.Linear);

        yield return null;
    }

    //-------------------------------------------------------------
    //  �C�x���g01�F�h�E�W�퓬�O
    //-------------------------------------------------------------
    private IEnumerator Event01()
    {
        Debug.Log("***�C�x���g01�F�h�E�W�퓬�O���J�n���܂�***");

        //  �����A�N�e�B�u��
        frameObject[(int)Frame.BOTTOM].SetActive(true);

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.CHIHIME_NORMAL,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        //  �{�X�𐶐����ړ�
        StartCoroutine(CreateBossAndMove(BossType.Douji,new Vector2(-1,5.5f)));

        //  ����A�N�e�B�u��
        frameObject[(int)Frame.TOP].SetActive(true);

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DOUJI_NORMAL,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.CHIHIME_EXCITE,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DOUJI_NORMAL,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.DODOME_NORMAL,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DOUJI_NORMAL,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.DODOME_NORMAL,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DOUJI_NORMAL,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.CHIHIME_EXCITE,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.DOUJI_SURPRISED,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DODOME_NORMAL,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.DOUJI_ANGRY,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DODOME_NORMAL,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.CHIHIME_EXCITE,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DODOME_NORMAL,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.CHIHIME_EXCITE,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DOUJI_NORMAL,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());


        //  �L�����o�X��OFF
        eventCanvas.SetActive(false);

        //  ���E�̏�C�I�u�W�F�N�g��L����
        bossFogObjectL.SetActive(true);
        bossFogObjectR.SetActive(true);

        //  ��p�̔w�i�I�u�W�F�N�g��L����
        bossBackGroundObj[(int)StageInfo.Stage01].SetActive(true);

        //  �{�X�o��SE�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_BOSS_APPEAR);

        //  �{�X�̖��O���A���t�@�A�j��������
        StartCoroutine(AlphaAnimationBossName());

        //  �V�b�҂�
        yield return new WaitForSeconds(7);

        //  �{�X���[�h�ֈڍs
        GameManager.Instance.SetGameState((int)eGameState.Boss);

        //  BossDouji�R���|�[�l���g�𖳌���
        BossObject.GetComponent<BossDouji>().enabled = false;

        Debug.Log("***�{�X�탂�[�h�ɂȂ�܂����B***");


        //  �{�X��J�n�t���OTRUE
        startBoss = true;

        //  �C�x���g�V�[���}�l�[�W���[�𖳌���
        this.gameObject.SetActive(false);
    }


    //-------------------------------------------------------------
    //  �C�x���g02�F�h�E�W�퓬��
    //-------------------------------------------------------------
    private IEnumerator Event02()
    {
        Debug.Log("***�C�x���g02�F�h�E�W�퓬����J�n���܂�***");

        //  �L�����o�X��ON
        eventCanvas.SetActive(true);

        //  ����A�N�e�B�u��
        frameObject[(int)Frame.TOP].SetActive(true);

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DOUJI_SURPRISED,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        //  ����A�N�e�B�u��
        frameObject[(int)Frame.BOTTOM].SetActive(true);

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.CHIHIME_NORMAL,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DODOME_NORMAL,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.CHIHIME_EXCITE,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DODOME_NORMAL,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.CHIHIME_SURPRISED,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());


        yield return ChangeFaceAndText(Frame.TOP,FaceType.DODOME_NORMAL,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.CHIHIME_EXCITE,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DOUJI_ANGRY,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DOUJI_ANGRY,gameText);

        //  ���͑҂�
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        //  �t���[���I�u�W�F�N�g���\���ɂ���
        frameObject[(int)Frame.TOP].SetActive(false);
        frameObject[(int)Frame.BOTTOM].SetActive(false);

        //  Pauser���t�����I�u�W�F�N�g���|�[�Y
        Pauser.Pause();

        //  �v���C���[��Animator�𖳌���
        GameManager.Instance.GetPlayer().GetComponent<Animator>().enabled = false;

        //  ���ʕ\���J�n�t���OTRUE
        startResult = true;

        Debug.Log("***���ʕ\�����[�h�ɂȂ�܂����B***");

        //*********���ʕ\����***********
    }
}
