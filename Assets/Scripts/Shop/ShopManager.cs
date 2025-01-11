using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  ショップシーン管理クラス(買い物処理)
//
//--------------------------------------------------------------
public class ShopManager : MonoBehaviour
{
    //  ヒエラルキーのテキストクラス
    [SerializeField] private TextMeshProUGUI shopText;

    //  通常テキスト
    private readonly string normalText = 
        "まいど！神蔵屋やで～。\nええもん安なってるから買ってってや～！";
    //  買い物失敗時テキスト
    private readonly string failedText = "魂が足らへんで～！";
    //  買い物成功時テキスト
    private readonly string successText = "おおきに！";

    bool canContorol;   //  操作可能フラグ

    //  生成されるボタンのオブジェクト
    private GameObject redheartButton,doubleupHeartButton,goldheartButton,bombButton,
        PowerupButton,SpeedupButton,ShieldButton;

    //  再入荷ボタンオブジェクト
    [SerializeField] private GameObject regenerateButton;

    //  完売御礼オブジェクトの子オブエクトID
    private static readonly int soldout_id = 4;

    //  アイテムの値段をボタンから取得する用
    private string RedHeartValueText;
    private string DoubleupHeartValueText;
    private string GoldHeartValueText;
    private string BombValueText;
    private string PowerupValueText;
    private string SpeedupValueText;
    private string ShieldValueText;

    void Start()
    {
        StartCoroutine(StartInit());
    }

    //--------------------------------------------------------------
    //  初期化コルーチン
    //--------------------------------------------------------------
    IEnumerator StartInit()
    {
        //  ショップBGM再生
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_SHOP);

        //  操作可能にする
        canContorol = true;

        yield return null;
    }

    void Update()
    {
        //  操作不能ならリターン
        if(!canContorol)return;
    }

    //--------------------------------------------------------------
    //  メッセージウィンドウを出す処理
    //--------------------------------------------------------------
    IEnumerator DisplayMessage(string msg)
    {
        //  メッセージオブジェクトを表示
        shopText.text = msg;

        //  １秒待つ
        yield return new WaitForSeconds(2);

        //  通常テキストに戻す
        shopText.text = normalText;
    }
    
    //--------------------------------------------------------------
    //  失敗メッセージウィンドウを出す処理
    //--------------------------------------------------------------
    public IEnumerator DisplayFailedMessage()
    {
        //  メッセージオブジェクトを表示
        shopText.text = failedText;

        //  １秒待つ
        yield return new WaitForSeconds(2);

        //  通常テキストに戻す
        shopText.text = normalText;
    }

    //--------------------------------------------------------------
    //  赤いハートボタンが押された時の処理
    //--------------------------------------------------------------
    public void OnRedHeartButtonDown()
    {
        //  ボタンのテキストから直接代金を取得
        int value = int.Parse(RemoveKonText(RedHeartValueText));

        Debug.Log($"赤いハートの値段は{value}です");

        //  回復量
        int heal_value = 2;

        //  購入可能なら代金分魂を減らす
        if(MoneyManager.Instance.CanBuyItem(value))
        { 
            //  体力をハート１個分増やす
            PlayerHealth ph = GameManager.Instance.GetPlayer().GetComponent<PlayerHealth>();
            ph.Heal(heal_value);

            //  ButtonのInterctiveを無効化
            redheartButton.GetComponent<Button>().interactable = false;

            //  完売御礼を表示(ボタンオブジェクトからID４の子オブジェクト)
            redheartButton.transform.GetChild(soldout_id).gameObject.SetActive(true);

            //  再入荷ボタンオブジェクトを選択状態にする
            EventSystem.current.SetSelectedGameObject(regenerateButton);

            //  メッセージ表示
            StartCoroutine(DisplayMessage(successText));

            //  現在体力を取得
            int health = ph.GetCurrentHealth();
            Debug.Log($"体力が{heal_value}回復して{health}になった！");
        }
        else
        {
            //  メッセージ表示
            StartCoroutine(DisplayMessage(failedText));
        }

        
    }
    //--------------------------------------------------------------
    //  ダブルアップハートボタンが押された時の処理
    //--------------------------------------------------------------
    public void OnDoubleupHeartButtonDown()
    {
        //  ボタンのテキストから直接代金を取得
        int value = int.Parse(RemoveKonText(DoubleupHeartValueText));

        Debug.Log($"ダブルアップハートの値段は{value}です");

        //  回復量
        int heal_value = 0;

        //  50%の確率で追加回復
        int rand = Random.Range(1,101);

        //  購入可能なら代金分魂を減らす
        if(MoneyManager.Instance.CanBuyItem(value))
        { 
            if(rand % 2 == 0)   //  追加回復
            {
                heal_value = 4;
            }
            else // １だけ回復
            {
                heal_value = 2;
            }

            //  体力を増やす
            PlayerHealth ph = GameManager.Instance.GetPlayer().GetComponent<PlayerHealth>();
            ph.Heal(heal_value);

            //  ButtonのInterctiveを無効化
            doubleupHeartButton.GetComponent<Button>().interactable = false;

            //  完売御礼を表示(ボタンオブジェクトからID４の子オブジェクト)
            doubleupHeartButton.transform.GetChild(soldout_id).gameObject.SetActive(true);

            //  再入荷ボタンオブジェクトを選択状態にする
            EventSystem.current.SetSelectedGameObject(regenerateButton);

            //  メッセージ表示
            StartCoroutine(DisplayMessage(successText));

            //  現在体力を取得
            int health = ph.GetCurrentHealth();
            Debug.Log($"体力が{heal_value}回復して{health}になった！");
        }
        else
        {
            //  メッセージ表示
            StartCoroutine(DisplayMessage(failedText));
        }
    }
    //--------------------------------------------------------------
    //  金色のハートボタンが押された時の処理
    //--------------------------------------------------------------
    public void OnGoldHeartButtonDown()
    {
        //  ボタンのテキストから直接代金を取得
        int value = int.Parse(RemoveKonText(GoldHeartValueText));;

        Debug.Log($"金色のハートの値段は{value}です");

        //  ハート１個分の体力
        int one_heart = int.Parse(RemoveKonText(GoldHeartValueText));

        //  購入可能なら代金分魂を減らす
        if(MoneyManager.Instance.CanBuyItem(value))
        { 
            //  最大体力をハート１個分増やす
            GameManager.Instance.GetPlayer().GetComponent<PlayerHealth>()
                .IncreaseHP(one_heart);

            //  最大体力を取得
            PlayerHealth ph = GameManager.Instance.GetPlayer().GetComponent<PlayerHealth>();
            int limit_health = ph.GetCurrentMaxHealth();
            
            //  体力を全回復
            GameManager.Instance.GetPlayer().GetComponent<PlayerHealth>()
                .Heal(limit_health);

            //  ButtonのInterctiveを無効化
            goldheartButton.GetComponent<Button>().interactable = false;

            //  完売御礼を表示(ボタンオブジェクトからID４の子オブジェクト)
            goldheartButton.transform.GetChild(soldout_id).gameObject.SetActive(true);

            //  再入荷ボタンオブジェクトを選択状態にする
            EventSystem.current.SetSelectedGameObject(regenerateButton);

            //  メッセージ表示
            StartCoroutine(DisplayMessage(successText));

            //  現在体力を取得
            int health = ph.GetCurrentHealth();
            Debug.Log($"最大体力が2増えて全回復した！現在体力:{health}");
        }
        else
        {
            //  メッセージ表示
            StartCoroutine(DisplayMessage(failedText));
        }
    }
    //--------------------------------------------------------------
    //  骨Gのボムボタンが押された時の処理
    //--------------------------------------------------------------
    public void OnHoneGBombButtonDown()
    {
        //  ボタンのテキストから直接代金を取得
        int value = int.Parse(RemoveKonText(BombValueText));

        Debug.Log($"ボムの値段は{value}です");

        //  購入可能なら代金分魂を減らす
        if(MoneyManager.Instance.CanBuyItem(value))
        { 
            PlayerBombManager.Instance.AddBomb();

            //  ButtonのInterctiveを無効化
            bombButton.GetComponent<Button>().interactable = false;

            //  完売御礼を表示(ボタンオブジェクトからID４の子オブジェクト)
            bombButton.transform.GetChild(soldout_id).gameObject.SetActive(true);

            //  再入荷ボタンオブジェクトを選択状態にする
            EventSystem.current.SetSelectedGameObject(regenerateButton);

            //  メッセージ表示
            StartCoroutine(DisplayMessage(successText));
        }
        else
        {
            //  メッセージ表示
            StartCoroutine(DisplayMessage(failedText));
        }
    }
    //--------------------------------------------------------------
    //  ショット強化ボタンが押された時の処理
    //--------------------------------------------------------------
    public void OnPowerupButtonDown()
    {
        //  ボタンのテキストから直接代金を取得
        int value = int.Parse(RemoveKonText(PowerupValueText));

        Debug.Log($"ショット強化の値段は{value}です");

        //  購入可能なら代金分魂を減らす
        if(MoneyManager.Instance.CanBuyItem(value))
        { 
            //  通常ショットのレベルアップ
            PlayerShotManager.Instance.LevelupNormalShot();
            PlayerShotManager.Instance.UpdateShotPowerIcon();

            //  ButtonのInterctiveを無効化
            PowerupButton.GetComponent<Button>().interactable = false;

            //  完売御礼を表示(ボタンオブジェクトからID４の子オブジェクト)
            PowerupButton.transform.GetChild(soldout_id).gameObject.SetActive(true);

            //  再入荷ボタンオブジェクトを選択状態にする
            EventSystem.current.SetSelectedGameObject(regenerateButton);

            //  メッセージ表示
            StartCoroutine(DisplayMessage(successText));
        }
        else
        {
            //  メッセージ表示
            StartCoroutine(DisplayMessage(failedText));
        }

    }
    //--------------------------------------------------------------
    //  スピード強化ボタンが押された時の処理
    //--------------------------------------------------------------
    public void OnSpeedupButtonDown()
    {
        //  ボタンのテキストから直接代金を取得
        int value = int.Parse(RemoveKonText(SpeedupValueText));

        Debug.Log($"スピード強化の値段は{value}です");

        //  購入可能なら代金分魂を減らす
        if(MoneyManager.Instance.CanBuyItem(value))
        { 
            //  スピードのレベルアップ
            PlayerMovement.Instance.LevelupMoveSpeed();
            PlayerMovement.Instance.UpdateSpeedLevelIcon();

            //  ButtonのInterctiveを無効化
            SpeedupButton.GetComponent<Button>().interactable = false;

            //  完売御礼を表示(ボタンオブジェクトからID４の子オブジェクト)
            SpeedupButton.transform.GetChild(soldout_id).gameObject.SetActive(true);

            //  再入荷ボタンオブジェクトを選択状態にする
            EventSystem.current.SetSelectedGameObject(regenerateButton);

            //  メッセージ表示
            StartCoroutine(DisplayMessage(successText));
        }
        else
        {
            //  メッセージ表示
            StartCoroutine(DisplayMessage(failedText));
        }

    }
    //--------------------------------------------------------------
    //  シールドボタンが押された時の処理
    //--------------------------------------------------------------
    public void OnShieldButtonDown()
    {
        //  ボタンのテキストから直接代金を取得
        int value = int.Parse(RemoveKonText(ShieldValueText));

        Debug.Log($"シールドの値段は{value}です");

        //  購入可能なら代金分魂を減らす
        if(MoneyManager.Instance.CanBuyItem(value))
        {
            // プレイヤーにシールドを追加する 
            int hp =  PlayerHealth.Instance.GetCurrentHealth();
            PlayerHealth.Instance.SetIsShielded(true);
            PlayerHealth.Instance.CalculateHealthUI(hp);

            //  ButtonのInterctiveを無効化
            ShieldButton.GetComponent<Button>().interactable = false;

            //  完売御礼を表示(ボタンオブジェクトからID４の子オブジェクト)
            ShieldButton.transform.GetChild(soldout_id).gameObject.SetActive(true);

            //  再入荷ボタンオブジェクトを選択状態にする
            EventSystem.current.SetSelectedGameObject(regenerateButton);

            //  メッセージ表示
            StartCoroutine(DisplayMessage(successText));

            //  シールドが付与されているかどうかを取得
            //bool canShield = PlayerHealth.Instance.GetBombNum();
            Debug.Log($"シールドが付与された！");
        }
        else
        {
            //  メッセージ表示
            StartCoroutine(DisplayMessage(failedText));
        }

    }

    //--------------------------------------------------------------
    //  文字列から魂を取り除く
    //--------------------------------------------------------------
    private string RemoveKonText(string s)
    {
        return s.Replace("魂","");
    }

    //--------------------------------------------------------------
    //  プロパティ
    //--------------------------------------------------------------
    public void SetRedHeartButton(GameObject obj){ redheartButton = obj; }
    public void SetDoubleupHeartButton(GameObject obj){ doubleupHeartButton = obj; }
    public void SetGoldHeartButton(GameObject obj){ goldheartButton = obj; }
    public void SetBombButton(GameObject obj){ bombButton = obj; }
    public void SetPowerupButton(GameObject obj){ PowerupButton = obj; }
    public void SetSpeedupButton(GameObject obj){ SpeedupButton = obj; }
    public void SetShieldButton(GameObject obj){ ShieldButton = obj; }

    public void SetRedHeartValueText(string s){ RedHeartValueText = s; }
    public void SetDoubleupValueText(string s){ DoubleupHeartValueText = s; }
    public void SetGoldHeartValueText(string s){ GoldHeartValueText = s; }
    public void SetBombValueText(string s){ BombValueText = s; }
    public void SetPowerupValueText(string s){ PowerupValueText = s; }
    public void SetSpeedupValueText(string s){ SpeedupValueText = s; }
    public void SetShieldValueText(string s){ ShieldValueText = s; }
}
