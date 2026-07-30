using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIPlayerInfoCtrl : MonoBehaviour
{
    #region 基础元件
    [SerializeField]
    private PlayerDB _playerDB;
    [SerializeField]
    private Image _headImg;
    [SerializeField]
    private TextMeshProUGUI _nameText;
    [SerializeField]
    private Image _hpBarImg;        // 紅色即時血條
    [SerializeField]
    private Image _hpBufferBarImg;  // 黃色緩衝血條
    [SerializeField]
    private TextMeshProUGUI _hpBarText;

    private float _HP;
    private float _maxHP;
    private Coroutine _bufferCoroutine;
    #endregion 基础元件

    #region 公用参数
    private int PlayerIndex => GameManager.playerIndex;
    private string StrHP => $"{(int)_HP}/{(int)_maxHP}";
    private float PercentHP => _HP / _maxHP;
    #endregion 公用参数

    private void OnEnable()
    {
        InitialUI();
        GameManager.SetPlayerHPBar(UpdateHPBar);
    }
    private void OnDisable()
    {
        GameManager.RemovePlayerHPBar(UpdateHPBar);
        if (_bufferCoroutine != null)
        {
            StopCoroutine(_bufferCoroutine);
            _bufferCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        GameManager.RemovePlayerHPBar(UpdateHPBar);
    }

    private void InitialUI()
    {
        if (_playerDB == null)
        {
            Debug.LogWarning("PlayerDB 尚未準備就緒！");
            return;
        }
        _headImg.sprite = _playerDB.GetPlayerData(PlayerIndex).icon;
        _nameText.text = _playerDB.GetPlayerData(PlayerIndex).name;
    }

    private void UpdateHPBar(float HP, float maxHP)
    {
        _HP = HP;
        _maxHP = maxHP;
        _hpBarText.text = StrHP;

        float targetFill = PercentHP;
        if (maxHP <= 0) targetFill = 0f;

        // 1. 紀錄「更新前」紅色血條的值，用以精準判定是扣血還是回血
        float previousRedFill = _hpBarImg.fillAmount;

        // 2. 瞬間更新紅色即時血條
        _hpBarImg.fillAmount = targetFill;

        // 3. 判斷本次血量變化
        if (targetFill >= previousRedFill)
        {
            // --- 情況 A：回血、初始、或數值無變化 ---
            if (_bufferCoroutine != null)
            {
                StopCoroutine(_bufferCoroutine);
                _bufferCoroutine = null;
            }
            // 黃色緩衝條直接同步對齊
            _hpBufferBarImg.fillAmount = targetFill;
        }
        else
        {
            // --- 情況 B：受傷扣血 ---
            if (_bufferCoroutine != null)
            {
                StopCoroutine(_bufferCoroutine);
            }
            _bufferCoroutine = StartCoroutine(DelayBufferBarLerp(targetFill));
        }

        // 【安全鎖第一重】更新結束時，確保黃條絕對不低於紅條
        _hpBufferBarImg.fillAmount = Mathf.Max(_hpBufferBarImg.fillAmount, _hpBarImg.fillAmount);
    }

    private IEnumerator DelayBufferBarLerp(float targetFill)
    {
        yield return new WaitForSeconds(0.2f);

        float startFill = _hpBufferBarImg.fillAmount;
        float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 【安全鎖第二重】在漸變滑動過程中，黃條也絕對不能低於當前紅條（防止滑行時突然回血導致紅條穿出）
            float currentLerp = Mathf.Lerp(startFill, targetFill, elapsed / duration);
            _hpBufferBarImg.fillAmount = Mathf.Max(currentLerp, _hpBarImg.fillAmount);

            yield return null;
        }

        _hpBufferBarImg.fillAmount = Mathf.Max(targetFill, _hpBarImg.fillAmount);
        _bufferCoroutine = null;
    }
}