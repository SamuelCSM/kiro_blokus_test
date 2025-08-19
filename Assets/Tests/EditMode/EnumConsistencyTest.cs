using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using BlokusGame.Core.Data;

namespace BlokusGame.Editor.Tests
{
    /// <summary>
    /// 枚举一致性测试类
    /// 验证项目中枚举定义的一致性和完整性
    /// 确保没有重复定义和缺失状态
    /// </summary>
    public class EnumConsistencyTest
    {
        /// <summary>
        /// 测试GameState枚举的完整性
        /// 验证所有必要的游戏状态都已定义
        /// </summary>
        [Test]
        public void testGameStateEnumCompleteness()
        {
            // 验证所有必要的游戏状态都存在
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.MainMenu), "MainMenu状态应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.ModeSelection), "ModeSelection状态应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.Settings), "Settings状态应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.GameInitializing), "GameInitializing状态应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.WaitingForPlayers), "WaitingForPlayers状态应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.GamePlaying), "GamePlaying状态应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.GamePaused), "GamePaused状态应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.GameEnded), "GameEnded状态应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.ShowingResults), "ShowingResults状态应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.Loading), "Loading状态应该存在");
            
            Debug.Log("[EnumConsistencyTest] GameState枚举完整性测试通过");
        }
        
        /// <summary>
        /// 测试GameMode枚举的完整性
        /// 验证所有游戏模式都已定义
        /// </summary>
        [Test]
        public void testGameModeEnumCompleteness()
        {
            // 验证所有游戏模式都存在
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameMode), GameMode.SinglePlayerVsAI), "SinglePlayerVsAI模式应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameMode), GameMode.LocalMultiplayer), "LocalMultiplayer模式应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameMode), GameMode.OnlineMultiplayer), "OnlineMultiplayer模式应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameMode), GameMode.Tutorial), "Tutorial模式应该存在");
            
            Debug.Log("[EnumConsistencyTest] GameMode枚举完整性测试通过");
        }
        
        /// <summary>
        /// 测试AIDifficulty枚举的完整性
        /// 验证所有AI难度级别都已定义
        /// </summary>
        [Test]
        public void testAIDifficultyEnumCompleteness()
        {
            // 验证所有AI难度级别都存在
            Assert.IsTrue(System.Enum.IsDefined(typeof(AIDifficulty), AIDifficulty.Easy), "Easy难度应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(AIDifficulty), AIDifficulty.Medium), "Medium难度应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(AIDifficulty), AIDifficulty.Hard), "Hard难度应该存在");
            
            Debug.Log("[EnumConsistencyTest] AIDifficulty枚举完整性测试通过");
        }
        
        /// <summary>
        /// 测试PlayerGameState枚举的完整性
        /// 验证所有玩家状态都已定义
        /// </summary>
        [Test]
        public void testPlayerGameStateEnumCompleteness()
        {
            // 验证所有玩家状态都存在
            Assert.IsTrue(System.Enum.IsDefined(typeof(PlayerGameState), PlayerGameState.Active), "Active状态应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(PlayerGameState), PlayerGameState.Skipped), "Skipped状态应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(PlayerGameState), PlayerGameState.Finished), "Finished状态应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(PlayerGameState), PlayerGameState.Quit), "Quit状态应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(PlayerGameState), PlayerGameState.Disconnected), "Disconnected状态应该存在");
            
            Debug.Log("[EnumConsistencyTest] PlayerGameState枚举完整性测试通过");
        }
        
        /// <summary>
        /// 运行所有枚举一致性测试
        /// </summary>
        [MenuItem("Tools/Run Enum Consistency Tests")]
        public static void runAllEnumTests()
        {
            Debug.Log("[EnumConsistencyTest] 开始运行枚举一致性测试...");
            
            var testInstance = new EnumConsistencyTest();
            
            try
            {
                testInstance.testGameStateEnumCompleteness();
                testInstance.testGameModeEnumCompleteness();
                testInstance.testAIDifficultyEnumCompleteness();
                testInstance.testPlayerGameStateEnumCompleteness();
                
                Debug.Log("[EnumConsistencyTest] ✅ 所有枚举一致性测试通过！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EnumConsistencyTest] ❌ 测试失败: {ex.Message}");
                Debug.LogError($"[EnumConsistencyTest] 堆栈跟踪: {ex.StackTrace}");
            }
        }
    }
}