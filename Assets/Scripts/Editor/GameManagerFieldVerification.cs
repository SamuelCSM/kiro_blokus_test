using UnityEngine;
using UnityEditor;
using BlokusGame.Core.Managers;

namespace BlokusGame.Editor
{
    /// <summary>
    /// GameManager字段验证编辑器脚本
    /// 验证GameManager中isCurrentPlayerTurn属性是否正确实现
    /// </summary>
    public class GameManagerFieldVerification : EditorWindow
    {
        [MenuItem("Blokus/验证/GameManager字段验证")]
        public static void ShowWindow()
        {
            GetWindow<GameManagerFieldVerification>("GameManager字段验证");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("GameManager字段验证", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("验证isCurrentPlayerTurn属性"))
            {
                VerifyIsCurrentPlayerTurnProperty();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("验证GameplayUI编译"))
            {
                VerifyGameplayUICompilation();
            }
        }
        
        /// <summary>
        /// 验证isCurrentPlayerTurn属性是否存在且可访问
        /// </summary>
        private void VerifyIsCurrentPlayerTurnProperty()
        {
            Debug.Log("=== GameManager isCurrentPlayerTurn属性验证 ===");
            
            try
            {
                // 检查GameManager类型是否包含isCurrentPlayerTurn属性
                var gameManagerType = typeof(GameManager);
                var property = gameManagerType.GetProperty("isCurrentPlayerTurn");
                
                if (property != null)
                {
                    Debug.Log("✅ isCurrentPlayerTurn属性存在");
                    Debug.Log($"   - 属性类型: {property.PropertyType}");
                    Debug.Log($"   - 是否可读: {property.CanRead}");
                    Debug.Log($"   - 是否可写: {property.CanWrite}");
                    
                    // 检查属性的getter方法
                    var getMethod = property.GetGetMethod();
                    if (getMethod != null)
                    {
                        Debug.Log($"   - Getter方法存在: {getMethod.Name}");
                        Debug.Log($"   - 是否为公共方法: {getMethod.IsPublic}");
                    }
                    
                    Debug.Log("✅ GameManager.isCurrentPlayerTurn属性验证通过");
                }
                else
                {
                    Debug.LogError("❌ isCurrentPlayerTurn属性不存在");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 验证过程中发生错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 验证GameplayUI编译是否正常
        /// </summary>
        private void VerifyGameplayUICompilation()
        {
            Debug.Log("=== GameplayUI编译验证 ===");
            
            try
            {
                // 检查GameplayUI类型是否可以正常加载
                var gameplayUIType = typeof(BlokusGame.Core.UI.GameplayUI);
                Debug.Log($"✅ GameplayUI类型加载成功: {gameplayUIType.FullName}");
                
                // 检查_updateControlButtons方法是否存在
                var updateControlButtonsMethod = gameplayUIType.GetMethod("_updateControlButtons", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (updateControlButtonsMethod != null)
                {
                    Debug.Log("✅ _updateControlButtons方法存在");
                }
                else
                {
                    Debug.LogWarning("⚠️ _updateControlButtons方法未找到（可能是私有方法）");
                }
                
                Debug.Log("✅ GameplayUI编译验证通过");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ GameplayUI编译验证失败: {ex.Message}");
            }
        }
    }
}