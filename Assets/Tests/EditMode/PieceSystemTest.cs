using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using BlokusGame.Core.Pieces;
using BlokusGame.Core.Managers;
using BlokusGame.Core.Data;
using BlokusGame.Core.Interfaces;
using System.Reflection;

namespace BlokusGame.Editor.Tests
{
    /// <summary>
    /// 方块系统单元测试 - 验证方块相关功能的正确性
    /// 测试GamePiece、PieceManager、PieceVisualizer等组件的核心功能
    /// 确保方块系统符合设计要求和游戏规则
    /// </summary>
    public class PieceSystemTest
    {
        private GameObject _testGameObject;
        private GamePiece _testPiece;
        private PieceData _testPieceData;
        
        /// <summary>
        /// 测试前的设置
        /// </summary>
        [SetUp]
        public void setUp()
        {
            // 创建测试用的游戏对象
            _testGameObject = new GameObject("TestPiece");
            _testPiece = _testGameObject.AddComponent<GamePiece>();
            
            // 创建测试用的方块数据
            _testPieceData = ScriptableObject.CreateInstance<PieceData>();
            _setupTestPieceData();
        }
        
        /// <summary>
        /// 测试后的清理
        /// </summary>
        [TearDown]
        public void tearDown()
        {
            if (_testGameObject != null)
            {
                Object.DestroyImmediate(_testGameObject);
            }
            
            if (_testPieceData != null)
            {
                Object.DestroyImmediate(_testPieceData);
            }
        }
        
        /// <summary>
        /// 测试方块初始化
        /// </summary>
        [Test]
        public void testPieceInitialization()
        {
            // 测试初始化
            Color testColor = Color.red;
            int testPlayerId = 1;
            
            _testPiece.initialize(_testPieceData, testPlayerId, testColor);
            
            // 验证初始化结果
            Assert.AreEqual(1, _testPiece.pieceId, "方块ID应该正确设置");
            Assert.AreEqual(testPlayerId, _testPiece.playerId, "玩家ID应该正确设置");
            Assert.AreEqual(testColor, _testPiece.pieceColor, "方块颜色应该正确设置");
            Assert.IsFalse(_testPiece.isPlaced, "方块初始状态应该是未放置");
            
            Debug.Log("[PieceSystemTest] 方块初始化测试通过");
        }
        
        /// <summary>
        /// 测试方块旋转功能
        /// </summary>
        [Test]
        public void testPieceRotation()
        {
            _testPiece.initialize(_testPieceData, 0, Color.white);
            
            // 获取初始形状
            Vector2Int[] originalShape = _testPiece.currentShape;
            
            // 旋转90度
            _testPiece.rotate90Clockwise();
            Vector2Int[] rotatedShape = _testPiece.currentShape;
            
            // 验证形状已改变（除非是对称形状）
            Assert.AreEqual(originalShape.Length, rotatedShape.Length, "旋转后方块大小应该保持不变");
            Assert.AreEqual(1, _testPiece.getRotationCount(), "旋转次数应该正确记录");
            
            // 旋转4次应该回到原始状态
            _testPiece.rotate90Clockwise();
            _testPiece.rotate90Clockwise();
            _testPiece.rotate90Clockwise();
            
            Vector2Int[] finalShape = _testPiece.currentShape;
            Assert.AreEqual(0, _testPiece.getRotationCount(), "旋转4次后应该回到初始状态");
            
            // 验证形状是否回到原始状态
            for (int i = 0; i < originalShape.Length; i++)
            {
                Assert.AreEqual(originalShape[i], finalShape[i], $"位置 {i} 应该回到原始状态");
            }
            
            Debug.Log("[PieceSystemTest] 方块旋转测试通过");
        }
        
        /// <summary>
        /// 测试方块翻转功能
        /// </summary>
        [Test]
        public void testPieceFlip()
        {
            _testPiece.initialize(_testPieceData, 0, Color.white);
            
            // 获取初始状态
            bool initialFlipState = _testPiece.getFlippedState();
            Vector2Int[] originalShape = _testPiece.currentShape;
            
            // 翻转方块
            _testPiece.flipHorizontal();
            bool flippedState = _testPiece.getFlippedState();
            Vector2Int[] flippedShape = _testPiece.currentShape;
            
            // 验证翻转状态
            Assert.AreNotEqual(initialFlipState, flippedState, "翻转状态应该改变");
            Assert.AreEqual(originalShape.Length, flippedShape.Length, "翻转后方块大小应该保持不变");
            
            // 再次翻转应该回到原始状态
            _testPiece.flipHorizontal();
            bool finalFlipState = _testPiece.getFlippedState();
            
            Assert.AreEqual(initialFlipState, finalFlipState, "两次翻转后应该回到初始状态");
            
            Debug.Log("[PieceSystemTest] 方块翻转测试通过");
        }
        
        /// <summary>
        /// 测试方块位置计算
        /// </summary>
        [Test]
        public void testPiecePositionCalculation()
        {
            _testPiece.initialize(_testPieceData, 0, Color.white);
            
            Vector2Int testPosition = new Vector2Int(5, 5);
            var occupiedPositions = _testPiece.getOccupiedPositions(testPosition);
            
            // 验证占用位置数量
            Assert.AreEqual(_testPiece.currentShape.Length, occupiedPositions.Count, 
                "占用位置数量应该等于方块格子数量");
            
            // 验证所有位置都是相对于基准位置的偏移
            foreach (var pos in occupiedPositions)
            {
                Assert.IsTrue(pos.x >= testPosition.x - 2 && pos.x <= testPosition.x + 2, 
                    "X坐标应该在合理范围内");
                Assert.IsTrue(pos.y >= testPosition.y - 2 && pos.y <= testPosition.y + 2, 
                    "Y坐标应该在合理范围内");
            }
            
            Debug.Log("[PieceSystemTest] 方块位置计算测试通过");
        }
        
        /// <summary>
        /// 测试方块边界检查
        /// </summary>
        [Test]
        public void testPieceBoundaryCheck()
        {
            _testPiece.initialize(_testPieceData, 0, Color.white);
            
            Vector2Int boardSize = new Vector2Int(20, 20);
            
            // 测试有效位置
            Assert.IsTrue(_testPiece.canPlaceAt(new Vector2Int(10, 10), boardSize), 
                "中心位置应该可以放置");
            
            // 测试边界位置
            Assert.IsTrue(_testPiece.canPlaceAt(new Vector2Int(0, 0), boardSize), 
                "左上角应该可以放置");
            
            // 测试无效位置
            Assert.IsFalse(_testPiece.canPlaceAt(new Vector2Int(-1, 0), boardSize), 
                "超出左边界的位置不应该可以放置");
            Assert.IsFalse(_testPiece.canPlaceAt(new Vector2Int(0, -1), boardSize), 
                "超出上边界的位置不应该可以放置");
            Assert.IsFalse(_testPiece.canPlaceAt(new Vector2Int(20, 10), boardSize), 
                "超出右边界的位置不应该可以放置");
            Assert.IsFalse(_testPiece.canPlaceAt(new Vector2Int(10, 20), boardSize), 
                "超出下边界的位置不应该可以放置");
            
            Debug.Log("[PieceSystemTest] 方块边界检查测试通过");
        }
        
        /// <summary>
        /// 测试方块状态管理
        /// </summary>
        [Test]
        public void testPieceStateManagement()
        {
            _testPiece.initialize(_testPieceData, 0, Color.white);
            
            // 测试初始状态
            Assert.IsFalse(_testPiece.isPlaced, "初始状态应该是未放置");
            
            // 设置为已放置
            _testPiece.setPlacedState(true);
            Assert.IsTrue(_testPiece.isPlaced, "应该正确设置为已放置状态");
            
            // 重置到初始状态
            _testPiece.resetToOriginalState();
            Assert.IsFalse(_testPiece.isPlaced, "重置后应该是未放置状态");
            Assert.AreEqual(0, _testPiece.getRotationCount(), "重置后旋转次数应该为0");
            Assert.IsFalse(_testPiece.getFlippedState(), "重置后翻转状态应该为false");
            
            Debug.Log("[PieceSystemTest] 方块状态管理测试通过");
        }
        
        /// <summary>
        /// 测试方块边界矩形计算
        /// </summary>
        [Test]
        public void testPieceBounds()
        {
            _testPiece.initialize(_testPieceData, 0, Color.white);
            
            RectInt bounds = _testPiece.getBounds();
            
            // 验证边界矩形的有效性
            Assert.IsTrue(bounds.width > 0, "边界宽度应该大于0");
            Assert.IsTrue(bounds.height > 0, "边界高度应该大于0");
            Assert.IsTrue(bounds.width <= 5, "边界宽度不应该超过5（最大方块尺寸）");
            Assert.IsTrue(bounds.height <= 5, "边界高度不应该超过5（最大方块尺寸）");
            
            Debug.Log($"[PieceSystemTest] 方块边界测试通过，边界: {bounds}");
        }
        
        /// <summary>
        /// 测试PieceManager的基本功能
        /// </summary>
        [Test]
        public void testPieceManagerBasics()
        {
            // 创建PieceManager测试对象
            GameObject managerObj = new GameObject("TestPieceManager");
            PieceManager manager = managerObj.AddComponent<PieceManager>();
            
            try
            {
                // 测试单例
                Assert.IsNotNull(PieceManager.instance, "PieceManager单例应该存在");
                
                // 测试玩家方块创建（需要配置数据）
                // 这里只测试基本功能，实际创建需要配置数据
                
                Debug.Log("[PieceSystemTest] PieceManager基础测试通过");
            }
            finally
            {
                Object.DestroyImmediate(managerObj);
            }
        }
        
        /// <summary>
        /// 设置测试用的方块数据
        /// </summary>
        private void _setupTestPieceData()
        {
            // 创建一个简单的L形方块用于测试
            Vector2Int[] lShape = new Vector2Int[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(0, 1)
            };
            
            // 使用反射设置私有字段（仅用于测试）
            var pieceIdField = typeof(PieceData).GetField("_m_pieceId", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var shapeField = typeof(PieceData).GetField("_m_originalShape", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sizeField = typeof(PieceData).GetField("_m_size", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            pieceIdField?.SetValue(_testPieceData, 1);
            shapeField?.SetValue(_testPieceData, lShape);
            sizeField?.SetValue(_testPieceData, lShape.Length);
        }
        
        /// <summary>
        /// 运行所有方块系统测试
        /// </summary>
        [MenuItem("Tools/Run Piece System Tests")]
        public static void runAllTests()
        {
            Debug.Log("[PieceSystemTest] 开始运行方块系统测试...");
            
            var testInstance = new PieceSystemTest();
            
            try
            {
                testInstance.setUp();
                testInstance.testPieceInitialization();
                testInstance.tearDown();
                
                testInstance.setUp();
                testInstance.testPieceRotation();
                testInstance.tearDown();
                
                testInstance.setUp();
                testInstance.testPieceFlip();
                testInstance.tearDown();
                
                testInstance.setUp();
                testInstance.testPiecePositionCalculation();
                testInstance.tearDown();
                
                testInstance.setUp();
                testInstance.testPieceBoundaryCheck();
                testInstance.tearDown();
                
                testInstance.setUp();
                testInstance.testPieceStateManagement();
                testInstance.tearDown();
                
                testInstance.setUp();
                testInstance.testPieceBounds();
                testInstance.tearDown();
                
                testInstance.testPieceManagerBasics();
                
                Debug.Log("[PieceSystemTest] ✅ 所有方块系统测试通过！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PieceSystemTest] ❌ 测试失败: {ex.Message}");
                Debug.LogError($"[PieceSystemTest] 堆栈跟踪: {ex.StackTrace}");
            }
        }
    }
}