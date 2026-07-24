using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LZY.Lidar
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class LidarDataVisualizer : MaskableGraphic
    {
        [Header("Draw Settings")]
        public float lidarLineThickness = 1f;
        public Color lidarLineColor = Color.white;
        public Color lidarObjectLineColor = Color.green;
        public float detectedObjectThickness = 4f;
        public float detectedObjectSize = 50;
        public Color detectedObjectColor = Color.blue;
        public float constraintAreaThickness = 4f;
        public Color constrainAreaColor = Color.red;

        private Vector2 _pivotOffset;
        private LidarDeviceController _deviceController;
        
        protected override void OnDisable()
        {
            base.OnDisable();
            Deinitialize();
        }

        public void Initialize(LidarDeviceController deviceController)
        {
            _deviceController = deviceController;
            RegisterCallbacks();
            CalculateData();
        }

        public void Deinitialize()
        {
            UnregisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            if (_deviceController != null)
            {
                _deviceController.onConnectionChanged.AddListener(OnConnectionChanged);
                _deviceController.onResolutionChanged.AddListener(OnResolutionChanged);
            }
        }

        private void UnregisterCallbacks()
        {
            if (_deviceController != null)
            {
                _deviceController.onConnectionChanged.RemoveListener(OnConnectionChanged);
                _deviceController.onResolutionChanged.RemoveListener(OnResolutionChanged);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (!Application.isPlaying) return;
            if (_deviceController == null) return;
            if (!_deviceController.IsConnected) return;
            if (_deviceController.Resolution.x <= 0 || _deviceController.Resolution.y <= 0) return;

            var index = 0;
            VisualizeURGLidarPoints(vh, ref index);
            VisualizeURGProcessedObjects(vh, ref index);
            VisualizeConstraintArea(vh, ref index);
        }
        
        private void OnConnectionChanged(bool isConnected)
        {
            if (isConnected)
                CalculateData();
        }

        private void OnResolutionChanged(Vector2 arg0)
        {
            CalculateData();
        }

        private void CalculateData()
        {
            if (_deviceController == null) return;
            if (_deviceController.Resolution.x <= 0 || _deviceController.Resolution.y <= 0) return;

            if (transform.parent.transform is RectTransform parentRectTransform)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = parentRectTransform.rect.size;
                _pivotOffset = LidarUtility.CalculatePivotOffset(rectTransform);
            }
        }

        private void VisualizeURGLidarPoints(VertexHelper vh, ref int index)
        {
            if (_deviceController.points == null) return;
            
            var centerPoint = LidarUtility.MapRectPoint(_deviceController.CenterPosition, _deviceController.Resolution, rectTransform.rect.size);
            var objectPointIndices = _deviceController.rawObjects.SelectMany(o => o.idList).ToHashSet();
            var rawPoints = _deviceController.points.Where((point, index) => !objectPointIndices.Contains(index)) .ToList();

            foreach (var point in rawPoints)
            {
                var adjustedPoint = LidarUtility.MapRectPoint(point, _deviceController.Resolution, rectTransform.rect.size);
                LidarUtility.CreateLine(vh, centerPoint, adjustedPoint, index, lidarLineThickness, lidarLineColor, _pivotOffset);
                index++;
            }
            
            foreach (var i in objectPointIndices)
            {
                var adjustedPoint = LidarUtility.MapRectPoint(_deviceController.points[i], _deviceController.Resolution, rectTransform.rect.size);
                LidarUtility.CreateLine(vh, centerPoint, adjustedPoint, index, lidarLineThickness, lidarObjectLineColor, _pivotOffset);
                index++;
            }
        }

        private void VisualizeURGProcessedObjects(VertexHelper vh, ref int index)
        {
            if (_deviceController.detectedObjects == null) return;
            
            foreach (var processedObject in _deviceController.detectedObjects)
            {
                var point = LidarUtility.MapRectPoint(processedObject.Position, _deviceController.Resolution, rectTransform.rect.size);
                LidarUtility.DrawWireSquare(vh, point, detectedObjectSize, ref index, detectedObjectThickness, detectedObjectColor, _pivotOffset);
            }
        }
        
        private void VisualizeConstraintArea(VertexHelper vh, ref int index)
        {
            LidarUtility.DrawWireRectangle(vh, _pivotOffset, rectTransform.sizeDelta, ref index, constraintAreaThickness, constrainAreaColor, _pivotOffset);
        }

        private void Update()
        {
            RefreshMesh();
        }
        
        public void RefreshMesh()
        {
            SetVerticesDirty();
        }
    }
}