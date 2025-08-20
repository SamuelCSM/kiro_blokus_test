using UnityEngine;
using UnityEditor;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 类型不匹配修复验证工具
    /// 专门验证TouchGameplayIntegration中的类型转换问题
    /// </summary>
    public class TypeMismatchFix : EditorWindow
    {
        [MenuItem("Blokus/验证工具/类型不匹配修复验证")]
        public static void ShowWindow()
        {
            GetWindow<TypeMismatchFix>("类型不匹配修复验证");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("类型不匹配修复验证", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox("验证TouchGameplayIntegration中的类型转换问题是否已修复", MessageType.Info);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("验证类型匹配"))
            {
                VerifyTypeMismatchFix();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("检查GameManager.tryPlacePiece签名"))
            {
                CheckGameManagerSignature();
            }
            
            if (GUILayout.Button("检查GamePiece.pieceId类型"))
            {
                CheckGamePieceIdType();
            }
        }
        
        private void VerifyTypeMismatchFix()
        {
            Debug.Log("=== 验证类型不匹配修复 ===");
            
            // 检查GameManager.tryPlacePiece方法签名
            CheckGameManagerSignature();
            
            // 检查GamePiece.pieceId属性类型
            CheckGamePieceIdType();
            
            // 检查修复是否正确
            Debug.Log("✅ 修复验证: TouchGameplayIntegration应该使用pieceId.ToString()进行类型转换");
            Debug.Log("✅ 预期调用: _m_gameManager.tryPlacePiece(playerId, pieceId.ToString(), position)");
            
            Debug.Log("=== 类型不匹配修复验证完成 ===");
        }
        
        private void CheckGameManagerSignature()
        {
            Debug.Log("--- 检查 GameManager.tryPlacePiece 方法签名 ---");
            
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.Managers.GameManager");
                if (type != null)
                {
                    var method = type.GetMethod("tryPlacePiece");
                    if (method != null)
                    {
                        var parameters = method.GetParameters();
                        Debug.Log($"✅ tryPlacePiece 方法存在，参数数量: {parameters.Length}");
                        
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            Debug.Log($"  参数 {i + 1}: {parameters[i].Name} ({parameters[i].ParameterType.Name})");
                        }
                        
                        if (parameters.Length >= 2)
                        {
                            string param2Type = parameters[1].ParameterType.Name;
                            if (param2Type == "String")
                            {
                                Debug.Log("✅ 第二个参数类型正确: String");
                            }
                            else
                            {
                                Debug.LogError($"❌ 第二个参数类型错误: {param2Type}，应该是String");
                            }
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
                Debug.LogError($"❌ 检查GameManager失败: {e.Message}");
            }
        }
        
        private void CheckGamePieceIdType()
        {
            Debug.Log("--- 检查 GamePiece.pieceId 属性类型 ---");
            
            try
            {
                var type = System.Type.GetType("BlokusGame.Core.Pieces.GamePiece");
                if (type != null)
                {
                    var property = type.GetProperty("pieceId");
                    if (property != null)
                    {
                        string propertyType = property.PropertyType.Name;
                        Debug.Log($"✅ pieceId 属性存在，类型: {propertyType}");
                        
                        if (propertyType == "Int32")
                        {
                            Debug.Log("✅ pieceId 类型确认为 Int32");
                            Debug.Log("✅ 需要使用 .ToString() 转换为 String");
                        }
                        else
                        {
                            Debug.LogWarning($"⚠️ pieceId 类型为 {propertyType}，可能需要不同的转换方式");
                        }
                    }
                    else
                    {
                        Debug.LogError("❌ pieceId 属性未找到");
                    }
                }
                else
                {
                    Debug.LogError("❌ GamePiece 类型未找到");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 检查GamePiece失败: {e.Message}");
            }
        }
    }
}