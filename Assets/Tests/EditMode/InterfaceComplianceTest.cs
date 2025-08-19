using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using BlokusGame.Core.Managers;
using BlokusGame.Core.Interfaces;

namespace BlokusGame.Editor.Tests
{
    /// <summary>
    /// 接口合规性测试
    /// 验证GameManager是否正确实现了_IGameStateManager接口
    /// </summary>
    public class InterfaceComplianceTest
    {
        /// <summary>
        /// 测试GameManager接口实现
        /// </summary>
        [MenuItem("Tools/Test Interface Compliance")]
        public static void testInterfaceCompliance()
        {
            Debug.Log("[InterfaceComplianceTest] 开始接口合规性测试...");
            
            try
            {
                // 创建测试对象
                GameObject testObj = new GameObject("TestGameManager");
                GameManager gameManager = testObj.AddComponent<GameManager>();
                
                // 验证接口实现
                _IGameStateManager stateManager = gameManager;
                
                Debug.Log($"[InterfaceComplianceTest] ✅ GameManager正确实现了_IGameStateManager接口");
                Debug.Log($"[InterfaceComplianceTest] ✅ currentState属性: {stateManager.currentState}");
                Debug.Log($"[InterfaceComplianceTest] ✅ currentPlayerId属性: {stateManager.currentPlayerId}");
                Debug.Log($"[InterfaceComplianceTest] ✅ turnNumber属性: {stateManager.turnNumber}");
                
                // 测试方法调用
                bool canAdvance = stateManager.canAdvanceTurn();
                float progress = stateManager.getGameProgress();
                
                Debug.Log($"[InterfaceComplianceTest] ✅ canAdvanceTurn方法: {canAdvance}");
                Debug.Log($"[InterfaceComplianceTest] ✅ getGameProgress方法: {progress}");
                
                // 清理测试对象
                Object.DestroyImmediate(testObj);
                
                Debug.Log("[InterfaceComplianceTest] ✅ 所有接口合规性测试通过！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[InterfaceComplianceTest] ❌ 接口合规性测试失败: {ex.Message}");
                Debug.LogError($"[InterfaceComplianceTest] 堆栈跟踪: {ex.StackTrace}");
            }
        }
    }
}