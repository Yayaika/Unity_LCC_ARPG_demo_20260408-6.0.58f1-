using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadCoverCtrl : MonoBehaviour
{
    [Tooltip("請拖入 FadeImage 上的 CanvasGroup")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.5f;

    private void Awake()
    {
        // 跨場景不銷毀，確保加載完成後能順利執行淡出
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        // 1. 黑屏淡入 (遮住舊畫面)
        yield return StartCoroutine(Fade(1f));

        string targetScene = SceneChanger.targetSceneName;   // 主要載入場景 (如 "GamingUI")
        string extraScene = SceneChanger.additiveSceneName; // 疊加場景 (如 "Stage01")

        // 2.【載入主要場景】採用 Single 模式 (這會把 PlayerSetting 等舊場景完全清空)
        if (!string.IsNullOrEmpty(targetScene))
        {
            // 檢查是否已經載入，避免重複
            if (!IsSceneLoaded(targetScene))
            {
                AsyncOperation loadMainOp = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Single);
                while (!loadMainOp.isDone)
                {
                    yield return null;
                }
            }
        }

        // 3.【載入疊加場景（如世界關卡）】
        if (!string.IsNullOrEmpty(extraScene))
        {
            // 防重覆機制：先檢查 extraScene 是否已經存在於 Hierarchy 中！
            if (!IsSceneLoaded(extraScene))
            {
                AsyncOperation loadExtraOp = SceneManager.LoadSceneAsync(extraScene, LoadSceneMode.Additive);
                while (!loadExtraOp.isDone)
                {
                    yield return null;
                }
            }

            // 將世界場景設為 Active Scene
            Scene loadedExtraScene = SceneManager.GetSceneByName(extraScene);
            if (loadedExtraScene.IsValid())
            {
                SceneManager.SetActiveScene(loadedExtraScene);
            }
        }

        // 4. 等待所有物件的 Awake() 與 Start() 跑完，並完成第一幀渲染
        yield return null;
        yield return new WaitForEndOfFrame();

        // 5. 黑幕淡出
        yield return StartCoroutine(Fade(0f));

        if (IsSceneLoaded("LoadCover"))
        {
            SceneManager.UnloadSceneAsync("LoadCover");
        }

        // 6. 轉場完畢，銷毀黑幕自身
        Destroy(gameObject);
    }

    /// <summary>
    /// 檢查特定場景目前是否已經被加載
    /// </summary>
    private bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == sceneName && scene.isLoaded)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.blocksRaycasts = true;
        float startAlpha = fadeCanvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
        if (targetAlpha == 0f) fadeCanvasGroup.blocksRaycasts = false;
    }
}