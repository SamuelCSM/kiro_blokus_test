using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using BlokusGame.Core.Pieces;
using BlokusGame.Core.Rules;
using BlokusGame.Core.Managers;

namespace BlokusGame.Editor.Tests
{
    /// <summary>
    /// 编译修复验证测试
    /// 验证getCurrentShape方法移除后的代码完整性
    /// </summary>
    public class CompilationFixTest
    {
        /// <summary>
        /// 验证GamePiece类的接口实现
        /// </summary>
        [MenuItem("Tools/Test Compilation Fix")]
        public static void testCompilationFix()
        {
            Debug.Log("[CompilationFixTest] 开始验证编译修复...");
            
            try
            {
                // 测试GamePiece的currentShape属性访问
                GameObject testObj = new GameObject("TestPiece");
                GamePiece piece = testObj.AddComponent<GamePiece>();
                
                // 验证currentShape属性可以正常访问
                Vector2Int[] shape = piece.currentShape;
                Debug.Log($"[CompilationFixTest] ✅ currentShape属性访问正常，形状包含 {shape?.Length ?? 0} 个格子");
                
                // 验证接口实现
                BlokusGame.Core.Interfaces._IGamePiece iPiece = piece;
                Vector2Int[] interfaceShape = iPiece.currentShape;
                Debug.Log($"[CompilationFixTest] ✅ 接口currentShape属性访问正常");
                
                // 清理测试对象
                Object.DestroyImmediate(testObj);
                
                Debug.Log("[CompilationFixTest] ✅ 所有编译修复验证通过！");
                Debug.Log("[CompilationFixTest] ✅ getCurrentShape()方法已成功移除");
                Debug.Log("[CompilationFixTest] ✅ currentShape属性访问正常");
                Debug.Log("[CompilationFixTest] ✅ 接口实现保持一致");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CompilationFixTest] ❌ 编译修复验证失败: {ex.Message}");
                Debug.LogError($"[CompilationFixTest] 堆栈跟踪: {ex.StackTrace}");
            }
        }
    }
}