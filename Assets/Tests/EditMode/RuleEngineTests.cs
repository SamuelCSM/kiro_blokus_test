using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using BlokusGame.Core.Rules;
using BlokusGame.Core.Pieces;
using BlokusGame.Core.Data;
using BlokusGame.Core.Managers;

namespace BlokusGame.Tests
{
    /// <summary>
    /// 规则引擎单元测试类
    /// 测试Blokus游戏规则的验证逻辑
    /// 确保所有游戏规则都能正确执行
    /// </summary>
    public class RuleEngineTests
    {
        /// <summary>测试用的规则引擎实例</summary>
        private RuleEngine _m_ruleEngine;
        
        /// <summary>测试用的游戏对象</summary>
        private GameObject _m_testGameObject;
        
        /// <summary>测试用的方块管理器</summary>
        private PieceManager _m_pieceManager;
        
        /// <summary>测试用的棋盘管理器</summary>
        private BoardManager _m_boardManager;
        
        /// <summary>测试用的方块数据</summary>
        private PieceData _m_testPieceData;
        
        /// <summary>
        /// 每个测试前的初始化设置
        /// 创建测试环境和必要的组件
        /// </summary>
        [SetUp]
        public void setUp()
        {
            // 创建测试游戏对象
            _m_testGameObject = new GameObject("TestRuleEngine");
            
            // 添加必要的组件
            _m_ruleEngine = _m_testGameObject.AddComponent<RuleEngine>();
            _m_pieceManager = _m_testGameObject.AddComponent<PieceManager>();
            _m_boardManager = _m_testGameObject.AddComponent<BoardManager>();
            
            // 创建测试方块数据
            _m_testPieceData = ScriptableObject.CreateInstance<PieceData>();
            _setupTestPieceData();
        }
        
        /// <summary>
        /// 每个测试后的清理工作
        /// 销毁测试对象和清理资源
        /// </summary>
        [TearDown]
        public void tearDown()
        {
            if (_m_testGameObject != null)
            {
                Object.DestroyImmediate(_m_testGameObject);
                _m_testGameObject = null;
            }
            
            if (_m_testPieceData != null)
            {
                Object.DestroyImmediate(_m_testPieceData);
                _m_testPieceData = null;
            }
            
            // 清理其他引用
            _m_ruleEngine = null;
            _m_pieceManager = null;
            _m_boardManager = null;
        }
        
        /// <summary>
        /// 测试玩家起始角落位置获取
        /// 验证每个玩家的起始位置是否正确
        /// </summary>
        [Test]
        public void testGetPlayerStartCorner()
        {
            // 测试四个玩家的起始角落
            Assert.AreEqual(new Vector2Int(0, 0), _m_ruleEngine.getPlayerStartCorner(0), "玩家0应该在左下角");
            Assert.AreEqual(new Vector2Int(19, 0), _m_ruleEngine.getPlayerStartCorner(1), "玩家1应该在右下角");
            Assert.AreEqual(new Vector2Int(19, 19), _m_ruleEngine.getPlayerStartCorner(2), "玩家2应该在右上角");
            Assert.AreEqual(new Vector2Int(0, 19), _m_ruleEngine.getPlayerStartCorner(3), "玩家3应该在左上角");
            
            // 测试无效玩家ID
            Assert.AreEqual(Vector2Int.zero, _m_ruleEngine.getPlayerStartCorner(-1), "无效玩家ID应该返回零向量");
            Assert.AreEqual(Vector2Int.zero, _m_ruleEngine.getPlayerStartCorner(4), "超出范围的玩家ID应该返回零向量");
        }
        
        /// <summary>
        /// 测试首次放置检测
        /// 验证能否正确识别玩家的首次放置
        /// </summary>
        [Test]
        public void testIsFirstPlacement()
        {
            // 初始状态下所有玩家都是首次放置
            Assert.IsTrue(_m_ruleEngine.isFirstPlacement(0), "玩家0初始应该是首次放置");
            Assert.IsTrue(_m_ruleEngine.isFirstPlacement(1), "玩家1初始应该是首次放置");
            
            // 模拟玩家0放置了一个方块后
            _m_pieceManager.createPlayerPieces(0);
            var playerPieces = _m_pieceManager.getPlayerPieces(0);
            if (playerPieces.Count > 0)
            {
                _m_pieceManager.markPieceAsPlaced(playerPieces[0], Vector2Int.zero);
                Assert.IsFalse(_m_ruleEngine.isFirstPlacement(0), "玩家0放置方块后不应该是首次放置");
            }
            
            // 玩家1仍然是首次放置
            Assert.IsTrue(_m_ruleEngine.isFirstPlacement(1), "玩家1仍然应该是首次放置");
        }
        
        /// <summary>
        /// 测试首次放置的角落位置验证
        /// 验证首次放置必须占据角落位置的规则
        /// </summary>
        [Test]
        public void testIsValidCornerPlacement()
        {
            // 创建测试方块
            var testPiece = _createTestGamePiece();
            
            try
            {
                // 测试玩家0的角落位置放置
                Vector2Int player0Corner = new Vector2Int(0, 0);
                Assert.IsTrue(_m_ruleEngine.isValidCornerPlacement(testPiece, player0Corner, 0), 
                    "在角落位置放置应该有效");
                
                // 测试非角落位置放置
                Vector2Int nonCornerPosition = new Vector2Int(5, 5);
                Assert.IsFalse(_m_ruleEngine.isValidCornerPlacement(testPiece, nonCornerPosition, 0), 
                    "在非角落位置放置应该无效");
                
                // 测试其他玩家的角落位置
                Vector2Int player1Corner = new Vector2Int(19, 0);
                Assert.IsFalse(_m_ruleEngine.isValidCornerPlacement(testPiece, player1Corner, 0), 
                    "在其他玩家角落位置放置应该无效");
            }
            finally
            {
                // 清理测试方块
                if (testPiece != null && testPiece.gameObject != null)
                {
                    Object.DestroyImmediate(testPiece.gameObject);
                }
            }
        }
        
        /// <summary>
        /// 测试方块重叠检测
        /// 验证能否正确检测方块重叠
        /// </summary>
        [Test]
        public void testHasOverlap()
        {
            var testPiece = _createTestGamePiece();
            
            try
            {
                // 在空棋盘上放置应该没有重叠
                Assert.IsFalse(_m_ruleEngine.hasOverlap(testPiece, Vector2Int.zero), 
                    "在空棋盘上放置不应该有重叠");
                
                // 模拟棋盘上已有方块的情况需要BoardManager的支持
                // 这里只测试基本逻辑
            }
            finally
            {
                // 清理测试方块
                if (testPiece != null && testPiece.gameObject != null)
                {
                    Object.DestroyImmediate(testPiece.gameObject);
                }
            }
        }
        
        /// <summary>
        /// 测试游戏结束条件检测
        /// 验证能否正确判断游戏是否结束
        /// </summary>
        [Test]
        public void testIsGameOver()
        {
            // 初始状态游戏不应该结束
            Assert.IsFalse(_m_ruleEngine.isGameOver(), "初始状态游戏不应该结束");
            
            // 当所有玩家都无法继续时游戏应该结束
            // 这个测试需要更复杂的设置，暂时只测试基本调用
        }
        
        /// <summary>
        /// 测试规则验证结果创建
        /// 验证RuleValidationResult的创建和使用
        /// </summary>
        [Test]
        public void testRuleValidationResult()
        {
            // 测试成功结果
            var successResult = RuleValidationResult.createSuccess();
            Assert.IsTrue(successResult.isValid, "成功结果应该有效");
            Assert.AreEqual("放置合法", successResult.getUserFriendlyMessage(), "成功结果消息应该正确");
            
            // 测试失败结果
            var failureResult = RuleValidationResult.createFailure("测试失败", RuleType.Overlap);
            Assert.IsFalse(failureResult.isValid, "失败结果应该无效");
            Assert.AreEqual("方块不能放置在已被占据的位置", failureResult.getUserFriendlyMessage(), 
                "失败结果消息应该正确");
        }
        
        /// <summary>
        /// 测试规则类型枚举
        /// 验证所有规则类型都已定义
        /// </summary>
        [Test]
        public void testRuleTypeEnum()
        {
            // 验证所有规则类型都存在
            Assert.IsTrue(System.Enum.IsDefined(typeof(RuleType), RuleType.None), "None规则类型应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(RuleType), RuleType.FirstPlacementCorner), "FirstPlacementCorner规则类型应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(RuleType), RuleType.CornerContact), "CornerContact规则类型应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(RuleType), RuleType.EdgeContact), "EdgeContact规则类型应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(RuleType), RuleType.Overlap), "Overlap规则类型应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(RuleType), RuleType.OutOfBounds), "OutOfBounds规则类型应该存在");
        }
        
        /// <summary>
        /// 设置测试方块数据
        /// 创建用于测试的简单方块配置
        /// </summary>
        private void _setupTestPieceData()
        {
            // 使用反射设置私有字段（仅用于测试）
            var pieceIdField = typeof(PieceData).GetField("_m_pieceId", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pieceNameField = typeof(PieceData).GetField("_m_pieceName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var originalShapeField = typeof(PieceData).GetField("_m_originalShape", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sizeField = typeof(PieceData).GetField("_m_size", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // 创建简单的单格方块用于测试
            Vector2Int[] testShape = new Vector2Int[] { new Vector2Int(0, 0) };
            
            pieceIdField?.SetValue(_m_testPieceData, 1);
            pieceNameField?.SetValue(_m_testPieceData, "测试方块");
            originalShapeField?.SetValue(_m_testPieceData, testShape);
            sizeField?.SetValue(_m_testPieceData, 1);
        }
        
        /// <summary>
        /// 创建测试用的GamePiece实例
        /// </summary>
        /// <returns>测试方块实例</returns>
        private GamePiece _createTestGamePiece()
        {
            GameObject pieceObject = new GameObject("TestPiece");
            GamePiece piece = pieceObject.AddComponent<GamePiece>();
            
            // 直接调用公共的initialize方法
            piece.initialize(_m_testPieceData, 0, Color.red);
            
            return piece;
        }
    }
}