using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �c�N����Phase2�̒e�N���X
//
//--------------------------------------------------------------
public class TsukumoPhase2Bullet : MonoBehaviour
{
    private int power;

    public enum Direction
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT,

        MAX
    }
    private Direction direction;

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
    public void SetDirection(Direction d) { this.direction = d; }

    //---------------------------------------------------------------
    //  �e���ړ����Ă���
    //---------------------------------------------------------------
    public IEnumerator BulletMove()
    {
        float duration = 3.0f;  //  �A�j���[�V��������

        //  ������ݒ�
        Destroy(gameObject, duration);

        //  SE���Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.ENEMY_SHOT,
            (int)SFXList.SFX_ENEMY_SHOT);

        if(direction == Direction.TOP)
        {
            //  Y���ړ�
            transform.DOMoveY(-20f,duration);
        }
        else if(direction == Direction.BOTTOM)
        {
            //  Y���ړ�
            transform.DOMoveY(20f,duration);
        }
        else if(direction == Direction.LEFT)
        {
            //  X���ړ�
            transform.DOMoveX(20f,duration);
        }
        else if(direction == Direction.RIGHT)
        {
            //  X���ړ�
            transform.DOMoveX(-20f,duration);
        }

        yield return null;
    }
}
