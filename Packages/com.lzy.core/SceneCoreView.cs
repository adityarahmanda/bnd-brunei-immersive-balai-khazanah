using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace LZY
{
    public class SceneCoreView : SceneService
    {
        protected override string GetId() => "SceneCoreView";

        public Canvas Canvas => _canvas;
        private Canvas _canvas;

        public CanvasScaler CanvasScaler => _canvasScaler;
        private CanvasScaler _canvasScaler;

        [Header("Cached Views"), ShowInInspector]
        private List<BaseView> _views = new List<BaseView>();

        [Header("Stacked Views"), ShowInInspector]
        private Stack<BaseView> _stackedViews = new Stack<BaseView>();

        [SerializeField] private BaseView initialView;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvasScaler = GetComponent<CanvasScaler>();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            PopulateView();
        }

        protected override void OnActivate()
        {
            foreach (var view in _views)
                view.Activate();
            ShowAsStack(initialView);
        }

        /// <summary>
        /// Populate all views in the scene.
        /// Child counted = only the first level of hierarchy.
        /// </summary>
        private void PopulateView()
        {
            var childCount = transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                var view = child.GetComponent<BaseView>();
                if (view != null)
                {
                    view.Initialize();
                    view.SilentHide();
                    _views.Add(view);
                }
            }
        }

        public void ShowAsStack(BaseView view)
        {
            _ = ShowAsStackAsync(view);
        }

        public async Task ShowAsStackAsync(BaseView view)
        {
            if (view == null) return;

            var transitionTasks = new List<Task>();
            if (_stackedViews.Count > 0)
            {
                var topView = _stackedViews.Peek();
                transitionTasks.Add(topView.HideOverlayAsync(false));
            }

            _stackedViews.Push(view);
            transitionTasks.Add(view.ShowOverlayAsync());
            await Task.WhenAll(transitionTasks);
        }

        public void ShowAsFirstView(BaseView view)
        {
            _ = ShowAsFirstViewAsync(view);
        }

        public async Task ShowAsFirstViewAsync(BaseView view)
        {
            if (view == null) return;
            
            if (_stackedViews.Count == 1 && _stackedViews.Peek() == view) return;

            var transitionTasks = new List<Task>();
            if (_stackedViews.Count > 0)
            {
                var veryTopView = _stackedViews.Pop();
                transitionTasks.Add(veryTopView.HideOverlayAsync());
            }
            
            while (_stackedViews.Count > 0)
            {
                var stackView = _stackedViews.Pop();
                transitionTasks.Add(stackView.HideOverlayAsync(false));
            }

            _stackedViews.Push(view);
            transitionTasks.Add(view.ShowOverlayAsync());
            await Task.WhenAll(transitionTasks);
        }

        public void ReturnToFirstView()
        {
            _ = ReturnToFirstViewAsync();
        }

        public async Task ReturnToFirstViewAsync()
        {
            if (_stackedViews.Count == 0 || _stackedViews.Count == 1) return;

            var transitionTasks = new List<Task>();
            var veryTopView = _stackedViews.Pop();
            transitionTasks.Add(veryTopView.HideOverlayAsync());

            while (_stackedViews.Count > 1)
            {
                var view = _stackedViews.Pop();
                transitionTasks.Add(view.HideOverlayAsync(false));
            }
            
            var firstView = _stackedViews.Peek();
            transitionTasks.Add(firstView.ShowOverlayAsync());
            await Task.WhenAll(transitionTasks);
        }

        public void ReturnToPreviousView()
        {
            _ = ReturnToPreviousViewAsync();
        }

        public async Task ReturnToPreviousViewAsync()
        {
            if (_stackedViews == null || _stackedViews.Count == 0) return;
            if (_stackedViews.Count == 1)
            {
                Debug.LogWarning("Hide view invalid. There should only be at least one view in this scene.");
                return;
            }

            var view = _stackedViews.Pop();
            var topView = _stackedViews.Peek();
            await Task.WhenAll(view.HideOverlayAsync(), topView.ShowOverlayAsync(false));
        }

        public T GetView<T>() where T : BaseView
        {
            var view = _views.Find(v => v.GetType() == typeof(T));
            if (view == null)
            {
                Debug.LogError($"GetView fail, {typeof(T).Name} is not exist.");
                return null;
            }

            return (T)view;
        }
    }
}
