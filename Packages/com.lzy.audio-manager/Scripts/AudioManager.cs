using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LZY.SimpleAudioManager
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        
        private static AudioManager Instance
        {
            get 
            {
                if (_instance == null)
                {
                    var audioManager = FindAnyObjectByType<AudioManager>();
                    audioManager.TryInitialize();
                }

                return _instance;
            }
        }

        [SerializeField] private AudioBankConfig audioBankConfig;
        [SerializeField] private AudioSource BGMAudioSource;
        [SerializeField] private int SFXAudioSourceSize = 10;

        private Stack<AudioSourceData> _sfxAudioSources = new Stack<AudioSourceData>();
        private Tween _bgmTween;
        private Coroutine _bgmCoroutine;
        private float _cachedBGMVolume;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            TryInitialize();
        }

        private void TryInitialize()
        {
            if (_instance == this) return;
            
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            PopulateSFXAudioSources();
        }

        #endregion

        #region SFX
        
        private void PopulateSFXAudioSources()
        {
            var remainingSize = Mathf.Max(0, SFXAudioSourceSize - _sfxAudioSources.Count);
            for (int i = 0; i < remainingSize; i++)
                SpawnAudioSource();
        }

        private void SpawnAudioSource()
        {
            var audioSource = new GameObject("SFXAudioSource").AddComponent<AudioSource>();
            audioSource.transform.SetParent(transform);
            _sfxAudioSources.Push(new AudioSourceData() { source = audioSource });
        }

        public static void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            Instance?.InternalPlaySFX(clip, volume, pitch);
        }

        public static void PlaySFX(AudioData data)
        {
            Instance?.InternalPlaySFX(data);
        }

        public static void PlaySFX(string id)
        {
            Instance?.InternalPlaySFX(id);
        }

        private void InternalPlaySFX(string id)
        {
            if (audioBankConfig.AudioMap.ContainsKey(id))
            {
                InternalPlaySFX(audioBankConfig.AudioMap[id]);
            }
        }
        
        private void InternalPlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            StartCoroutine(InternalPlaySFXCoroutine(clip, volume, pitch));
        }

        private void InternalPlaySFX(AudioData data)
        {
            StartCoroutine(InternalPlaySFXCoroutine(data));
        }

        private IEnumerator InternalPlaySFXCoroutine(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) yield break;
            
            var audioSource = GetAudioSourceData();
            if (audioSource == null) yield break;
            
            yield return InternalPlayAudioSourceSFXCoroutine(audioSource, clip, volume, pitch);
        }

        private IEnumerator InternalPlaySFXCoroutine(AudioData data)
        {
            var clip = data.GetRandomizedClip();
            if (clip == null) yield break;
            
            var audioSource = GetAudioSourceData();
            if (audioSource == null) yield break;
            
            if (data.delay > 0)
                yield return new WaitForSeconds(data.delay);
            
            yield return InternalPlayAudioSourceSFXCoroutine(audioSource, clip, data.GetProcessedVolume(), data.GetProcessedPitch());
        }

        private IEnumerator InternalPlayAudioSourceSFXCoroutine(AudioSourceData audioSource, AudioClip clip, float volume, float pitch)
        {
            if (audioSource.fadeTween != null && audioSource.fadeTween.active)
                audioSource.fadeTween.Kill();
            
            audioSource.source.clip = clip;
            audioSource.source.pitch = pitch;
            audioSource.source.volume = volume;
            audioSource.source.Play();

            yield return new WaitForSeconds(clip.length);

            _sfxAudioSources.Push(audioSource);
        }

        private AudioSourceData GetAudioSourceData()
        {
            var isAvailable = _sfxAudioSources.Count > 0;
            if (!isAvailable)
                SpawnAudioSource();

            return _sfxAudioSources.Pop();
        }
        
        #endregion

        #region BGM
        
        public static void PlayBGM(AudioClip clip, float fadeDuration = 1f, float delay = 0f, float volume = 1f, float pitch = 1f)
        {
            Instance?.InternalPlayBGM(clip, fadeDuration, delay, volume, pitch);
        }

        public static void PlayBGM(AudioData data, float fadeDuration = 1f)
        {
            Instance?.InternalPlayBGM(data, fadeDuration);
        }

        public static void PlayBGM(string id, float fadeDuration = 1f)
        {
            Instance?.InternalPlayBGM(id, fadeDuration);
        }
        
        private void InternalPlayBGM(string id, float fadeDuration = 1f)
        {
            if (audioBankConfig.AudioMap.ContainsKey(id))
            {
                InternalPlayBGM(audioBankConfig.AudioMap[id], fadeDuration);
            }
        }
        
        private void InternalPlayBGM(AudioClip clip, float fadeDuration = 1f, float delay = 0f, float volume = 1f, float pitch = 1f)
        {
            if (_bgmCoroutine != null)
                StopCoroutine(_bgmCoroutine);
            
            _bgmCoroutine = StartCoroutine(InternalPlayBGMCoroutine(clip, fadeDuration, delay, volume, pitch));
        }

        private void InternalPlayBGM(AudioData data, float fadeDuration = 1f)
        {
            var clip = data.GetRandomizedClip();
            if (clip == null) return;
            
            InternalPlayBGM(clip, fadeDuration, data.delay, data.GetProcessedVolume(), data.GetProcessedPitch());
        }

        private IEnumerator InternalPlayBGMCoroutine(AudioClip clip, float fadeDuration = 1f, float delay = 0f, float volume = 1f, float pitch = 1f)
        {
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            
            if (_bgmTween != null && _bgmTween.active)
                _bgmTween.Kill();
            
            var halfDuration = fadeDuration / 2f;
            if (BGMAudioSource.isPlaying)
            {
                _bgmTween = DOTween.To(() => BGMAudioSource.volume, x => BGMAudioSource.volume = x, 0, halfDuration);
                yield return _bgmTween.WaitForCompletion();
            }
            
            if (BGMAudioSource.isPlaying)
                BGMAudioSource.Stop();

            BGMAudioSource.clip = clip;
            BGMAudioSource.pitch = pitch;
            BGMAudioSource.volume = 0;
            BGMAudioSource.loop = true;
            BGMAudioSource.Play();
            _cachedBGMVolume = volume;

            _bgmTween = DOTween.To(() => BGMAudioSource.volume, x => BGMAudioSource.volume = x, volume, halfDuration);
            yield return _bgmTween.WaitForCompletion();

            _bgmCoroutine = null;
        }
        
        public static void PauseBGM(float fadeDuration = 1f, float delay = 0f)
        {
            Instance?.InternalPauseBGM(fadeDuration, delay);
        }

        private void InternalPauseBGM(float fadeDuration = 1f, float delay = 0f)
        {
            if (!BGMAudioSource.isPlaying) return;
            
            if (_bgmCoroutine != null)
                StopCoroutine(_bgmCoroutine);
            
            _bgmCoroutine = StartCoroutine(InternalPauseBGMCoroutine(fadeDuration, delay));
        }
        
        private IEnumerator InternalPauseBGMCoroutine(float fadeDuration = 1f, float delay = 0f)
        {
            yield return InternalSetBGMVolumeCoroutine(0f, fadeDuration, delay);
            BGMAudioSource.Pause();
        }
        
        public static void ResumeBGM(float fadeDuration = 1f, float delay = 0f)
        {
            Instance?.InternalSetBGMVolume(0, fadeDuration, delay);
        }
        
        private void InternalResumeBGM(float fadeDuration = 1f, float delay = 0f)
        {
            if (BGMAudioSource.isPlaying) return;
            
            if (_bgmCoroutine != null)
                StopCoroutine(_bgmCoroutine);
            
            _bgmCoroutine = StartCoroutine(InternalResumeBGMCoroutine(fadeDuration, delay));
        }
        
        private IEnumerator InternalResumeBGMCoroutine(float fadeDuration = 1f, float delay = 0f)
        {
            BGMAudioSource.Play();
            yield return InternalSetBGMVolumeCoroutine(_cachedBGMVolume, fadeDuration, delay);
        }
        
        public static void SetBGMVolume(float volume, float fadeDuration = 1f, float delay = 0f)
        {
            Instance?.InternalSetBGMVolume(volume, fadeDuration, delay);
        }
        
        private void InternalSetBGMVolume(float volume, float fadeDuration = 1f, float delay = 0f)
        {
            if (_bgmCoroutine != null)
                StopCoroutine(_bgmCoroutine);
            
            _bgmCoroutine = StartCoroutine(InternalSetBGMVolumeCoroutine(volume, fadeDuration, delay));
        }
        
        private IEnumerator InternalSetBGMVolumeCoroutine(float volume, float fadeDuration = 1f, float delay = 0f)
        {
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            
            if (_bgmTween != null && _bgmTween.active)
                _bgmTween.Kill();
            
            if (BGMAudioSource.isPlaying)
            {
                _bgmTween = DOTween.To(() => BGMAudioSource.volume, x => BGMAudioSource.volume = x, volume, fadeDuration);
                yield return _bgmTween.WaitForCompletion();
            }

            _bgmCoroutine = null;
        }
        
        #endregion
    }

    [Serializable]
    public class AudioSourceData
    {
        public AudioSource source;
        public Tween fadeTween;
    }

    [Serializable]
    public class AudioData
    {
        public string id;
        public AudioClip[] clips;
        public float delay;
        
        [Header("Volume")]
        public float volume = 1f;
        public bool isRandomVolume;
        public float randomMinVolume = 1f; 
        public float randomMaxVolume = 1f;
        
        [Header("Pitch")]
        public float pitch = 1f;
        public bool isRandomPitch;
        public float randomMinPitch = 1f;
        public float randomMaxPitch = 1f;

        public AudioClip GetRandomizedClip()
        {
            return clips.Length > 0 ? clips[Random.Range(0, clips.Length)] : null;
        }
        
        public float GetProcessedVolume()
        {
            return isRandomVolume ? Random.Range(randomMinVolume, randomMaxVolume) : volume;
        }
        
        public float GetProcessedPitch()
        {
            return isRandomPitch ? Random.Range(randomMinPitch, randomMaxPitch) : pitch;
        }
    }   
}