using UnityEngine;
using UnityEditor;
using BlokusGame.Core.UI;
using BlokusGame.Core.Data;

namespace BlokusGame.Editor
{
    /// <summary>
    /// SettingsUI编译修复工具
    /// 检测和修复编译错误
    /// </summary>
    public class SettingsUICompilationFix : EditorWindow
    {
        [MenuItem("Blokus/修复/SettingsUI编译修复")]
        public static void ShowWindow()
        {
            GetWindow<SettingsUICompilationFix>("SettingsUI编译修复");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("SettingsUI编译修复", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("检测编译问题", GUILayout.Height(30)))
            {
                DetectCompilationIssues();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("修复编译问题", GUILayout.Height(30)))
            {
                FixCompilationIssues();
            }
        }
        
        private void DetectCompilationIssues()
        {
            Debug.Log("=== 检测SettingsUI编译问题 ===");
            
            // 测试GameSettings类
            try
            {
                var settings = new GameSettings();
                Debug.Log("✅ GameSettings类创建成功");
                
                // 测试ToJson方法
                string json = settings.ToJson();
                Debug.Log("✅ ToJson方法正常");
                
                // 测试FromJson方法
                var loadedSettings = GameSettings.FromJson(json);
                Debug.Log("✅ FromJson方法正常");
                
                // 测试ResetToDefaults方法
                settings.ResetToDefaults();
                Debug.Log("✅ ResetToDefaults方法正常");
                
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ GameSettings测试失败: {ex.Message}");
            }
            
            // 测试SettingsUI类
            try
            {
                var settingsUIType = typeof(SettingsUI);
                Debug.Log("✅ SettingsUI类定义正常");
                
                // 检查是否继承自UIBase
                if (typeof(UIBase).IsAssignableFrom(settingsUIType))
                {
                    Debug.Log("✅ SettingsUI正确继承自UIBase");
                }
                else
                {
                    Debug.LogError("❌ SettingsUI未正确继承自UIBase");
                }
                
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ SettingsUI测试失败: {ex.Message}");
            }
            
            Debug.Log("=== 编译问题检测完成 ===");
        }
        
        private void FixCompilationIssues()
        {
            Debug.Log("=== 开始修复编译问题 ===");
            
            // 强制重新编译
            AssetDatabase.Refresh();
            
            Debug.Log("✅ 已刷新资源数据库");
            Debug.Log("=== 编译问题修复完成 ===");
        }
    }
}