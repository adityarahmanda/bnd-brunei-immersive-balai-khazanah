using UnityEngine;
using System;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

#pragma warning disable 414
namespace LZY.SimpleTextLogger
{	
    [DefaultExecutionOrder(-10)] // run before scene core so it would log the scene core too
    public class TextLogger : MonoBehaviour
    {
        private static TextLogger _instance;
        
        private const string fileName = "LZY_Logger.txt";

        [SerializeField] private bool logMessage = true;
        [SerializeField] private bool stacktraceMessage = false;
        [SerializeField] private bool logWarning = true;
        [SerializeField] private bool stacktraceWarning = false;
        [SerializeField] private bool logError = true;
        [SerializeField] private bool stacktraceError = true;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
#if !UNITY_EDITOR
            ClearLogs();
#endif
        }

#if !UNITY_EDITOR
        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }
        
        private string GetSettingsPath()
        {
            var folderPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
            return Path.Combine(folderPath, fileName);
        }

        private void ClearLogs()
        {
            var filePath = GetSettingsPath();
            try
            {
                File.WriteAllText(filePath, string.Empty);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Logger] Failed to clear log file: {e.Message}");
            }
        }

        private void HandleLog(string condition, string stacktrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    LogError(condition, stacktrace);
                    break;
                case LogType.Warning:
                    LogWarning(condition);
                    break; 
                default:
                    LogMessage(condition);
                    break;
            }
        }
        
        private void Log(string message, string stacktrace = null)
        {
            var filePath = GetSettingsPath();
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            try
            {
                File.AppendAllText(filePath, logEntry + Environment.NewLine);
                if (!string.IsNullOrEmpty(stacktrace))
                    File.AppendAllText(filePath, stacktrace + Environment.NewLine);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Logger] Failed to write log: {e.Message}");
            }
        }
        private void LogMessage(string message, string stacktrace = null)
        {
            if (!logMessage) return;
            
            if (stacktraceMessage)
                Log(message, stacktrace);
            else
                Log(message);
        }

        private void LogWarning(string message, string stacktrace = null)
        {
            if (!logWarning) return;

            var warningMessage = "[WARNING] " + message;
            if (stacktraceWarning)
                Log(warningMessage, stacktrace);
            else
                Log(warningMessage);
        }

        private void LogError(string message, string stacktrace = null)
        {
            if (!logError) return;

            var errorMessage = "[ERROR] " + message;
            if (stacktraceError)
                Log(errorMessage, stacktrace);
            else
                Log(errorMessage);
        }
#endif
    }
}