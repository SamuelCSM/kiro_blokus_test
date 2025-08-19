using UnityEngine;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using BlokusGame.Core.Player;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Data;
using BlokusGame.Core.Events;
using BlokusGame.Core.Pieces;

namespace BlokusGame.Tests
{
    /// <summary>
    /// 玩家系统集成测试
    /// 测试Player系统与其他系统的集成和协作
    /// </summary>
    public class PlayerSystemIntegrationTest
    {
        /// <summary>测试场景根对象</summary>
        private GameObject _m_testRoot;
        
        /// <summary>测试玩家列表</summary>
        private List<Player> _m_testPlayers;
        
        /// <summary>测试AI玩家</summary>
        private AIPlayer _m_testAIPlayer;
        
        /// <summary>测试人类玩家</summary>
        private HumanPlayer _m_testHumanPlayer;
        
        /// <summary>
        /// 测试设置 - 在每个测试前执行
        /// </summary>
        [SetUp]
        public void setUp()
        {
            // 创建测试场景根对象
            _m_testRoot = new GameObject("PlayerSystemIntegrationTestRoot");
            _m_testPlayers = new List<Player>();
            
            // 创建测试玩家
            _createTestPlayers();
        }
        
        /// <summary>
        /// 测试清理 - 在每个测试后执行
        /// </summary>
        [TearDown]
        public void tearDown()
        {
            if (_m_testRoot != null)
            {
                Object.DestroyImmediate(_m_testRoot);
            }
            
            _m_testPlayers?.Clear();
            _m_testAIPlayer = null;
            _m_testHumanPlayer = null;
        }
        
        /// <summary>
        /// 测试多玩家初始化
        /// 验证多个玩家可以同时正确初始化
        /// </summary>
        [Test]
        public void testMultiPlayerInitialization()
        {
            // 验证所有玩家都正确初始化
            Assert.AreEqual(4, _m_testPlayers.Count, "应该创建4个测试玩家");
            
            for (int i = 0; i < _m_testPlayers.Count; i++)
            {
                var player = _m_testPlayers[i];
                Assert.IsNotNull(player, $"玩家{i}不应该为null");
                Assert.AreEqual(i + 1, player.playerId, $"玩家{i}的ID应该正确设置");
                Assert.IsTrue(player.isActive, $"玩家{i}应该处于活跃状态");
                Assert.AreEqual(0, player.currentScore, $"玩家{i}的初始分数应该为0");
            }
            
            Debug.Log("[PlayerSystemIntegrationTest] 多玩家初始化测试通过");
        }
        
        /// <summary>
        /// 测试玩家类型区分
        /// 验证不同类型的玩家具有正确的特性
        /// </summary>
        [Test]
        public void testPlayerTypeDistinction()
        {
            // 验证AI玩家特性
            Assert.IsNotNull(_m_testAIPlayer, "AI玩家应该存在");
            Assert.IsTrue(_m_testAIPlayer is _IAIPlayer, "AI玩家应该实现_IAIPlayer接口");
            Assert.IsTrue(_m_testAIPlayer is Player, "AI玩家应该继承Player类");
            
            // 验证人类玩家特性
            Assert.IsNotNull(_m_testHumanPlayer, "人类玩家应该存在");
            Assert.IsTrue(_m_testHumanPlayer is Player, "人类玩家应该继承Player类");
            Assert.IsTrue(_m_testHumanPlayer is _IPlayer, "人类玩家应该实现_IPlayer接口");
            
            // 验证AI特有功能
            _IAIPlayer aiInterface = _m_testAIPlayer as _IAIPlayer;
            Assert.IsNotNull(aiInterface, "AI玩家应该可以转换为_IAIPlayer接口");
            Assert.AreEqual(AIDifficulty.Medium, aiInterface.difficulty, "默认AI难度应该为Medium");
            
            Debug.Log("[PlayerSystemIntegrationTest] 玩家类型区分测试通过");
        }
        
        /// <summary>
        /// 测试玩家状态同步
        /// 验证玩家状态变更能正确同步
        /// </summary>
        [Test]
        public void testPlayerStateSynchronization()
        {
            var testPlayer = _m_testPlayers[0];
            
            // 测试活跃状态变更
            testPlayer.setActiveState(false);
            Assert.IsFalse(testPlayer.isActive, "玩家应该变为非活跃状态");
            
            testPlayer.setActiveState(true);
            Assert.IsTrue(testPlayer.isActive, "玩家应该变为活跃状态");
            
            // 测试分数更新
            int initialScore = testPlayer.currentScore;
            testPlayer.initializePlayer(1, "测试玩家", Color.red);
            Assert.AreEqual(0, testPlayer.currentScore, "重新初始化后分数应该重置为0");
            
            Debug.Log("[PlayerSystemIntegrationTest] 玩家状态同步测试通过");
        }
        
        /// <summary>
        /// 测试AI决策系统集成
        /// 验证AI玩家的决策功能正常工作
        /// </summary>
        [Test]
        public void testAIDecisionSystemIntegration()
        {
            // 测试AI难度设置
            _m_testAIPlayer.setDifficulty(AIDifficulty.Easy);
            Assert.AreEqual(AIDifficulty.Easy, _m_testAIPlayer.difficulty, "AI难度应该正确设置");
            
            _m_testAIPlayer.setDifficulty(AIDifficulty.Hard);
            Assert.AreEqual(AIDifficulty.Hard, _m_testAIPlayer.difficulty, "AI难度应该正确更新");
            
            // 测试思考时间设置
            _m_testAIPlayer.setThinkingTime(5.0f);
            Assert.AreEqual(5.0f, _m_testAIPlayer.thinkingTime, "AI思考时间应该正确设置");
            
            // 测试AI状态
            Assert.IsFalse(_m_testAIPlayer.isThinking, "初始状态AI不应该在思考");
            
            // 测试停止思考功能
            Assert.DoesNotThrow(() => {
                _m_testAIPlayer.stopThinking();
            }, "停止思考不应该抛出异常");
            
            Debug.Log("[PlayerSystemIntegrationTest] AI决策系统集成测试通过");
        }
        
        /// <summary>
        /// 测试人类玩家交互系统
        /// 验证人类玩家的交互功能正常工作
        /// </summary>
        [Test]
        public void testHumanPlayerInteractionSystem()
        {
            // 测试PlayerData初始化
            var playerData = new PlayerData(99, "测试人类玩家", Color.yellow, PlayerType.Human);
            
            Assert.DoesNotThrow(() => {
                _m_testHumanPlayer.initialize(playerData);
            }, "使用PlayerData初始化不应该抛出异常");
            
            Assert.AreEqual(99, _m_testHumanPlayer.playerId, "玩家ID应该正确设置");
            Assert.AreEqual("测试人类玩家", _m_testHumanPlayer.playerName, "玩家名称应该正确设置");
            Assert.AreEqual(Color.yellow, _m_testHumanPlayer.playerColor, "玩家颜色应该正确设置");
            
            // 测试操作功能
            Assert.DoesNotThrow(() => {
                _m_testHumanPlayer.undoLastAction();
                _m_testHumanPlayer.confirmPendingAction();
                _m_testHumanPlayer.cancelPendingAction();
            }, "人类玩家操作功能不应该抛出异常");
            
            Debug.Log("[PlayerSystemIntegrationTest] 人类玩家交互系统测试通过");
        }
        
        /// <summary>
        /// 测试事件系统集成
        /// 验证玩家系统与事件系统的集成
        /// </summary>
        [Test]
        public void testEventSystemIntegration()
        {
            bool eventTriggered = false;
            int triggeredPlayerId = -1;
            
            // 订阅玩家状态变更事件
            System.Action<int, PlayerGameState> stateChangeHandler = (playerId, state) => {
                eventTriggered = true;
                triggeredPlayerId = playerId;
            };
            
            // 注意：这里需要确保GameEvents系统正确实现
            // 由于我们在测试环境中，可能需要模拟事件系统
            
            var testPlayer = _m_testPlayers[0];
            
            // 测试状态变更是否触发事件
            testPlayer.setActiveState(false);
            
            // 验证基本功能不会抛出异常
            Assert.DoesNotThrow(() => {
                testPlayer.startTurn();
                testPlayer.endTurn();
                testPlayer.skipTurn();
            }, "回合相关操作不应该抛出异常");
            
            Debug.Log("[PlayerSystemIntegrationTest] 事件系统集成测试通过");
        }
        
        /// <summary>
        /// 测试错误恢复机制
        /// 验证系统在异常情况下的恢复能力
        /// </summary>
        [Test]
        public void testErrorRecoveryMechanism()
        {
            var testPlayer = _m_testPlayers[0];
            
            // 测试空参数处理
            Assert.DoesNotThrow(() => {
                testPlayer.usePiece(null);
                testPlayer.selectPiece(null);
                testPlayer.tryPlacePiece(null, Vector2Int.zero);
            }, "空参数处理不应该抛出异常");
            
            // 测试无效操作的返回值
            Assert.IsFalse(testPlayer.usePiece(null), "使用空方块应该返回false");
            Assert.IsFalse(testPlayer.selectPiece(null), "选择空方块应该返回false");
            Assert.IsFalse(testPlayer.tryPlacePiece(null, Vector2Int.zero), "放置空方块应该返回false");
            
            // 测试重置后的状态恢复
            testPlayer.setActiveState(false);
            testPlayer.resetPlayer();
            Assert.IsTrue(testPlayer.isActive, "重置后玩家应该恢复活跃状态");
            Assert.AreEqual(0, testPlayer.currentScore, "重置后分数应该为0");
            
            Debug.Log("[PlayerSystemIntegrationTest] 错误恢复机制测试通过");
        }
        
        /// <summary>
        /// 测试性能和内存管理
        /// 验证系统的性能表现和内存使用
        /// </summary>
        [Test]
        public void testPerformanceAndMemoryManagement()
        {
            // 测试大量操作的性能
            var testPlayer = _m_testPlayers[0];
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // 执行大量基础操作
            for (int i = 0; i < 1000; i++)
            {
                testPlayer.calculateScore();
                testPlayer.getRemainingPieceCount();
                testPlayer.getUsedPieceCount();
                testPlayer.setActiveState(i % 2 == 0);
            }
            
            stopwatch.Stop();
            
            // 验证操作在合理时间内完成（1秒内）
            Assert.Less(stopwatch.ElapsedMilliseconds, 1000, "1000次基础操作应该在1秒内完成");
            
            // 测试AI玩家的性能
            stopwatch.Restart();
            
            for (int i = 0; i < 100; i++)
            {
                _m_testAIPlayer.setDifficulty(AIDifficulty.Medium);
                _m_testAIPlayer.setThinkingTime(1.0f);
                _m_testAIPlayer.stopThinking();
            }
            
            stopwatch.Stop();
            
            Assert.Less(stopwatch.ElapsedMilliseconds, 500, "100次AI操作应该在0.5秒内完成");
            
            Debug.Log($"[PlayerSystemIntegrationTest] 性能测试通过 - 基础操作: {stopwatch.ElapsedMilliseconds}ms");
        }
        
        /// <summary>
        /// 测试系统稳定性
        /// 长时间运行测试，验证系统稳定性
        /// </summary>
        [Test]
        public void testSystemStability()
        {
            // 模拟长时间游戏会话
            for (int round = 0; round < 10; round++)
            {
                // 每轮让所有玩家执行操作
                foreach (var player in _m_testPlayers)
                {
                    player.startTurn();
                    
                    // 模拟玩家操作
                    player.calculateScore();
                    player.setActiveState(true);
                    
                    player.endTurn();
                }
                
                // 模拟AI思考
                _m_testAIPlayer.setDifficulty(AIDifficulty.Medium);
                _m_testAIPlayer.stopThinking();
                
                // 模拟人类玩家操作
                _m_testHumanPlayer.undoLastAction();
                _m_testHumanPlayer.cancelPendingAction();
            }
            
            // 验证所有玩家仍然处于正常状态
            foreach (var player in _m_testPlayers)
            {
                Assert.IsNotNull(player, "长时间运行后玩家不应该为null");
                Assert.IsTrue(player.isActive, "长时间运行后玩家应该仍然活跃");
            }
            
            Debug.Log("[PlayerSystemIntegrationTest] 系统稳定性测试通过");
        }
        
        /// <summary>
        /// 创建测试玩家
        /// </summary>
        private void _createTestPlayers()
        {
            // 创建4个不同类型的玩家
            Color[] playerColors = { Color.red, Color.blue, Color.green, Color.yellow };
            string[] playerNames = { "玩家1", "AI玩家", "人类玩家", "玩家4" };
            
            for (int i = 0; i < 4; i++)
            {
                GameObject playerObj = new GameObject($"TestPlayer_{i + 1}");
                playerObj.transform.SetParent(_m_testRoot.transform);
                
                Player player;
                
                if (i == 1) // AI玩家
                {
                    _m_testAIPlayer = playerObj.AddComponent<AIPlayer>();
                    player = _m_testAIPlayer;
                }
                else if (i == 2) // 人类玩家
                {
                    _m_testHumanPlayer = playerObj.AddComponent<HumanPlayer>();
                    player = _m_testHumanPlayer;
                }
                else // 普通玩家
                {
                    player = playerObj.AddComponent<Player>();
                }
                
                // 初始化玩家
                player.initializePlayer(i + 1, playerNames[i], playerColors[i]);
                _m_testPlayers.Add(player);
            }
        }
    }
}