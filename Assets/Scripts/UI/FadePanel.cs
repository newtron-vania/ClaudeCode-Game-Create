using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면 페이드 효과용 패널
/// </summary>
public class FadePanel : UIPanel
{
    [Header("Fade Settings")]
    [SerializeField] private Image _fadeImage;
    [SerializeField] private Color _fadeColor = Color.black;
    [SerializeField] private float _defaultFadeDuration = 0.5f;

    private Coroutine _fadeCoroutine;

    protected override void Awake()
    {
        base.Awake();

        // Fade Image 자동 찾기 또는 생성
        if (_fadeImage == null)
        {
            _fadeImage = GetComponentInChildren<Image>();
            if (_fadeImage == null)
            {
                GameObject imageObj = new GameObject("FadeImage");
                imageObj.transform.SetParent(transform, false);

                RectTransform rectTransform = imageObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;

                _fadeImage = imageObj.AddComponent<Image>();
                _fadeImage.color = _fadeColor;
            }
        }

        // 초기 상태: 완전 투명
        SetAlpha(0f);
    }

    /// <summary>
    /// 페이드 인 (투명 → 불투명)
    /// </summary>
    /// <param name="duration">페이드 시간 (초)</param>
    /// <param name="onComplete">완료 콜백</param>
    public void FadeIn(float duration, Action onComplete = null)
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        Open();
        _fadeCoroutine = StartCoroutine(FadeCoroutine(0f, 1f, duration, onComplete));
    }

    /// <summary>
    /// 페이드 인 (기본 시간)
    /// </summary>
    /// <param name="onComplete">완료 콜백</param>
    public void FadeIn(Action onComplete = null)
    {
        FadeIn(_defaultFadeDuration, onComplete);
    }

    /// <summary>
    /// 페이드 아웃 (불투명 → 투명)
    /// </summary>
    /// <param name="duration">페이드 시간 (초)</param>
    /// <param name="onComplete">완료 콜백</param>
    public void FadeOut(float duration, Action onComplete = null)
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(FadeCoroutine(1f, 0f, duration, () =>
        {
            Close();
            onComplete?.Invoke();
        }));
    }

    /// <summary>
    /// 페이드 아웃 (기본 시간)
    /// </summary>
    /// <param name="onComplete">완료 콜백</param>
    public void FadeOut(Action onComplete = null)
    {
        FadeOut(_defaultFadeDuration, onComplete);
    }

    /// <summary>
    /// 페이드 코루틴
    /// </summary>
    private IEnumerator FadeCoroutine(float startAlpha, float targetAlpha, float duration, Action onComplete)
    {
        float elapsed = 0f;
        Color color = _fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            _fadeImage.color = color;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = color.a;
            }

            yield return null;
        }

        color.a = targetAlpha;
        _fadeImage.color = color;

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = targetAlpha;
        }

        _fadeCoroutine = null;
        onComplete?.Invoke();
    }

    /// <summary>
    /// 페이드 색상 설정
    /// </summary>
    /// <param name="color">페이드 색상</param>
    public void SetFadeColor(Color color)
    {
        _fadeColor = color;
        if (_fadeImage != null)
        {
            Color currentColor = _fadeImage.color;
            currentColor.r = color.r;
            currentColor.g = color.g;
            currentColor.b = color.b;
            _fadeImage.color = currentColor;
        }
    }
}
