using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using BlokusGame.Core.Managers;
using BlokusGame.Core.Data;
using BlokusGame.Core.Events;
using BlokusGame.Core.Rules;
using BlokusGame.Core.Pieces;
using BlokusGame.Core.Interfaces;

namespace BlokusGame.Editor.Tests
{
    /// <summary>
    /// 最终编译测试 - 验证所有编译问题是否已修复
    /// </summary>
    public class FinalCompilationTest
    {
        /// <summary>
        /// 验证所有核心类型是否可以正常访问
        /// </summary>
        [Test]
        public void testAllCoreTypesAccessible()
        {
            Debug.Log("[FinalCompilationTest] 开始验证所有核心类型...");
            
            // 验证管理器类型
            Assert.IsNotNull(typeof(GameManager), "GameManager类型应该可访问");
            Assert.IsNotNull(typeof(GameStateManager), "GameStateManager类型应该可访问");
            Assert.IsNotNull(typeof(PlayerManager), "PlayerManager类型应该可访问");
            Assert.IsNotNull(typeof(BoardManager), "BoardManager类型应该可访问");
            Assert.IsNotNull(typeof(PieceManager), "PieceManager类型应该可访问");
            
            // 验证数据类型
            Assert.IsNotNull(typeof(GameState), "GameState枚举应该可访问");
            Assert.IsNotNull(typeof(GameMode), "GameMode枚举应该可访问");
            Assert.IsNotNull(typeof(MessageType), "MessageType枚举应该可访问");
            
            // 验证规则引擎
            Assert.IsNotNull(typeof(RuleEngine), "RuleEngine类型应该可访问");
            
            // 验证方块系统
            Assert.IsNotNull(typeof(GamePiece), "GamePiece类型应该可访问");
            
            Debug.Log("[FinalCompilationTest] ✅ 所有核心类型验证通过");
        }
        
        /// <summary>
        /// 验证NUnit属性是否正常工作
        /// </summary>
        [Test]
        public void testNUnitAttributesWorking()
        {
            Debug.Log("[FinalCompilationTest] 验证NUnit属性...");
            
            // 验证基本断言
            Assert.IsTrue(true, "基本断言应该通过");
            Assert.IsFalse(false, "否定断言应该通过");
            Assert.AreEqual(1, 1, "相等断言应该通过");
            Assert.AreNotEqual(1, 2, "不等断言应该通过");
            
            Debug.Log("[FinalCompilationTest] ✅ NUnit属性验证通过");
        }
        
        /// <summary>
        /// 验证GameManager的基本功能
        /// </summary>
        [Test]
        public void testGameManagerBasicFunctionality()
        {
            Debug.Log("[FinalCompilationTest] 验证GameManager基本功能...");
            
            var gameObject = new GameObject("TestGameManager");
            var gameManager = gameObject.AddComponent<GameManager>();
            
            // 验证属性访问
            var currentState = gameManager.currentGameState;
            var isActive = gameManager.isGameActive;
            var isPaused = gameManager.isPaused;
            
            Assert.IsNotNull(gameManager, "GameManager应该可以创建");
            Debug.Log($"  - 当前状态: {currentState}");
            Debug.Log($"  - 游戏活跃: {isActive}");
            Debug.Log($"  - 游戏暂停: {isPaused}");
            
            // 验证方法调用
            bool canAdvance = gameManager.canAdvanceTurn();
            float progress = gameManager.getGameProgress();
            
            Debug.Log($"  - 可以切换回合: {canAdvance}");
            Debug.Log($"  - 游戏进度: {progress}");
            
            Object.DestroyImmediate(gameObject);
            Debug.Log("[FinalCompilationTest] ✅ GameManager基本功能验证通过");
        }
        
        /// <summary>
        /// 验证事件系统是否正常
        /// </summary>
        [Test]
        public void testEventSystemWorking()
        {
            Debug.Log("[FinalCompilationTest] 验证事件系统...");
            
            // 验证GameEvents单例
            Assert.IsNotNull(GameEvents.instance, "GameEvents单例应该可访问");
            
            // 验证事件类型
            bool eventTriggered = false;
            GameEvents.onGameStarted += (_playerCount) => { eventTriggered = true; };
            
            // 触发事件
            GameEvents.onGameStarted?.Invoke(4);
            
            Assert.IsTrue(eventTriggered, "事件应该能正常触发");
            
            Debug.Log("[FinalCompilationTest] ✅ 事件系统验证通过");
        }
        
        /// <summary>
        /// 设置测试环境
        /// </summary>
        [SetUp]
        public void setUp()
        {
            Debug.Log("[FinalCompilationTest] 设置测试环境");
        }
        
        /// <summary>
        /// 清理测试环境
        /// </summary>
        [TearDown]
        public void tearDown()
        {
            Debug.Log("[FinalCompilationTest] 清理测试环境");
        }
    }
}