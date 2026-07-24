using UnityEngine;
using UnityEngine.EventSystems;

namespace LZY.BND
{
    public class MainSceneCore : SceneCore
    {
        [Header("UI Navigation Runtime")]
        [SerializeField] private GameObject lastSelectedElement;
        
        private EventSystem _eventSystem;

        protected override void OnActivate()
        {
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
        }
    }
}
