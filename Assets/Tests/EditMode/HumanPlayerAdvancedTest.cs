using UnityEngine;
using NUnit.Framework;
using BlokusGame.Core.Player;
using BlokusGame.Core.Data;
using BlokusGame.Core.Pieces;
using BlokusGame.Core.Events;

namespace BlokusGame.Tests
{
    /// <summary>
    /// 人类玩家高级测试
    /// 专门测试人类玩家的交互功能和操作历史管理
    /// </summary>
    public class HumanPlayerAdvancedTest
    {
        /// <summary>测试用的人类玩家</summary>
        private HumanPlayer _m_testHumanPlayer;
        
        /// <summary>测试用的游戏对象</summary>
        private GameObject _m_testGameObject;
        
        /// <summary>
        /// 测试设置 - 在每个测试前执行
        /// </summary>
        [SetUp]
        public void setUp()
        {
            _m_testGameObject = new GameObject("TestHumanPlayer");
            _m_testHumanPlayer = _m_testGameObject.AddComponent<HumanPlayer>();
            _m_testHumanPlayer.initializePlayer(1, "测试人类玩家", Color.blue);
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
        /// 测试PlayerData初始化
        /// 验证使用PlayerData结构初始化人类玩家
        /// </summary>
        [Test]
        public void testPlayerDataInitialization()
        {
            // 创建测试用的PlayerData（使用构造函数）
            var playerData = new PlayerData(99, "PlayerData测试玩家", Color.magenta, PlayerType.Human);
            
            // 测试使用PlayerData初始化
            Assert.DoesNotThrow(() => {
                _m_testHumanPlayer.initialize(playerData);
            }, "使用PlayerData初始化不应该抛出异常");
            
            // 验证初始化结果
            Assert.AreEqual(99, _m_testHumanPlayer.playerId, "玩家ID应该正确设置");
            Assert.AreEqual("PlayerData测试玩家", _m_testHumanPlayer.playerName, "玩家名称应该正确设置");
            Assert.AreEqual(Color.magenta, _m_testHumanPlayer.playerColor, "玩家颜色应该正确设置");
            
            Debug.Log("[HumanPlayerAdvancedTest] PlayerData初始化测试通过");
        }
        
        /// <summary>
        /// 测试空PlayerData处理
        /// 验证传入空PlayerData时的错误处理
        /// </summary>
        [Test]
        public void testNullPlayerDataHandling()
        {
            // 测试空PlayerData处理
            Assert.DoesNotThrow(() => {
                _m_testHumanPlayer.initialize(null);
            }, "空PlayerData处理不应该抛出异常");
            
            // 验证玩家状态没有被破坏
            Assert.AreEqual(1, _m_testHumanPlayer.playerId, "空PlayerData不应该改变现有玩家ID");
            Assert.AreEqual("测试人类玩家", _m_testHumanPlayer.playerName, "空PlayerData不应该改变现有玩家名称");
            
            Debug.Log("[HumanPlayerAdvancedTest] 空PlayerData处理测试通过");
        }
        
        /// <summary>
        /// 测试非人类玩家类型警告
        /// 验证使用非Human类型的PlayerData时的警告处理
        /// </summary>
        [Test]
        public void testNonHumanPlayerTypeWarning()
        {
            // 创建AI类型的PlayerData（使用构造函数）
            var aiPlayerData = new PlayerData(88, "AI类型测试", Color.cyan, PlayerType.AI);
            
            // 测试使用AI类型的PlayerData初始化人类玩家
            Assert.DoesNotThrow(() => {
                _m_testHumanPlayer.initialize(aiPlayerData);
            }, "使用AI类型PlayerData不应该抛出异常");
            
            // 验证仍然可以正常初始化
            Assert.AreEqual(88, _m_testHumanPlayer.playerId, "即使类型不匹配，玩家ID也应该正确设置");
            Assert.AreEqual("AI类型测试", _m_testHumanPlayer.playerName, "即使类型不匹配，玩家名称也应该正确设置");
            
            Debug.Log("[HumanPlayerAdvancedTest] 非人类玩家类型警告测试通过");
        }
        
        /// <summary>
        /// 测试操作撤销功能
        /// 验证人类玩家的撤销操作功能
        /// </summary>
        [Test]
        public void testUndoFunctionality()
        {
            // 测试初始状态下的撤销操作
            bool undoResult = _m_testHumanPlayer.undoLastAction();
            Assert.IsFalse(undoResult, "没有操作历史时撤销应该返回false");
            
            // 测试多次撤销
            Assert.DoesNotThrow(() => {
                _m_testHumanPlayer.undoLastAction();
                _m_testHumanPlayer.undoLastAction();
                _m_testHumanPlayer.undoLastAction();
            }, "多次撤销操作不应该抛出异常");
            
            Debug.Log("[HumanPlayerAdvancedTest] 操作撤销功能测试通过");
        }
        
        /// <summary>
        /// 测试操作确认功能
        /// 验证人类玩家的操作确认机制
        /// </summary>
        [Test]
        public void testConfirmationFunctionality()
        {
            // 测试确认待确认操作
            Assert.DoesNotThrow(() => {
                _m_testHumanPlayer.confirmPendingAction();
            }, "确认操作不应该抛出异常");
            
            // 测试取消待确认操作
            Assert.DoesNotThrow(() => {
                _m_testHumanPlayer.cancelPendingAction();
            }, "取消操作不应该抛出异常");
            
            // 测试多次确认和取消
            Assert.DoesNotThrow(() => {
                _m_testHumanPlayer.confirmPendingAction();
                _m_testHumanPlayer.cancelPendingAction();
                _m_testHumanPlayer.confirmPendingAction();
                _m_testHumanPlayer.cancelPendingAction();
            }, "多次确认和取消操作不应该抛出异常");
            
            Debug.Log("[HumanPlayerAdvancedTest] 操作确认功能测试通过");
        }
        
        /// <summary>
        /// 测试方块交互功能
        /// 验证人类玩家与方块的交互处理
        /// </summary>
        [Test]
        public void testPieceInteractionFunctionality()
        {
            // 测试空方块点击处理
            Assert.DoesNotThrow(() => {
                _m_testHumanPlayer.onPieceClicked(null);
            }, "空方块点击处理不应该抛出异常");
            
            // 测试拖拽功能
            Assert.DoesNotThrow(() => {
                _m_testHumanPlayer.onPieceDragStart(null, Vector3.zero);
                _m_testHumanPlayer.onPieceDragging(Vector3.one);
                _m_testHumanPlayer.onPieceDragEnd(Vector3.zero, Vector2Int.zero);
            }, "方块拖拽功能不应该抛出异常");
            
            Debug.Log("[HumanPlayerAdvancedTest] 方块交互功能测试通过");
        }
        
        /// <summary>
        /// 测试继承关系和接口实现
        /// 验证人类玩家正确继承和实现相关接口
        /// </summary>
        [Test]
        public void testInheritanceAndInterfaceImplementation()
        {
            // 验证继承关系
            Assert.IsTrue(_m_testHumanPlayer is Player, "HumanPlayer应该继承自Player");
            Assert.IsTrue(_m_testHumanPlayer is MonoBehaviour, "HumanPlayer应该继承自MonoBehaviour");
            
            // 验证接口实现
            Assert.IsTrue(_m_testHumanPlayer is BlokusGame.Core.Interfaces._IPlayer, "HumanPlayer应该实现_IPlayer接口");
            
            // 测试基类方法可用性
            Assert.DoesNotThrow(() => {
                _m_testHumanPlayer.calculateScore();
                _m_testHumanPlayer.setActiveState(true);
                _m_testHumanPlayer.startTurn();
                _m_testHumanPlayer.endTurn();
            }, "基类方法应该可以正常调用");
            
            Debug.Log("[HumanPlayerAdvancedTest] 继承关系和接口实现测试通过");
        }
        
        /// <summary>
        /// 测试重置功能
        /// 验证人类玩家的重置功能正确清理所有状态
        /// </summary>
        [Test]
        public void testResetFunctionality()
        {
            // 执行一些操作来改变状态
            _m_testHumanPlayer.setActiveState(false);
            _m_testHumanPlayer.undoLastAction();
            _m_testHumanPlayer.confirmPendingAction();
            
            // 执行重置
            Assert.DoesNotThrow(() => {
                _m_testHumanPlayer.resetPlayer();
            }, "重置操作不应该抛出异常");
            
            // 验证重置后的状态
            Assert.IsTrue(_m_testHumanPlayer.isActive, "重置后玩家应该处于活跃状态");
            Assert.AreEqual(0, _m_testHumanPlayer.currentScore, "重置后分数应该为0");
            
            // 测试重置后的功能可用性
            Assert.DoesNotThrow(() => {
                _m_testHumanPlayer.undoLastAction();
                _m_testHumanPlayer.confirmPendingAction();
                _m_testHumanPlayer.cancelPendingAction();
            }, "重置后所有功能应该可以正常使用");
            
            Debug.Log("[HumanPlayerAdvancedTest] 重置功能测试通过");
        }
        
        /// <summary>
        /// 测试错误处理机制
        /// 验证人类玩家在各种异常情况下的处理能力
        /// </summary>
        [Test]
        public void testErrorHandlingMechanism()
        {
            // 测试在非当前回合时的操作
            // 注意：由于没有GameStateManager，isCurrentPlayer可能始终为false
            Assert.DoesNotThrow(() => {
                _m_testHumanPlayer.onPieceClicked(null);
                _m_testHumanPlayer.onPieceDragStart(null, Vector3.zero);
                _m_testHumanPlayer.undoLastAction();
            }, "非当前回合的操作不应该抛出异常");
            
            // 测试极端参数值
            Assert.DoesNotThrow(() => {
                _m_testHumanPlayer.onPieceDragStart(null, new Vector3(float.MaxValue, float.MinValue, 0));
                _m_testHumanPlayer.onPieceDragging(new Vector3(float.NaN, float.PositiveInfinity, float.NegativeInfinity));
                _m_testHumanPlayer.onPieceDragEnd(Vector3.zero, new Vector2Int(int.MaxValue, int.MinValue));
            }, "极端参数值不应该抛出异常");
            
            Debug.Log("[HumanPlayerAdvancedTest] 错误处理机制测试通过");
        }
        
        /// <summary>
        /// 测试性能特性
        /// 验证人类玩家操作的性能表现
        /// </summary>
        [Test]
        public void testPerformanceCharacteristics()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // 测试大量交互操作的性能
            for (int i = 0; i < 1000; i++)
            {
                _m_testHumanPlayer.onPieceClicked(null);
                _m_testHumanPlayer.onPieceDragStart(null, Vector3.zero);
                _m_testHumanPlayer.onPieceDragging(Vector3.one);
                _m_testHumanPlayer.onPieceDragEnd(Vector3.zero, Vector2Int.zero);
                _m_testHumanPlayer.undoLastAction();
                _m_testHumanPlayer.confirmPendingAction();
                _m_testHumanPlayer.cancelPendingAction();
            }
            
            stopwatch.Stop();
            
            // 验证性能在合理范围内（2秒内完成1000次操作循环）
            Assert.Less(stopwatch.ElapsedMilliseconds, 2000, "1000次交互操作循环应该在2秒内完成");
            
            Debug.Log($"[HumanPlayerAdvancedTest] 性能测试通过 - 交互操作: {stopwatch.ElapsedMilliseconds}ms");
        }
        
        /// <summary>
        /// 测试内存管理
        /// 验证人类玩家不会造成内存泄漏
        /// </summary>
        [Test]
        public void testMemoryManagement()
        {
            // 记录初始内存使用
            long initialMemory = System.GC.GetTotalMemory(true);
            
            // 执行大量操作
            for (int i = 0; i < 100; i++)
            {
                // 使用构造函数创建PlayerData
                var playerData = new PlayerData(i, $"测试玩家{i}", Color.red, PlayerType.Human);
                
                _m_testHumanPlayer.initialize(playerData);
                _m_testHumanPlayer.undoLastAction();
                _m_testHumanPlayer.confirmPendingAction();
                _m_testHumanPlayer.cancelPendingAction();
                _m_testHumanPlayer.resetPlayer();
            }
            
            // 强制垃圾回收
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            long finalMemory = System.GC.GetTotalMemory(false);
            long memoryIncrease = finalMemory - initialMemory;
            
            // 验证内存增长在合理范围内（小于1MB）
            Assert.Less(memoryIncrease, 1024 * 1024, "人类玩家操作不应该造成显著的内存增长");
            
            Debug.Log($"[HumanPlayerAdvancedTest] 内存管理测试通过 - 内存增长: {memoryIncrease} bytes");
        }
        
        /// <summary>
        /// 测试状态一致性
        /// 验证人类玩家在各种操作后状态保持一致
        /// </summary>
        [Test]
        public void testStateConsistency()
        {
            // 记录初始状态
            int initialPlayerId = _m_testHumanPlayer.playerId;
            string initialPlayerName = _m_testHumanPlayer.playerName;
            Color initialPlayerColor = _m_testHumanPlayer.playerColor;
            
            // 执行各种操作
            _m_testHumanPlayer.onPieceClicked(null);
            _m_testHumanPlayer.undoLastAction();
            _m_testHumanPlayer.confirmPendingAction();
            _m_testHumanPlayer.cancelPendingAction();
            _m_testHumanPlayer.setActiveState(false);
            _m_testHumanPlayer.setActiveState(true);
            
            // 验证基本信息没有被意外改变
            Assert.AreEqual(initialPlayerId, _m_testHumanPlayer.playerId, "操作后玩家ID不应该改变");
            Assert.AreEqual(initialPlayerName, _m_testHumanPlayer.playerName, "操作后玩家名称不应该改变");
            Assert.AreEqual(initialPlayerColor, _m_testHumanPlayer.playerColor, "操作后玩家颜色不应该改变");
            
            // 验证状态一致性
            Assert.IsTrue(_m_testHumanPlayer.isActive, "最终状态应该是活跃的");
            
            Debug.Log("[HumanPlayerAdvancedTest] 状态一致性测试通过");
        }
    }
}