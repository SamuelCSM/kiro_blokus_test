using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 最终编译检查工具
    /// 验证所有修复后的代码是否能正常编译
    /// </summary>
    public class FinalCompilationCheck : EditorWindow
    {
        [MenuItem("Blokus/验证工具/最终编译检查")]
        public static void ShowWindow()
        {
            GetWindow<FinalCompilationCheck>("最终编译检查");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("最终编译检查", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("执行完整编译检查"))
            {
                PerformFullCompilationCheck();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("检查项目:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("1. 检查TouchFeedbackSystem"))
            {
                CheckTouchFeedbackSystem();
            }
            
            if (GUILayout.Button("2. 检查GameManager"))
            {
                CheckGameManager();
            }
            
            if (GUILayout.Button("3. 检查BoardManager"))
            {
                CheckBoardManager();
            }
            
            if (GUILayout.Button("4. 检查BoardController"))
            {
                CheckBoardController();
            }
            
            if (GUILayout.Button("5. 检查GamePiece"))
            {
                CheckGamePiece();
            }
            
            if (GUILayout.Button("6. 检查TouchGameplayIntegration"))
            {
                CheckTouchGameplayIntegration();
            }
        }
        
        private void PerformFullCompilationCheck()
        {
            Debug.Log("=== 开始最终编译检查 ===");
            
            CheckTouchFeedbackSystem();
            CheckGameManager();
            CheckBoardManager();
            CheckBoardController();
            CheckGamePiece();
            CheckTouchGameplayIntegration();
            
            Debug.Log("=== 最终编译检查完成 ===");
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
                        
                        bool hasSuccess = System.Array.Exists(values, v => v == "Success");
                        bool hasError = System.Array.Exists(values, v => v == "Error");
                        
                        if (hasSuccess && hasError)
                        {
                            Debug.Log("✅ Success 和 Error 枚举值存在");
                        }
                        else
                        {
                            Debug.LogError("❌ 缺少 Success 或 Error 枚举值");
                        }
                    }
                    else
                    {
                        Debug.LogError("❌ FeedbackType 枚举未找到");
                    }
                    
                    // 检查playHapticFeedback方法
                    var method = type.GetMethod("playHapticFeedback");
                    if (method != null)
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
        
        private void CheckGameManager()
        {
            Debug.Log("--- 检查 GameManager ---");
            
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.Managers.GameManager");
                if (type != null)
                {
                    Debug.Log("✅ GameManager 类型存在");
                    
                    // 检查tryPlacePiece方法
                    var method = type.GetMethod("tryPlacePiece");
                    if (method != null)
                    {
                        Debug.Log("✅ tryPlacePiece 方法存在");
                        var parameters = method.GetParameters();
                        if (parameters.Length == 3)
                        {
                            Debug.Log($"✅ tryPlacePiece 参数正确: {parameters[0].ParameterType.Name}, {parameters[1].ParameterType.Name}, {parameters[2].ParameterType.Name}");
                        }
                        else
                        {
                            Debug.LogError($"❌ tryPlacePiece 参数数量错误: {parameters.Length}");
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
                Debug.LogError($"❌ GameManager 检查失败: {e.Message}");
            }
        }
        
        private void CheckBoardManager()
        {
            Debug.Log("--- 检查 BoardManager ---");
            
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.Managers.BoardManager");
                if (type != null)
                {
                    Debug.Log("✅ BoardManager 类型存在");
                    
                    // 检查tryPlacePiece方法
                    var tryPlaceMethod = type.GetMethod("tryPlacePiece");
                    if (tryPlaceMethod != null)
                    {
                        Debug.Log("✅ tryPlacePiece 方法存在");
                    }
                    else
                    {
                        Debug.LogError("❌ tryPlacePiece 方法未找到");
                    }
                    
                    // 检查getBoardSystem方法
                    var getBoardSystemMethod = type.GetMethod("getBoardSystem");
                    if (getBoardSystemMethod != null)
                    {
                        Debug.Log("✅ getBoardSystem 方法存在");
                    }
                    else
                    {
                        Debug.LogError("❌ getBoardSystem 方法未找到");
                    }
                }
                else
                {
                    Debug.LogError("❌ BoardManager 类型未找到");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ BoardManager 检查失败: {e.Message}");
            }
        }
        
        private void CheckBoardController()
        {
            Debug.Log("--- 检查 BoardController ---");
            
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.Board.BoardController");
                if (type != null)
                {
                    Debug.Log("✅ BoardController 类型存在");
                    
                    // 检查getBoardSystem方法
                    var method = type.GetMethod("getBoardSystem");
                    if (method != null)
                    {
                        Debug.Log("✅ getBoardSystem 方法存在");
                    }
                    else
                    {
                        Debug.LogError("❌ getBoardSystem 方法未找到");
                    }
                }
                else
                {
                    Debug.LogError("❌ BoardController 类型未找到");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ BoardController 检查失败: {e.Message}");
            }
        }
        
        private void CheckGamePiece()
        {
            Debug.Log("--- 检查 GamePiece ---");
            
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.Pieces.GamePiece");
                if (type != null)
                {
                    Debug.Log("✅ GamePiece 类型存在");
                    
                    // 检查setPlaced方法
                    var method = type.GetMethod("setPlaced");
                    if (method != null)
                    {
                        Debug.Log("✅ setPlaced 方法存在");
                        var parameters = method.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(bool))
                        {
                            Debug.Log("✅ setPlaced 参数正确");
                        }
                        else
                        {
                            Debug.LogError("❌ setPlaced 参数错误");
                        }
                    }
                    else
                    {
                        Debug.LogError("❌ setPlaced 方法未找到");
                    }
                }
                else
                {
                    Debug.LogError("❌ GamePiece 类型未找到");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ GamePiece 检查失败: {e.Message}");
            }
        }
        
        private void CheckTouchGameplayIntegration()
        {
            Debug.Log("--- 检查 TouchGameplayIntegration ---");
            
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.InputSystem.TouchGameplayIntegration");
                if (type != null)
                {
                    Debug.Log("✅ TouchGameplayIntegration 类型存在");
                    
                    // 检查关键私有方法
                    var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
                    bool hasValidateMethod = false;
                    bool hasDragMethods = false;
                    
                    foreach (var method in methods)
                    {
                        if (method.Name.Contains("validatePiecePosition"))
                            hasValidateMethod = true;
                        if (method.Name.Contains("onPieceDrag"))
                            hasDragMethods = true;
                    }
                    
                    if (hasValidateMethod)
                    {
                        Debug.Log("✅ 位置验证方法存在");
                    }
                    else
                    {
                        Debug.LogError("❌ 位置验证方法未找到");
                    }
                    
                    if (hasDragMethods)
                    {
                        Debug.Log("✅ 拖拽处理方法存在");
                    }
                    else
                    {
                        Debug.LogError("❌ 拖拽处理方法未找到");
                    }
                }
                else
                {
                    Debug.LogError("❌ TouchGameplayIntegration 类型未找到");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ TouchGameplayIntegration 检查失败: {e.Message}");
            }
        }
    }
}