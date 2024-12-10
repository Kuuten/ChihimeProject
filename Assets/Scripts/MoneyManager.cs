using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using Live2D.Cubism.Framework.Json;

//--------------------------------------------------------------
//
//  お金管理クラス(８ケタまで表示)
//
//--------------------------------------------------------------
public class MoneyManager : MonoBehaviour
{
    //  魂に表示されるテキスト
    [SerializeField] private TextMeshProUGUI scoreText;
    private int money;
    private int targetMoney;
    private bool countFlag;

    //  大魂で得られる魂の数
    private const int konNumGainedFromLarge = 500;
    //  小魂で得られる魂の数
    private const int konNumGainedFromSmall = 100;
    //  最大強化時に強化アイテムで得られる魂の数
    private const int konNumGainedFromPowerup = 300;

    //  シングルトンなインスタンス
    public static MoneyManager Instance
    {
        get; private set;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    void Start()
    {
        scoreText.text = "" + money;

        //  PlayerInfoManagerから情報をセット
        money = PlayerInfoManager.g_KONNUM;
        targetMoney = money;
        countFlag = true;
    }

    void Update()
    {
        //  countFlagがtrueならアニメーション開始
        if(countFlag)StartCoroutine(CountMoney());

        //  お金のテキストを更新
        scoreText.text = $"{money}";
    }

    //  魂（お金）を加算
    public void AddMoney(int value )
    {
        //money += value;
        //scoreText.text = "" + money;

        //  目標値を更新
        if(targetMoney >= 99999999)
        {
            targetMoney = 99999999;
        }
        else targetMoney += value;
    }

    //  魂（お金）を減算
    public void SubMoney(int value )
    {
        //  目標値を更新
        if(targetMoney <= 0)
        {
            targetMoney = 0;
        }
        else targetMoney -= value;
    }

    private IEnumerator CountMoney()
    {
        //  フラグをfalseに
       countFlag = false;

       yield return StartCoroutine(CountAnimation());
    }

    private IEnumerator CountAnimation()
    {
        // 数値の変更
        DOTween.To(
            () => money,          // 何を対象にするのか
            num => money = num,   // 値の更新
            targetMoney,          // 最終的な値
            1.0f                  // アニメーション時間
        )
            .OnComplete(() =>
            {
                //  フラグをリセット
                countFlag = true;
            });

        yield return null;
    }

    //-------------------------------------------------------------------------------
    //  プロパティ
    //-------------------------------------------------------------------------------
    public int GetKonNumGainedFromLarge(){ return konNumGainedFromLarge; }
    public int GetKonNumGainedFromSmall(){ return konNumGainedFromSmall; }
    public int GetKonNumGainedFromPowerup(){ return konNumGainedFromPowerup; }
    public int GetKonNum(){ return money; }
    public void SetKonNum(int num){ money = num; }

    //-------------------------------------------------------------------------------
    //  残金でアイテムを購入可能かどうかを判定する(value:アイテムの値段)
    //-------------------------------------------------------------------------------
    public bool CanBuyItem(int value)
    {
        int kon = targetMoney - value;

        //  お金が足りなかったら
        if(kon < 0)
        {
            //  SEを再生
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_TITLE_INCORRECT);

            return false;
        }
        //  購入成功！
        else
        {
            //  SEを再生
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_RESULT_CASH);

            //  お金を減らす
            SubMoney(value);

            return true;
        }
    }
}
