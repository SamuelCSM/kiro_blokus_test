using UnityEngine;
using NUnit.Framework;
using BlokusGame.Core.Player;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Data;

namespace BlokusGame.Tests
{
    /// <summary>
    /// 玩家系统最终验证测试
    /// 验证所有修正是否正确应用，确保系统完整性
    /// </summary>
    public class PlayerSystemFinalVerification
    {
        /// <summary>测试用的玩家实例</summary>
        private Player _m_testPlayer;
        
        /// <summary>测试用的AI玩家实例</summary>
        private AIPlayer _m_testAIPlayer;
        
        /// <summary>测试用的人类玩家实例</summary>
        private HumanPlayer _m_testHumanPlayer;
        
        /// <summary>
        /// 测试设置 - 在每个测试前执行
        /// </summary>
        [SetUp]
        public void setUp()
        {
            // 创建测试用的GameObject
            GameObject playerObj = new GameObject("TestPlayer");
            GameObject aiPlayerObj = new GameObject("TestAIPlayer");
            GameObject humanPlayerObj = new GameObject("TestHumanPlayer");
            
            // 添加组件
            _m_testPlayer = playerObj.AddComponent<Player>();
            _m_testAIPlayer = aiPlayerObj.AddComponent<AIPlayer>();
            _m_testHumanPlayer = humanPlayerObj.AddComponent<HumanPlayer>();
        }
        
        /// <summary>
        /// 测试清理 - 在每个测试后执行
        /// </summary>
        [TearDown]
        public void tearDown()
        {
            if (_m_testPlayer != null)
                Object.DestroyImmediate(_m_testPlayer.gameObject);
            if (_m_testAIPlayer != null)
                Object.DestroyImmediate(_m_testAIPlayer.gameObject);
            if (_m_testHumanPlayer != null)
                Object.DestroyImmediate(_m_testHumanPlayer.gameObject);
        }
        
        /// <summary>
        /// 测试Player基类接口实现
        /// 验证所有_IPlayer接口方法都正确实现
        /// </summary>
        [Test]
        public void testPlayerInterfaceImplementation()
        {
            // 验证Player类实现了_IPlayer接口
            Assert.IsTrue(_m_testPlayer is _IPlayer, "Player类应该实现_IPlayer接口");
            
            // 验证接口属性可以正常访问
            _IPlayer playerInterface = _m_testPlayer as _IPlayer;
            Assert.IsNotNull(playerInterface, "Player类应该可以转换为_IPlayer接口");
            
            // 测试接口属性
            Assert.AreEqual(0, playerInterface.playerId, "默认玩家ID应该为0");
            Assert.IsNotNull(playerInterface.availablePieces, "可用方块列表不应该为null");
            Assert.IsNotNull(playerInterface.usedPieces, "已使用方块列表不应该为null");
            
            Debug.Log("[PlayerSystemFinalVerification] Player接口实现验证通过");
        }
        
        /// <summary>
        /// 测试AIPlayer类接口实现
        /// 验证AIPlayer正确实现了_IPlayer和_IAIPlayer接口
        /// </summary>
        [Test]
        public void testAIPlayerInterfaceImplementation()
        {
            // 验证AIPlayer实现了_IPlayer接口
            Assert.IsTrue(_m_testAIPlayer is _IPlayer, "AIPlayer类应该实现_IPlayer接口");
            
            // 验证AIPlayer实现了_IAIPlayer接口
            Assert.IsTrue(_m_testAIPlayer is _IAIPlayer, "AIPlayer类应该实现_IAIPlayer接口");
            
            // 测试AI特有属性
            _IAIPlayer aiInterface = _m_testAIPlayer as _IAIPlayer;
            Assert.IsNotNull(aiInterface, "AIPlayer应该可以转换为_IAIPlayer接口");
            
            // 验证AI属性
            Assert.AreEqual(AIDifficulty.Medium, aiInterface.difficulty, "默认AI难度应该为Medium");
            Assert.IsFalse(aiInterface.isThinking, "初始状态AI不应该在思考");
            Assert.Greater(aiInterface.thinkingTime, 0f, "思考时间应该大于0");
            
            Debug.Log("[PlayerSystemFinalVerification] AIPlayer接口实现验证通过");
        }
        
        /// <summary>
        /// 测试HumanPlayer类继承
        /// 验证HumanPlayer正确继承了Player类
        /// </summary>
        [Test]
        public void testHumanPlayerInheritance()
        {
            // 验证HumanPlayer继承自Player
            Assert.IsTrue(_m_testHumanPlayer is Player, "HumanPlayer应该继承自Player类");
            
            // 验证HumanPlayer实现了_IPlayer接口
            Assert.IsTrue(_m_testHumanPlayer is _IPlayer, "HumanPlayer应该实现_IPlayer接口");
            
            // 测试继承的方法
            _m_testHumanPlayer.initializePlayer(1, "测试玩家", Color.red);
            Assert.AreEqual(1, _m_testHumanPlayer.playerId, "玩家ID应该正确设置");
            Assert.AreEqual("测试玩家", _m_testHumanPlayer.playerName, "玩家名称应该正确设置");
            
            Debug.Log("[PlayerSystemFinalVerification] HumanPlayer继承验证通过");
        }
        
        /// <summary>
        /// 测试方法调用兼容性
        /// 验证所有方法调用都与接口定义匹配
        /// </summary>
        [Test]
        public void testMethodCallCompatibility()
        {
            // 初始化测试玩家
            _m_testPlayer.initializePlayer(1, "兼容性测试玩家", Color.blue);
            _m_testAIPlayer.initializePlayer(2, "AI兼容性测试", Color.yellow);
            
            // 测试基础方法调用并验证返回值
            Assert.DoesNotThrow(() => {
                int score = _m_testPlayer.calculateScore();
                int remainingCount = _m_testPlayer.getRemainingPieceCount();
                int usedCount = _m_testPlayer.getUsedPieceCount();
                _m_testPlayer.setActiveState(true);
                
                // 验证返回值的合理性
                Assert.GreaterOrEqual(score, 0, "分数应该大于等于0");
                Assert.GreaterOrEqual(remainingCount, 0, "剩余方块数应该大于等于0");
                Assert.GreaterOrEqual(usedCount, 0, "已使用方块数应该大于等于0");
            }, "基础方法调用不应该抛出异常");
            
            // 测试AI特有方法
            Assert.DoesNotThrow(() => {
                _m_testAIPlayer.setDifficulty(AIDifficulty.Hard);
                _m_testAIPlayer.setThinkingTime(3.0f);
                _m_testAIPlayer.stopThinking();
                
                // 验证设置是否生效
                Assert.AreEqual(AIDifficulty.Hard, _m_testAIPlayer.difficulty, "AI难度设置应该生效");
                Assert.AreEqual(3.0f, _m_testAIPlayer.thinkingTime, "思考时间设置应该生效");
            }, "AI方法调用不应该抛出异常");
            
            Debug.Log("[PlayerSystemFinalVerification] 方法调用兼容性验证通过");
        }
        
        /// <summary>
        /// 测试Unity生命周期方法
        /// 验证所有Unity生命周期方法都正确重写
        /// </summary>
        [Test]
        public void testUnityLifecycleMethods()
        {
            // 这个测试主要验证编译时没有错误
            // Unity生命周期方法的正确性在运行时验证
            
            // 验证AIPlayer的生命周期方法存在
            var aiPlayerType = typeof(AIPlayer);
            var awakeMethod = aiPlayerType.GetMethod("Awake", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var startMethod = aiPlayerType.GetMethod("Start", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var destroyMethod = aiPlayerType.GetMethod("OnDestroy", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.IsNotNull(awakeMethod, "AIPlayer应该有Awake方法");
            Assert.IsNotNull(startMethod, "AIPlayer应该有Start方法");
            Assert.IsNotNull(destroyMethod, "AIPlayer应该有OnDestroy方法");
            
            Debug.Log("[PlayerSystemFinalVerification] Unity生命周期方法验证通过");
        }
        
        /// <summary>
        /// 测试错误处理
        /// 验证所有错误处理都正确实现
        /// </summary>
        [Test]
        public void testErrorHandling()
        {
            // 初始化玩家以确保测试环境正确
            _m_testPlayer.initializePlayer(1, "错误测试玩家", Color.red);
            
            // 测试空参数处理 - 这些方法应该优雅处理null参数而不抛出异常
            Assert.DoesNotThrow(() => {
                _m_testPlayer.usePiece(null);
                _m_testPlayer.selectPiece(null);
                _m_testPlayer.tryPlacePiece(null, Vector2Int.zero);
            }, "空参数处理不应该抛出异常");
            
            // 测试无效操作的返回值
            Assert.IsFalse(_m_testPlayer.usePiece(null), "使用空方块应该返回false");
            Assert.IsFalse(_m_testPlayer.selectPiece(null), "选择空方块应该返回false");
            Assert.IsFalse(_m_testPlayer.tryPlacePiece(null, Vector2Int.zero), "放置空方块应该返回false");
            
            Debug.Log("[PlayerSystemFinalVerification] 错误处理验证通过");
        }
        
        /// <summary>
        /// 测试事件系统集成
        /// 验证所有事件都正确定义和使用
        /// </summary>
        [Test]
        public void testEventSystemIntegration()
        {
            // 验证GameEvents中的相关事件存在
            var gameEventsType = typeof(BlokusGame.Core.Events.GameEvents);
            
            // 检查静态事件
            var playerSkippedField = gameEventsType.GetField("onPlayerSkipped");
            var playerStateChangedField = gameEventsType.GetField("onPlayerStateChanged");
            var turnStartedField = gameEventsType.GetField("onTurnStarted");
            var turnEndedField = gameEventsType.GetField("onTurnEnded");
            
            Assert.IsNotNull(playerSkippedField, "onPlayerSkipped事件应该存在");
            Assert.IsNotNull(playerStateChangedField, "onPlayerStateChanged事件应该存在");
            Assert.IsNotNull(turnStartedField, "onTurnStarted事件应该存在");
            Assert.IsNotNull(turnEndedField, "onTurnEnded事件应该存在");
            
            // 检查实例事件
            var gameEventsInstance = BlokusGame.Core.Events.GameEvents.instance;
            Assert.IsNotNull(gameEventsInstance, "GameEvents实例应该存在");
            
            Debug.Log("[PlayerSystemFinalVerification] 事件系统集成验证通过");
        }
        
        /// <summary>
        /// 测试编码规范遵循
        /// 验证代码是否遵循项目编码规范
        /// </summary>
        [Test]
        public void testCodingStandardsCompliance()
        {
            // 验证类名规范
            Assert.AreEqual("Player", typeof(Player).Name, "Player类名应该符合PascalCase规范");
            Assert.AreEqual("AIPlayer", typeof(AIPlayer).Name, "AIPlayer类名应该符合PascalCase规范");
            Assert.AreEqual("HumanPlayer", typeof(HumanPlayer).Name, "HumanPlayer类名应该符合PascalCase规范");
            
            // 验证接口名规范
            Assert.IsTrue(typeof(_IPlayer).Name.StartsWith("_I"), "接口名应该以_I开头");
            Assert.IsTrue(typeof(_IAIPlayer).Name.StartsWith("_I"), "接口名应该以_I开头");
            
            Debug.Log("[PlayerSystemFinalVerification] 编码规范遵循验证通过");
        }
        
        /// <summary>
        /// 综合功能测试
        /// 测试完整的玩家操作流程
        /// </summary>
        [Test]
        public void testComprehensiveFunctionality()
        {
            // 初始化玩家
            _m_testPlayer.initializePlayer(1, "综合测试玩家", Color.green);
            
            // 验证初始状态
            Assert.AreEqual(1, _m_testPlayer.playerId);
            Assert.AreEqual("综合测试玩家", _m_testPlayer.playerName);
            Assert.AreEqual(Color.green, _m_testPlayer.playerColor);
            Assert.IsTrue(_m_testPlayer.isActive);
            Assert.AreEqual(0, _m_testPlayer.currentScore);
            
            // 测试状态管理
            _m_testPlayer.setActiveState(false);
            Assert.IsFalse(_m_testPlayer.isActive);
            
            _m_testPlayer.setActiveState(true);
            Assert.IsTrue(_m_testPlayer.isActive);
            
            // 测试重置功能
            _m_testPlayer.resetPlayer();
            Assert.AreEqual(0, _m_testPlayer.currentScore);
            Assert.IsTrue(_m_testPlayer.isActive);
            
            Debug.Log("[PlayerSystemFinalVerification] 综合功能测试通过");
        }
    }
}