using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace BlokusGame.Editor
{
    /// <summary>
    /// onGameEnded 事件验证工具
    /// 验证事件定义、订阅和调用的正确性
    /// </summary>
    public class GameEndedEventVerification : EditorWindow
    {
        [MenuItem("Blokus/验证工具/onGameEnded事件验证")]
        public static void ShowWindow()
        {
            GetWindow<GameEndedEventVerification>("onGameEnded事件验证");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("onGameEnded 事件验证", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("验证事件定义"))
            {
                VerifyEventDefinition();
            }
            
            if (GUILayout.Button("验证事件订阅"))
            {
                VerifyEventSubscriptions();
            }
            
            if (GUILayout.Button("验证事件调用"))
            {
                VerifyEventInvocations();
            }
            
            if (GUILayout.Button("完整验证"))
            {
                VerifyEventDefinition();
                VerifyEventSubscriptions();
                VerifyEventInvocations();
            }
        }
        
        private void VerifyEventDefinition()
        {
            Debug.Log("=== 验证 onGameEnded 事件定义 ===");
            
            try
            {
                var gameEventsType = typeof(BlokusGame.Core.Events.GameEvents);
                var onGameEndedField = gameEventsType.GetField("onGameEnded", BindingFlags.Public | BindingFlags.Static);
                
                if (onGameEndedField != null)
                {
                    Debug.Log($"✅ onGameEnded 事件定义正确");
                    Debug.Log($"   类型: {onGameEndedField.FieldType}");
                    Debug.Log($"   是否静态: {onGameEndedField.IsStatic}");
                    Debug.Log($"   是否公共: {onGameEndedField.IsPublic}");
                    
                    // 检查参数类型
                    if (onGameEndedField.FieldType.IsGenericType)
                    {
                        var genericArgs = onGameEndedField.FieldType.GetGenericArguments();
                        Debug.Log($"   参数类型: {string.Join(", ", genericArgs.Select(t => t.Name))}");                  
                        // 验证参数类型是否为 Dictionary<int, int>
                        if (genericArgs.Length == 1 && 
                            genericArgs[0].IsGenericType && 
                            genericArgs[0].GetGenericTypeDefinition() == typeof(Dictionary<,>))
                        {
                            var dictArgs = genericArgs[0].GetGenericArguments();
                            if (dictArgs.Length == 2 && dictArgs[0] == typeof(int) && dictArgs[1] == typeof(int))
                            {
                                Debug.Log("✅ 参数类型 Dictionary<int, int> 正确");
                            }
                            else
                            {
                                Debug.LogError($"❌ 参数类型错误，期望 Dictionary<int, int>，实际 Dictionary<{dictArgs[0].Name}, {dictArgs[1].Name}>");
                            }
                        }
                        else
                        {
                            Debug.LogError($"❌ 参数类型错误，期望 Dictionary<int, int>，实际 {genericArgs[0].Name}");
                        }
                    }
                }
                else
                {
                    Debug.LogError("❌ onGameEnded 事件未找到");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 事件定义验证失败: {e.Message}");
            }
        }
        
        private void VerifyEventSubscriptions()
        {
            Debug.Log("=== 验证 onGameEnded 事件订阅 ===");
            
            try
            {
                // 验证 AudioManager
                VerifyClassSubscription(typeof(BlokusGame.Core.Audio.AudioManager), "AudioManager");
                
                // 验证 GameRecordManager
                VerifyClassSubscription(typeof(BlokusGame.Core.Managers.GameRecordManager), "GameRecordManager");
                
                // 验证 ScoreSystem
                VerifyClassSubscription(typeof(BlokusGame.Core.Scoring.ScoreSystem), "ScoreSystem");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 事件订阅验证失败: {e.Message}");
            }
        }
        
        private void VerifyClassSubscription(System.Type classType, string className)
        {
            // 检查 _onGameEnded 方法
            var onGameEndedMethod = classType.GetMethod("_onGameEnded", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (onGameEndedMethod != null)
            {
                var parameters = onGameEndedMethod.GetParameters();
                Debug.Log($"✅ {className}._onGameEnded 方法存在");
                Debug.Log($"   参数数量: {parameters.Length}");
                
                if (parameters.Length == 1)
                {
                    var paramType = parameters[0].ParameterType;
                    Debug.Log($"   参数类型: {paramType}");
                    
                    if (paramType.IsGenericType && 
                        paramType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        var dictArgs = paramType.GetGenericArguments();
                        if (dictArgs.Length == 2 && dictArgs[0] == typeof(int) && dictArgs[1] == typeof(int))
                        {
                            Debug.Log($"✅ {className} 参数类型正确: Dictionary<int, int>");
                        }
                        else
                        {
                            Debug.LogError($"❌ {className} 参数类型错误: Dictionary<{dictArgs[0].Name}, {dictArgs[1].Name}>");
                        }
                    }
                    else
                    {
                        Debug.LogError($"❌ {className} 参数类型错误: {paramType.Name}");
                    }
                }
                else
                {
                    Debug.LogError($"❌ {className} 参数数量错误: {parameters.Length}");
                }
            }
            else
            {
                Debug.LogError($"❌ {className}._onGameEnded 方法未找到");
            }
        }
        
        private void VerifyEventInvocations()
        {
            Debug.Log("=== 验证 onGameEnded 事件调用 ===");
            
            try
            {
                // 验证 GameManager
                VerifyClassInvocation(typeof(BlokusGame.Core.Managers.GameManager), "GameManager");
                
                // 验证 GameStateManager
                VerifyClassInvocation(typeof(BlokusGame.Core.Managers.GameStateManager), "GameStateManager");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 事件调用验证失败: {e.Message}");
            }
        }
        
        private void VerifyClassInvocation(System.Type classType, string className)
        {
            Debug.Log($"检查 {className} 中的 onGameEnded 调用...");
            
            // 这里只能检查类是否存在，具体的调用需要通过代码分析
            if (classType != null)
            {
                Debug.Log($"✅ {className} 类存在，需要手动检查 onGameEnded 调用");
            }
            else
            {
                Debug.LogError($"❌ {className} 类未找到");
            }
        }
    }
}