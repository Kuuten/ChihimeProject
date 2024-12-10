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
//  イベントシーン管理クラス
//
//--------------------------------------------------------------

//  ボス戦前後に入る会話シーンを管理する。
//  ここでコルーチンや関数を作成して再生する。

//  イベントの種類
public enum EventType
{
    Ev01,    //  イベント01：ドウジ戦闘前
    Ev02,    //  イベント02：ドウジ戦闘後
    Ev03,    //  イベント03：ツクモ戦闘前
    Ev04,    //  イベント04：ツクモ戦闘後
    Ev05,    //  イベント05：クチナワ戦闘前
    Ev06,    //  イベント06：クチナワ戦闘後
    Ev07,    //  イベント07：クラマ戦闘前
    Ev08,    //  イベント08：クラマ戦闘後
    Ev09,    //  イベント09：ワダツミ戦闘前
    Ev10,    //  イベント10：ワダツミ戦闘後
    Ev11,    //  イベント11：ハクメン戦闘前
    Ev12,    //  イベント12：ハクメン戦闘後

    Max
}

public class EventSceneManager : MonoBehaviour
{
    //  シングルトンなインスタンス
    public static EventSceneManager Instance
    {
        get; private set;
    }

    //  イベントシーン用背景オブジェクト
    [SerializeField] private  GameObject eventCanvas; 

    //  入力
    private InputAction textNext;

    //  差し替え用の顔グラフィックプレハブ
    [SerializeField] private  Sprite[] faceSprite;

    //  フレームオブジェクト
    [SerializeField] private  GameObject[] frameObject;

    //  顔グラフィックオブジェクト
    [SerializeField] private  Image[] faceImage;

    //  テキストオブジェクト
    [SerializeField] private  GameObject[] TextObject;

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

        DOUJI_NORMAL,       //  ドウジ・通常
        DOUJI_ANGRY,        //  ドウジ・怒り
        DOUJI_SURPRISED,    //  ドウジ・驚き

        DODOME_NORMAL,      //  百々目・通常

        HONEG_NORMAL,       //  骨G・通常

        Max
    }

    //  テキストロード用テキストアセット
    [SerializeField] TextAsset[] TextFile;

    //  実際にロードするテキストアセット
    private TextAsset textFile;

    //  行ごとに保存するリスト
    List<string> TextData = new List<string>();

    //  リスト番号を送り用
    int textNum = 0;

    //  メッセージテキスト
    string gameText = string.Empty;

    //  イベント格納用
    private IEnumerator[] eventFunc;

    //  ボスのプレハブ
    [SerializeField] private GameObject[] BossPrefab;

    //  ボスオブジェクト格納用
    private GameObject BossObject;

    //  ボス戦開始フラグ
    private bool startBoss;

    //  結果表示開始フラグ
    private bool startResult;

    //  ボスの障気オブジェクト
    [SerializeField] private GameObject bossFogObjectL;
    [SerializeField] private GameObject bossFogObjectR;

    //  ボス戦前にボスの名前を表示するテキスト
     [SerializeField] private GameObject bossNameText;

    //  ボス戦（夜）用の背景オブジェクト
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

        //  一旦全て非表示
        frameObject[(int)Frame.TOP].SetActive(false);
        frameObject[(int)Frame.BOTTOM].SetActive(false);
        bossNameText.SetActive(false);

        // InputActionにShotを設定
        PlayerInput playerInput = GameManager.Instance.GetPlayer().GetComponent<PlayerInput>();
        textNext = playerInput.actions["TextNext"];

        //  イベント関数を設定
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


        Debug.Log("***ボス戦開始フラグ*** " + startBoss);

        //  読み込むテキストファイルを選択
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

        //  テキスト読み込み＆行ごとに保存
        StringReader reader = new StringReader(textFile.text);
        TextData.Clear();
        textNum = 0;
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            TextData.Add(line);
        }

        //  テキストを初期化
        gameText = null;
        gameText = TextData[0].ToString();

        //  イベントを開始
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

        Debug.Log("***イベントモードになりました。***");
    }

    void Update()
    {
        eventCanvas.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(0,0);

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
    public GameObject GetBossObject(){ return BossObject; }
    public GameObject GetFogObjectL(){ return bossFogObjectL; }
    public GameObject GetFogObjectR(){ return bossFogObjectR; }

    //-------------------------------------------------------------
    //  顔UIとテキストを変更する
    //-------------------------------------------------------------
    private IEnumerator ChangeFaceAndText(Frame frame,FaceType type,string str)
    {
        //  顔UIをプレハブを指定して変更
        faceImage[(int)frame].sprite = faceSprite[(int)type];

        //  テキストに引数を指定
        TextObject[(int)frame].GetComponent<TextMeshProUGUI>().text = Regex.Unescape(str);

        yield return null;
    }

    //-------------------------------------------------------------
    //  ボスを生成して指定の座標に移動させる
    //-------------------------------------------------------------
    private IEnumerator CreateBossAndMove(BossType type,Vector2 target)
    {
        float duration = 1.0f;

        //  ボスを生成する
        Vector2 pos = new Vector2(-1,11);   //  初期位置
        BossObject = Instantiate(BossPrefab[(int)type],pos,Quaternion.identity);

        //  ボス情報セット
        EnemyManager.Instance.SetBoss(type, ePowerupItems.PowerUp);

        //  BossDoujiコンポーネントを無効化
        BossObject.GetComponent<BossDouji>().enabled = false;
        BossObject.GetComponent<BoxCollider2D>().enabled = false;

        //  目標座標に向かって移動開始
        BossObject.GetComponent<RectTransform>().DOAnchorPos(target,duration);

        //  ３秒待つ
        yield return new WaitForSeconds(duration);
    }

    //-------------------------------------------------------------
    //  ボスの名前をアルファアニメさせる
    //-------------------------------------------------------------
    private IEnumerator AlphaAnimationBossName()
    {
        float duration = 5.0f;  //  フェードインにかかる時間
        float duration2 = 2.0f; //  フェードアウトにかかる時間

        //  表示する内容を表示
        string douji     = "剛魔天 ドウジ";
        string tsukumo   = "操魔天 ツクモ";
        string kuchinawa = "輪魔天 クチナワ";
        string kurama    = "楓魔天 クラマ";
        string wadatsumi = "海魔天 ワダツミ";
        string hakumen   = "焔魔天 ハクメン";
        string name = "";

        //  今のステージによってボスの名前を設定
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

        //  テキストを有効化
        bossNameText.SetActive(true);

        //  ボスの名前を設定
        bossNameText.GetComponent<TextMeshProUGUI>().text = name;

        //  テキストのアルファを0にする
        bossNameText.GetComponent<TextMeshProUGUI>().DOFade(0f,0f);

        yield return null;

        //  透明状態からゆっくり出現する
        bossNameText.GetComponent<TextMeshProUGUI>().DOFade(1f,duration)
            .OnComplete(()=>
            bossNameText.GetComponent<TextMeshProUGUI>().DOFade(0f, duration2).SetEase(Ease.Linear).SetEase(Ease.Linear))
            .SetEase(Ease.Linear);

        yield return null;
    }

    //-------------------------------------------------------------
    //  イベント01：ドウジ戦闘前
    //-------------------------------------------------------------
    private IEnumerator Event01()
    {
        Debug.Log("***イベント01：ドウジ戦闘前を開始します***");

        //  下をアクティブ化
        frameObject[(int)Frame.BOTTOM].SetActive(true);

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.CHIHIME_NORMAL,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        //  ボスを生成＆移動
        StartCoroutine(CreateBossAndMove(BossType.Douji,new Vector2(-1,5.5f)));

        //  上をアクティブ化
        frameObject[(int)Frame.TOP].SetActive(true);

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DOUJI_NORMAL,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.CHIHIME_EXCITE,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DOUJI_NORMAL,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.DODOME_NORMAL,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DOUJI_NORMAL,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.DODOME_NORMAL,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DOUJI_NORMAL,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.CHIHIME_EXCITE,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.DOUJI_SURPRISED,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DODOME_NORMAL,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.DOUJI_ANGRY,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DODOME_NORMAL,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.CHIHIME_EXCITE,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DODOME_NORMAL,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.CHIHIME_EXCITE,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DOUJI_NORMAL,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());


        //  キャンバスをOFF
        eventCanvas.SetActive(false);

        //  左右の障気オブジェクトを有効化
        bossFogObjectL.SetActive(true);
        bossFogObjectR.SetActive(true);

        //  夜用の背景オブジェクトを有効化
        bossBackGroundObj[(int)StageInfo.Stage01].SetActive(true);

        //  ボス登場SE再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_BOSS_APPEAR);

        //  ボスの名前をアルファアニメさせる
        StartCoroutine(AlphaAnimationBossName());

        //  ７秒待つ
        yield return new WaitForSeconds(7);

        //  ボスモードへ移行
        GameManager.Instance.SetGameState((int)eGameState.Boss);

        //  BossDoujiコンポーネントを無効化
        BossObject.GetComponent<BossDouji>().enabled = false;

        Debug.Log("***ボス戦モードになりました。***");


        //  ボス戦開始フラグTRUE
        startBoss = true;

        //  イベントシーンマネージャーを無効化
        this.gameObject.SetActive(false);
    }


    //-------------------------------------------------------------
    //  イベント02：ドウジ戦闘後
    //-------------------------------------------------------------
    private IEnumerator Event02()
    {
        Debug.Log("***イベント02：ドウジ戦闘後を開始します***");

        //  キャンバスをON
        eventCanvas.SetActive(true);

        //  上をアクティブ化
        frameObject[(int)Frame.TOP].SetActive(true);

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DOUJI_SURPRISED,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        //  上をアクティブ化
        frameObject[(int)Frame.BOTTOM].SetActive(true);

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.CHIHIME_NORMAL,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DODOME_NORMAL,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.CHIHIME_EXCITE,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DODOME_NORMAL,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.CHIHIME_SURPRISED,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());


        yield return ChangeFaceAndText(Frame.TOP,FaceType.DODOME_NORMAL,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.BOTTOM,FaceType.CHIHIME_EXCITE,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DOUJI_ANGRY,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        yield return ChangeFaceAndText(Frame.TOP,FaceType.DOUJI_ANGRY,gameText);

        //  入力待ち
        yield return new WaitUntil(() => textNext.WasPressedThisFrame());

        //  フレームオブジェクトを非表示にする
        frameObject[(int)Frame.TOP].SetActive(false);
        frameObject[(int)Frame.BOTTOM].SetActive(false);

        //  Pauserが付いたオブジェクトをポーズ
        Pauser.Pause();

        //  プレイヤーのAnimatorを無効化
        GameManager.Instance.GetPlayer().GetComponent<Animator>().enabled = false;

        //  結果表示開始フラグTRUE
        startResult = true;

        Debug.Log("***結果表示モードになりました。***");

        //*********結果表示へ***********
    }
}
