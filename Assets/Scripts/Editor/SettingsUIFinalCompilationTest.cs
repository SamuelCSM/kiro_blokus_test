using UnityEngine;
using UnityEditor;
using System.Reflection;
using BlokusGame.Core.UI;
using BlokusGame.Core.Data;

namespace BlokusGame.Editor
{
    /// <summary>
    /// SettingsUI最终编译测试
    /// 验证SettingsUI类的完整性和正确性
    /// </summary>
    public class SettingsUIFinalCompilationTest : EditorWindow
    {
        [MenuItem("Blokus/验证/SettingsUI最终编译测试")]
        public static void ShowWindow()
        {
            GetWindow<SettingsUIFinalCompilationTest>("SettingsUI最终编译测试");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("SettingsUI最终编译测试", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("执行验证", GUILayout.Height(30)))
            {
                PerformVerification();
            }
        }
        
        private void PerformVerification()
        {
            Debug.Log("=== SettingsUI最终编译测试开始 ===");
            
            bool allPassed = true;
            
            // 1. 验证SettingsUI类存在
            allPassed &= VerifySettingsUIClass();
            
            // 2. 验证GameSettings类存在
            allPassed &= VerifyGameSettingsClass();
            
            // 3. 验证方法完整性
            allPassed &= VerifyMethods();
            
            // 4. 验证字段完整性
            allPassed &= VerifyFields();
            
            string result = allPassed ? "✅ 所有验证通过" : "❌ 部分验证失败";
            Debug.Log($"=== SettingsUI最终编译测试完成: {result} ===");
            
            if (allPassed)
            {
                EditorUtility.DisplayDialog("验证完成", "SettingsUI编译验证全部通过！", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("验证完成", "SettingsUI编译验证发现问题，请查看Console日志", "确定");
            }
        }
        
        private bool VerifySettingsUIClass()
        {
            Debug.Log("--- 验证SettingsUI类 ---");
            
            try
            {
                var settingsUIType = typeof(SettingsUI);
                
                // 检查是否继承自UIBase
                if (!typeof(UIBase).IsAssignableFrom(settingsUIType))
                {
                    Debug.LogError("SettingsUI未继承自UIBase");
                    return false;
                }
                
                Debug.Log("✅ SettingsUI类验证通过");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"SettingsUI类验证失败: {ex.Message}");
                return false;
            }
        }
        
        private bool VerifyGameSettingsClass()
        {
            Debug.Log("--- 验证GameSettings类 ---");
            
            try
            {
                var gameSettingsType = typeof(GameSettings);
                var gameSettings = new GameSettings();
                
                // 检查序列化
                string json = gameSettings.ToJson();
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError("GameSettings序列化失败");
                    return false;
                }
                
                Debug.Log("✅ GameSettings类验证通过");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"GameSettings类验证失败: {ex.Message}");
                return false;
            }
        }
        
        private bool VerifyMethods()
        {
            Debug.Log("--- 验证方法完整性 ---");
            
            try
            {
                var settingsUIType = typeof(SettingsUI);
                
                // 检查UIBase实现方法
                var requiredMethods = new string[]
                {
                    "InitializeUIContent",
                    "OnUIShown",
                    "OnUIHidden"
                };
                
                foreach (var methodName in requiredMethods)
                {
                    var method = settingsUIType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public);
                    if (method == null)
                    {
                        Debug.LogError($"SettingsUI缺少方法: {methodName}");
                        return false;
                    }
                }
                
                Debug.Log("✅ 方法完整性验证通过");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"方法验证失败: {ex.Message}");
                return false;
            }
        }
        
        private bool VerifyFields()
        {
            Debug.Log("--- 验证字段完整性 ---");
            
            try
            {
                var settingsUIType = typeof(SettingsUI);
                
                // 检查关键字段
                var requiredFields = new string[]
                {
                    "_m_masterVolumeSlider",
                    "_m_qualityDropdown",
                    "_m_languageDropdown",
                    "_m_touchSensitivitySlider"
                };
                
                foreach (var fieldName in requiredFields)
                {
                    var field = settingsUIType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field == null)
                    {
                        Debug.LogError($"SettingsUI缺少字段: {fieldName}");
                        return false;
                    }
                }
                
                Debug.Log("✅ 字段完整性验证通过");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"字段验证失败: {ex.Message}");
                return false;
            }
        }
    }
}