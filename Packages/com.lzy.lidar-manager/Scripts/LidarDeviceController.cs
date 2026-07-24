// Modified from https://github.com/wangyangwang/hokuyo-unity

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace LZY.Lidar
{
    // http://sourceforge.net/p/urgnetwork/wiki/top_jp/
    // https://www.hokuyo-aut.co.jp/02sensor/07scanner/download/pdf/URG_SCIP20.pdf
    [RequireComponent(typeof(EthernetPAVODevice), typeof(EthernetURGDevice))]
    public class LidarDeviceController : MonoBehaviour
    {
        public bool IsConnected => _isConnected;
        public Vector2 CenterPosition => _centerPosition;
        public Vector2Int Resolution
        {
            get => settings.Resolution;
            set => SetResolution(value);
        }
        
        public Vector4 Margin
        {
            get => settings.Margin;
            set
            {
                settings.Margin = value;
                onResolutionChanged?.Invoke(settings.Resolution);
            }
        }
        
        public Vector2Int ScreenSize
        {
            get => settings.ScreenSize;
            set => settings.ScreenSize = value;
        }

        public int SensorScanSteps { get; private set; }
        
        [SerializeField] private bool connectOnInit = true;
        [SerializeField] private LidarCallibrationView callibrationView;
        public LidarSettings settings;

        [Header("Debug Draw")]
        [SerializeField] private bool debugDrawDistance = false;
        [SerializeField] private bool drawObjectRays = true;
        [SerializeField] private bool drawObjectCenterRay = true;
        [SerializeField] private bool drawObject = true;
        [SerializeField] private bool drawProcessedObject = true;
        [SerializeField] private bool drawRunningLine = true;
        
        [Header("Debug Draw Colors")]
        [SerializeField] private Color distanceColor = Color.white;
        [SerializeField] private Color strengthColor = Color.red;
        [SerializeField] private Color objectColor = Color.green;
        [SerializeField] private Color processedObjectColor = Color.cyan;
        
        public UnityEvent<ProcessedObject> onNewObject;
        public UnityEvent<ProcessedObject> onLostObject;
        public UnityEvent<Vector2> onResolutionChanged;
        public UnityEvent<bool> onConnectionChanged;
        
        private Rect _resolutionRect;
        private long[] _distanceConstrainList;
        private List<long> _smoothByTimePreviousList = new ();
        private ILidarDevice _device;
        [HideInInspector] public List<RawObject> rawObjects = new ();
        private List<ProcessedObject> _processedObjects = new ();
        [HideInInspector] public List<ProcessedObject> detectedObjects = new ();
        [HideInInspector] public List<long> originalDistances = new ();
        [HideInInspector] public List<long> croppedDistances = new ();
        [HideInInspector] public List<long> strengths = new (); // currently strength is ignored
        [HideInInspector] public Vector2[] directions;
        [HideInInspector] public Vector2[] points;
        private Vector2 _centerPosition;
        private bool _isConnected;
        private bool _isInitialized;

        private HashSet<int> _usedFingerIds = new HashSet<int>();
        private Queue<int> _availableFingerIds = new Queue<int>();

        private const int TouchStartIndex = 2000;

        private void OnDestroy()
        {
            if (_device != null)
                _device.onConnectionChanged.RemoveListener(OnURGDeviceConnectionChanged);
        }

        private void OnURGDeviceConnectionChanged(bool newValue)
        {
            _isConnected = newValue;
            onConnectionChanged?.Invoke(_isConnected);
        }

        public void Initialize()
        {
            if (_isInitialized) return;
            
            switch (settings.DeviceType)
            {
                case LidarDeviceType.EthernetHokuyoURG:
                    _device = GetComponent<EthernetURGDevice>();
                    break;
                case LidarDeviceType.EthernetSiminicsPAVO:
                    _device = GetComponent<EthernetPAVODevice>();
                    break;
                default:
                    _device = null;
                    break;
            }

            if (_device == null)
            {
                Debug.LogError("[LidarDeviceController] Unable to initialize, Lidar Device is null");
                return;
            }

            _device.onConnectionChanged.AddListener(OnURGDeviceConnectionChanged);
            _isInitialized = true;
            
            if (callibrationView != null)
                callibrationView.Initialize(this);
            
            if (connectOnInit)
                Connect();
        }

        public void Connect()
        {
            if (!_isInitialized)
                Initialize();
            
            if (_device == null || _device.isConnected) return;

            croppedDistances = new List<long>();
            strengths = new List<long>();
            rawObjects = new List<RawObject>();
            detectedObjects = new List<ProcessedObject>();
            _processedObjects = new List<ProcessedObject>();
            _resolutionRect = new Rect(Vector2.zero, settings.Resolution);
            for (int i = 0; i < settings.MaxTouchCount; i++)
                _availableFingerIds.Enqueue(TouchStartIndex + i);
            
            if (_device is EthernetURGDevice urgEthernet)
            {
                if (settings.ConnectionType == LidarConnectionType.DeviceDefaultAddress)
                    urgEthernet.Connect();
                else
                    urgEthernet.Connect(settings.CustomIPAddress, settings.CustomPortNumber);
            } 
            else if (_device is EthernetPAVODevice pavoEthernet)
            {
                if (settings.ConnectionType == LidarConnectionType.DeviceDefaultAddress)
                    pavoEthernet.Connect();
                else
                    pavoEthernet.Connect(settings.CustomIPAddress, (ushort)settings.CustomPortNumber);
            }
        }
        
        public void Disconnect()
        {
            if (_device == null || !_device.isConnected) return;
            
            _device.Disconnect();
        }

        private void CacheDirections()
        {
            var deltaAngle = _device.maxDegreeScope - _device.minDegreeScope;
            var step = deltaAngle / SensorScanSteps;
            var startAngle = _device.minDegreeScope + 90f;
            directions = new Vector2[SensorScanSteps];
            for (int i = 0; i < directions.Length; i++)
            {
                var angleDeg = startAngle + settings.RotationDegrees + i * step;
                var angleRad = angleDeg * Mathf.Deg2Rad;
                var cos = Mathf.Cos(angleRad);
                var sin = Mathf.Sin(angleRad);
                directions[i] = new Vector2(-cos, -sin);
            }
        }

        private void Update()
        {
            if (_device == null) return;
            if (!_device.isConnected) return;
            if (settings.SmoothKernelSize % 2 == 0) { settings.SmoothKernelSize += 1; }

            originalDistances.Clear();
            lock (_device.distances)
            {
                if (_device == null || _device.distances.Count <= 0) return;
                originalDistances = _device.distances.ToList();
            }
            if (originalDistances.Count <= 0) return;

            //Setting up things, one time
            if (SensorScanSteps <= 0)
            {
                SensorScanSteps = _device.distances.Count;
                _distanceConstrainList = new long[SensorScanSteps];
                CacheDirections();
                CalculateCenterPosition();
                CalculateDistanceConstrainList(SensorScanSteps);
            }

            if (Time.frameCount % 10 == 0)
            {
                CacheDirections();
                CalculateCenterPosition();
                CalculateDistanceConstrainList(SensorScanSteps);
            }

            CalculateDetectionArea();
            UpdateDetectObjects();
        }

        private void CalculateDetectionArea()
        {
            if (settings.SmoothDistanceCurve)
                originalDistances = SmoothDistanceCurve(originalDistances, settings.SmoothKernelSize);
            if (settings.SmoothDistanceByTime)
                originalDistances = SmoothDistanceCurveByTime(originalDistances, ref _smoothByTimePreviousList, settings.TimeSmoothFactor);
            
            croppedDistances.Clear();
            for (int i = 0; i < originalDistances.Count && i < _distanceConstrainList.Length; i++)
            {
                if (originalDistances[i] > _distanceConstrainList[i] || originalDistances[i] <= 0)
                    croppedDistances.Add(_distanceConstrainList[i]);
                else
                    croppedDistances.Add(originalDistances[i]);
            }
            
            points = new Vector2[croppedDistances.Count];
            for (int i = 0; i < croppedDistances.Count && i < directions.Length; i++)
            {
                var point = directions[i] * croppedDistances[i];
                if (point == Vector2.zero)
                {
                    points[i] = default;
                    continue;
                }
                
                points[i] = _centerPosition + point;
            }
        }

        private List<long> SmoothDistanceCurve(List<long> croppedDistances, int smoothKernelSize)
        {
            var movingAverageFilterFilter = new MovingAverageFilter();
            return movingAverageFilterFilter.Filter(croppedDistances.ToArray(), smoothKernelSize).ToList();
        }

        private List<RawObject> GetRawObjectsInsideRect(List<long> croppedDistances, long[] distanceConstrainList)
        {
            if (directions.Length <= 0)
                return new List<RawObject>();

            int objectId = 0;

            var resultList = new List<RawObject>();
            bool isGrouping = false;
            for (int i = 1; i < croppedDistances.Count - 1; i++)
            {

                float deltaA = Mathf.Abs(croppedDistances[i] - croppedDistances[i - 1]);
                float deltaB = Mathf.Abs(croppedDistances[i + 1] - croppedDistances[i]);

                var dist = croppedDistances[i];
                var ubDist = distanceConstrainList[i];

                //
                if (dist < ubDist && (deltaA < settings.DeltaLimit && deltaB < settings.DeltaLimit))
                {
                    if (!isGrouping)
                    {
                        //is not grouping
                        isGrouping = true;
                        //start a new object and start grouping
                        RawObject newObject = new RawObject(directions, objectId++);
                        newObject.idList.Add(i);
                        newObject.distancesList.Add(dist);
                        resultList.Add(newObject);
                    }
                    else
                    {
                        //already grouping, add stuff to existing group
                        var newObject = resultList[resultList.Count - 1];
                        newObject.idList.Add(i);
                        newObject.distancesList.Add(dist);
                    }

                }
                else
                {
                    if (isGrouping)
                    {
                        isGrouping = false;
                    }
                }
            }
            //remove the ones that might be noise
            resultList.RemoveAll(item => item.idList.Count < settings.NoiseLimit);
            //all finished, calculate position and save it for later use
            resultList.ForEach(i => { i.CalculatePosition(); });
            return resultList;
        }

        private void UpdateDetectObjects()
        {
            var newlyDetectedObjects = GetRawObjectsInsideRect(croppedDistances, _distanceConstrainList);
            for (int i = newlyDetectedObjects.Count - 1; i >= 0; i--)
            {
                newlyDetectedObjects[i].position += _centerPosition;
                if (!IsInsideResolutionRect(newlyDetectedObjects[i].position))
                    newlyDetectedObjects.RemoveAt(i);
            }
            rawObjects = new List<RawObject>(newlyDetectedObjects);
            
            lock (_processedObjects)
            {
                //update existing objects
                if (_processedObjects.Count != 0)
                {
                    foreach (var oldObj in _processedObjects)
                    {
                        var closeObjects = new List<RawObject>();
                        RawObject closestObject = null;
                        var closestDistance = float.MaxValue;
                        foreach (var newObj in newlyDetectedObjects)
                        {
                            float distance = Vector3.Distance(newObj.position, oldObj.Position);
                            if (distance < settings.DistanceThresholdForMerge)
                            {
                                closeObjects.Add(newObj);
                                if (distance < closestDistance)
                                {
                                    closestDistance = distance;
                                    closestObject = newObj;
                                }
                            }
                        }

                        if (closeObjects.Count == 0)
                        {
                            oldObj.Update();
                        }
                        else
                        {
                            var averagePosition = Vector2.zero;
                            foreach (var obj in closeObjects)
                                averagePosition += obj.position;

                            averagePosition /= closeObjects.Count;
                            oldObj.Update(averagePosition, closestObject.size);

                            foreach (var obj in closeObjects)
                                newlyDetectedObjects.Remove(obj);
                        }
                    }

                    // remove all missed and out of bonds objects
                    for (int i = _processedObjects.Count - 1; i >= 0; i--)
                    {
                        var obj = _processedObjects[i];
                        if (obj.IsClear)
                        {
                            if (obj.IsValid) onLostObject?.Invoke(obj);
                            _processedObjects.RemoveAt(i);
                            ReturnFingerId(obj.fingerId);
                        }
                    }

                    //create new object for those newobject that cannot find match from the old objects
                    foreach (var leftOverNewObject in newlyDetectedObjects)
                    {
                        var fingerId = GetAvailableFingerId();
                        if (fingerId == -1) break;
                        
                        var newbie = new ProcessedObject(this, fingerId, leftOverNewObject.position, leftOverNewObject.size, settings.ObjectPositionSmoothTime);
                        _processedObjects.Add(newbie);
                    }
                }
                else //add all raw objects into detectedObjects
                {
                    var used = new HashSet<RawObject>();
                    foreach (var obj in rawObjects.ToList())
                    {
                        if (used.Contains(obj)) continue;
                        
                        var fingerId = GetAvailableFingerId();
                        if (fingerId == -1) break;
                        
                        var group = new List<RawObject> { obj };
                        used.Add(obj);

                        foreach (var otherObj in rawObjects)
                        {
                            if (used.Contains(otherObj) || obj == otherObj) continue;

                            float distance = Vector2.Distance(otherObj.position, obj.position);
                            if (distance < settings.DistanceThresholdForMerge)
                            {
                                group.Add(otherObj);
                                used.Add(otherObj);
                            }
                        }
                        
                        var averagePosition = Vector2.zero;
                        foreach (var o in group)
                            averagePosition += o.position;
                        averagePosition /= group.Count;
                        
                        var newbie = new ProcessedObject(this, fingerId, averagePosition, obj.size, settings.ObjectPositionSmoothTime);
                        _processedObjects.Add(newbie);
                    }
                }
                
                // filter objects, object that has age less than cancelBriefTime will be ignored
                detectedObjects.Clear();
                foreach (var obj in _processedObjects)
                {
                    if (obj.Age > settings.CancelBriefTime)
                    {
                        switch (obj.TouchPhase)
                        {
                            case TouchPhase.Moved:
                                detectedObjects.Add(obj);
                                break;
                            case TouchPhase.Ended:
                                detectedObjects.Add(obj);
                                break;
                            case TouchPhase.Began:
                                detectedObjects.Add(obj);
                                onNewObject?.Invoke(obj);
                                obj.IsValid = true;
                                break;
                        }
                    }
                }
            }
        }

        private int GetAvailableFingerId()
        {
            if (_availableFingerIds.Count == 0)
            {
                Debug.LogWarning("[LidarDeviceController] Maximum number of URG touches reached.");
                return -1;
            }

            var fingerId = _availableFingerIds.Dequeue();
            _usedFingerIds.Add(fingerId);
            return fingerId;
        }
        
        private void ReturnFingerId(int fingerId)
        {
            if (_usedFingerIds.Contains(fingerId))
            {
                _usedFingerIds.Remove(fingerId);
                _availableFingerIds.Enqueue(fingerId);
            }
        }

        private List<long> SmoothDistanceCurveByTime(List<long> newList, ref List<long> previousList, float smoothFactor)
        {
            if (previousList.Count <= 0)
            {
                previousList = newList;
                return newList;
            }
            else
            {
                long[] result = new long[newList.Count];
                for (int i = 0; i < result.Length; i++)
                {

                    float diff = newList[i] - previousList[i];
                    if (diff > settings.TimeSmoothBreakingDistanceChange)
                    {
                        result[i] = newList[i];
                        previousList[i] = result[i];
                    }
                    else
                    {
                        float smallDiff = diff * smoothFactor;
                        float final = previousList[i] + smallDiff;

                        result[i] = (long)final;
                        previousList[i] = result[i];
                    }
                }
                return result.ToList();
            }

        }

        public ProcessedObject GetObjectByGuid(int id)
        {
            ProcessedObject o = null;
            foreach (var obj in detectedObjects)
            {
                if (obj.fingerId == id)
                {
                    o = obj;
                }
            }
            if (o == null) Debug.LogWarning("[LidarDeviceController] Cannot find object with id " + id);
            return o;
        }

        public List<ProcessedObject> GetObjects(float ageFilter = 0.5f)
        {
            var o = from obj in detectedObjects
                    where obj.Age > ageFilter
                    select obj;
            return o.ToList();
        }

        private void CalculateDistanceConstrainList(int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                Vector2 dir = directions[i];
                Vector2 targetPosition = _centerPosition + dir.normalized * 10000f;
                _distanceConstrainList[i] = (long)GetLimitedDistance(_centerPosition, targetPosition);
            }
        }
        
        float GetLimitedDistance(Vector2 start, Vector2 target)
        {
            Vector2 limitedPosition = target;

            var offsetLeft = settings.Margin.x;
            var offsetRight = settings.Margin.y;
            var offsetTop = settings.Margin.z;
            var offsetBottom = settings.Margin.w;
            
            var minBounds = new Vector2(offsetLeft, offsetBottom);
            var maxBounds = new Vector2(settings.Resolution.x - offsetRight, settings.Resolution.y - offsetTop);
            if (target.x < minBounds.x)
            {
                limitedPosition.x = minBounds.x;
                limitedPosition.y = start.y + (minBounds.x - start.x) * (target.y - start.y) / (target.x - start.x);
            }
            else if (target.x > maxBounds.x)
            {
                limitedPosition.x = maxBounds.x;
                limitedPosition.y = start.y + (maxBounds.x - start.x) * (target.y - start.y) / (target.x - start.x);
            }

            if (limitedPosition.y < minBounds.y)
            {
                limitedPosition.y = minBounds.y;
                limitedPosition.x = start.x + (minBounds.y - start.y) * (target.x - start.x) / (target.y - start.y);
            }
            else if (limitedPosition.y > maxBounds.y)
            {
                limitedPosition.y = maxBounds.y;
                limitedPosition.x = start.x + (maxBounds.y - start.y) * (target.x - start.x) / (target.y - start.y);
            }

            return Vector2.Distance(start, limitedPosition);
        }
        
        public void SetResolution(Vector2Int newValue)
        {
            if (settings.Resolution == newValue) return;
            
            settings.Resolution = newValue;
            _resolutionRect.size = settings.Resolution;
            onResolutionChanged?.Invoke(settings.Resolution);
        }
        
        public void SetResolutionWidth(int resolutionWidth)
        {
            if (resolutionWidth == settings.Resolution.x) return;
            
            settings.Resolution.x = resolutionWidth;
            _resolutionRect.width = resolutionWidth;
            onResolutionChanged?.Invoke(settings.Resolution);
        }
        
        public void SetResolutionHeight(int resolutionHeight)
        {
            if (resolutionHeight == settings.Resolution.y) return;

            settings.Resolution.y = resolutionHeight;
            _resolutionRect.height = resolutionHeight;
            onResolutionChanged?.Invoke(settings.Resolution);
        }

        private bool IsInsideResolutionRect(Vector2 position)
        {
            return _resolutionRect.Contains(position);
        }
        
        private void CalculateCenterPosition()
        {
            switch (settings.DeviceAnchor)
            {
                case LidarDeviceAnchor.TOP:
                    _centerPosition = settings.DeviceOffset + new Vector2(settings.Resolution.x / 2f, settings.Resolution.y);
                    return;
                case LidarDeviceAnchor.BOTTOM:
                    _centerPosition = settings.DeviceOffset + new Vector2(settings.Resolution.x / 2f, 0);
                    return;
                case LidarDeviceAnchor.LEFT:
                    _centerPosition = settings.DeviceOffset + new Vector2(0, settings.Resolution.y / 2f);
                    break;
                case LidarDeviceAnchor.RIGHT:
                    _centerPosition = settings.DeviceOffset + new Vector2(settings.Resolution.x, settings.Resolution.y / 2f);
                    break;
            }
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position + new Vector3(settings.Resolution.x / 2f, settings.Resolution.y / 2f), new Vector3(settings.Resolution.x, settings.Resolution.y, 1));
            
            var positionOffset = transform.position + (Vector3)_centerPosition;
            if (debugDrawDistance && croppedDistances != null)
            {
                for (int i = 0; i < croppedDistances.Count; i++)
                {
                    var dir = directions[i];
                    var dist = croppedDistances[i];
                    Debug.DrawLine(positionOffset, (Vector3)(dist * dir) + positionOffset, distanceColor);
                }
            }
            
            if (rawObjects == null) return;
            for (int i = 0; i < rawObjects.Count; i++)
            {
                var obj = rawObjects[i];
                if (obj.idList.Count == 0 || obj.distancesList.Count == 0) return;
                var dir = directions[obj.medianId];
                long dist = obj.medianDist;
                if (drawObjectRays)
                {
                    for (int j = 0; j < obj.distancesList.Count; j++)
                    {
                        var myDir = directions[obj.idList[j]];
                        Debug.DrawLine(positionOffset, (Vector3)(myDir * obj.distancesList[j]) + positionOffset, objectColor);
                    }
                }
                if (drawObjectCenterRay) Debug.DrawLine(positionOffset, (Vector3)(dir * dist) + positionOffset, Color.blue);
                if (drawObject) Gizmos.DrawWireCube(transform.position + (Vector3)obj.position, new Vector3(100, 100, 0));
            }

            if (drawProcessedObject)
            {
                foreach (var pObj in detectedObjects)
                {
                    Gizmos.color = processedObjectColor;
                    float size = 30;// pObj.size;
                    Gizmos.DrawCube(transform.position + pObj.Position, new Vector3(size, size, 1));
                    UnityEditor.Handles.Label(transform.position + pObj.Position, pObj.Position.ToString());
                }
            }

            if (drawRunningLine)
            {
                for (int i = 1; i < croppedDistances.Count; i++)
                    Gizmos.DrawLine(new Vector3(i, settings.Resolution.y + croppedDistances[i], 0), new Vector3(i - 1, settings.Resolution.y + croppedDistances[i - 1], 0));
            }
        }
        #endif
    }
    
    [Serializable]
	public class LidarSettings
    {
        public const string SETTINGS_VERSION = "2.0.0";
        public string Version = SETTINGS_VERSION;

        public int MaxTouchCount = 25;

        [Header("Connection with Sensor")] 
        public LidarConnectionType ConnectionType = LidarConnectionType.DeviceDefaultAddress;
        public string CustomIPAddress = EthernetPAVODevice.DEFAULT_IP_ADDRESS;
		public int CustomPortNumber = EthernetPAVODevice.DEFAULT_PORT;
		
		[Header("Detection Area")]
        public LidarDeviceType DeviceType = LidarDeviceType.EthernetSiminicsPAVO;
		public LidarDeviceAnchor DeviceAnchor = LidarDeviceAnchor.BOTTOM;
		public Vector2 DeviceOffset;
		[Range(0, 360)]
		public float RotationDegrees;
		public Vector2Int Resolution = new Vector2Int(1920, 1080);
        public Vector4 Margin = new Vector4(0, 0, 0, 0);
        public Vector2Int ScreenSize = new Vector2Int(1920, 1080);

		[Header("Object Tracking")]
		[Tooltip("Minimum age an object must reach to be tracked. Objects younger than this threshold will not be tracked.")]
		[Range(0f, 1f)] 
		public float CancelBriefTime = 0f;
		[Tooltip("The threshold distance within which objects will be merged into a single tracked object.")]
		public float DistanceThresholdForMerge = 0.65f;
		[Tooltip("Smoothing time for object positions, controlling how quickly they interpolate.")]
		[Range(0f, 1f)] 
		public float ObjectPositionSmoothTime = 0f;
        [Tooltip("Specifies the maximum allowable change in detection between frames.")]
		[Range(1, 1000)] 
		public int DeltaLimit = 65;
        [Tooltip("Defines the limit of noise allowed in detection. Lower values means more sensitive object detection.")]
		[Range(1, 1000)] 
		public int NoiseLimit = 2;
		public bool SmoothDistanceCurve = false;
		[Range(1, 130)] 
		[Tooltip("The size of the kernel used for smoothing. The kernel size must be odd and greater than 1. If change between two consecutive frame is bigger than the size, then do not do smoothing.")]
		public int SmoothKernelSize = 10;
		public bool SmoothDistanceByTime = false;
		[Range(1, 500)] 
		[Tooltip("The maximum distance change allowed between frames for smoothing to be applied. If the change exceeds this value, smoothing will not be applied.")]
		public int TimeSmoothBreakingDistanceChange = 200;
		[Range(0.01f, 1f)] 
		[Tooltip("The factor by which the distance smoothing is multiplied to smooth out abrupt changes over time.")] 
		public float TimeSmoothFactor = 0.01f;
	}
    
    public enum LidarConnectionType
    {
        DeviceDefaultAddress = 0,
        CustomAddress
    }

    public enum LidarDeviceType
    {
        EthernetSiminicsPAVO = 0,
        EthernetHokuyoURG,
    }
    
    public enum LidarDeviceAnchor
    {
        TOP = 0,
        BOTTOM = 1,
        LEFT = 2,
        RIGHT = 3,
    }
}