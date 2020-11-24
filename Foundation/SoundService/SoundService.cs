using System;
using System.Collections.Generic;
using Foundation.AssetBundles;
using Foundation.ServicesResolver;
using Foundation.TimerUtils;
using Foundation.Utils.OperationUtils;
using UnityEngine;

namespace Foundation.Sound
{
    public class SoundService : BaseService
    {
        private const string _soundSettingsSuffix = "_soundSettings";
        private SoundConfig _config;
        private List<AudioSource> _configuredAudioSources;
        private AudioSource _defaultAudioSource;
        private GameObject _audioSourceHolder;
        private SoundSettings _defaultSettings;

        private AudioSource AvailableAudioSource
        {
            get
            {
                if (_configuredAudioSources.Count > 0)
                {
                    AudioSource source = _configuredAudioSources[0];
                    _configuredAudioSources.Remove(source);
                    return source;
                }

                return _audioSourceHolder.AddComponent<AudioSource>();
            }
        }

        protected override void Initialize()
        {
            _config = GetConfig<SoundConfig>();
            _configuredAudioSources = new List<AudioSource>();
            CreateAudioSourceHolder();
        }
        
        public override void Dispose()
        {
            _configuredAudioSources = null;
            _defaultAudioSource = null;
            _defaultSettings = null;

            if (_audioSourceHolder != null)
            {
                GameObject.Destroy(_audioSourceHolder.gameObject);
            }
        }
        
        private void CreateAudioSourceHolder()
        {
            _audioSourceHolder = new GameObject("SoundService");
            _defaultAudioSource = _audioSourceHolder.AddComponent<AudioSource>();
            for (int i = 0; i < _config._initialSourcePoolSize; i++)
            {
                _configuredAudioSources.Add(_audioSourceHolder.AddComponent<AudioSource>());
            }

            GameObject.DontDestroyOnLoad(_audioSourceHolder);

            _defaultSettings = ScriptableObject.CreateInstance<SoundSettings>();
            _defaultSettings.Volume = _defaultAudioSource.volume;
            _defaultSettings.Pitch = _defaultAudioSource.pitch;
            _defaultSettings.Priority = _defaultAudioSource.priority;
        }

        public void LoadAndPlaySound(string soundName)
        {
            OperationsQueue
            .Do<string, LoadedSound>(soundName, GetSoundClip)
            .Then<LoadedSound, LoadedSound>(GetSoundSettings)
            .Then<LoadedSound>(PlaySound)
            .Run("Load & Play Sound");
        }

        private void GetSoundClip(string soundName, Action<Result<LoadedSound>> operationComplete)
        {
            AssetBundlesService.LoadAsset<AudioClip>(soundName, result =>
            {
                Result<LoadedSound> loadedSound = new Result<LoadedSound>();
                loadedSound.Data = new LoadedSound();
                loadedSound.Data.clip = result.Data;
                operationComplete.Invoke(loadedSound); 
            });
        }
        
        private void GetSoundSettings(LoadedSound data, Action<Result<LoadedSound>> operationComplete)
        {
            string soundSettingsName = data.clip.name + _soundSettingsSuffix;
            Result<LoadedSound> loadedSound = new Result<LoadedSound> {Data = data};
            
            if (AssetBundlesService.AssetExists(soundSettingsName))
            {
                AssetBundlesService.LoadAsset<SoundSettings>(soundSettingsName, result =>
                {
                    loadedSound.Data.settings = result.Data;
                    operationComplete.Invoke(loadedSound); 
                });
            }
            else
            {
                operationComplete.Invoke(loadedSound);
            }
        }

        private void PlaySound(LoadedSound data, Action<Result> operationComplete)
        {
            PlaySoundClip(data.clip, data.settings);
            operationComplete.Invoke(Result.Successful);
        }

        public void PlaySoundClip(AudioClip clip, SoundSettings settings = null)
        {
            if (settings == null)
            {
                _defaultAudioSource.PlayOneShot(clip);
            }
            else
            {
                AudioSource source = AvailableAudioSource;
                ApplySettings(source, settings);
                source.PlayOneShot(clip);
                TimerService.StartTimer(clip.length, () =>
                {
                    source.clip = null;
                    ApplySettings(source, _defaultSettings);
                    _configuredAudioSources.Add(source);
                }, true);
            }
        }

        private void ApplySettings(AudioSource source, SoundSettings settings)
        {
            source.volume = settings.Volume;
            source.pitch = settings.Pitch;
            source.priority = settings.Priority;
        }

        private struct LoadedSound
        {
            public AudioClip clip;
            public SoundSettings settings;
        }
    }
}
