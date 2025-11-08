using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UndeadSurvivor;

/// <summary>
/// 레벨업 선택지 개별 버튼
/// 선택지 데이터를 표시하고 클릭 이벤트 발생
/// </summary>
public class LevelUpOptionButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button _button; // 버튼 컴포넌트
    [SerializeField] private Image _iconImage; // 아이콘 이미지
    [SerializeField] private TextMeshProUGUI _titleText; // 제목 텍스트
    [SerializeField] private TextMeshProUGUI _descriptionText; // 설명 텍스트

    [Header("Visual Settings")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _hoverColor = Color.yellow;
    [SerializeField] private float _hoverScale = 1.05f;

    private LevelUpOption _option;
    private Vector3 _originalScale;

    /// <summary>
    /// 버튼 클릭 이벤트
    /// </summary>
    public event Action OnButtonClicked;

    #region Unity Lifecycle

    private void Awake()
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        if (_button != null)
        {
            _button.onClick.AddListener(OnClick);
        }

        _originalScale = transform.localScale;
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnClick);
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// 선택지 데이터 설정 및 UI 업데이트
    /// </summary>
    public void SetOption(LevelUpOption option)
    {
        _option = option;
        UpdateUI();
    }

    #endregion

    #region UI Update

    /// <summary>
    /// UI 업데이트 (텍스트, 아이콘)
    /// </summary>
    private void UpdateUI()
    {
        if (_option == null)
        {
            Debug.LogWarning("[WARNING] LevelUpOptionButton::UpdateUI - Option is null");
            return;
        }

        // 제목 설정
        if (_titleText != null)
        {
            _titleText.text = _option.Title;
        }

        // 설명 설정
        if (_descriptionText != null)
        {
            _descriptionText.text = _option.Description;
        }

        // 아이콘 로드 (Resources 또는 Addressables)
        LoadIcon(_option.IconPath);

        Debug.Log($"[INFO] LevelUpOptionButton::UpdateUI - Updated with option: {_option.Title}");
    }

    /// <summary>
    /// 아이콘 로드 (Resources)
    /// </summary>
    private void LoadIcon(string iconPath)
    {
        if (_iconImage == null || string.IsNullOrEmpty(iconPath))
        {
            if (_iconImage != null)
            {
                _iconImage.enabled = false;
            }
            return;
        }

        // Resources 폴더에서 스프라이트 로드
        Sprite iconSprite = Resources.Load<Sprite>(iconPath);

        if (iconSprite != null)
        {
            _iconImage.sprite = iconSprite;
            _iconImage.enabled = true;
        }
        else
        {
            Debug.LogWarning($"[WARNING] LevelUpOptionButton::LoadIcon - Icon not found: {iconPath}");
            _iconImage.enabled = false;
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// 버튼 클릭 시 호출
    /// </summary>
    private void OnClick()
    {
        Debug.Log($"[INFO] LevelUpOptionButton::OnClick - Button clicked: {_option?.Title ?? "Unknown"}");

        // 클릭 이벤트 발생
        OnButtonClicked?.Invoke();

        // 클릭 효과 (옵션)
        PlayClickAnimation();
    }

    #endregion

    #region Visual Effects

    /// <summary>
    /// 마우스 오버 효과
    /// </summary>
    public void OnPointerEnter()
    {
        if (_button != null && _button.interactable)
        {
            transform.localScale = _originalScale * _hoverScale;

            if (_titleText != null)
            {
                _titleText.color = _hoverColor;
            }
        }
    }

    /// <summary>
    /// 마우스 벗어남 효과
    /// </summary>
    public void OnPointerExit()
    {
        transform.localScale = _originalScale;

        if (_titleText != null)
        {
            _titleText.color = _normalColor;
        }
    }

    /// <summary>
    /// 클릭 애니메이션 (간단한 스케일 효과)
    /// </summary>
    private void PlayClickAnimation()
    {
        // TODO: 애니메이션 또는 효과 추가
        // 예: DOTween을 사용한 스케일 애니메이션
    }

    #endregion
}
