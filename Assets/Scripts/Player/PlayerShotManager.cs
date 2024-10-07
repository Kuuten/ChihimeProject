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
//  プレイヤーのショット管理クラス
//
//--------------------------------------------------------------
//  ショットの種類
public enum SHOT_TYPE
{
    DOUJI,              // ドウジ
    TSUKUMO,            // ツクモ
    KUCHINAWA,          // クチナワ
    KURAMA,             // クラマ
    WADATSUMI,          // ワダツミ
    HAKUMEN,            // ハクメン

    TYPE_MAX
}

//  ノーマル弾のレベルリスト
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

    //  シングルトンなインスタンス
    public static PlayerShotManager Instance
    {
        get; private set;
    }

    //  弾の発射点
    [SerializeField]private Transform firePoint1;
    [SerializeField]private Transform firePoint2_L;
    [SerializeField]private Transform firePoint2_R;
    [SerializeField]private Transform firePoint3_L;
    [SerializeField]private Transform firePoint3_R;
    //  通常弾のプレハブ
    [SerializeField]private GameObject normalBulletPrefab;
    //  魂バート弾のプレハブ
    [SerializeField]private GameObject[] convertBulletPrefab;
    //  吸魂フィールドオブジェクト
    [SerializeField]private GameObject fieldObject;
    //  吸魂フィールドオブジェクトのスケール
    private Vector3 fieldObjectScale;

    //  弾の移動ベクトル
    private Vector3 velocity;
    //  ノーマル弾のショット可能フラグ
    private bool canShot;
    //  ノーマル弾のショットCDのカウント用
    private float shotCount = 0;
    //  ノーマル弾の弾を何秒毎に撃てるか
    private float shotInterval = 0.05f;
    //  ノーマル弾の移動量
    private const float normalSpeed = -20f; 
    //  ノーマル弾の攻撃力
    private float normalShotPower;
    //  ノーマル弾のレベル
    private int normalShotLevel;
    //  ノーマル弾のレベル表示用画像のプレハブ
    [SerializeField] private GameObject shotPowerLevelIcon;
    //  ノーマル弾のショットパワーアイコンのリスト
    private List<GameObject> shotPowerIconList = new List<GameObject>();
    //  ノーマル弾のショットパワーアイコンの親オブジェクトの位置取得用
    [SerializeField] private GameObject shotPowerIconRootObj;

    //  魂バート弾の種類
    public enum CONVERT_TYPE
    {
        MIDDLE,
        FULL,

        MAX
    }

    //  魂バートゲージ用のスライダー１
    [SerializeField]private GameObject sliderObj1;
    //  魂バートゲージ用のスライダー２
    [SerializeField]private GameObject sliderObj2;
    //  魂バートゲージ１が溜まるスピード
    private const float gauge1Speed = 2.0f;
    //  魂バートゲージ１が減少するスピード
    private const float gauge1MinusSpeed = 1.0f;
    //  魂バートゲージ２が溜まるスピード
    private const float gauge2Speed = 2.0f;
    //  魂バートゲージ１が減少するスピード
    private const float gauge2MinusSpeed = 1.0f;
    //  魂バートショットが撃てるかどうか
    private bool canConvert;
    //  魂バートゲージの増減用変数
    private float gaugeValue;
    //  魂バートゲージの溜まり具合
    enum ConvertState
    {
        None,               //  無
        Restore,            //  チャージ中
        ReleaseRestore,     //  不完全解放中
        MidPower,           //  中攻撃
        ReleaseMidPower,    //  中攻撃解放中
        FullPower,          //  強攻撃
        ReleaseFullPower    //  強攻撃解放中
    }
    private ConvertState convertState;
    //  コンバート弾が強攻撃かどうか
    bool convertIsFullPower;
    //  魂バート弾の威力(中攻撃)
    private float[] convertShotPowerHalf = new float[(int)SHOT_TYPE.TYPE_MAX];
    //  魂バート弾の威力(強攻撃)
    private float[] convertShotPowerFull = new float[(int)SHOT_TYPE.TYPE_MAX];

    //  ゲームの進行状態
    public int gamestatus;

    //  入力
    InputAction shot;
    InputAction shotConvert;

    //  通常弾の通常威力
    private const float normalShotPower_normal = 1.0f;
    //  通常弾の溜め威力
    private const float normalShotPower_charge = 1.5f;
    //  溜め時間
    private const float chargeDuration = 1.0f;
    //  連続でチャージ威力で撃てる通常弾の数
    private const int chargeNormalShotNum = 30;
    //  通常弾の溜めフラグ
    private bool normalShotChargeFlag;
    //  溜めタイマー用
    private float chargeTimer;

    //  チャージされてから通常弾を何発撃ったか(wayは含まない)
    private int chargeNormalShotCount;


    void Start()
    {
        // InputActionにShotを設定
        PlayerInput playerInput = GetComponent<PlayerInput>();
        shot = playerInput.actions["Shot"];
        shotConvert = playerInput.actions["ConvertShot"];

        normalShotPower = normalShotPower_normal;
        normalShotChargeFlag = false;
        chargeTimer =0f;
        chargeNormalShotCount = 0;
        shotCount = 0;
        canShot = true;
        normalShotLevel = 1; // 最初はレベル１
        gaugeValue = 0.0f;
        convertState = ConvertState.None;
        fieldObjectScale = new Vector3(3f,3f,3f);
        canConvert =true;
        convertIsFullPower = false;

        //  魂バート弾ごとの弾の威力
        convertShotPowerHalf[(int)SHOT_TYPE.DOUJI] = 30f;
        convertShotPowerHalf[(int)SHOT_TYPE.TSUKUMO] = 2f;
        convertShotPowerHalf[(int)SHOT_TYPE.KUCHINAWA] = 1f;
        convertShotPowerHalf[(int)SHOT_TYPE.KURAMA] = 7f;
        convertShotPowerHalf[(int)SHOT_TYPE.WADATSUMI] = 2f;
        convertShotPowerHalf[(int)SHOT_TYPE.HAKUMEN] = 5f;

        convertShotPowerFull[(int)SHOT_TYPE.DOUJI] = 90f;
        convertShotPowerFull[(int)SHOT_TYPE.TSUKUMO] = 5f;
        convertShotPowerFull[(int)SHOT_TYPE.KUCHINAWA] = 2f;
        convertShotPowerFull[(int)SHOT_TYPE.KURAMA] = 15f;
        convertShotPowerFull[(int)SHOT_TYPE.WADATSUMI] = 5f;
        convertShotPowerFull[(int)SHOT_TYPE.HAKUMEN] = 10f;

        //  親オブジェクトの子オブジェクトとしてボムアイコンを生成
        for( int i=0; i<(int)eNormalShotLevel.Lv3;i++ )
        {
            GameObject obj = Instantiate(shotPowerLevelIcon);
            obj.GetComponent<RectTransform>().SetParent( shotPowerIconRootObj.transform);
            obj.GetComponent<RectTransform>().localScale = Vector3.one;
            obj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0,0,0);

            shotPowerIconList.Add( obj );   //  リストに追加
        }

        //  弾の向きはとりあえず通常弾に合わせる
        velocity = new Vector3(0,normalSpeed,0);   //  最初は下方向へ撃つ
    }

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

    //----------------------------------------------------------
    //  ショットの初期化・有効化
    //----------------------------------------------------------
    public void InitShot()
    {
        shotCount = 0;
        canShot = true;
        normalShotChargeFlag = false;
        chargeTimer =0f;
        chargeNormalShotCount = 0;

        fieldObjectScale = new Vector3(3f,3f,3f);

        //  吸魂フィールドのスケール初期化
        KonFieldAlphaAnimation(0.784f,0.0f,0.5f);
        fieldObject.transform.DOScale(
            new Vector3(3f,3f,3f),
            0.5f);

        canConvert = true; //  コンバート溜め可能

        //  スライダー２をリセット
        sliderObj2.GetComponent<Slider>().value = 0f;
        sliderObj2.SetActive(false);

        //  スライダー１をリセット
        sliderObj1.GetComponent<Slider>().value = 0f;
        sliderObj1.SetActive(false);
        gaugeValue = 0.0f;

        //  無にリセット
        convertState = ConvertState.None;  
    }

    //----------------------------------------------------------
    //  ショットの有効化
    //----------------------------------------------------------
    public void EnableShot()
    {
        shot.Enable();
        shotConvert.Enable();
    }

    //----------------------------------------------------------
    //  ショットの無効化
    //----------------------------------------------------------
    public void DisableShot()
    {
        shot.Disable();
        shotConvert.Disable();

        //  ショットの初期化
        InitShot();
    }

    void Update()
    {
        //  GameManagerから状態を取得
        gamestatus = gameManager.GetGameState();

        //  通常弾レベルアイコンを更新
        UpdateShotPowerIcon();

        //  ゲーム段階別処理
        switch (gamestatus)
        {
            case (int)eGameState.Zako:
                NormalShot(true);                    //  通常弾
                ConvertShot(true);                   //  魂バート弾
                break;
            case (int)eGameState.Boss:
                NormalShot(false);                   //  通常弾
                ConvertShot(false);                  //  魂バート弾
                break;
            case (int)eGameState.Event:
                NormalShot(false);                   //  通常弾
                ConvertShot(false);                  //  魂バート弾
                break;
        }

    }

    //------------------------------------------------
    //  通常弾のパワーアイコンの数を更新する
    //------------------------------------------------
    private void UpdateShotPowerIcon()
    {
        if(normalShotLevel <= 0)Debug.LogError("normalShotLevelに0以下の値が入っています！");

        //  1回全部表示
        for(int i=0;i<shotPowerIconList.Count;i++)
        {
            shotPowerIconList[i].gameObject.SetActive(true);
        }

        //  非表示処理
        for(int i=shotPowerIconList.Count-1;i>normalShotLevel-1;i--)
        {
            shotPowerIconList[i].gameObject.SetActive(false);
        }
    }

    //---------------------------------------------------------
    //  プロパティ
    //---------------------------------------------------------
    public int GetNormalShotLevel(){ return normalShotLevel; }
    public void SetNormalShotLevel(int level)
    {
        Debug.Assert(normalShotLevel >= (int)eNormalShotLevel.Lv1 &&
            normalShotLevel <= (int)eNormalShotLevel.Lv3,
            "通常弾レベルの設定値が範囲外になっています！");
        if(normalShotLevel != level)normalShotLevel = level;
    }
    public float GetNormalShotPower(){ return normalShotPower; }
    public void SetNormalShotPower(float power) { normalShotPower = power; }
    public bool IsConvertFullPower(){ return convertIsFullPower; }
    public float GetConvertShotPower()
    {
        if(convertIsFullPower)return convertShotPowerFull[(int)PlayerInfoManager.g_CONVERTSHOT];
        else return convertShotPowerHalf[(int)PlayerInfoManager.g_CONVERTSHOT];
    }

    //  弾の移動ベクトルを反転する
    public Vector3 GetReverseVelocity(int state)
    {
        //  ザコ戦中はデフォルト設定にする
        if( state == (int)eGameState.Zako )
        {
            //  移動ベクトル設定
            velocity.x = 0f;
            velocity.y = normalSpeed;
            velocity.z = 0f;
            return velocity;
        }
        else if( state == (int)eGameState.Boss ) // ボス戦中なら反転
        {
            //  移動ベクトル設定
            velocity.x = 0f;
            velocity.y = 20f;
            velocity.z = 0f;
            return velocity;
        }
        else if( state == (int)eGameState.Event ) // 会話イベント中なら撃てない
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
    //  通常弾
    //-------------------------------------------
    private void NormalShot(bool flipY)
    {
        //  ボタンを離している間
        if (!shot.IsPressed())
        {
            //  通常弾強化カウント開始
            if(chargeTimer >= chargeDuration)
            {
                chargeTimer = 0f;
                //  通常弾強化ON
                normalShotChargeFlag = true;

                //Debug.Log("通常弾強化ON！！");
            }
            else chargeTimer += Time.deltaTime;
        }

        //  通常弾強化
        if(normalShotChargeFlag)
        {
            normalShotPower = normalShotPower_charge;
        }
        else
        {
            normalShotPower = normalShotPower_normal;
        }

        //  通常弾を撃つ
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
            //  弾発射
            if (shot.IsPressed())
            {
                //  チャージされていたらカウントを増やす
                if(normalShotChargeFlag)
                {
                    if(chargeNormalShotCount > chargeNormalShotNum)
                    {
                        chargeNormalShotCount = 0;
                        normalShotChargeFlag = false;

                        //Debug.Log("通常弾強化OFF...");
                    }
                    else chargeNormalShotCount++;
                }
                

                //  フラグリセット
                canShot = false;

                //  オブジェクト一時格納用
                GameObject obj = null;

                //  通常弾の速度設定用
                NormalBullet n = null;;

                //  Velocity格納用
                Vector3 v = Vector3.zero;

                //  Y反転用のSpriteRenderer
                SpriteRenderer sr = null;

                //  Y反転時の発射口のY座標バイアス
                const float biasY = 0.44f;

                //  通常弾SE再生
                SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.SFX_NORMAL_SHOT,
                    (int)SFXList.SFX_NORMAL_SHOT);

                switch(normalShotLevel)
                {
                    case 1: //  レベル１
                        obj = Instantiate(
                        normalBulletPrefab,
                        firePoint1.position,
                        Quaternion.identity);

                        //  Yを反転するかどうか設定する
                        sr = obj.GetComponent<SpriteRenderer>(); 
                        sr.flipY = flipY;

                        //  反転時に座標を調整
                        if(!sr.flipY)
                        {
                            obj.transform.position = 
                                new Vector3(firePoint1.position.x,
                                firePoint1.position.y + biasY,
                                firePoint1.position.z);
                        }

                        //  ボス戦かどうかでVelocityを取得して設定
                        v = GetReverseVelocity(gamestatus);
                        velocity = v;
                        n = obj.GetComponent<NormalBullet>();
                        n.SetVelocity(velocity);

                        break;
                    case 2: //  レベル２
                        const int lv2BulletNum = 3; //  一度に出る弾の数

                        //  弾の数分のリストを確保
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

                            //  Yを反転するかどうか設定する
                            sr = obj.GetComponent<SpriteRenderer>(); 
                            sr.flipY = flipY;

                            //  反転時に座標を調整
                            if(!sr.flipY)
                            {
                                obj.transform.position = 
                                    new Vector3(
                                        firePointLv2[i].position.x,
                                        firePointLv2[i].position.y + biasY,
                                        firePointLv2[i].position.z);
                            }

                            //  ボス戦かどうかでVelocityを取得して設定
                            v = GetReverseVelocity(gamestatus);
                            velocity = v;
                            n = obj.GetComponent<NormalBullet>();
                            n.SetVelocity(velocity);
                        }
                        break;
                    case 3: //  レベル３
                        const int lv3BulletNum = 5; //  一度に出る弾の数
                        float Degree = 10;  //  角度

                        //  弾の数分のリストを確保
                        List<Transform> firePointLv3 = new List<Transform>(lv3BulletNum);
                        firePointLv3.Add(firePoint1.transform);
                        firePointLv3.Add(firePoint2_L.transform);
                        firePointLv3.Add(firePoint2_R.transform);
                        firePointLv3.Add(firePoint3_L.transform);
                        firePointLv3.Add(firePoint3_R.transform);

                        for(int i=0;i<firePointLv3.Count;i++)
                        {
                            //  弾を生成
                            obj = Instantiate(
                                normalBulletPrefab,
                                firePointLv3[i].position,
                                Quaternion.identity);

                            //  Yを反転するかどうか設定する
                            sr = obj.GetComponent<SpriteRenderer>();
                            sr.flipY = flipY;

                            //  反転時に座標を調整
                            if (!sr.flipY)
                            {
                                obj.transform.position =
                                    new Vector3(firePointLv3[i].position.x,
                                    firePointLv3[i].position.y + biasY,
                                    firePointLv3[i].position.z);
                            }

                            //  ボス戦かどうかでVelocityを取得
                            v = GetReverseVelocity(gamestatus);
                            //  端の弾だけ角度をつける
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
            //  ボタンを離した！
            if(shot.WasReleasedThisFrame())
            {
                chargeNormalShotCount = 0;
                normalShotChargeFlag = false;

                Debug.Log("通常弾強化OFF...");
            }
        }
    }

    //---------------------------------------------------
    //  通常弾のレベルアップ
    //---------------------------------------------------
    public void LevelupNormalShot()
    {
        if(normalShotLevel < (int)eNormalShotLevel.Lv3)normalShotLevel++;
    }


    //---------------------------------------------------
    //  通常弾のレベルダウン
    //---------------------------------------------------
    public void LeveldownNormalShot()
    {
        if(normalShotLevel > (int)eNormalShotLevel.Lv1)normalShotLevel--;
    }
    
    //-------------------------------------------
    //  魂バート弾
    //-------------------------------------------
    private void ConvertShot(bool flipY)
    {
        //  Sliderコンポーネントを取得
        Slider slider1 = sliderObj1.GetComponent<Slider>();
        Slider slider2 = sliderObj2.GetComponent<Slider>();

        //  Velocity格納用
        Vector3 v = Vector3.zero;

        switch(convertState)
        {
            case ConvertState.None:         //  無
                //  ゲージ１をリセット
                slider1.value = 0.0f;
                sliderObj1.SetActive(false);

                //  スライダー２をリセット
                slider2.value = 0f;
                sliderObj2.SetActive(false);
                break;
            case ConvertState.Restore:      // 溜め途中
                //  スライダー１有効化
                sliderObj1.SetActive(true);

                //  ゲージ１の値を増加させる
                if(1.0f <= slider1.value)
                {
                    slider1.value = 1.0f;
                    gaugeValue = 0.0f;

                    sliderObj2.SetActive(true);

                    //  溜めSE再生
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
            case ConvertState.ReleaseRestore:  //  不完全解放
                //  ゲージをリセット
                slider1.value = 0.0f;
                sliderObj1.SetActive(false);

                //  不発SE再生
                SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.SFX,
                    (int)SFXList.SFX_CONVERT_SHOT_FAIL);

                //  無にリセット
                gaugeValue = 0.0f;
                convertState = ConvertState.None;

                break;
            case ConvertState.MidPower:  // 中攻撃
                //  ゲージ２の値を増加させる
                if(1.0f <= slider2.value)
                {
                    slider2.value = 1.0f;

                    //  溜め完了SE再生
                    SoundManager.Instance.PlaySFX(
                        (int)AudioChannel.SFX_CONVERT_SHOT,
                        (int)SFXList.SFX_CONVERT_SHOT_GAUGE3);
                    convertState = ConvertState.FullPower;
                }
                else
                {
                    gaugeValue += gauge2Speed * Time.deltaTime;
                    slider2.value = gaugeValue;
                }
                break;

            case ConvertState.ReleaseMidPower:  // 中攻撃の時に離した

                canConvert = true; //  コンバート溜め可能

                //  スライダー１をリセット
                slider1.value = 0f;
                sliderObj1.SetActive(false);
                gaugeValue = 0.0f;
                slider1.value = gaugeValue;

                //  無にリセット
                convertState = ConvertState.None;

                break;
            case ConvertState.FullPower:        // 強攻撃
                break;
            case ConvertState.ReleaseFullPower:        // 強攻撃の時に離した

                canConvert = true; //  コンバート溜め可能

                //  スライダー２をリセット
                slider2.value = 0f;
                sliderObj2.SetActive(false);      

                //  スライダー１をリセット
                slider1.value = 0f;
                sliderObj1.SetActive(false);
                gaugeValue = 0.0f;

                //  無にリセット
                convertState = ConvertState.None;

                break;
        
        }


        if(canConvert)
        {
            //  押している間ゲージを増加させる
            if (shotConvert.IsPressed())
            {
                if(convertState == ConvertState.None)
                {
                    KonFieldAlphaAnimation(0.0f,0.784f,0.5f);
                    fieldObject.transform.DOScale(
                        new Vector3(6f,6f,6f),
                        0.5f);

                    //  溜めSE再生
                    SoundManager.Instance.PlaySFX(
                        (int)AudioChannel.SFX_CONVERT_SHOT,
                        (int)SFXList.SFX_CONVERT_SHOT_GAUGE1);

                    convertState = ConvertState.Restore;
                }
            }

            // ボタンを離した
            if (shotConvert.WasReleasedThisFrame())
            {
                KonFieldAlphaAnimation(0.784f,0.0f,0.5f);
                fieldObject.transform.DOScale(
                    new Vector3(3f,3f,3f),
                    0.5f);
                if(convertState == ConvertState.Restore)
                {
                    Debug.Log("溜め途中で離した！");

                    convertState = ConvertState.ReleaseRestore;
                } 
                else if(convertState == ConvertState.MidPower)
                {
                    Debug.Log("中攻撃で離した！");

                    canConvert = false; //  クールダウン

                    //  スライダー２をリセット
                    slider2.value = 0f;
                    sliderObj2.SetActive(false);

                    //  威力を中攻撃に設定
                    convertIsFullPower = false;

                    if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.DOUJI)
                    {
                        GenerateConvertDouji(flipY);
                    }
                    else if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.TSUKUMO)
                    {
                        GenerateConvertTsukumo(flipY);
                    }
                    else if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.KUCHINAWA)
                    {
                       GenerateConvertKuchinawa(flipY);
                    }
                    else if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.KURAMA)
                    {
                        GenerateConvertKurama(flipY);
                    }
                    else if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.WADATSUMI)
                    {
                        GenerateConvertWadatsumi(flipY);
                    }    
                    else if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.HAKUMEN)
                    {
                        GenerateConvertHakumen(flipY);
                    }

                }
                else if(convertState == ConvertState.FullPower)
                {
                    Debug.Log("強攻撃で離した！");
                    
                    //  威力を強攻撃に設定
                    convertIsFullPower = true;

                    if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.DOUJI)
                    {
                        GenerateConvertDouji(flipY);
                    }
                    else if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.TSUKUMO)
                    {
                        GenerateConvertTsukumo(flipY);
                    }
                    else if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.KUCHINAWA)
                    {
                       GenerateConvertKuchinawa(flipY);
                    }
                    else if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.KURAMA)
                    {
                        GenerateConvertKurama(flipY);
                    }
                    else if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.WADATSUMI)
                    {
                        GenerateConvertWadatsumi(flipY);
                    }    
                    else if(PlayerInfoManager.g_CONVERTSHOT == SHOT_TYPE.HAKUMEN)
                    {
                        GenerateConvertHakumen(flipY);
                    }

                }
            }
        }
    }

    //-----------------------------------------------------
    //  吸魂フィールドのアルファアニメーション
    //-----------------------------------------------------
    private void KonFieldAlphaAnimation(float start, float goal, float time)
    {
        //  すーっと出現する
        SpriteRenderer  sr = fieldObject.GetComponent<SpriteRenderer>();
        sr.enabled = true;
        var c = sr.color;
        c.a = start; // 初期値
        sr.color = c;

        DOTween.ToAlpha(
	        ()=> sr.color,
	        color => sr.color = color,
	        goal, // 目標値
	        time // 所要時間
        );
    }

    //-----------------------------------------------------
    //  ドウジの魂バート弾生成処理
    //-----------------------------------------------------
    private void GenerateConvertDouji(bool fripY)
    {
        canConvert = false; //  クールダウン

        //  魂バート弾生成
        GameObject obj = Instantiate(
            convertBulletPrefab[(int)PlayerInfoManager.g_CONVERTSHOT],
            this.transform.position,
            Quaternion.identity);

        //  強攻撃かどうかを設定
        obj.GetComponent<ConvertDoujiBullet>().SetFullPower(convertIsFullPower);

        //  減衰前の威力を設定
        if(convertIsFullPower)  //  強攻撃の時
        {
            //  初期威力を設定
            obj.GetComponent<ConvertDoujiBullet>().SetInitialPower(
                    convertShotPowerFull[(int)SHOT_TYPE.DOUJI]
                );
        }
        else　// 中攻撃の時
        {
            //  初期威力を設定
            obj.GetComponent<ConvertDoujiBullet>().SetInitialPower(
                    convertShotPowerHalf[(int)SHOT_TYPE.DOUJI]
                );
        }

        //  Yを反転するかどうか設定する
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        sr.flipY = fripY;

        if(convertIsFullPower)
        {
            convertState = ConvertState.ReleaseFullPower;
        }
        else
        {
            //  スケールを半分に
            obj.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
            convertState = ConvertState.ReleaseMidPower;
        }
    }

    //-----------------------------------------------------
    //  ツクモの魂バート弾生成処理
    //-----------------------------------------------------
    private void GenerateConvertTsukumo(bool fripY)
    {
        canConvert = false; //  クールダウン

        //  魂バート弾生成
        GameObject objL = Instantiate(
            convertBulletPrefab[(int)PlayerInfoManager.g_CONVERTSHOT],
            transform.position,
            Quaternion.identity);
        GameObject objR = Instantiate(
            convertBulletPrefab[(int)PlayerInfoManager.g_CONVERTSHOT],
            transform.position,
            Quaternion.identity);

        //  中攻撃か強攻撃かを設定
        objL.GetComponent<ConvertTsukumoBullet>().SetFullPower(convertIsFullPower);
        objL.GetComponent<ConvertTsukumoBullet>().SetIsL(true);
        objR.GetComponent<ConvertTsukumoBullet>().SetFullPower(convertIsFullPower);
        objR.GetComponent<ConvertTsukumoBullet>().SetIsL(false);

        //  Yを反転するかどうか設定する
        SpriteRenderer srL = objL.GetComponent<SpriteRenderer>(); 
        srL.flipY = fripY;
        SpriteRenderer srR = objR.GetComponent<SpriteRenderer>(); 
        srR.flipY = fripY;

        //  設定別処理
        if(convertIsFullPower)
        {
            convertState = ConvertState.ReleaseFullPower;
        }
        else
        {
            //  スケールを半分に
            objL.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
            objR.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
            convertState = ConvertState.ReleaseMidPower;
        }
    }

    //-----------------------------------------------------
    //  クチナワの魂バート弾生成処理
    //-----------------------------------------------------
    private void GenerateConvertKuchinawa(bool fripY)
    {
        //canConvert = false; //  クールダウン

        ////  魂バート弾生成
        //GameObject obj = Instantiate(
        //    convertBulletPrefab[(int)PlayerInfoManager.g_CONVERTSHOT],
        //    this.transform.position,
        //    Quaternion.identity);

        //obj.GetComponent<ConvertKuchinawaBullet>().SetFullPower(convertIsFullPower);
        ////  Yを反転するかどうか設定する
        //SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        //sr.flipY = fripY;

        //if(convertIsFullPower)
        //{
        //    convertState = ConvertState.ReleaseFullPower;
        //}
        //else
        //{
        //    //  スケールを半分に
        //    obj.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
        //    convertState = ConvertState.ReleaseMidPower;
        //}
    }

    //-----------------------------------------------------
    //  クラマの魂バート弾生成処理
    //-----------------------------------------------------
    private void GenerateConvertKurama(bool fripY)
    {
        //canConvert = false; //  クールダウン

        ////  魂バート弾生成
        //GameObject obj = Instantiate(
        //    convertBulletPrefab[(int)PlayerInfoManager.g_CONVERTSHOT],
        //    this.transform.position,
        //    Quaternion.identity);

        //obj.GetComponent<ConvertKuramaBullet>().SetFullPower(convertIsFullPower);
        ////  Yを反転するかどうか設定する
        //SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        //sr.flipY = fripY;

        //if(convertIsFullPower)
        //{
        //    convertState = ConvertState.ReleaseFullPower;
        //}
        //else
        //{
        //    //  スケールを半分に
        //    obj.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
        //    convertState = ConvertState.ReleaseMidPower;
        //}
    }

    //-----------------------------------------------------
    //  ツクモの魂バート弾生成処理
    //-----------------------------------------------------
    private void GenerateConvertWadatsumi(bool fripY)
    {
        //canConvert = false; //  クールダウン

        ////  魂バート弾生成
        //GameObject obj = Instantiate(
        //    convertBulletPrefab[(int)PlayerInfoManager.g_CONVERTSHOT],
        //    this.transform.position,
        //    Quaternion.identity);

        //obj.GetComponent<ConvertDoujiBullet>().SetFullPower(convertIsFullPower);
        ////  Yを反転するかどうか設定する
        //SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        //sr.flipY = fripY;

        //if(convertIsFullPower)
        //{
        //    convertState = ConvertState.ReleaseFullPower;
        //}
        //else
        //{
        //    //  スケールを半分に
        //    obj.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
        //    convertState = ConvertState.ReleaseMidPower;
        //}
    }

    //-----------------------------------------------------
    //  ツクモの魂バート弾生成処理
    //-----------------------------------------------------
    private void GenerateConvertHakumen(bool fripY)
    {
        //canConvert = false; //  クールダウン

        ////  魂バート弾生成
        //GameObject obj = Instantiate(
        //    convertBulletPrefab[(int)PlayerInfoManager.g_CONVERTSHOT],
        //    this.transform.position,
        //    Quaternion.identity);

        //obj.GetComponent<ConvertDoujiBullet>().SetFullPower(convertIsFullPower);
        ////  Yを反転するかどうか設定する
        //SpriteRenderer sr = obj.GetComponent<SpriteRenderer>(); 
        //sr.flipY = fripY;

        //if(convertIsFullPower)
        //{
        //    convertState = ConvertState.ReleaseFullPower;
        //}
        //else
        //{
        //    //  スケールを半分に
        //    obj.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
        //    convertState = ConvertState.ReleaseMidPower;
        //}
    }
}
