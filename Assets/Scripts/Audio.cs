using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  �I�[�f�B�I�����̃N���X
//
//--------------------------------------------------------------
public class Audio : MonoBehaviour
{
    //  Audio�~�L�T�[������Ƃ��ł�
    [SerializeField] AudioMixer audioMixer;

    //  ���ꂼ��̃X���C�_�[������Ƃ��ł��B�B
    [SerializeField] Slider BGMSlider;
    [SerializeField] Slider SESlider;

    private void Start()
    {
        //  �X���C�_�[��volume�Ƀ~�L�T�[�O���[�v��volume�����Ă܂��B
        float seSliderMaxRange = 90f; //  SE�X���C�_�[�̍ő�͈�


        /*�@�������@���JSON�t�@�C����ǂݍ���œK�p������悤�ɕύX����@*/
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
        //  SE(CONVERT_SHOT����Ƃ���)
        audioMixer.GetFloat("SFX", out float seVolume);
        SESlider.value = Mathf.Abs(seSliderMaxRange/seVolume);
    }

    public void SetBGM()
    {
        audioMixer.SetFloat("BGM", BGMSlider.value);
    }

    public void SetSE()
    {
        //  �~�L�T�[�̍ŏ�����
        float mixerMinVlaue = -80f;
        //  �X���C�_�[�̕ω���
        float plusValue = (SESlider.maxValue - SESlider.minValue);
        //  SFX�̊�l
        float baseValue = mixerMinVlaue + plusValue * SESlider.value;

        //  CONVERT_SHOT����Ƃ���
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