using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �h�E�W��Phase2�̒e�N���X
//
//--------------------------------------------------------------
public class DoujiPhase2Bullet : MonoBehaviour
{
    private int power;

    public enum KooniDirection
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT,

        MAX
    }
    private KooniDirection direction;

    void Start()
    {

    }

    private void OnDestroy()
    {
        
    }

    void Update()
    {
        
    }

    //---------------------------------------------------------------
    //  �v���p�e�B
    //---------------------------------------------------------------
    public void SetPower(int p){ power = p; }
    public int GetPower() { return power; }
    public void SetDirection(KooniDirection d) { this.direction = d; }

    //---------------------------------------------------------------
    //  �q�S���ˌ�����
    //---------------------------------------------------------------
    public IEnumerator KooniRush()
    {
        float duration = 3.0f;  //  �A�j���[�V��������

        //  �h�炷
        transform.DOPunchScale(Vector3.one * 0.98f, duration, 5, 0.01f)
            .SetLoops(-1, LoopType.Incremental);

        //  ������ݒ�
        Destroy(gameObject, duration);

        //  SE���Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.ENEMY_SHOT,
            (int)SFXList.SFX_DOUJI_KOONIRUSH);

        if(direction == KooniDirection.TOP)
        {
            //  Y���ړ�
            transform.DOMoveY(-20f,duration);
        }
        else if(direction == KooniDirection.BOTTOM)
        {
            //  Y���ړ�
            transform.DOMoveY(20f,duration);
        }
        else if(direction == KooniDirection.LEFT)
        {
            //  X���ړ�
            transform.DOMoveX(20f,duration);
        }
        else if(direction == KooniDirection.RIGHT)
        {
            //  X���ړ�
            transform.DOMoveX(-20f,duration);
        }

        yield return null;
    }
}
