using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using Unity.VisualScripting;

//--------------------------------------------------------------
//
//  プレイヤーの体力管理クラス
//
//--------------------------------------------------------------

//  基本体力はハート３個分（半ハート６個分）
//  最大体力はハート６個分（半ハート１２個分）
//  最小単位の半ハート換算でプログラムする
public class PlayerHealth : MonoBehaviour
{
    //  ハートのタイプ
    enum HeartType
    {
        Half,         //  半分
        Full,         //  ハート１個分
        ShieldNone,   //  シールドハート０個分
        ShieldHalf,   //  シールド半分
        ShieldFull,   //  シールドハート１個分

        Max
    }

    [SerializeField] private int currentMaxHealth;  //  必ず偶数
    [SerializeField] private int currentHealth;
    private const int limitHealth = 10;
    //[Watch]public bool bSuperMode;
    private bool bSuperMode;
    private bool bDeath;
    private bool bDamage;

    //  シングルトンなインスタンス
    public static PlayerHealth Instance
    {
        get; private set;
    }

    //  ダメージ演出用のAnimator
    [SerializeField] private RuntimeAnimatorController animPlayerFront;
    [SerializeField] private RuntimeAnimatorController animPlayerFrontLeft;
    [SerializeField] private RuntimeAnimatorController animPlayerFrontRight;
    [SerializeField] private RuntimeAnimatorController animPlayerFrontDamage;
    [SerializeField] private RuntimeAnimatorController animPlayerBack;
    [SerializeField] private RuntimeAnimatorController animPlayerBackLeft;
    [SerializeField] private RuntimeAnimatorController animPlayerBackRight;
    [SerializeField] private RuntimeAnimatorController animPlayerBackDamage;

    //  死亡演出用のAnimator
    [SerializeField] private RuntimeAnimatorController animPlayerDeath1;
    [SerializeField] private RuntimeAnimatorController animPlayerDeath2;

    //  シールド演出用のAnimator
    [SerializeField] private RuntimeAnimatorController animPlayerShieldFront;
    [SerializeField] private RuntimeAnimatorController animPlayerShieldFrontLeft;
    [SerializeField] private RuntimeAnimatorController animPlayerShieldFrontRight;
    [SerializeField] private RuntimeAnimatorController animPlayerShieldBack;
    [SerializeField] private RuntimeAnimatorController animPlayerShieldBackLeft;
    [SerializeField] private RuntimeAnimatorController animPlayerShieldBackRight;


    //  点滅させるためのSpriteRenderer
    SpriteRenderer sp;

    //  点滅の間隔
    private float flashInterval;

    //  点滅させるときのループカウント
    private int loopCount;

    //  プレイヤーの死亡エフェクト
    [SerializeField] private GameObject playerDeathEffect;
    //  ディレクショナルライト
    [SerializeField] private GameObject directionalLight;
    //  ハート画像の親オブジェクトの位置取得用
    [SerializeField] private GameObject heartRootObj;
    //  ハートフレームのプレハブ
    [SerializeField] private GameObject heartFrameObj;
    //  ハートフレームオブジェクトのリスト
    private List<GameObject> heartList = new List<GameObject>();

    //  シールドフラグ
    private bool isShielded;

    //-------------------------------------------------------------
    //  FaceUI周り
    //-------------------------------------------------------------
    //  顔UIオブジェクト
    [SerializeField] private GameObject faceObject;
    //  顔UIオブジェクトのImageコンポーネント
    private Image faceImage;
    //  千姫通常顔スプライト
    [SerializeField] private Sprite faceNormal;
    //  千姫ダメージ顔スプライト
    [SerializeField] private Sprite faceDamage;
    //  顔UIの絆創膏１
    [SerializeField] private GameObject faceBand1;
    //  顔UIの絆創膏２
    [SerializeField] private GameObject faceBand2;

    //  GameOver表示
    [SerializeField] private GameObject gameOver;

    //  ドロップパワーアップアイテム一覧
    [SerializeField] private GameObject DropItem;


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

        //  PlayerInfoManagerから初期化
        currentMaxHealth = PlayerInfoManager.g_MAXHP;
        if (currentMaxHealth > limitHealth)
            Debug.LogError("最大体力値が制限を超過しています！");
        if (currentMaxHealth % 2 != 0)
            Debug.LogError("currentMaxHealthは必ず偶数でなければいけません！");
        currentHealth = PlayerInfoManager.g_CURRENTHP;
        if (currentHealth > currentMaxHealth)
            Debug.LogError("現在体力値が最大体力値を超過しています！");

        //  顔UIのImageコンポーネントを取得
        faceImage = faceObject.GetComponent<Image>();

        //  顔UIの初期化
        faceImage.sprite = faceNormal;

        //  絆創膏１を無効化
        faceBand1.SetActive(false);

        //  絆創膏２を無効化
        faceBand2.SetActive(false);

        //  SpriteRenderを取得
        sp = GetComponent<SpriteRenderer>();

        //  ループカウントを設定
        loopCount = 10;

        //  点滅の間隔を設定
        flashInterval = 0.1f;

        //  ダメージフラグOFF
        bDamage = false;

        //  死亡フラグOFF
        bDeath = false;

        //  最初は無敵モードOFF
        bSuperMode = false;

        //  最初はGameOverも非表示
        gameOver.SetActive(false);

        //  最初はシールドなし
        isShielded = PlayerInfoManager.g_IS_SHIELD;

        //  親オブジェクトの子オブジェクトとしてハートフレームを生成
        for (int i = 0; i < currentMaxHealth / 2; i++)
        {
            GameObject obj = Instantiate(heartFrameObj);
            obj.GetComponent<RectTransform>().SetParent(heartRootObj.transform);
            obj.GetComponent<RectTransform>().localScale = Vector3.one;
            obj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            obj.transform.GetChild((int)HeartType.Half).gameObject.SetActive(true);
            obj.transform.GetChild((int)HeartType.Full).gameObject.SetActive(true);


            heartList.Add(obj);   //  リストに追加
        }
    }

    void Update()
    {
        //  ハート画像を更新
        CalculateHealthUI(currentHealth);

        //  プレイヤーが死んでいたら処理しない
        if (bDeath) return;

        //  顔UIの絆創膏を更新
        ChangeDamageBand();

        //  ゲーム段階別でAnimatorの切り替え
        int gamestatus = GameManager.Instance.GetGameState();
        switch (gamestatus)
        {
            case (int)eGameState.Zako:
                ChangePlayerSpriteToFront(true);     //  手前向き
                break;
            case (int)eGameState.Boss:
                ChangePlayerSpriteToFront(false);    //  奥向き
                break;
            case (int)eGameState.Event:
                ChangePlayerSpriteToFront(false);    //  奥向き
                break;
        }
    }

    //----------------------------------------------------------------
    //  プレイヤーの当たり判定
    //----------------------------------------------------------------
    private async void OnTriggerEnter2D(Collider2D collision)
    {
        //  死亡しているなら飛ばす
        if (bDeath) return;

        //  シールド状態
        if (isShielded)
        {
            //  無敵モードなら飛ばす
            if (bSuperMode) return;

            if (collision.CompareTag("Enemy") ||    //  ザコ敵本体にHIT！
                collision.CompareTag("Boss") ||     //  ボス本体にHIT！
                collision.CompareTag("EnemyBullet") //  敵弾にHIT！
            )
            {
                //  無敵モードON
                bSuperMode = true;

                //  シールドを削除
                isShielded = false;

                //  シールド破壊SE再生
                SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.SFX_DAMAGE,
                    (int)SFXList.SFX_SHIELDBREAK);

                //  無敵演出開始
                var taskBlink = Blink(flashInterval, loopCount);
                await taskBlink;
            }
        }
        //  通常状態
        else
        {
            if (collision.CompareTag("Enemy"))    //  敵本体にHIT！
            {
                //  無敵モードなら飛ばす
                if (bSuperMode) return;

                //  無敵モードON
                bSuperMode = true;

                //  プレイヤーのダメージ処理
                EnemyData ed = collision.GetComponent<Enemy>().GetEnemyData();


                //  ダメージ関連処理
                await AfterDamage(ed.Attack,flashInterval, loopCount);

            }
            else if (collision.CompareTag("Boss"))    //  敵本体にHIT！
            {
                //  ノックバック！
                KnockBack(collision);

                //  無敵モードなら飛ばす
                if (bSuperMode) return;

                //  無敵モードON
                bSuperMode = true;

                //  プレイヤーのダメージ処理
                EnemyData ed = null;

                BossBase boss = null;
                switch (PlayerInfoManager.stageInfo)
                {
                    case PlayerInfoManager.StageInfo.Stage01:
                        boss = collision.GetComponent<BossDouji>();
                        break;
                    case PlayerInfoManager.StageInfo.Stage02:
                        boss = collision.GetComponent<BossTsukumo>();
                        break;
                    case PlayerInfoManager.StageInfo.Stage03:
                       boss = collision.GetComponent<BossKuchinawa>();
                       break;
                    //case PlayerInfoManager.StageInfo.Stage04:
                    //    boss = collision.GetComponent<BossKurama>();
                    //    break;
                    //case PlayerInfoManager.StageInfo.Stage05:
                    //    boss = collision.GetComponent<BossWadatsumi>();
                    //    break;
                    //case PlayerInfoManager.StageInfo.Stage06:
                    //    boss = collision.GetComponent<BossHakumen>();
                    //    break;
                    default:
                        Debug.LogError("PlayerInfoManager.stageInfoに範囲外の値が入っています");
                        break;
                }
                if (!boss) Debug.LogError("BossBaseクラスの'boss'がnullになっています");


                //  ダメージ量の設定
                ed = boss.GetEnemyData();
                if (ed == null) Debug.LogError("EnemyDataの取得に失敗しました");


                //  ダメージ関連処理
                await AfterDamage(ed.Attack,flashInterval, loopCount);

            }
            else if (collision.CompareTag("EnemyBullet"))    //  敵弾にHIT！
            {
                //  無敵モードなら飛ばす
                if (bSuperMode) return;

                //  プレイヤーのダメージ処理
                int power = 0;

                if (collision.GetComponent<EnemyBullet>())
                {
                    power = collision.GetComponent<EnemyBullet>().GetPower();
                }
                else if (collision.GetComponent<TsukumoFireworks>())
                {
                    power = collision.GetComponent<TsukumoFireworks>().GetPower();
                }
                else if (collision.GetComponent<DoujiPhase2Bullet>())
                {
                    power = collision.GetComponent<DoujiPhase2Bullet>().GetPower();
                }
                else if (collision.GetComponent<DoujiPhase3Bullet>())
                {
                    power = collision.GetComponent<DoujiPhase3Bullet>().GetPower();
                }
                else if (collision.GetComponent<TsukumoHomingBullet>())
                {
                    power = collision.GetComponent<TsukumoHomingBullet>().GetPower();
                }
                else if (collision.GetComponent<TsukumoPhase2Bullet>())
                {
                    power = collision.GetComponent<TsukumoPhase2Bullet>().GetPower();
                }
                else if (collision.GetComponent<TsukumoPhase3Bullet>())
                {
                    power = collision.GetComponent<TsukumoPhase3Bullet>().GetPower();
                }
                // else if (collision.GetComponent<KuchinawaPhase1Bullet>())
                // {
                //     power = collision.GetComponent<KuchinawaPhase1Bullet>().GetPower();
                // }
                // else if (collision.GetComponent<KuchinawaPhase2Bullet>())
                // {
                //     power = collision.GetComponent<KuchinawaPhase2Bullet>().GetPower();
                // }
                else if (collision.GetComponent<KuchinawaPhase3Bullet>())
                {
                    power = collision.GetComponent<KuchinawaPhase3Bullet>().GetPower();

                    //  点滅時間用の変数
                    float interval = 0.01f;
                    int loop_count = 1;

                    //  ダメージ関連処理
                    await AfterDamage(power,interval, loop_count);

                    return;
                }



                //  ダメージ関連処理
                await AfterDamage(power,flashInterval, loopCount);

            }
            else if (collision.CompareTag("Obstacles"))    //  障害物にHIT！
            {

            }
        }
    }

    //----------------------------------------------------------------
    //  レーザーなどの特殊な当たり判定
    //----------------------------------------------------------------
    private async void OnTriggerStay2D(Collider2D collision)
    {
        //  点滅時間用の変数
        float interval = 0.01f;
        int loop_count = 1;

        if (collision.CompareTag("EnemyBullet"))    //  敵弾にHIT！
        {
            //  無敵モードなら飛ばす
            if (bSuperMode) return;

            //  プレイヤーのダメージ処理
            int power = 0;

            //  クチナワレーザーがHIT！
            if (collision.GetComponent<KuchinawaPhase3Bullet>())
            {
                power = collision.GetComponent<KuchinawaPhase3Bullet>().GetPower();
            }

            //  ダメージ関連処理
            await AfterDamage(power,interval, loop_count);
        }
    }

    //-------------------------------------------
    //  ダメージの一連の処理
    //-------------------------------------------
    private async UniTask AfterDamage(int damage_value,float flashInterval, int loopCount)
    {
        //  無敵モードON
        bSuperMode = true;

        //  ダメージ処理
        Damage(damage_value);

        //  死亡フラグON
        if (currentHealth <= 0)
        {
            bDamage = false;
            bDeath = true;
            //  HitCircleを非表示にする
            this.transform.GetChild(6).gameObject.SetActive(false);

            //  ショットの無効化
            PlayerShotManager psm = this.GetComponent<PlayerShotManager>();
            psm.DisableShot();

            //  ボムの無効化
            this.GetComponent<PlayerBombManager>().enabled = false;

            StartCoroutine(Death());       //  やられ演出
            return;
        }

        //  ダメージ顔UIに変更
        StartCoroutine(ChangeToDmageFace());

        //  ダメージSE再生
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_DAMAGE,
        (int)SFXList.SFX_PLAYER_DAMAGE);

        //  全強化１段階ダウン
        PlayerShotManager ps = this.GetComponent<PlayerShotManager>();
        ps.LeveldownNormalShot();
        //PlayerMovement pm = this.GetComponent<PlayerMovement>();
        //pm.LeveldownMoveSpeed();

        //  デバッグ表示
        Debug.Log("ショット強化１段階ダウン！");
        Debug.Log("ショット強化 :" + ps.GetNormalShotLevel());
        Debug.Log("Playerの体力 :" + currentHealth);

        //  ダメージ時の赤くなる点滅演出開始
        var task1 = DamageAnimation();
        await task1;

        //  無敵演出開始
        var taskBlink = Blink(flashInterval, loopCount);
        await taskBlink;
    }

    //-------------------------------------------
    //  ダメージ時の赤くなる点滅演出
    //-------------------------------------------
    private async UniTask DamageAnimation()
    {
        //GameObjectが破棄された時にキャンセルを飛ばすトークンを作成
        var token = this.GetCancellationTokenOnDestroy();

        //  一瞬色が変わる
        await UniTask.Delay(TimeSpan.FromSeconds(0.3f))
        .AttachExternalCancellation(token);

        //  ダメージフラグOFF
        bDamage = false;

    }

    //-------------------------------------------
    //  ダメージ時の無敵点滅演出
    //-------------------------------------------
    private async UniTask Blink(float flashInterval,int loopCount)
    {
        //GameObjectが破棄された時にキャンセルを飛ばすトークンを作成
        var token = this.GetCancellationTokenOnDestroy();

        //点滅ループ開始
        for (int i = 0; i < loopCount; i++)
        {
            //flashInterval待ってから
            await UniTask.Delay(TimeSpan.FromSeconds(flashInterval))
                .AttachExternalCancellation(token);

            //spriteRendererをオフ
            sp.enabled = false;

            //flashInterval待ってから
            await UniTask.Delay(TimeSpan.FromSeconds(flashInterval))
                .AttachExternalCancellation(token);

            //spriteRendererをオン
            sp.enabled = true;
        }

        //  無敵モードOFF
        bSuperMode = false;
    }

    //-------------------------------------------
    //  プレイヤーのスプライトを差し替える
    //-------------------------------------------
    private void ChangePlayerSpriteToFront(bool front)
    {
        if (front)   //  ザコ戦中
        {
            if (bDamage) //  ダメージ中！
            {
                this.GetComponent<Animator>().runtimeAnimatorController =
                    animPlayerFrontDamage;
            }
            else // 通常
            {
                //  水平方向の入力値チェック
                int check = this.GetComponent<PlayerMovement>().GetHorizontalCheck();

                if (check == 1)          //  入力が+方向なら
                {
                    if (isShielded)  //  シールド状態時
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerShieldFrontRight;
                    }
                    else
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerFrontRight;
                    }

                }
                else if (check == -1)    //  入力が-方向なら
                {
                    if (isShielded)  //  シールド状態時
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerShieldFrontLeft;
                    }
                    else
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerFrontLeft;
                    }
                }
                else   //   入力なしなら
                {
                    if (isShielded)  //  シールド状態時
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerShieldFront;
                    }
                    else
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerFront;
                    }
                }
            }

        }
        else        //  ボス戦中
        {
            if (bDamage) //  ダメージ中！
            {
                this.GetComponent<Animator>().runtimeAnimatorController =
                    animPlayerBackDamage;
            }
            else // 通常
            {
                //  水平方向の入力値チェック
                int check = this.GetComponent<PlayerMovement>().GetHorizontalCheck();

                if (check == 1)          //  入力が+方向なら
                {
                    if (isShielded)  //  シールド状態時
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerShieldBackRight;
                    }
                    else
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerBackRight;
                    }
                }
                else if (check == -1)    //  入力が-方向なら
                {
                    if (isShielded)  //  シールド状態時
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerShieldBackLeft;
                    }
                    else
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerBackLeft;
                    }
                }
                else   //   入力なしなら
                {
                    if (isShielded)  //  シールド状態時
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerShieldBack;
                    }
                    else
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerBack;
                    }
                }
            }
        }
    }

    //-------------------------------------------
    //  ダメージ処理
    //-------------------------------------------
    public void Damage(int value)
    {
        //  ダメージ！
        bDamage = true;

        int target = currentHealth - value;
        //  最低体力で止める
        if (target <= 0)
        {
            currentHealth = 0;
        }

        currentHealth = target;

        ////  プレイヤーの通常弾レベルが１じゃないなら
        //if(GetComponent<PlayerShotManager>().GetNormalShotLevel() > 1)
        //{
        //    Debug.Log("ドロップ！");

        //    //  ショット強化アイテムを落とす
        //    Instantiate(DropItem,this.transform.position,Quaternion.identity);
        //}


        //  デバッグ表示
        Debug.Log($"プレイヤーの体力が{value}減少して\n" +
            $"{currentHealth}になりました！");

    }

    //-------------------------------------------
    //  回復処理
    //-------------------------------------------
    public void Heal(int value)
    {
        int target = currentHealth + value;
        //  最大体力で止める
        if (target >= currentMaxHealth)
        {
            target = currentMaxHealth;
        }

        currentHealth = target;

        //  ハート画像を更新
        CalculateHealthUI(currentHealth);

        //  顔UIの絆創膏を更新
        ChangeDamageBand();

        //  デバッグ表示
        Debug.Log($"プレイヤーの体力が{value}回復して\n" +
            $"{currentHealth}になりました！");
    }

    //-------------------------------------------
    //  最大体力増加処理
    //-------------------------------------------
    public void IncreaseHP(int value)
    {
        int target = currentMaxHealth + value;
        //  最大体力10で止める
        if (target >= limitHealth)
        {
            //  11以上は処理しない
            if (target > limitHealth)
            {
                return;
            }
            //  10以上は10で止める
            target = limitHealth;
        }

        currentMaxHealth = target;

        //  親オブジェクトの子オブジェクトとしてハートフレームを生成
        GameObject obj = Instantiate(heartFrameObj);
        obj.GetComponent<RectTransform>().SetParent(heartRootObj.transform);
        obj.GetComponent<RectTransform>().localScale = Vector3.one;
        obj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
        //obj.transform.GetChild((int)HeartType.Half).gameObject.SetActive(true);
        //obj.transform.GetChild((int)HeartType.Full).gameObject.SetActive(true);
        heartList.Add(obj);   //  リストに追加

        //  ハート画像を更新
        CalculateHealthUI(currentHealth);

        //  顔UIの絆創膏を更新
        ChangeDamageBand();

        //  デバッグ表示
        Debug.Log($"プレイヤーの最大体力が{value}増加して\n" +
            $"{currentMaxHealth}になりました！");
    }

    //-------------------------------------------
    //  プロパティ
    //-------------------------------------------
    public void SetCurrentHealth(int value)
    {
        Assert.IsFalse((value < 0 || value > currentMaxHealth),
            "体力に範囲外の数が設定されています！");

        if (currentMaxHealth != value) currentMaxHealth = value;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void SetCurrentMaxHealth(int value)
    {
        Assert.IsFalse((value < 0 || value > limitHealth),
            "最大体力に範囲外の数が設定されています！");

        if (currentMaxHealth != value) currentMaxHealth = value;
    }

    public int GetCurrentMaxHealth()
    {
        return currentMaxHealth;
    }

    public void SetDamageFlag(bool flag)
    {
        bDamage = flag;
    }

    public bool GetDamageFlag()
    {
        return bDamage;
    }

    public void SetSuperMode(bool flag) { bSuperMode = flag; }
    public bool GetSuperMode() { return bSuperMode; }
    public void SetIsShielded(bool flag) { isShielded = flag; }
    public bool GetIsShielded() { return isShielded; }

    //-------------------------------------------
    //  やられ演出
    //-------------------------------------------
    private IEnumerator GameOverAnimation()
    {
        //  最終待機時間
        float procedural_time = 2.0f;
        //  アニメーションにかかる時間
        float duration = 1.0f;

        //  初期座標の設定
        gameOver.GetComponent<RectTransform>().anchoredPosition
            = new Vector2(-17, 563);

        //  GameOver表示を有効化
        gameOver.SetActive(true);

        //  移動開始
        gameOver.GetComponent<RectTransform>()
            .DOAnchorPos(new Vector2(-17f, 0f), duration)
            .SetEase(Ease.OutElastic);

        //  GameOverジングルを鳴らす
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_SYSTEM,
        (int)SFXList.SFX_GAMEOVER);

        //  １秒待つ
        yield return new WaitForSeconds(duration);

        //  回転開始
        gameOver.GetComponent<RectTransform>()
            .DOLocalRotate(new Vector3(0, 0, -35f), duration)
            .SetEase(Ease.InCubic);

        //  １秒待つ
        yield return new WaitForSeconds(duration);

        //  移動開始
        gameOver.GetComponent<RectTransform>()
            .DOAnchorPos(new Vector2(-17f, -700f), duration)
            .SetEase(Ease.OutElastic);

        //  １秒待つ
        yield return new WaitForSeconds(duration);

        //  GameOver表示を無効化
        gameOver.SetActive(false);

        //  最終待機
        yield return new WaitForSeconds(procedural_time);

        yield return null;
    }

    //-------------------------------------------
    //  やられ演出
    //-------------------------------------------
    private IEnumerator Death()
    {
        //  デバッグ表示
        Debug.Log("プレイヤーが死亡しました！");

        //  現在のBGMをストップ
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);

        //  現在ステージをリセット
        PlayerInfoManager.stageInfo = PlayerInfoManager.StageInfo.Stage01;

        //  ダメージ顔に変更する
        faceImage.sprite = faceDamage;

        //  死亡SE再生
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_DAMAGE,
        (int)SFXList.SFX_PLAYER_DEATH);

        //  ライトOFF
        directionalLight.gameObject.SetActive(false);

        //  子オブジェクトの吸魂フィールドを非アクティブに
        this.gameObject.transform.Find("Field").gameObject.SetActive(false);

        //  プレイヤーを止める
        this.GetComponent<CircleCollider2D>().enabled = false;
        this.GetComponent<PlayerMovement>().enabled = false;
        this.GetComponent<PlayerShotManager>().enabled = false;

        //  Animatorを「death1」に差し替え
        this.GetComponent<Animator>().runtimeAnimatorController =
            animPlayerDeath1;

        //  0.8秒待つ
        yield return new WaitForSeconds(0.8f);

        //  Animatorを「death2」に差し替え
        this.GetComponent<Animator>().runtimeAnimatorController =
            animPlayerDeath2;

        //  1秒待つ
        yield return new WaitForSeconds(1.0f);

        //  プレイヤーを非表示にする
        this.GetComponent<SpriteRenderer>().enabled = false;

        //  プレイヤーの影(７番目の子オブジェクト)も非表示にする
        this.transform.GetChild(7).gameObject.SetActive(false);

        //  プレイヤーのやられエフェクト
        GameObject obj = Instantiate(
            playerDeathEffect,
            this.transform.position,
            Quaternion.identity);

        //  やられエフェクトの終了を待つ
        yield return new WaitForSeconds(0.583f);

        //  GameOverアニメーションの終了を待つ
        yield return StartCoroutine(GameOverAnimation());

        //  GameOverへシーン遷移
        LoadingScene.Instance.LoadNextScene("GameOver");

        yield return null;
    }

    //---------------------------------------------------
    //  現在体力を受け取って体力UIを計算する
    //---------------------------------------------------
    public void CalculateHealthUI(int health)
    {
        if (health < 0)
        {
            health = 0;
        }

        //  体力0ならハートを全部非表示にする
        if (health == 0)
        {
            for (int i = 0; i < heartList.Count; i++)
            {
                heartList[i].transform.GetChild((int)HeartType.Half)
                    .gameObject.SetActive(false);
                heartList[i].transform.GetChild((int)HeartType.Full)
                    .gameObject.SetActive(false);
                heartList[i].transform.GetChild((int)HeartType.ShieldNone)
                    .gameObject.SetActive(false);
                heartList[i].transform.GetChild((int)HeartType.ShieldHalf)
                    .gameObject.SetActive(false);
                heartList[i].transform.GetChild((int)HeartType.ShieldFull)
                    .gameObject.SetActive(false);
            }
        }
        else if (health == 1)
        {
            for (int i = 0; i < heartList.Count; i++)
            {
                if (i == 0)
                {
                    //  シールド状態時の処理
                    if (isShielded)
                    {
                        heartList[i].transform.GetChild((int)HeartType.Half)
                            .gameObject.SetActive(true);
                        heartList[i].transform.GetChild((int)HeartType.Full)
                            .gameObject.SetActive(false);
                        heartList[i].transform.GetChild((int)HeartType.ShieldNone)
                            .gameObject.SetActive(true);
                        heartList[i].transform.GetChild((int)HeartType.ShieldHalf)
                            .gameObject.SetActive(true);
                        heartList[i].transform.GetChild((int)HeartType.ShieldFull)
                            .gameObject.SetActive(false);
                    }
                    //  通常状態時の処理
                    else
                    {
                        heartList[i].transform.GetChild((int)HeartType.Half)
                            .gameObject.SetActive(true);
                        heartList[i].transform.GetChild((int)HeartType.Full)
                            .gameObject.SetActive(false);
                        heartList[i].transform.GetChild((int)HeartType.ShieldNone)
                            .gameObject.SetActive(false);
                        heartList[i].transform.GetChild((int)HeartType.ShieldHalf)
                            .gameObject.SetActive(false);
                        heartList[i].transform.GetChild((int)HeartType.ShieldFull)
                            .gameObject.SetActive(false);
                    }
                }

                //  残りを非表示にする
                for (int j = 1; j < heartList.Count; j++)
                {
                    //  シールド状態時の処理
                    if (isShielded)
                    {
                        heartList[j].transform.GetChild((int)HeartType.Half)
                            .gameObject.SetActive(false);
                        heartList[j].transform.GetChild((int)HeartType.Full)
                            .gameObject.SetActive(false);
                        heartList[j].transform.GetChild((int)HeartType.ShieldNone)
                            .gameObject.SetActive(true);
                        heartList[j].transform.GetChild((int)HeartType.ShieldHalf)
                            .gameObject.SetActive(false);
                        heartList[j].transform.GetChild((int)HeartType.ShieldFull)
                            .gameObject.SetActive(false);
                    }
                    //  通常状態時の処理
                    else
                    {
                        heartList[j].transform.GetChild((int)HeartType.Half)
                            .gameObject.SetActive(false);
                        heartList[j].transform.GetChild((int)HeartType.Full)
                            .gameObject.SetActive(false);
                        heartList[j].transform.GetChild((int)HeartType.ShieldNone)
                            .gameObject.SetActive(false);
                        heartList[j].transform.GetChild((int)HeartType.ShieldHalf)
                            .gameObject.SetActive(false);
                        heartList[j].transform.GetChild((int)HeartType.ShieldFull)
                            .gameObject.SetActive(false);
                    }
                }

            }
        }
        else // 体力が２以上の時
        {
            //  一旦現在体力のとこまで全部フルで埋める
            int fullNum = health / 2;
            for (int i = 0; i < fullNum; i++)
            {
                //  シールド状態時の処理
                if (isShielded)
                {
                    heartList[i].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.ShieldNone)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.ShieldHalf)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.ShieldFull)
                        .gameObject.SetActive(true);
                }
                //  通常状態時の処理
                else
                {
                    heartList[i].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.ShieldNone)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.ShieldHalf)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.ShieldFull)
                        .gameObject.SetActive(false);
                }
            }

            //  奇数だった場合は最後の番号だけハーフにする
            int taegetNum = health - fullNum;
            if (health % 2 != 0)
            {
                //  シールド状態時の処理
                if (isShielded)
                {
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(true);
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.ShieldNone)
                        .gameObject.SetActive(true);
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.ShieldHalf)
                        .gameObject.SetActive(true);
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.ShieldFull)
                        .gameObject.SetActive(false);
                }
                //  通常状態時の処理
                else
                {
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(true);
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.ShieldNone)
                        .gameObject.SetActive(false);
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.ShieldHalf)
                        .gameObject.SetActive(false);
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.ShieldFull)
                        .gameObject.SetActive(false);
                }

            }

            //  残りを非表示にする
            for (int i = taegetNum; i < heartList.Count; i++)
            {
                //  シールド状態時の処理
                if (isShielded)
                {
                    heartList[i].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.ShieldNone)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.ShieldHalf)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.ShieldFull)
                        .gameObject.SetActive(false);
                }
                //  通常状態時の処理
                else
                {
                    heartList[i].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.ShieldNone)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.ShieldHalf)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.ShieldFull)
                        .gameObject.SetActive(false);
                }
            }
        }
    }

    //---------------------------------------------------
    //  顔UIをダメージ顔に変更する
    //---------------------------------------------------
    private IEnumerator ChangeToDmageFace()
    {
        //  ダメージ顔に変更する
        faceImage.sprite = faceDamage;

        //  指定時間待つ
        yield return new WaitForSeconds(1);

        //  通常顔に戻す
        faceImage.sprite = faceNormal;
    }

    //---------------------------------------------------
    //  顔UIの絆創膏を残り体力によって切り替える
    //---------------------------------------------------
    private void ChangeDamageBand()
    {
        //  体力が最大体力の半分以下で絆創膏１が有効化
        if (currentHealth <= currentMaxHealth / 2)
            faceBand1.SetActive(true);
        else faceBand1.SetActive(false);

        //  残り体力が2で絆創膏２が有効化
        if (currentHealth <= 2)
            faceBand2.SetActive(true);
        else faceBand2.SetActive(false);

    }

    //------------------------------------------------------
    //  ノックバック
    //------------------------------------------------------
    private void KnockBack(Collider2D collision)
    {
        float distance = 5.0f;  //  ノックバック距離
        float duration = 0.1f; //  移動にかかる時間（秒）

        //  ボスからのベクトルを計算
        Vector3 vec = -collision.transform.up;
        Vector3 ppos = collision.transform.position;
        Vector3 direction = default;    //  ノックバック方向

        //Debug.Log($"<color=yellow>ノックバック！</color>");

        //  問答無用でボスの前にノックバックする
        direction = vec;

        //  ノックバック後の座標を計算
        Vector3 pos = ppos + direction * distance;

        //  移動開始
        this.transform.DOMove(pos, duration)
            .SetEase(Ease.Linear);
    }
}
