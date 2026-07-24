using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LZY
{
    [DefaultExecutionOrder(-5)]
    public abstract class SceneCore : MonoBehaviour
    {
        public bool IsActive { get; private set; }

        protected bool _isInitialized;

        // scene core UI acts as the main canvas for the scene
        [SerializeField] protected SceneCoreView sceneCoreUI;
        [SerializeField] protected List<SceneService> sceneServices = new List<SceneService>();
        [SerializeField] protected bool initializeOnAwake = true;

        // Store in dictionary for easy access
        protected static Dictionary<string, SceneService> _serviceMap = new Dictionary<string, SceneService>();
        protected static SceneCoreView _sceneCoreView;

        public void Initialize()
        {
            if (_isInitialized) return;

            OnPreInitialize();
            RefreshServices();
            for (int i = 0; i < sceneServices.Count; i++)
            {
                sceneServices[i].Initialize();
            }
            _isInitialized = true;
            OnPostInitialize();
            Activate();
        }

        public void Deinitialize()
        {
            if (!_isInitialized) return;
            OnPreDeinitialize();
            Deactivate();
            for (int i = 0; i < sceneServices.Count; i++)
            {
                sceneServices[i].Deinitialize();
            }
            _serviceMap.Clear();
            _isInitialized = false;
            OnPostDeinitialize();
        }

        public void Activate()
        {
            if (!_isInitialized) return;
            for (int i = 0; i < sceneServices.Count; i++)
                sceneServices[i].Activate();
            IsActive = true;
            OnActivate();
        }

        public void Deactivate()
        {
            if (!IsActive) return;
            for (int i = 0; i < sceneServices.Count; i++)
            {
                sceneServices[i].Deactivate();
            }
            IsActive = false;
            OnDeactivate();
        }

        public static T GetService<T>() where T : SceneService
        {
            var name = typeof(T).Name;
            var isValueExist = _serviceMap.TryGetValue(name, out var sceneService);
            if (isValueExist == false)
                Debug.LogError($"GetService fail, {name} is not exist.");

            return isValueExist ? (T)sceneService : default;
        }

        public static T GetServiceView<T>() where T : BaseView
        {
            if (_sceneCoreView == null)
                _sceneCoreView = GetService<SceneCoreView>();
            
            if (_sceneCoreView != null)
                return _sceneCoreView.GetView<T>();

            return null;
        }

        public void Quit()
        {
            Deinitialize();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
		    Application.Quit();
#endif
        }

        private void Awake()
        {
            if (initializeOnAwake)
                Initialize();

            Input.multiTouchEnabled = true;
        }

        private void Update()
        {
            if (!IsActive) return;
            
            for (int i = 0, count = sceneServices.Count; i < count; i++)
                sceneServices[i].DoUpdate();
            
            OnUpdate();
        }

        private void OnDestroy()
        {
            Deinitialize();
        }

        // PROTECTED METHODS
        protected virtual void OnPreInitialize() { }

        protected virtual void OnPostInitialize() { }
        
        // its safe to call GetService and GetServiceView here
        protected virtual void OnActivate() { }
        
        protected virtual void OnUpdate() { }

        protected virtual void OnPreDeinitialize() { }

        protected virtual void OnDeactivate() { }
        
        protected virtual void OnPostDeinitialize() { }


        public void AddService(SceneService service)
        {
            if (service == null) return;
            
            if (_serviceMap.ContainsKey(service.ServiceId) == true)
            {
                Debug.LogWarning($"Service {service} already added.");
                return;
            }

            _serviceMap.Add(service.ServiceId, service);
        }

        public void RemoveService(SceneService service)
        {
            if (_serviceMap.ContainsValue(service) == true)
            {
                _serviceMap.Remove(nameof(service));
            }
        }

        private void RefreshServices()
        {
            sceneServices = GetComponentsInChildren<SceneService>().ToList();
            if (sceneCoreUI != null)
                sceneServices.Add(sceneCoreUI);
            _serviceMap.Clear();
            foreach (var service in sceneServices)
            {
                AddService(service);
            }
        }
    }
}
