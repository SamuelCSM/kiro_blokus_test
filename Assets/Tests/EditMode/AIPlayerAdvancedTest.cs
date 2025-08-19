using UnityEngine;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using BlokusGame.Core.Player;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Data;

namespace BlokusGame.Tests
{
    /// <summary>
    /// AI玩家高级测试
    /// 专门测试AI玩家的决策算法和行为逻辑
    /// </summary>
    public class AIPlayerAdvancedTest
    {
        /// <summary>测试用的AI玩家</summary>
        private AIPlayer _m_testAIPlayer;
        
        /// <summary>测试用的游戏对象</summary>
        private GameObject _m_testGameObject;
        
        /// <summary>
        /// 测试设置 - 在每个测试前执行
        /// </summary>
        [SetUp]
        public void setUp()
        {
            _m_testGameObject = new GameObject("TestAIPlayer");
            _m_testAIPlayer = _m_testGameObject.AddComponent<AIPlayer>();
            _m_testAIPlayer.initializePlayer(1, "测试AI", Color.red);
        }
        
        /// <summary>
        /// 测试清理 - 在每个测试后执行
        /// </summary>
        [TearDown]
        public void tearDown()
        {
            if (_m_testGameObject != null)
            {
                Object.DestroyImmediate(_m_testGameObject);
            }
        }
        
        /// <summary>
        /// 测试AI难度设置和行为变化
        /// 验证不同难度等级下AI的行为差异
        /// </summary>
        [Test]
        public void testAIDifficultyBehavior()
        {
            // 测试简单难度
            _m_testAIPlayer.setDifficulty(_IAIPlayer.AIDifficulty.Easy);
            Assert.AreEqual(_IAIPlayer.AIDifficulty.Easy, _m_testAIPlayer.difficulty, "AI难度应该设置为Easy");
            Assert.AreEqual(1.0f, _m_testAIPlayer.thinkingTime, "Easy难度的思考时间应该为1秒");
            
            // 测试中等难度
            _m_testAIPlayer.setDifficulty(_IAIPlayer.AIDifficulty.Medium);
            Assert.AreEqual(_IAIPlayer.AIDifficulty.Medium, _m_testAIPlayer.difficulty, "AI难度应该设置为Medium");
            Assert.AreEqual(2.0f, _m_testAIPlayer.thinkingTime, "Medium难度的思考时间应该为2秒");
            
            // 测试困难难度
            _m_testAIPlayer.setDifficulty(_IAIPlayer.AIDifficulty.Hard);
            Assert.AreEqual(_IAIPlayer.AIDifficulty.Hard, _m_testAIPlayer.difficulty, "AI难度应该设置为Hard");
            Assert.AreEqual(3.0f, _m_testAIPlayer.thinkingTime, "Hard难度的思考时间应该为3秒");
            
            Debug.Log("[AIPlayerAdvancedTest] AI难度设置测试通过");
        }
        
        /// <summary>
        /// 测试AI思考时间自定义设置
        /// 验证AI思考时间可以独立于难度设置
        /// </summary>
        [Test]
        public void testCustomThinkingTime()
        {
            // 测试自定义思考时间
            _m_testAIPlayer.setThinkingTime(5.5f);
            Assert.AreEqual(5.5f, _m_testAIPlayer.thinkingTime, "自定义思考时间应该正确设置");
            
            // 测试最小思考时间限制
            _m_testAIPlayer.setThinkingTime(0.1f);
            Assert.AreEqual(0.5f, _m_testAIPlayer.thinkingTime, "思考时间不应该小于0.5秒");
            
            // 测试负数思考时间
            _m_testAIPlayer.setThinkingTime(-1.0f);
            Assert.AreEqual(0.5f, _m_testAIPlayer.thinkingTime, "负数思考时间应该被限制为0.5秒");
            
            Debug.Log("[AIPlayerAdvancedTest] 自定义思考时间测试通过");
        }
        
        /// <summary>
        /// 测试AI状态管理
        /// 验证AI的思考状态正确管理
        /// </summary>
        [Test]
        public void testAIStateManagement()
        {
            // 初始状态验证
            Assert.IsFalse(_m_testAIPlayer.isThinking, "初始状态AI不应该在思考");
            
            // 测试停止思考功能
            _m_testAIPlayer.stopThinking();
            Assert.IsFalse(_m_testAIPlayer.isThinking, "停止思考后AI不应该在思考状态");
            
            // 测试多次停止思考
            Assert.DoesNotThrow(() => {
                _m_testAIPlayer.stopThinking();
                _m_testAIPlayer.stopThinking();
                _m_testAIPlayer.stopThinking();
            }, "多次停止思考不应该抛出异常");
            
            Debug.Log("[AIPlayerAdvancedTest] AI状态管理测试通过");
        }
        
        /// <summary>
        /// 测试AI决策方法接口
        /// 验证AI决策相关方法的基本功能
        /// </summary>
        [Test]
        public void testAIDecisionMethods()
        {
            // 创建模拟棋盘（这里用null测试错误处理）
            _IGameBoard mockBoard = null;
            
            // 测试evaluateMove方法
            Assert.DoesNotThrow(() => {
                float score = _m_testAIPlayer.evaluateMove(null, Vector2Int.zero, mockBoard);
                Assert.AreEqual(0f, score, "空参数的移动评估应该返回0分");
            }, "evaluateMove方法不应该抛出异常");
            
            // 测试getBestMove方法
            Assert.DoesNotThrow(() => {
                var bestMove = _m_testAIPlayer.getBestMove(mockBoard);
                Assert.IsNull(bestMove.piece, "空棋盘的最佳移动应该返回null方块");
                Assert.AreEqual(Vector2Int.zero, bestMove.position, "空棋盘的最佳移动位置应该为零向量");
            }, "getBestMove方法不应该抛出异常");
            
            Debug.Log("[AIPlayerAdvancedTest] AI决策方法测试通过");
        }
        
        /// <summary>
        /// 测试AI接口完整性
        /// 验证AI玩家完整实现了_IAIPlayer接口
        /// </summary>
        [Test]
        public void testAIInterfaceCompleteness()
        {
            _IAIPlayer aiInterface = _m_testAIPlayer as _IAIPlayer;
            Assert.IsNotNull(aiInterface, "AI玩家应该实现_IAIPlayer接口");
            
            // 测试所有接口属性
            Assert.IsNotNull(aiInterface.difficulty, "difficulty属性应该可访问");
            Assert.IsNotNull(aiInterface.thinkingTime, "thinkingTime属性应该可访问");
            Assert.IsNotNull(aiInterface.isThinking, "isThinking属性应该可访问");
            
            // 测试所有接口方法
            Assert.DoesNotThrow(() => {
                aiInterface.setDifficulty(_IAIPlayer.AIDifficulty.Medium);
                aiInterface.setThinkingTime(2.0f);
                aiInterface.evaluateMove(null, Vector2Int.zero, null);
                aiInterface.getBestMove(null);
                aiInterface.stopThinking();
            }, "所有_IAIPlayer接口方法都应该可以调用");
            
            Debug.Log("[AIPlayerAdvancedTest] AI接口完整性测试通过");
        }
        
        /// <summary>
        /// 测试AI继承关系
        /// 验证AI玩家正确继承了Player基类
        /// </summary>
        [Test]
        public void testAIInheritanceRelationship()
        {
            // 验证继承关系
            Assert.IsTrue(_m_testAIPlayer is Player, "AIPlayer应该继承自Player");
            Assert.IsTrue(_m_testAIPlayer is _IPlayer, "AIPlayer应该实现_IPlayer接口");
            Assert.IsTrue(_m_testAIPlayer is MonoBehaviour, "AIPlayer应该继承自MonoBehaviour");
            
            // 测试基类方法可用性
            Assert.DoesNotThrow(() => {
                _m_testAIPlayer.calculateScore();
                _m_testAIPlayer.setActiveState(true);
                _m_testAIPlayer.resetPlayer();
                _m_testAIPlayer.startTurn();
                _m_testAIPlayer.endTurn();
            }, "基类方法应该可以正常调用");
            
            // 测试重写方法
            Assert.DoesNotThrow(() => {
                _m_testAIPlayer.initializePlayer(2, "新AI", Color.blue);
            }, "重写的initializePlayer方法应该可以正常调用");
            
            Assert.AreEqual(2, _m_testAIPlayer.playerId, "重写方法应该正确设置玩家ID");
            Assert.AreEqual("新AI", _m_testAIPlayer.playerName, "重写方法应该正确设置玩家名称");
            
            Debug.Log("[AIPlayerAdvancedTest] AI继承关系测试通过");
        }
        
        /// <summary>
        /// 测试AI错误处理机制
        /// 验证AI在异常情况下的处理能力
        /// </summary>
        [Test]
        public void testAIErrorHandling()
        {
            // 测试无效难度设置（虽然枚举通常不会有无效值，但测试边界情况）
            Assert.DoesNotThrow(() => {
                _m_testAIPlayer.setDifficulty(_IAIPlayer.AIDifficulty.Easy);
                _m_testAIPlayer.setDifficulty(_IAIPlayer.AIDifficulty.Medium);
                _m_testAIPlayer.setDifficulty(_IAIPlayer.AIDifficulty.Hard);
            }, "设置有效难度不应该抛出异常");
            
            // 测试极端思考时间值
            Assert.DoesNotThrow(() => {
                _m_testAIPlayer.setThinkingTime(float.MaxValue);
                _m_testAIPlayer.setThinkingTime(float.MinValue);
                _m_testAIPlayer.setThinkingTime(0f);
            }, "设置极端思考时间值不应该抛出异常");
            
            // 测试空参数处理
            Assert.DoesNotThrow(() => {
                _m_testAIPlayer.evaluateMove(null, Vector2Int.zero, null);
                _m_testAIPlayer.getBestMove(null);
            }, "空参数处理不应该抛出异常");
            
            Debug.Log("[AIPlayerAdvancedTest] AI错误处理测试通过");
        }
        
        /// <summary>
        /// 测试AI性能特性
        /// 验证AI决策方法的性能表现
        /// </summary>
        [Test]
        public void testAIPerformanceCharacteristics()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // 测试大量决策评估的性能
            for (int i = 0; i < 1000; i++)
            {
                _m_testAIPlayer.evaluateMove(null, new Vector2Int(i % 20, i % 20), null);
            }
            
            stopwatch.Stop();
            
            // 验证性能在合理范围内（1秒内完成1000次评估）
            Assert.Less(stopwatch.ElapsedMilliseconds, 1000, "1000次移动评估应该在1秒内完成");
            
            // 测试难度设置的性能
            stopwatch.Restart();
            
            for (int i = 0; i < 1000; i++)
            {
                _IAIPlayer.AIDifficulty difficulty = (i % 3) switch
                {
                    0 => _IAIPlayer.AIDifficulty.Easy,
                    1 => _IAIPlayer.AIDifficulty.Medium,
                    _ => _IAIPlayer.AIDifficulty.Hard
                };
                _m_testAIPlayer.setDifficulty(difficulty);
            }
            
            stopwatch.Stop();
            
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, "1000次难度设置应该在100ms内完成");
            
            Debug.Log($"[AIPlayerAdvancedTest] AI性能测试通过 - 评估: {stopwatch.ElapsedMilliseconds}ms");
        }
        
        /// <summary>
        /// 测试AI内存使用
        /// 验证AI不会造成内存泄漏
        /// </summary>
        [Test]
        public void testAIMemoryUsage()
        {
            // 记录初始内存使用
            long initialMemory = System.GC.GetTotalMemory(true);
            
            // 执行大量AI操作
            for (int i = 0; i < 100; i++)
            {
                _m_testAIPlayer.setDifficulty(_IAIPlayer.AIDifficulty.Medium);
                _m_testAIPlayer.setThinkingTime(1.0f);
                _m_testAIPlayer.evaluateMove(null, Vector2Int.zero, null);
                _m_testAIPlayer.getBestMove(null);
                _m_testAIPlayer.stopThinking();
            }
            
            // 强制垃圾回收
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            long finalMemory = System.GC.GetTotalMemory(false);
            long memoryIncrease = finalMemory - initialMemory;
            
            // 验证内存增长在合理范围内（小于1MB）
            Assert.Less(memoryIncrease, 1024 * 1024, "AI操作不应该造成显著的内存增长");
            
            Debug.Log($"[AIPlayerAdvancedTest] AI内存使用测试通过 - 内存增长: {memoryIncrease} bytes");
        }
    }
}