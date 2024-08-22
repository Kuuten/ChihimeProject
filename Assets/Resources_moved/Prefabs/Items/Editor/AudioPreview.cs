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

  // 音のプレビュー再生に使うオブジェクト
  static GameObject audioGameObject;

  [DidReloadScripts]
  static AudioPreview()
  {
    EditorApplication.projectWindowItemOnGUI += (string guid, Rect selectionRect) =>
    {
      if (Selection.activeObject != null && Selection.activeObject != previousObject)
      {
        // プレビュー再生用のオブジェクト作成
        CreateObject();

        //Debug.Log("Play : " + Selection.activeObject.name);

        AudioClip clip = Selection.activeObject as AudioClip;
        if (clip != null)
        {
          // ProjectビューでAudioClipを選択したら再生
          audioSource.clip = clip;
          audioSource.Play();
        }
        else
        {
          // AudioClip以外を選択したらプレビュー停止
          GameObject.DestroyImmediate(audioGameObject);
        }
      }

      if (Selection.activeObject == null)
      {
        // 選択が外れたらプレビュー停止
        GameObject.DestroyImmediate(audioGameObject);
      }

      previousObject = Selection.activeObject;
    };
  }

  /// <summary>
  /// プレビュー再生用のオブジェクト作成
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