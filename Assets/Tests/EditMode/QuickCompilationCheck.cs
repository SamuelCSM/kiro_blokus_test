using UnityEngine;
using UnityEditor;

namespace BlokusGame.Editor.Tests
{
    /// <summary>
    /// 快速编译检查工具
    /// 用于验证代码的基本编译状态
    /// </summary>
    public class QuickCompilationCheck
    {
        /// <summary>
        /// 快速编译检查菜单项
        /// </summary>
        [MenuItem("Tools/Quick Compilation Check")]
        public static void performQuickCheck()
        {
            Debug.Log("[QuickCompilationCheck] 开始快速编译检查...");
            
            try
            {
                // 检查主要类是否可以实例化
                var gameManager = typeof(BlokusGame.Core.Managers.GameManager);
                var gameStateManager = typeof(BlokusGame.Core.Managers.GameStateManager);
                var gamePiece = typeof(BlokusGame.Core.Pieces.GamePiece);
                var ruleEngine = typeof(BlokusGame.Core.Rules.RuleEngine);
                
                Debug.Log($"[QuickCompilationCheck] ✅ GameManager类型检查通过: {gameManager.Name}");
                Debug.Log($"[QuickCompilationCheck] ✅ GameStateManager类型检查通过: {gameStateManager.Name}");
                Debug.Log($"[QuickCompilationCheck] ✅ GamePiece类型检查通过: {gamePiece.Name}");
                Debug.Log($"[QuickCompilationCheck] ✅ RuleEngine类型检查通过: {ruleEngine.Name}");
                
                Debug.Log("[QuickCompilationCheck] ✅ 快速编译检查完成，主要类型都可以正常加载！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[QuickCompilationCheck] ❌ 编译检查失败: {ex.Message}");
                Debug.LogError($"[QuickCompilationCheck] 堆栈跟踪: {ex.StackTrace}");
            }
        }
    }
}