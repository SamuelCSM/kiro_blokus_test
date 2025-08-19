using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BlokusGame.Core.Managers;
using BlokusGame.Core.Data;
using BlokusGame.Core.Events;

namespace BlokusGame.Core.UI
{
    /// <summary>
    /// 设置UI - 游戏设置和选项界面
    /// 提供音效、画质、语言等设置选项
    /// </summary>
    public class SettingsUI : UIBase
    {
        [Header("设置UI配置")]
        /// <summary>设置标题文本</summary>
        [SerializeField] private Text _m_titleText;
        
        /// <summary>返回按钮</summary>
        [SerializeField] private Button _m_backButton;
        
        /// <summary>重置设置按钮</summary>
        [SerializeField] private Button _m_resetButton;
        
        /// <summary>应用设置按钮</summary>
        [SerializeField] private Button _m_applyButton;
        
        [Header("音效设置")]
        /// <summary>主音量滑块</summary>
        [SerializeField] private Slider _m_masterVolumeSlider;
        
        /// <summary>主音量文本</summary>
        [SerializeField] private Text _m_masterVolumeText;
        
        /// <summary>音效音量滑块</summary>
        [SerializeField] private Slider _m_sfxVolumeSlider;
        
        /// <summary>音效音量文本</summary>
        [SerializeField] private Text _m_sfxVolumeText;
        
        /// <summary>背景音乐音量滑块</summary>
        [SerializeField] private Slider _m_musicVolumeSlider;
        
        /// <summary>背景音乐音量文本</summary>
        [SerializeField] private Text _m_musicVolumeText;
        
        /// <summary>静音切换</summary>
        [SerializeField] private Toggle _m_muteToggle;
        
        [Header("画质设置")]
        /// <summary>画质等级下拉菜单</summary>
        [SerializeField] private Dropdown _m_qualityDropdown;
        
        /// <summary>分辨率下拉菜单</summary>
        [SerializeField] private Dropdown _m_resolutionDropdown;
        
        /// <summary>全屏切换</summary>
        [SerializeField] private Toggle _m_fullscreenToggle;
        
        /// <summary>垂直同步切换</summary>
        [SerializeField] private Toggle _m_vsyncToggle;
        
        /// <summary>帧率限制滑块</summary>
        [SerializeField] private Slider _m_frameRateSlider;
        
        /// <summary>帧率限制文本</summary>
        [SerializeField] private Text _m_frameRateText;
        
        [Header("游戏设置")]
        /// <summary>语言选择下拉菜单</summary>
        [SerializeField] private Dropdown _m_languageDropdown;
        
        /// <summary>自动保存切换</summary>
        [SerializeField] private Toggle _m_autoSaveToggle;
        
        /// <summary>显示提示切换</summary>
        [SerializeField] private Toggle _m_showHintsToggle;
        
        /// <summary>动画速度滑块</summary>
        [SerializeField] private Slider _m_animationSpeedSlider;
        
        /// <summary>动画速度文本</summary>
        [SerializeField] private Text _m_animationSpeedText;
        
        /// <summary>触觉反馈切换</summary>
        [SerializeField] private Toggle _m_hapticFeedbackToggle;
        
        [Header("控制设置")]
        /// <summary>触摸灵敏度滑块</summary>
        [SerializeField] private Slider _m_touchSensitivitySlider;
        
        /// <summary>触摸灵敏度文本</summary>
        [SerializeField] private Text _m_touchSensitivityText;
        
        /// <summary>双击间隔滑块</summary>
        [SerializeField] private Slider _m_doubleTapIntervalSlider;
        
        /// <summary>双击间隔文本</summary>
        [SerializeField] private Text _m_doubleTapIntervalText;
        
        // 私有字段 - 设置值
        /// <summary>当前设置数据</summary>
        private GameSettings _m_currentSettings;
        
        /// <summary>可用的屏幕分辨率数组</summary>
        private Resolution[] _m_availableResolutions;
        
        /// <summary>设置是否已修改的标志</summary>
        private bool _m_settingsModified = false;
        
        #region UIBase实现
        
        /// <summary>
        /// 重写排序等级，设置UI显示优先级
        /// 设置UI的排序等级为100，确保在其他UI之上显示
        /// </summary>
        public override int sortingOrder => 100;
        
        /// <summary>
        /// 初始化UI特定内容
        /// </summary>
        protected override void InitializeUIContent()
        {
            _setupBasicUI();
            _bindBasicEvents();
        }
        
        /// <summary>
        /// 处理UI显示时的逻辑
        /// </summary>
        protected override void OnUIShown()
        {
            _loadCurrentSettings();
        }
        
        /// <summary>
        /// 处理UI隐藏时的逻辑
        /// </summary>
        protected override void OnUIHidden()
        {
            // 隐藏时的清理逻辑
        }
        
        #endregion
        
        #region 私有方法 - 基础设置
        
        /// <summary>
        /// 设置基础UI元素
        /// </summary>
        private void _setupBasicUI()
        {
            if (_m_titleText != null)
            {
                _m_titleText.text = "游戏设置";
            }
            
            _setupAudioUI();
            _setupGraphicsUI();
            _setupGameplayUI();
            _setupControlsUI();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[SettingsUI] 基础UI设置完成");
            }
        }
        
        /// <summary>
        /// 绑定基础事件
        /// </summary>
        private void _bindBasicEvents()
        {
            if (_m_backButton != null)
                _m_backButton.onClick.AddListener(_onBackClicked);
            
            if (_m_resetButton != null)
                _m_resetButton.onClick.AddListener(_onResetClicked);
            
            if (_m_applyButton != null)
                _m_applyButton.onClick.AddListener(_onApplyClicked);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[SettingsUI] 基础事件绑定完成");
            }
        }
        
        /// <summary>
        /// 设置音频UI
        /// </summary>
        private void _setupAudioUI()
        {
            // 设置音量滑块范围
            if (_m_masterVolumeSlider != null)
            {
                _m_masterVolumeSlider.minValue = 0f;
                _m_masterVolumeSlider.maxValue = 1f;
                _m_masterVolumeSlider.onValueChanged.AddListener(_onMasterVolumeChanged);
            }
            
            if (_m_sfxVolumeSlider != null)
            {
                _m_sfxVolumeSlider.minValue = 0f;
                _m_sfxVolumeSlider.maxValue = 1f;
                _m_sfxVolumeSlider.onValueChanged.AddListener(_onSFXVolumeChanged);
            }
            
            if (_m_musicVolumeSlider != null)
            {
                _m_musicVolumeSlider.minValue = 0f;
                _m_musicVolumeSlider.maxValue = 1f;
                _m_musicVolumeSlider.onValueChanged.AddListener(_onMusicVolumeChanged);
            }
            
            if (_m_muteToggle != null)
            {
                _m_muteToggle.onValueChanged.AddListener(_onMuteToggleChanged);
            }
        }
        
        /// <summary>
        /// 设置画质UI
        /// </summary>
        private void _setupGraphicsUI()
        {
            // 设置画质选项
            if (_m_qualityDropdown != null)
            {
                _m_qualityDropdown.ClearOptions();
                var qualityOptions = new List<string> { "低", "中", "高", "极高" };
                _m_qualityDropdown.AddOptions(qualityOptions);
                _m_qualityDropdown.onValueChanged.AddListener(_onQualityChanged);
            }
            
            // 设置分辨率选项
            if (_m_resolutionDropdown != null)
            {
                _setupResolutionDropdown();
                _m_resolutionDropdown.onValueChanged.AddListener(_onResolutionChanged);
            }
            
            // 设置其他画质选项
            if (_m_fullscreenToggle != null)
                _m_fullscreenToggle.onValueChanged.AddListener(_onFullscreenToggleChanged);
            
            if (_m_vsyncToggle != null)
                _m_vsyncToggle.onValueChanged.AddListener(_onVSyncToggleChanged);
            
            if (_m_frameRateSlider != null)
            {
                _m_frameRateSlider.minValue = 30f;
                _m_frameRateSlider.maxValue = 120f;
                _m_frameRateSlider.wholeNumbers = true;
                _m_frameRateSlider.onValueChanged.AddListener(_onFrameRateChanged);
            }
        }
        
        /// <summary>
        /// 设置游戏玩法UI
        /// </summary>
        private void _setupGameplayUI()
        {
            // 设置语言选项
            if (_m_languageDropdown != null)
            {
                _m_languageDropdown.ClearOptions();
                var languageOptions = new List<string> { "中文", "English" };
                _m_languageDropdown.AddOptions(languageOptions);
                _m_languageDropdown.onValueChanged.AddListener(_onLanguageChanged);
            }
            
            // 设置游戏选项
            if (_m_autoSaveToggle != null)
                _m_autoSaveToggle.onValueChanged.AddListener(_onAutoSaveToggleChanged);
            
            if (_m_showHintsToggle != null)
                _m_showHintsToggle.onValueChanged.AddListener(_onShowHintsToggleChanged);
            
            if (_m_hapticFeedbackToggle != null)
                _m_hapticFeedbackToggle.onValueChanged.AddListener(_onHapticFeedbackToggleChanged);
            
            if (_m_animationSpeedSlider != null)
            {
                _m_animationSpeedSlider.minValue = 0.5f;
                _m_animationSpeedSlider.maxValue = 2f;
                _m_animationSpeedSlider.onValueChanged.AddListener(_onAnimationSpeedChanged);
            }
        }
        
        /// <summary>
        /// 设置控制UI
        /// </summary>
        private void _setupControlsUI()
        {
            if (_m_touchSensitivitySlider != null)
            {
                _m_touchSensitivitySlider.minValue = 0.5f;
                _m_touchSensitivitySlider.maxValue = 2f;
                _m_touchSensitivitySlider.onValueChanged.AddListener(_onTouchSensitivityChanged);
            }
            
            if (_m_doubleTapIntervalSlider != null)
            {
                _m_doubleTapIntervalSlider.minValue = 0.1f;
                _m_doubleTapIntervalSlider.maxValue = 0.5f;
                _m_doubleTapIntervalSlider.onValueChanged.AddListener(_onDoubleTapIntervalChanged);
            }
        }
        
        /// <summary>
        /// 设置分辨率下拉菜单
        /// </summary>
        private void _setupResolutionDropdown()
        {
            _m_availableResolutions = Screen.resolutions;
            _m_resolutionDropdown.ClearOptions();
            
            var resolutionOptions = new List<string>();
            for (int i = 0; i < _m_availableResolutions.Length; i++)
            {
                string option = $"{_m_availableResolutions[i].width} x {_m_availableResolutions[i].height}";
                resolutionOptions.Add(option);
            }
            
            _m_resolutionDropdown.AddOptions(resolutionOptions);
        }
        
        /// <summary>
        /// 加载当前设置
        /// </summary>
        private void _loadCurrentSettings()
        {
            // TODO: 从设置管理器加载当前设置
            _m_currentSettings = _getDefaultSettings();
            _applySettingsToUI();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[SettingsUI] 加载当前设置");
            }
        }
        
        /// <summary>
        /// 获取默认设置
        /// </summary>
        /// <returns>默认游戏设置</returns>
        private GameSettings _getDefaultSettings()
        {
            return new GameSettings
            {
                // 音频设置
                masterVolume = 1.0f,
                sfxVolume = 1.0f,
                musicVolume = 0.8f,
                isMuted = false,
                
                // 画质设置
                qualityLevel = 2, // 高画质
                screenWidth = Screen.currentResolution.width,
                screenHeight = Screen.currentResolution.height,
                isFullscreen = true,
                enableVSync = true,
                targetFrameRate = 60,
                
                // 游戏玩法设置
                languageIndex = 0, // 中文
                enableAutoSave = true,
                showHints = true,
                enableHapticFeedback = true,
                animationSpeed = 1.0f,
                
                // 控制设置
                touchSensitivity = 1.0f,
                doubleTapInterval = 0.3f
            };
        }
        
        /// <summary>
        /// 将设置应用到UI控件
        /// </summary>
        private void _applySettingsToUI()
        {
            if (_m_currentSettings == null) return;
            
            // 应用音频设置
            if (_m_masterVolumeSlider != null)
            {
                _m_masterVolumeSlider.value = _m_currentSettings.masterVolume;
                _updateVolumeText(_m_masterVolumeText, _m_currentSettings.masterVolume);
            }
            if (_m_sfxVolumeSlider != null)
            {
                _m_sfxVolumeSlider.value = _m_currentSettings.sfxVolume;
                _updateVolumeText(_m_sfxVolumeText, _m_currentSettings.sfxVolume);
            }
            if (_m_musicVolumeSlider != null)
            {
                _m_musicVolumeSlider.value = _m_currentSettings.musicVolume;
                _updateVolumeText(_m_musicVolumeText, _m_currentSettings.musicVolume);
            }
            if (_m_muteToggle != null)
                _m_muteToggle.isOn = _m_currentSettings.isMuted;
            
            // 应用画质设置
            if (_m_qualityDropdown != null)
                _m_qualityDropdown.value = _m_currentSettings.qualityLevel;
            if (_m_resolutionDropdown != null)
                _m_resolutionDropdown.value = _getCurrentResolutionIndex();
            if (_m_fullscreenToggle != null)
                _m_fullscreenToggle.isOn = _m_currentSettings.isFullscreen;
            if (_m_vsyncToggle != null)
                _m_vsyncToggle.isOn = _m_currentSettings.enableVSync;
            if (_m_frameRateSlider != null)
            {
                _m_frameRateSlider.value = _m_currentSettings.targetFrameRate;
                _updateFrameRateText(_m_currentSettings.targetFrameRate);
            }
            
            // 应用游戏玩法设置
            if (_m_languageDropdown != null)
                _m_languageDropdown.value = _m_currentSettings.languageIndex;
            if (_m_autoSaveToggle != null)
                _m_autoSaveToggle.isOn = _m_currentSettings.enableAutoSave;
            if (_m_showHintsToggle != null)
                _m_showHintsToggle.isOn = _m_currentSettings.showHints;
            if (_m_hapticFeedbackToggle != null)
                _m_hapticFeedbackToggle.isOn = _m_currentSettings.enableHapticFeedback;
            if (_m_animationSpeedSlider != null)
            {
                _m_animationSpeedSlider.value = _m_currentSettings.animationSpeed;
                _updateAnimationSpeedText(_m_currentSettings.animationSpeed);
            }
            
            // 应用控制设置
            if (_m_touchSensitivitySlider != null)
            {
                _m_touchSensitivitySlider.value = _m_currentSettings.touchSensitivity;
                _updateTouchSensitivityText(_m_currentSettings.touchSensitivity);
            }
            if (_m_doubleTapIntervalSlider != null)
            {
                _m_doubleTapIntervalSlider.value = _m_currentSettings.doubleTapInterval;
                _updateDoubleTapIntervalText(_m_currentSettings.doubleTapInterval);
            }
        }
        
        /// <summary>
        /// 获取当前分辨率在可用分辨率列表中的索引
        /// </summary>
        /// <returns>分辨率索引，如果未找到返回0</returns>
        private int _getCurrentResolutionIndex()
        {
            if (_m_availableResolutions == null || _m_currentSettings == null)
                return 0;
            
            // 查找匹配当前设置的分辨率
            for (int i = 0; i < _m_availableResolutions.Length; i++)
            {
                var resolution = _m_availableResolutions[i];
                if (resolution.width == _m_currentSettings.screenWidth && 
                    resolution.height == _m_currentSettings.screenHeight)
                {
                    return i;
                }
            }
            
            // 如果没有找到匹配的分辨率，返回最接近的或默认值
            return 0;
        }
        
        /// <summary>
        /// 标记设置已修改
        /// </summary>
        private void _markSettingsModified()
        {
            _m_settingsModified = true;
            
            // 可以在这里更新UI显示，比如启用应用按钮
            if (_m_applyButton != null)
            {
                _m_applyButton.interactable = true;
            }
        }
        
        /// <summary>
        /// 更新帧率显示文本
        /// </summary>
        /// <param name="_frameRate">帧率值</param>
        private void _updateFrameRateText(float _frameRate)
        {
            if (_m_frameRateText != null)
            {
                _m_frameRateText.text = $"{(int)_frameRate} FPS";
            }
        }
        
        /// <summary>
        /// 更新音量显示文本
        /// </summary>
        /// <param name="_textComponent">要更新的文本组件</param>
        /// <param name="_volume">音量值</param>
        private void _updateVolumeText(Text _textComponent, float _volume)
        {
            if (_textComponent != null)
            {
                _textComponent.text = $"{(_volume * 100):F0}%";
            }
        }
        
        /// <summary>
        /// 更新动画速度显示文本
        /// </summary>
        /// <param name="_speed">动画速度值</param>
        private void _updateAnimationSpeedText(float _speed)
        {
            if (_m_animationSpeedText != null)
            {
                _m_animationSpeedText.text = $"{_speed:F1}x";
            }
        }
        
        /// <summary>
        /// 更新触摸灵敏度显示文本
        /// </summary>
        /// <param name="_sensitivity">灵敏度值</param>
        private void _updateTouchSensitivityText(float _sensitivity)
        {
            if (_m_touchSensitivityText != null)
            {
                _m_touchSensitivityText.text = $"{_sensitivity:F1}";
            }
        }
        
        /// <summary>
        /// 更新双击间隔显示文本
        /// </summary>
        /// <param name="_interval">间隔时间值</param>
        private void _updateDoubleTapIntervalText(float _interval)
        {
            if (_m_doubleTapIntervalText != null)
            {
                _m_doubleTapIntervalText.text = $"{_interval:F2}s";
            }
        }
        
        #endregion
        
        #region 事件处理方法
        
        /// <summary>
        /// 主音量滑块值变化事件处理
        /// </summary>
        /// <param name="_value">新的音量值</param>
        private void _onMasterVolumeChanged(float _value)
        {
            if (_m_currentSettings != null)
            {
                _m_currentSettings.masterVolume = _value;
                _updateVolumeText(_m_masterVolumeText, _value);
                _applyAudioSettings();
                _markSettingsModified();
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[SettingsUI] 主音量变更为: {_value:F2}");
                }
            }
        }
        
        /// <summary>
        /// 音效音量滑块值变化事件处理
        /// </summary>
        /// <param name="_value">新的音量值</param>
        private void _onSFXVolumeChanged(float _value)
        {
            if (_m_currentSettings != null)
            {
                _m_currentSettings.sfxVolume = _value;
                _updateVolumeText(_m_sfxVolumeText, _value);
                _applyAudioSettings();
                _markSettingsModified();
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[SettingsUI] 音效音量变更为: {_value:F2}");
                }
            }
        }
        
        /// <summary>
        /// 音乐音量滑块值变化事件处理
        /// </summary>
        /// <param name="_value">新的音量值</param>
        private void _onMusicVolumeChanged(float _value)
        {
            if (_m_currentSettings != null)
            {
                _m_currentSettings.musicVolume = _value;
                _updateVolumeText(_m_musicVolumeText, _value);
                _applyAudioSettings();
                _markSettingsModified();
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[SettingsUI] 音乐音量变更为: {_value:F2}");
                }
            }
        }
        
        /// <summary>
        /// 静音开关变化事件处理
        /// </summary>
        /// <param name="_isMuted">是否静音</param>
        private void _onMuteToggleChanged(bool _isMuted)
        {
            if (_m_currentSettings != null)
            {
                _m_currentSettings.isMuted = _isMuted;
                _applyAudioSettings();
                _markSettingsModified();
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[SettingsUI] 静音状态变更为: {_isMuted}");
                }
            }
        }
        
        /// <summary>
        /// 画质等级下拉菜单变化事件处理
        /// </summary>
        /// <param name="_qualityIndex">画质等级索引</param>
        private void _onQualityChanged(int _qualityIndex)
        {
            if (_m_currentSettings != null)
            {
                _m_currentSettings.qualityLevel = _qualityIndex;
                _applyGraphicsSettings();
                _markSettingsModified();
                
                if (_m_enableDetailedLogging)
                {
                    string[] qualityNames = { "低", "中", "高", "极高" };
                    string qualityName = _qualityIndex < qualityNames.Length ? qualityNames[_qualityIndex] : "未知";
                    Debug.Log($"[SettingsUI] 画质等级变更为: {qualityName}");
                }
            }
        }
        
        /// <summary>
        /// 分辨率下拉菜单变化事件处理
        /// </summary>
        /// <param name="_resolutionIndex">分辨率索引</param>
        private void _onResolutionChanged(int _resolutionIndex)
        {
            if (_m_currentSettings != null && _m_availableResolutions != null && 
                _resolutionIndex >= 0 && _resolutionIndex < _m_availableResolutions.Length)
            {
                var resolution = _m_availableResolutions[_resolutionIndex];
                _m_currentSettings.screenWidth = resolution.width;
                _m_currentSettings.screenHeight = resolution.height;
                _applyGraphicsSettings();
                _markSettingsModified();
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[SettingsUI] 分辨率变更为: {resolution.width}x{resolution.height}");
                }
            }
        }
        
        /// <summary>
        /// 全屏开关变化事件处理
        /// </summary>
        /// <param name="_isFullscreen">是否全屏</param>
        private void _onFullscreenToggleChanged(bool _isFullscreen)
        {
            if (_m_currentSettings != null)
            {
                _m_currentSettings.isFullscreen = _isFullscreen;
                _applyGraphicsSettings();
                _markSettingsModified();
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[SettingsUI] 全屏模式变更为: {_isFullscreen}");
                }
            }
        }
        
        /// <summary>
        /// 垂直同步开关变化事件处理
        /// </summary>
        /// <param name="_enableVSync">是否启用垂直同步</param>
        private void _onVSyncToggleChanged(bool _enableVSync)
        {
            if (_m_currentSettings != null)
            {
                _m_currentSettings.enableVSync = _enableVSync;
                _applyGraphicsSettings();
                _markSettingsModified();
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[SettingsUI] 垂直同步变更为: {_enableVSync}");
                }
            }
        }
        
        /// <summary>
        /// 帧率滑块值变化事件处理
        /// </summary>
        /// <param name="_frameRate">目标帧率</param>
        private void _onFrameRateChanged(float _frameRate)
        {
            if (_m_currentSettings != null)
            {
                _m_currentSettings.targetFrameRate = (int)_frameRate;
                _updateFrameRateText(_frameRate);
                _applyGraphicsSettings();
                _markSettingsModified();
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[SettingsUI] 目标帧率变更为: {(int)_frameRate}");
                }
            }
        }
        
        /// <summary>
        /// 语言下拉菜单变化事件处理
        /// </summary>
        /// <param name="_languageIndex">语言索引</param>
        private void _onLanguageChanged(int _languageIndex)
        {
            if (_m_currentSettings != null)
            {
                string[] languages = { "中文", "English" };
                if (_languageIndex >= 0 && _languageIndex < languages.Length)
                {
                    _m_currentSettings.languageIndex = _languageIndex;
                    _applyGameplaySettings();
                    _markSettingsModified();
                    
                    if (_m_enableDetailedLogging)
                    {
                        Debug.Log($"[SettingsUI] 语言变更为: {languages[_languageIndex]}");
                    }
                }
            }
        }
        
        /// <summary>
        /// 自动保存开关变化事件处理
        /// </summary>
        /// <param name="_enableAutoSave">是否启用自动保存</param>
        private void _onAutoSaveToggleChanged(bool _enableAutoSave)
        {
            if (_m_currentSettings != null)
            {
                _m_currentSettings.enableAutoSave = _enableAutoSave;
                _applyGameplaySettings();
                _markSettingsModified();
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[SettingsUI] 自动保存变更为: {_enableAutoSave}");
                }
            }
        }
        
        /// <summary>
        /// 显示提示开关变化事件处理
        /// </summary>
        /// <param name="_showHints">是否显示提示</param>
        private void _onShowHintsToggleChanged(bool _showHints)
        {
            if (_m_currentSettings != null)
            {
                _m_currentSettings.showHints = _showHints;
                _applyGameplaySettings();
                _markSettingsModified();
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[SettingsUI] 显示提示变更为: {_showHints}");
                }
            }
        }
        
        /// <summary>
        /// 触觉反馈开关变化事件处理
        /// </summary>
        /// <param name="_enableHapticFeedback">是否启用触觉反馈</param>
        private void _onHapticFeedbackToggleChanged(bool _enableHapticFeedback)
        {
            if (_m_currentSettings != null)
            {
                _m_currentSettings.enableHapticFeedback = _enableHapticFeedback;
                _applyGameplaySettings();
                _markSettingsModified();
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[SettingsUI] 触觉反馈变更为: {_enableHapticFeedback}");
                }
            }
        }
        
        /// <summary>
        /// 动画速度滑块值变化事件处理
        /// </summary>
        /// <param name="_animationSpeed">动画速度倍率</param>
        private void _onAnimationSpeedChanged(float _animationSpeed)
        {
            if (_m_currentSettings != null)
            {
                _m_currentSettings.animationSpeed = _animationSpeed;
                _updateAnimationSpeedText(_animationSpeed);
                _applyGameplaySettings();
                _markSettingsModified();
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[SettingsUI] 动画速度变更为: {_animationSpeed:F1}x");
                }
            }
        }
        
        /// <summary>
        /// 触摸灵敏度滑块值变化事件处理
        /// </summary>
        /// <param name="_sensitivity">触摸灵敏度</param>
        private void _onTouchSensitivityChanged(float _sensitivity)
        {
            if (_m_currentSettings != null)
            {
                _m_currentSettings.touchSensitivity = _sensitivity;
                _updateTouchSensitivityText(_sensitivity);
                _applyControlSettings();
                _markSettingsModified();
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[SettingsUI] 触摸灵敏度变更为: {_sensitivity:F1}");
                }
            }
        }
        
        /// <summary>
        /// 双击间隔滑块值变化事件处理
        /// </summary>
        /// <param name="_interval">双击间隔时间</param>
        private void _onDoubleTapIntervalChanged(float _interval)
        {
            if (_m_currentSettings != null)
            {
                _m_currentSettings.doubleTapInterval = _interval;
                _updateDoubleTapIntervalText(_interval);
                _applyControlSettings();
                _markSettingsModified();
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[SettingsUI] 双击间隔变更为: {_interval:F2}秒");
                }
            }
        }
        
        #endregion
        
        #region 设置应用方法
        
        /// <summary>
        /// 应用音频设置到系统
        /// </summary>
        private void _applyAudioSettings()
        {
            if (_m_currentSettings == null) return;
            
            // 应用主音量
            AudioListener.volume = _m_currentSettings.isMuted ? 0f : _m_currentSettings.masterVolume;
            
            // TODO: 应用音效和音乐音量到音频管理器
            // AudioManager.instance?.setMasterVolume(_m_currentSettings.masterVolume);
            // AudioManager.instance?.setSFXVolume(_m_currentSettings.sfxVolume);
            // AudioManager.instance?.setMusicVolume(_m_currentSettings.musicVolume);
            // AudioManager.instance?.setMuted(_m_currentSettings.isMuted);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[SettingsUI] 音频设置已应用 - 主音量: {_m_currentSettings.masterVolume:F2}, 静音: {_m_currentSettings.isMuted}");
            }
        }
        
        /// <summary>
        /// 应用画质设置到系统
        /// </summary>
        private void _applyGraphicsSettings()
        {
            if (_m_currentSettings == null) return;
            
            // 应用画质等级
            QualitySettings.SetQualityLevel(_m_currentSettings.qualityLevel, true);
            
            // 应用分辨率设置
            Screen.SetResolution(_m_currentSettings.screenWidth, _m_currentSettings.screenHeight, _m_currentSettings.isFullscreen);
            
            // 应用垂直同步
            QualitySettings.vSyncCount = _m_currentSettings.enableVSync ? 1 : 0;
            
            // 应用目标帧率
            Application.targetFrameRate = _m_currentSettings.targetFrameRate;
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[SettingsUI] 画质设置已应用 - 等级: {_m_currentSettings.qualityLevel}, 帧率: {_m_currentSettings.targetFrameRate}");
            }
        }
        
        /// <summary>
        /// 应用游戏玩法设置到系统
        /// </summary>
        private void _applyGameplaySettings()
        {
            if (_m_currentSettings == null) return;
            
            // TODO: 应用语言设置到本地化管理器
            // string[] languages = { "中文", "English" };
            // string currentLanguage = _m_currentSettings.languageIndex < languages.Length ? languages[_m_currentSettings.languageIndex] : "中文";
            // LocalizationManager.instance?.setLanguage(currentLanguage);
            
            // TODO: 应用其他游戏设置到游戏管理器
            // GameManager.instance?.setAutoSave(_m_currentSettings.enableAutoSave);
            // GameManager.instance?.setShowHints(_m_currentSettings.showHints);
            // GameManager.instance?.setAnimationSpeed(_m_currentSettings.animationSpeed);
            
            // 应用触觉反馈设置
            #if UNITY_ANDROID || UNITY_IOS
            if (_m_currentSettings.enableHapticFeedback)
            {
                // 启用移动设备触觉反馈
            }
            #endif
            
            if (_m_enableDetailedLogging)
            {
                string[] languages = { "中文", "English" };
                string currentLanguage = _m_currentSettings.languageIndex < languages.Length ? languages[_m_currentSettings.languageIndex] : "未知";
                Debug.Log($"[SettingsUI] 游戏玩法设置已应用 - 语言: {currentLanguage}, 自动保存: {_m_currentSettings.enableAutoSave}");
            }
        }
        
        /// <summary>
        /// 应用控制设置到系统
        /// </summary>
        private void _applyControlSettings()
        {
            if (_m_currentSettings == null) return;
            
            // TODO: 应用控制设置到输入管理器
            // InputManager.instance?.setTouchSensitivity(_m_currentSettings.touchSensitivity);
            // InputManager.instance?.setDoubleTapInterval(_m_currentSettings.doubleTapInterval);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[SettingsUI] 控制设置已应用 - 触摸灵敏度: {_m_currentSettings.touchSensitivity:F1}, 双击间隔: {_m_currentSettings.doubleTapInterval:F2}");
            }
        }
        
        #endregion
        
        #region 按钮事件处理
        
        /// <summary>
        /// 处理返回按钮点击
        /// </summary>
        private void _onBackClicked()
        {
            if (_m_settingsModified)
            {
                // TODO: 显示确认对话框询问是否保存设置
                UIManager.instance?.ShowMessage("设置已修改，是否保存？", MessageType.Warning);
            }
            
            UIManager.instance?.GoBack();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[SettingsUI] 返回上一界面");
            }
        }
        
        /// <summary>
        /// 处理重置按钮点击
        /// </summary>
        private void _onResetClicked()
        {
            _m_currentSettings = _getDefaultSettings();
            _applySettingsToUI();
            _markSettingsModified();
            
            UIManager.instance?.ShowMessage("设置已重置为默认值", MessageType.Info);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[SettingsUI] 重置设置到默认值");
            }
        }
        
        /// <summary>
        /// 处理应用按钮点击
        /// </summary>
        private void _onApplyClicked()
        {
            _applyAllSettings();
            _saveSettings();
            _m_settingsModified = false;
            
            if (_m_applyButton != null)
                _m_applyButton.interactable = false;
            
            UIManager.instance?.ShowMessage("设置已保存并应用", MessageType.Success);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[SettingsUI] 应用并保存设置");
            }
        }
        
        /// <summary>
        /// 应用所有设置到系统
        /// </summary>
        private void _applyAllSettings()
        {
            _applyAudioSettings();
            _applyGraphicsSettings();
            _applyGameplaySettings();
            _applyControlSettings();
        }
        
        /// <summary>
        /// 保存设置到持久化存储
        /// </summary>
        private void _saveSettings()
        {
            // TODO: 实现设置保存到PlayerPrefs或文件
            if (_m_currentSettings != null)
            {
                // 保存音频设置
                PlayerPrefs.SetFloat("MasterVolume", _m_currentSettings.masterVolume);
                PlayerPrefs.SetFloat("SFXVolume", _m_currentSettings.sfxVolume);
                PlayerPrefs.SetFloat("MusicVolume", _m_currentSettings.musicVolume);
                PlayerPrefs.SetInt("IsMuted", _m_currentSettings.isMuted ? 1 : 0);
                
                // 保存画质设置
                PlayerPrefs.SetInt("QualityLevel", _m_currentSettings.qualityLevel);
                PlayerPrefs.SetInt("IsFullscreen", _m_currentSettings.isFullscreen ? 1 : 0);
                PlayerPrefs.SetInt("EnableVSync", _m_currentSettings.enableVSync ? 1 : 0);
                PlayerPrefs.SetInt("TargetFrameRate", _m_currentSettings.targetFrameRate);
                
                // 保存游戏设置
                PlayerPrefs.SetInt("LanguageIndex", _m_currentSettings.languageIndex);
                PlayerPrefs.SetInt("EnableAutoSave", _m_currentSettings.enableAutoSave ? 1 : 0);
                PlayerPrefs.SetInt("ShowHints", _m_currentSettings.showHints ? 1 : 0);
                PlayerPrefs.SetInt("EnableHapticFeedback", _m_currentSettings.enableHapticFeedback ? 1 : 0);
                PlayerPrefs.SetFloat("AnimationSpeed", _m_currentSettings.animationSpeed);
                
                // 保存控制设置
                PlayerPrefs.SetFloat("TouchSensitivity", _m_currentSettings.touchSensitivity);
                PlayerPrefs.SetFloat("DoubleTapInterval", _m_currentSettings.doubleTapInterval);
                
                PlayerPrefs.Save();
            }
        }
        
        #endregion
    }
}