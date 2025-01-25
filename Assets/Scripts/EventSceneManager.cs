using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static BillboardButton;


//--------------------------------------------------------------
//
//  �C�x���g�V�[���Ǘ��N���X
//
//--------------------------------------------------------------

//  �C�x���g�̎��
public enum EventType
{
    BeforeBattle,    //  �퓬�O
    AfterBattle,     //  �퓬��

    Max
}

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

    DODOME_NORMAL,      //  �S�X�ځE�ʏ�

    HONEG_NORMAL,       //  ��G�E�ʏ�
    HONEG_EXCITE,       //  ��G�E><
    HONEG_CONFUSE,      //  ��G�E���f

    DOUJI_NORMAL,       //  �h�E�W�E�ʏ�
    DOUJI_ANGRY,        //  �h�E�W�E�{��
    DOUJI_SURPRISED,    //  �h�E�W�E����

    TSUKUMO_NORMAL,     //  �c�N���E�ʏ�
    TSUKUMO_ANGRY,      //  �c�N���E�v�`����
    TSUKUMO_CLOSEEYE,   //  �c�N���E�ڕ�
    TSUKUMO_SMILE,      //  �c�N���E�Í�����


    Max
}


public class EventSceneManager : MonoBehaviour
{
    //  �V���O���g���ȃC���X�^���X
    public static EventSceneManager Instance
    {
        get; private set;
    }

    //  �{�X�̃v���n�u
    [SerializeField,ShowAssetPreview]
    private GameObject BossPrefab;

    //  �e�L�X�g���[�h�p�e�L�X�g�A�Z�b�g
    [SerializeField,EnumIndex(typeof(EventType))]
    TextAsset[] TextFile;

    //  �C�x���g�V�[���p�w�i�I�u�W�F�N�g
    [SerializeField] private GameObject eventCanvas;

    //  �t���[���I�u�W�F�N�g
    [SerializeField,EnumIndex(typeof(Frame))]
    private GameObject[] frameObject;

    //  �t���[������Face�I�u�W�F�N�g
    [SerializeField,EnumIndex(typeof(Frame))]
    private Image[] faceImage;

    //  �e�L�X�g�I�u�W�F�N�g
    [SerializeField,EnumIndex(typeof(Frame))]
    private GameObject[] TextObject;

    //  �{�X�̏�C�I�u�W�F�N�g
    [SerializeField] private GameObject bossFogObjectL;
    [SerializeField] private GameObject bossFogObjectR;

    //  �{�X��O�Ƀ{�X�̖��O��\������e�L�X�g�I�u�W�F�N�g
    [SerializeField] private GameObject bossNameTextObj;

    //  ����
    private InputAction textNext;



    //  ���ۂɃ��[�h����e�L�X�g�A�Z�b�g
    private TextAsset textFile;

    //  �s���Ƃɕۑ����郊�X�g
    List<string> TextData = new List<string>();

    //  ���X�g�ԍ��𑗂�p
    int textNum = 0;

    //  ���b�Z�[�W�e�L�X�g
    string gameText = string.Empty;

    //  �{�X��J�n�t���O
    private bool startBoss;

    //  ���ʕ\���J�n�t���O
    private bool startResult;

    //  �����ւ��p�̊�O���t�B�b�N�v���n�u
    [SerializeField,ShowAssetPreview,EnumIndex(typeof(FaceType))]
    private Sprite[] faceSprite;

    //  �C�x���g�V�[���N���X
    private EventScene currentEventScene;

    //  �{�X�I�u�W�F�N�g�i�[�p
    private GameObject BossObject;

    //  �{�X��i��j�p�̔w�i�I�u�W�F�N�g
    [SerializeField] private GameObject bossBackGroundObj;



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


        //  ��U��\��
        frameObject[(int)Frame.TOP].SetActive(false);
        frameObject[(int)Frame.BOTTOM].SetActive(false);
        bossNameTextObj.SetActive(false);


        // InputAction��Shot��ݒ�
        PlayerInput playerInput = GameManager.Instance.GetPlayer().GetComponent<PlayerInput>();
        textNext = playerInput.actions["TextNext"];


        Debug.Log("***�{�X��J�n�t���O*** " + startBoss);


        //  �e�L�X�g�ǂݍ��݁��s���Ƃɕۑ�
        ReadTextAndCopyByLine();


        //  �e�L�X�g��������
        InitText();


        //  �C�x���g�V�[�������[�h
        LoadEventScene();


        Debug.Log("***�C�x���g���[�h�ɂȂ�܂����B***");
    }

    void Update()
    {
        //eventCanvas.GetComponent<RectTransform>().anchoredPosition =
        //    new Vector2(0,0);

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
    public void SetFrameObjectActive(Frame frame,bool b){ frameObject[(int)frame].SetActive(b); }
    public bool GetFrameObjectActive(Frame frame){ return frameObject[(int)frame].activeSelf; }
    public void SetEventCanvasActive(bool b){ eventCanvas.SetActive(b); }
    public bool GetEventCanvasActive(){ return eventCanvas.activeSelf; }
    public void SetBossNameTextActive(bool b){ bossNameTextObj.SetActive(b); }
    public GameObject GetBossNameTextObj(){ return bossNameTextObj; }
    public void SetBossName(string name){ bossNameTextObj.GetComponent<TextMeshProUGUI>().text = name; }
    public void SetFaceToFrame(Frame frame,FaceType face){ faceImage[(int)frame].sprite = faceSprite[(int)face]; }
    public void SetTextToFrame(Frame frame,string str){ TextObject[(int)frame].GetComponent<TextMeshProUGUI>().text = Regex.Unescape(str); }
    public String GetGameText(){ return gameText; }
    public GameObject GetEventCanvas(){ return eventCanvas; }
    public GameObject GetBossPrefab(){ return BossPrefab; }
    public GameObject GetBossObject(){ return BossObject; }
    public void SetFogObjectActiveL(bool b){ bossFogObjectL.SetActive(b); }
    public GameObject GetFogObjectL(){ return bossFogObjectL; }
    public void SetFogObjectActiveR(bool b){ bossFogObjectR.SetActive(b); }
    public GameObject GetFogObjectR(){ return bossFogObjectR; }
    public bool GetTextNextInput(){ return textNext.WasPressedThisFrame(); }
    public void SetBossBackGroundObj(bool b){ bossBackGroundObj.SetActive(b); }
    public GameObject GetBossBackGroundObj(){ return bossBackGroundObj; }

    //-------------------------------------------------------------
    //  �e�L�X�g�ǂݍ��݁��s���Ƃɕۑ�
    //-------------------------------------------------------------
    public void ReadTextAndCopyByLine()
    {
       //  �ǂݍ��ރe�L�X�g�t�@�C����I��
        if(!startBoss)textFile = TextFile[(int)EventType.BeforeBattle];
        else textFile = TextFile[(int)EventType.AfterBattle];

        StringReader reader = new StringReader(textFile.text);
        TextData.Clear();
        textNum = 0;
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            TextData.Add(line);
        }
    }

    //-------------------------------------------------------------
    //  �e�L�X�g��������
    //-------------------------------------------------------------
    public void InitText()
    {
        gameText = null;
        gameText = TextData[0].ToString();
    }

    //-------------------------------------------------------------
    //  �{�X�v���n�u��Instantiate����
    //-------------------------------------------------------------
    public void InstantiateBossPrefab(BossType type,Vector2 pos)
    {
        BossObject = Instantiate(BossPrefab,pos,Quaternion.identity);
    }

    //-------------------------------------------------------------
    //  EventScene�����[�h����
    //-------------------------------------------------------------
    public void LoadEventScene()
    {
        //  ���݂̃X�e�[�W�����擾
        int stageNumber = (int)PlayerInfoManager.stageInfo;


        //  ���݂̃X�e�[�W���ɂ���ăN���X���Ăѕ�����
        switch (stageNumber)
        {
            case (int)PlayerInfoManager.StageInfo.Stage01:
                //  �{�X�퓬�O���ォ�ŕ�����
                if(!startBoss)currentEventScene = gameObject.AddComponent<EventScene01>();
                else currentEventScene = gameObject.AddComponent<EventScene02>();
                break;
            case (int)PlayerInfoManager.StageInfo.Stage02:
                if(!startBoss)currentEventScene = gameObject.AddComponent<EventScene03>();
                else currentEventScene = gameObject.AddComponent<EventScene04>();
                break;
            case (int)PlayerInfoManager.StageInfo.Stage03:
                //if(!startBoss)currentEventScene = gameObject.AddComponent<EventScene05>();
                //else currentEventScene = gameObject.AddComponent<EventScene06>();
                break;
            case (int)PlayerInfoManager.StageInfo.Stage04:
                //if(!startBoss)currentEventScene = gameObject.AddComponent<EventScene07>();
                //else currentEventScene = gameObject.AddComponent<EventScene08>();
                break;
            case (int)PlayerInfoManager.StageInfo.Stage05:
                //if(!startBoss)currentEventScene = gameObject.AddComponent<EventScene09>();
                //else currentEventScene = gameObject.AddComponent<EventScene10>();
                break;
            case (int)PlayerInfoManager.StageInfo.Stage06:
                //if(!startBoss)currentEventScene = gameObject.AddComponent<EventScene11>();
                //else currentEventScene = gameObject.AddComponent<EventScene12>();
                break;
            default:
                Debug.LogError("Invalid stage number.");
                return;
        }


        //  �C�x���g�����s
        StartCoroutine(currentEventScene.PlayEvent());
    }
}
