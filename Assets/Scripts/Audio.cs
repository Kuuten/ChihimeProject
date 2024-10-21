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
    [SerializeField] Slider SESlider;

    //  それぞれの初期値
    const float initialBGMVolume = 6.0f;
    const float initialSFXVolume = 6.0f;

    //  値の範囲
    const float AudioMixerMinVolume = -80f;
    const float AudioMixerVolumeRange = 90f;
    const float SliderValueRange = 10f;

    // 上書き情報の保存先
    private string BGM_savePath = "bgm_config.json";
    private string SFX_savePath = "sfx_config.json";

    private void Start()
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

    public void SetSE()
    {
        //  0～10(スライダー)の値を-80～0（AudioMixer）の値にする
        float base_value = AudioMixerMinVolume + 
            (AudioMixerVolumeRange/SliderValueRange) * SESlider.value;

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
        //  SESlider.valueをJson形式で保存

        //  スライダーの値をstring型にして格納
        string data = SESlider.value.ToString();

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

            return;
        }

         Debug.Log("bgm_config.jsonが見つかりました！\n" +
                "ロードします。");

        var json = File.ReadAllText(path);

        // JSONファイルの情報をスライダーに設定
        BGMSlider.value = int.Parse(json);

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
            SESlider.value = initialSFXVolume;
            return;
        }

         Debug.Log("sfx_config.jsonが見つかりました！\n" +
                "ロードします。");

        var json = File.ReadAllText(path);

        // JSONファイルの情報をスライダーに設定
        SESlider.value = int.Parse(json);

        //  AudioMixerに反映
        SetBGM();

        Debug.Log("ロード完了"); 
    }
}