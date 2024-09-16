using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//--------------------------------------------------------------
//
//  �V�[���̃��[�f�B���O�N���X
//
//--------------------------------------------------------------
public class LoadingScene : MonoBehaviour
{
    [SerializeField] private GameObject _loadingUI;
    [SerializeField] private Slider _slider;

    //  �V���O���g���ȃC���X�^���X
    public static LoadingScene Instance
    {
        get; private set;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void LoadNextScene(string scene)
    {
        _loadingUI.SetActive(true);
        StartCoroutine(LoadScene(scene));
    }
    IEnumerator LoadScene(string scene)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(scene);

        while (!async.isDone)
        {
            _slider.value = async.progress;
            yield return null;
        }
    }
}