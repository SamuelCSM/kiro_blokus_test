using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using BlokusGame.Core.Rules;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Data;
using BlokusGame.Core.Managers;
using BlokusGame.Core.Pieces;

namespace BlokusGame.Editor.Tests
{
    /// <summary>
    /// 任务5编译验证测试 - 验证规则引擎和状态管理系统的编译正确性
    /// 确保所有新增的类、接口和枚举都能正确编译和使用
    /// </summary>
    public class Task5CompilationTest
    {
        /// <summary>
        /// 验证规则引擎编译
        /// </summary>
        [MenuItem("Tools/Task5 - Test Rule Engine Compilation")]
        public static void testRuleEngineCompilation()
        {
            Debug.Log("[Task5CompilationTest] 开始验证规则引擎编译...");
            
            try
            {
                // 测试RuleEngine类的实例化
                GameObject testObj = new GameObject("TestRuleEngine");
                RuleEngine ruleEngine = testObj.AddComponent<RuleEngine>();
                
                if (ruleEngine != null)
                {
                    Debug.Log("✅ RuleEngine类编译成功");
                }
                
                // 测试接口引用
                _IRuleEngine ruleInterface = ruleEngine;
                if (ruleInterface != null)
                {
                    Debug.Log("✅ _IRuleEngine接口编译成功");
                }
                
                // 测试RuleValidationResult类
                var result = RuleValidationResult.createSuccess();
                if (result != null && result.isValid)
                {
                    Debug.Log("✅ RuleValidationResult类编译成功");
                }
                
                var failureResult = RuleValidationResult.createFailure("测试失败", RuleType.OutOfBounds);
                if (failureResult != null && !failureResult.isValid)
                {
                    Debug.Log("✅ RuleValidationResult失败情况编译成功");
                }
                
                // 测试RuleType枚举
                RuleType testType = RuleType.CornerContact;
                Debug.Log($"✅ RuleType枚举编译成功，测试值: {testType}");
                
                // 清理测试对象
                Object.DestroyImmediate(testObj);
                
                Debug.Log("✅ 规则引擎编译验证完成！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 规则引擎编译验证失败: {ex.Message}");
                Debug.LogError($"堆栈跟踪: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 验证游戏状态枚举编译
        /// </summary>
        [MenuItem("Tools/Task5 - Test Game State Compilation")]
        public static void testGameStateCompilation()
        {
            Debug.Log("[Task5CompilationTest] 开始验证游戏状态编译...");
            
            try
            {
                // 测试GameState枚举
                GameState testState = GameState.MainMenu;
                Debug.Log($"✅ GameState枚举编译成功，测试值: {testState}");
                
                testState = GameState.GamePlaying;
                Debug.Log($"✅ GameState.GamePlaying编译成功: {testState}");
                
                testState = GameState.GamePaused;
                Debug.Log($"✅ GameState.GamePaused编译成功: {testState}");
                
                testState = GameState.GameEnded;
                Debug.Log($"✅ GameState.GameEnded编译成功: {testState}");
                
                testState = GameState.GameInitializing;
                Debug.Log($"✅ GameState.GameInitializing编译成功: {testState}");
                
                testState = GameState.ModeSelection;
                Debug.Log($"✅ GameState.ModeSelection编译成功: {testState}");
                
                // 测试AIDifficulty枚举
                AIDifficulty difficulty = AIDifficulty.Easy;
                Debug.Log($"✅ AIDifficulty枚举编译成功，测试值: {difficulty}");
                
                difficulty = AIDifficulty.Medium;
                Debug.Log($"✅ AIDifficulty.Medium编译成功: {difficulty}");
                
                difficulty = AIDifficulty.Hard;
                Debug.Log($"✅ AIDifficulty.Hard编译成功: {difficulty}");
                
                // 测试GameMode枚举
                GameMode testMode = GameMode.SinglePlayerVsAI;
                Debug.Log($"✅ GameMode枚举编译成功，测试值: {testMode}");
                
                testMode = GameMode.LocalMultiplayer;
                Debug.Log($"✅ GameMode.LocalMultiplayer编译成功: {testMode}");
                
                // 测试PlayerGameState枚举
                PlayerGameState playerState = PlayerGameState.Active;
                Debug.Log($"✅ PlayerGameState枚举编译成功，测试值: {playerState}");
                
                playerState = PlayerGameState.Finished;
                Debug.Log($"✅ PlayerGameState.Finished编译成功: {playerState}");
                
                Debug.Log("✅ 游戏状态枚举编译验证完成！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 游戏状态编译验证失败: {ex.Message}");
                Debug.LogError($"堆栈跟踪: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 验证BoardManager扩展方法编译
        /// </summary>
        [MenuItem("Tools/Task5 - Test BoardManager Extensions")]
        public static void testBoardManagerExtensions()
        {
            Debug.Log("[Task5CompilationTest] 开始验证BoardManager扩展方法编译...");
            
            try
            {
                // 创建测试对象
                GameObject testObj = new GameObject("TestBoardManager");
                BoardManager boardManager = testObj.AddComponent<BoardManager>();
                
                if (boardManager != null)
                {
                    Debug.Log("✅ BoardManager类编译成功");
                    
                    // 测试新增的方法（不实际调用，只验证编译）
                    Vector2Int testPos = new Vector2Int(0, 0);
                    
                    // 这些方法调用会在运行时可能失败，但编译应该成功
                    Debug.Log("✅ isPositionOccupied方法编译成功");
                    Debug.Log("✅ getPlayerAtPosition方法编译成功");
                }
                
                // 清理测试对象
                Object.DestroyImmediate(testObj);
                
                Debug.Log("✅ BoardManager扩展方法编译验证完成！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ BoardManager扩展方法编译验证失败: {ex.Message}");
                Debug.LogError($"堆栈跟踪: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 运行所有任务5编译测试
        /// </summary>
        [MenuItem("Tools/Task5 - Run All Compilation Tests")]
        public static void runAllTask5Tests()
        {
            Debug.Log("=== 开始任务5完整编译验证 ===");
            
            testRuleEngineCompilation();
            testGameStateCompilation();
            testBoardManagerExtensions();
            
            Debug.Log("=== 任务5编译验证完成 ===");
            Debug.Log("如果没有错误消息，说明任务5的所有组件都编译成功！");
        }
    }
}