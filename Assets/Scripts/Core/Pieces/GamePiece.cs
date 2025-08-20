using UnityEngine;
using System.Collections.Generic;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Data;

namespace BlokusGame.Core.Pieces
{
    /// <summary>
    /// 游戏方块类 - 实现_IGamePiece接口
    /// 代表游戏中的一个具体方块实例，包含形状、状态和变换逻辑
    /// 支持旋转、翻转和位置计算等核心功能
    /// </summary>
    public class GamePiece : MonoBehaviour, _IGamePiece
    {
        [Header("方块基本信息")]
        /// <summary>方块数据引用，包含形状和属性信息</summary>
        [SerializeField] private PieceData _m_pieceData;
        
        /// <summary>方块所属的玩家ID</summary>
        [SerializeField] private int _m_playerId;
        
        /// <summary>方块的颜色</summary>
        [SerializeField] private Color _m_pieceColor = Color.white;
        
        [Header("可视化组件")]
        /// <summary>方块可视化组件</summary>
        private PieceVisualizer _m_pieceVisualizer;
        
        /// <summary>方块交互控制器</summary>
        private PieceInteractionController _m_interactionController;
        
        [Header("方块状态")]
        /// <summary>方块是否已被放置在棋盘上</summary>
        [SerializeField] private bool _m_isPlaced = false;
        
        /// <summary>当前旋转次数（0-3，每次90度）</summary>
        [SerializeField] private int _m_rotationCount = 0;
        
        /// <summary>是否水平翻转</summary>
        [SerializeField] private bool _m_isFlipped = false;
        
        // 缓存的当前形状数据，避免重复计算
        private Vector2Int[] _m_cachedCurrentShape;
        private bool _m_shapeCacheDirty = true;
        
        #region _IGamePiece接口实现
        
        /// <summary>方块的唯一标识符</summary>
        public int pieceId => _m_pieceData?.pieceId ?? -1;
        
        /// <summary>方块所属的玩家ID</summary>
        public int playerId => _m_playerId;
        
        /// <summary>方块的当前形状数据（相对坐标数组）</summary>
        public Vector2Int[] currentShape
        {
            get
            {
                if (_m_shapeCacheDirty || _m_cachedCurrentShape == null)
                {
                    _updateCurrentShape();
                }
                return _m_cachedCurrentShape;
            }
        }
        
        /// <summary>方块的颜色</summary>
        public Color pieceColor => _m_pieceColor;
        
        /// <summary>方块是否已被放置在棋盘上</summary>
        public bool isPlaced => _m_isPlaced;
        
        /// <summary>
        /// 顺时针旋转方块90度
        /// </summary>
        public void rotate90Clockwise()
        {
            _m_rotationCount = (_m_rotationCount + 1) % 4;
            _m_shapeCacheDirty = true;
            
            // 更新可视化
            if (_m_pieceVisualizer != null)
            {
                _m_pieceVisualizer.initialize(this); // 重新生成可视化
            }
            
            Debug.Log($"[GamePiece] 方块 {pieceId} 旋转到 {_m_rotationCount * 90} 度");
        }
        
        /// <summary>
        /// 水平翻转方块
        /// </summary>
        public void flipHorizontal()
        {
            _m_isFlipped = !_m_isFlipped;
            _m_shapeCacheDirty = true;
            
            // 更新可视化
            if (_m_pieceVisualizer != null)
            {
                _m_pieceVisualizer.initialize(this); // 重新生成可视化
            }
            
            Debug.Log($"[GamePiece] 方块 {pieceId} 翻转状态: {_m_isFlipped}");
        }
        
        /// <summary>
        /// 获取方块在指定位置的所有占用坐标
        /// </summary>
        /// <param name="_position">方块放置的基准位置</param>
        /// <returns>方块占用的所有棋盘坐标</returns>
        public List<Vector2Int> getOccupiedPositions(Vector2Int _position)
        {
            List<Vector2Int> occupiedPositions = new List<Vector2Int>();
            
            foreach (Vector2Int relativePos in currentShape)
            {
                occupiedPositions.Add(_position + relativePos);
            }
            
            return occupiedPositions;
        }
        
        /// <summary>
        /// 设置方块的放置状态
        /// </summary>
        /// <param name="_placed">是否已放置</param>
        public void setPlacedState(bool _placed)
        {
            _m_isPlaced = _placed;
            
            // 更新交互状态
            if (_m_interactionController != null)
            {
                if (_placed)
                {
                    _m_interactionController.setInteractionState(PieceInteractionController.InteractionState.Placed);
                    _m_interactionController.setInteractionEnabled(false);
                }
                else
                {
                    _m_interactionController.setInteractionState(PieceInteractionController.InteractionState.Idle);
                    _m_interactionController.setInteractionEnabled(true);
                }
            }
            
            Debug.Log($"[GamePiece] 方块 {pieceId} 放置状态设置为: {_placed}");
        }
        
        /// <summary>
        /// 重置方块到初始状态（未旋转、未翻转）
        /// </summary>
        public void resetToOriginalState()
        {
            _m_rotationCount = 0;
            _m_isFlipped = false;
            _m_isPlaced = false;
            _m_shapeCacheDirty = true;
            
            Debug.Log($"[GamePiece] 方块 {pieceId} 重置到初始状态");
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 初始化方块
        /// </summary>
        /// <param name="_pieceData">方块数据</param>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_color">方块颜色</param>
        public void initialize(PieceData _pieceData, int _playerId, Color _color)
        {
            _m_pieceData = _pieceData;
            _m_playerId = _playerId;
            _m_pieceColor = _color;
            _m_shapeCacheDirty = true;
            
            // 初始化可视化组件
            if (_m_pieceVisualizer != null)
            {
                _m_pieceVisualizer.initialize(this);
            }
            
            Debug.Log($"[GamePiece] 初始化方块 {pieceId}，玩家 {_playerId}");
        }
        
        /// <summary>
        /// 获取方块的边界矩形
        /// </summary>
        /// <returns>包含方块所有格子的最小矩形</returns>
        public RectInt getBounds()
        {
            if (currentShape == null || currentShape.Length == 0)
                return new RectInt(0, 0, 0, 0);
            
            int minX = currentShape[0].x;
            int maxX = currentShape[0].x;
            int minY = currentShape[0].y;
            int maxY = currentShape[0].y;
            
            foreach (Vector2Int pos in currentShape)
            {
                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.y > maxY) maxY = pos.y;
            }
            
            return new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }
        
        /// <summary>
        /// 检查方块是否可以放置在指定位置（不考虑游戏规则，只检查边界）
        /// </summary>
        /// <param name="_position">放置位置</param>
        /// <param name="_boardSize">棋盘大小</param>
        /// <returns>是否可以放置</returns>
        public bool canPlaceAt(Vector2Int _position, Vector2Int _boardSize)
        {
            foreach (Vector2Int relativePos in currentShape)
            {
                Vector2Int worldPos = _position + relativePos;
                
                // 检查是否超出棋盘边界
                if (worldPos.x < 0 || worldPos.x >= _boardSize.x ||
                    worldPos.y < 0 || worldPos.y >= _boardSize.y)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取当前旋转次数
        /// </summary>
        /// <returns>旋转次数（0-3）</returns>
        public int getRotationCount()
        {
            return _m_rotationCount;
        }
        
        /// <summary>
        /// 获取是否翻转状态
        /// </summary>
        /// <returns>是否翻转</returns>
        public bool getFlippedState()
        {
            return _m_isFlipped;
        }
        
        /// <summary>
        /// 设置方块变换状态
        /// </summary>
        /// <param name="_rotationCount">旋转次数</param>
        /// <param name="_isFlipped">是否翻转</param>
        public void setTransform(int _rotationCount, bool _isFlipped)
        {
            _m_rotationCount = _rotationCount % 4;
            if (_m_rotationCount < 0) _m_rotationCount += 4;
            
            _m_isFlipped = _isFlipped;
            _m_shapeCacheDirty = true;
            
            Debug.Log($"[GamePiece] 方块 {pieceId} 设置变换: 旋转{_m_rotationCount * 90}度, 翻转{_isFlipped}");
        }
        
        /// <summary>
        /// 设置方块的放置状态
        /// </summary>
        /// <param name="_placed">是否已放置</param>
        public void setPlaced(bool _placed)
        {
            _m_isPlaced = _placed;
            
            // 更新可视化状态
            if (_m_pieceVisualizer != null)
            {
                if (_placed)
                {
                    _m_pieceVisualizer.setVisualState(PieceVisualizer.PieceVisualState.Placed);
                }
                else
                {
                    _m_pieceVisualizer.setVisualState(PieceVisualizer.PieceVisualState.Normal);
                }
            }
            
            // 更新交互状态
            if (_m_interactionController != null)
            {
                _m_interactionController.setInteractionEnabled(!_placed);
            }
            
            Debug.Log($"[GamePiece] 方块 {pieceId} 放置状态设置为: {_placed}");
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 更新当前形状缓存
        /// </summary>
        private void _updateCurrentShape()
        {
            if (_m_pieceData == null)
            {
                _m_cachedCurrentShape = new Vector2Int[0];
                _m_shapeCacheDirty = false;
                return;
            }
            
            _m_cachedCurrentShape = _m_pieceData.getTransformedShape(_m_rotationCount, _m_isFlipped);
            _m_shapeCacheDirty = false;
        }
        
        #endregion
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法 - 初始化组件
        /// </summary>
        private void Awake()
        {
            // 确保形状缓存是最新的
            _m_shapeCacheDirty = true;
            
            // 获取或添加可视化组件
            _m_pieceVisualizer = GetComponent<PieceVisualizer>();
            if (_m_pieceVisualizer == null)
            {
                _m_pieceVisualizer = gameObject.AddComponent<PieceVisualizer>();
            }
            
            // 获取或添加交互控制器
            _m_interactionController = GetComponent<PieceInteractionController>();
            if (_m_interactionController == null)
            {
                _m_interactionController = gameObject.AddComponent<PieceInteractionController>();
            }
        }
        
        /// <summary>
        /// Unity OnValidate方法 - Inspector中数据变化时调用
        /// </summary>
        private void OnValidate()
        {
            // 确保旋转次数在有效范围内
            _m_rotationCount = _m_rotationCount % 4;
            if (_m_rotationCount < 0) _m_rotationCount += 4;
            
            // 标记形状缓存需要更新
            _m_shapeCacheDirty = true;
        }
        
        #endregion
    }
}