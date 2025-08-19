using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace BlokusGame.Core.Data
{
    /// <summary>
    /// Blokus标准方块定义 - 包含所有21种标准Blokus方块的形状数据
    /// 这是一个静态类，提供标准方块的形状定义和创建方法
    /// 方块编号1-21对应不同大小和形状的方块
    /// </summary>
    public static class BlokusPieceDefinitions
    {
        /// <summary>
        /// 所有标准Blokus方块的形状定义字典
        /// 键为方块ID（1-21），值为该方块的相对坐标数组
        /// </summary>
        public static readonly Dictionary<int, Vector2Int[]> standardShapes = new Dictionary<int, Vector2Int[]>
        {
            // 1格方块 (1个)
            { 1, new Vector2Int[] { 
                new Vector2Int(0, 0) 
            }},
            
            // 2格方块 (1个)
            { 2, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0) 
            }},
            
            // 3格方块 (2个)
            // I形 (直线)
            { 3, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) 
            }},
            // L形 (弯角)
            { 4, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) 
            }},
            
            // 4格方块 (5个)
            // I形 (直线)
            { 5, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0) 
            }},
            // O形 (正方形)
            { 6, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) 
            }},
            // L形 (长L)
            { 7, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1) 
            }},
            // T形 (T字形)
            { 8, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1) 
            }},
            // Z形 (Z字形)
            { 9, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1) 
            }},
            
            // 5格方块 (12个)
            // I形 (长直线)
            { 10, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0), new Vector2Int(4, 0) 
            }},
            // L形 (长L)
            { 11, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0), new Vector2Int(0, 1) 
            }},
            // Y形 (Y字形)
            { 12, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0), new Vector2Int(1, 1) 
            }},
            // N形 (N字形)
            { 13, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(3, 1) 
            }},
            // P形 (P字形)
            { 14, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(0, 2) 
            }},
            // U形 (U字形)
            { 15, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(2, 0), new Vector2Int(2, 1) 
            }},
            // T形 (长T)
            { 16, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1), new Vector2Int(1, 2) 
            }},
            // V形 (V字形)
            { 17, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) 
            }},
            // W形 (W字形)
            { 18, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(2, 2) 
            }},
            // Z形 (长Z)
            { 19, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(2, 2) 
            }},
            // F形 (F字形)
            { 20, new Vector2Int[] { 
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2) 
            }},
            // X形 (十字形)
            { 21, new Vector2Int[] { 
                new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2) 
            }}
        };
        
        /// <summary>
        /// 方块名称映射，提供每个方块的标准名称
        /// </summary>
        public static readonly Dictionary<int, string> pieceNames = new Dictionary<int, string>
        {
            { 1, "单格" },
            { 2, "双格" },
            { 3, "三格直线" },
            { 4, "三格L形" },
            { 5, "四格直线" },
            { 6, "四格正方形" },
            { 7, "四格L形" },
            { 8, "四格T形" },
            { 9, "四格Z形" },
            { 10, "五格直线" },
            { 11, "五格L形" },
            { 12, "五格Y形" },
            { 13, "五格N形" },
            { 14, "五格P形" },
            { 15, "五格U形" },
            { 16, "五格T形" },
            { 17, "五格V形" },
            { 18, "五格W形" },
            { 19, "五格Z形" },
            { 20, "五格F形" },
            { 21, "五格X形" }
        };
        
        /// <summary>
        /// 方块描述映射，提供每个方块的详细描述
        /// </summary>
        public static readonly Dictionary<int, string> pieceDescriptions = new Dictionary<int, string>
        {
            { 1, "最小的方块，只有一个格子" },
            { 2, "两个格子组成的直线方块" },
            { 3, "三个格子组成的直线方块" },
            { 4, "三个格子组成的L形方块" },
            { 5, "四个格子组成的直线方块" },
            { 6, "四个格子组成的正方形方块" },
            { 7, "四个格子组成的L形方块" },
            { 8, "四个格子组成的T形方块" },
            { 9, "四个格子组成的Z形方块" },
            { 10, "五个格子组成的直线方块，最长的方块" },
            { 11, "五个格子组成的L形方块" },
            { 12, "五个格子组成的Y形方块" },
            { 13, "五个格子组成的N形方块" },
            { 14, "五个格子组成的P形方块" },
            { 15, "五个格子组成的U形方块" },
            { 16, "五个格子组成的T形方块" },
            { 17, "五个格子组成的V形方块" },
            { 18, "五个格子组成的W形方块" },
            { 19, "五个格子组成的Z形方块" },
            { 20, "五个格子组成的F形方块" },
            { 21, "五个格子组成的X形方块，十字形状" }
        };
        
        /// <summary>
        /// 获取指定方块的形状数据
        /// </summary>
        /// <param name="_pieceId">方块ID（1-21）</param>
        /// <returns>方块形状坐标数组，如果ID无效返回null</returns>
        public static Vector2Int[] getShapeById(int _pieceId)
        {
            if (standardShapes.ContainsKey(_pieceId))
            {
                return (Vector2Int[])standardShapes[_pieceId].Clone();
            }
            
            Debug.LogError($"[BlokusPieceDefinitions] 无效的方块ID: {_pieceId}");
            return null;
        }
        
        /// <summary>
        /// 获取指定方块的名称
        /// </summary>
        /// <param name="_pieceId">方块ID（1-21）</param>
        /// <returns>方块名称，如果ID无效返回"未知方块"</returns>
        public static string getNameById(int _pieceId)
        {
            if (pieceNames.ContainsKey(_pieceId))
            {
                return pieceNames[_pieceId];
            }
            
            Debug.LogWarning($"[BlokusPieceDefinitions] 无效的方块ID: {_pieceId}");
            return "未知方块";
        }
        
        /// <summary>
        /// 获取指定方块的描述
        /// </summary>
        /// <param name="_pieceId">方块ID（1-21）</param>
        /// <returns>方块描述，如果ID无效返回"无描述"</returns>
        public static string getDescriptionById(int _pieceId)
        {
            if (pieceDescriptions.ContainsKey(_pieceId))
            {
                return pieceDescriptions[_pieceId];
            }
            
            Debug.LogWarning($"[BlokusPieceDefinitions] 无效的方块ID: {_pieceId}");
            return "无描述";
        }
        
        /// <summary>
        /// 创建指定方块的PieceData实例
        /// </summary>
        /// <param name="_pieceId">方块ID（1-21）</param>
        /// <returns>配置好的PieceData实例</returns>
        public static PieceData createPieceData(int _pieceId)
        {
            var shape = getShapeById(_pieceId);
            if (shape == null)
            {
                Debug.LogError($"[BlokusPieceDefinitions] 无法创建方块数据，无效的ID: {_pieceId}");
                return null;
            }
            
            var pieceData = ScriptableObject.CreateInstance<PieceData>();
            
            // 使用反射设置私有字段（仅用于运行时创建）
            var pieceIdField = typeof(PieceData).GetField("_m_pieceId", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var pieceNameField = typeof(PieceData).GetField("_m_pieceName", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var descriptionField = typeof(PieceData).GetField("_m_description", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var originalShapeField = typeof(PieceData).GetField("_m_originalShape", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var sizeField = typeof(PieceData).GetField("_m_size", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            pieceIdField?.SetValue(pieceData, _pieceId);
            pieceNameField?.SetValue(pieceData, getNameById(_pieceId));
            descriptionField?.SetValue(pieceData, getDescriptionById(_pieceId));
            originalShapeField?.SetValue(pieceData, shape);
            sizeField?.SetValue(pieceData, shape.Length);
            
            Debug.Log($"[BlokusPieceDefinitions] 创建方块数据: {getNameById(_pieceId)} (ID: {_pieceId})");
            return pieceData;
        }
        
        /// <summary>
        /// 验证所有方块定义的完整性
        /// </summary>
        /// <returns>验证是否通过</returns>
        public static bool validateAllDefinitions()
        {
            bool isValid = true;
            
            // 检查是否有21个方块
            if (standardShapes.Count != 21)
            {
                Debug.LogError($"[BlokusPieceDefinitions] 方块数量不正确，期望21个，实际{standardShapes.Count}个");
                isValid = false;
            }
            
            // 检查每个方块的数据完整性
            for (int i = 1; i <= 21; i++)
            {
                if (!standardShapes.ContainsKey(i))
                {
                    Debug.LogError($"[BlokusPieceDefinitions] 缺少方块ID {i} 的形状定义");
                    isValid = false;
                    continue;
                }
                
                if (!pieceNames.ContainsKey(i))
                {
                    Debug.LogError($"[BlokusPieceDefinitions] 缺少方块ID {i} 的名称定义");
                    isValid = false;
                }
                
                if (!pieceDescriptions.ContainsKey(i))
                {
                    Debug.LogError($"[BlokusPieceDefinitions] 缺少方块ID {i} 的描述定义");
                    isValid = false;
                }
                
                // 验证形状数据
                var shape = standardShapes[i];
                if (shape == null || shape.Length == 0)
                {
                    Debug.LogError($"[BlokusPieceDefinitions] 方块ID {i} 的形状数据为空");
                    isValid = false;
                }
            }
            
            if (isValid)
            {
                Debug.Log("[BlokusPieceDefinitions] 所有方块定义验证通过");
            }
            
            return isValid;
        }
        
        /// <summary>
        /// 获取所有方块ID的列表
        /// </summary>
        /// <returns>方块ID列表</returns>
        public static List<int> getAllPieceIds()
        {
            return new List<int>(standardShapes.Keys);
        }
        
        /// <summary>
        /// 获取指定大小的所有方块ID
        /// </summary>
        /// <param name="_size">方块大小（格子数量）</param>
        /// <returns>指定大小的方块ID列表</returns>
        public static List<int> getPieceIdsBySize(int _size)
        {
            List<int> result = new List<int>();
            
            foreach (var kvp in standardShapes)
            {
                if (kvp.Value.Length == _size)
                {
                    result.Add(kvp.Key);
                }
            }
            
            return result;
        }
    }
}