using UnityEngine;
using UnityEditor;
using BlokusGame.Core.UI;
using BlokusGame.Core.Interfaces;
using System.Collections.Generic;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 最终UI编译检查工具
    /// 验证所有UI组件是否正确编译和工作
    /// </summary>
    public class FinalUICompilationCheck : EditorWindow
    {
        [MenuItem("Blokus/验证/最终UI编译检查")]
        public static void ShowWindow()
        {
            GetWindow<FinalUICompilationCheck>("最终UI编译检查");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("最终UI编译检查", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("执行完整编译验证"))
            {
                PerformFullCompilationCheck();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("验证所有UI组件"))
            {
                VerifyAllUIComponents();
            }
            
            if (GUILayout.Button("验证接口实现"))
            {
                VerifyInterfaceImplementations();
            }
        }
        
        /// <summary>
        /// 执行完整编译验证
        /// </summary>
        private void PerformFullCompilationCheck()
        {
            Debug.Log("=== 开始最终UI编译检查 ===");
            
            bool allTestsPassed = true;
            
            // 验证接口
            allTestsPassed &= VerifyIGamePieceInterface();
            
            // 验证UI组件
            allTestsPassed &= VerifyPlayerInfoUI();
            allTestsPassed &= VerifyPieceIconUI();
            allTestsPassed &= VerifyPlayerResultUI();
            allTestsPassed &= VerifyPieceInventoryUI();
            allTestsPassed &= VerifyPieceDetailUI();
            
            if (allTestsPassed)
            {
                Debug.Log("✅ 所有UI编译检查通过！");
            }
            else
            {
                Debug.LogError("❌ 部分UI编译检查失败！");
            }
            
            Debug.Log("=== 最终UI编译检查完成 ===");
        }
        
        /// <summary>
        /// 验证_IGamePiece接口
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool VerifyIGamePieceInterface()
        {
            Debug.Log("验证_IGamePiece接口...");
            
            try
            {
                var testPiece = new TestGamePiece();
                
                // 测试所有属性
                int id = testPiece.pieceId;
                int playerId = testPiece.playerId;
                var shape = testPiece.currentShape;
                var color = testPiece.pieceColor;
                bool placed = testPiece.isPlaced;
                
                // 测试所有方法
                testPiece.rotate90Clockwise();
                testPiece.flipHorizontal();
                testPiece.setPlacedState(true);
                testPiece.resetToOriginalState();
                
                var positions = testPiece.getOccupiedPositions(Vector2Int.zero);
                int size = testPiece.getSize();
                
                Debug.Log("✅ _IGamePiece接口验证通过");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ _IGamePiece接口验证失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 验证PlayerInfoUI
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool VerifyPlayerInfoUI()
        {
            Debug.Log("验证PlayerInfoUI...");
            
            try
            {
                var testObj = new GameObject("TestPlayerInfoUI");
                var playerInfoUI = testObj.AddComponent<PlayerInfoUI>();
                
                playerInfoUI.Initialize(0, "测试玩家", Color.red, 100, 15, false);
                playerInfoUI.UpdateScore(150);
                playerInfoUI.UpdateRemainingPieces(12);
                playerInfoUI.SetHighlight(true);
                
                DestroyImmediate(testObj);
                
                Debug.Log("✅ PlayerInfoUI验证通过");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ PlayerInfoUI验证失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 验证PieceIconUI
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool VerifyPieceIconUI()
        {
            Debug.Log("验证PieceIconUI...");
            
            try
            {
                var testObj = new GameObject("TestPieceIconUI");
                var pieceIconUI = testObj.AddComponent<PieceIconUI>();
                
                var testPiece = new TestGamePiece();
                pieceIconUI.Initialize(testPiece);
                pieceIconUI.SetSelected(true);
                pieceIconUI.SetEnabled(false);
                pieceIconUI.SetPieceColor(Color.blue);
                pieceIconUI.RefreshDisplay();
                
                DestroyImmediate(testObj);
                
                Debug.Log("✅ PieceIconUI验证通过");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ PieceIconUI验证失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 验证PlayerResultUI
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool VerifyPlayerResultUI()
        {
            Debug.Log("验证PlayerResultUI...");
            
            try
            {
                var testObj = new GameObject("TestPlayerResultUI");
                var playerResultUI = testObj.AddComponent<PlayerResultUI>();
                
                var playerResult = new BlokusGame.Core.Data.PlayerResult
                {
                    playerId = 0,
                    playerName = "测试玩家",
                    playerColor = Color.blue,
                    finalScore = 200,
                    placedPieces = 18,
                    remainingPieces = 3,
                    isAI = false
                };
                
                playerResultUI.Initialize(playerResult, 1);
                playerResultUI.UpdateResult(playerResult, 1);
                
                DestroyImmediate(testObj);
                
                Debug.Log("✅ PlayerResultUI验证通过");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ PlayerResultUI验证失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 验证PieceInventoryUI
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool VerifyPieceInventoryUI()
        {
            Debug.Log("验证PieceInventoryUI...");
            
            try
            {
                var testObj = new GameObject("TestPieceInventoryUI");
                var inventoryUI = testObj.AddComponent<PieceInventoryUI>();
                
                var testPieces = new List<_IGamePiece> { new TestGamePiece() };
                inventoryUI.Initialize(0, testPieces);
                inventoryUI.UpdateInventory(testPieces);
                inventoryUI.SetSelectedPiece(testPieces[0]);
                
                DestroyImmediate(testObj);
                
                Debug.Log("✅ PieceInventoryUI验证通过");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ PieceInventoryUI验证失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 验证PieceDetailUI
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool VerifyPieceDetailUI()
        {
            Debug.Log("验证PieceDetailUI...");
            
            try
            {
                var testObj = new GameObject("TestPieceDetailUI");
                var detailUI = testObj.AddComponent<PieceDetailUI>();
                
                var testPiece = new TestGamePiece();
                detailUI.ShowPieceDetail(testPiece);
                detailUI.RefreshDisplay();
                detailUI.Hide();
                
                DestroyImmediate(testObj);
                
                Debug.Log("✅ PieceDetailUI验证通过");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ PieceDetailUI验证失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 验证所有UI组件
        /// </summary>
        private void VerifyAllUIComponents()
        {
            Debug.Log("=== 验证所有UI组件 ===");
            
            var uiComponents = new System.Type[]
            {
                typeof(PlayerInfoUI),
                typeof(PieceIconUI),
                typeof(PlayerResultUI),
                typeof(PieceInventoryUI),
                typeof(PieceDetailUI)
            };
            
            foreach (var componentType in uiComponents)
            {
                Debug.Log($"检查组件类型: {componentType.Name}");
            }
            
            Debug.Log("✅ 所有UI组件类型验证完成");
        }
        
        /// <summary>
        /// 验证接口实现
        /// </summary>
        private void VerifyInterfaceImplementations()
        {
            Debug.Log("=== 验证接口实现 ===");
            
            try
            {
                var interfaceType = typeof(_IGamePiece);
                var methods = interfaceType.GetMethods();
                
                Debug.Log($"_IGamePiece接口包含{methods.Length}个方法:");
                foreach (var method in methods)
                {
                    Debug.Log($"- {method.Name}");
                }
                
                Debug.Log("✅ 接口实现验证完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 接口实现验证失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 测试用的游戏方块类
        /// </summary>
        private class TestGamePiece : _IGamePiece
        {
            public int pieceId { get; set; } = 1;
            public int playerId { get; set; } = 0;
            public Vector2Int[] currentShape { get; set; } = new Vector2Int[] { Vector2Int.zero };
            public Color pieceColor { get; set; } = Color.white;
            public bool isPlaced { get; set; } = false;
            
            public void rotate90Clockwise() { }
            public void flipHorizontal() { }
            public List<Vector2Int> getOccupiedPositions(Vector2Int _position) 
            { 
                return new List<Vector2Int> { _position }; 
            }
            public void setPlacedState(bool _placed) { isPlaced = _placed; }
            public void resetToOriginalState() { }
            public int getSize() { return currentShape?.Length ?? 0; }
        }
    }
}