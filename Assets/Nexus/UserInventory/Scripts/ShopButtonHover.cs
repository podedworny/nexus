using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Button))]
public class ShopButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image backgroundImage;
    public Image topBorder;
    public Image bottomBorder;
    public TextMeshProUGUI buttonText;
    public float fadeDuration = 0.2f;

    [Header("Normal State")]
    [SerializeField] private Color32 _normalBgColor = new Color32(40, 55, 80, 255);
    [SerializeField] private Color32 _hoverBgColor = new Color32(232, 200, 64, 255);
    [SerializeField] private Color32 _normalTextColor = new Color32(232, 200, 64, 255);
    [SerializeField] private Color32 _hoverTextColor = new Color32(14, 21, 32, 255);
    [SerializeField] private Color32 _borderColor = new Color32(232, 200, 64, 255);

    [Header("Disabled State")]
    [SerializeField] private Color32 _disabledBgColor = new Color32(20, 25, 35, 255);
    [SerializeField] private Color32 _disabledTextColor = new Color32(100, 110, 125, 255);
    [SerializeField] private Color32 _disabledBorderColor = new Color32(100, 110, 125, 255);

    private Button _button;
    private Coroutine _fadeCoroutine;
    private bool _isHovered = false;
    private bool _wasInteractable;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _wasInteractable = _button.interactable;
    }

    private void OnEnable()
    {
        _isHovered = false;
        SnapToCurrentState();
    }

    private void Start()
    {
        SnapToCurrentState();
    }

    private void Update()
    {
        if (_button.interactable != _wasInteractable)
        {
            _wasInteractable = _button.interactable;
            RefreshVisuals(false);
        }
    }

    public void SnapToCurrentState()
    {
        if (_button == null) _button = GetComponent<Button>();
        if (_button != null)
        {
            _wasInteractable = _button.interactable;
            RefreshVisuals(true);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        if (_button.interactable)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeColors(_hoverBgColor, _borderColor, _hoverTextColor));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        if (_button.interactable)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeColors(_normalBgColor, _borderColor, _normalTextColor));
        }
    }

    private void RefreshVisuals(bool instant)
    {
        Color targetBg = _button.interactable ? (_isHovered ? _hoverBgColor : _normalBgColor) : _disabledBgColor;
        Color targetBorder = _button.interactable ? _borderColor : _disabledBorderColor;
        Color targetText = _button.interactable ? (_isHovered ? _hoverTextColor : _normalTextColor) : _disabledTextColor;

        if (instant)
        {
            SetColors(targetBg, targetBorder, targetText);
        }
        else
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeColors(targetBg, targetBorder, targetText));
        }
    }

    private void SetColors(Color bgColor, Color borderColor, Color textColor)
    {
        if (backgroundImage != null) backgroundImage.color = bgColor;
        if (topBorder != null) topBorder.color = borderColor;
        if (bottomBorder != null) bottomBorder.color = borderColor;
        if (buttonText != null) buttonText.color = textColor;
    }

    private IEnumerator FadeColors(Color targetBg, Color targetBorder, Color targetText)
    {
        Color startBg = backgroundImage != null ? backgroundImage.color : targetBg;
        Color startTopBorder = topBorder != null ? topBorder.color : targetBorder;
        Color startBottomBorder = bottomBorder != null ? bottomBorder.color : targetBorder;
        Color startText = buttonText != null ? buttonText.color : targetText;

        float time = 0;
        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / fadeDuration;

            if (backgroundImage != null) backgroundImage.color = Color.Lerp(startBg, targetBg, t);
            if (topBorder != null) topBorder.color = Color.Lerp(startTopBorder, targetBorder, t);
            if (bottomBorder != null) bottomBorder.color = Color.Lerp(startBottomBorder, targetBorder, t);
            if (buttonText != null) buttonText.color = Color.Lerp(startText, targetText, t);

            yield return null;
        }

        SetColors(targetBg, targetBorder, targetText);
    }
}
