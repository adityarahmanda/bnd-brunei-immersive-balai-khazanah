using System.Collections.Generic;
using UnityEngine;

namespace LZY.SimpleAudioManager
{
    [CreateAssetMenu(fileName = "AudioBankConfig", menuName = "LZY/Simple Audio Manager/Audio Bank Config")]
    public class AudioBankConfig : ScriptableObject
    {
        public Dictionary<string, AudioData> AudioMap
        {
            get
            {
                if (_audioMap == null)
                    RefreshDictionary();
                return _audioMap;
            }
        }

        [SerializeField] private List<AudioData> audioDataList;

        private Dictionary<string, AudioData> _audioMap;

        private void RefreshDictionary()
        {
            _audioMap = new Dictionary<string, AudioData>();
            foreach (AudioData data in audioDataList)
                _audioMap.Add(data.id, data);
        }
    }
}
