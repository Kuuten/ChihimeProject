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
//  プレイヤーのボム・魂バースト管理クラス
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

    //  シングルトンなインスタンス
    public static PlayerBombManager Instance
    {
        get; private set;
    }

    //  ドウジの魂バーストプレハブ
    [SerializeField] GameObject doujiKonburstPrefab;
    //  魂バーストカットイン画像プレハブ
    [SerializeField] GameObject[] konburstCutinPrefab;
    //  魂バーストの威力
    private float[] konburstShotPower = new float[(int)SHOT_TYPE.TYPE_MAX];
    //  魂バーストゲージのスライダー
    [SerializeField] Slider konburstSlider;
    //  魂バーストゲージMAX字のランプ
    [SerializeField] GameObject konburstLamp;
    //  魂バーストゲージの1回あたりの増加量
    private const float konbrstPlusValue = 0.1f;    //  暫定値
    //  魂バーストゲージのFillオブジェクト
    [SerializeField] GameObject koburstGaugeFill;
    //  魂バーストゲージのFillのデフォルト画像
    [SerializeField] Sprite koburstDefaultSprite;

    //  ボムアイコンの親オブジェクトの位置取得用
    [SerializeField] private GameObject bombIconRootObj;
    //  ボムアイコンのプレハブ
    [SerializeField] private GameObject bombIconPrefab;
    //  ボムアイコンオブジェクトのリスト
    private List<GameObject> bombIconList = new List<GameObject>();
    private int bombNum;
    private const int bombMaxNum = 5;
    private float bombPower = 20f; //  ボム1発の威力

    InputAction inputBomb;
    bool bCanBomb;      //  ボムが発動できるかどうか

    //  ザコ戦終了後のボムコリジョンフラグ
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
        // InputActionにMoveを設定
        PlayerInput playerInput = GetComponent<PlayerInput>();
        inputBomb = playerInput.actions["Bomb"];

        //  PlayerInfoManagerから個数をセット
        bombNum = PlayerInfoManager.g_BOMBNUM;

        //  最初はnull
        FadeObj = null;

        //  最初は発動できる
        bCanBomb = true;

        //  フラグOFF
        isCalledOnce =false;

        //  最初はレインボーOFF
        koburstGaugeFill.GetComponent<Animator>().enabled = false;

        //  魂バーストごとの弾の威力
        konburstShotPower[(int)SHOT_TYPE.DOUJI]     = 100f;
        konburstShotPower[(int)SHOT_TYPE.TSUKUMO]   = 1f;
        konburstShotPower[(int)SHOT_TYPE.KUCHINAWA] = 5f;
        konburstShotPower[(int)SHOT_TYPE.KURAMA]    = 40f;
        konburstShotPower[(int)SHOT_TYPE.WADATSUMI] = 1f;   //  ハート回復量
        konburstShotPower[(int)SHOT_TYPE.HAKUMEN]   = 10f;

        //  親オブジェクトの子オブジェクトとしてボムアイコンを生成
        for( int i=0; i<bombMaxNum;i++ )
        {
            GameObject obj = Instantiate(bombIconPrefab);
            obj.GetComponent<RectTransform>().SetParent( bombIconRootObj.transform);
            obj.GetComponent<RectTransform>().localScale = Vector3.one;
            obj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0,0,0);

            bombIconList.Add( obj );   //  リストに追加
        }
    }

    void Update()
    {
        //  ボムのアイコンを更新
        UpdateBombIcon();

        //  GameManagerから状態を取得
        int gamestatus = GameManager.Instance.GetGameState();

        //  ゲーム段階別処理
        switch(gamestatus)
        {
            case (int)eGameState.Zako:
                inputBomb.Enable();
                BombAndKonBurstUpdate(true);    //  ボム・魂バーストの更新
                break;
            case (int)eGameState.Boss:
                inputBomb.Enable();
                BombAndKonBurstUpdate(false);   //  ボム・魂バーストの更新
                break;
            case (int)eGameState.Event:
                inputBomb.Disable();

                //  1回ボムコリジョンをONにして弾を消す
                if (!isCalledOnce) {
                    isCalledOnce = true;
                    //  ボムコリジョンON！
                    bombCollision.SetActive(true);

                    //  ２秒後にコリジョンをリセット
                    StartCoroutine(ResetCollision());
                }
                break;
        }
    }

    //  ボムを加算
    public void AddBomb()
    {
        if(bombNum < bombMaxNum)
        {
            bombNum++;
        }
        else bombNum = bombMaxNum;
    }

    //  ボムを減算
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
    //  ボム・魂バーストを発動する
    //-------------------------------------------
    private void BombAndKonBurstUpdate(bool fripY)
    {
        //  魂バーストゲージがMAXだったら
        if( konburstSlider.value >= 1.0f )
        {
            //  魂バーストゲージを点滅Animatorに切り替え

            if(inputBomb.WasPressedThisFrame()) //  ボムボタンが押された！
            {
                //  ボムを発動不可能にする
                bCanBomb = false;

                //  魂バーストゲージをリセット&ランプOFF
                ResetKonburstGauge();

                //  魂バーストゲージを通常Animatorに切り替え

                //  魂バースト演出を開始する
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
        else // ボムが魂バーストではない時
        {
            if( bCanBomb )  //  ボム発動が可能な時
            {
                if(inputBomb.WasPressedThisFrame()) //  ボムボタンが押された！
                {
                    bCanBomb = false;

                    Debug.Log("ボム演出開始！");

                    //  ボムを1個減らす
                    SubBomb();

                    //  ボム演出を開始する
                    StartCoroutine( BombAnimation() );
                }
            }
            else // 発動中かボムの残弾がない時
            {
                if( inputBomb.WasPressedThisFrame() ) //  ボムボタンが押された！
                {
                    //  何もしない
                }
            }
        }

        //  ボムが0なら発動できない
        if(bombNum <= 0)bCanBomb = false; 
    }

    //-------------------------------------------
    //  ボム演出
    //-------------------------------------------
    private IEnumerator BombAnimation()
    {
        //  ボムの当たり判定オブジェクトを有効化
        bombCollision.SetActive(true);

        //  プレイヤーを無敵にする
        PlayerHealth ph = this.GetComponent<PlayerHealth>();
        ph.SetSuperMode(true);

        //  弾を全て消す用のフェード板を生成
        GameObject FadeObj = bombFadeObject;
        FadeObj.gameObject.transform.SetParent( CanvasObject.transform );
        FadeObj.gameObject.transform.SetAsFirstSibling();   //  ヒエラルキーの一番上に
        FadeObj.GetComponent<RectTransform>().transform.localPosition = Vector3.zero;

        //  小さい光がキンッ！って光る

        //  0.3秒待つ
        yield return new WaitForSeconds(0.3f);

        //  小さい光オブジェクト削除

        //  画面がホワイトでフェードアウトする
        BombFade bombFade = FadeObj.GetComponent<BombFade>();
        yield return StartCoroutine(bombFade.StartFadeOut(FadeObj, 0.2f));

        //  0.2秒待つ
        yield return new WaitForSeconds(0.2f);

        //  爆発アニメオブジェクト再生

        //  再生終了を待つ
        yield return new WaitForSeconds(1.5f);

        //  爆発アニメオブジェクト削除

        //  画面がホワイトでフェードインする
        yield return StartCoroutine(bombFade.StartFadeIn(FadeObj, 0.2f));

        //  0.2秒待つ
        yield return new WaitForSeconds(0.2f);

        //  ボムの当たり判定オブジェクトを無効化
        bombCollision.SetActive(false);

        //  プレイヤーの無敵を解除
        ph.SetSuperMode(false);

        //  ボム発動可能に戻す
        bCanBomb = true;


        Debug.Log("ボム演出終了！");

        yield return null;
    }

    //-------------------------------------------
    //  魂バースト開始演出
    //-------------------------------------------
    private IEnumerator StartingKonBurst(PlayerHealth ph)
    {
        //  魂バーストの弾消し用の当たり判定オブジェクトを有効化
        konBurstCollision.SetActive(true);

        //  プレイヤーを無敵にする
        ph = this.GetComponent<PlayerHealth>();
        ph.SetSuperMode(true);

        //  弾を全て消す用のフェード板を生成
        FadeObj = bombFadeObject;
        FadeObj.gameObject.transform.SetParent( CanvasObject.transform );
        FadeObj.gameObject.transform.SetAsFirstSibling();   //  ヒエラルキーの一番上に
        FadeObj.GetComponent<RectTransform>().transform.localPosition = Vector3.zero;

        //  カットイン再生
        GameObject obj =
            Instantiate(konburstCutinPrefab[(int)PlayerInfoManager.g_CONVERTSHOT]);
        obj.transform.SetParent( MainCanvasObject.transform, false );

        //  カットインSE再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_BOMB,
            (int)SFXList.SFX_KONBURST_CUTIN);

        //  画面がホワイトでフェードアウトする
        bombFade = FadeObj.GetComponent<BombFade>();
        yield return StartCoroutine(bombFade.StartFadeOut(FadeObj, 0.2f));
    }

    //-------------------------------------------
    //  魂バースト終了演出
    //-------------------------------------------
    private IEnumerator EndingKonBurst(PlayerHealth ph)
    {
        //  画面がホワイトでフェードインする
        yield return StartCoroutine(bombFade.StartFadeIn(FadeObj, 0.2f));

         //  魂バーストの弾消し用の当たり判定オブジェクトを有効化
        konBurstCollision.SetActive(false);

        //  プレイヤーの無敵を解除
        ph.SetSuperMode(false);

        //  ボム発動可能に戻す
        bCanBomb = true;
    }

    //-------------------------------------------
    //  ドウジの魂バースト演出
    //-------------------------------------------
    private IEnumerator DoujiKonBurst(bool fripY)
    {
        PlayerHealth ph = this.GetComponent<PlayerHealth>();

        //  魂バースト開始演出
        yield return StartCoroutine(StartingKonBurst(ph));

        //  魂バーストオブジェクト生成
        GameObject obj = Instantiate( doujiKonburstPrefab,
                                    this.transform.position,
                                    Quaternion.identity );

        //  Yを反転するかどうか設定する
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        sr.flipY = fripY;

        //  5秒待つ
        yield return new WaitForSeconds(5);

        //  魂バースト終了演出
        yield return StartCoroutine(EndingKonBurst(ph));

        Debug.Log("ドウジの魂バースト演出終了！");

        yield return null;
    }

    //-------------------------------------------
    //  ツクモの魂バースト演出
    //-------------------------------------------
    private IEnumerator TsukumoKonBurst(bool fripY)
    {
        PlayerHealth ph = this.GetComponent<PlayerHealth>();

        //  魂バースト開始演出
        yield return StartCoroutine(StartingKonBurst(ph));

        //  魂バーストオブジェクト生成
        GameObject obj = null;

        //  威力を設定

        //  Yを反転するかどうか設定する
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        sr.flipY = fripY;

        //  魂バースト終了演出
        yield return StartCoroutine(EndingKonBurst(ph));

        Debug.Log("ツクモの魂バースト演出終了！");

        yield return null;
    }

    //-------------------------------------------
    //  クチナワの魂バースト演出
    //-------------------------------------------
    private IEnumerator KuchinawaKonBurst(bool fripY)
    {
        PlayerHealth ph = this.GetComponent<PlayerHealth>();

        //  魂バースト開始演出
        yield return StartCoroutine(StartingKonBurst(ph));

        //  魂バーストオブジェクト生成
        GameObject obj = null;

        //  威力を設定

        //  Yを反転するかどうか設定する
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        sr.flipY = fripY;

        //  魂バースト終了演出
        yield return StartCoroutine(EndingKonBurst(ph));

        Debug.Log("クチナワの魂バースト演出終了！");

        yield return null;
    }

    //-------------------------------------------
    //  クラマの魂バースト演出
    //-------------------------------------------
    private IEnumerator KuramaKonBurst(bool fripY)
    {
         PlayerHealth ph = this.GetComponent<PlayerHealth>();

        //  魂バースト開始演出
        yield return StartCoroutine(StartingKonBurst(ph));

        //  魂バーストオブジェクト生成
        GameObject obj = null;

        //  威力を設定

        //  Yを反転するかどうか設定する
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        sr.flipY = fripY;

        //  魂バースト終了演出
        yield return StartCoroutine(EndingKonBurst(ph));

        Debug.Log("クラマの魂バースト演出終了！");

        yield return null;
    }

    //-------------------------------------------
    //  ワダツミの魂バースト演出
    //-------------------------------------------
    private IEnumerator WadatsumiKonBurst(bool fripY)
    {
        PlayerHealth ph = this.GetComponent<PlayerHealth>();

        //  魂バースト開始演出
        yield return StartCoroutine(StartingKonBurst(ph));

        //  魂バーストオブジェクト生成
        GameObject obj = null;

        //  威力を設定

        //  Yを反転するかどうか設定する
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        sr.flipY = fripY;

        //  魂バースト終了演出
        yield return StartCoroutine(EndingKonBurst(ph));

        Debug.Log("ワダツミの魂バースト演出終了！");

        yield return null;
    }

    //-------------------------------------------
    //  ハクメンの魂バースト演出
    //-------------------------------------------
    private IEnumerator HakumenKonBurst(bool fripY)
    {
        PlayerHealth ph = this.GetComponent<PlayerHealth>();

        //  魂バースト開始演出
        yield return StartCoroutine(StartingKonBurst(ph));

        //  魂バーストオブジェクト生成
        GameObject obj = null;

        //  威力を設定

        //  Yを反転するかどうか設定する
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        sr.flipY = fripY;

        //  魂バースト終了演出
        yield return StartCoroutine(EndingKonBurst(ph));

        Debug.Log("ハクメンの魂バースト演出終了！");

        yield return null;
    }

    //------------------------------------------------
    //  ボムアイコンの数を更新する
    //------------------------------------------------
    private void UpdateBombIcon()
    {
        if(bombNum < 0)Debug.LogError("bombNumにマイナスの値が入っています！");

        //  1回全部表示
        for(int i=0;i<bombIconList.Count;i++)
        {
            bombIconList[i].gameObject.SetActive(true);
        }

        //  非表示処理
        for(int i=bombIconList.Count-1;i>bombNum-1;i--)
        {
            bombIconList[i].gameObject.SetActive(false);
        }
    }

    //------------------------------------------------
    //  プロパティ
    //------------------------------------------------
    public int GetBombNum(){ return bombNum; }
    public int GetBombMaxNum(){ return bombMaxNum; }
    public void SetCanBomb(bool b){ bCanBomb = b; }
    public bool GetCanBomb(){ return bCanBomb; }
    public float GetBombPower(){ return bombPower; }
    public float GetKonburstShotPower(){ return konburstShotPower[(int)PlayerInfoManager.g_CONVERTSHOT]; }

    //------------------------------------------------
    //  魂バーストゲージを増やす
    //------------------------------------------------
    public void PlusKonburstGauge(bool full)
    {
        if(full)konburstSlider.value += konbrstPlusValue;
        else konburstSlider.value += konbrstPlusValue / 2;
            
        if(konburstSlider.value >= 1.0f)
        {   
            konburstSlider.value = 1.0f;

            //  ゲージのレインボーON
            koburstGaugeFill.GetComponent<Animator>().enabled = true;

            //  MAXランプを有効にする
            SetLampActive(true);
        }
    }

    //------------------------------------------------
    //  魂バーストゲージをリセットする
    //------------------------------------------------
    public void ResetKonburstGauge()
    {
        konburstSlider.value = 0.0f;

        //  ゲージのレインボーOFF
        koburstGaugeFill.GetComponent<Animator>().enabled = false;

        //  ゲージの画像を元に戻す
        koburstGaugeFill.GetComponent<Image>().sprite = koburstDefaultSprite;

        SetLampActive(false);
    }

    //------------------------------------------------
    //  MAXランプを有効・無効にする
    //------------------------------------------------
    public void SetLampActive(bool active)
    {
        if(konburstLamp.activeSelf != active)
        {
            konburstLamp.SetActive(active);
        }
    }
    //------------------------------------------------
    //  ２秒後にコリジョンOFFにする
    //------------------------------------------------
    private IEnumerator ResetCollision()
    {
        yield return new WaitForSeconds(2);

        //  ボムコリジョンOFF
        bombCollision.SetActive(false);
    }

}
