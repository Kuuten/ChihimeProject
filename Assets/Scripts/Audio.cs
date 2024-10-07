using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  オーディオ調整のクラス
//
//--------------------------------------------------------------
public class Audio : MonoBehaviour
{
    //  Audioミキサーを入れるとこです
    [SerializeField] AudioMixer audioMixer;

    //  それぞれのスライダーを入れるとこです。。
    [SerializeField] Slider BGMSlider;
    [SerializeField] Slider SESlider;

    private void Start()
    {
        //  スライダーのvolumeにミキサーグループのvolumeを入れてます。
        float seSliderMaxRange = 90f; //  SEスライダーの最大範囲


        /*　仮処理　後でJSONファイルを読み込んで適用させるように変更する　*/
        audioMixer.SetFloat("CONVERT_SHOT", 0);
        audioMixer.SetFloat("SFX", -10);
        audioMixer.SetFloat("SFX_ENEMY", -20f);
        audioMixer.SetFloat("ENEMY_SHOT", -10);
        audioMixer.SetFloat("SFX_DAMAGE", -10);
        audioMixer.SetFloat("SFX_SYSTEM", -10);
        audioMixer.SetFloat("SFX_SYSTEM2", 0);
        audioMixer.SetFloat("NORMAL_SHOT", -20);
        audioMixer.SetFloat("BOMB", 0);
        audioMixer.SetFloat("SFX_LOOP", -20);

        //  BGM
        audioMixer.GetFloat("BGM", out float bgmVolume);
        BGMSlider.value = bgmVolume;
        //  SE(CONVERT_SHOTを基準とする)
        audioMixer.GetFloat("SFX", out float seVolume);
        SESlider.value = Mathf.Abs(seSliderMaxRange/seVolume);
    }

    public void SetBGM()
    {
        audioMixer.SetFloat("BGM", BGMSlider.value);
    }

    public void SetSE()
    {
        //  ミキサーの最小音量
        float mixerMinVlaue = -80f;
        //  スライダーの変化量
        float plusValue = (SESlider.maxValue - SESlider.minValue);
        //  SFXの基準値
        float baseValue = mixerMinVlaue + plusValue * SESlider.value;

        //  CONVERT_SHOTを基準とする
        audioMixer.SetFloat("CONVERT_SHOT", baseValue+10);
        audioMixer.SetFloat("SFX", baseValue);
        audioMixer.SetFloat("SFX_ENEMY", baseValue-10f);
        audioMixer.SetFloat("ENEMY_SHOT", baseValue);
        audioMixer.SetFloat("SFX_DAMAGE", baseValue);
        audioMixer.SetFloat("SFX_SYSTEM", baseValue);
        audioMixer.SetFloat("SFX_SYSTEM2", baseValue +10);
        audioMixer.SetFloat("NORMAL_SHOT", baseValue -10f);
        audioMixer.SetFloat("BOMB", baseValue +10);
        audioMixer.SetFloat("SFX_LOOP", baseValue -10f);

    }
}