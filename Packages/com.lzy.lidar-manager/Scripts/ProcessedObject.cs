// https://github.com/wangyangwang/hokuyo-unity

using System;
using UnityEngine;

namespace LZY.Lidar
{
    [Serializable]
    public class ProcessedObject
    {
        public static readonly int MISSING_FRAME_LIMIT = 5;
        public static readonly int ENDED_FRAME_LIMIT = 1;
        public readonly int fingerId;

        public LidarDeviceController Controller => _controller;
        public Vector3 Position => position;
        public Vector3 ScreenPosition => screenPosition;
        public float Size => size;
        public Vector3 DeltaMovement => deltaMovement;
        public float Age => Time.time - _birthTime;
        public TouchPhase TouchPhase => touchPhase;
        public bool IsClear;
        public bool IsValid;

        [SerializeField] private Vector3 position;
        [SerializeField] private Vector3 screenPosition;
        [SerializeField] private Vector3 deltaMovement;
        [SerializeField] private float size;
        [SerializeField] private TouchPhase touchPhase;
        
        private float _birthTime;
        private int _missingFrame;
        private bool _useSmooth = true;
        private Vector3 _currentVelocity;
        private Vector3 _oldPosition;
        private float _posSmoothTime;

        private LidarDeviceController _controller;

        public ProcessedObject(LidarDeviceController controller, int fingerId, Vector3 position, float size, float objectPositionSmoothTime = 0.2f)
        {
            _controller = controller;
            this.fingerId = fingerId;
            SetPosition(position);
            this.size = size;
            _posSmoothTime = objectPositionSmoothTime;
            _currentVelocity = new Vector3();
            _birthTime = Time.time;
            touchPhase = TouchPhase.Began;
            IsValid = false;
        }

        private void SetPosition(Vector3 position)
        {
            this.position = position;
            screenPosition = LidarUtility.MapRectPoint(position, _controller.Resolution, _controller.ScreenSize);
        }

        public static ProcessedObject Clone(ProcessedObject obj)
        {
            var s = JsonUtility.ToJson(obj);
            var newObj = JsonUtility.FromJson<ProcessedObject>(s);
            return newObj;
        }

        public void Update(Vector3 newPos, float newSize)
        {
            size = newSize;
            _oldPosition = Position;

            if (_useSmooth)
                SetPosition(Vector3.SmoothDamp(Position, newPos, ref _currentVelocity, _posSmoothTime));
            else
                SetPosition(newPos);
            
            _missingFrame = 0;
            deltaMovement = Position - _oldPosition;
            
            if (!IsValid) return;
            
            touchPhase = TouchPhase.Moved;
        }
        
        public void Update()
        {
            if (IsClear) return;
            
            _missingFrame++;
            if (touchPhase == TouchPhase.Ended && _missingFrame >= ENDED_FRAME_LIMIT)
            {
                IsClear = true;
                return;
            }
            
            if (touchPhase == TouchPhase.Ended) return;
            
            if (_missingFrame >= MISSING_FRAME_LIMIT)
                touchPhase = TouchPhase.Ended;
            else if (IsValid) 
                touchPhase = TouchPhase.Stationary;
        }
    }

}
