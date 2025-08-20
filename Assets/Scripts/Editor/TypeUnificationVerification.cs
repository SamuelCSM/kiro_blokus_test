using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 类型统一验证工具
    /// 验证pieceId在整个系统中的类型一致性
    /// </summary>
    public class TypeUnificationVerification : EditorWindow
    {
        [MenuItem("Blokus/验证工具/类型统一验证")]
        public static void ShowWindow()
        {
            GetWindow<TypeUnificationVerification>("类型统一验证");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("pieceId类型统一验证", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox("验证整个系统中pieceId类型的一致性", MessageType.Info);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("执行完整类型统一验证"))
            {
                PerformFullTypeUnificationCheck();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("分项检查:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("检查接口定义"))
            {
                CheckInterfaceDefinitions();
            }
            
            if (GUILayout.Button("检查数据层"))
            {
                CheckDataLayer();
            }
            
            if (GUILayout.Button("检查管理器层"))
            {
                CheckManagerLayer();
            }
            
            if (GUILayout.Button("检查实现层"))
            {
                CheckImplementationLayer();
            }
        }
        
        private void PerformFullTypeUnificationCheck()
        {
            Debug.Log("=== 开始pieceId类型统一验证 ===");
            
            CheckInterfaceDefinitions();
            CheckDataLayer();
            CheckManagerLayer();
            CheckImplementationLayer();
            
            Debug.Log("=== pieceId类型统一验证完成 ===");
        }
        
        private void CheckInterfaceDefinitions()
        {
            Debug.Log("--- 检查接口定义 ---");
            
            // 检查 _IGamePiece.pieceId
            try
            {
                var gamePieceInterface = System.Type.GetType("BlokusGame.Core.Interfaces._IGamePiece");
                if (gamePieceInterface != null)
                {
                    var pieceIdProperty = gamePieceInterface.GetProperty("pieceId");
                    if (pieceIdProperty != null)
                    {
                        string type = pieceIdProperty.PropertyType.Name;
                        Debug.Log($"✅ _IGamePiece.pieceId 类型: {type}");
                        
                        if (type == "Int32")
                        {
                            Debug.Log("✅ _IGamePiece.pieceId 使用正确的 int 类型");
                        }
                        else
                        {
                            Debug.LogError($"❌ _IGamePiece.pieceId 类型错误: {type}，应该是 Int32");
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 检查 _IGamePiece 失败: {e.Message}");
            }
            
            // 检查 _IPlayer.getPiece 参数类型
            try
            {
                var playerInterface = System.Type.GetType("BlokusGame.Core.Interfaces._IPlayer");
                if (playerInterface != null)
                {
                    var getPieceMethod = playerInterface.GetMethod("getPiece");
                    if (getPieceMethod != null)
                    {
                        var parameters = getPieceMethod.GetParameters();
                        if (parameters.Length > 0)
                        {
                            string paramType = parameters[0].ParameterType.Name;
                            Debug.Log($"✅ _IPlayer.getPiece 参数类型: {paramType}");
                            
                            if (paramType == "Int32")
                            {
                                Debug.Log("✅ _IPlayer.getPiece 使用正确的 int 参数类型");
                            }
                            else
                            {
                                Debug.LogError($"❌ _IPlayer.getPiece 参数类型错误: {paramType}，应该是 Int32");
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 检查 _IPlayer 失败: {e.Message}");
            }
        }
        
        private void CheckDataLayer()
        {
            Debug.Log("--- 检查数据层 ---");
            
            try
            {
                var pieceDataType = System.Type.GetType("BlokusGame.Core.Data.PieceData");
                if (pieceDataType != null)
                {
                    var pieceIdProperty = pieceDataType.GetProperty("pieceId");
                    if (pieceIdProperty != null)
                    {
                        string type = pieceIdProperty.PropertyType.Name;
                        Debug.Log($"✅ PieceData.pieceId 类型: {type}");
                        
                        if (type == "Int32")
                        {
                            Debug.Log("✅ PieceData.pieceId 使用正确的 int 类型");
                        }
                        else
                        {
                            Debug.LogError($"❌ PieceData.pieceId 类型错误: {type}，应该是 Int32");
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 检查 PieceData 失败: {e.Message}");
            }
        }
        
        private void CheckManagerLayer()
        {
            Debug.Log("--- 检查管理器层 ---");
            
            try
            {
                var gameManagerType = System.Type.GetType("BlokusGame.Core.Managers.GameManager");
                if (gameManagerType != null)
                {
                    var tryPlacePieceMethod = gameManagerType.GetMethod("tryPlacePiece");
                    if (tryPlacePieceMethod != null)
                    {
                        var parameters = tryPlacePieceMethod.GetParameters();
                        Debug.Log($"✅ GameManager.tryPlacePiece 方法存在，参数数量: {parameters.Length}");
                        
                        if (parameters.Length >= 2)
                        {
                            string param2Type = parameters[1].ParameterType.Name;
                            Debug.Log($"✅ GameManager.tryPlacePiece 第二个参数类型: {param2Type}");
                            
                            if (param2Type == "Int32")
                            {
                                Debug.Log("✅ GameManager.tryPlacePiece 使用正确的 int 参数类型");
                            }
                            else
                            {
                                Debug.LogError($"❌ GameManager.tryPlacePiece 参数类型错误: {param2Type}，应该是 Int32");
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 检查 GameManager 失败: {e.Message}");
            }
        }
        
        private void CheckImplementationLayer()
        {
            Debug.Log("--- 检查实现层 ---");
            
            try
            {
                var gamePieceType = System.Type.GetType("BlokusGame.Core.Pieces.GamePiece");
                if (gamePieceType != null)
                {
                    var pieceIdProperty = gamePieceType.GetProperty("pieceId");
                    if (pieceIdProperty != null)
                    {
                        string type = pieceIdProperty.PropertyType.Name;
                        Debug.Log($"✅ GamePiece.pieceId 类型: {type}");
                        
                        if (type == "Int32")
                        {
                            Debug.Log("✅ GamePiece.pieceId 使用正确的 int 类型");
                        }
                        else
                        {
                            Debug.LogError($"❌ GamePiece.pieceId 类型错误: {type}，应该是 Int32");
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 检查 GamePiece 失败: {e.Message}");
            }
        }
    }
}