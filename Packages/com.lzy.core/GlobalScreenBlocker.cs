using UnityEngine;

namespace LZY
{
    public class GlobalScreenBlocker : MonoBehaviour
    {
        public static GlobalScreenBlocker Instance;

        [SerializeField] private CanvasGroup canvasGroup;

        public bool IsBlocking
        {
            get { return canvasGroup.interactable; }
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            canvasGroup.alpha = 0;
            Instance.InternalHide();
            DontDestroyOnLoad(this);
        }

        public static void Show()
        {
            Instance.InternalShow();
        }

        private void InternalShow()
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public static void Hide()
        {
            Instance.InternalHide();
        }

        private void InternalHide()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
