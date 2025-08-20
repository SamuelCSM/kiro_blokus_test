using UnityEngine;
using UnityEditor;
using BlokusGame.Core.Managers;
using BlokusGame.Core.InputSystem;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 触摸输入系统验证工具
    /// 用于验证触摸输入系统的完整性和功能正确性
    /// 检查组件依赖关系和配置参数
    /// </summary>
    public class TouchInputSystemVerification : EditorWindow
    {
        /// <summary>验证结果文本</summary>
        private string _m_verificationResults = "";
        
        /// <summary>滚动位置</summary>
        private Vector2 _m_scrollPosition;
        
        [MenuItem("Blokus/验证/触摸输入系统验证")]
        public static void ShowWindow()
        {
            TouchInputSystemVerification window = GetWindow<TouchInputSystemVerification>();
            window.titleContent = new GUIContent("触摸输入系统验证");
            window.Show();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("触摸输入系统验证工具", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("开始验证", GUILayout.Height(30)))
            {
                _performVerification();
            }
            
            GUILayout.Space(10);
            
            _m_scrollPosition = EditorGUILayout.BeginScrollView(_m_scrollPosition);
            EditorGUILayout.TextArea(_m_verificationResults, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
        
        /// <summary>
        /// 执行验证
        /// </summary>
        private void _performVerification()
        {
            _m_verificationResults = "开始触摸输入系统验证...\n\n";
            
            bool allTestsPassed = true;
            
            // 验证InputManager
            allTestsPassed &= _verifyInputManager();
            
            // 验证TouchInputManager
            allTestsPassed &= _verifyTouchInputManager();
            
            // 验证TouchFeedbackSystem
            allTestsPassed &= _verifyTouchFeedbackSystem();
            
            // 验证组件集成
            allTestsPassed &= _verifyComponentIntegration();
            
            // 验证事件系统集成
            allTestsPassed &= _verifyEventSystemIntegration();
            
            _m_verificationResults += "\n" + new string('=', 50) + "\n";
            _m_verificationResults += allTestsPassed ? 
                "✅ 所有验证通过！触摸输入系统配置正确。\n" : 
                "❌ 验证失败！请检查上述错误并修复。\n";
        }
        
        /// <summary>
        /// 验证InputManager
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyInputManager()
        {
            _m_verificationResults += "1. 验证InputManager类...\n";
            
            bool passed = true;
            
            // 检查类是否存在
            var inputManagerType = typeof(InputManager);
            if (inputManagerType == null)
            {
                _m_verificationResults += "   ❌ InputManager类不存在\n";
                return false;
            }
            
            _m_verificationResults += "   ✅ InputManager类存在\n";
            
            // 检查必要的方法
            var methods = new string[]
            {
                "setInputEnabled",
                "setTouchInputEnabled", 
                "setMouseInputEnabled",
                "resetInputSystem",
                "isInitialized"
            };
            
            foreach (string methodName in methods)
            {
                var method = inputManagerType.GetMethod(methodName);
                if (method == null)
                {
                    _m_verificationResults += $"   ❌ 缺少方法: {methodName}\n";
                    passed = false;
                }
                else
                {
                    _m_verificationResults += $"   ✅ 方法存在: {methodName}\n";
                }
            }
            
            return passed;
        }
        
        /// <summary>
        /// 验证TouchInputManager
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyTouchInputManager()
        {
            _m_verificationResults += "\n2. 验证TouchInputManager类...\n";
            
            bool passed = true;
            
            // 检查类是否存在
            var touchInputManagerType = typeof(TouchInputManager);
            if (touchInputManagerType == null)
            {
                _m_verificationResults += "   ❌ TouchInputManager类不存在\n";
                return false;
            }
            
            _m_verificationResults += "   ✅ TouchInputManager类存在\n";
            
            // 检查必要的方法
            var methods = new string[]
            {
                "setInputEnabled",
                "setTouchInputEnabled",
                "setMouseInputEnabled",
                "resetInput",
                "getCurrentTouchState",
                "getSelectedPiece"
            };
            
            foreach (string methodName in methods)
            {
                var method = touchInputManagerType.GetMethod(methodName);
                if (method == null)
                {
                    _m_verificationResults += $"   ❌ 缺少方法: {methodName}\n";
                    passed = false;
                }
                else
                {
                    _m_verificationResults += $"   ✅ 方法存在: {methodName}\n";
                }
            }
            
            // 检查TouchState枚举
            var touchStateType = touchInputManagerType.GetNestedType("TouchState");
            if (touchStateType == null)
            {
                _m_verificationResults += "   ❌ TouchState枚举不存在\n";
                passed = false;
            }
            else
            {
                _m_verificationResults += "   ✅ TouchState枚举存在\n";
                
                // 检查枚举值
                var expectedValues = new string[] { "None", "SingleTouch", "Dragging", "MultiTouch", "Pinching" };
                foreach (string value in expectedValues)
                {
                    if (System.Enum.IsDefined(touchStateType, value))
                    {
                        _m_verificationResults += $"   ✅ TouchState.{value} 存在\n";
                    }
                    else
                    {
                        _m_verificationResults += $"   ❌ TouchState.{value} 不存在\n";
                        passed = false;
                    }
                }
            }
            
            return passed;
        }
        
        /// <summary>
        /// 验证TouchFeedbackSystem
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyTouchFeedbackSystem()
        {
            _m_verificationResults += "\n3. 验证TouchFeedbackSystem类...\n";
            
            bool passed = true;
            
            // 检查类是否存在
            var feedbackSystemType = typeof(TouchFeedbackSystem);
            if (feedbackSystemType == null)
            {
                _m_verificationResults += "   ❌ TouchFeedbackSystem类不存在\n";
                return false;
            }
            
            _m_verificationResults += "   ✅ TouchFeedbackSystem类存在\n";
            
            // 检查必要的方法
            var methods = new string[]
            {
                "showTouchPoint",
                "hideTouchPoint",
                "showRippleEffect",
                "startDragTrail",
                "updateDragTrail",
                "endDragTrail",
                "playHapticFeedback",
                "playAudioFeedback",
                "setVisualFeedbackEnabled",
                "setHapticFeedbackEnabled",
                "setAudioFeedbackEnabled"
            };
            
            foreach (string methodName in methods)
            {
                var method = feedbackSystemType.GetMethod(methodName);
                if (method == null)
                {
                    _m_verificationResults += $"   ❌ 缺少方法: {methodName}\n";
                    passed = false;
                }
                else
                {
                    _m_verificationResults += $"   ✅ 方法存在: {methodName}\n";
                }
            }
            
            // 检查FeedbackType枚举
            var feedbackTypeType = feedbackSystemType.GetNestedType("FeedbackType");
            if (feedbackTypeType == null)
            {
                _m_verificationResults += "   ❌ FeedbackType枚举不存在\n";
                passed = false;
            }
            else
            {
                _m_verificationResults += "   ✅ FeedbackType枚举存在\n";
                
                // 检查枚举值
                var expectedValues = new string[] { "Light", "Medium", "Strong" };
                foreach (string value in expectedValues)
                {
                    if (System.Enum.IsDefined(feedbackTypeType, value))
                    {
                        _m_verificationResults += $"   ✅ FeedbackType.{value} 存在\n";
                    }
                    else
                    {
                        _m_verificationResults += $"   ❌ FeedbackType.{value} 不存在\n";
                        passed = false;
                    }
                }
            }
            
            return passed;
        }
        
        /// <summary>
        /// 验证组件集成
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyComponentIntegration()
        {
            _m_verificationResults += "\n4. 验证组件集成...\n";
            
            bool passed = true;
            
            // 查找场景中的InputManager
            InputManager inputManager = FindObjectOfType<InputManager>();
            if (inputManager == null)
            {
                _m_verificationResults += "   ⚠️  场景中未找到InputManager实例（运行时会自动创建）\n";
            }
            else
            {
                _m_verificationResults += "   ✅ 场景中找到InputManager实例\n";
                
                // 检查TouchInputManager组件
                TouchInputManager touchInputManager = inputManager.GetComponent<TouchInputManager>();
                if (touchInputManager == null)
                {
                    _m_verificationResults += "   ⚠️  InputManager上未找到TouchInputManager组件（运行时会自动创建）\n";
                }
                else
                {
                    _m_verificationResults += "   ✅ InputManager上找到TouchInputManager组件\n";
                    
                    // 检查TouchFeedbackSystem组件
                    TouchFeedbackSystem feedbackSystem = touchInputManager.GetComponent<TouchFeedbackSystem>();
                    if (feedbackSystem == null)
                    {
                        _m_verificationResults += "   ⚠️  TouchInputManager上未找到TouchFeedbackSystem组件（运行时会自动创建）\n";
                    }
                    else
                    {
                        _m_verificationResults += "   ✅ TouchInputManager上找到TouchFeedbackSystem组件\n";
                    }
                }
            }
            
            return passed;
        }
        
        /// <summary>
        /// 验证事件系统集成
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyEventSystemIntegration()
        {
            _m_verificationResults += "\n5. 验证事件系统集成...\n";
            
            bool passed = true;
            
            // 检查GameEvents类中的相关事件
            var gameEventsType = typeof(BlokusGame.Core.Events.GameEvents);
            if (gameEventsType == null)
            {
                _m_verificationResults += "   ❌ GameEvents类不存在\n";
                return false;
            }
            
            _m_verificationResults += "   ✅ GameEvents类存在\n";
            
            // 检查触摸相关事件
            var touchEvents = new string[]
            {
                "onPieceSelected",
                "onPieceDragStart",
                "onPieceDragging",
                "onPieceDragEnd",
                "onPieceRotated",
                "onPieceFlipped",
                "onPieceClicked"
            };
            
            foreach (string eventName in touchEvents)
            {
                var eventField = gameEventsType.GetField(eventName);
                if (eventField == null)
                {
                    _m_verificationResults += $"   ❌ 缺少事件: {eventName}\n";
                    passed = false;
                }
                else
                {
                    _m_verificationResults += $"   ✅ 事件存在: {eventName}\n";
                }
            }
            
            return passed;
        }
    }
}