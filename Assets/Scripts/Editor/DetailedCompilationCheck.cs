using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 详细编译检查工具 - 检查所有可能的编译问题
    /// </summary>
    public class DetailedCompilationCheck : EditorWindow
    {
        [MenuItem("Blokus/验证工具/详细编译检查")]
        public static void ShowWindow()
        {
            GetWindow<DetailedCompilationCheck>("详细编译检查");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("详细编译检查", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("执行详细编译检查"))
            {
                PerformDetailedCheck();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("检查方法参数匹配"))
            {
                CheckMethodParameters();
            }
            
            if (GUILayout.Button("检查类型定义"))
            {
                CheckTypeDefinitions();
            }
        }
        
        private void PerformDetailedCheck()
        {
            Debug.Log("=== 开始详细编译检查 ===");
            
            CheckTypeDefinitions();
            CheckMethodParameters();
            CheckEventDefinitions();
            
            Debug.Log("=== 详细编译检查完成 ===");
        }
        
        private void CheckTypeDefinitions()
        {
            Debug.Log("--- 检查类型定义 ---");
            
            try
            {
                // 检查GameResults
                var gameResultsType = typeof(BlokusGame.Core.Data.GameResults);
                Debug.Log("✅ GameResults 类型存在");
                
                // 检查TurnRecord
                var turnRecordType = typeof(BlokusGame.Core.Data.TurnRecord);
                Debug.Log("✅ TurnRecord 类型存在");
                
                // 检查placedPieceId字段类型
                var placedPieceIdField = turnRecordType.GetField("placedPieceId");
                if (placedPieceIdField != null)
                {
                    Debug.Log($"✅ TurnRecord.placedPieceId 字段类型: {placedPieceIdField.FieldType.Name}");
                }
                
                // 检查GameRecordManager
                var gameRecordManagerType = typeof(BlokusGame.Core.Managers.GameRecordManager);
                Debug.Log("✅ GameRecordManager 类型存在");
                
                // 检查BoardManager
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
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 类型定义检查失败: {e.Message}");
            }
        }
        
        private void CheckMethodParameters()
        {
            Debug.Log("--- 检查方法参数 ---");
            
            try
            {
                // 检查GameRecordManager.endTurnRecord
                var gameRecordManagerType = typeof(BlokusGame.Core.Managers.GameRecordManager);
                var endTurnRecordMethod = gameRecordManagerType.GetMethod("endTurnRecord");
                
                if (endTurnRecordMethod != null)
                {
                    var parameters = endTurnRecordMethod.GetParameters();
                    Debug.Log($"✅ endTurnRecord 参数: {string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))}");
                }
                
                // 检查GameManager方法
                var gameManagerType = typeof(BlokusGame.Core.Managers.GameManager);
                
                var startNewGameMethod = gameManagerType.GetMethod("StartNewGame");
                if (startNewGameMethod != null)
                {
                    Debug.Log("✅ GameManager.StartNewGame 方法存在");
                }
                
                var pauseGameMethod = gameManagerType.GetMethod("PauseGame");
                if (pauseGameMethod != null)
                {
                    Debug.Log("✅ GameManager.PauseGame 方法存在");
                }
                
                var skipCurrentTurnMethod = gameManagerType.GetMethod("SkipCurrentTurn");
                if (skipCurrentTurnMethod != null)
                {
                    Debug.Log("✅ GameManager.SkipCurrentTurn 方法存在");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 方法参数检查失败: {e.Message}");
            }
        }
        
        private void CheckEventDefinitions()
        {
            Debug.Log("--- 检查事件定义 ---");
            
            try
            {
                var gameEventsType = typeof(BlokusGame.Core.Events.GameEvents);
                
                // 检查静态事件
                var onPiecePlacedField = gameEventsType.GetField("onPiecePlaced", BindingFlags.Public | BindingFlags.Static);
                if (onPiecePlacedField != null)
                {
                    Debug.Log($"✅ GameEvents.onPiecePlaced 静态事件存在，类型: {onPiecePlacedField.FieldType}");
                }
                
                // 检查实例事件
                var instanceFields = gameEventsType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                Debug.Log($"✅ GameEvents 实例事件数量: {instanceFields.Length}");
                
                foreach (var field in instanceFields.Take(5)) // 只显示前5个
                {
                    Debug.Log($"  事件: {field.Name} - 类型: {field.FieldType.Name}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 事件定义检查失败: {e.Message}");
            }
        }
    }
}