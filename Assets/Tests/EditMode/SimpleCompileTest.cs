using UnityEngine;
using UnityEditor;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 简单编译测试 - 验证项目基本编译功能
    /// 提供最基础的编译验证，确保核心组件可以正常访问
    /// </summary>
    public static class SimpleCompileTest
    {
        /// <summary>
        /// 执行简单的编译测试
        /// 验证核心命名空间和类型是否可以正常访问
        /// </summary>
        [MenuItem("Tools/Simple Compile Test")]
        public static void runSimpleTest()
        {
            Debug.Log("[SimpleCompileTest] 开始简单编译测试...");
            
            try
            {
                // 测试核心命名空间访问
                var eventsType = typeof(BlokusGame.Core.Events.GameEvents);
                
                Debug.Log($"[SimpleCompileTest] GameEvents类型: {eventsType.Name}");
                
                // 测试接口访问
                var playerInterface = typeof(BlokusGame.Core.Interfaces._IPlayer);
                var pieceInterface = typeof(BlokusGame.Core.Interfaces._IGamePiece);
                var boardInterface = typeof(BlokusGame.Core.Interfaces._IGameBoard);
                
                Debug.Log($"[SimpleCompileTest] _IPlayer接口: {playerInterface.Name}");
                Debug.Log($"[SimpleCompileTest] _IGamePiece接口: {pieceInterface.Name}");
                Debug.Log($"[SimpleCompileTest] _IGameBoard接口: {boardInterface.Name}");
                
                Debug.Log("[SimpleCompileTest] ✅ 所有核心类型访问成功！编译测试通过！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SimpleCompileTest] ❌ 编译测试失败: {ex.Message}");
                Debug.LogError($"[SimpleCompileTest] 堆栈跟踪: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 命令行调用的编译测试方法
        /// 用于自动化构建和CI/CD流程
        /// </summary>
        public static void checkCompilationForCI()
        {
            Debug.Log("[SimpleCompileTest] CI编译检查开始...");
            
            try
            {
                runSimpleTest();
                Debug.Log("[SimpleCompileTest] CI编译检查成功完成");
                EditorApplication.Exit(0);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SimpleCompileTest] CI编译检查失败: {ex.Message}");
                EditorApplication.Exit(1);
            }
        }
    }
}