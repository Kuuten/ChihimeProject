using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using Live2D.Cubism.Framework.Json;

//--------------------------------------------------------------
//
//  お金管理クラス
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

        //  PlayerInfoManagerから情報をセット
        PlayerInfoManager.g_KONNUM = money;
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
        targetMoney += value;
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

    //------------------------------------------------
    //  プロパティ
    //------------------------------------------------
    public int GetKonNumGainedFromLarge(){ return konNumGainedFromLarge; }
    public int GetKonNumGainedFromSmall(){ return konNumGainedFromSmall; }
    public int GetKonNumGainedFromPowerup(){ return konNumGainedFromPowerup; }
}
