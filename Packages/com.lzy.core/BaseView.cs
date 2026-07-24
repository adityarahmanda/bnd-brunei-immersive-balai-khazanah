using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace LZY
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BaseView : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected float fadeDuration = .25f;

        public bool IsVisible => _isVisible;

        public UnityEvent OnShowComplete;
        public UnityEvent OnHideComplete;

        protected bool _isVisible = false;
        
        protected SceneCoreView _sceneCoreView;
        protected Tween _fadeTween;

        private void OnValidate()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }
        
        public void Initialize()
        {
            _sceneCoreView = SceneCore.GetService<SceneCoreView>();
            OnInitialize();
        }
        
        public void Activate()
        {
            OnActivate();
        }

        // OnInitialize is run before awake
        protected virtual void OnInitialize() { }
        
        // its safe to call GetService and GetServiceView here
        protected virtual void OnActivate() { }

        // called when alpha = 0
        protected virtual void OnPreShow() { }

        // called after alpha = 1
        protected virtual void OnShow() { }

        // called after alpha = 0
        protected virtual void OnHide() { }

        public void ShowAsStack()
        {
            _sceneCoreView.ShowAsStack(this);
        }
        
        public async Task ShowAsStackAsync()
        {
            await _sceneCoreView.ShowAsStackAsync(this);
        }
        
        public void ShowAsFirstView()
        {
            _sceneCoreView.ShowAsFirstView(this);
        }
        
        public async Task ShowAsFirstViewAsync()
        {
            await _sceneCoreView.ShowAsFirstViewAsync(this);
        }

        public void Return()
        {
            _sceneCoreView.ReturnToPreviousView();
        }
        
        public async Task ReturnAsync()
        {
            await _sceneCoreView.ReturnToPreviousViewAsync();
        }
        
        public void ReturnToFirstView()
        {
            _sceneCoreView.ReturnToFirstView();
        }
        
        public async Task ReturnToFirstViewAsync()
        {
            await _sceneCoreView.ReturnToFirstViewAsync();
        }
        
        public void ShowOverlay(bool hasAnimation = true)
        {
            _ = ShowOverlayAsync(hasAnimation);
        }

        public async Task ShowOverlayAsync(bool hasAnimation = true)
        {
            if (_isVisible) return;
            _isVisible = true;

            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            if (hasAnimation)
            {
                if (_fadeTween != null && _fadeTween.IsPlaying())
                    _fadeTween.Kill(true);
                _fadeTween = canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.InSine);;
                OnPreShow();
                await _fadeTween.AsyncWaitForCompletion();
                OnShow();
                OnShowComplete?.Invoke();
            }
            else
            {
                canvasGroup.alpha = 1;
                OnPreShow();
                OnShow();
                OnShowComplete?.Invoke();
            }
        }
        
        public void HideOverlay(bool hasAnimation = true)
        {
            _ = HideOverlayAsync(hasAnimation);
        }

        public async Task HideOverlayAsync(bool animation = true)
        {
            if (_isVisible == false) return;
            _isVisible = false;

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            if (animation)
            {
                if (_fadeTween != null && _fadeTween.IsPlaying())
                    _fadeTween.Kill(true);
                _fadeTween = canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InSine);
                OnHide();
                await _fadeTween.AsyncWaitForCompletion();
                OnHideComplete?.Invoke();
            }
            else
            {
                canvasGroup.alpha = 0;
                OnHide();
                OnHideComplete?.Invoke();
            }
        }

        public void SilentShow()
        {
            _isVisible = true;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        public void SilentHide()
        {
            _isVisible = false;
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}
