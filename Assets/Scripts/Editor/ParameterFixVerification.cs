using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 参数修复验证工具
    /// </summary>
    public class ParameterFixVerification : EditorWindow
    {
        [MenuItem("Blokus/验证工具/参数修复验证")]
        public static void ShowWindow()
        {
            GetWindow<ParameterFixVerification>("参数修复验证");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("参数修复验证", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("验证参数修复"))
            {
                VerifyParameterFixes();
            }
        }
        
        private void VerifyParameterFixes()
        {
            Debug.Log("=== 开始验证参数修复 ===");
            
            try
            {
                // 验证GameRecordManager.endTurnRecord方法
                var gameRecordManagerType = typeof(BlokusGame.Core.Managers.GameRecordManager);
                var endTurnRecordMethod = gameRecordManagerType.GetMethod("endTurnRecord");
                
                if (endTurnRecordMethod != null)
                {
                    var parameters = endTurnRecordMethod.GetParameters();
                    Debug.Log($"✅ endTurnRecord 方法存在，参数数量: {parameters.Length}");
                    
                    foreach (var param in parameters)
                    {
                        Debug.Log($"  参数: {param.Name} - 类型: {param.ParameterType.Name}");
                    }
                    
                    // 检查第二个参数是否为string类型
                    if (parameters.Length >= 2 && parameters[1].ParameterType == typeof(string))
                    {
                        Debug.Log("✅ placedPieceId 参数类型正确 (string)");
                    }
                    else
                    {
                        Debug.LogError("❌ placedPieceId 参数类型不正确");
                    }
                }
                else
                {
                    Debug.LogError("❌ endTurnRecord 方法未找到");
                }
                
                // 验证TurnRecord.placedPieceId字段
                var turnRecordType = typeof(BlokusGame.Core.Data.TurnRecord);
                var placedPieceIdField = turnRecordType.GetField("placedPieceId");
                
                if (placedPieceIdField != null)
                {
                    Debug.Log($"✅ TurnRecord.placedPieceId 字段存在，类型: {placedPieceIdField.FieldType.Name}");
                    
                    if (placedPieceIdField.FieldType == typeof(string))
                    {
                        Debug.Log("✅ TurnRecord.placedPieceId 字段类型正确 (string)");
                    }
                    else
                    {
                        Debug.LogError($"❌ TurnRecord.placedPieceId 字段类型不正确: {placedPieceIdField.FieldType.Name}");
                    }
                }
                else
                {
                    Debug.LogError("❌ TurnRecord.placedPieceId 字段未找到");
                }
                
                // 验证BoardManager.instance
                var boardManagerType = typeof(BlokusGame.Core.Managers.BoardManager);
                var instanceProperty = boardManagerType.GetProperty("instance");
                
                if (instanceProperty != null)
                {
                    Debug.Log("✅ BoardManager.instance 属性存在");
                }
                else
                {
                    Debug.LogError("❌ BoardManager.instance 属性缺失");
                }
                
                Debug.Log("🎉 参数修复验证完成！");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 验证过程中出现错误: {e.Message}");
            }
            
            Debug.Log("=== 参数修复验证结束 ===");
        }
    }
}