using UnityEngine;
using UnityEditor;
using BlokusGame.Core.Managers;
using BlokusGame.Core.Audio;
using BlokusGame.Core.Scoring;
using BlokusGame.Core.Data;
using BlokusGame.Core.Events;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 快速编译测试 - 验证新系统是否能正常编译
    /// </summary>
    public class QuickCompilationTest : EditorWindow
    {
        [MenuItem("Blokus/验证工具/快速编译测试")]
        public static void ShowWindow()
        {
            GetWindow<QuickCompilationTest>("快速编译测试");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("快速编译测试", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("执行编译测试"))
            {
                PerformCompilationTest();
            }
        }
        
        private void PerformCompilationTest()
        {
            Debug.Log("=== 开始快速编译测试 ===");
            
            try
            {
                // 测试ScoreSystem
                var scoreSystem = typeof(ScoreSystem);
                Debug.Log("✅ ScoreSystem 编译正常");
                
                // 测试AudioManager
                var audioManager = typeof(AudioManager);
                Debug.Log("✅ AudioManager 编译正常");
                
                // 测试GameRecordManager
                var gameRecordManager = typeof(GameRecordManager);
                Debug.Log("✅ GameRecordManager 编译正常");
                
                // 测试GameResults
                var gameResults = typeof(GameResults);
                Debug.Log("✅ GameResults 编译正常");
                
                // 测试GameEvents
                var gameEvents = typeof(GameEvents);
                Debug.Log("✅ GameEvents 编译正常");
                
                // 测试TouchGameplayIntegration
                var touchIntegration = typeof(BlokusGame.Core.InputSystem.TouchGameplayIntegration);
                Debug.Log("✅ TouchGameplayIntegration 编译正常");
                
                Debug.Log("🎉 所有新系统编译测试通过！");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 编译测试失败: {e.Message}");
            }
            
            Debug.Log("=== 快速编译测试完成 ===");
        }
    }
}