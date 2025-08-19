using UnityEngine;
using UnityEditor;
using BlokusGame.Core.Player;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Data;

namespace BlokusGame.Editor
{
    /// <summary>
    /// AI玩家编译验证脚本
    /// 验证AI玩家系统的编译正确性和接口实现完整性
    /// </summary>
    public class AIPlayerCompilationVerification
    {
        /// <summary>
        /// 验证AI玩家编译状态
        /// </summary>
        [MenuItem("Blokus/验证/AI玩家编译验证")]
        public static void verifyAIPlayerCompilation()
        {
            Debug.Log("=== AI玩家编译验证开始 ===");
            
            bool allTestsPassed = true;
            
            // 测试1: 验证AIPlayer类可以实例化
            try
            {
                GameObject testObj = new GameObject("TestAIPlayer");
                AIPlayer aiPlayer = testObj.AddComponent<AIPlayer>();
                
                if (aiPlayer != null)
                {
                    Debug.Log("✅ AIPlayer类实例化成功");
                }
                else
                {
                    Debug.LogError("❌ AIPlayer类实例化失败");
                    allTestsPassed = false;
                }
                
                Object.DestroyImmediate(testObj);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ AIPlayer实例化异常: {ex.Message}");
                allTestsPassed = false;
            }
            
            // 测试2: 验证接口实现
            try
            {
                GameObject testObj = new GameObject("TestAIPlayer");
                AIPlayer aiPlayer = testObj.AddComponent<AIPlayer>();
                
                // 测试_IPlayer接口
                _IPlayer playerInterface = aiPlayer as _IPlayer;
                if (playerInterface != null)
                {
                    Debug.Log("✅ _IPlayer接口实现正确");
                }
                else
                {
                    Debug.LogError("❌ _IPlayer接口实现失败");
                    allTestsPassed = false;
                }
                
                // 测试_IAIPlayer接口
                _IAIPlayer aiInterface = aiPlayer as _IAIPlayer;
                if (aiInterface != null)
                {
                    Debug.Log("✅ _IAIPlayer接口实现正确");
                }
                else
                {
                    Debug.LogError("❌ _IAIPlayer接口实现失败");
                    allTestsPassed = false;
                }
                
                Object.DestroyImmediate(testObj);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 接口验证异常: {ex.Message}");
                allTestsPassed = false;
            }
            
            // 测试3: 验证AI特有方法
            try
            {
                GameObject testObj = new GameObject("TestAIPlayer");
                AIPlayer aiPlayer = testObj.AddComponent<AIPlayer>();
                
                // 测试难度设置
                aiPlayer.setDifficulty(AIDifficulty.Easy);
                if (aiPlayer.difficulty == AIDifficulty.Easy)
                {
                    Debug.Log("✅ AI难度设置功能正常");
                }
                else
                {
                    Debug.LogError("❌ AI难度设置功能异常");
                    allTestsPassed = false;
                }
                
                // 测试思考时间设置
                aiPlayer.setThinkingTime(1.5f);
                if (aiPlayer.thinkingTime == 1.5f)
                {
                    Debug.Log("✅ AI思考时间设置功能正常");
                }
                else
                {
                    Debug.LogError("❌ AI思考时间设置功能异常");
                    allTestsPassed = false;
                }
                
                Object.DestroyImmediate(testObj);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ AI方法验证异常: {ex.Message}");
                allTestsPassed = false;
            }
            
            // 测试4: 验证枚举引用
            try
            {
                AIDifficulty testDifficulty = AIDifficulty.Medium;
                Debug.Log($"✅ AIDifficulty枚举引用正常: {testDifficulty}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ AIDifficulty枚举引用异常: {ex.Message}");
                allTestsPassed = false;
            }
            
            // 输出最终结果
            if (allTestsPassed)
            {
                Debug.Log("🎉 AI玩家编译验证全部通过！");
            }
            else
            {
                Debug.LogError("💥 AI玩家编译验证存在问题，请检查上述错误信息");
            }
            
            Debug.Log("=== AI玩家编译验证结束 ===");
        }
    }
}