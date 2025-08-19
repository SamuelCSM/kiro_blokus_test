using UnityEngine;
using UnityEditor;
using BlokusGame.Core.Player;
using BlokusGame.Core.Data;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 人类玩家测试验证工具
    /// 用于验证HumanPlayerAdvancedTest测试用例的编译和基本功能
    /// </summary>
    public class HumanPlayerTestVerification : MonoBehaviour
    {
        /// <summary>
        /// 验证人类玩家高级测试的编译和基本功能
        /// </summary>
        [MenuItem("Tools/Blokus/Verify Human Player Advanced Test")]
        public static void verifyHumanPlayerAdvancedTest()
        {
            Debug.Log("[HumanPlayerTestVerification] 开始验证人类玩家高级测试...");
            
            bool allTestsPassed = true;
            
            try
            {
                // 测试1: 验证HumanPlayer类可以正常创建
                GameObject testObj = new GameObject("TestHumanPlayer");
                HumanPlayer humanPlayer = testObj.AddComponent<HumanPlayer>();
                
                if (humanPlayer == null)
                {
                    Debug.LogError("[HumanPlayerTestVerification] 无法创建HumanPlayer组件");
                    allTestsPassed = false;
                }
                else
                {
                    Debug.Log("[HumanPlayerTestVerification] ✅ HumanPlayer组件创建成功");
                }
                
                // 测试2: 验证基本初始化
                humanPlayer.initializePlayer(1, "测试玩家", Color.blue);
                
                if (humanPlayer.playerId != 1 || humanPlayer.playerName != "测试玩家")
                {
                    Debug.LogError("[HumanPlayerTestVerification] 基本初始化失败");
                    allTestsPassed = false;
                }
                else
                {
                    Debug.Log("[HumanPlayerTestVerification] ✅ 基本初始化成功");
                }
                
                // 测试3: 验证PlayerData初始化
                var playerData = new PlayerData(99, "PlayerData测试", Color.red, PlayerType.Human);
                humanPlayer.initialize(playerData);
                
                if (humanPlayer.playerId != 99 || humanPlayer.playerName != "PlayerData测试")
                {
                    Debug.LogError("[HumanPlayerTestVerification] PlayerData初始化失败");
                    allTestsPassed = false;
                }
                else
                {
                    Debug.Log("[HumanPlayerTestVerification] ✅ PlayerData初始化成功");
                }
                
                // 测试4: 验证交互方法不抛出异常
                try
                {
                    humanPlayer.onPieceClicked(null);
                    humanPlayer.onPieceDragStart(null, Vector3.zero);
                    humanPlayer.onPieceDragging(Vector3.one);
                    humanPlayer.onPieceDragEnd(Vector3.zero, Vector2Int.zero);
                    humanPlayer.undoLastAction();
                    humanPlayer.confirmPendingAction();
                    humanPlayer.cancelPendingAction();
                    
                    Debug.Log("[HumanPlayerTestVerification] ✅ 交互方法调用成功");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[HumanPlayerTestVerification] 交互方法调用失败: {ex.Message}");
                    allTestsPassed = false;
                }
                
                // 测试5: 验证重置功能
                humanPlayer.resetPlayer();
                
                if (!humanPlayer.isActive)
                {
                    Debug.LogError("[HumanPlayerTestVerification] 重置后状态不正确");
                    allTestsPassed = false;
                }
                else
                {
                    Debug.Log("[HumanPlayerTestVerification] ✅ 重置功能正常");
                }
                
                // 清理测试对象
                DestroyImmediate(testObj);
                
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[HumanPlayerTestVerification] 验证过程中发生异常: {ex.Message}");
                allTestsPassed = false;
            }
            
            // 输出最终结果
            if (allTestsPassed)
            {
                Debug.Log("[HumanPlayerTestVerification] 🎉 所有验证测试通过！HumanPlayerAdvancedTest应该可以正常运行");
            }
            else
            {
                Debug.LogError("[HumanPlayerTestVerification] ❌ 部分验证测试失败，请检查相关问题");
            }
        }
        
        /// <summary>
        /// 验证测试用例的编译完整性
        /// </summary>
        [MenuItem("Tools/Blokus/Verify Test Compilation")]
        public static void verifyTestCompilation()
        {
            Debug.Log("[HumanPlayerTestVerification] 开始验证测试编译完整性...");
            
            // 检查关键类型是否存在
            bool compilationValid = true;
            
            // 检查HumanPlayer类
            var humanPlayerType = typeof(HumanPlayer);
            if (humanPlayerType == null)
            {
                Debug.LogError("[HumanPlayerTestVerification] HumanPlayer类型未找到");
                compilationValid = false;
            }
            
            // 检查PlayerData类
            var playerDataType = typeof(PlayerData);
            if (playerDataType == null)
            {
                Debug.LogError("[HumanPlayerTestVerification] PlayerData类型未找到");
                compilationValid = false;
            }
            
            // 检查PlayerType枚举
            var playerTypeType = typeof(PlayerType);
            if (playerTypeType == null)
            {
                Debug.LogError("[HumanPlayerTestVerification] PlayerType枚举未找到");
                compilationValid = false;
            }
            
            // 检查关键方法是否存在
            try
            {
                var initializeMethod = humanPlayerType.GetMethod("initialize", new[] { typeof(PlayerData) });
                if (initializeMethod == null)
                {
                    Debug.LogError("[HumanPlayerTestVerification] HumanPlayer.initialize(PlayerData)方法未找到");
                    compilationValid = false;
                }
                
                var undoMethod = humanPlayerType.GetMethod("undoLastAction");
                if (undoMethod == null)
                {
                    Debug.LogError("[HumanPlayerTestVerification] HumanPlayer.undoLastAction()方法未找到");
                    compilationValid = false;
                }
                
                var confirmMethod = humanPlayerType.GetMethod("confirmPendingAction");
                if (confirmMethod == null)
                {
                    Debug.LogError("[HumanPlayerTestVerification] HumanPlayer.confirmPendingAction()方法未找到");
                    compilationValid = false;
                }
                
                var cancelMethod = humanPlayerType.GetMethod("cancelPendingAction");
                if (cancelMethod == null)
                {
                    Debug.LogError("[HumanPlayerTestVerification] HumanPlayer.cancelPendingAction()方法未找到");
                    compilationValid = false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[HumanPlayerTestVerification] 方法检查时发生异常: {ex.Message}");
                compilationValid = false;
            }
            
            if (compilationValid)
            {
                Debug.Log("[HumanPlayerTestVerification] ✅ 测试编译完整性验证通过");
            }
            else
            {
                Debug.LogError("[HumanPlayerTestVerification] ❌ 测试编译完整性验证失败");
            }
        }
    }
}