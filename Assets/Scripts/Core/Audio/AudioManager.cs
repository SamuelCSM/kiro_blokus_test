using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using BlokusGame.Core.Data;
using BlokusGame.Core.Events;
using BlokusGame.Core.Interfaces;

namespace BlokusGame.Core.Audio
{
    /// <summary>
    /// 音效管理器 - 统一管理游戏中的所有音效和背景音乐
    /// 提供音效播放、音量控制、音效池管理等功能
    /// 支持2D和3D音效，以及音效的淡入淡出效果
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("音效配置")]
        /// <summary>是否启用音效</summary>
        [SerializeField] private bool _m_enableSoundEffects = true;
        
        /// <summary>是否启用背景音乐</summary>
        [SerializeField] private bool _m_enableBackgroundMusic = true;
        
        /// <summary>主音量</summary>
        [SerializeField] [Range(0f, 1f)] private float _m_masterVolume = 1f;
        
        /// <summary>音效音量</summary>
        [SerializeField] [Range(0f, 1f)] private float _m_soundEffectVolume = 0.8f;
        
        /// <summary>背景音乐音量</summary>
        [SerializeField] [Range(0f, 1f)] private float _m_backgroundMusicVolume = 0.6f;
        
        /// <summary>UI音效音量</summary>
        [SerializeField] [Range(0f, 1f)] private float _m_uiSoundVolume = 0.7f;
        
        [Header("音效资源")]
        /// <summary>方块选择音效</summary>
        [SerializeField] private AudioClip _m_pieceSelectSound;
        
        /// <summary>方块放置音效</summary>
        [SerializeField] private AudioClip _m_piecePlaceSound;
        
        /// <summary>方块旋转音效</summary>
        [SerializeField] private AudioClip _m_pieceRotateSound;
        
        /// <summary>方块翻转音效</summary>
        [SerializeField] private AudioClip _m_pieceFlipSound;
        
        /// <summary>无效操作音效</summary>
        [SerializeField] private AudioClip _m_invalidActionSound;
        
        /// <summary>回合开始音效</summary>
        [SerializeField] private AudioClip _m_turnStartSound;
        
        /// <summary>游戏胜利音效</summary>
        [SerializeField] private AudioClip _m_gameWinSound;
        
        /// <summary>游戏失败音效</summary>
        [SerializeField] private AudioClip _m_gameLoseSound;
        
        [Header("UI音效")]
        /// <summary>按钮点击音效</summary>
        [SerializeField] private AudioClip _m_buttonClickSound;
        
        /// <summary>菜单打开音效</summary>
        [SerializeField] private AudioClip _m_menuOpenSound;
        
        /// <summary>菜单关闭音效</summary>
        [SerializeField] private AudioClip _m_menuCloseSound;
        
        /// <summary>错误提示音效</summary>
        [SerializeField] private AudioClip _m_errorSound;
        
        /// <summary>成功提示音效</summary>
        [SerializeField] private AudioClip _m_successSound;
        
        [Header("背景音乐")]
        /// <summary>主菜单背景音乐</summary>
        [SerializeField] private AudioClip _m_mainMenuMusic;
        
        /// <summary>游戏内背景音乐</summary>
        [SerializeField] private AudioClip _m_gameplayMusic;
        
        /// <summary>胜利背景音乐</summary>
        [SerializeField] private AudioClip _m_victoryMusic;
        
        [Header("音效池配置")]
        /// <summary>音效源池大小</summary>
        [SerializeField] private int _m_audioSourcePoolSize = 10;
        
        /// <summary>音效淡入淡出时间</summary>
        [SerializeField] private float _m_fadeTime = 1f;
        
        // 私有字段
        /// <summary>背景音乐音频源</summary>
        private AudioSource _m_backgroundMusicSource;
        
        /// <summary>音效源对象池</summary>
        private Queue<AudioSource> _m_audioSourcePool = new Queue<AudioSource>();
        
        /// <summary>活跃的音效源列表</summary>
        private List<AudioSource> _m_activeAudioSources = new List<AudioSource>();
        
        /// <summary>当前播放的背景音乐</summary>
        private AudioClip _m_currentBackgroundMusic;
        
        /// <summary>音效淡出协程字典</summary>
        private Dictionary<AudioSource, Coroutine> _m_fadeCoroutines = new Dictionary<AudioSource, Coroutine>();
        
        /// <summary>单例实例</summary>
        public static AudioManager instance { get; private set; }
        
        /// <summary>
        /// 音效类型枚举
        /// </summary>
        public enum SoundType
        {
            /// <summary>游戏音效</summary>
            GameSound,
            /// <summary>UI音效</summary>
            UISound,
            /// <summary>背景音乐</summary>
            BackgroundMusic
        }
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法 - 初始化单例和音效系统
        /// </summary>
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                _initializeAudioManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Unity Start方法 - 订阅事件和开始背景音乐
        /// </summary>
        private void Start()
        {
            _subscribeToEvents();
            _startBackgroundMusic();
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 清理资源
        /// </summary>
        private void OnDestroy()
        {
            _unsubscribeFromEvents();
            _cleanup();
            
            if (instance == this)
            {
                instance = null;
            }
        }
        
        #endregion
        
        #region 公共方法 - 音效播放
        
        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="_audioClip">音效片段</param>
        /// <param name="_volume">音量（0-1）</param>
        /// <param name="_pitch">音调（0.1-3）</param>
        /// <param name="_soundType">音效类型</param>
        /// <returns>播放的音频源</returns>
        public AudioSource playSound(AudioClip _audioClip, float _volume = 1f, float _pitch = 1f, SoundType _soundType = SoundType.GameSound)
        {
            if (_audioClip == null) return null;
            
            // 检查音效是否启用
            if (!_isSoundTypeEnabled(_soundType)) return null;
            
            AudioSource audioSource = _getAudioSourceFromPool();
            if (audioSource == null) return null;
            
            // 配置音频源
            audioSource.clip = _audioClip;
            audioSource.volume = _calculateFinalVolume(_volume, _soundType);
            audioSource.pitch = _pitch;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f; // 2D音效
            
            // 播放音效
            audioSource.Play();
            
            // 启动自动回收协程
            StartCoroutine(_autoReturnAudioSource(audioSource, _audioClip.length / _pitch));
            
            return audioSource;
        }
        
        /// <summary>
        /// 播放3D音效
        /// </summary>
        /// <param name="_audioClip">音效片段</param>
        /// <param name="_position">3D位置</param>
        /// <param name="_volume">音量</param>
        /// <param name="_pitch">音调</param>
        /// <param name="_minDistance">最小距离</param>
        /// <param name="_maxDistance">最大距离</param>
        /// <returns>播放的音频源</returns>
        public AudioSource playSound3D(AudioClip _audioClip, Vector3 _position, float _volume = 1f, float _pitch = 1f, float _minDistance = 1f, float _maxDistance = 500f)
        {
            if (_audioClip == null || !_m_enableSoundEffects) return null;
            
            AudioSource audioSource = _getAudioSourceFromPool();
            if (audioSource == null) return null;
            
            // 配置3D音频源
            audioSource.clip = _audioClip;
            audioSource.volume = _calculateFinalVolume(_volume, SoundType.GameSound);
            audioSource.pitch = _pitch;
            audioSource.loop = false;
            audioSource.spatialBlend = 1f; // 3D音效
            audioSource.minDistance = _minDistance;
            audioSource.maxDistance = _maxDistance;
            audioSource.transform.position = _position;
            
            // 播放音效
            audioSource.Play();
            
            // 启动自动回收协程
            StartCoroutine(_autoReturnAudioSource(audioSource, _audioClip.length / _pitch));
            
            return audioSource;
        }
        
        /// <summary>
        /// 播放循环音效
        /// </summary>
        /// <param name="_audioClip">音效片段</param>
        /// <param name="_volume">音量</param>
        /// <param name="_soundType">音效类型</param>
        /// <returns>播放的音频源</returns>
        public AudioSource playLoopingSound(AudioClip _audioClip, float _volume = 1f, SoundType _soundType = SoundType.GameSound)
        {
            if (_audioClip == null || !_isSoundTypeEnabled(_soundType)) return null;
            
            AudioSource audioSource = _getAudioSourceFromPool();
            if (audioSource == null) return null;
            
            // 配置循环音频源
            audioSource.clip = _audioClip;
            audioSource.volume = _calculateFinalVolume(_volume, _soundType);
            audioSource.pitch = 1f;
            audioSource.loop = true;
            audioSource.spatialBlend = 0f;
            
            // 播放循环音效
            audioSource.Play();
            
            return audioSource;
        }
        
        /// <summary>
        /// 停止音效
        /// </summary>
        /// <param name="_audioSource">要停止的音频源</param>
        /// <param name="_fadeOut">是否淡出</param>
        public void stopSound(AudioSource _audioSource, bool _fadeOut = false)
        {
            if (_audioSource == null) return;
            
            if (_fadeOut)
            {
                StartCoroutine(_fadeOutAndStop(_audioSource));
            }
            else
            {
                _audioSource.Stop();
                _returnAudioSourceToPool(_audioSource);
            }
        }
        
        /// <summary>
        /// 停止所有音效
        /// </summary>
        /// <param name="_fadeOut">是否淡出</param>
        public void stopAllSounds(bool _fadeOut = false)
        {
            var activeSources = new List<AudioSource>(_m_activeAudioSources);
            
            foreach (var audioSource in activeSources)
            {
                stopSound(audioSource, _fadeOut);
            }
        }
        
        #endregion
        
        #region 公共方法 - 背景音乐
        
        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="_musicClip">音乐片段</param>
        /// <param name="_fadeIn">是否淡入</param>
        /// <param name="_loop">是否循环</param>
        public void playBackgroundMusic(AudioClip _musicClip, bool _fadeIn = true, bool _loop = true)
        {
            if (_musicClip == null || !_m_enableBackgroundMusic) return;
            
            // 如果正在播放相同的音乐，不做任何操作
            if (_m_currentBackgroundMusic == _musicClip && _m_backgroundMusicSource.isPlaying) return;
            
            // 停止当前背景音乐
            if (_m_backgroundMusicSource.isPlaying)
            {
                if (_fadeIn)
                {
                    StartCoroutine(_crossFadeBackgroundMusic(_musicClip, _loop));
                    return;
                }
                else
                {
                    _m_backgroundMusicSource.Stop();
                }
            }
            
            // 播放新的背景音乐
            _m_currentBackgroundMusic = _musicClip;
            _m_backgroundMusicSource.clip = _musicClip;
            _m_backgroundMusicSource.loop = _loop;
            _m_backgroundMusicSource.volume = _fadeIn ? 0f : _calculateFinalVolume(1f, SoundType.BackgroundMusic);
            _m_backgroundMusicSource.Play();
            
            // 淡入效果
            if (_fadeIn)
            {
                StartCoroutine(_fadeInBackgroundMusic());
            }
        }
        
        /// <summary>
        /// 停止背景音乐
        /// </summary>
        /// <param name="_fadeOut">是否淡出</param>
        public void stopBackgroundMusic(bool _fadeOut = true)
        {
            if (!_m_backgroundMusicSource.isPlaying) return;
            
            if (_fadeOut)
            {
                StartCoroutine(_fadeOutBackgroundMusic());
            }
            else
            {
                _m_backgroundMusicSource.Stop();
                _m_currentBackgroundMusic = null;
            }
        }
        
        /// <summary>
        /// 暂停背景音乐
        /// </summary>
        public void pauseBackgroundMusic()
        {
            if (_m_backgroundMusicSource.isPlaying)
            {
                _m_backgroundMusicSource.Pause();
            }
        }
        
        /// <summary>
        /// 恢复背景音乐
        /// </summary>
        public void resumeBackgroundMusic()
        {
            if (!_m_backgroundMusicSource.isPlaying && _m_backgroundMusicSource.clip != null)
            {
                _m_backgroundMusicSource.UnPause();
            }
        }
        
        #endregion 
       
        #region 公共方法 - 音量控制
        
        /// <summary>
        /// 设置主音量
        /// </summary>
        /// <param name="_volume">音量（0-1）</param>
        public void setMasterVolume(float _volume)
        {
            _m_masterVolume = Mathf.Clamp01(_volume);
            _updateAllVolumes();
        }
        
        /// <summary>
        /// 设置音效音量
        /// </summary>
        /// <param name="_volume">音量（0-1）</param>
        public void setSoundEffectVolume(float _volume)
        {
            _m_soundEffectVolume = Mathf.Clamp01(_volume);
            _updateAllVolumes();
        }
        
        /// <summary>
        /// 设置背景音乐音量
        /// </summary>
        /// <param name="_volume">音量（0-1）</param>
        public void setBackgroundMusicVolume(float _volume)
        {
            _m_backgroundMusicVolume = Mathf.Clamp01(_volume);
            _m_backgroundMusicSource.volume = _calculateFinalVolume(1f, SoundType.BackgroundMusic);
        }
        
        /// <summary>
        /// 设置UI音效音量
        /// </summary>
        /// <param name="_volume">音量（0-1）</param>
        public void setUISoundVolume(float _volume)
        {
            _m_uiSoundVolume = Mathf.Clamp01(_volume);
        }
        
        /// <summary>
        /// 设置音效是否启用
        /// </summary>
        /// <param name="_enabled">是否启用</param>
        public void setSoundEffectsEnabled(bool _enabled)
        {
            _m_enableSoundEffects = _enabled;
            
            if (!_enabled)
            {
                stopAllSounds(true);
            }
        }
        
        /// <summary>
        /// 设置背景音乐是否启用
        /// </summary>
        /// <param name="_enabled">是否启用</param>
        public void setBackgroundMusicEnabled(bool _enabled)
        {
            _m_enableBackgroundMusic = _enabled;
            
            if (!_enabled)
            {
                stopBackgroundMusic(true);
            }
            else if (_m_currentBackgroundMusic != null)
            {
                playBackgroundMusic(_m_currentBackgroundMusic);
            }
        }
        
        #endregion
        
        #region 公共方法 - 预设音效
        
        /// <summary>
        /// 播放方块选择音效
        /// </summary>
        public void playPieceSelectSound()
        {
            playSound(_m_pieceSelectSound, 1f, 1f, SoundType.GameSound);
        }
        
        /// <summary>
        /// 播放方块放置音效
        /// </summary>
        public void playPiecePlaceSound()
        {
            playSound(_m_piecePlaceSound, 1f, 1f, SoundType.GameSound);
        }
        
        /// <summary>
        /// 播放方块旋转音效
        /// </summary>
        public void playPieceRotateSound()
        {
            playSound(_m_pieceRotateSound, 0.8f, 1f, SoundType.GameSound);
        }
        
        /// <summary>
        /// 播放方块翻转音效
        /// </summary>
        public void playPieceFlipSound()
        {
            playSound(_m_pieceFlipSound, 0.8f, 1f, SoundType.GameSound);
        }
        
        /// <summary>
        /// 播放无效操作音效
        /// </summary>
        public void playInvalidActionSound()
        {
            playSound(_m_invalidActionSound, 0.7f, 1f, SoundType.GameSound);
        }
        
        /// <summary>
        /// 播放回合开始音效
        /// </summary>
        public void playTurnStartSound()
        {
            playSound(_m_turnStartSound, 0.9f, 1f, SoundType.GameSound);
        }
        
        /// <summary>
        /// 播放游戏胜利音效
        /// </summary>
        public void playGameWinSound()
        {
            playSound(_m_gameWinSound, 1f, 1f, SoundType.GameSound);
        }
        
        /// <summary>
        /// 播放游戏失败音效
        /// </summary>
        public void playGameLoseSound()
        {
            playSound(_m_gameLoseSound, 1f, 1f, SoundType.GameSound);
        }
        
        /// <summary>
        /// 播放按钮点击音效
        /// </summary>
        public void playButtonClickSound()
        {
            playSound(_m_buttonClickSound, 1f, 1f, SoundType.UISound);
        }
        
        /// <summary>
        /// 播放菜单打开音效
        /// </summary>
        public void playMenuOpenSound()
        {
            playSound(_m_menuOpenSound, 0.8f, 1f, SoundType.UISound);
        }
        
        /// <summary>
        /// 播放菜单关闭音效
        /// </summary>
        public void playMenuCloseSound()
        {
            playSound(_m_menuCloseSound, 0.8f, 1f, SoundType.UISound);
        }
        
        /// <summary>
        /// 播放错误提示音效
        /// </summary>
        public void playErrorSound()
        {
            playSound(_m_errorSound, 0.9f, 1f, SoundType.UISound);
        }
        
        /// <summary>
        /// 播放成功提示音效
        /// </summary>
        public void playSuccessSound()
        {
            playSound(_m_successSound, 0.9f, 1f, SoundType.UISound);
        }
        
        #endregion
        
        #region 私有方法 - 初始化和清理
        
        /// <summary>
        /// 初始化音效管理器
        /// </summary>
        private void _initializeAudioManager()
        {
            // 创建背景音乐音频源
            _m_backgroundMusicSource = gameObject.AddComponent<AudioSource>();
            _m_backgroundMusicSource.playOnAwake = false;
            _m_backgroundMusicSource.loop = true;
            _m_backgroundMusicSource.spatialBlend = 0f;
            
            // 初始化音效源对象池
            _initializeAudioSourcePool();
            
            Debug.Log("[AudioManager] 音效管理器初始化完成");
        }
        
        /// <summary>
        /// 初始化音频源对象池
        /// </summary>
        private void _initializeAudioSourcePool()
        {
            for (int i = 0; i < _m_audioSourcePoolSize; i++)
            {
                GameObject audioSourceObj = new GameObject($"AudioSource_{i}");
                audioSourceObj.transform.SetParent(transform);
                
                AudioSource audioSource = audioSourceObj.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
                
                audioSourceObj.SetActive(false);
                _m_audioSourcePool.Enqueue(audioSource);
            }
        }
        
        /// <summary>
        /// 订阅游戏事件
        /// </summary>
        private void _subscribeToEvents()
        {
            if (GameEvents.instance != null)
            {
                GameEvents.instance.onPieceSelected += _onPieceSelected;
                GameEvents.onPiecePlaced += _onPiecePlaced;
                GameEvents.instance.onPieceRotated += _onPieceRotated;
                GameEvents.instance.onPieceFlipped += _onPieceFlipped;
                GameEvents.onTurnStarted += _onTurnStarted;
                GameEvents.onGameEnded += _onGameEnded;
                GameEvents.instance.onGameStateChanged += _onGameStateChanged;
                GameEvents.instance.onPiecePlacementFailed += _onPiecePlacementFailed;
            }
        }
        
        /// <summary>
        /// 取消订阅游戏事件
        /// </summary>
        private void _unsubscribeFromEvents()
        {
            if (GameEvents.instance != null)
            {
                GameEvents.instance.onPieceSelected -= _onPieceSelected;
                GameEvents.onPiecePlaced -= _onPiecePlaced;
                GameEvents.instance.onPieceRotated -= _onPieceRotated;
                GameEvents.instance.onPieceFlipped -= _onPieceFlipped;
                GameEvents.onTurnStarted -= _onTurnStarted;
                GameEvents.onGameEnded -= _onGameEnded;
                GameEvents.instance.onGameStateChanged -= _onGameStateChanged;
                GameEvents.instance.onPiecePlacementFailed -= _onPiecePlacementFailed;
            }
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        private void _cleanup()
        {
            StopAllCoroutines();
            stopAllSounds(false);
            stopBackgroundMusic(false);
        }
        
        #endregion
        
        #region 私有方法 - 对象池管理
        
        /// <summary>
        /// 从对象池获取音频源
        /// </summary>
        /// <returns>音频源</returns>
        private AudioSource _getAudioSourceFromPool()
        {
            AudioSource audioSource = null;
            
            if (_m_audioSourcePool.Count > 0)
            {
                audioSource = _m_audioSourcePool.Dequeue();
            }
            else
            {
                // 如果池为空，创建新的音频源
                GameObject audioSourceObj = new GameObject("AudioSource_Dynamic");
                audioSourceObj.transform.SetParent(transform);
                audioSource = audioSourceObj.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
            
            if (audioSource != null)
            {
                audioSource.gameObject.SetActive(true);
                _m_activeAudioSources.Add(audioSource);
            }
            
            return audioSource;
        }
        
        /// <summary>
        /// 将音频源返回对象池
        /// </summary>
        /// <param name="_audioSource">音频源</param>
        private void _returnAudioSourceToPool(AudioSource _audioSource)
        {
            if (_audioSource == null) return;
            
            // 停止淡出协程
            if (_m_fadeCoroutines.ContainsKey(_audioSource))
            {
                if (_m_fadeCoroutines[_audioSource] != null)
                {
                    StopCoroutine(_m_fadeCoroutines[_audioSource]);
                }
                _m_fadeCoroutines.Remove(_audioSource);
            }
            
            // 重置音频源
            _audioSource.Stop();
            _audioSource.clip = null;
            _audioSource.volume = 1f;
            _audioSource.pitch = 1f;
            _audioSource.loop = false;
            _audioSource.spatialBlend = 0f;
            _audioSource.gameObject.SetActive(false);
            
            // 从活跃列表移除
            _m_activeAudioSources.Remove(_audioSource);
            
            // 返回对象池
            _m_audioSourcePool.Enqueue(_audioSource);
        }
        
        #endregion        

        #region 私有方法 - 音量和淡入淡出
        
        /// <summary>
        /// 计算最终音量
        /// </summary>
        /// <param name="_volume">基础音量</param>
        /// <param name="_soundType">音效类型</param>
        /// <returns>最终音量</returns>
        private float _calculateFinalVolume(float _volume, SoundType _soundType)
        {
            float typeVolume = 1f;
            
            switch (_soundType)
            {
                case SoundType.GameSound:
                    typeVolume = _m_soundEffectVolume;
                    break;
                case SoundType.UISound:
                    typeVolume = _m_uiSoundVolume;
                    break;
                case SoundType.BackgroundMusic:
                    typeVolume = _m_backgroundMusicVolume;
                    break;
            }
            
            return _volume * typeVolume * _m_masterVolume;
        }
        
        /// <summary>
        /// 检查音效类型是否启用
        /// </summary>
        /// <param name="_soundType">音效类型</param>
        /// <returns>是否启用</returns>
        private bool _isSoundTypeEnabled(SoundType _soundType)
        {
            switch (_soundType)
            {
                case SoundType.GameSound:
                case SoundType.UISound:
                    return _m_enableSoundEffects;
                case SoundType.BackgroundMusic:
                    return _m_enableBackgroundMusic;
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// 更新所有音量
        /// </summary>
        private void _updateAllVolumes()
        {
            // 更新背景音乐音量
            if (_m_backgroundMusicSource != null)
            {
                _m_backgroundMusicSource.volume = _calculateFinalVolume(1f, SoundType.BackgroundMusic);
            }
            
            // 更新活跃音效的音量
            foreach (var audioSource in _m_activeAudioSources)
            {
                if (audioSource != null && audioSource.isPlaying)
                {
                    // 这里需要记录原始音量来正确更新
                    // 暂时使用当前音量作为基础
                    audioSource.volume = _calculateFinalVolume(audioSource.volume, SoundType.GameSound);
                }
            }
        }
        
        /// <summary>
        /// 背景音乐淡入协程
        /// </summary>
        /// <returns>协程</returns>
        private IEnumerator _fadeInBackgroundMusic()
        {
            float targetVolume = _calculateFinalVolume(1f, SoundType.BackgroundMusic);
            float currentTime = 0f;
            
            while (currentTime < _m_fadeTime)
            {
                currentTime += Time.deltaTime;
                float progress = currentTime / _m_fadeTime;
                _m_backgroundMusicSource.volume = Mathf.Lerp(0f, targetVolume, progress);
                yield return null;
            }
            
            _m_backgroundMusicSource.volume = targetVolume;
        }
        
        /// <summary>
        /// 背景音乐淡出协程
        /// </summary>
        /// <returns>协程</returns>
        private IEnumerator _fadeOutBackgroundMusic()
        {
            float startVolume = _m_backgroundMusicSource.volume;
            float currentTime = 0f;
            
            while (currentTime < _m_fadeTime)
            {
                currentTime += Time.deltaTime;
                float progress = currentTime / _m_fadeTime;
                _m_backgroundMusicSource.volume = Mathf.Lerp(startVolume, 0f, progress);
                yield return null;
            }
            
            _m_backgroundMusicSource.volume = 0f;
            _m_backgroundMusicSource.Stop();
            _m_currentBackgroundMusic = null;
        }
        
        /// <summary>
        /// 背景音乐交叉淡入淡出协程
        /// </summary>
        /// <param name="_newMusic">新的背景音乐</param>
        /// <param name="_loop">是否循环</param>
        /// <returns>协程</returns>
        private IEnumerator _crossFadeBackgroundMusic(AudioClip _newMusic, bool _loop)
        {
            float startVolume = _m_backgroundMusicSource.volume;
            float targetVolume = _calculateFinalVolume(1f, SoundType.BackgroundMusic);
            float halfFadeTime = _m_fadeTime * 0.5f;
            float currentTime = 0f;
            
            // 淡出当前音乐
            while (currentTime < halfFadeTime)
            {
                currentTime += Time.deltaTime;
                float progress = currentTime / halfFadeTime;
                _m_backgroundMusicSource.volume = Mathf.Lerp(startVolume, 0f, progress);
                yield return null;
            }
            
            // 切换音乐
            _m_backgroundMusicSource.Stop();
            _m_currentBackgroundMusic = _newMusic;
            _m_backgroundMusicSource.clip = _newMusic;
            _m_backgroundMusicSource.loop = _loop;
            _m_backgroundMusicSource.volume = 0f;
            _m_backgroundMusicSource.Play();
            
            // 淡入新音乐
            currentTime = 0f;
            while (currentTime < halfFadeTime)
            {
                currentTime += Time.deltaTime;
                float progress = currentTime / halfFadeTime;
                _m_backgroundMusicSource.volume = Mathf.Lerp(0f, targetVolume, progress);
                yield return null;
            }
            
            _m_backgroundMusicSource.volume = targetVolume;
        }
        
        /// <summary>
        /// 音效淡出并停止协程
        /// </summary>
        /// <param name="_audioSource">音频源</param>
        /// <returns>协程</returns>
        private IEnumerator _fadeOutAndStop(AudioSource _audioSource)
        {
            if (_audioSource == null) yield break;
            
            float startVolume = _audioSource.volume;
            float currentTime = 0f;
            float fadeTime = _m_fadeTime * 0.5f; // 音效淡出时间较短
            
            _m_fadeCoroutines[_audioSource] = StartCoroutine(_fadeOutAndStop(_audioSource));
            
            while (currentTime < fadeTime && _audioSource != null)
            {
                currentTime += Time.deltaTime;
                float progress = currentTime / fadeTime;
                _audioSource.volume = Mathf.Lerp(startVolume, 0f, progress);
                yield return null;
            }
            
            if (_audioSource != null)
            {
                _returnAudioSourceToPool(_audioSource);
            }
            
            _m_fadeCoroutines.Remove(_audioSource);
        }
        
        /// <summary>
        /// 自动回收音频源协程
        /// </summary>
        /// <param name="_audioSource">音频源</param>
        /// <param name="_duration">持续时间</param>
        /// <returns>协程</returns>
        private IEnumerator _autoReturnAudioSource(AudioSource _audioSource, float _duration)
        {
            yield return new WaitForSeconds(_duration);
            
            if (_audioSource != null && !_audioSource.isPlaying)
            {
                _returnAudioSourceToPool(_audioSource);
            }
        }
        
        #endregion
        
        #region 私有方法 - 背景音乐管理
        
        /// <summary>
        /// 开始背景音乐
        /// </summary>
        private void _startBackgroundMusic()
        {
            if (_m_mainMenuMusic != null)
            {
                playBackgroundMusic(_m_mainMenuMusic, true, true);
            }
        }
        
        #endregion
        
        #region 私有方法 - 事件处理
        
        /// <summary>
        /// 方块选择事件处理
        /// </summary>
        /// <param name="_piece">选中的方块</param>
        /// <param name="_playerId">玩家ID</param>
        private void _onPieceSelected(_IGamePiece _piece, int _playerId)
        {
            playPieceSelectSound();
        }
        
        /// <summary>
        /// 方块放置事件处理
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_piece">放置的方块</param>
        /// <param name="_position">放置位置</param>
        private void _onPiecePlaced(int _playerId, _IGamePiece _piece, Vector2Int _position)
        {
            playPiecePlaceSound();
        }
        
        /// <summary>
        /// 方块旋转事件处理
        /// </summary>
        /// <param name="_piece">旋转的方块</param>
        /// <param name="_playerId">玩家ID</param>
        private void _onPieceRotated(_IGamePiece _piece, int _playerId)
        {
            playPieceRotateSound();
        }
        
        /// <summary>
        /// 方块翻转事件处理
        /// </summary>
        /// <param name="_piece">翻转的方块</param>
        /// <param name="_playerId">玩家ID</param>
        private void _onPieceFlipped(_IGamePiece _piece, int _playerId)
        {
            playPieceFlipSound();
        }
        
        /// <summary>
        /// 回合开始事件处理
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_turnNumber">回合数</param>
        private void _onTurnStarted(int _playerId, int _turnNumber)
        {
            playTurnStartSound();
        }
        
        /// <summary>
        /// 游戏结束事件处理
        /// </summary>
        /// <param name="_finalScores">最终分数</param>
        private void _onGameEnded(Dictionary<int, int> _finalScores)
        {
            // 根据玩家是否获胜播放不同音效
            // 这里需要判断当前玩家是否获胜
            playGameWinSound(); // 暂时播放胜利音效
            
            // 切换到胜利背景音乐
            if (_m_victoryMusic != null)
            {
                playBackgroundMusic(_m_victoryMusic, true, false);
            }
        }
        
        /// <summary>
        /// 游戏状态变更事件处理
        /// </summary>
        /// <param name="_newState">新的游戏状态</param>
        private void _onGameStateChanged(GameState _oldState, GameState _newState)
        {
            switch (_newState)
            {
                case GameState.MainMenu:
                    if (_m_mainMenuMusic != null)
                    {
                        playBackgroundMusic(_m_mainMenuMusic, true, true);
                    }
                    break;
                    
                case GameState.GamePlaying:
                    if (_m_gameplayMusic != null)
                    {
                        playBackgroundMusic(_m_gameplayMusic, true, true);
                    }
                    break;
                    
                case GameState.GamePaused:
                    pauseBackgroundMusic();
                    break;
                    
                case GameState.GameEnded:
                    // 在_onGameEnded中处理
                    break;
            }
        }
        
        /// <summary>
        /// 方块放置失败事件处理
        /// </summary>
        /// <param name="_piece">尝试放置的方块</param>
        /// <param name="_position">尝试放置的位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_reason">失败原因</param>
        private void _onPiecePlacementFailed(_IGamePiece _piece, Vector2Int _position, int _playerId, string _reason)
        {
            playInvalidActionSound();
        }
        
        #endregion
    }
}