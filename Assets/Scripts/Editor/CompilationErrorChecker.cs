using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 编译错误检查工具
    /// 用于验证代码是否存在编译错误
    /// </summary>
    public class CompilationErrorChecker : EditorWindow
    {
        [MenuItem("Blokus/验证工具/编译错误检查")]
        public static void ShowWindow()
        {
            GetWindow<CompilationErrorChecker>("编译错误检查");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("编译错误检查工具", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("检查TouchInputManager"))
            {
                CheckTouchInputManager();
            }
            
            if (GUILayout.Button("检查TouchFeedbackSystem"))
            {
                CheckTouchFeedbackSystem();
            }
            
            if (GUILayout.Button("检查TouchGameplayIntegration"))
            {
                CheckTouchGameplayIntegration();
            }
            
            if (GUILayout.Button("检查GameManager"))
            {
                CheckGameManager();
            }
            
            if (GUILayout.Button("检查BoardManager"))
            {
                CheckBoardManager();
            }
        }
        
        private void CheckTouchInputManager()
        {
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.Managers.TouchInputManager");
                if (type != null)
                {
                    Debug.Log("✅ TouchInputManager 编译正常");
                    
                    // 检查关键方法
                    var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
                    bool hasMultiTouch = false;
                    bool hasAntiMistouch = false;
                    
                    foreach (var method in methods)
                    {
                        if (method.Name.Contains("handleMultiTouch") || method.Name.Contains("handlePinchGesture"))
                            hasMultiTouch = true;
                        if (method.Name.Contains("isValidTouch") || method.Name.Contains("processAntiMistouch"))
                            hasAntiMistouch = true;
                    }
                    
                    Debug.Log($"多点触摸功能: {(hasMultiTouch ? "✅" : "❌")}");
                    Debug.Log($"防误触功能: {(hasAntiMistouch ? "✅" : "❌")}");
                }
                else
                {
                    Debug.LogError("❌ TouchInputManager 类型未找到");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ TouchInputManager 编译错误: {e.Message}");
            }
        }
        
        private void CheckTouchFeedbackSystem()
        {
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.InputSystem.TouchFeedbackSystem");
                if (type != null)
                {
                    Debug.Log("✅ TouchFeedbackSystem 编译正常");
                    
                    // 检查FeedbackType枚举
                    var feedbackType = type.GetNestedType("FeedbackType");
                    if (feedbackType != null)
                    {
                        var values = System.Enum.GetNames(feedbackType);
                        Debug.Log($"FeedbackType 枚举值: {string.Join(", ", values)}");
                        
                        bool hasSuccess = System.Array.Exists(values, v => v == "Success");
                        bool hasError = System.Array.Exists(values, v => v == "Error");
                        
                        Debug.Log($"Success 枚举: {(hasSuccess ? "✅" : "❌")}");
                        Debug.Log($"Error 枚举: {(hasError ? "✅" : "❌")}");
                    }
                }
                else
                {
                    Debug.LogError("❌ TouchFeedbackSystem 类型未找到");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ TouchFeedbackSystem 编译错误: {e.Message}");
            }
        }
        
        private void CheckTouchGameplayIntegration()
        {
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.InputSystem.TouchGameplayIntegration");
                if (type != null)
                {
                    Debug.Log("✅ TouchGameplayIntegration 编译正常");
                    
                    // 检查关键方法
                    var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
                    bool hasDragHandling = false;
                    bool hasValidation = false;
                    
                    foreach (var method in methods)
                    {
                        if (method.Name.Contains("onPieceDrag"))
                            hasDragHandling = true;
                        if (method.Name.Contains("validatePiecePosition"))
                            hasValidation = true;
                    }
                    
                    Debug.Log($"拖拽处理: {(hasDragHandling ? "✅" : "❌")}");
                    Debug.Log($"位置验证: {(hasValidation ? "✅" : "❌")}");
                }
                else
                {
                    Debug.LogError("❌ TouchGameplayIntegration 类型未找到");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ TouchGameplayIntegration 编译错误: {e.Message}");
            }
        }
        
        private void CheckGameManager()
        {
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.Managers.GameManager");
                if (type != null)
                {
                    Debug.Log("✅ GameManager 编译正常");
                    
                    // 检查tryPlacePiece方法
                    var method = type.GetMethod("tryPlacePiece");
                    if (method != null)
                    {
                        Debug.Log("✅ tryPlacePiece 方法存在");
                        var parameters = method.GetParameters();
                        Debug.Log($"参数数量: {parameters.Length}");
                        foreach (var param in parameters)
                        {
                            Debug.Log($"  - {param.Name}: {param.ParameterType.Name}");
                        }
                    }
                    else
                    {
                        Debug.LogError("❌ tryPlacePiece 方法未找到");
                    }
                }
                else
                {
                    Debug.LogError("❌ GameManager 类型未找到");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ GameManager 编译错误: {e.Message}");
            }
        }
        
        private void CheckBoardManager()
        {
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.Managers.BoardManager");
                if (type != null)
                {
                    Debug.Log("✅ BoardManager 编译正常");
                    
                    // 检查tryPlacePiece方法
                    var method = type.GetMethod("tryPlacePiece");
                    if (method != null)
                    {
                        Debug.Log("✅ tryPlacePiece 方法存在");
                    }
                    else
                    {
                        Debug.LogError("❌ tryPlacePiece 方法未找到");
                    }
                }
                else
                {
                    Debug.LogError("❌ BoardManager 类型未找到");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ BoardManager 编译错误: {e.Message}");
            }
        }
    }
}