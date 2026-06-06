using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 마우스 이벤트를 받기 위해 필수

public class PixelButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private Image buttonImage;

    [Header("설정")]
    public Color hoverColor = new Color(1.1f, 1.1f, 1.1f); // 살짝 밝아짐
    public Vector3 pressedScale = new Vector3(0.95f, 0.95f, 1f); // 눌렸을 때 작아짐

    private Color originalColor;
    private Vector3 originalScale;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        buttonImage = GetComponent<Image>();
        originalColor = buttonImage.color;
        originalScale = rectTransform.localScale;
    }

    // 마우스를 올렸을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.color = originalColor * hoverColor;
    }

    // 마우스가 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.color = originalColor;
        rectTransform.localScale = originalScale;
    }

    // 마우스를 눌렀을 때
    public void OnPointerDown(PointerEventData eventData)
    {
        rectTransform.localScale = pressedScale;
    }

    // 마우스에서 손을 뗐을 때
    public void OnPointerUp(PointerEventData eventData)
    {
        rectTransform.localScale = originalScale;
    }
}