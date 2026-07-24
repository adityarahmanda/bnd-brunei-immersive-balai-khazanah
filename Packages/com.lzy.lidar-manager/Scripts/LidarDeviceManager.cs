using System;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Newtonsoft.Json;

namespace LZY.Lidar
{
    public class LidarDeviceManager : MonoBehaviour
    {
        private const string LidarSettingsJson = "LZY_LidarSettings.json";

        [SerializeField] private bool initOnStart = true;
        [SerializeField] private List<LidarDeviceController> devices = new List<LidarDeviceController>();

        public static LidarDeviceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new Exception ("[LidarDeviceManager] Could not find the LidarDeviceManager object. Please ensure you have added the LidarDeviceManager Prefab to your scene.");
                }
                return _instance;
            }
        }

        public static List<LidarDeviceController> Devices => _instance?.devices;

        private static LidarDeviceManager _instance;

        private void Start()
        {
            if (initOnStart) Initialize();
        }
        
        public void Initialize()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);    
            }
            
            LoadSettings();
            foreach (var device in devices)
                device.Initialize();
        }
        
        private static string GetSettingsPath()
        {
#if UNITY_EDITOR
            var folderPath = Application.dataPath;
#else
            var folderPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
#endif
            return Path.Combine(folderPath, LidarSettingsJson);
        }
        
        [ContextMenu("Save Settings")]
        public void SaveSettings()
        {
            var settingsList = new List<LidarSettings>();
            foreach (var device in devices)
                settingsList.Add(device.settings);
            var json = JsonConvert.SerializeObject(settingsList, Formatting.Indented, new JsonSerializerSettings 
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            File.WriteAllText(GetSettingsPath(), json);
            Debug.Log("[LidarDeviceManager] Settings saved to: " + GetSettingsPath());
#if UNITY_EDITOR
            if (!Application.isPlaying)
                AssetDatabase.Refresh();
#endif
        }
        
        [ContextMenu("Load Settings")]
        public void LoadSettings()
        {
            var settingsPath = GetSettingsPath();
            var hasSettingsFile = File.Exists(settingsPath);
            var settingsList = new List<LidarSettings>();
            if (hasSettingsFile)
            {
                var json = File.ReadAllText(settingsPath);
                settingsList = JsonConvert.DeserializeObject<List<LidarSettings>>(json);

                var mismatchVersion = false;
                foreach (var settings in settingsList)
                {
                    if (settings.Version != LidarSettings.SETTINGS_VERSION)
                    {
                        Debug.LogWarning("[LidarDeviceManager] Settings version not matched with current Lidar package version. Some settings may not be loaded properly.");
                        settings.Version = LidarSettings.SETTINGS_VERSION;
                        mismatchVersion = true;
                    }
                }

                for (var i = 0; i < devices.Count && i < settingsList.Count; i++)
                    devices[i].settings = settingsList[i];
                
                if (mismatchVersion)
                    SaveSettings();
            }
            else
            {
                SaveSettings();
            }
        }
    }
}