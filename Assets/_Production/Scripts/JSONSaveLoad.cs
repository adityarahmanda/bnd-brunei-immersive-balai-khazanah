using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace LZY.BND
{
    [Serializable]
    public abstract class JSONSaveLoad<T>
    {
        protected virtual string relativeSettingsPath
        {
            get
            {
                var typeName = typeof(T).Name;
                return $"LZY_{typeName}.json";
            }
        }

        [SerializeField] protected T _settings;
        
        public T GetSettings()
        {
            if (_settings == null)
            {
                _settings = GetDefaultSettings();
            }
            
            return _settings;
        }
        
        public abstract T GetDefaultSettings();
        
        public string GetSaveFilePath()
        {
#if UNITY_EDITOR
            var folderPath = Application.dataPath;
#else
            var folderPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
#endif
            return Path.Combine(folderPath, relativeSettingsPath);
        }
        
        public void Load()
        {
            var path = GetSaveFilePath();
            var hasSettingsFile = File.Exists(path);
            if (hasSettingsFile)
            {
                try
                {
                    var json = File.ReadAllText(path);
                    _settings = JsonConvert.DeserializeObject<T>(json);
                    Debug.Log("Settings loaded from: " + path);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            else
            {
                Save();
            }
        }

        public void Save(Formatting serializerFormatting = Formatting.Indented, JsonSerializerSettings serializerSettings = null)
        {
            if (serializerSettings == null)
            {
                serializerSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
            }

            if (_settings == null)
                _settings = GetDefaultSettings();
            
            var json = JsonConvert.SerializeObject(_settings, serializerFormatting, serializerSettings);
            var path = GetSaveFilePath();
            
            File.WriteAllText(path, json);
            Debug.Log("Settings saved to: " + path);
        }
    }
}