using UnityEngine;
using UnityEditor;
using System.Reflection;
using BlokusGame.Core.Player;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Data;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 玩家系统编译验证工具
    /// 验证所有Player相关类的编译状态和接口实现
    /// </summary>
    public class PlayerSystemCompilationVerification
    {
        /// <summary>
        /// 验证Player系统编译状态
        /// </summary>
        [MenuItem("Tools/Blokus/Verify Player System Compilation")]
        public static void verifyPlayerSystemCompilation()
        {
            Debug.Log("=== 开始验证Player系统编译状态 ===");
            
            bool allTestsPassed = true;
            
            // 验证基础类编译
            allTestsPassed &= _verifyClassCompilation();
            
            // 验证接口实现
            allTestsPassed &= _verifyInterfaceImplementation();
            
            // 验证方法签名
            allTestsPassed &= _verifyMethodSignatures();
            
            // 验证继承关系
            allTestsPassed &= _verifyInheritanceRelationships();
            
            // 验证事件系统集成
            allTestsPassed &= _verifyEventSystemIntegration();
            
            if (allTestsPassed)
            {
                Debug.Log("✅ Player系统编译验证全部通过！");
                EditorUtility.DisplayDialog("验证成功", "Player系统编译验证全部通过！", "确定");
            }
            else
            {
                Debug.LogError("❌ Player系统编译验证发现问题，请检查上述错误信息");
                EditorUtility.DisplayDialog("验证失败", "Player系统编译验证发现问题，请检查控制台输出", "确定");
            }
            
            Debug.Log("=== Player系统编译验证完成 ===");
        }
        
        /// <summary>
        /// 验证类编译状态
        /// </summary>
        /// <returns>是否验证通过</returns>
        private static bool _verifyClassCompilation()
        {
            Debug.Log("--- 验证类编译状态 ---");
            
            bool success = true;
            
            try
            {
                // 验证Player类
                var playerType = typeof(Player);
                Debug.Log($"✅ Player类编译成功: {playerType.FullName}");
                
                // 验证AIPlayer类
                var aiPlayerType = typeof(AIPlayer);
                Debug.Log($"✅ AIPlayer类编译成功: {aiPlayerType.FullName}");
                
                // 验证HumanPlayer类
                var humanPlayerType = typeof(HumanPlayer);
                Debug.Log($"✅ HumanPlayer类编译成功: {humanPlayerType.FullName}");
                
                // 验证接口
                var iPlayerType = typeof(_IPlayer);
                Debug.Log($"✅ _IPlayer接口编译成功: {iPlayerType.FullName}");
                
                var iAIPlayerType = typeof(_IAIPlayer);
                Debug.Log($"✅ _IAIPlayer接口编译成功: {iAIPlayerType.FullName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 类编译验证失败: {ex.Message}");
                success = false;
            }
            
            return success;
        }
        
        /// <summary>
        /// 验证接口实现
        /// </summary>
        /// <returns>是否验证通过</returns>
        private static bool _verifyInterfaceImplementation()
        {
            Debug.Log("--- 验证接口实现 ---");
            
            bool success = true;
            
            try
            {
                // 验证Player实现_IPlayer
                if (typeof(_IPlayer).IsAssignableFrom(typeof(Player)))
                {
                    Debug.Log("✅ Player类正确实现_IPlayer接口");
                }
                else
                {
                    Debug.LogError("❌ Player类未正确实现_IPlayer接口");
                    success = false;
                }
                
                // 验证AIPlayer实现_IPlayer和_IAIPlayer
                if (typeof(_IPlayer).IsAssignableFrom(typeof(AIPlayer)))
                {
                    Debug.Log("✅ AIPlayer类正确实现_IPlayer接口");
                }
                else
                {
                    Debug.LogError("❌ AIPlayer类未正确实现_IPlayer接口");
                    success = false;
                }
                
                if (typeof(_IAIPlayer).IsAssignableFrom(typeof(AIPlayer)))
                {
                    Debug.Log("✅ AIPlayer类正确实现_IAIPlayer接口");
                }
                else
                {
                    Debug.LogError("❌ AIPlayer类未正确实现_IAIPlayer接口");
                    success = false;
                }
                
                // 验证HumanPlayer继承Player
                if (typeof(Player).IsAssignableFrom(typeof(HumanPlayer)))
                {
                    Debug.Log("✅ HumanPlayer类正确继承Player类");
                }
                else
                {
                    Debug.LogError("❌ HumanPlayer类未正确继承Player类");
                    success = false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 接口实现验证失败: {ex.Message}");
                success = false;
            }
            
            return success;
        }
        
        /// <summary>
        /// 验证方法签名
        /// </summary>
        /// <returns>是否验证通过</returns>
        private static bool _verifyMethodSignatures()
        {
            Debug.Log("--- 验证方法签名 ---");
            
            bool success = true;
            
            try
            {
                var playerType = typeof(Player);
                
                // 验证_IPlayer接口方法
                var initializeMethod = playerType.GetMethod("initializePlayer");
                if (initializeMethod != null)
                {
                    Debug.Log("✅ initializePlayer方法存在");
                }
                else
                {
                    Debug.LogError("❌ initializePlayer方法不存在");
                    success = false;
                }
                
                var usePieceMethod = playerType.GetMethod("usePiece");
                if (usePieceMethod != null)
                {
                    Debug.Log("✅ usePiece方法存在");
                }
                else
                {
                    Debug.LogError("❌ usePiece方法不存在");
                    success = false;
                }
                
                var calculateScoreMethod = playerType.GetMethod("calculateScore");
                if (calculateScoreMethod != null)
                {
                    Debug.Log("✅ calculateScore方法存在");
                }
                else
                {
                    Debug.LogError("❌ calculateScore方法不存在");
                    success = false;
                }
                
                var resetPlayerMethod = playerType.GetMethod("resetPlayer");
                if (resetPlayerMethod != null)
                {
                    Debug.Log("✅ resetPlayer方法存在");
                }
                else
                {
                    Debug.LogError("❌ resetPlayer方法不存在");
                    success = false;
                }
                
                // 验证AIPlayer特有方法
                var aiPlayerType = typeof(AIPlayer);
                
                var setDifficultyMethod = aiPlayerType.GetMethod("setDifficulty");
                if (setDifficultyMethod != null)
                {
                    Debug.Log("✅ setDifficulty方法存在");
                }
                else
                {
                    Debug.LogError("❌ setDifficulty方法不存在");
                    success = false;
                }
                
                var makeMoveMethod = aiPlayerType.GetMethod("makeMove");
                if (makeMoveMethod != null)
                {
                    Debug.Log("✅ makeMove方法存在");
                }
                else
                {
                    Debug.LogError("❌ makeMove方法不存在");
                    success = false;
                }
                
                var getBestMoveMethod = aiPlayerType.GetMethod("getBestMove");
                if (getBestMoveMethod != null)
                {
                    Debug.Log("✅ getBestMove方法存在");
                }
                else
                {
                    Debug.LogError("❌ getBestMove方法不存在");
                    success = false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 方法签名验证失败: {ex.Message}");
                success = false;
            }
            
            return success;
        }
        
        /// <summary>
        /// 验证继承关系
        /// </summary>
        /// <returns>是否验证通过</returns>
        private static bool _verifyInheritanceRelationships()
        {
            Debug.Log("--- 验证继承关系 ---");
            
            bool success = true;
            
            try
            {
                // 验证Player继承MonoBehaviour
                if (typeof(MonoBehaviour).IsAssignableFrom(typeof(Player)))
                {
                    Debug.Log("✅ Player类正确继承MonoBehaviour");
                }
                else
                {
                    Debug.LogError("❌ Player类未正确继承MonoBehaviour");
                    success = false;
                }
                
                // 验证AIPlayer继承Player
                if (typeof(Player).IsAssignableFrom(typeof(AIPlayer)))
                {
                    Debug.Log("✅ AIPlayer类正确继承Player");
                }
                else
                {
                    Debug.LogError("❌ AIPlayer类未正确继承Player");
                    success = false;
                }
                
                // 验证HumanPlayer继承Player
                if (typeof(Player).IsAssignableFrom(typeof(HumanPlayer)))
                {
                    Debug.Log("✅ HumanPlayer类正确继承Player");
                }
                else
                {
                    Debug.LogError("❌ HumanPlayer类未正确继承Player");
                    success = false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 继承关系验证失败: {ex.Message}");
                success = false;
            }
            
            return success;
        }
        
        /// <summary>
        /// 验证事件系统集成
        /// </summary>
        /// <returns>是否验证通过</returns>
        private static bool _verifyEventSystemIntegration()
        {
            Debug.Log("--- 验证事件系统集成 ---");
            
            bool success = true;
            
            try
            {
                var gameEventsType = typeof(BlokusGame.Core.Events.GameEvents);
                
                // 验证静态事件
                var playerSkippedField = gameEventsType.GetField("onPlayerSkipped");
                if (playerSkippedField != null)
                {
                    Debug.Log("✅ onPlayerSkipped事件存在");
                }
                else
                {
                    Debug.LogError("❌ onPlayerSkipped事件不存在");
                    success = false;
                }
                
                var playerStateChangedField = gameEventsType.GetField("onPlayerStateChanged");
                if (playerStateChangedField != null)
                {
                    Debug.Log("✅ onPlayerStateChanged事件存在");
                }
                else
                {
                    Debug.LogError("❌ onPlayerStateChanged事件不存在");
                    success = false;
                }
                
                var turnStartedField = gameEventsType.GetField("onTurnStarted");
                if (turnStartedField != null)
                {
                    Debug.Log("✅ onTurnStarted事件存在");
                }
                else
                {
                    Debug.LogError("❌ onTurnStarted事件不存在");
                    success = false;
                }
                
                var turnEndedField = gameEventsType.GetField("onTurnEnded");
                if (turnEndedField != null)
                {
                    Debug.Log("✅ onTurnEnded事件存在");
                }
                else
                {
                    Debug.LogError("❌ onTurnEnded事件不存在");
                    success = false;
                }
                
                // 验证实例事件
                var gameEventsInstance = BlokusGame.Core.Events.GameEvents.instance;
                if (gameEventsInstance != null)
                {
                    Debug.Log("✅ GameEvents实例可以正常访问");
                }
                else
                {
                    Debug.LogError("❌ GameEvents实例访问失败");
                    success = false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 事件系统集成验证失败: {ex.Message}");
                success = false;
            }
            
            return success;
        }
        
        /// <summary>
        /// 快速编译测试
        /// </summary>
        [MenuItem("Tools/Blokus/Quick Player System Test")]
        public static void quickPlayerSystemTest()
        {
            Debug.Log("=== 快速Player系统测试 ===");
            
            try
            {
                // 创建测试实例
                GameObject testObj = new GameObject("QuickTest");
                
                var player = testObj.AddComponent<Player>();
                var aiPlayer = testObj.AddComponent<AIPlayer>();
                var humanPlayer = testObj.AddComponent<HumanPlayer>();
                
                // 基础功能测试
                player.initializePlayer(1, "测试玩家", Color.red);
                aiPlayer.setDifficulty(_IAIPlayer.AIDifficulty.Hard);
                humanPlayer.initializePlayer(2, "人类玩家", Color.blue);
                
                Debug.Log("✅ 快速测试通过 - 所有类都能正常创建和调用");
                
                // 清理
                Object.DestroyImmediate(testObj);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 快速测试失败: {ex.Message}");
            }
            
            Debug.Log("=== 快速Player系统测试完成 ===");
        }
    }
}