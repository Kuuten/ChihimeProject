using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

//--------------------------------------------------------------
//
//  コンフィグ画面のオーディオ調整のクラス
//
//--------------------------------------------------------------
public class Audio : MonoBehaviour
{
    //  Audioミキサーを入れるとこ
    [SerializeField] AudioMixer audioMixer;

    //  それぞれのスライダーを入れるとこ
    [SerializeField] Slider BGMSlider;
    [SerializeField] Slider SFXSlider;

    //  変更前のコンフィグに入った瞬間の値を保存する変数
    float preBGMVolume;
    float preSFXVolume;

    //  それぞれの初期値
    static readonly float initialBGMVolume = 6.0f;
    static readonly float initialSFXVolume = 6.0f;

    //  値の範囲
    static readonly float AudioMixerMinVolume = -80f;
    static readonly float AudioMixerVolumeRange = 90f;
    static readonly float SliderValueRange = 10f;

    // 上書き情報の保存先
    private string BGM_savePath = "bgm_config.json";
    private string SFX_savePath = "sfx_config.json";

    private void Start()
    {
        //  初期化
        Init();
    }

    //---------------------------------------------------------------------
    //  初期化
    //---------------------------------------------------------------------
    public void Init()
    {
        //  AudioMixerのBGMのボリューム値を取得してスライダーに設定
        LoadBGM();

        //  AudioMixerのCONVERT_SHOTのボリューム値を取得してスライダーに設定
        LoadSFX();
    }

    //---------------------------------------------------------------------
    //  スライダーの値を変換してAudioMixerに適用する
    //---------------------------------------------------------------------
    public void SetBGM()
    {
        //  0～10(スライダー)の値を-80～0（AudioMixer）の値にする
        float value = AudioMixerMinVolume + 
            (AudioMixerVolumeRange/SliderValueRange) * BGMSlider.value;

        audioMixer.SetFloat("BGM", value);
    }

    public void SetBGM(float base_value)
    {
        //  0～10(スライダー)の値を-80～0（AudioMixer）の値にする
        float value = AudioMixerMinVolume + 
            (AudioMixerVolumeRange/SliderValueRange) * base_value;

        audioMixer.SetFloat("BGM", value);
    }

    public void SetSFX()
    {
        //  0～10(スライダー)の値を-80～0（AudioMixer）の値にする
        float base_value = AudioMixerMinVolume + 
            (AudioMixerVolumeRange/SliderValueRange) * SFXSlider.value;

        //  CONVERT_SHOTを基準とする
        audioMixer.SetFloat("CONVERT_SHOT", base_value);
        audioMixer.SetFloat("SFX", base_value-10);
        audioMixer.SetFloat("SFX_ENEMY", base_value-20f);
        audioMixer.SetFloat("ENEMY_SHOT", base_value-10);
        audioMixer.SetFloat("SFX_DAMAGE", base_value-10);
        audioMixer.SetFloat("SFX_SYSTEM", base_value-10);
        audioMixer.SetFloat("SFX_SYSTEM2", base_value);
        audioMixer.SetFloat("NORMAL_SHOT", base_value-20);
        audioMixer.SetFloat("BOMB", base_value);
        audioMixer.SetFloat("SFX_LOOP", base_value-20);
    }

    public void SetSFX(float baseValue)
    {
        //  0～10(スライダー)の値を-80～0（AudioMixer）の値にする
        float base_value = AudioMixerMinVolume + 
            (AudioMixerVolumeRange/SliderValueRange) * baseValue;

        //  CONVERT_SHOTを基準とする
        audioMixer.SetFloat("CONVERT_SHOT", base_value);
        audioMixer.SetFloat("SFX", base_value-10);
        audioMixer.SetFloat("SFX_ENEMY", base_value-20f);
        audioMixer.SetFloat("ENEMY_SHOT", base_value-10);
        audioMixer.SetFloat("SFX_DAMAGE", base_value-10);
        audioMixer.SetFloat("SFX_SYSTEM", base_value-10);
        audioMixer.SetFloat("SFX_SYSTEM2", base_value);
        audioMixer.SetFloat("NORMAL_SHOT", base_value-20);
        audioMixer.SetFloat("BOMB", base_value);
        audioMixer.SetFloat("SFX_LOOP", base_value-20);
    }

    //---------------------------------------------------------------------
    //  セーブ用メソッド(Json形式で保存する)
    //---------------------------------------------------------------------
    public void SaveBGM()
    {
        //  BGMSlider.valueをJson形式で保存

        //  スライダーの値をstring型にして格納
        string data = BGMSlider.value.ToString();

        // ファイルに保存
        var path = Path.Combine(Application.persistentDataPath, BGM_savePath);
        File.WriteAllText(path, data);
    }

    public void SaveSFX()
    {
        //  SFXSlider.valueをJson形式で保存

        //  スライダーの値をstring型にして格納
        string data = SFXSlider.value.ToString();

        // ファイルに保存
        var path = Path.Combine(Application.persistentDataPath, SFX_savePath);
        File.WriteAllText(path, data);
    }

    //---------------------------------------------------------------------
    //  ロード用メソッド(タイトルでコンフィグが押された時に実行する)
    //---------------------------------------------------------------------
    public void LoadBGM()
    {
        // ファイルから読み込み
        var path = Path.Combine(Application.persistentDataPath, BGM_savePath);
        if (!File.Exists(path))
        {
            Debug.Log("bgm_config.jsonが見つかりませんでした。\n" +
                "このまま続行します。");
            
            //  とりあえず規定値を設定
            BGMSlider.value = initialBGMVolume;
        }
        else
        {
             Debug.Log("bgm_config.jsonが見つかりました！\n" +
                    "ロードします。");

            var json = File.ReadAllText(path);

            // JSONファイルの情報をスライダーに設定
            BGMSlider.value = int.Parse(json);
        }
        
        //  この時点の値を保存
        preBGMVolume = BGMSlider.value;

        //  AudioMixerに反映
        SetBGM();

        Debug.Log("ロード完了"); 
    }

    public void LoadSFX()
    {
        // ファイルから読み込み
        var path = Path.Combine(Application.persistentDataPath, SFX_savePath);
        if (!File.Exists(path))
        {
            Debug.Log("sfx_config.jsonが見つかりませんでした。\n" +
                "このまま続行します。");

            //  とりあえず規定値を設定
            SFXSlider.value = initialSFXVolume;
        }
        else
        {
             Debug.Log("sfx_config.jsonが見つかりました！\n" +
                    "ロードします。");

            var json = File.ReadAllText(path);

            // JSONファイルの情報をスライダーに設定
            SFXSlider.value = int.Parse(json);
        }

        //  この時点の値を保存
        preSFXVolume = SFXSlider.value;

        //  AudioMixerに反映
        SetSFX();

        Debug.Log("ロード完了"); 
    }

    //---------------------------------------------------------------------
    //  サウンドコンフィグでキャンセルが押された時の処理
    //---------------------------------------------------------------------
    public void OnCancelButtonDown()
    {
        //  初期化
        Init();
    }
}