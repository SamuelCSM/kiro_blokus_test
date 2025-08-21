using UnityEngine;
using UnityEditor;
using BlokusGame.Core.Managers;
using BlokusGame.Core.InputSystem;

namespace BlokusGame.Editor
{
    /// <summary>
    /// TouchInputManager编译验证脚本
    /// 验证TouchInputManager类的编译正确性和基本功能
    /// </summary>
    public class TouchInputManagerCompilationVerification
    {
        /// <summary>
        /// 验证TouchInputManager编译
        /// </summary>
        [MenuItem("Blokus/验证/TouchInputManager编译验证Ex")]
        public static void verifyTouchInputManagerCompilation()
        {
            Debug.Log("=== TouchInputManager编译验证开始 ===");
            
            bool allTestsPassed = true;
            
            // 1. 验证类型存在
            if (!_verifyClassExists())
            {
                allTestsPassed = false;
            }
            
            // 2. 验证组件创建
            if (!_verifyComponentCreation())
            {
                allTestsPassed = false;
            }
            
            // 3. 验证公共方法
            if (!_verifyPublicMethods())
            {
                allTestsPassed = false;
            }
            
            // 4. 验证依赖组件
            if (!_verifyDependencies())
            {
                allTestsPassed = false;
            }
            
            // 输出结果
            if (allTestsPassed)
            {
                Debug.Log("✅ TouchInputManager编译验证通过！");
            }
            else
            {
                Debug.LogError("❌ TouchInputManager编译验证失败！");
            }
            
            Debug.Log("=== TouchInputManager编译验证结束 ===");
        }
        
        /// <summary>
        /// 验证类型是否存在
        /// </summary>
        /// <returns>验证是否通过</returns>
        private static bool _verifyClassExists()
        {
            try
            {
                var type = typeof(TouchInputManager);
                Debug.Log($"✅ TouchInputManager类型存在: {type.FullName}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ TouchInputManager类型不存在: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 验证组件创建
        /// </summary>
        /// <returns>验证是否通过</returns>
        private static bool _verifyComponentCreation()
        {
            try
            {
                GameObject testObj = new GameObject("TestTouchInputManager");
                var component = testObj.AddComponent<TouchInputManager>();
                
                if (component != null)
                {
                    Debug.Log("✅ TouchInputManager组件创建成功");
                    
                    // 清理测试对象
                    Object.DestroyImmediate(testObj);
                    return true;
                }
                else
                {
                    Debug.LogError("❌ TouchInputManager组件创建失败");
                    Object.DestroyImmediate(testObj);
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ TouchInputManager组件创建异常: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 验证公共方法
        /// </summary>
        /// <returns>验证是否通过</returns>
        private static bool _verifyPublicMethods()
        {
            try
            {
                var type = typeof(TouchInputManager);
                
                // 检查关键公共方法
                string[] requiredMethods = {
                    "setInputEnabled",
                    "setTouchInputEnabled", 
                    "setMouseInputEnabled",
                    "resetInput",
                    "getCurrentTouchState",
                    "getSelectedPiece"
                };
                
                bool allMethodsExist = true;
                
                foreach (string methodName in requiredMethods)
                {
                    var method = type.GetMethod(methodName);
                    if (method != null)
                    {
                        Debug.Log($"✅ 方法存在: {methodName}");
                    }
                    else
                    {
                        Debug.LogError($"❌ 方法缺失: {methodName}");
                        allMethodsExist = false;
                    }
                }
                
                return allMethodsExist;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 公共方法验证异常: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 验证依赖组件
        /// </summary>
        /// <returns>验证是否通过</returns>
        private static bool _verifyDependencies()
        {
            try
            {
                // 验证TouchFeedbackSystem类型
                var feedbackType = typeof(TouchFeedbackSystem);
                Debug.Log($"✅ TouchFeedbackSystem类型存在: {feedbackType.FullName}");
                
                // 验证TouchState枚举
                var touchStateType = typeof(TouchInputManager.TouchState);
                Debug.Log($"✅ TouchState枚举存在: {touchStateType.FullName}");
                
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ 依赖组件验证失败: {ex.Message}");
                return false;
            }
        }
    }
}