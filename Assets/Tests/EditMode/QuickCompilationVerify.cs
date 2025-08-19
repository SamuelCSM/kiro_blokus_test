using UnityEngine;
using UnityEditor;
using BlokusGame.Core.Managers;
using BlokusGame.Core.Data;
using BlokusGame.Core.Events;

namespace BlokusGame.Editor.Tests
{
    /// <summary>
    /// 快速编译验证 - 验证GameManager修复后的编译状态
    /// </summary>
    public class QuickCompilationVerify
    {
        /// <summary>
        /// 验证GameManager类的基本功能
        /// </summary>
        [MenuItem("Tools/Verify GameManager Compilation")]
        public static void verifyGameManagerCompilation()
        {
            Debug.Log("[QuickCompilationVerify] 开始验证GameManager编译状态...");
            
            try
            {
                // 尝试创建GameManager实例
                var gameObject = new GameObject("TestGameManager");
                var gameManager = gameObject.AddComponent<GameManager>();
                
                // 验证基本属性访问
                var currentState = gameManager.currentGameState;
                var isActive = gameManager.isGameActive;
                var isPaused = gameManager.isPaused;
                
                Debug.Log($"[QuickCompilationVerify] GameManager基本属性验证成功");
                Debug.Log($"  - 当前状态: {currentState}");
                Debug.Log($"  - 游戏活跃: {isActive}");
                Debug.Log($"  - 游戏暂停: {isPaused}");
                
                // 清理测试对象
                Object.DestroyImmediate(gameObject);
                
                Debug.Log("[QuickCompilationVerify] ✅ GameManager编译验证通过！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[QuickCompilationVerify] ❌ GameManager编译验证失败: {ex.Message}");
                Debug.LogError($"[QuickCompilationVerify] 堆栈跟踪: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 验证GameManager的方法调用
        /// </summary>
        [MenuItem("Tools/Verify GameManager Methods")]
        public static void verifyGameManagerMethods()
        {
            Debug.Log("[QuickCompilationVerify] 开始验证GameManager方法调用...");
            
            try
            {
                var gameObject = new GameObject("TestGameManager");
                var gameManager = gameObject.AddComponent<GameManager>();
                
                // 验证公共方法存在性
                bool canAdvance = gameManager.canAdvanceTurn();
                Debug.Log($"[QuickCompilationVerify] canAdvanceTurn() 返回: {canAdvance}");
                
                // 验证游戏进度获取
                float progress = gameManager.getGameProgress();
                Debug.Log($"[QuickCompilationVerify] getGameProgress() 返回: {progress}");
                
                // 清理测试对象
                Object.DestroyImmediate(gameObject);
                
                Debug.Log("[QuickCompilationVerify] ✅ GameManager方法验证通过！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[QuickCompilationVerify] ❌ GameManager方法验证失败: {ex.Message}");
            }
        }
    }
}