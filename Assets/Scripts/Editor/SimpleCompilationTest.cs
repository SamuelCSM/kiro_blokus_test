using UnityEngine;
using UnityEditor;
using BlokusGame.Core.UI;
using BlokusGame.Core.Data;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 简单编译测试
    /// </summary>
    public class SimpleCompilationTest
    {
        [MenuItem("Blokus/测试/简单编译测试")]
        public static void RunTest()
        {
            Debug.Log("=== 开始简单编译测试 ===");
            
            try
            {
                // 测试GameSettings
                var settings = new GameSettings();
                Debug.Log("✅ GameSettings创建成功");
                
                // 测试基本属性
                settings.masterVolume = 0.5f;
                settings.qualityLevel = 1;
                settings.languageIndex = 0;
                Debug.Log("✅ GameSettings属性设置成功");
                
                // 测试方法
                settings.ResetToDefaults();
                Debug.Log("✅ ResetToDefaults方法调用成功");
                
                string json = settings.ToJson();
                Debug.Log("✅ ToJson方法调用成功");
                
                var newSettings = GameSettings.FromJson(json);
                Debug.Log("✅ FromJson方法调用成功");
                
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 编译测试失败: {ex.Message}");
                Debug.LogError($"堆栈跟踪: {ex.StackTrace}");
            }
            
            Debug.Log("=== 简单编译测试完成 ===");
        }
    }
}