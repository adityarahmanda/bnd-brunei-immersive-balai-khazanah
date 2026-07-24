#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

namespace LZY.BND
{
    public class MainSceneCore : SceneCore
    {
        [Header("UI Navigation Runtime")]
        [SerializeField] private GameObject lastSelectedElement;

        private CallibrationView _callibrationView;
        private EventSystem _eventSystem;

        protected override void OnActivate()
        {
            _callibrationView = GetServiceView<CallibrationView>();
            _eventSystem = EventSystem.current;
        }

        protected override void OnUpdate()
        {
            // Disable deselecting gameobject when outfocused by mouse click
            if (_eventSystem)
            {
                if (_eventSystem.currentSelectedGameObject &&
                    lastSelectedElement != _eventSystem.currentSelectedGameObject)
                    lastSelectedElement = _eventSystem.currentSelectedGameObject;

                if (!_eventSystem.currentSelectedGameObject && lastSelectedElement)
                    _eventSystem.SetSelectedGameObject(lastSelectedElement);
            }
            
            // Show/Hide Callibration View
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (_callibrationView.IsVisible)
                    _callibrationView.SilentHide();
                else
                    _callibrationView.SilentShow();
            }
        }
        
        
#if UNITY_EDITOR
        [Header("Editor Only")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private RenderTexture mainCameraRT;
        [SerializeField, ReadOnly] private bool renderOnCamera;

        [Button]
        private void DebugRenderOnCamera()
        {
            renderOnCamera = !renderOnCamera;
            mainCamera.targetTexture = renderOnCamera ? null : mainCameraRT;
        }
#endif
    }
}
