using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using BlokusGame.Core.UI;
using BlokusGame.Core.Managers;

namespace BlokusGame.Editor
{
    /// <summary>
    /// LoadingUI实现验证工具
    /// 验证LoadingUI的完整性和UIManager集成
    /// </summary>
    public class LoadingUIVerification : EditorWindow
    {
        /// <summary>验证结果文本</summary>
        private string _m_verificationResult = "";
        
        /// <summary>滚动位置</summary>
        private Vector2 _m_scrollPosition;
        
        [MenuItem("Blokus/验证工具/LoadingUI实现验证")]
        public static void ShowWindow()
        {
            GetWindow<LoadingUIVerification>("LoadingUI实现验证");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("LoadingUI实现验证工具", EditorStyles.boldLabel);
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
        /// 执行LoadingUI验证
        /// </summary>
        private void _performVerification()
        {
            _m_verificationResult = "=== LoadingUI实现验证开始 ===\n\n";
            
            bool allTestsPassed = true;
            
            // 1. 验证LoadingUI类存在和基本结构
            allTestsPassed &= _verifyLoadingUIClass();
            
            // 2. 验证LoadingUI的UIBase继承
            allTestsPassed &= _verifyUIBaseInheritance();
            
            // 3. 验证LoadingUI的核心方法
            allTestsPassed &= _verifyCoreMethods();
            
            // 4. 验证LoadingUI的字段和属性
            allTestsPassed &= _verifyFieldsAndProperties();
            
            // 5. 验证UIManager集成
            allTestsPassed &= _verifyUIManagerIntegration();
            
            // 6. 验证便利方法
            allTestsPassed &= _verifyConvenienceMethods();
            
            // 7. 验证步骤管理功能
            allTestsPassed &= _verifyStepManagement();
            
            _m_verificationResult += "\n=== 验证总结 ===\n";
            if (allTestsPassed)
            {
                _m_verificationResult += "✅ 所有验证通过！LoadingUI实现完整。\n";
            }
            else
            {
                _m_verificationResult += "❌ 部分验证失败，请检查上述错误。\n";
            }
            
            _m_verificationResult += "=== LoadingUI实现验证结束 ===\n";
        }
        
        /// <summary>
        /// 验证LoadingUI类存在和基本结构
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyLoadingUIClass()
        {
            _m_verificationResult += "1. 验证LoadingUI类存在和基本结构\n";
            
            var loadingUIType = typeof(LoadingUI);
            if (loadingUIType == null)
            {
                _m_verificationResult += "❌ LoadingUI类不存在\n\n";
                return false;
            }
            
            _m_verificationResult += "✅ LoadingUI类存在\n";
            _m_verificationResult += $"   - 命名空间: {loadingUIType.Namespace}\n";
            _m_verificationResult += $"   - 完整名称: {loadingUIType.FullName}\n\n";
            
            return true;
        }
        
        /// <summary>
        /// 验证LoadingUI的UIBase继承
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyUIBaseInheritance()
        {
            _m_verificationResult += "2. 验证LoadingUI的UIBase继承\n";
            
            var loadingUIType = typeof(LoadingUI);
            var uiBaseType = typeof(UIBase);
            
            if (!uiBaseType.IsAssignableFrom(loadingUIType))
            {
                _m_verificationResult += "❌ LoadingUI没有继承UIBase\n\n";
                return false;
            }
            
            _m_verificationResult += "✅ LoadingUI正确继承UIBase\n";
            _m_verificationResult += $"   - 基类: {loadingUIType.BaseType.Name}\n\n";
            
            return true;
        }
        
        /// <summary>
        /// 验证LoadingUI的核心方法
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyCoreMethods()
        {
            _m_verificationResult += "3. 验证LoadingUI的核心方法\n";
            
            var loadingUIType = typeof(LoadingUI);
            bool allMethodsExist = true;
            
            // 核心方法列表
            string[] requiredMethods = {
                "ShowLoading",
                "HideLoading", 
                "UpdateProgress",
                "SetLoadingText",
                "SetBackgroundAlpha",
                "InitializeUIContent",
                "OnUIShown",
                "OnUIHidden"
            };
            
            foreach (string methodName in requiredMethods)
            {
                var method = loadingUIType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    _m_verificationResult += $"✅ {methodName} 方法存在\n";
                }
                else
                {
                    _m_verificationResult += $"❌ {methodName} 方法不存在\n";
                    allMethodsExist = false;
                }
            }
            
            _m_verificationResult += "\n";
            return allMethodsExist;
        }
        
        /// <summary>
        /// 验证LoadingUI的字段和属性
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyFieldsAndProperties()
        {
            _m_verificationResult += "4. 验证LoadingUI的字段和属性\n";
            
            var loadingUIType = typeof(LoadingUI);
            bool allFieldsExist = true;
            
            // 必需字段列表
            string[] requiredFields = {
                "_m_loadingText",
                "_m_progressText",
                "_m_progressBar",
                "_m_loadingSpinner",
                "_m_backgroundMask",
                "_m_spinnerRotationSpeed",
                "_m_enableSpinnerAnimation",
                "_m_currentProgress",
                "_m_showProgressBar"
            };
            
            foreach (string fieldName in requiredFields)
            {
                var field = loadingUIType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    _m_verificationResult += $"✅ {fieldName} 字段存在 ({field.FieldType.Name})\n";
                }
                else
                {
                    _m_verificationResult += $"❌ {fieldName} 字段不存在\n";
                    allFieldsExist = false;
                }
            }
            
            _m_verificationResult += "\n";
            return allFieldsExist;
        }
        
        /// <summary>
        /// 验证UIManager集成
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyUIManagerIntegration()
        {
            _m_verificationResult += "5. 验证UIManager集成\n";
            
            var uiManagerType = typeof(UIManager);
            bool allIntegrationMethodsExist = true;
            
            // UIManager中应该存在的LoadingUI相关方法
            string[] requiredMethods = {
                "ShowLoading",
                "HideLoading",
                "UpdateLoadingProgress",
                "ShowSimpleLoading",
                "ShowProgressLoading",
                "SetLoadingSteps",
                "CompleteLoading"
            };
            
            foreach (string methodName in requiredMethods)
            {
                var method = uiManagerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (method != null)
                {
                    _m_verificationResult += $"✅ UIManager.{methodName} 方法存在\n";
                }
                else
                {
                    _m_verificationResult += $"❌ UIManager.{methodName} 方法不存在\n";
                    allIntegrationMethodsExist = false;
                }
            }
            
            _m_verificationResult += "\n";
            return allIntegrationMethodsExist;
        }
        
        /// <summary>
        /// 验证便利方法
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyConvenienceMethods()
        {
            _m_verificationResult += "6. 验证便利方法\n";
            
            var loadingUIType = typeof(LoadingUI);
            bool allConvenienceMethodsExist = true;
            
            // 便利方法列表
            string[] convenienceMethods = {
                "ShowSimpleLoading",
                "ShowProgressLoading",
                "SetProgressPercentage",
                "IncrementProgress",
                "CompleteLoading"
            };
            
            foreach (string methodName in convenienceMethods)
            {
                var method = loadingUIType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (method != null)
                {
                    _m_verificationResult += $"✅ {methodName} 便利方法存在\n";
                }
                else
                {
                    _m_verificationResult += $"❌ {methodName} 便利方法不存在\n";
                    allConvenienceMethodsExist = false;
                }
            }
            
            _m_verificationResult += "\n";
            return allConvenienceMethodsExist;
        }
        
        /// <summary>
        /// 验证步骤管理功能
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool _verifyStepManagement()
        {
            _m_verificationResult += "7. 验证步骤管理功能\n";
            
            var loadingUIType = typeof(LoadingUI);
            bool allStepMethodsExist = true;
            
            // 步骤管理方法列表
            string[] stepMethods = {
                "SetLoadingSteps",
                "NextLoadingStep",
                "GoToLoadingStep",
                "GetCurrentStepInfo",
                "ClearLoadingSteps"
            };
            
            foreach (string methodName in stepMethods)
            {
                var methods = loadingUIType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == methodName).ToArray();
                
                if (methods.Length > 0)
                {
                    _m_verificationResult += $"✅ {methodName} 步骤管理方法存在 ({methods.Length}个重载)\n";
                }
                else
                {
                    _m_verificationResult += $"❌ {methodName} 步骤管理方法不存在\n";
                    allStepMethodsExist = false;
                }
            }
            
            _m_verificationResult += "\n";
            return allStepMethodsExist;
        }
    }
}