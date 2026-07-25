using System;
using System.Collections.Generic;
using Lean.Pool;
using LZY.Lidar;
using LZY.SimpleAudioManager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LZY.BND
{
    public class ButterflySpawner : SceneService
    {
        protected override string GetId() => nameof(ButterflySpawner);
        
        [SerializeField] private Camera spawnCamera;
        [SerializeField] private Transform spawnParent;
        [SerializeField] private ButterflyParticle particlePrefab;

        [Header("SFX Settings")]
        [SerializeField] private string spawnSfxKey = "SFX_Spawn";

        private Dictionary<int, SpawnedParticleData> _spawnedParticleDict = new Dictionary<int, SpawnedParticleData>();
        private float _spawnSfxTimer;

        private const int MouseFingerId = -1;
        
        protected override void OnUpdate()
        {
            UpdateTouch();
            UpdateSpawnSfx();
        }

        private void UpdateTouch()
        {
            if (LidarDeviceManager.Devices == null || LidarDeviceManager.Devices.Count == 0) return;

            var lidarDevice = LidarDeviceManager.Devices[0];
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                var mousePos = LidarUtility.MapRectPoint(Input.mousePosition, Screen.width, Screen.height, lidarDevice.ScreenSize.x, lidarDevice.ScreenSize.y);
                SpawnParticle(MouseFingerId, mousePos);
            }

            if (Input.GetMouseButton(0))
            {
                var mousePos = LidarUtility.MapRectPoint(Input.mousePosition, Screen.width, Screen.height, lidarDevice.ScreenSize.x, lidarDevice.ScreenSize.y);
                TrySpawnFlowerParticleOnLidarObjectMoved(MouseFingerId, mousePos);
            }
#endif

            foreach (var lidarObj in LidarDeviceManager.Devices[0].detectedObjects)
            {
                if (lidarObj.TouchPhase == TouchPhase.Began)
                    SpawnParticle(lidarObj.fingerId, lidarObj.ScreenPosition);

                if (lidarObj.TouchPhase == TouchPhase.Moved || lidarObj.TouchPhase == TouchPhase.Stationary)
                    TrySpawnFlowerParticleOnLidarObjectMoved(lidarObj.fingerId, lidarObj.ScreenPosition);
            }
        }

        private void TrySpawnFlowerParticleOnLidarObjectMoved(int fingerId, Vector2 screenPosition)
        {
            if (_spawnedParticleDict.TryGetValue(fingerId, out var spawnedFlowerData))
            {
                var lifetime = Time.time - spawnedFlowerData.LastSpawnedTime;
                var deltaMovement = screenPosition - spawnedFlowerData.LastSpawnedPos;
                if (lifetime >= MainSceneCore.settings.spawnDelay || deltaMovement.magnitude > MainSceneCore.settings.spawnDistance)
                    SpawnParticle(fingerId, screenPosition);
            }
            else
            {
                SpawnParticle(fingerId, screenPosition);
            }
        }
        
        private void UpdateSpawnSfx()
        {
            if (_spawnSfxTimer > 0)
                _spawnSfxTimer -= Time.deltaTime;
        }

        private void SpawnParticle(int fingerId, Vector2 screenPosition)
        {
            var finalScreenPos = new Vector3(screenPosition.x, screenPosition.y, spawnCamera.nearClipPlane);
            var worldPos = spawnCamera.ScreenToWorldPoint(finalScreenPos);
            var particle = LeanPool.Spawn(particlePrefab, spawnParent);
            particle.transform.position = worldPos;
            particle.transform.rotation = Quaternion.identity;
            particle.transform.localScale = Vector3.one * Random.Range(MainSceneCore.settings.spawnMinScale, MainSceneCore.settings.spawnMaxScale);
            particle.SetFlippedX(Random.value > 0.5f);
            
            if (_spawnedParticleDict.TryGetValue(fingerId, out var spawnedFlowerData))
            {
                spawnedFlowerData.LastSpawnedPos = screenPosition;
                spawnedFlowerData.LastSpawnedTime = Time.time;
            }
            else
            {
                _spawnedParticleDict.Add(fingerId, new SpawnedParticleData()
                {
                    LastSpawnedPos = screenPosition,
                    LastSpawnedTime = Time.time
                });
            }

            TryPlayParticleSFX();
        }

        private void TryPlayParticleSFX()
        {
            if (_spawnSfxTimer <= 0)
            {
                AudioManager.PlaySFX(spawnSfxKey);
                _spawnSfxTimer = MainSceneCore.settings.spawnSfxDelay;
            }
        }
    }

    [Serializable]
    public class SpawnedParticleData
    {
        public Vector2 LastSpawnedPos;
        public float LastSpawnedTime;
    }
}