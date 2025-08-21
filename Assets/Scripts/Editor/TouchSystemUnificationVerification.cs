using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 触摸系统统一验证工具
    /// 验证触摸相关枚举和功能的统一性，确保没有重复定义
    /// </summary>
    public class TouchSystemUnificationVerification : EditorWindow
    {
        /// <summary>验证结果文本</summary>
        private string _m_verificationResult = "";
        
        /// <summary>滚动位置</summary>
        private Vector2 _m_scrollPosition;
        
        [MenuItem("Blokus/验证工具/触摸系统统一验证")]
        public static void ShowWindow()
        {
            GetWindow<TouchSystemUnificationVerification>("触摸系统统一验证");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("触摸系统统一验证工具", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("开始验证", GUILayout.Height(30)))
            {
                _performVerification();
            }
            
            GUILayout.Space(10);
            
            _m_scrollPosition = GUILayout.BeginScrollView(_m_scrollPosition);
            GUILayout.TextArea(_m_verificationResult, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
        }
        
        /// <summary>
        /// 执行触摸系统统一验证
        /// </summary>
        private void _performVerification()
        {
            _m_verificationResult = "=== 触摸系统统一验证开始 ===\n\n";
            
            bool allTestsPassed = true;
            
            // 1. 验证TouchState枚举统一
            allTestsPassed &= _verifyTouchStateUnification();
            
            // 2. 验证FeedbackType枚举统一
            allTestsPassed &= _verifyFeedbackTypeUnification();
            
            // 3. 验证TouchStateMapper工具类
            allTestsPassed &= _verifyTouchStateMapper();
            
            // 4. 验证重复功能消除
            allTestsPassed &= _verifyDuplicationElimination();
            
            // 5. 验证代码复用情况
            allTestsPassed &= _verifyCodeReuse();
            
            _m_verificationResult += "\n=== 验证总结 ===\n";
            if (allTestsPassed)
            {
                _m_verificationResult += "✅ 所有验证通过！触摸系统统一完成。\n";
            }
            else
            {
                _m_verificationResult += "❌ 部分验证失败，请检查上述错误。\n";
            }
            
            _m_verificationResult += "=== 触摸系统统一验证结束 ===\n";
        }
        
        /// <summary>
        /// 验证TouchState枚举统一
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyTouchStateUnification()
        {
            _m_verificationResult += "1. 验证TouchState枚举统一\n";
            
            var touchInputType = System.Type.GetType("BlokusGame.Core.Managers.TouchInputManager, Assembly-CSharp");
            if (touchInputType == null)
            {
                _m_verificationResult += "❌ TouchInputManager类未找到\n\n";
                return false;
            }
            
            // 检查TouchState枚举
            var touchStateType = touchInputType.GetNestedType("TouchState");
            if (touchStateType == null)
            {
                _m_verificationResult += "❌ TouchState枚举未找到\n\n";
                return false;
            }
            
            _m_verificationResult += "✅ TouchState枚举存在\n";
            
            // 检查枚举值
            var enumValues = System.Enum.GetNames(touchStateType);
            var expectedValues = new string[] 
            {
                "None", "Tap", "Dragging", "LongPress", "DoubleTap", 
                "MultiTouch", "Pinching", "Rotation", "Pan", "EdgeSwipe"
            };
            
            bool allValuesExist = true;
            foreach (var expectedValue in expectedValues)
            {
                if (System.Array.Exists(enumValues, v => v == expectedValue))
                {
                    _m_verificationResult += $"✅ TouchState.{expectedValue} 存在\n";
                }
                else
                {
                    _m_verificationResult += $"❌ TouchState.{expectedValue} 缺失\n";
                    allValuesExist = false;
                }
            }
            
            // 检查是否还存在GestureType枚举（应该已被移除）
            var gestureTypeType = touchInputType.GetNestedType("GestureType");
            if (gestureTypeType != null)
            {
                _m_verificationResult += "❌ GestureType枚举仍然存在，应该已被移除\n";
                allValuesExist = false;
            }
            else
            {
                _m_verificationResult += "✅ GestureType枚举已正确移除\n";
            }
            
            _m_verificationResult += "\n";
            return allValuesExist;
        }
        
        /// <summary>
        /// 验证FeedbackType枚举统一
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyFeedbackTypeUnification()
        {
            _m_verificationResult += "2. 验证FeedbackType枚举统一\n";
            
            // 检查TouchFeedbackSystem.FeedbackType
            var feedbackSystemType = System.Type.GetType("BlokusGame.Core.InputSystem.TouchFeedbackSystem, Assembly-CSharp");
            if (feedbackSystemType == null)
            {
                _m_verificationResult += "❌ TouchFeedbackSystem类未找到\n\n";
                return false;
            }
            
            var feedbackType = feedbackSystemType.GetNestedType("FeedbackType");
            if (feedbackType == null)
            {
                _m_verificationResult += "❌ TouchFeedbackSystem.FeedbackType枚举未找到\n\n";
                return false;
            }
            
            _m_verificationResult += "✅ TouchFeedbackSystem.FeedbackType枚举存在\n";
            
            // 检查TouchInputManager是否还有TouchFeedbackType（应该已被移除）
            var touchInputType = System.Type.GetType("BlokusGame.Core.Managers.TouchInputManager, Assembly-CSharp");
            var touchFeedbackType = touchInputType?.GetNestedType("TouchFeedbackType");
            
            if (touchFeedbackType != null)
            {
                _m_verificationResult += "❌ TouchInputManager.TouchFeedbackType仍然存在，应该使用TouchFeedbackSystem.FeedbackType\n";
                return false;
            }
            else
            {
                _m_verificationResult += "✅ TouchInputManager.TouchFeedbackType已正确移除\n";
            }
            
            _m_verificationResult += "\n";
            return true;
        }
        
        /// <summary>
        /// 验证TouchStateMapper工具类
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyTouchStateMapper()
        {
            _m_verificationResult += "3. 验证TouchStateMapper工具类\n";
            
            var mapperType = System.Type.GetType("BlokusGame.Core.InputSystem.TouchStateMapper, Assembly-CSharp");
            if (mapperType == null)
            {
                _m_verificationResult += "❌ TouchStateMapper类未找到\n\n";
                return false;
            }
            
            _m_verificationResult += "✅ TouchStateMapper类存在\n";
            
            // 检查关键方法
            var methods = new string[]
            {
                "mapTouchPhaseToState",
                "detectGestureType", 
                "isDoubleTap",
                "getTouchStateDescription"
            };
            
            bool allMethodsExist = true;
            foreach (var methodName in methods)
            {
                var method = mapperType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                if (method != null)
                {
                    _m_verificationResult += $"✅ TouchStateMapper.{methodName} 方法存在\n";
                }
                else
                {
                    _m_verificationResult += $"❌ TouchStateMapper.{methodName} 方法缺失\n";
                    allMethodsExist = false;
                }
            }
            
            _m_verificationResult += "\n";
            return allMethodsExist;
        }
        
        /// <summary>
        /// 验证重复功能消除
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyDuplicationElimination()
        {
            _m_verificationResult += "4. 验证重复功能消除\n";
            
            bool noDuplication = true;
            
            // 检查是否还有重复的触摸状态枚举
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            int touchStateEnumCount = 0;
            int feedbackTypeEnumCount = 0;
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        // 检查嵌套枚举
                        var nestedTypes = type.GetNestedTypes();
                        foreach (var nestedType in nestedTypes)
                        {
                            if (nestedType.IsEnum)
                            {
                                if (nestedType.Name == "TouchState")
                                {
                                    touchStateEnumCount++;
                                    _m_verificationResult += $"发现TouchState枚举: {type.FullName}.{nestedType.Name}\n";
                                }
                                else if (nestedType.Name == "FeedbackType" || nestedType.Name == "TouchFeedbackType")
                                {
                                    feedbackTypeEnumCount++;
                                    _m_verificationResult += $"发现FeedbackType枚举: {type.FullName}.{nestedType.Name}\n";
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // 忽略无法访问的程序集
                }
            }
            
            if (touchStateEnumCount <= 1)
            {
                _m_verificationResult += "✅ TouchState枚举无重复\n";
            }
            else
            {
                _m_verificationResult += $"❌ 发现{touchStateEnumCount}个TouchState枚举，存在重复\n";
                noDuplication = false;
            }
            
            if (feedbackTypeEnumCount <= 1)
            {
                _m_verificationResult += "✅ FeedbackType枚举无重复\n";
            }
            else
            {
                _m_verificationResult += $"❌ 发现{feedbackTypeEnumCount}个FeedbackType相关枚举，存在重复\n";
                noDuplication = false;
            }
            
            _m_verificationResult += "\n";
            return noDuplication;
        }
        
        /// <summary>
        /// 验证代码复用情况
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyCodeReuse()
        {
            _m_verificationResult += "5. 验证代码复用情况\n";
            
            // 检查TouchInputManager是否正确使用TouchFeedbackSystem.FeedbackType
            var touchInputType = System.Type.GetType("BlokusGame.Core.Managers.TouchInputManager, Assembly-CSharp");
            if (touchInputType != null)
            {
                var methods = touchInputType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
                var showVisualFeedbackMethod = methods.FirstOrDefault(m => m.Name == "_showVisualFeedback");
                
                if (showVisualFeedbackMethod != null)
                {
                    var parameters = showVisualFeedbackMethod.GetParameters();
                    if (parameters.Length >= 2)
                    {
                        var feedbackParam = parameters[1];
                        if (feedbackParam.ParameterType.Name == "FeedbackType" && 
                            feedbackParam.ParameterType.DeclaringType?.Name == "TouchFeedbackSystem")
                        {
                            _m_verificationResult += "✅ TouchInputManager正确使用TouchFeedbackSystem.FeedbackType\n";
                        }
                        else
                        {
                            _m_verificationResult += $"❌ TouchInputManager使用了错误的反馈类型: {feedbackParam.ParameterType.FullName}\n";
                            return false;
                        }
                    }
                }
            }
            
            _m_verificationResult += "✅ 代码复用验证通过\n";
            _m_verificationResult += "\n";
            return true;
        }
    }
}