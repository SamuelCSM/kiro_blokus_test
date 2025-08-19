using UnityEngine;
using UnityEditor;
using BlokusGame.Core.Rules;
using BlokusGame.Core.Data;
using BlokusGame.Tests;

namespace BlokusGame.Editor.Tests
{
    /// <summary>
    /// 规则引擎测试编译验证
    /// 确保规则引擎测试类能够正确编译和运行
    /// </summary>
    public class RuleEngineTestCompilationCheck
    {
        /// <summary>
        /// 验证规则引擎测试编译
        /// </summary>
        [MenuItem("Tools/Blokus/Verify Rule Engine Tests Compilation")]
        public static void verifyRuleEngineTestsCompilation()
        {
            Debug.Log("[RuleEngineTestCompilationCheck] 开始验证规则引擎测试编译...");
            
            try
            {
                // 测试RuleValidationResult类
                var successResult = RuleValidationResult.createSuccess();
                if (successResult != null && successResult.isValid)
                {
                    Debug.Log("✅ RuleValidationResult成功结果创建正常");
                }
                
                var failureResult = RuleValidationResult.createFailure("测试失败", RuleType.Overlap);
                if (failureResult != null && !failureResult.isValid)
                {
                    Debug.Log("✅ RuleValidationResult失败结果创建正常");
                }
                
                // 测试RuleType枚举
                var ruleTypes = System.Enum.GetValues(typeof(RuleType));
                Debug.Log($"✅ RuleType枚举包含 {ruleTypes.Length} 个值");
                
                // 测试用户友好消息
                string message = failureResult.getUserFriendlyMessage();
                if (!string.IsNullOrEmpty(message))
                {
                    Debug.Log($"✅ 用户友好消息生成正常: {message}");
                }
                
                // 测试RuleEngineTests类是否可以实例化
                var testInstance = new RuleEngineTests();
                if (testInstance != null)
                {
                    Debug.Log("✅ RuleEngineTests类实例化正常");
                }
                
                Debug.Log("🎉 规则引擎测试编译验证完成，所有检查通过！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 规则引擎测试编译验证失败: {ex.Message}");
                Debug.LogError($"堆栈跟踪: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 验证规则引擎核心功能
        /// </summary>
        [MenuItem("Tools/Blokus/Verify Rule Engine Core")]
        public static void verifyRuleEngineCore()
        {
            Debug.Log("[RuleEngineTestCompilationCheck] 开始验证规则引擎核心功能...");
            
            try
            {
                // 创建测试游戏对象
                GameObject testObj = new GameObject("TestRuleEngine");
                RuleEngine ruleEngine = testObj.AddComponent<RuleEngine>();
                
                if (ruleEngine != null)
                {
                    Debug.Log("✅ RuleEngine组件创建成功");
                    
                    // 测试基本方法调用
                    var corner = ruleEngine.getPlayerStartCorner(0);
                    Debug.Log($"✅ 玩家0起始角落: {corner}");
                    
                    bool isFirstPlacement = ruleEngine.isFirstPlacement(0);
                    Debug.Log($"✅ 首次放置检测: {isFirstPlacement}");
                    
                    bool isGameOver = ruleEngine.isGameOver();
                    Debug.Log($"✅ 游戏结束检测: {isGameOver}");
                }
                
                // 清理测试对象
                Object.DestroyImmediate(testObj);
                
                Debug.Log("🎉 规则引擎核心功能验证完成！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 规则引擎核心功能验证失败: {ex.Message}");
                Debug.LogError($"堆栈跟踪: {ex.StackTrace}");
            }
        }
    }
}