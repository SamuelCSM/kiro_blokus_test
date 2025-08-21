using UnityEngine;
using UnityEditor;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.UI;
using System.Collections.Generic;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 编译错误修复工具
    /// 用于检查和修复编译错误
    /// </summary>
    public class CompilationErrorFix : EditorWindow
    {
        [MenuItem("Blokus/修复/编译错误检查")]
        public static void ShowWindow()
        {
            GetWindow<CompilationErrorFix>("编译错误检查");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("编译错误检查和修复", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("测试_IGamePiece接口"))
            {
                TestIGamePieceInterface();
            }
            
            if (GUILayout.Button("测试UI组件编译"))
            {
                TestUIComponentsCompilation();
            }
        }
        
        /// <summary>
        /// 测试_IGamePiece接口
        /// </summary>
        private void TestIGamePieceInterface()
        {
            Debug.Log("=== 测试_IGamePiece接口 ===");
            
            try
            {
                // 创建测试方块
                var testPiece = new TestGamePiece();
                
                // 测试所有接口方法
                Debug.Log($"方块ID: {testPiece.pieceId}");
                Debug.Log($"玩家ID: {testPiece.playerId}");
                Debug.Log($"方块颜色: {testPiece.pieceColor}");
                Debug.Log($"是否放置: {testPiece.isPlaced}");
                Debug.Log($"方块大小: {testPiece.getSize()}");
                
                // 测试方法调用
                testPiece.rotate90Clockwise();
                testPiece.flipHorizontal();
                testPiece.setPlacedState(true);
                testPiece.resetToOriginalState();
                
                var positions = testPiece.getOccupiedPositions(Vector2Int.zero);
                Debug.Log($"占用位置数量: {positions.Count}");
                
                Debug.Log("✅ _IGamePiece接口测试通过");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ _IGamePiece接口测试失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 测试UI组件编译
        /// </summary>
        private void TestUIComponentsCompilation()
        {
            Debug.Log("=== 测试UI组件编译 ===");
            
            try
            {
                // 测试PieceIconUI
                var iconObj = new GameObject("TestPieceIconUI");
                var pieceIconUI = iconObj.AddComponent<PieceIconUI>();
                
                var testPiece = new TestGamePiece();
                pieceIconUI.Initialize(testPiece);
                
                DestroyImmediate(iconObj);
                Debug.Log("✅ PieceIconUI编译测试通过");
                
                // 测试PieceInventoryUI
                var inventoryObj = new GameObject("TestPieceInventoryUI");
                var inventoryUI = inventoryObj.AddComponent<PieceInventoryUI>();
                
                var pieces = new List<_IGamePiece> { testPiece };
                inventoryUI.Initialize(0, pieces);
                
                DestroyImmediate(inventoryObj);
                Debug.Log("✅ PieceInventoryUI编译测试通过");
                
                // 测试PieceDetailUI
                var detailObj = new GameObject("TestPieceDetailUI");
                var detailUI = detailObj.AddComponent<PieceDetailUI>();
                
                detailUI.ShowPieceDetail(testPiece);
                
                DestroyImmediate(detailObj);
                Debug.Log("✅ PieceDetailUI编译测试通过");
                
                Debug.Log("✅ 所有UI组件编译测试通过");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ UI组件编译测试失败: {e.Message}");
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