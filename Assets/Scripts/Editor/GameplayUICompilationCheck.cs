using UnityEngine;
using UnityEditor;
using BlokusGame.Core.UI;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Data;
using System.Collections.Generic;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 游戏内UI交互组件编译验证工具
    /// 验证PlayerInfoUI、PieceIconUI、PlayerResultUI等组件的完整性
    /// </summary>
    public class GameplayUICompilationCheck : EditorWindow
    {
        [MenuItem("Blokus/验证/游戏内UI交互组件编译检查")]
        public static void ShowWindow()
        {
            GetWindow<GameplayUICompilationCheck>("游戏内UI交互组件编译检查");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("游戏内UI交互组件编译验证", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("执行完整编译检查"))
            {
                PerformCompilationCheck();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("测试PlayerInfoUI功能"))
            {
                TestPlayerInfoUI();
            }
            
            if (GUILayout.Button("测试PieceIconUI功能"))
            {
                TestPieceIconUI();
            }
            
            if (GUILayout.Button("测试PlayerResultUI功能"))
            {
                TestPlayerResultUI();
            }
            
            if (GUILayout.Button("测试方块库存可视化"))
            {
                TestPieceInventoryVisualization();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("测试PieceInventoryUI组件"))
            {
                TestPieceInventoryUIComponent();
            }
            
            if (GUILayout.Button("测试PieceDetailUI组件"))
            {
                TestPieceDetailUIComponent();
            }
        }
        
        /// <summary>
        /// 执行完整的编译检查
        /// </summary>
        private void PerformCompilationCheck()
        {
            Debug.Log("=== 开始游戏内UI交互组件编译检查 ===");
            
            bool allTestsPassed = true;
            
            // 检查PlayerInfoUI
            allTestsPassed &= CheckPlayerInfoUI();
            
            // 检查PieceIconUI
            allTestsPassed &= CheckPieceIconUI();
            
            // 检查PlayerResultUI
            allTestsPassed &= CheckPlayerResultUI();
            
            // 检查GameplayUI集成
            allTestsPassed &= CheckGameplayUIIntegration();
            
            // 检查新增的UI组件
            allTestsPassed &= CheckPieceInventoryUI();
            allTestsPassed &= CheckPieceDetailUI();
            
            if (allTestsPassed)
            {
                Debug.Log("✅ 所有游戏内UI交互组件编译检查通过！");
            }
            else
            {
                Debug.LogError("❌ 部分游戏内UI交互组件编译检查失败！");
            }
            
            Debug.Log("=== 游戏内UI交互组件编译检查完成 ===");
        }
        
        /// <summary>
        /// 检查PlayerInfoUI组件
        /// </summary>
        /// <returns>检查是否通过</returns>
        private bool CheckPlayerInfoUI()
        {
            Debug.Log("检查PlayerInfoUI组件...");
            
            try
            {
                // 创建测试GameObject
                var testObj = new GameObject("TestPlayerInfoUI");
                var playerInfoUI = testObj.AddComponent<PlayerInfoUI>();
                
                // 测试初始化
                playerInfoUI.Initialize(0, "测试玩家", Color.red, 100, 15, false);
                
                // 测试更新方法
                playerInfoUI.UpdateScore(150);
                playerInfoUI.UpdateRemainingPieces(12);
                playerInfoUI.SetHighlight(true);
                playerInfoUI.SetPlayerActive(true);
                
                // 测试获取方法
                int playerId = playerInfoUI.GetPlayerId();
                bool isHighlighted = playerInfoUI.IsHighlighted();
                
                // 清理
                DestroyImmediate(testObj);
                
                Debug.Log("✅ PlayerInfoUI组件检查通过");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ PlayerInfoUI组件检查失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 检查PieceIconUI组件
        /// </summary>
        /// <returns>检查是否通过</returns>
        private bool CheckPieceIconUI()
        {
            Debug.Log("检查PieceIconUI组件...");
            
            try
            {
                // 创建测试GameObject
                var testObj = new GameObject("TestPieceIconUI");
                var pieceIconUI = testObj.AddComponent<PieceIconUI>();
                
                // 创建模拟的方块数据
                var mockPiece = new MockGamePiece();
                
                // 测试初始化
                pieceIconUI.Initialize(mockPiece);
                
                // 测试状态设置
                pieceIconUI.SetSelected(true);
                pieceIconUI.SetEnabled(false);
                
                // 测试获取方法
                var gamePiece = pieceIconUI.GetGamePiece();
                bool isSelected = pieceIconUI.IsSelected();
                bool isEnabled = pieceIconUI.IsEnabled();
                
                // 清理
                DestroyImmediate(testObj);
                
                Debug.Log("✅ PieceIconUI组件检查通过");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ PieceIconUI组件检查失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 检查PlayerResultUI组件
        /// </summary>
        /// <returns>检查是否通过</returns>
        private bool CheckPlayerResultUI()
        {
            Debug.Log("检查PlayerResultUI组件...");
            
            try
            {
                // 创建测试GameObject
                var testObj = new GameObject("TestPlayerResultUI");
                var playerResultUI = testObj.AddComponent<PlayerResultUI>();
                
                // 创建测试数据
                var playerResult = new PlayerResult
                {
                    playerId = 0,
                    playerName = "测试玩家",
                    playerColor = Color.blue,
                    finalScore = 200,
                    placedPieces = 18,
                    remainingPieces = 3,
                    isAI = false
                };
                
                // 测试初始化
                playerResultUI.Initialize(playerResult, 1);
                
                // 测试更新
                playerResult.finalScore = 250;
                playerResultUI.UpdateResult(playerResult, 1);
                
                // 测试获取方法
                var result = playerResultUI.GetPlayerResult();
                int rank = playerResultUI.GetRank();
                
                // 清理
                DestroyImmediate(testObj);
                
                Debug.Log("✅ PlayerResultUI组件检查通过");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ PlayerResultUI组件检查失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 检查GameplayUI集成
        /// </summary>
        /// <returns>检查是否通过</returns>
        private bool CheckGameplayUIIntegration()
        {
            Debug.Log("检查GameplayUI集成...");
            
            try
            {
                // 创建测试GameObject
                var testObj = new GameObject("TestGameplayUI");
                var gameplayUI = testObj.AddComponent<GameplayUI>();
                
                // 测试初始化
                gameplayUI.InitializeForGame(4);
                
                // 测试玩家更新
                gameplayUI.UpdateCurrentPlayer(0, "玩家1", Color.red);
                gameplayUI.UpdatePlayerScore(0, 100);
                gameplayUI.UpdatePlayerRemainingPieces(0, 15);
                
                // 测试方块库存更新
                var mockPieces = new List<_IGamePiece> { new MockGamePiece(), new MockGamePiece() };
                gameplayUI.UpdatePieceInventory(mockPieces);
                
                // 测试选中方块
                gameplayUI.SetSelectedPiece(mockPieces[0]);
                
                // 测试提示显示
                gameplayUI.ShowHint("这是一个测试提示");
                gameplayUI.HideHint();
                
                // 清理
                DestroyImmediate(testObj);
                
                Debug.Log("✅ GameplayUI集成检查通过");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ GameplayUI集成检查失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 测试PlayerInfoUI功能
        /// </summary>
        private void TestPlayerInfoUI()
        {
            Debug.Log("=== 测试PlayerInfoUI功能 ===");
            
            var testObj = new GameObject("PlayerInfoUI_Test");
            var playerInfoUI = testObj.AddComponent<PlayerInfoUI>();
            
            // 测试不同的初始化参数
            playerInfoUI.Initialize(0, "人类玩家", Color.red, 0, 21, false);
            Debug.Log("✅ 人类玩家初始化完成");
            
            playerInfoUI.Initialize(1, "AI玩家", Color.blue, 50, 18, true);
            Debug.Log("✅ AI玩家初始化完成");
            
            // 测试状态更新
            playerInfoUI.UpdateScore(150);
            playerInfoUI.UpdateRemainingPieces(10);
            playerInfoUI.SetHighlight(true);
            playerInfoUI.SetPlayerActive(false);
            
            Debug.Log("✅ PlayerInfoUI功能测试完成");
            
            DestroyImmediate(testObj);
        }
        
        /// <summary>
        /// 测试PieceIconUI功能
        /// </summary>
        private void TestPieceIconUI()
        {
            Debug.Log("=== 测试PieceIconUI功能 ===");
            
            var testObj = new GameObject("PieceIconUI_Test");
            var pieceIconUI = testObj.AddComponent<PieceIconUI>();
            
            // 创建不同类型的模拟方块
            var smallPiece = new MockGamePiece { pieceId = 1, currentShape = new Vector2Int[] { Vector2Int.zero } };
            var largePiece = new MockGamePiece { pieceId = 21, currentShape = new Vector2Int[] { 
                Vector2Int.zero, Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left 
            }};
            
            // 测试小方块
            pieceIconUI.Initialize(smallPiece);
            pieceIconUI.SetSelected(true);
            Debug.Log("✅ 小方块图标测试完成");
            
            // 测试大方块
            pieceIconUI.Initialize(largePiece);
            pieceIconUI.SetEnabled(false);
            Debug.Log("✅ 大方块图标测试完成");
            
            Debug.Log("✅ PieceIconUI功能测试完成");
            
            DestroyImmediate(testObj);
        }
        
        /// <summary>
        /// 测试PlayerResultUI功能
        /// </summary>
        private void TestPlayerResultUI()
        {
            Debug.Log("=== 测试PlayerResultUI功能 ===");
            
            var testObj = new GameObject("PlayerResultUI_Test");
            var playerResultUI = testObj.AddComponent<PlayerResultUI>();
            
            // 测试不同排名的玩家结果
            var winnerResult = new PlayerResult
            {
                playerId = 0,
                playerName = "获胜者",
                playerColor = Color.yellow,
                finalScore = 300,
                placedPieces = 21,
                remainingPieces = 0,
                isAI = false
            };
            
            var loserResult = new PlayerResult
            {
                playerId = 1,
                playerName = "AI对手",
                playerColor = Color.red,
                finalScore = 150,
                placedPieces = 15,
                remainingPieces = 6,
                isAI = true
            };
            
            // 测试获胜者显示
            playerResultUI.Initialize(winnerResult, 1);
            Debug.Log("✅ 获胜者结果显示测试完成");
            
            // 测试失败者显示
            playerResultUI.Initialize(loserResult, 2);
            Debug.Log("✅ 失败者结果显示测试完成");
            
            Debug.Log("✅ PlayerResultUI功能测试完成");
            
            DestroyImmediate(testObj);
        }
        
        /// <summary>
        /// 测试方块库存可视化
        /// </summary>
        private void TestPieceInventoryVisualization()
        {
            Debug.Log("=== 测试方块库存可视化 ===");
            
            // 测试PieceInventoryUI组件
            var inventoryTestObj = new GameObject("PieceInventoryUI_Test");
            var pieceInventoryUI = inventoryTestObj.AddComponent<PieceInventoryUI>();
            
            // 创建完整的21个方块库存
            var allPieces = new List<_IGamePiece>();
            for (int i = 1; i <= 21; i++)
            {
                allPieces.Add(new MockGamePiece { pieceId = i });
            }
            
            // 测试库存初始化
            pieceInventoryUI.Initialize(0, allPieces);
            Debug.Log("✅ PieceInventoryUI初始化测试完成");
            
            // 测试库存更新
            var partialPieces = allPieces.GetRange(0, 10);
            pieceInventoryUI.UpdateInventory(partialPieces);
            Debug.Log("✅ PieceInventoryUI更新测试完成");
            
            // 测试方块选择
            pieceInventoryUI.SetSelectedPiece(allPieces[0]);
            Debug.Log("✅ PieceInventoryUI方块选择测试完成");
            
            DestroyImmediate(inventoryTestObj);
            
            // 测试PieceDetailUI组件
            var detailTestObj = new GameObject("PieceDetailUI_Test");
            var pieceDetailUI = detailTestObj.AddComponent<PieceDetailUI>();
            
            // 测试详情显示
            pieceDetailUI.ShowPieceDetail(allPieces[0]);
            Debug.Log("✅ PieceDetailUI显示测试完成");
            
            // 测试详情隐藏
            pieceDetailUI.Hide();
            Debug.Log("✅ PieceDetailUI隐藏测试完成");
            
            DestroyImmediate(detailTestObj);
            
            // 测试原有的GameplayUI库存功能
            var gameplayTestObj = new GameObject("GameplayUI_Test");
            var gameplayUI = gameplayTestObj.AddComponent<GameplayUI>();
            
            // 测试完整库存显示
            gameplayUI.UpdatePieceInventory(allPieces);
            Debug.Log("✅ GameplayUI完整方块库存显示测试完成");
            
            // 测试部分库存显示
            gameplayUI.UpdatePieceInventory(partialPieces);
            Debug.Log("✅ GameplayUI部分方块库存显示测试完成");
            
            // 测试空库存显示
            gameplayUI.UpdatePieceInventory(new List<_IGamePiece>());
            Debug.Log("✅ GameplayUI空方块库存显示测试完成");
            
            DestroyImmediate(gameplayTestObj);
            
            Debug.Log("✅ 方块库存可视化测试完成");
        }
        
        /// <summary>
        /// 测试PieceInventoryUI组件
        /// </summary>
        private void TestPieceInventoryUIComponent()
        {
            Debug.Log("=== 测试PieceInventoryUI组件 ===");
            
            var testObj = new GameObject("PieceInventoryUI_Component_Test");
            var inventoryUI = testObj.AddComponent<PieceInventoryUI>();
            
            // 创建测试方块数据
            var testPieces = new List<_IGamePiece>();
            for (int i = 1; i <= 21; i++)
            {
                var piece = new MockGamePiece 
                { 
                    pieceId = i,
                    currentShape = new Vector2Int[] { Vector2Int.zero },
                    isPlaced = i > 15 // 前15个可用，后6个已放置
                };
                testPieces.Add(piece);
            }
            
            // 测试初始化
            inventoryUI.Initialize(0, testPieces);
            Debug.Log("✅ PieceInventoryUI初始化测试完成");
            
            // 测试方块选择
            inventoryUI.SetSelectedPiece(testPieces[0]);
            var selectedPiece = inventoryUI.GetSelectedPiece();
            Debug.Log($"✅ 方块选择测试完成，选中方块ID: {selectedPiece?.pieceId}");
            
            // 测试方块可用性设置
            inventoryUI.SetPieceAvailable(testPieces[0], false);
            Debug.Log("✅ 方块可用性设置测试完成");
            
            // 测试库存更新
            var updatedPieces = testPieces.GetRange(0, 10);
            inventoryUI.UpdateInventory(updatedPieces);
            Debug.Log("✅ 库存更新测试完成");
            
            // 测试滚动到方块
            inventoryUI.ScrollToPiece(testPieces[5]);
            Debug.Log("✅ 滚动到方块测试完成");
            
            DestroyImmediate(testObj);
            Debug.Log("✅ PieceInventoryUI组件测试完成");
        }
        
        /// <summary>
        /// 测试PieceDetailUI组件
        /// </summary>
        private void TestPieceDetailUIComponent()
        {
            Debug.Log("=== 测试PieceDetailUI组件 ===");
            
            var testObj = new GameObject("PieceDetailUI_Component_Test");
            var detailUI = testObj.AddComponent<PieceDetailUI>();
            
            // 创建测试方块
            var testPiece = new MockGamePiece
            {
                pieceId = 1,
                pieceColor = Color.red,
                currentShape = new Vector2Int[] 
                { 
                    Vector2Int.zero, 
                    Vector2Int.up, 
                    Vector2Int.right 
                },
                isPlaced = false
            };
            
            // 测试显示方块详情
            detailUI.ShowPieceDetail(testPiece);
            var currentPiece = detailUI.GetCurrentPiece();
            Debug.Log($"✅ 显示方块详情测试完成，当前方块ID: {currentPiece?.pieceId}");
            
            // 测试刷新显示
            detailUI.RefreshDisplay();
            Debug.Log("✅ 刷新显示测试完成");
            
            // 测试隐藏详情
            detailUI.Hide();
            Debug.Log("✅ 隐藏详情测试完成");
            
            // 测试空方块处理
            detailUI.ShowPieceDetail(null);
            Debug.Log("✅ 空方块处理测试完成");
            
            DestroyImmediate(testObj);
            Debug.Log("✅ PieceDetailUI组件测试完成");
        }
        
        /// <summary>
        /// 检查PieceInventoryUI组件
        /// </summary>
        /// <returns>检查是否通过</returns>
        private bool CheckPieceInventoryUI()
        {
            Debug.Log("检查PieceInventoryUI组件...");
            
            try
            {
                var testObj = new GameObject("TestPieceInventoryUI");
                var inventoryUI = testObj.AddComponent<PieceInventoryUI>();
                
                // 测试基本功能
                var testPieces = new List<_IGamePiece> { new MockGamePiece() };
                inventoryUI.Initialize(0, testPieces);
                inventoryUI.UpdateInventory(testPieces);
                inventoryUI.SetSelectedPiece(testPieces[0]);
                inventoryUI.SetPieceAvailable(testPieces[0], true);
                
                DestroyImmediate(testObj);
                
                Debug.Log("✅ PieceInventoryUI组件检查通过");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ PieceInventoryUI组件检查失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 检查PieceDetailUI组件
        /// </summary>
        /// <returns>检查是否通过</returns>
        private bool CheckPieceDetailUI()
        {
            Debug.Log("检查PieceDetailUI组件...");
            
            try
            {
                var testObj = new GameObject("TestPieceDetailUI");
                var detailUI = testObj.AddComponent<PieceDetailUI>();
                
                // 测试基本功能
                var testPiece = new MockGamePiece();
                detailUI.ShowPieceDetail(testPiece);
                detailUI.RefreshDisplay();
                detailUI.Hide();
                
                DestroyImmediate(testObj);
                
                Debug.Log("✅ PieceDetailUI组件检查通过");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ PieceDetailUI组件检查失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 模拟的游戏方块类，用于测试
        /// </summary>
        private class MockGamePiece : _IGamePiece
        {
            public int pieceId { get; set; } = 1;
            public Vector2Int[] currentShape { get; set; } = new Vector2Int[] { Vector2Int.zero };
            public Color pieceColor { get; set; } = Color.white;
            public bool isPlaced { get; set; } = false;
            public int playerId { get; set; } = 0;
            
            public void rotate90Clockwise() { }
            public void flipHorizontal() { }
            public List<Vector2Int> getOccupiedPositions(Vector2Int _position) 
            { 
                var positions = new List<Vector2Int>();
                if (currentShape != null)
                {
                    foreach (var offset in currentShape)
                    {
                        positions.Add(_position + offset);
                    }
                }
                return positions;
            }
            public void setPlacedState(bool _placed) { isPlaced = _placed; }
            public void resetToOriginalState() { }
        }
    }
}