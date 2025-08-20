using UnityEngine;
using UnityEditor;
using BlokusGame.Core.UI;
using BlokusGame.Core.Data;

namespace BlokusGame.Editor
{
    /// <summary>
    /// SettingsUI编译验证测试
    /// 验证SettingsUI类的编译正确性和基本功能
    /// </summary>
    public class SettingsUICompilationTest
    {
        /// <summary>
        /// 验证SettingsUI编译
        /// </summary>
        [MenuItem("Blokus/Test/Verify SettingsUI Compilation")]
        public static void VerifySettingsUICompilation()
        {
            Debug.Log("[SettingsUICompilationTest] 开始验证SettingsUI编译...");
            
            try
            {
                // 测试SettingsUI类的基本实例化
                var gameObject = new GameObject("TestSettingsUI");
                var settingsUI = gameObject.AddComponent<SettingsUI>();
                
                if (settingsUI != null)
                {
                    Debug.Log("[SettingsUICompilationTest] ✅ SettingsUI组件创建成功");
                }
                
                // 清理测试对象
                Object.DestroyImmediate(gameObject);
                
                Debug.Log("[SettingsUICompilationTest] ✅ 所有编译验证通过！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SettingsUICompilationTest] ❌ 编译验证失败: {ex.Message}");
                Debug.LogError($"[SettingsUICompilationTest] 堆栈跟踪: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 验证GameSettings字段完整性
        /// </summary>
        [MenuItem("Blokus/Test/Verify GameSettings Fields")]
        public static void VerifyGameSettingsFields()
        {
            Debug.Log("[SettingsUICompilationTest] 开始验证GameSettings字段...");
            
            try
            {
                var settings = new GameSettings();
                
                // 验证所有字段都可以访问
                Debug.Log($"主音量: {settings.masterVolume}");
                Debug.Log($"音效音量: {settings.sfxVolume}");
                Debug.Log($"音乐音量: {settings.musicVolume}");
                Debug.Log($"静音: {settings.isMuted}");
                Debug.Log($"画质等级: {settings.qualityLevel}");
                Debug.Log($"屏幕宽度: {settings.screenWidth}");
                Debug.Log($"屏幕高度: {settings.screenHeight}");
                Debug.Log($"全屏: {settings.isFullscreen}");
                Debug.Log($"垂直同步: {settings.enableVSync}");
                Debug.Log($"目标帧率: {settings.targetFrameRate}");
                Debug.Log($"语言索引: {settings.languageIndex}");
                Debug.Log($"自动保存: {settings.enableAutoSave}");
                Debug.Log($"显示提示: {settings.showHints}");
                Debug.Log($"触觉反馈: {settings.enableHapticFeedback}");
                Debug.Log($"动画速度: {settings.animationSpeed}");
                Debug.Log($"触摸灵敏度: {settings.touchSensitivity}");
                Debug.Log($"双击间隔: {settings.doubleTapInterval}");
                
                Debug.Log("[SettingsUICompilationTest] ✅ 所有GameSettings字段验证通过！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SettingsUICompilationTest] ❌ GameSettings字段验证失败: {ex.Message}");
            }
        }
    }
}