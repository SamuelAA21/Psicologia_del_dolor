using UnityEngine;
using UnityEngine.EventSystems;

public class UnderstandApplyDraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private UnderstandApplyMiniGame miniGame;
    private RectTransform rectTransform;
    private Vector2 startPosition;

    public void Configure(UnderstandApplyMiniGame owner)
    {
        miniGame = owner;
    }

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        if (rectTransform != null)
        {
            startPosition = rectTransform.anchoredPosition;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rectTransform != null)
        {
            startPosition = rectTransform.anchoredPosition;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        miniGame?.SubmitDroppedCard(eventData.position);

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = startPosition;
        }
    }
}
