#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

[InitializeOnLoad]
public class AudioPreview
{
  const string AudioGameObjectName = "[AudioSourcePreview]";

  static Object previousObject = null;
  static AudioSource audioSource;

  // ���̃v���r���[�Đ��Ɏg���I�u�W�F�N�g
  static GameObject audioGameObject;

  [DidReloadScripts]
  static AudioPreview()
  {
    EditorApplication.projectWindowItemOnGUI += (string guid, Rect selectionRect) =>
    {
      if (Selection.activeObject != null && Selection.activeObject != previousObject)
      {
        // �v���r���[�Đ��p�̃I�u�W�F�N�g�쐬
        CreateObject();

        //Debug.Log("Play : " + Selection.activeObject.name);

        AudioClip clip = Selection.activeObject as AudioClip;
        if (clip != null)
        {
          // Project�r���[��AudioClip��I��������Đ�
          audioSource.clip = clip;
          audioSource.Play();
        }
        else
        {
          // AudioClip�ȊO��I��������v���r���[��~
          GameObject.DestroyImmediate(audioGameObject);
        }
      }

      if (Selection.activeObject == null)
      {
        // �I�����O�ꂽ��v���r���[��~
        GameObject.DestroyImmediate(audioGameObject);
      }

      previousObject = Selection.activeObject;
    };
  }

  /// <summary>
  /// �v���r���[�Đ��p�̃I�u�W�F�N�g�쐬
  /// </summary>
  private static void CreateObject()
  {
    audioGameObject = GameObject.Find(AudioGameObjectName);
    if (audioGameObject == null)
    {
      audioGameObject = new GameObject(AudioGameObjectName);
    }

    audioSource = audioGameObject.GetComponent<AudioSource>();
    if (audioSource == null)
    {
      audioSource = audioGameObject.AddComponent<AudioSource>();
    }

    audioSource.playOnAwake = false;
  }

}
#endif