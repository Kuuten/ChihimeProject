using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static EnemyManager;

//--------------------------------------------------------------
//
//  �c�N���̃z�[�~���O�e�N���X
//
//--------------------------------------------------------------
public class TsukumoHomingBullet : MonoBehaviour
{
    //  �O������e�̃x�N�g���ƈړ��X�s�[�h���擾���A�ړ����邾��

    private Vector3 velocity;
    private float speed;
    private int power;

    private int step;

    private static readonly float radius = 1.0f;
    [SerializeField] Vector3 controlPos;

    //  �J��Ԃ�����(�b)
    private float timer = 3;

    // ��]��
    [SerializeField] private Vector3 axis = Vector3.forward;

    // �~�^������
    [SerializeField] private float period = 2;

    //  �ڕW�ԍ����o�p
    private int targetNum;
    
    void Awake()
    {
        step = 0;
        speed = 0f;
        power = 0;
        velocity = Vector3.zero;
        controlPos = Vector3.zero;
        targetNum = 0;
    }

    private void Start()
    {
        //  �����_���ȃR���g���[���|�C���g�ɔ��
        FlyToRandomControlPoint();
    }

    void Update()
    {
        switch(step)
        {
            case 0: //  �R���g���[���|�C���g�֋߂Â�
                CaculateDistance();
                //  ���W���X�V
                transform.position += speed * velocity * Time.deltaTime;
                break;
            case 1: //  ��]���Ȃ���I�u�W�F�N�g���ς��
                RorateAroundControlPoint();
                break;
            case 2:
                //  ���W���X�V
                transform.position += speed * velocity * Time.deltaTime;
                break;
        }


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
    public void SetVelocity(Vector3 v){ velocity = v; }
    public void SetSpeed(float s){ speed = s; }
    public void SetPower(int p){ power = p; }
    public int GetPower(){ return power; }
    public void SetTargetNum(int n){ targetNum = n; }
    public int GetTargetNum(){ return targetNum; }

    //------------------------------------------------------
    //  �ڕW�n�_�Ƃ̋����𑪂�
    //------------------------------------------------------
    private void CaculateDistance()
    {
        //  �������v�Z����
        float distance = Vector3.Distance( controlPos, transform.position ); 

        //  ��苗���ɂȂ�Ɠ��B�t���O��True
        if(distance <= radius)
        {
            //  �F�ς�
            StartCoroutine(FadeoutBullet(1.5f));
            step++;
        }
    }

    //------------------------------------------------------
    //  �����_���ȃR���g���[���|�C���g�ɔ��ł���
    //------------------------------------------------------
    private void FlyToRandomControlPoint()
    {
        //  �R���g���[���|�C���g�̍��W���擾
        controlPos = EnemyManager.Instance.GetControlPointPos(targetNum);

        //  ���̍��W�ւ̃x�N�g�����v�Z
        Vector3 vec = controlPos - this.transform.position;
        vec.Normalize();

        //  velocity�ɂ��̃x�N�g����ݒ�
        velocity = vec;
    }

    //------------------------------------------------------
    //  �R���g���[���|�C���g�̎������]����
    //------------------------------------------------------
    private void RorateAroundControlPoint()
    {
        Transform tr = this.transform;

        // ��]�̃N�H�[�^�j�I���쐬
        var angleAxis = Quaternion.AngleAxis(360 / period * Time.deltaTime, axis);

        // �~�^���̈ʒu�v�Z
        var pos = tr.position;

        pos -= controlPos;
        pos = angleAxis * pos;
        pos += controlPos;

        tr.position = pos;
    }

    //------------------------------------------------
    //  �w��̍��W�Ɏw��̓G�Z�b�g�𐶐�����
    //------------------------------------------------
    public GameObject SetEnemy(GameObject prefab,Vector3 pos,ePowerupItems item = ePowerupItems.None)
    {
        //  �G�I�u�W�F�N�g�̐���
        EnemyManager.Instance.GetEnemyObjectList()
            .Add(Instantiate(prefab,pos,transform.rotation));

        //  �G���̐ݒ�
        EnemySetting es = EnemyManager.Instance.GetEnemySetting();
        EnemyManager.Instance.GetEnemyObjectList()
            .Last().GetComponent<Enemy>().SetEnemyData(es, item);

        return EnemyManager.Instance.GetEnemyObjectList().Last();
    }

    //------------------------------------------------------
    //  �e���t�F�[�h�A�E�g����
    //------------------------------------------------------
    private IEnumerator FadeoutBullet(float duration)
    {
        SpriteRenderer fadeSprite = this.GetComponent<SpriteRenderer>();
        fadeSprite.enabled = true;
        var c = fadeSprite.color;
        c.a = 1.0f; // �����l
        fadeSprite.color = c;

        DOTween.ToAlpha(
        () => fadeSprite.color,
        color => fadeSprite.color = color,
        0.0f, // �ڕW�l
        duration // ���v����
        );

        yield return new WaitForSeconds(duration);

        //  �G���t�F�[�h�C������
        StartCoroutine(FadeinEnemy(1.5f));
    }

    //------------------------------------------------------
    //  �G���t�F�[�h�C������
    //------------------------------------------------------
    private IEnumerator FadeinEnemy(float duration)
    {
        /*
           �I�u�W�F�N�g��Sprite�̉摜�����l�`�̂��̂ɂ���
           ������t�F�[�h�C��������B
        �@�@������{���𐶐����Ă��̃I�u�W�F�N�g�͔j������B
        */

        //  �����x��0�ɂ���
        SpriteRenderer s = this.GetComponent<SpriteRenderer>();
        SpriteRenderer s2 = EnemyManager.Instance.GetHomingBulletSprite();
        s.sprite = s2.sprite;
        s.color = new Color(1,1,1,0);

        //  Animator��ǉ�����
        this.AddComponent<Animator>();

        //  �l�`��Animator��K�p����
        Animator animator = EnemyManager.Instance.GetHomingBulletAnimator();
        this.GetComponent<Animator>().runtimeAnimatorController
            = animator.runtimeAnimatorController;

        //  �X�P�[����1.4�{�ɍ��킹��
        this.transform.localScale = new Vector3(1.4f,1.4f,1.4f);

        //  �t�F�[�h�C���J�n
        DOTween.ToAlpha(
        () => s.color,
        color => s.color = color,
        1.0f,       // �ڕW�l
        duration    // ���v����
        );

        //  �t�F�[�h�C���̎��ԑ҂�
        yield return new WaitForSeconds(duration);

        //  �l�`�̃v���n�u���擾
        GameObject prefab = EnemyManager.Instance.GetEnemyPrefab((int)EnemyPattern.E01);

        //  �l�`�I�u�W�F�N�g�𐶐����f�[�^���Z�b�g
        GameObject obj = SetEnemy(prefab, transform.position);

        //  moveType��Charge�ɂ���
        obj.GetComponent<Enemy>().SetMoveType((int)MOVE_TYPE.Charge);

        //  ���̃I�u�W�F�N�g���폜
        Destroy(this.gameObject);
    }
}
