using UnityEngine;
using UnityEngine.SceneManagement; // 引入場景管理

public class SceneChanger : MonoBehaviour
{
    // 靜態變數：用來跨場景傳遞目標主要場景 (例如 "Stage01")
    public static string targetSceneName;

    // 靜態變數：用來跨場景傳遞需要疊加的 UI 場景 (例如 "GamingUI"，可留空)
    public static string additiveSceneName;

    /// <summary>
    /// 一般載入（預設為 Single，不會疊加，直接替換）
    /// </summary>
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// 疊加載入（會疊加在目前場景上方，例如 LoadCover 或 UIScene）
    /// </summary>
    public void LoadSceneAdditive(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    /// <summary>
    /// [供腳本直接呼叫] 靜態方法：帶黑屏轉場 (支援傳入主要場景 + 可選的 UI 場景)
    /// </summary>
    public static void LoadWithCover(string mainScene, string extraUI = null)
    {
        targetSceneName = mainScene;
        additiveSceneName = extraUI;
        SceneManager.LoadScene("LoadCover", LoadSceneMode.Additive);
    }

    /// <summary>
    /// [供 Inspector 按鈕綁定] 實體方法：只帶單一目標場景的黑屏轉場
    /// </summary>
    public void LoadSceneWithCover(string nextSceneName)
    {
        LoadWithCover(nextSceneName, null);
    }

    /// <summary>
    /// 卸載指定的疊加場景
    /// </summary>
    public void UnloadScene(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = true;
#else
        Application.Quit();
#endif
    }
}