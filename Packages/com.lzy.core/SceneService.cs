using UnityEngine;

namespace LZY
{
    public abstract class SceneService : MonoBehaviour
    {
        public string ServiceId => GetId();
        public bool IsActive => _isActive;
        public bool IsInitialized => _isInitialized;

        private bool _isInitialized;
        private bool _isActive;

        public void Initialize()
        {
            if (_isInitialized) return;

            OnInitialize();
            _isInitialized = true;
        }

        public void Deinitialize()
        {
            if (!_isInitialized) return;

            Deactivate();
            OnDeinitialize();

            _isInitialized = false;
        }

        public void Activate()
        {
            if (!_isInitialized) return;
            if (_isActive) return;

            OnActivate();
            _isActive = true;
        }

        public void DoUpdate()
        {
            if (!_isActive) return;
            OnUpdate();
        }

        public void Deactivate()
        {
            if (!_isActive) return;

            OnDeactivate();
            _isActive = false;
        }

        protected abstract string GetId();

        protected virtual void OnInitialize() { }

        protected virtual void OnDeinitialize() { }

        // its safe to call GetService and GetServiceView here
        protected virtual void OnActivate() { }

        protected virtual void OnDeactivate() { }

        protected virtual void OnUpdate() { }
    }
}
