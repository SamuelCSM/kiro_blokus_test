using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 新系统编译检查工具
    /// 验证新创建的ScoreSystem和AudioManager是否能正常编译
    /// </summary>
    public class NewSystemsCompilationCheck : EditorWindow
    {
        [MenuItem("Blokus/验证工具/新系统编译检查")]
        public static void ShowWindow()
        {
            GetWindow<NewSystemsCompilationCheck>("新系统编译检查");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("新系统编译检查", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("执行完整编译检查"))
            {
                PerformFullCompilationCheck();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("检查项目:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("1. 检查ScoreSystem"))
            {
                CheckScoreSystem();
            }
            
            if (GUILayout.Button("2. 检查AudioManager"))
            {
                CheckAudioManager();
            }
            
            if (GUILayout.Button("3. 检查TouchInputManager"))
            {
                CheckTouchInputManager();
            }
            
            if (GUILayout.Button("4. 检查TouchFeedbackSystem"))
            {
                CheckTouchFeedbackSystem();
            }
            
            if (GUILayout.Button("5. 检查GameRecordManager"))
            {
                CheckGameRecordManager();
            }
        }
        
        private void PerformFullCompilationCheck()
        {
            Debug.Log("=== 开始新系统编译检查 ===");
            
            CheckScoreSystem();
            CheckAudioManager();
            CheckTouchInputManager();
            CheckTouchFeedbackSystem();
            CheckGameRecordManager();
            
            Debug.Log("=== 新系统编译检查完成 ===");
        }
        
        private void CheckScoreSystem()
        {
            Debug.Log("--- 检查 ScoreSystem ---");
            
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.Scoring.ScoreSystem");
                if (type != null)
                {
                    Debug.Log("✅ ScoreSystem 类型存在");
                    
                    // 检查PlayerScore嵌套类
                    var playerScoreType = type.GetNestedType("PlayerScore");
                    if (playerScoreType != null)
                    {
                        Debug.Log("✅ PlayerScore 嵌套类存在");
                        
                        // 检查关键字段
                        var fields = playerScoreType.GetFields();
                        bool hasPlayerId = System.Array.Exists(fields, f => f.Name == "playerId");
                        bool hasTotalScore = System.Array.Exists(fields, f => f.Name == "totalScore");
                        bool hasRank = System.Array.Exists(fields, f => f.Name == "rank");
                        
                        if (hasPlayerId && hasTotalScore && hasRank)
                        {
                            Debug.Log("✅ PlayerScore 关键字段存在");
                        }
                        else
                        {
                            Debug.LogError("❌ PlayerScore 缺少关键字段");
                        }
                    }
                    else
                    {
                        Debug.LogError("❌ PlayerScore 嵌套类未找到");
                    }
                    
                    // 检查关键方法
                    var calculateMethod = type.GetMethod("calculatePlayerScore");
                    if (calculateMethod != null)
                    {
                        Debug.Log("✅ calculatePlayerScore 方法存在");
                    }
                    else
                    {
                        Debug.LogError("❌ calculatePlayerScore 方法未找到");
                    }
                    
                    var finalScoresMethod = type.GetMethod("calculateFinalScores");
                    if (finalScoresMethod != null)
                    {
                        Debug.Log("✅ calculateFinalScores 方法存在");
                    }
                    else
                    {
                        Debug.LogError("❌ calculateFinalScores 方法未找到");
                    }
                }
                else
                {
                    Debug.LogError("❌ ScoreSystem 类型未找到");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ ScoreSystem 检查失败: {e.Message}");
            }
        }
        
        private void CheckAudioManager()
        {
            Debug.Log("--- 检查 AudioManager ---");
            
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.Audio.AudioManager");
                if (type != null)
                {
                    Debug.Log("✅ AudioManager 类型存在");
                    
                    // 检查SoundType枚举
                    var soundType = type.GetNestedType("SoundType");
                    if (soundType != null)
                    {
                        var values = System.Enum.GetNames(soundType);
                        Debug.Log($"✅ SoundType 枚举值: {string.Join(", ", values)}");
                        
                        bool hasGameSound = System.Array.Exists(values, v => v == "GameSound");
                        bool hasUISound = System.Array.Exists(values, v => v == "UISound");
                        bool hasBackgroundMusic = System.Array.Exists(values, v => v == "BackgroundMusic");
                        
                        if (hasGameSound && hasUISound && hasBackgroundMusic)
                        {
                            Debug.Log("✅ SoundType 枚举值完整");
                        }
                        else
                        {
                            Debug.LogError("❌ SoundType 枚举值不完整");
                        }
                    }
                    else
                    {
                        Debug.LogError("❌ SoundType 枚举未找到");
                    }
                    
                    // 检查关键方法
                    var playMethods = type.GetMethods().Where(m => m.Name == "playSound");
                    if (playMethods.Any())
                    {
                        Debug.Log("✅ playSound 方法存在");
                    }
                    else
                    {
                        Debug.LogError("❌ playSound 方法未找到");
                    }
                    
                    var backgroundMusicMethod = type.GetMethod("playBackgroundMusic");
                    if (backgroundMusicMethod != null)
                    {
                        Debug.Log("✅ playBackgroundMusic 方法存在");
                    }
                    else
                    {
                        Debug.LogError("❌ playBackgroundMusic 方法未找到");
                    }
                }
                else
                {
                    Debug.LogError("❌ AudioManager 类型未找到");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ AudioManager 检查失败: {e.Message}");
            }
        }
        
        private void CheckTouchInputManager()
        {
            Debug.Log("--- 检查 TouchInputManager ---");
            
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.Managers.TouchInputManager");
                if (type != null)
                {
                    Debug.Log("✅ TouchInputManager 类型存在");
                    
                    // 检查TouchState枚举
                    var touchState = type.GetNestedType("TouchState");
                    if (touchState != null)
                    {
                        var values = System.Enum.GetNames(touchState);
                        Debug.Log($"✅ TouchState 枚举值: {string.Join(", ", values)}");
                    }
                    else
                    {
                        Debug.LogError("❌ TouchState 枚举未找到");
                    }
                    
                    // 检查关键方法
                    var setInputMethod = type.GetMethod("setInputEnabled");
                    if (setInputMethod != null)
                    {
                        Debug.Log("✅ setInputEnabled 方法存在");
                    }
                    else
                    {
                        Debug.LogError("❌ setInputEnabled 方法未找到");
                    }
                }
                else
                {
                    Debug.LogError("❌ TouchInputManager 类型未找到");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ TouchInputManager 检查失败: {e.Message}");
            }
        }
        
        private void CheckTouchFeedbackSystem()
        {
            Debug.Log("--- 检查 TouchFeedbackSystem ---");
            
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.InputSystem.TouchFeedbackSystem");
                if (type != null)
                {
                    Debug.Log("✅ TouchFeedbackSystem 类型存在");
                    
                    // 检查FeedbackType枚举
                    var feedbackType = type.GetNestedType("FeedbackType");
                    if (feedbackType != null)
                    {
                        var values = System.Enum.GetNames(feedbackType);
                        Debug.Log($"✅ FeedbackType 枚举值: {string.Join(", ", values)}");
                    }
                    else
                    {
                        Debug.LogError("❌ FeedbackType 枚举未找到");
                    }
                    
                    // 检查关键方法
                    var showTouchMethod = type.GetMethod("showTouchPoint");
                    if (showTouchMethod != null)
                    {
                        Debug.Log("✅ showTouchPoint 方法存在");
                    }
                    else
                    {
                        Debug.LogError("❌ showTouchPoint 方法未找到");
                    }
                    
                    var hapticMethod = type.GetMethod("playHapticFeedback");
                    if (hapticMethod != null)
                    {
                        Debug.Log("✅ playHapticFeedback 方法存在");
                    }
                    else
                    {
                        Debug.LogError("❌ playHapticFeedback 方法未找到");
                    }
                }
                else
                {
                    Debug.LogError("❌ TouchFeedbackSystem 类型未找到");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ TouchFeedbackSystem 检查失败: {e.Message}");
            }
        }
    
        private void CheckGameRecordManager()
        {
            Debug.Log("--- 检查 GameRecordManager ---");
            
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.Managers.GameRecordManager");
                if (type != null)
                {
                    Debug.Log("✅ GameRecordManager 类型存在");
                    
                    // 检查关键方法
                    var startMethod = type.GetMethod("startNewGameRecord");
                    if (startMethod != null)
                    {
                        Debug.Log("✅ startNewGameRecord 方法存在");
                    }
                    else
                    {
                        Debug.LogError("❌ startNewGameRecord 方法未找到");
                    }
                    
                    var completeMethod = type.GetMethod("completeCurrentGameRecord");
                    if (completeMethod != null)
                    {
                        Debug.Log("✅ completeCurrentGameRecord 方法存在");
                    }
                    else
                    {
                        Debug.LogError("❌ completeCurrentGameRecord 方法未找到");
                    }
                    
                    var getRecordsMethod = type.GetMethod("getAllGameRecords");
                    if (getRecordsMethod != null)
                    {
                        Debug.Log("✅ getAllGameRecords 方法存在");
                    }
                    else
                    {
                        Debug.LogError("❌ getAllGameRecords 方法未找到");
                    }
                }
                else
                {
                    Debug.LogError("❌ GameRecordManager 类型未找到");
                }
                
                // 检查GameRecord类型
                var gameRecordType = System.Type.GetType("BlokusGame.Core.Data.GameRecord");
                if (gameRecordType != null)
                {
                    Debug.Log("✅ GameRecord 类型存在");
                    
                    // 检查关键字段
                    var fields = gameRecordType.GetFields();
                    bool hasGameId = System.Array.Exists(fields, f => f.Name == "gameId");
                    bool hasPlayerRecords = System.Array.Exists(fields, f => f.Name == "playerRecords");
                    bool hasWinnerId = System.Array.Exists(fields, f => f.Name == "winnerId");
                    
                    if (hasGameId && hasPlayerRecords && hasWinnerId)
                    {
                        Debug.Log("✅ GameRecord 关键字段存在");
                    }
                    else
                    {
                        Debug.LogError("❌ GameRecord 缺少关键字段");
                    }
                }
                else
                {
                    Debug.LogError("❌ GameRecord 类型未找到");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ GameRecordManager 检查失败: {e.Message}");
            }
        }
    }
}