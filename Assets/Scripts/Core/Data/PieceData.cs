using UnityEngine;
using System.Collections.Generic;

namespace BlokusGame.Core.Data
{
    /// <summary>
    /// 方块数据ScriptableObject - 存储Blokus方块的形状和属性信息
    /// 用于在Inspector中配置方块数据，支持运行时加载和管理
    /// 每个PieceData实例代表一种标准Blokus方块类型
    /// </summary>
    [CreateAssetMenu(fileName = "New Piece Data", menuName = "Blokus/Piece Data")]
    public class PieceData : ScriptableObject
    {
        [Header("方块基本信息")]
        /// <summary>方块的唯一标识符，范围1-21对应21种标准Blokus方块</summary>
        [SerializeField] private int _m_pieceId;
        
        /// <summary>方块的显示名称，用于UI显示和调试</summary>
        [SerializeField] private string _m_pieceName;
        
        /// <summary>方块的描述信息，说明方块的特点</summary>
        [SerializeField] [TextArea(2, 4)] private string _m_description;
        
        [Header("方块形状数据")]
        /// <summary>方块的原始形状数据，使用相对坐标表示每个格子的位置</summary>
        [SerializeField] private Vector2Int[] _m_originalShape;
        
        /// <summary>方块占用的格子数量，等于_m_originalShape数组的长度</summary>
        [SerializeField] private int _m_size;
        
        [Header("视觉设置")]
        /// <summary>方块的默认颜色，可以被玩家颜色覆盖</summary>
        [SerializeField] private Color _m_defaultColor = Color.white;
        
        /// <summary>方块的材质引用，用于3D渲染</summary>
        [SerializeField] private Material _m_pieceMaterial;
        
        /// <summary>方块的预制体引用，用于实例化方块对象</summary>
        [SerializeField] private GameObject _m_piecePrefab;
        
        // 公共属性访问器
        /// <summary>方块ID</summary>
        public int pieceId => _m_pieceId;
        
        /// <summary>方块名称</summary>
        public string pieceName => _m_pieceName;
        
        /// <summary>方块描述</summary>
        public string description => _m_description;
        
        /// <summary>方块原始形状</summary>
        public Vector2Int[] originalShape => _m_originalShape;
        
        /// <summary>方块大小</summary>
        public int size => _m_size;
        
        /// <summary>默认颜色</summary>
        public Color defaultColor => _m_defaultColor;
        
        /// <summary>方块材质</summary>
        public Material pieceMaterial => _m_pieceMaterial;
        
        /// <summary>方块预制体</summary>
        public GameObject piecePrefab => _m_piecePrefab;
        
        /// <summary>
        /// 验证方块数据的有效性
        /// 在Inspector中修改数据时自动调用，确保数据的正确性
        /// </summary>
        private void OnValidate()
        {
            // 确保方块ID在有效范围内
            if (_m_pieceId < 1 || _m_pieceId > 21)
            {
                Debug.LogWarning($"[PieceData] 方块ID {_m_pieceId} 超出有效范围(1-21)");
            }
            
            // 确保形状数据不为空
            if (_m_originalShape == null || _m_originalShape.Length == 0)
            {
                Debug.LogWarning($"[PieceData] 方块 {_m_pieceName} 的形状数据为空");
                return;
            }
            
            // 自动更新方块大小
            _m_size = _m_originalShape.Length;
            
            // 验证形状数据的连通性（所有格子必须相互连接）
            if (!_isShapeConnected())
            {
                Debug.LogWarning($"[PieceData] 方块 {_m_pieceName} 的形状不连通，请检查坐标数据");
            }
        }
        
        /// <summary>
        /// 获取方块在指定旋转状态下的形状
        /// </summary>
        /// <param name="_rotationCount">旋转次数（0-3，每次90度顺时针旋转）</param>
        /// <returns>旋转后的形状坐标数组</returns>
        public Vector2Int[] getRotatedShape(int _rotationCount)
        {
            // 确保旋转次数在有效范围内
            _rotationCount = _rotationCount % 4;
            if (_rotationCount < 0) _rotationCount += 4;
            
            Vector2Int[] rotatedShape = new Vector2Int[_m_originalShape.Length];
            
            for (int i = 0; i < _m_originalShape.Length; i++)
            {
                Vector2Int pos = _m_originalShape[i];
                
                // 根据旋转次数应用旋转变换
                for (int r = 0; r < _rotationCount; r++)
                {
                    // 顺时针90度旋转：(x, y) -> (y, -x)
                    int newX = pos.y;
                    int newY = -pos.x;
                    pos = new Vector2Int(newX, newY);
                }
                
                rotatedShape[i] = pos;
            }
            
            // 标准化坐标（移动到原点附近）
            return _normalizeShape(rotatedShape);
        }
        
        /// <summary>
        /// 获取方块水平翻转后的形状
        /// </summary>
        /// <param name="_isFlipped">是否翻转</param>
        /// <returns>翻转后的形状坐标数组</returns>
        public Vector2Int[] getFlippedShape(bool _isFlipped)
        {
            if (!_isFlipped)
                return (Vector2Int[])_m_originalShape.Clone();
            
            Vector2Int[] flippedShape = new Vector2Int[_m_originalShape.Length];
            
            for (int i = 0; i < _m_originalShape.Length; i++)
            {
                // 水平翻转：(x, y) -> (-x, y)
                flippedShape[i] = new Vector2Int(-_m_originalShape[i].x, _m_originalShape[i].y);
            }
            
            // 标准化坐标
            return _normalizeShape(flippedShape);
        }
        
        /// <summary>
        /// 获取方块在指定变换状态下的形状
        /// </summary>
        /// <param name="_rotationCount">旋转次数</param>
        /// <param name="_isFlipped">是否翻转</param>
        /// <returns>变换后的形状坐标数组</returns>
        public Vector2Int[] getTransformedShape(int _rotationCount, bool _isFlipped)
        {
            Vector2Int[] shape = _isFlipped ? getFlippedShape(true) : (Vector2Int[])_m_originalShape.Clone();
            
            // 先翻转再旋转
            if (_rotationCount != 0)
            {
                shape = _rotateShape(shape, _rotationCount);
            }
            
            return _normalizeShape(shape);
        }
        
        /// <summary>
        /// 检查形状是否连通（所有格子通过边相邻连接）
        /// </summary>
        /// <returns>形状是否连通</returns>
        private bool _isShapeConnected()
        {
            if (_m_originalShape == null || _m_originalShape.Length <= 1)
                return true;
            
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            HashSet<Vector2Int> allPositions = new HashSet<Vector2Int>(_m_originalShape);
            
            // 从第一个位置开始BFS
            queue.Enqueue(_m_originalShape[0]);
            visited.Add(_m_originalShape[0]);
            
            // 四个方向的偏移
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            
            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                
                // 检查四个相邻位置
                foreach (Vector2Int dir in directions)
                {
                    Vector2Int neighbor = current + dir;
                    
                    if (allPositions.Contains(neighbor) && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
            
            // 如果访问到的位置数量等于总位置数量，说明形状连通
            return visited.Count == _m_originalShape.Length;
        }
        
        /// <summary>
        /// 标准化形状坐标，将最小坐标移动到(0,0)附近
        /// </summary>
        /// <param name="_shape">要标准化的形状</param>
        /// <returns>标准化后的形状</returns>
        private Vector2Int[] _normalizeShape(Vector2Int[] _shape)
        {
            if (_shape == null || _shape.Length == 0)
                return _shape;
            
            // 找到最小坐标
            int minX = _shape[0].x;
            int minY = _shape[0].y;
            
            foreach (Vector2Int pos in _shape)
            {
                if (pos.x < minX) minX = pos.x;
                if (pos.y < minY) minY = pos.y;
            }
            
            // 将所有坐标减去最小值
            Vector2Int[] normalizedShape = new Vector2Int[_shape.Length];
            for (int i = 0; i < _shape.Length; i++)
            {
                normalizedShape[i] = _shape[i] - new Vector2Int(minX, minY);
            }
            
            return normalizedShape;
        }
        
        /// <summary>
        /// 旋转形状指定次数
        /// </summary>
        /// <param name="_shape">要旋转的形状</param>
        /// <param name="_rotationCount">旋转次数</param>
        /// <returns>旋转后的形状</returns>
        private Vector2Int[] _rotateShape(Vector2Int[] _shape, int _rotationCount)
        {
            _rotationCount = _rotationCount % 4;
            if (_rotationCount < 0) _rotationCount += 4;
            
            Vector2Int[] rotatedShape = new Vector2Int[_shape.Length];
            
            for (int i = 0; i < _shape.Length; i++)
            {
                Vector2Int pos = _shape[i];
                
                for (int r = 0; r < _rotationCount; r++)
                {
                    int newX = pos.y;
                    int newY = -pos.x;
                    pos = new Vector2Int(newX, newY);
                }
                
                rotatedShape[i] = pos;
            }
            
            return rotatedShape;
        }
    }
}