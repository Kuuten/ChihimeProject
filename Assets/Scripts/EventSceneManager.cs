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
//  イベントシーン管理クラス
//
//--------------------------------------------------------------

//  イベントの種類
public enum EventType
{
    BeforeBattle,    //  戦闘前
    AfterBattle,     //  戦闘後

    Max
}

//  フレーム
public enum Frame
{
    TOP,    //  上
    BOTTOM, //  下

    Max
}

//  差し替え顔グラフィックの種類
public enum FaceType
{
    CHIHIME_NORMAL,     //  千姫・通常
    CHIHIME_EXCITE,     //  千姫・><。
    CHIHIME_SURPRISED,  //  千姫・きょとん

    DODOME_NORMAL,      //  百々目・通常

    HONEG_NORMAL,       //  骨G・通常
    HONEG_EXCITE,       //  骨G・><
    HONEG_CONFUSE,      //  骨G・困惑

    DOUJI_NORMAL,       //  ドウジ・通常
    DOUJI_ANGRY,        //  ドウジ・怒り
    DOUJI_SURPRISED,    //  ドウジ・驚き

    TSUKUMO_NORMAL,     //  ツクモ・通常
    TSUKUMO_ANGRY,      //  ツクモ・プチおこ
    TSUKUMO_CLOSEEYE,   //  ツクモ・目閉じ
    TSUKUMO_SMILE,      //  ツクモ・暗黒微笑


    Max
}


public class EventSceneManager : MonoBehaviour
{
    //  シングルトンなインスタンス
    public static EventSceneManager Instance
    {
        get; private set;
    }

    //  ボスのプレハブ
    [SerializeField,ShowAssetPreview]
    private GameObject BossPrefab;

    //  テキストロード用テキストアセット
    [SerializeField,EnumIndex(typeof(EventType))]
    TextAsset[] TextFile;

    //  イベントシーン用背景オブジェクト
    [SerializeField] private GameObject eventCanvas;

    //  フレームオブジェクト
    [SerializeField,EnumIndex(typeof(Frame))]
    private GameObject[] frameObject;

    //  フレーム内のFaceオブジェクト
    [SerializeField,EnumIndex(typeof(Frame))]
    private Image[] faceImage;

    //  テキストオブジェクト
    [SerializeField,EnumIndex(typeof(Frame))]
    private GameObject[] TextObject;

    //  ボスの障気オブジェクト
    [SerializeField] private GameObject bossFogObjectL;
    [SerializeField] private GameObject bossFogObjectR;

    //  ボス戦前にボスの名前を表示するテキストオブジェクト
    [SerializeField] private GameObject bossNameTextObj;

    //  入力
    private InputAction textNext;



    //  実際にロードするテキストアセット
    private TextAsset textFile;

    //  行ごとに保存するリスト
    List<string> TextData = new List<string>();

    //  リスト番号を送り用
    int textNum = 0;

    //  メッセージテキスト
    string gameText = string.Empty;

    //  ボス戦開始フラグ
    private bool startBoss;

    //  結果表示開始フラグ
    private bool startResult;

    //  差し替え用の顔グラフィックプレハブ
    [SerializeField,ShowAssetPreview,EnumIndex(typeof(FaceType))]
    private Sprite[] faceSprite;

    //  イベントシーンクラス
    private EventScene currentEventScene;

    //  ボスオブジェクト格納用
    private GameObject BossObject;

    //  ボス戦（夜）用の背景オブジェクト
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
        //  ボス戦開始フラグを初期化
        startBoss = false;
        //  結果表示開始フラグを初期化
        startResult = false;
    }

    //------------------------------------------------------------
    //  有効化された時に呼ばれる(Startよりも前)
    //------------------------------------------------------------
    void OnEnable()
    {
        //  背景オブジェクトON
        eventCanvas.SetActive(true);


        //  一旦非表示
        frameObject[(int)Frame.TOP].SetActive(false);
        frameObject[(int)Frame.BOTTOM].SetActive(false);
        bossNameTextObj.SetActive(false);


        // InputActionにShotを設定
        PlayerInput playerInput = GameManager.Instance.GetPlayer().GetComponent<PlayerInput>();
        textNext = playerInput.actions["TextNext"];


        Debug.Log("***ボス戦開始フラグ*** " + startBoss);


        //  テキスト読み込み＆行ごとに保存
        ReadTextAndCopyByLine();


        //  テキストを初期化
        InitText();


        //  イベントシーンをロード
        LoadEventScene();


        Debug.Log("***イベントモードになりました。***");
    }

    void Update()
    {
        //eventCanvas.GetComponent<RectTransform>().anchoredPosition =
        //    new Vector2(0,0);

        if (TextData[textNum] != "ENDTEXT")
        {
            if (textNext.WasPressedThisFrame())
            {
                textNum++; //行を下（次）にする
            }
            gameText = TextData[textNum];
        }
    }

    //-----------------------------------------------------------------
    //  プロパティ
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
    //  テキスト読み込み＆行ごとに保存
    //-------------------------------------------------------------
    public void ReadTextAndCopyByLine()
    {
       //  読み込むテキストファイルを選択
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
    //  テキストを初期化
    //-------------------------------------------------------------
    public void InitText()
    {
        gameText = null;
        gameText = TextData[0].ToString();
    }

    //-------------------------------------------------------------
    //  ボスプレハブをInstantiateする
    //-------------------------------------------------------------
    public void InstantiateBossPrefab(BossType type,Vector2 pos)
    {
        BossObject = Instantiate(BossPrefab,pos,Quaternion.identity);
    }

    //-------------------------------------------------------------
    //  EventSceneをロードする
    //-------------------------------------------------------------
    public void LoadEventScene()
    {
        //  現在のステージ数を取得
        int stageNumber = (int)PlayerInfoManager.stageInfo;


        //  現在のステージ数によってクラスを呼び分ける
        switch (stageNumber)
        {
            case (int)PlayerInfoManager.StageInfo.Stage01:
                //  ボス戦闘前か後かで分ける
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


        //  イベントを実行
        StartCoroutine(currentEventScene.PlayEvent());
    }
}
