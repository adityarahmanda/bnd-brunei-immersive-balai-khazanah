using Sirenix.OdinInspector;
using System;
using LZY.Resolume;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LZY.BND
{
    public class MainSceneCore : SceneCore
    {
        private static MainSceneCore instance;
        public static AppSettings settings { get { return instance?.appPersistence.GetSettings(); } }

        [Header("App Persistence")]
        [SerializeField, HideLabel] private AppPersistence appPersistence;
        
        [Header("UI Navigation Runtime")]
        [SerializeField] private GameObject lastSelectedElement;
        
#if UNITY_EDITOR
        [Header("Editor Only")]
        [SerializeField] private bool loadSettingsOnEditor;
#endif
        
        private CallibrationView _callibrationView;
        private EventSystem _eventSystem;

        protected override void OnPreInitialize()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.LogWarning("Only one instance of MainSceneCore can be active at a time.");
                Destroy(gameObject);
                return;
            }
            
#if UNITY_EDITOR
            if (loadSettingsOnEditor)
                appPersistence.Load();
#else
            appPersistence.Load();
#endif
        }

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
    
    [Serializable]
    public class AppSettings
    {
        public float spawnDelay = 2f;
        public float spawnDistance = 180f;
        public float spawnMinScale = 0.4f;
        public float spawnMaxScale = 0.65f;
        public float spawnSfxDelay = 0.5f;
        public float sfxVolume = 1f;
        
        public int oscInPort = 7001;
        public int oscOutPort = 7000;
        
        public ClipRawData entranceClip = new ClipRawData()
        {
            layer = 1,
            column = 1
        };

        public ClipRawData interactiveClip = new ClipRawData()
        {
            layer = 1,
            column = 2
        };
    }

    [Serializable]
    public class AppPersistence : JSONPersistence<AppSettings>
    {
        public override AppSettings GetDefaultSettings()
        {
            return new AppSettings();
        }
    }
}
