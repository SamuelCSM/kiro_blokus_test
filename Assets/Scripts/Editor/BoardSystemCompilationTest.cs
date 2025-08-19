using UnityEngine;
using UnityEditor;
using BlokusGame.Core.Board;
using BlokusGame.Core.Interfaces;

namespace BlokusGame.Editor
{
    /// <summary>
    /// BoardSystem编译测试 - 验证BoardSystem类是否正确编译
    /// </summary>
    public static class BoardSystemCompilationTest
    {
        [MenuItem("Tools/Test BoardSystem Compilation")]
        public static void TestBoardSystemCompilation()
        {
            Debug.Log("[BoardSystemCompilationTest] 开始测试BoardSystem编译...");
            
            try
            {
                // 尝试创建BoardSystem实例
                BoardSystem boardSystem = new BoardSystem();
                boardSystem.initializeBoard();
                
                Debug.Log($"[BoardSystemCompilationTest] BoardSystem创建成功，棋盘尺寸: {boardSystem.boardSize}");
                
                // 测试基本功能
                bool isValidPos = boardSystem.isPositionValid(new Vector2Int(10, 10));
                Debug.Log($"[BoardSystemCompilationTest] 位置验证测试: {isValidPos}");
                
                Vector2Int corner = boardSystem.getStartingCorner(1);
                Debug.Log($"[BoardSystemCompilationTest] 起始角落测试: {corner}");
                
                Debug.Log("[BoardSystemCompilationTest] BoardSystem编译和基本功能测试通过！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BoardSystemCompilationTest] BoardSystem测试失败: {ex.Message}");
                Debug.LogError($"[BoardSystemCompilationTest] 堆栈跟踪: {ex.StackTrace}");
            }
        }
    }
}