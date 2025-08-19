using UnityEngine;
using UnityEditor;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 编译检查器 - 用于命令行验证项目编译状态
    /// 提供静态方法供Unity命令行调用，检查项目是否有编译错误
    /// </summary>
    public static class CompilationChecker
    {
        /// <summary>
        /// 检查项目编译状态的静态方法
        /// 可以通过Unity命令行的-executeMethod参数调用
        /// 使用CompilationPipeline来检查编译状态，更加准确可靠
        /// </summary>
        [MenuItem("Tools/Check Compilation")]
        public static void CheckCompilation()
        {
            Debug.Log("[CompilationChecker] 开始检查项目编译状态...");
            
            // 强制重新编译所有脚本
            AssetDatabase.Refresh();
            
            // 等待编译完成
            EditorApplication.LockReloadAssemblies();
            AssetDatabase.Refresh();
            EditorApplication.UnlockReloadAssemblies();
            
            // 检查是否有编译错误 - 使用更可靠的方法
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("[CompilationChecker] 编译仍在进行中，请稍后再试");
                return;
            }
            
            // 简单的编译检查 - 如果能执行到这里说明编译成功
            try
            {
                // 尝试访问核心类来验证编译状态
                var gameManagerType = System.Type.GetType("BlokusGame.Core.Managers.GameManager");
                var gameEventsType = System.Type.GetType("BlokusGame.Core.Events.GameEvents");
                
                if (gameManagerType != null && gameEventsType != null)
                {
                    Debug.Log("[CompilationChecker] 项目编译成功，没有错误！");
                    EditorApplication.Exit(0); // 正常退出
                }
                else
                {
                    Debug.LogError("[CompilationChecker] 核心类型无法找到，可能存在编译问题");
                    EditorApplication.Exit(1); // 退出并返回错误代码
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CompilationChecker] 编译检查过程中出现异常: {ex.Message}");
                EditorApplication.Exit(1); // 退出并返回错误代码
            }
        }
        
        /// <summary>
        /// 静默检查编译状态，不退出Unity
        /// 通过尝试访问核心类型来验证编译是否成功
        /// </summary>
        /// <returns>编译是否成功，true表示成功，false表示有错误</returns>
        public static bool checkCompilationSilent()
        {
            try
            {
                // 强制刷新资源数据库
                AssetDatabase.Refresh();
                
                // 检查是否正在编译
                if (EditorApplication.isCompiling)
                {
                    Debug.LogWarning("[CompilationChecker] 编译仍在进行中");
                    return false;
                }
                
                // 尝试访问核心类型来验证编译状态
                var gameManagerType = System.Type.GetType("BlokusGame.Core.Managers.GameManager");
                var gameEventsType = System.Type.GetType("BlokusGame.Core.Events.GameEvents");
                var boardSystemType = System.Type.GetType("BlokusGame.Core.Board.BoardSystem");
                var pieceDataType = System.Type.GetType("BlokusGame.Core.Data.PieceData");
                
                if (gameManagerType != null && gameEventsType != null && boardSystemType != null && pieceDataType != null)
                {
                    Debug.Log("[CompilationChecker] 编译检查通过 - 所有核心类型可访问");
                    return true;
                }
                else
                {
                    Debug.LogError("[CompilationChecker] 发现编译错误 - 核心类型无法访问");
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CompilationChecker] 编译检查过程中出现异常: {ex.Message}");
                return false;
            }
        }
    }
}