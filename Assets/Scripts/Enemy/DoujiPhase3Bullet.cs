using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//--------------------------------------------------------------
//
//  �h�E�W��Phase3�̒e�N���X
//
//--------------------------------------------------------------
public class DoujiPhase3Bullet : MonoBehaviour
{
    private int power;

    void Awake()
    {
        power = 0;
    }

    void Start()
    {
        //  �����e�̈ړ�
        StartCoroutine(BerserkBullet());
    }

    void Update()
    {

    }

    //-------------------------------------------------
    //  �����蔻��
    //-------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeadWall") ||
            collision.CompareTag("DeadWallBottom"))
        {
            Destroy(this.gameObject);
        }
    }


    //------------------------------------------------------
    //  �v���p�e�B
    //------------------------------------------------------
    public void SetPower(int p){ power = p; }
    public int GetPower(){ return power; }

    //------------------------------------------------------
    //  �����e�̈ړ�
    //------------------------------------------------------
    private IEnumerator BerserkBullet()
    {
        //  �A���t�@�A�j���[�V����
        float duration = 1.0f;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.enabled = true;
        Color c = sr.color;
        c.a = 0.0f; // �����l
        sr.color = c;

        DOTween.ToAlpha(
	        ()=> sr.color,
	        color => sr.color = color,
	        1.0f,       // �ڕW�l
	        duration    // ���v����
        );

        yield return new WaitForSeconds(duration);

        //  �ړ��J�n
        this.transform.DOMoveY(-20f,0.5f)
            .SetRelative(true)
            .SetEase(Ease.InOutQuint);

        //  SE���Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_ENEMY,
            (int)SFXList.SFX_DOUJI_SHOT2);

        yield return null;
    }
}
