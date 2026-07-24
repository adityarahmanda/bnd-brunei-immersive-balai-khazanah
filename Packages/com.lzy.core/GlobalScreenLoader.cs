using UnityEngine;
using DG.Tweening;
using System;

namespace LZY
{
    public class GlobalScreenLoader : MonoBehaviour
    {
        public static GlobalScreenLoader Instance;

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Animation loadingAnimation;

        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private AnimationCurve tweenEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            DontDestroyOnLoad(this);

            // snap hide
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public static void Show(Action onDone = null)
        {
            Instance.InternalShow(onDone);
        }

        private void InternalShow(Action onDone)
        {
            if (loadingAnimation != null)
                loadingAnimation.Play();

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            canvasGroup.DOFade(1, fadeDuration).SetEase(tweenEase)
                .OnComplete(() => { onDone?.Invoke(); });
        }

        public static void Hide()
        {
            Instance.InternalHide();
        }

        private void InternalHide()
        {
            canvasGroup.DOFade(0, fadeDuration)
                .SetEase(tweenEase)
                .OnComplete(OnHideComplete);

            void OnHideComplete()
            {
                if (loadingAnimation != null)
                    loadingAnimation.Stop();

                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }
}
