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

        float previousRedFill = _hpBarImg.fillAmount;
        bool isJustOpened = !IsShow;

        _hpBarImg.fillAmount = targetFill;

        if (isJustOpened)
        {
            if (_bufferCoroutine != null)
            {
                StopCoroutine(_bufferCoroutine);
                _bufferCoroutine = null;
            }
            _hpBufferBarImg.fillAmount = targetFill;
        }
        else if (targetFill < previousRedFill)
        {
            if (_bufferCoroutine != null)
            {
                StopCoroutine(_bufferCoroutine);
            }
            _bufferCoroutine = StartCoroutine(DelayBufferBarLerp(targetFill));
        }
        else if (targetFill > previousRedFill)
        {
            if (_bufferCoroutine != null)
            {
                StopCoroutine(_bufferCoroutine);
                _bufferCoroutine = null;
            }
            _hpBufferBarImg.fillAmount = targetFill;
        }

        _hpBufferBarImg.fillAmount = Mathf.Max(_hpBufferBarImg.fillAmount, _hpBarImg.fillAmount);

        if (_HP > 0)
        {
            Switch(true);
        }
        else
        {
            Switch(false);
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

            float currentLerp = Mathf.Lerp(startFill, targetFill, elapsed / duration);
            _hpBufferBarImg.fillAmount = Mathf.Max(currentLerp, _hpBarImg.fillAmount);

            yield return null;
        }

        _hpBufferBarImg.fillAmount = Mathf.Max(targetFill, _hpBarImg.fillAmount);
        _bufferCoroutine = null;
    }

    /// <summary>
    /// 外部呼叫：當玩家離開 Boss 房間且 Boss 未擊敗時主動關閉 UI
    /// </summary>
    public void HideBossUI()
    {
        if (_HP > 0)
        {
            Switch(false);
        }
    }
}