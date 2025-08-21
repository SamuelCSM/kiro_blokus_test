using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

namespace BlokusGame.Editor
{
    /// <summary>
    /// UI系统编译验证工具
    /// 验证所有UI面板的完整性和功能实现
    /// </summary>
    public class UISystemCompilationVerification : EditorWindow
    {
        [MenuItem("Blokus/验证/UI系统编译验证")]
        public static void ShowWindow()
        {
            GetWindow<UISystemCompilationVerification>("UI系统编译验证");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("UI系统编译验证", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("验证UI系统完整性", GUILayout.Height(30)))
            {
                VerifyUISystem();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("验证UI面板功能", GUILayout.Height(30)))
            {
                VerifyUIPanelFunctionality();
            }
        }
        
        /// <summary>
        /// 验证UI系统完整性
        /// </summary>
        private void VerifyUISystem()
        {
            Debug.Log("=== UI系统编译验证开始 ===");
            
            // 验证UI基础类
            VerifyUIBaseClass();
            
            // 验证UI管理器
            VerifyUIManager();
            
            // 验证各个UI面板
            VerifySettingsUI();
            VerifyPauseMenuUI();
            VerifyGameOverUI();
            VerifyMessageUI();
            VerifyLoadingUI();
            
            Debug.Log("=== UI系统编译验证完成 ===");
        }
        
        /// <summary>
        /// 验证UIBase基础类
        /// </summary>
        private void VerifyUIBaseClass()
        {
            var uiBaseType = System.Type.GetType("BlokusGame.Core.UI.UIBase, Assembly-CSharp");
            if (uiBaseType != null)
            {
                Debug.Log("✅ UIBase基础类验证通过");
                
                // 检查关键方法
                var showMethod = uiBaseType.GetMethod("Show");
                var hideMethod = uiBaseType.GetMethod("Hide");
                var toggleMethod = uiBaseType.GetMethod("Toggle");
                
                if (showMethod != null && hideMethod != null && toggleMethod != null)
                {
                    Debug.Log("✅ UIBase核心方法验证通过");
                }
                else
                {
                    Debug.LogError("❌ UIBase缺少核心方法");
                }
            }
            else
            {
                Debug.LogError("❌ UIBase基础类未找到");
            }
        }
        
        /// <summary>
        /// 验证UI管理器
        /// </summary>
        private void VerifyUIManager()
        {
            var uiManagerType = System.Type.GetType("BlokusGame.Core.Managers.UIManager, Assembly-CSharp");
            if (uiManagerType != null)
            {
                Debug.Log("✅ UIManager管理器验证通过");
                
                // 检查单例属性
                var instanceProperty = uiManagerType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProperty != null)
                {
                    Debug.Log("✅ UIManager单例模式验证通过");
                }
                else
                {
                    Debug.LogError("❌ UIManager缺少单例实例");
                }
            }
            else
            {
                Debug.LogError("❌ UIManager管理器未找到");
            }
        }
        
        /// <summary>
        /// 验证SettingsUI
        /// </summary>
        private void VerifySettingsUI()
        {
            var settingsUIType = System.Type.GetType("BlokusGame.Core.UI.SettingsUI, Assembly-CSharp");
            if (settingsUIType != null)
            {
                Debug.Log("✅ SettingsUI设置界面验证通过");
                
                // 检查关键方法
                var getCurrentSettingsMethod = settingsUIType.GetMethod("GetCurrentSettings");
                var setCurrentSettingsMethod = settingsUIType.GetMethod("SetCurrentSettings");
                var applyAndSaveMethod = settingsUIType.GetMethod("ApplyAndSaveSettings");
                
                if (getCurrentSettingsMethod != null && setCurrentSettingsMethod != null && applyAndSaveMethod != null)
                {
                    Debug.Log("✅ SettingsUI功能方法验证通过");
                }
                else
                {
                    Debug.LogError("❌ SettingsUI缺少功能方法");
                }
            }
            else
            {
                Debug.LogError("❌ SettingsUI设置界面未找到");
            }
        }
        
        /// <summary>
        /// 验证PauseMenuUI
        /// </summary>
        private void VerifyPauseMenuUI()
        {
            var pauseMenuUIType = System.Type.GetType("BlokusGame.Core.UI.PauseMenuUI, Assembly-CSharp");
            if (pauseMenuUIType != null)
            {
                Debug.Log("✅ PauseMenuUI暂停菜单验证通过");
                
                // 检查关键方法
                var quickResumeMethod = pauseMenuUIType.GetMethod("QuickResume");
                var isConfirmationDialogShowingMethod = pauseMenuUIType.GetMethod("IsConfirmationDialogShowing");
                
                if (quickResumeMethod != null && isConfirmationDialogShowingMethod != null)
                {
                    Debug.Log("✅ PauseMenuUI功能方法验证通过");
                }
                else
                {
                    Debug.LogError("❌ PauseMenuUI缺少功能方法");
                }
            }
            else
            {
                Debug.LogError("❌ PauseMenuUI暂停菜单未找到");
            }
        }
        
        /// <summary>
        /// 验证GameOverUI
        /// </summary>
        private void VerifyGameOverUI()
        {
            var gameOverUIType = System.Type.GetType("BlokusGame.Core.UI.GameOverUI, Assembly-CSharp");
            if (gameOverUIType != null)
            {
                Debug.Log("✅ GameOverUI游戏结束界面验证通过");
                
                // 检查关键方法
                var showGameResultsMethod = gameOverUIType.GetMethod("ShowGameResults");
                var showSimpleResultsMethod = gameOverUIType.GetMethod("ShowSimpleResults");
                var setButtonVisibilityMethod = gameOverUIType.GetMethod("SetButtonVisibility");
                
                if (showGameResultsMethod != null && showSimpleResultsMethod != null && setButtonVisibilityMethod != null)
                {
                    Debug.Log("✅ GameOverUI功能方法验证通过");
                }
                else
                {
                    Debug.LogError("❌ GameOverUI缺少功能方法");
                }
            }
            else
            {
                Debug.LogError("❌ GameOverUI游戏结束界面未找到");
            }
        }
        
        /// <summary>
        /// 验证MessageUI
        /// </summary>
        private void VerifyMessageUI()
        {
            var messageUIType = System.Type.GetType("BlokusGame.Core.UI.MessageUI, Assembly-CSharp");
            if (messageUIType != null)
            {
                Debug.Log("✅ MessageUI消息提示组件验证通过");
                
                // 检查关键方法
                var showMessageMethod = messageUIType.GetMethod("ShowMessage");
                var showMessageImmediateMethod = messageUIType.GetMethod("ShowMessageImmediate");
                var clearMessageQueueMethod = messageUIType.GetMethod("ClearMessageQueue");
                
                if (showMessageMethod != null && showMessageImmediateMethod != null && clearMessageQueueMethod != null)
                {
                    Debug.Log("✅ MessageUI功能方法验证通过");
                }
                else
                {
                    Debug.LogError("❌ MessageUI缺少功能方法");
                }
            }
            else
            {
                Debug.LogError("❌ MessageUI消息提示组件未找到");
            }
        }
        
        /// <summary>
        /// 验证LoadingUI
        /// </summary>
        private void VerifyLoadingUI()
        {
            var loadingUIType = System.Type.GetType("BlokusGame.Core.UI.LoadingUI, Assembly-CSharp");
            if (loadingUIType != null)
            {
                Debug.Log("✅ LoadingUI加载界面验证通过");
                
                // 检查关键方法
                var showLoadingMethod = loadingUIType.GetMethod("ShowLoading");
                var updateProgressMethod = loadingUIType.GetMethod("UpdateProgress");
                var setLoadingStepsMethod = loadingUIType.GetMethods().FirstOrDefault(m => m.Name == "SetLoadingSteps" && m.GetParameters().Length == 2);
                var completeLoadingMethod = loadingUIType.GetMethod("CompleteLoading");
                
                if (showLoadingMethod != null && updateProgressMethod != null && setLoadingStepsMethod != null && completeLoadingMethod != null)
                {
                    Debug.Log("✅ LoadingUI功能方法验证通过");
                }
                else
                {
                    Debug.LogError("❌ LoadingUI缺少功能方法");
                }
            }
            else
            {
                Debug.LogError("❌ LoadingUI加载界面未找到");
            }
        }
        
        /// <summary>
        /// 验证UI面板功能
        /// </summary>
        private void VerifyUIPanelFunctionality()
        {
            Debug.Log("=== UI面板功能验证开始 ===");
            
            // 验证MessageData结构
            var messageDataType = System.Type.GetType("BlokusGame.Core.UI.MessageData, Assembly-CSharp");
            if (messageDataType != null)
            {
                Debug.Log("✅ MessageData数据结构验证通过");
            }
            else
            {
                Debug.LogError("❌ MessageData数据结构未找到");
            }
            
            // 验证GameEvents中的设置事件
            var gameEventsType = System.Type.GetType("BlokusGame.Core.Events.GameEvents, Assembly-CSharp");
            if (gameEventsType != null)
            {
                var onSettingsChangedField = gameEventsType.GetField("onSettingsChanged");
                if (onSettingsChangedField != null)
                {
                    Debug.Log("✅ GameEvents设置变更事件验证通过");
                }
                else
                {
                    Debug.LogError("❌ GameEvents缺少设置变更事件");
                }
            }
            
            Debug.Log("=== UI面板功能验证完成 ===");
        }
    }
}