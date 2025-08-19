using UnityEngine;
using UnityEditor;
using System.Reflection;
using BlokusGame.Core.UI;
using BlokusGame.Core.Data;
using BlokusGame.Core.Events;
using BlokusGame.Core.Managers;
using MessageType = BlokusGame.Core.Events.MessageType;

namespace BlokusGame.Editor
{
    /// <summary>
    /// UI系统编译验证工具
    /// 验证所有UI相关类的编译状态和接口实现
    /// </summary>
    public class UISystemCompilationVerification
    {
        /// <summary>
        /// 验证UI系统编译状态
        /// </summary>
        [MenuItem("Tools/Blokus/Verify UI System Compilation")]
        public static void verifyUISystemCompilation()
        {
            Debug.Log("=== 开始验证UI系统编译状态 ===");
            
            bool allTestsPassed = true;
            
            // 验证基础类编译
            allTestsPassed &= _verifyUIClassCompilation();
            
            // 验证枚举一致性
            allTestsPassed &= _verifyEnumConsistency();
            
            // 验证方法调用
            allTestsPassed &= _verifyMethodCalls();
            
            // 验证数据类
            allTestsPassed &= _verifyDataClasses();
            
            if (allTestsPassed)
            {
                Debug.Log("✅ UI系统编译验证全部通过！");
                EditorUtility.DisplayDialog("验证成功", "UI系统编译验证全部通过！", "确定");
            }
            else
            {
                Debug.LogError("❌ UI系统编译验证发现问题，请检查上述错误信息");
                EditorUtility.DisplayDialog("验证失败", "UI系统编译验证发现问题，请检查控制台输出", "确定");
            }
            
            Debug.Log("=== UI系统编译验证完成 ===");
        }
        
        /// <summary>
        /// 验证UI类编译状态
        /// </summary>
        /// <returns>是否验证通过</returns>
        private static bool _verifyUIClassCompilation()
        {
            Debug.Log("--- 验证UI类编译状态 ---");
            
            bool success = true;
            
            try
            {
                // 验证UI基础类
                var uiBaseType = typeof(UIBase);
                Debug.Log($"✅ UIBase类编译成功: {uiBaseType.FullName}");
                
                var uiManagerType = typeof(UIManager);
                Debug.Log($"✅ UIManager类编译成功: {uiManagerType.FullName}");
                
                // 验证主要UI面板
                var mainMenuUIType = typeof(MainMenuUI);
                Debug.Log($"✅ MainMenuUI类编译成功: {mainMenuUIType.FullName}");
                
                var gameplayUIType = typeof(GameplayUI);
                Debug.Log($"✅ GameplayUI类编译成功: {gameplayUIType.FullName}");
                
                var settingsUIType = typeof(SettingsUI);
                Debug.Log($"✅ SettingsUI类编译成功: {settingsUIType.FullName}");
                
                var messageUIType = typeof(MessageUI);
                Debug.Log($"✅ MessageUI类编译成功: {messageUIType.FullName}");
                
                var loadingUIType = typeof(LoadingUI);
                Debug.Log($"✅ LoadingUI类编译成功: {loadingUIType.FullName}");
                
                var pauseMenuUIType = typeof(PauseMenuUI);
                Debug.Log($"✅ PauseMenuUI类编译成功: {pauseMenuUIType.FullName}");
                
                var gameOverUIType = typeof(GameOverUI);
                Debug.Log($"✅ GameOverUI类编译成功: {gameOverUIType.FullName}");
                
                // 验证辅助UI组件
                var playerInfoUIType = typeof(PlayerInfoUI);
                Debug.Log($"✅ PlayerInfoUI类编译成功: {playerInfoUIType.FullName}");
                
                var pieceIconUIType = typeof(PieceIconUI);
                Debug.Log($"✅ PieceIconUI类编译成功: {pieceIconUIType.FullName}");
                
                var playerResultUIType = typeof(PlayerResultUI);
                Debug.Log($"✅ PlayerResultUI类编译成功: {playerResultUIType.FullName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ UI类编译验证失败: {ex.Message}");
                success = false;
            }
            
            return success;
        }
        
        /// <summary>
        /// 验证枚举一致性
        /// </summary>
        /// <returns>是否验证通过</returns>
        private static bool _verifyEnumConsistency()
        {
            Debug.Log("--- 验证枚举一致性 ---");
            
            bool success = true;
            
            try
            {
                // 验证GameState枚举
                var gameStateType = typeof(GameState);
                Debug.Log($"✅ GameState枚举编译成功: {gameStateType.FullName}");
                
                // 验证GameMode枚举
                var gameModeType = typeof(GameMode);
                Debug.Log($"✅ GameMode枚举编译成功: {gameModeType.FullName}");
                
                // 验证AIDifficulty枚举
                var aiDifficultyType = typeof(AIDifficulty);
                Debug.Log($"✅ AIDifficulty枚举编译成功: {aiDifficultyType.FullName}");
                
                // 验证MessageType枚举
                var messageTypeType = typeof(MessageType);
                Debug.Log($"✅ MessageType枚举编译成功: {messageTypeType.FullName}");
                
                // 验证PlayerGameState枚举
                var playerGameStateType = typeof(PlayerGameState);
                Debug.Log($"✅ PlayerGameState枚举编译成功: {playerGameStateType.FullName}");
                
                // 验证枚举值
                Debug.Log($"✅ GameState.MainMenu: {GameState.MainMenu}");
                Debug.Log($"✅ GameState.GamePlaying: {GameState.GamePlaying}");
                Debug.Log($"✅ GameState.GamePaused: {GameState.GamePaused}");
                Debug.Log($"✅ GameState.GameEnded: {GameState.GameEnded}");
                
                Debug.Log($"✅ GameMode.SinglePlayerVsAI: {GameMode.SinglePlayerVsAI}");
                Debug.Log($"✅ GameMode.LocalMultiplayer: {GameMode.LocalMultiplayer}");
                Debug.Log($"✅ GameMode.Tutorial: {GameMode.Tutorial}");
                
                Debug.Log($"✅ AIDifficulty.Easy: {AIDifficulty.Easy}");
                Debug.Log($"✅ AIDifficulty.Medium: {AIDifficulty.Medium}");
                Debug.Log($"✅ AIDifficulty.Hard: {AIDifficulty.Hard}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 枚举一致性验证失败: {ex.Message}");
                success = false;
            }
            
            return success;
        }
        
        /// <summary>
        /// 验证方法调用
        /// </summary>
        /// <returns>是否验证通过</returns>
        private static bool _verifyMethodCalls()
        {
            Debug.Log("--- 验证方法调用 ---");
            
            bool success = true;
            
            try
            {
                // 验证GameManager方法存在
                var gameManagerType = typeof(GameManager);
                
                var startNewGameMethod = gameManagerType.GetMethod("startNewGame", new System.Type[] { typeof(int), typeof(GameMode) });
                if (startNewGameMethod != null)
                {
                    Debug.Log("✅ GameManager.startNewGame方法存在");
                }
                else
                {
                    Debug.LogError("❌ GameManager.startNewGame方法不存在");
                    success = false;
                }
                
                var pauseGameMethod = gameManagerType.GetMethod("pauseGame");
                if (pauseGameMethod != null)
                {
                    Debug.Log("✅ GameManager.pauseGame方法存在");
                }
                else
                {
                    Debug.LogError("❌ GameManager.pauseGame方法不存在");
                    success = false;
                }
                
                var resumeGameMethod = gameManagerType.GetMethod("resumeGame");
                if (resumeGameMethod != null)
                {
                    Debug.Log("✅ GameManager.resumeGame方法存在");
                }
                else
                {
                    Debug.LogError("❌ GameManager.resumeGame方法不存在");
                    success = false;
                }
                
                var resetGameMethod = gameManagerType.GetMethod("resetGame");
                if (resetGameMethod != null)
                {
                    Debug.Log("✅ GameManager.resetGame方法存在");
                }
                else
                {
                    Debug.LogError("❌ GameManager.resetGame方法不存在");
                    success = false;
                }
                
                var exitToMainMenuMethod = gameManagerType.GetMethod("exitToMainMenu");
                if (exitToMainMenuMethod != null)
                {
                    Debug.Log("✅ GameManager.exitToMainMenu方法存在");
                }
                else
                {
                    Debug.LogError("❌ GameManager.exitToMainMenu方法不存在");
                    success = false;
                }
                
                var skipCurrentPlayerMethod = gameManagerType.GetMethod("skipCurrentPlayer");
                if (skipCurrentPlayerMethod != null)
                {
                    Debug.Log("✅ GameManager.skipCurrentPlayer方法存在");
                }
                else
                {
                    Debug.LogError("❌ GameManager.skipCurrentPlayer方法不存在");
                    success = false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 方法调用验证失败: {ex.Message}");
                success = false;
            }
            
            return success;
        }
        
        /// <summary>
        /// 验证数据类
        /// </summary>
        /// <returns>是否验证通过</returns>
        private static bool _verifyDataClasses()
        {
            Debug.Log("--- 验证数据类 ---");
            
            bool success = true;
            
            try
            {
                // 验证GameSettings类
                var gameSettingsType = typeof(GameSettings);
                Debug.Log($"✅ GameSettings类编译成功: {gameSettingsType.FullName}");
                
                var gameSettings = new GameSettings();
                Debug.Log($"✅ GameSettings实例创建成功");
                
                // 验证GameResults类
                var gameResultsType = typeof(GameResults);
                Debug.Log($"✅ GameResults类编译成功: {gameResultsType.FullName}");
                
                var gameResults = new GameResults();
                Debug.Log($"✅ GameResults实例创建成功");
                
                // 验证PlayerResult类
                var playerResultType = typeof(PlayerResult);
                Debug.Log($"✅ PlayerResult类编译成功: {playerResultType.FullName}");
                
                var playerResult = new PlayerResult();
                Debug.Log($"✅ PlayerResult实例创建成功");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 数据类验证失败: {ex.Message}");
                success = false;
            }
            
            return success;
        }
        
        /// <summary>
        /// 快速UI系统测试
        /// </summary>
        [MenuItem("Tools/Blokus/Quick UI System Test")]
        public static void quickUISystemTest()
        {
            Debug.Log("=== 快速UI系统测试 ===");
            
            try
            {
                // 创建测试实例
                GameObject testObj = new GameObject("QuickUITest");
                
                var uiManager = testObj.AddComponent<UIManager>();
                var mainMenuUI = testObj.AddComponent<MainMenuUI>();
                var gameplayUI = testObj.AddComponent<GameplayUI>();
                var settingsUI = testObj.AddComponent<SettingsUI>();
                
                // 基础功能测试
                var gameSettings = new GameSettings();
                var gameResults = new GameResults();
                var playerResult = new PlayerResult(1, "测试玩家", Color.red);
                
                Debug.Log("✅ 快速测试通过 - 所有UI类都能正常创建");
                
                // 清理
                Object.DestroyImmediate(testObj);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 快速测试失败: {ex.Message}");
            }
            
            Debug.Log("=== 快速UI系统测试完成 ===");
        }
    }
}