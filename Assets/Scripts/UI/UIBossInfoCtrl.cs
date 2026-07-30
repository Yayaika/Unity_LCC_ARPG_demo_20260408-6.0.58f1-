using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Collections;

public class UIBossInfoCtrl : UIPanelCtrl
{
    #region 基礎元件
    [SerializeField]
    private TextMeshProUGUI _nameText;
    [SerializeField]
    private Image _hpBarImg;
    [SerializeField]
    private Image _hpBufferBarImg; // 黃色緩衝血條
    [SerializeField]
    private TextMeshProUGUI _hpBarText;

    private bool _isReady;
    private float _HP;
    private float _maxHP;
    private string StrHP => $"{(int)_HP}/{(int)_maxHP}";
    private float PercentHP => _HP / _maxHP;
    private Coroutine _bufferCoroutine;
    #endregion 基礎元件

    #region 生命週期
    private void OnEnable()
    {
        GameManager.SetBossHPBar(UpdateHPBar);
    }

    private void OnDisable()
    {
        GameManager.RemoveBossHPBar(UpdateHPBar);
        if (_bufferCoroutine != null)
        {
            StopCoroutine(_bufferCoroutine);
            _bufferCoroutine = null;
        }
    }
    #endregion 週期

    private void UpdateHPBar(float HP, float maxHP)
    {
        _HP = HP;
        _maxHP = maxHP;
        _hpBarText.text = StrHP;

        float targetFill = PercentHP;
        if (maxHP <= 0) targetFill = 0f;

        // 1. 紀錄「更新前」紅色血條的值
        float previousRedFill = _hpBarImg.fillAmount;

        // 2. 更新即時紅色血條
        _hpBarImg.fillAmount = targetFill;

        // 3. 判斷血量變化
        if (!_isReady || targetFill >= previousRedFill)
        {
            // --- 情況 A：回血、初始、或無變化 ---
            if (_bufferCoroutine != null)
            {
                StopCoroutine(_bufferCoroutine);
                _bufferCoroutine = null;
            }
            _hpBufferBarImg.fillAmount = targetFill;
        }
        else
        {
            // --- 情況 B：扣血 ---
            if (_bufferCoroutine != null)
            {
                StopCoroutine(_bufferCoroutine);
            }
            _bufferCoroutine = StartCoroutine(DelayBufferBarLerp(targetFill));
        }

        // 【安全鎖第一重】確保黃條絕對不低於紅條
        _hpBufferBarImg.fillAmount = Mathf.Max(_hpBufferBarImg.fillAmount, _hpBarImg.fillAmount);

        // 第一次執行時初始
        if (!_isReady && _maxHP > 0)
        {
            Switch(true);
            _isReady = true;
        }

        // 如果血量歸零，關閉 UI
        if (_HP <= 0)
        {
            Switch(false);
            _isReady = false;
        }
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

            // 【安全鎖第二重】確保漸變滑動時，黃條絕對不低於當前紅條
            float currentLerp = Mathf.Lerp(startFill, targetFill, elapsed / duration);
            _hpBufferBarImg.fillAmount = Mathf.Max(currentLerp, _hpBarImg.fillAmount);

            yield return null;
        }

        _hpBufferBarImg.fillAmount = Mathf.Max(targetFill, _hpBarImg.fillAmount);
        _bufferCoroutine = null;
    }
}