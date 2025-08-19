using UnityEngine;
using UnityEditor;
using BlokusGame.Core.Events;
using BlokusGame.Core.Interfaces;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 编译修复测试 - 验证事件系统的编译问题是否已修复
    /// </summary>
    public static class CompilationFixTest
    {
        [MenuItem("Tools/Test Event System Compilation")]
        public static void TestEventSystemCompilation()
        {
            Debug.Log("[CompilationFixTest] 开始测试事件系统编译...");
            
            try
            {
                // 测试静态事件访问
                System.Action<int, _IGamePiece, Vector2Int> testAction = (playerId, piece, position) =>
                {
                    Debug.Log($"测试事件：玩家 {playerId} 放置方块");
                };
                
                // 测试事件订阅和取消订阅
                GameEvents.onPiecePlaced += testAction;
                GameEvents.onPiecePlaced -= testAction;
                
                // 测试其他静态事件
                System.Action testBoardAction = () => Debug.Log("棋盘事件测试");
                GameEvents.onBoardInitialized += testBoardAction;
                GameEvents.onBoardInitialized -= testBoardAction;
                
                GameEvents.onBoardCleared += testBoardAction;
                GameEvents.onBoardCleared -= testBoardAction;
                
                Debug.Log("[CompilationFixTest] ✓ 事件系统编译测试通过！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CompilationFixTest] ✗ 事件系统编译测试失败: {ex.Message}");
                Debug.LogError($"[CompilationFixTest] 堆栈跟踪: {ex.StackTrace}");
            }
        }
    }
}