using UnityEngine;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Events;
using BlokusGame.Core.Managers;
using BlokusGame.Core.Pieces;

namespace BlokusGame.Core.InputSystem
{
    /// <summary>
    /// 触摸游戏逻辑集成系统 - 连接触摸输入与游戏逻辑
    /// 处理触摸操作与游戏规则的验证和集成
    /// 提供实时预览和位置验证功能
    /// </summary>
    public class TouchGameplayIntegration : MonoBehaviour
    {
        [Header("集成配置")]
        /// <summary>是否启用实时预览</summary>
        [SerializeField] private bool _m_enableRealTimePreview = true;
        
        /// <summary>是否启用位置验证</summary>
        [SerializeField] private bool _m_enablePositionValidation = true;
        
        /// <summary>是否启用详细日志</summary>
        [SerializeField] private bool _m_enableDetailedLogging = false;
        
        // 私有字段
        /// <summary>当前预览的方块</summary>
        private GamePiece _m_previewPiece;
        
        /// <summary>当前预览位置</summary>
        private Vector2Int _m_previewPosition;
        
        /// <summary>是否正在预览</summary>
        private bool _m_isPreviewActive = false;
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Start方法 - 订阅事件
        /// </summary>
        private void Start()
        {
            _subscribeToEvents();
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 取消事件订阅
        /// </summary>
        private void OnDestroy()
        {
            _unsubscribeFromEvents();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 验证方块放置位置
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否为有效位置</returns>
        public bool validatePiecePosition(GamePiece _piece, Vector2Int _position, int _playerId)
        {
            if (_piece == null || !_m_enablePositionValidation)
                return true;
            
            // 获取BoardManager进行验证
            var boardManager = FindObjectOfType<BoardManager>();
            if (boardManager == null)
            {
                Debug.LogWarning("[TouchGameplayIntegration] BoardManager未找到");
                return false;
            }
            
            return boardManager.isValidPlacement(_piece, _position, _playerId);
        }
        
        /// <summary>
        /// 开始方块预览
        /// </summary>
        /// <param name="_piece">预览的方块</param>
        /// <param name="_position">预览位置</param>
        public void startPiecePreview(GamePiece _piece, Vector2Int _position)
        {
            if (!_m_enableRealTimePreview || _piece == null)
                return;
            
            _m_previewPiece = _piece;
            _m_previewPosition = _position;
            _m_isPreviewActive = true;
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[TouchGameplayIntegration] 开始方块预览: {_piece.pieceId} 位置: {_position}");
            }
        }
        
        /// <summary>
        /// 更新方块预览
        /// </summary>
        /// <param name="_position">新的预览位置</param>
        public void updatePiecePreview(Vector2Int _position)
        {
            if (!_m_isPreviewActive || _m_previewPiece == null)
                return;
            
            _m_previewPosition = _position;
            
            // 验证新位置
            bool isValid = validatePiecePosition(_m_previewPiece, _position, _m_previewPiece.playerId);
            
            // 触发预览更新事件
            GameEvents.instance.onPiecePreviewUpdated?.Invoke(_m_previewPiece, _position, isValid);
        }
        
        /// <summary>
        /// 结束方块预览
        /// </summary>
        public void endPiecePreview()
        {
            if (!_m_isPreviewActive)
                return;
            
            _m_isPreviewActive = false;
            _m_previewPiece = null;
            
            // 触发预览结束事件
            GameEvents.instance.onPiecePreviewEnded?.Invoke();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[TouchGameplayIntegration] 结束方块预览");
            }
        }
        
        /// <summary>
        /// 尝试放置方块
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <returns>是否放置成功</returns>
        public bool tryPlacePiece(GamePiece _piece, Vector2Int _position)
        {
            if (_piece == null)
                return false;
            
            // 验证位置
            if (!validatePiecePosition(_piece, _position, _piece.playerId))
            {
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[TouchGameplayIntegration] 方块放置位置无效: {_piece.pieceId} 位置: {_position}");
                }
                return false;
            }
            
            // 通过GameManager尝试放置
            var gameManager = GameManager.instance;
            if (gameManager == null)
            {
                Debug.LogError("[TouchGameplayIntegration] GameManager未找到");
                return false;
            }
            
            return gameManager.tryPlacePiece(_piece, _position, _piece.playerId);
        }
        
        #endregion
        
        #region 私有方法 - 事件处理
        
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
            }
        }
        
        /// <summary>
        /// 取消订阅游戏事件
        /// </summary>
        private void _unsubscribeFromEvents()
        {
            if (GameEvents.instance != null)
            {
                GameEvents.instance.onPieceDragStart -= _onPieceDragStart;
                GameEvents.instance.onPieceDragging -= _onPieceDragging;
                GameEvents.instance.onPieceDragEnd -= _onPieceDragEnd;
            }
        }
        
        /// <summary>
        /// 方块拖拽开始事件处理
        /// </summary>
        /// <param name="_piece">开始拖拽的方块</param>
        private void _onPieceDragStart(_IGamePiece _piece)
        {
            if (_piece is GamePiece gamePiece)
            {
                startPiecePreview(gamePiece, Vector2Int.zero);
            }
        }
        
        /// <summary>
        /// 方块拖拽中事件处理
        /// </summary>
        /// <param name="_piece">正在拖拽的方块</param>
        /// <param name="_worldPosition">世界坐标位置</param>
        private void _onPieceDragging(_IGamePiece _piece, Vector3 _worldPosition)
        {
            if (_piece is GamePiece gamePiece)
            {
                // 将世界坐标转换为棋盘坐标
                Vector2Int boardPosition = _worldToBoardPosition(_worldPosition);
                updatePiecePreview(boardPosition);
            }
        }
        
        /// <summary>
        /// 方块拖拽结束事件处理
        /// </summary>
        /// <param name="_piece">结束拖拽的方块</param>
        /// <param name="_worldPosition">最终世界坐标位置</param>
        private void _onPieceDragEnd(_IGamePiece _piece, Vector3 _worldPosition)
        {
            if (_piece is GamePiece gamePiece)
            {
                Vector2Int boardPosition = _worldToBoardPosition(_worldPosition);
                
                // 尝试放置方块
                bool success = tryPlacePiece(gamePiece, boardPosition);
                
                if (!success)
                {
                    // 放置失败，触发失败事件
                    GameEvents.instance.onPiecePlacementFailed?.Invoke(_piece, boardPosition, _piece.playerId, "位置无效或不符合规则");
                }
                
                // 结束预览
                endPiecePreview();
            }
        }
        
        /// <summary>
        /// 世界坐标转换为棋盘坐标
        /// </summary>
        /// <param name="_worldPosition">世界坐标</param>
        /// <returns>棋盘坐标</returns>
        private Vector2Int _worldToBoardPosition(Vector3 _worldPosition)
        {
            // 简单的坐标转换，假设棋盘从(0,0)开始，每个格子大小为1
            int x = Mathf.RoundToInt(_worldPosition.x);
            int z = Mathf.RoundToInt(_worldPosition.z);
            
            // 确保坐标在有效范围内
            x = Mathf.Clamp(x, 0, 19);
            z = Mathf.Clamp(z, 0, 19);
            
            return new Vector2Int(x, z);
        }
        
        #endregion
    }
}