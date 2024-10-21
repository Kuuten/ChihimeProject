using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindSaveManager : MonoBehaviour
{
    // 対象となるInputActionAsset
    [SerializeField] private InputActionAsset _actionAsset;

    // 上書き情報の保存先
    [SerializeField] private string _savePath = "InputActionOverrides.json";

    // 上書き情報の保存
    public void Save()
    {
        if (_actionAsset == null) return;

        // InputActionAssetの上書き情報の保存
        var json = _actionAsset.SaveBindingOverridesAsJson();

        // ファイルに保存
        var path = Path.Combine(Application.persistentDataPath, _savePath);
        File.WriteAllText(path, json);
    }

    // 上書き情報の読み込み
    public void Load()
    {
        if (_actionAsset == null) return;

        // ファイルから読み込み
        var path = Path.Combine(Application.persistentDataPath, _savePath);
        if (!File.Exists(path))
        {
            Debug.Log("InputActionOverrides.jsonが見つかりませんでした。\n" +
                "このまま続行します。");
            return;
        }

         Debug.Log("InputActionOverrides.jsonが見つかりました！\n" +
                "ロードします。");

        var json = File.ReadAllText(path);

        // InputActionAssetの上書き情報を設定
        _actionAsset.LoadBindingOverridesFromJson(json);

        Debug.Log("ロード完了");
    }
}