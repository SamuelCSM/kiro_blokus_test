using UnityEngine;
using UnityEditor;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 编译错误修复验证工具
    /// </summary>
    public class CompilationErrorFix : EditorWindow
    {
        [MenuItem("Blokus/验证工具/编译错误修复验证")]
        public static void ShowWindow()
        {
            GetWindow<CompilationErrorFix>("编译错误修复验证");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("编译错误修复验证", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("验证修复结果"))
            {
                VerifyFixes();
            }
        }
        
        private void VerifyFixes()
        {
            Debug.Log("=== 开始验证编译错误修复 ===");
            
            try
            {
                // 验证BoardManager.instance
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
                
                // 验证GameResults类
                var gameResultsType = typeof(BlokusGame.Core.Data.GameResults);
                Debug.Log("✅ GameResults 类存在");
                
                // 验证ScoreSystem方法
                var scoreSystemType = typeof(BlokusGame.Core.Scoring.ScoreSystem);
                var playerScoresMethod = scoreSystemType.GetMethod("playerScores");
                var playerRankingsMethod = scoreSystemType.GetMethod("playerRankings");
                
                if (playerScoresMethod != null && playerRankingsMethod != null)
                {
                    Debug.Log("✅ ScoreSystem playerScores 和 playerRankings 方法存在");
                }
                else
                {
                    Debug.LogError("❌ ScoreSystem 方法缺失");
                }
                
                // 验证GameManager方法
                var gameManagerType = typeof(BlokusGame.Core.Managers.GameManager);
                var startNewGameMethod = gameManagerType.GetMethod("StartNewGame");
                var pauseGameMethod = gameManagerType.GetMethod("PauseGame");
                var skipCurrentTurnMethod = gameManagerType.GetMethod("SkipCurrentTurn");
                
                if (startNewGameMethod != null && pauseGameMethod != null && skipCurrentTurnMethod != null)
                {
                    Debug.Log("✅ GameManager 兼容性方法存在");
                }
                else
                {
                    Debug.LogError("❌ GameManager 兼容性方法缺失");
                }
                
                Debug.Log("🎉 编译错误修复验证完成！");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 验证过程中出现错误: {e.Message}");
            }
            
            Debug.Log("=== 编译错误修复验证结束 ===");
        }
    }
}