using UnityEngine;

namespace BlokusGame.Core.Data
{
    /// <summary>
    /// 游戏设置数据类
    /// 存储所有游戏相关的设置选项
    /// </summary>
    [System.Serializable]
    public class GameSettings
    {
        [Header("音频设置")]
        /// <summary>主音量 (0-1)</summary>
        public float masterVolume = 1f;
        
        /// <summary>音效音量 (0-1)</summary>
        public float sfxVolume = 1f;
        
        /// <summary>背景音乐音量 (0-1)</summary>
        public float musicVolume = 0.7f;
        
        /// <summary>是否静音</summary>
        public bool isMuted = false;
        
        [Header("画质设置")]
        /// <summary>画质等级 (0-3: 低、中、高、极高)</summary>
        public int qualityLevel = 2;
        
        /// <summary>屏幕宽度</summary>
        public int screenWidth = 1920;
        
        /// <summary>屏幕高度</summary>
        public int screenHeight = 1080;
        
        /// <summary>是否全屏</summary>
        public bool isFullscreen = true;
        
        /// <summary>是否启用垂直同步</summary>
        public bool enableVSync = true;
        
        /// <summary>目标帧率</summary>
        public int targetFrameRate = 60;
        
        [Header("游戏设置")]
        /// <summary>语言索引 (0: 中文, 1: English)</summary>
        public int languageIndex = 0;
        
        /// <summary>是否启用自动保存</summary>
        public bool enableAutoSave = true;
        
        /// <summary>是否显示提示</summary>
        public bool showHints = true;
        
        /// <summary>是否启用触觉反馈</summary>
        public bool enableHapticFeedback = true;
        
        /// <summary>动画速度倍率 (0.5-2.0)</summary>
        public float animationSpeed = 1f;
        
        [Header("控制设置")]
        /// <summary>触摸灵敏度 (0.5-2.0)</summary>
        public float touchSensitivity = 1f;
        
        /// <summary>双击间隔时间 (0.1-0.5秒)</summary>
        public float doubleTapInterval = 0.3f;
        
        /// <summary>
        /// 构造函数 - 使用默认值
        /// </summary>
        public GameSettings()
        {
            // 使用字段的默认值
        }
        
        /// <summary>
        /// 复制构造函数
        /// </summary>
        /// <param name="_other">要复制的设置对象</param>
        public GameSettings(GameSettings _other)
        {
            if (_other == null) return;
            
            // 音频设置
            masterVolume = _other.masterVolume;
            sfxVolume = _other.sfxVolume;
            musicVolume = _other.musicVolume;
            isMuted = _other.isMuted;
            
            // 画质设置
            qualityLevel = _other.qualityLevel;
            screenWidth = _other.screenWidth;
            screenHeight = _other.screenHeight;
            isFullscreen = _other.isFullscreen;
            enableVSync = _other.enableVSync;
            targetFrameRate = _other.targetFrameRate;
            
            // 游戏设置
            languageIndex = _other.languageIndex;
            enableAutoSave = _other.enableAutoSave;
            showHints = _other.showHints;
            enableHapticFeedback = _other.enableHapticFeedback;
            animationSpeed = _other.animationSpeed;
            
            // 控制设置
            touchSensitivity = _other.touchSensitivity;
            doubleTapInterval = _other.doubleTapInterval;
        }
        
        /// <summary>
        /// 验证设置值的有效性
        /// </summary>
        /// <returns>是否有效</returns>
        public bool IsValid()
        {
            // 检查音频设置
            if (masterVolume < 0f || masterVolume > 1f) return false;
            if (sfxVolume < 0f || sfxVolume > 1f) return false;
            if (musicVolume < 0f || musicVolume > 1f) return false;
            
            // 检查画质设置
            if (qualityLevel < 0 || qualityLevel > 3) return false;
            if (screenWidth <= 0 || screenHeight <= 0) return false;
            if (targetFrameRate < 30 || targetFrameRate > 120) return false;
            
            // 检查游戏设置
            if (languageIndex < 0 || languageIndex > 1) return false;
            if (animationSpeed < 0.5f || animationSpeed > 2f) return false;
            
            // 检查控制设置
            if (touchSensitivity < 0.5f || touchSensitivity > 2f) return false;
            if (doubleTapInterval < 0.1f || doubleTapInterval > 0.5f) return false;
            
            return true;
        }
        
        /// <summary>
        /// 修正无效的设置值
        /// </summary>
        public void ClampValues()
        {
            // 修正音频设置
            masterVolume = Mathf.Clamp01(masterVolume);
            sfxVolume = Mathf.Clamp01(sfxVolume);
            musicVolume = Mathf.Clamp01(musicVolume);
            
            // 修正画质设置
            qualityLevel = Mathf.Clamp(qualityLevel, 0, 3);
            screenWidth = Mathf.Max(640, screenWidth);
            screenHeight = Mathf.Max(480, screenHeight);
            targetFrameRate = Mathf.Clamp(targetFrameRate, 30, 120);
            
            // 修正游戏设置
            languageIndex = Mathf.Clamp(languageIndex, 0, 1);
            animationSpeed = Mathf.Clamp(animationSpeed, 0.5f, 2f);
            
            // 修正控制设置
            touchSensitivity = Mathf.Clamp(touchSensitivity, 0.5f, 2f);
            doubleTapInterval = Mathf.Clamp(doubleTapInterval, 0.1f, 0.5f);
        }
        
        /// <summary>
        /// 重置为默认值
        /// </summary>
        public void ResetToDefaults()
        {
            // 音频设置
            masterVolume = 1f;
            sfxVolume = 1f;
            musicVolume = 0.7f;
            isMuted = false;
            
            // 画质设置
            qualityLevel = 2;
            screenWidth = Screen.currentResolution.width;
            screenHeight = Screen.currentResolution.height;
            isFullscreen = Screen.fullScreen;
            enableVSync = true;
            targetFrameRate = 60;
            
            // 游戏设置
            languageIndex = 0;
            enableAutoSave = true;
            showHints = true;
            enableHapticFeedback = true;
            animationSpeed = 1f;
            
            // 控制设置
            touchSensitivity = 1f;
            doubleTapInterval = 0.3f;
        }
        
        /// <summary>
        /// 转换为JSON字符串
        /// </summary>
        /// <returns>JSON字符串</returns>
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }
        
        /// <summary>
        /// 从JSON字符串创建设置对象
        /// </summary>
        /// <param name="_json">JSON字符串</param>
        /// <returns>设置对象</returns>
        public static GameSettings FromJson(string _json)
        {
            if (string.IsNullOrEmpty(_json))
                return new GameSettings();
            
            try
            {
                var settings = JsonUtility.FromJson<GameSettings>(_json);
                settings.ClampValues(); // 确保值有效
                return settings;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameSettings] 解析JSON失败: {ex.Message}");
                return new GameSettings();
            }
        }
        
        /// <summary>
        /// 比较两个设置对象是否相等
        /// </summary>
        /// <param name="_other">要比较的设置对象</param>
        /// <returns>是否相等</returns>
        public bool Equals(GameSettings _other)
        {
            if (_other == null) return false;
            
            return masterVolume == _other.masterVolume &&
                   sfxVolume == _other.sfxVolume &&
                   musicVolume == _other.musicVolume &&
                   isMuted == _other.isMuted &&
                   qualityLevel == _other.qualityLevel &&
                   screenWidth == _other.screenWidth &&
                   screenHeight == _other.screenHeight &&
                   isFullscreen == _other.isFullscreen &&
                   enableVSync == _other.enableVSync &&
                   targetFrameRate == _other.targetFrameRate &&
                   languageIndex == _other.languageIndex &&
                   enableAutoSave == _other.enableAutoSave &&
                   showHints == _other.showHints &&
                   enableHapticFeedback == _other.enableHapticFeedback &&
                   Mathf.Approximately(animationSpeed, _other.animationSpeed) &&
                   Mathf.Approximately(touchSensitivity, _other.touchSensitivity) &&
                   Mathf.Approximately(doubleTapInterval, _other.doubleTapInterval);
        }
        
        /// <summary>
        /// 获取设置的哈希码
        /// </summary>
        /// <returns>哈希码</returns>
        public override int GetHashCode()
        {
            return ToJson().GetHashCode();
        }
        
        /// <summary>
        /// 获取设置的字符串表示
        /// </summary>
        /// <returns>字符串表示</returns>
        public override string ToString()
        {
            return $"GameSettings: Quality={qualityLevel}, Resolution={screenWidth}x{screenHeight}, " +
                   $"MasterVolume={masterVolume:F2}, Language={languageIndex}";
        }
    }
}