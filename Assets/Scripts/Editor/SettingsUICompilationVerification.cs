using UnityEngine;
using UnityEditor;
using BlokusGame.Core.UI;
using BlokusGame.Core.Data;

namespace BlokusGame.Editor
{
    /// <summary>
    /// SettingsUI编译验证脚本
    /// 验证SettingsUI与GameSettings的数据结构匹配性
    /// </summary>
    public class SettingsUICompilationVerification
    {
        /// <summary>
        /// 验证SettingsUI编译状态
        /// </summary>
        [MenuItem("Blokus/验证/SettingsUI编译验证")]
        public static void VerifySettingsUICompilation()
        {
            Debug.Log("=== SettingsUI编译验证开始 ===");
            
            bool allTestsPassed = true;
            
            // 测试1：验证GameSettings数据结构
            allTestsPassed &= TestGameSettingsStructure();
            
            // 测试2：验证SettingsUI组件引用
            allTestsPassed &= TestSettingsUIComponents();
            
            // 测试3：验证数据字段匹配
            allTestsPassed &= TestDataFieldMatching();
            
            if (allTestsPassed)
            {
                Debug.Log("✅ SettingsUI编译验证通过！所有测试都成功。");
            }
            else
            {
                Debug.LogError("❌ SettingsUI编译验证失败！请检查上述错误。");
            }
            
            Debug.Log("=== SettingsUI编译验证结束 ===");
        }
        
        /// <summary>
        /// 测试GameSettings数据结构
        /// </summary>
        /// <returns>测试是否通过</returns>
        private static bool TestGameSettingsStructure()
        {
            Debug.Log("测试1：验证GameSettings数据结构");
            
            try
            {
                var settings = new GameSettings();
                
                // 验证音频字段
                float masterVolume = settings.masterVolume;
                float sfxVolume = settings.sfxVolume;
                float musicVolume = settings.musicVolume;
                bool isMuted = settings.isMuted;
                
                // 验证画质字段
                int qualityLevel = settings.qualityLevel;
                int screenWidth = settings.screenWidth;
                int screenHeight = settings.screenHeight;
                bool isFullscreen = settings.isFullscreen;
                bool enableVSync = settings.enableVSync;
                int targetFrameRate = settings.targetFrameRate;
                
                // 验证游戏设置字段
                int languageIndex = settings.languageIndex;
                bool enableAutoSave = settings.enableAutoSave;
                bool showHints = settings.showHints;
                bool enableHapticFeedback = settings.enableHapticFeedback;
                float animationSpeed = settings.animationSpeed;
                
                // 验证控制设置字段
                float touchSensitivity = settings.touchSensitivity;
                float doubleTapInterval = settings.doubleTapInterval;
                
                Debug.Log("✅ GameSettings数据结构验证通过");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ GameSettings数据结构验证失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 测试SettingsUI组件引用
        /// </summary>
        /// <returns>测试是否通过</returns>
        private static bool TestSettingsUIComponents()
        {
            Debug.Log("测试2：验证SettingsUI组件引用");
            
            try
            {
                // 创建临时GameObject来测试SettingsUI组件
                var testObject = new GameObject("TestSettingsUI");
                var settingsUI = testObject.AddComponent<SettingsUI>();
                
                // 验证组件创建成功
                if (settingsUI != null)
                {
                    Debug.Log("✅ SettingsUI组件创建成功");
                    
                    // 清理测试对象
                    Object.DestroyImmediate(testObject);
                    return true;
                }
                else
                {
                    Debug.LogError("❌ SettingsUI组件创建失败");
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ SettingsUI组件验证失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 测试数据字段匹配
        /// </summary>
        /// <returns>测试是否通过</returns>
        private static bool TestDataFieldMatching()
        {
            Debug.Log("测试3：验证数据字段匹配");
            
            try
            {
                var settings = new GameSettings();
                
                // 验证新字段存在且可访问
                settings.screenWidth = 1920;
                settings.screenHeight = 1080;
                settings.languageIndex = 0;
                
                // 验证字段值设置成功
                if (settings.screenWidth == 1920 && 
                    settings.screenHeight == 1080 && 
                    settings.languageIndex == 0)
                {
                    Debug.Log("✅ 数据字段匹配验证通过");
                    return true;
                }
                else
                {
                    Debug.LogError("❌ 数据字段值设置失败");
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 数据字段匹配验证失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 验证设置保存和加载功能
        /// </summary>
        [MenuItem("Blokus/验证/SettingsUI保存加载测试")]
        public static void TestSettingsSaveLoad()
        {
            Debug.Log("=== SettingsUI保存加载测试开始 ===");
            
            try
            {
                // 创建测试设置
                var originalSettings = new GameSettings();
                originalSettings.masterVolume = 0.8f;
                originalSettings.screenWidth = 1920;
                originalSettings.screenHeight = 1080;
                originalSettings.languageIndex = 1;
                originalSettings.enableAutoSave = false;
                
                // 转换为JSON
                string json = originalSettings.ToJson();
                Debug.Log($"设置JSON: {json}");
                
                // 从JSON恢复
                var restoredSettings = GameSettings.FromJson(json);
                
                // 验证数据一致性
                if (restoredSettings.masterVolume == originalSettings.masterVolume &&
                    restoredSettings.screenWidth == originalSettings.screenWidth &&
                    restoredSettings.screenHeight == originalSettings.screenHeight &&
                    restoredSettings.languageIndex == originalSettings.languageIndex &&
                    restoredSettings.enableAutoSave == originalSettings.enableAutoSave)
                {
                    Debug.Log("✅ 设置保存加载测试通过");
                }
                else
                {
                    Debug.LogError("❌ 设置保存加载测试失败：数据不一致");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 设置保存加载测试失败: {ex.Message}");
            }
            
            Debug.Log("=== SettingsUI保存加载测试结束 ===");
        }
    }
}