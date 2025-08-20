using UnityEngine;
using System.Collections.Generic;
using BlokusGame.Core.Events;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Pieces;
using BlokusGame.Core.Board;
using BlokusGame.Core.Managers;

namespace BlokusGame.Core.InputSystem
{
    /// <summary>
    /// 触摸游戏逻辑集成系统 - 连接触摸输入与游戏逻辑
    /// 处理方块拖拽到棋盘的完整流程，包括位置验证、预览显示等
    /// 提供触摸操作与Blokus游戏规则的深度集成
    /// </summary>
    public class TouchGameplayIntegration : MonoBehaviour
    {
        [Header("拖拽配置")]
        /// <summary>拖拽预览更新频率（秒）</summary>
        [SerializeField] private float _m_previewUpdateInterval = 0.1f;
        
        /// <summary>拖拽到棋盘的有效距离</summary>
        [SerializeField] private float _m_boardDropDistance = 2f;
        
        /// <summary>自动吸附到有效位置的距离</summary>
        [SerializeField] private float _m_snapDistance = 1f;
        
        [Header("反馈配置")]
        /// <summary>是否启用拖拽预览</summary>
        [SerializeField] private bool _m_enableDragPreview = true;
        
        /// <summary>是否启用位置吸附</summary>
        [SerializeField] private bool _m_enablePositionSnap = true;
        
        /// <summary>是否启用触觉反馈</summary>
        [SerializeField] private bool _m_enableHapticFeedback = true;
        
        // 私有字段
        /// <summary>游戏管理器引用</summary>
        private GameManager _m_gameManager;
        
        /// <summary>棋盘管理器引用</summary>
        private BoardManager _m_boardManager;
        
        /// <summary>棋盘可视化器引用</summary>
        private BoardVisualizer _m_boardVisualizer;
        
        /// <summary>触摸反馈系统引用</summary>
        private TouchFeedbackSystem _m_feedbackSystem;
        
        /// <summary>当前拖拽的方块</summary>
        private GamePiece _m_currentDragPiece;
        
        /// <summary>当前预览位置</summary>
        private Vector2Int _m_currentPreviewPosition;
        
        /// <summary>当前位置是否有效</summary>
        private bool _m_isCurrentPositionValid;
        
        /// <summary>上次预览更新时间</summary>
        private float _m_lastPreviewUpdateTime;
        
        /// <summary>拖拽开始时的方块位置</summary>
        private Vector3 _m_dragStartPosition;
        
        /// <summary>当前有效位置列表</summary>
        private List<Vector2Int> _m_validPositions;
        
        /// <summary>摄像机引用</summary>
        private Camera _m_camera;
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法 - 初始化组件引用
        /// </summary>
        private void Awake()
        {
            _m_camera = Camera.main;
            if (_m_camera == null)
            {
                _m_camera = FindObjectOfType<Camera>();
            }
            
            _m_validPositions = new List<Vector2Int>();
        }
        
        /// <summary>
        /// Unity Start方法 - 获取管理器引用并订阅事件
        /// </summary>
        private void Start()
        {
            _initializeReferences();
            _subscribeToEvents();
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 清理事件订阅
        /// </summary>
        private void OnDestroy()
        {
            _unsubscribeFromEvents();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 设置触摸反馈系统
        /// </summary>
        /// <param name="_feedbackSystem">触摸反馈系统实例</param>
        public void setFeedbackSystem(TouchFeedbackSystem _feedbackSystem)
        {
            _m_feedbackSystem = _feedbackSystem;
        }
        
        /// <summary>
        /// 启用或禁用拖拽预览
        /// </summary>
        /// <param name="_enabled">是否启用</param>
        public void setDragPreviewEnabled(bool _enabled)
        {
            _m_enableDragPreview = _enabled;
        }
        
        /// <summary>
        /// 启用或禁用位置吸附
        /// </summary>
        /// <param name="_enabled">是否启用</param>
        public void setPositionSnapEnabled(bool _enabled)
        {
            _m_enablePositionSnap = _enabled;
        }
        
        #endregion
        
        #region 私有方法 - 初始化
        
        /// <summary>
        /// 初始化组件引用
        /// </summary>
        private void _initializeReferences()
        {
            _m_gameManager = FindObjectOfType<GameManager>();
            _m_boardManager = FindObjectOfType<BoardManager>();
            _m_boardVisualizer = FindObjectOfType<BoardVisualizer>();
            _m_feedbackSystem = FindObjectOfType<TouchFeedbackSystem>();
            
            if (_m_gameManager == null)
            {
                Debug.LogError("[TouchGameplayIntegration] 未找到GameManager");
            }
            
            if (_m_boardManager == null)
            {
                Debug.LogError("[TouchGameplayIntegration] 未找到BoardManager");
            }
            
            if (_m_boardVisualizer == null)
            {
                Debug.LogError("[TouchGameplayIntegration] 未找到BoardVisualizer");
            }
        }
        
        /// <summary>
        /// 订阅游戏事件
        /// </summary>
        private void _subscribeToEvents()
        {
            if (GameEvents.instance != null)
            {
                GameEvents.instance.onPieceDragStart += _onPieceDragStart;
                GameEvents.instance.onPieceDragging += _onPieceDragging;
                GameEvents.instance.onPieceDragEnd += _onPieceDragEnd;
                GameEvents.instance.onPieceSelected += _onPieceSelected;
            }
        }
        
        /// <summary>
        /// 取消事件订阅
        /// </summary>
        private void _unsubscribeFromEvents()
        {
            if (GameEvents.instance != null)
            {
                GameEvents.instance.onPieceDragStart -= _onPieceDragStart;
                GameEvents.instance.onPieceDragging -= _onPieceDragging;
                GameEvents.instance.onPieceDragEnd -= _onPieceDragEnd;
                GameEvents.instance.onPieceSelected -= _onPieceSelected;
            }
        }
        
        #endregion
        
        #region 事件处理方法
        
        /// <summary>
        /// 方块拖拽开始事件处理
        /// </summary>
        /// <param name="_piece">开始拖拽的方块</param>
        private void _onPieceDragStart(_IGamePiece _piece)
        {
            if (_piece is GamePiece gamePiece)
            {
                _m_currentDragPiece = gamePiece;
                _m_dragStartPosition = gamePiece.transform.position;
                
                // 获取当前玩家的有效放置位置
                _updateValidPositions();
                
                // 显示有效位置高亮
                if (_m_boardVisualizer != null && _m_validPositions.Count > 0)
                {
                    _m_boardVisualizer.highlightValidPositions(_m_validPositions, gamePiece.playerId);
                }
                
                Debug.Log($"[TouchGameplayIntegration] 开始拖拽方块 {gamePiece.pieceId}，找到 {_m_validPositions.Count} 个有效位置");
            }
        }
        
        /// <summary>
        /// 方块拖拽中事件处理
        /// </summary>
        /// <param name="_piece">正在拖拽的方块</param>
        /// <param name="_worldPosition">当前世界坐标</param>
        private void _onPieceDragging(_IGamePiece _piece, Vector3 _worldPosition)
        {
            if (_m_currentDragPiece == null || _piece != _m_currentDragPiece) return;
            
            // 限制预览更新频率
            if (Time.time - _m_lastPreviewUpdateTime < _m_previewUpdateInterval) return;
            
            _m_lastPreviewUpdateTime = Time.time;
            
            // 将世界坐标转换为棋盘坐标
            Vector2Int boardPosition = _worldToBoardPosition(_worldPosition);
            
            // 检查位置是否在棋盘范围内
            if (!_isPositionInBounds(boardPosition)) return;
            
            // 如果启用位置吸附，寻找最近的有效位置
            if (_m_enablePositionSnap)
            {
                Vector2Int snappedPosition = _findNearestValidPosition(boardPosition);
                if (snappedPosition != Vector2Int.one * -1) // -1表示没找到有效位置
                {
                    boardPosition = snappedPosition;
                }
            }
            
            // 验证位置是否有效
            bool isValidPosition = _validatePiecePosition(boardPosition);
            
            // 更新预览
            if (_m_enableDragPreview && _m_boardVisualizer != null)
            {
                _m_boardVisualizer.showPiecePreview(_piece, boardPosition, isValidPosition);
            }
            
            // 更新当前状态
            _m_currentPreviewPosition = boardPosition;
            _m_isCurrentPositionValid = isValidPosition;
            
            // 提供触觉反馈
            if (_m_enableHapticFeedback && _m_feedbackSystem != null)
            {
                if (isValidPosition && !_m_isCurrentPositionValid)
                {
                    // 进入有效位置时的反馈
                    _m_feedbackSystem.playHapticFeedback(TouchFeedbackSystem.FeedbackType.Light);
                }
            }
        }
        
        /// <summary>
        /// 方块拖拽结束事件处理
        /// </summary>
        /// <param name="_piece">结束拖拽的方块</param>
        /// <param name="_worldPosition">最终世界坐标</param>
        private void _onPieceDragEnd(_IGamePiece _piece, Vector3 _worldPosition)
        {
            if (_m_currentDragPiece == null || _piece != _m_currentDragPiece) return;
            
            // 隐藏预览和高亮
            if (_m_boardVisualizer != null)
            {
                _m_boardVisualizer.hidePiecePreview();
                _m_boardVisualizer.clearHighlights();
            }
            
            // 尝试放置方块
            bool placementSuccessful = _attemptPiecePlacement(_worldPosition);
            
            if (!placementSuccessful)
            {
                // 放置失败，返回原位置
                _returnPieceToOriginalPosition();
                
                // 播放错误反馈
                if (_m_enableHapticFeedback && _m_feedbackSystem != null)
                {
                    _m_feedbackSystem.playHapticFeedback(TouchFeedbackSystem.FeedbackType.Error);
                }
                
                // 显示错误消息
                if (GameEvents.instance != null)
                {
                    GameEvents.instance.onShowMessage?.Invoke("无法在此位置放置方块", MessageType.Warning);
                }
            }
            else
            {
                // 放置成功，播放成功反馈
                if (_m_enableHapticFeedback && _m_feedbackSystem != null)
                {
                    _m_feedbackSystem.playHapticFeedback(TouchFeedbackSystem.FeedbackType.Success);
                }
            }
            
            // 清理状态
            _m_currentDragPiece = null;
            _m_currentPreviewPosition = Vector2Int.zero;
            _m_isCurrentPositionValid = false;
        }
        
        /// <summary>
        /// 方块选择事件处理
        /// </summary>
        /// <param name="_piece">被选择的方块</param>
        /// <param name="_playerId">玩家ID</param>
        private void _onPieceSelected(_IGamePiece _piece, int _playerId)
        {
            // 当选择新方块时，更新有效位置
            if (_piece is GamePiece gamePiece && !gamePiece.isPlaced)
            {
                _updateValidPositionsForPiece(gamePiece);
            }
        }
        
        #endregion
        
        #region 私有方法 - 游戏逻辑
        
        /// <summary>
        /// 更新当前玩家的有效放置位置
        /// </summary>
        private void _updateValidPositions()
        {
            if (_m_currentDragPiece == null || _m_gameManager == null) return;
            
            _updateValidPositionsForPiece(_m_currentDragPiece);
        }
        
        /// <summary>
        /// 更新指定方块的有效放置位置
        /// </summary>
        /// <param name="_piece">方块</param>
        private void _updateValidPositionsForPiece(GamePiece _piece)
        {
            _m_validPositions.Clear();
            
            if (_m_boardManager == null || _piece == null) return;
            
            // 遍历棋盘上的所有位置
            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    
                    // 检查这个位置是否可以放置方块
                    if (_validatePiecePositionInternal(_piece, position))
                    {
                        _m_validPositions.Add(position);
                    }
                }
            }
            
            Debug.Log($"[TouchGameplayIntegration] 方块 {_piece.pieceId} 有 {_m_validPositions.Count} 个有效位置");
        }
        
        /// <summary>
        /// 验证方块位置是否有效
        /// </summary>
        /// <param name="_boardPosition">棋盘坐标</param>
        /// <returns>是否有效</returns>
        private bool _validatePiecePosition(Vector2Int _boardPosition)
        {
            if (_m_currentDragPiece == null) return false;
            
            return _validatePiecePositionInternal(_m_currentDragPiece, _boardPosition);
        }
        
        /// <summary>
        /// 内部方块位置验证方法
        /// </summary>
        /// <param name="_piece">方块</param>
        /// <param name="_boardPosition">棋盘坐标</param>
        /// <returns>是否有效</returns>
        private bool _validatePiecePositionInternal(GamePiece _piece, Vector2Int _boardPosition)
        {
            if (_m_boardManager == null || _piece == null) return false;
            
            // 使用棋盘管理器验证位置
            var boardSystem = _m_boardManager.getBoardSystem();
            if (boardSystem == null) return false;
            
            // 检查方块是否可以放置在指定位置
            return boardSystem.isValidPlacement(_piece, _boardPosition, _piece.playerId);
        }
        
        /// <summary>
        /// 尝试放置方块
        /// </summary>
        /// <param name="_worldPosition">世界坐标</param>
        /// <returns>是否放置成功</returns>
        private bool _attemptPiecePlacement(Vector3 _worldPosition)
        {
            if (_m_currentDragPiece == null || _m_gameManager == null) return false;
            
            // 检查是否拖拽到棋盘附近
            if (!_isNearBoard(_worldPosition))
            {
                return false;
            }
            
            Vector2Int boardPosition = _worldToBoardPosition(_worldPosition);
            
            // 如果启用位置吸附，使用最近的有效位置
            if (_m_enablePositionSnap)
            {
                Vector2Int snappedPosition = _findNearestValidPosition(boardPosition);
                if (snappedPosition != Vector2Int.one * -1)
                {
                    boardPosition = snappedPosition;
                }
            }
            
            // 验证位置
            if (!_validatePiecePosition(boardPosition))
            {
                return false;
            }
            
            // 尝试通过游戏管理器放置方块
            return _m_gameManager.tryPlacePiece(_m_currentDragPiece.playerId, _m_currentDragPiece.pieceId, boardPosition);
        }
        
        /// <summary>
        /// 将方块返回到原始位置
        /// </summary>
        private void _returnPieceToOriginalPosition()
        {
            if (_m_currentDragPiece != null)
            {
                _m_currentDragPiece.transform.position = _m_dragStartPosition;
            }
        }
        
        /// <summary>
        /// 寻找最近的有效位置
        /// </summary>
        /// <param name="_targetPosition">目标位置</param>
        /// <returns>最近的有效位置，如果没找到返回(-1,-1)</returns>
        private Vector2Int _findNearestValidPosition(Vector2Int _targetPosition)
        {
            Vector2Int nearestPosition = Vector2Int.one * -1;
            float nearestDistance = float.MaxValue;
            
            foreach (Vector2Int validPos in _m_validPositions)
            {
                float distance = Vector2Int.Distance(_targetPosition, validPos);
                if (distance < nearestDistance && distance <= _m_snapDistance)
                {
                    nearestDistance = distance;
                    nearestPosition = validPos;
                }
            }
            
            return nearestPosition;
        }
        
        #endregion
        
        #region 私有方法 - 坐标转换和验证
        
        /// <summary>
        /// 世界坐标转换为棋盘坐标
        /// </summary>
        /// <param name="_worldPosition">世界坐标</param>
        /// <returns>棋盘坐标</returns>
        private Vector2Int _worldToBoardPosition(Vector3 _worldPosition)
        {
            // 假设棋盘格子大小为1单位，棋盘从(0,0)开始
            int x = Mathf.RoundToInt(_worldPosition.x);
            int z = Mathf.RoundToInt(_worldPosition.z);
            
            return new Vector2Int(x, z);
        }
        
        /// <summary>
        /// 检查位置是否在棋盘范围内
        /// </summary>
        /// <param name="_boardPosition">棋盘坐标</param>
        /// <returns>是否在范围内</returns>
        private bool _isPositionInBounds(Vector2Int _boardPosition)
        {
            return _boardPosition.x >= 0 && _boardPosition.x < 20 && 
                   _boardPosition.y >= 0 && _boardPosition.y < 20;
        }
        
        /// <summary>
        /// 检查世界坐标是否靠近棋盘
        /// </summary>
        /// <param name="_worldPosition">世界坐标</param>
        /// <returns>是否靠近棋盘</returns>
        private bool _isNearBoard(Vector3 _worldPosition)
        {
            // 检查Y坐标是否在合理范围内（接近棋盘高度）
            if (_worldPosition.y > _m_boardDropDistance)
            {
                return false;
            }
            
            // 检查XZ坐标是否在棋盘附近
            Vector2Int boardPos = _worldToBoardPosition(_worldPosition);
            return _isPositionInBounds(boardPos);
        }
        
        #endregion
    }
}