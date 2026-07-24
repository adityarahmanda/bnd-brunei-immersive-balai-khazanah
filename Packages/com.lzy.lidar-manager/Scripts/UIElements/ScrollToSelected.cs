using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LZY.Lidar.UIElements
{
    public class ScrollToSelected : MonoBehaviour, ISelectHandler
    {
        public ScrollRect scrollRect;
        public float scrollSpeed = 10f;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (scrollRect == null) return;

            Canvas.ForceUpdateCanvases();
            var sizeY = _rectTransform.rect.size.y;
            var targetPosition = (Vector2)scrollRect.transform.InverseTransformPoint(scrollRect.content.position)
                                     - (Vector2)scrollRect.transform.InverseTransformPoint(_rectTransform.position);
            scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, targetPosition.y - sizeY);
        }
    }
}
