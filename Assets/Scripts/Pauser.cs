using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;
using System.Xml.Linq;

//---------------------------------------------------------------------
// ��Destroy����铮�I�ȃI�u�W�F�N�g�ɂ͎g�p�ł��Ȃ�
//---------------------------------------------------------------------
public class Pauser : MonoBehaviour {
	static List<Pauser> targets = new List<Pauser>();	// �|�[�Y�Ώۂ̃X�N���v�g
	MonoBehaviour[] pauseBehavs = null;	// �|�[�Y�Ώۂ̃R���|�[�l���g

	// ������
	void Start() {
		// �|�[�Y�Ώۂɒǉ�����
		targets.Add(this);
	}

	// �j�������Ƃ�
	void OnDestroy() {
		// �|�[�Y�Ώۂ��珜�O����
		targets.Remove(this);
	}

	// �|�[�Y���ꂽ�Ƃ�
	void OnPause() {
        if ( pauseBehavs != null ) {
            return;
        }

        // �L����Behaviour���擾
		pauseBehavs = Array.FindAll(GetComponentsInChildren<MonoBehaviour>(), (obj) => { return obj.enabled; });

		foreach ( var com in pauseBehavs ) {
			com.enabled = false;
		}
	}

	// �|�[�Y�������ꂽ�Ƃ�
	void OnResume() {
		if ( pauseBehavs == null ) {
			return;
		}

		// �|�[�Y�O�̏�Ԃ�Behaviour�̗L����Ԃ𕜌�
		foreach ( var com in pauseBehavs ) {
			com.enabled = true;
		}

		pauseBehavs = null;
	}

	// �|�[�Y
	public static void Pause() {
		foreach ( var obj in targets ) {
			obj.OnPause();
		}
	}

	// �|�[�Y����
	public static void Resume() {
		foreach ( var obj in targets ) {
			obj.OnResume();
		}
	}
}