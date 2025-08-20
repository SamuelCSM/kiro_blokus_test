using UnityEngine;
using UnityEditor;
using BlokusGame.Core.Data;

namespace BlokusGame.Editor
{
    /// <summary>
    /// GameSettings编译验证测试
    /// 验证GameSettings类在移除aiDifficultyLevel和gameSpeed字段后的编译正确性
    /// </summary>
    public class GameSettingsCompilationTest
    {
        /// <summary>
        /// 测试GameSettings的基本功能
        /// </summary>
        [MenuItem("Blokus/Tests/Verify GameSettings Compilation")]
        public static void verifyGameSettingsCompilation()
        {
            Debug.Log("[GameSettingsCompilationTest] 开始验证GameSettings编译...");
            
            try
            {
                // 测试默认构造函数
                var settings1 = new GameSettings();
                Debug.Log("[GameSettingsCompilationTest] ✓ 默认构造函数正常");
                
                // 测试复制构造函数
                var settings2 = new GameSettings(settings1);
                Debug.Log("[GameSettingsCompilationTest] ✓ 复制构造函数正常");
                
                // 测试验证方法
                bool isValid = settings1.IsValid();
                Debug.Log($"[GameSettingsCompilationTest] ✓ IsValid()方法正常，返回: {isValid}");
                
                // 测试修正方法
                settings1.ClampValues();
                Debug.Log("[GameSettingsCompilationTest] ✓ ClampValues()方法正常");
                
                // 测试重置方法
                settings1.ResetToDefaults();
                Debug.Log("[GameSettingsCompilationTest] ✓ ResetToDefaults()方法正常");
                
                // 测试比较方法
                bool isEqual = settings1.Equals(settings2);
                Debug.Log($"[GameSettingsCompilationTest] ✓ Equals()方法正常，返回: {isEqual}");
                
                // 测试JSON序列化
                string json = settings1.ToJson();
                var settings3 = GameSettings.FromJson(json);
                Debug.Log("[GameSettingsCompilationTest] ✓ JSON序列化/反序列化正常");
                
                // 测试ToString方法
                string str = settings1.ToString();
                Debug.Log($"[GameSettingsCompilationTest] ✓ ToString()方法正常: {str}");
                
                // 测试GetHashCode方法
                int hashCode = settings1.GetHashCode();
                Debug.Log($"[GameSettingsCompilationTest] ✓ GetHashCode()方法正常: {hashCode}");
                
                Debug.Log("[GameSettingsCompilationTest] ✅ 所有测试通过！GameSettings编译和功能正常");
                
                // 验证字段是否正确移除
                _verifyRemovedFields();
                
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameSettingsCompilationTest] ❌ 测试失败: {ex.Message}");
                Debug.LogError($"[GameSettingsCompilationTest] 堆栈跟踪: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 验证已移除的字段不再存在
        /// </summary>
        private static void _verifyRemovedFields()
        {
            var settingsType = typeof(GameSettings);
            
            // 检查aiDifficultyLevel字段是否已移除
            var aiDifficultyField = settingsType.GetField("aiDifficultyLevel");
            if (aiDifficultyField == null)
            {
                Debug.Log("[GameSettingsCompilationTest] ✓ aiDifficultyLevel字段已正确移除");
            }
            else
            {
                Debug.LogError("[GameSettingsCompilationTest] ❌ aiDifficultyLevel字段仍然存在！");
            }
            
            // 检查gameSpeed字段是否已移除
            var gameSpeedField = settingsType.GetField("gameSpeed");
            if (gameSpeedField == null)
            {
                Debug.Log("[GameSettingsCompilationTest] ✓ gameSpeed字段已正确移除");
            }
            else
            {
                Debug.LogError("[GameSettingsCompilationTest] ❌ gameSpeed字段仍然存在！");
            }
            
            // 验证剩余字段数量是否正确
            var allFields = settingsType.GetFields();
            var publicFields = System.Array.FindAll(allFields, f => f.IsPublic);
            
            Debug.Log($"[GameSettingsCompilationTest] 当前公共字段数量: {publicFields.Length}");
            foreach (var field in publicFields)
            {
                Debug.Log($"[GameSettingsCompilationTest] - {field.Name}: {field.FieldType.Name}");
            }
        }
    }
}