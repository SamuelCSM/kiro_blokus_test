using UnityEngine;
using System.Collections.Generic;
using BlokusGame.Core.Data;
using BlokusGame.Core.Pieces;
using BlokusGame.Core.Events;
using BlokusGame.Core.Managers;

namespace BlokusGame.Core.Player
{
    /// <summary>
    /// 人类玩家类
    /// 继承自Player类，专门处理人类玩家的交互逻辑
    /// 支持触摸输入、操作撤销和用户反馈
    /// </summary>
    public class HumanPlayer : Player
    {
        [Header("人类玩家配置")]
        /// <summary>是否启用操作撤销功能</summary>
        [SerializeField] private bool _m_enableUndo = true;
        
        /// <summary>最大撤销步数</summary>
        [SerializeField] private int _m_maxUndoSteps = 5;
        
        /// <summary>操作确认延迟时间（秒）</summary>
        [SerializeField] private float _m_confirmationDelay = 1.0f;
        
        /// <summary>是否需要操作确认</summary>
        [SerializeField] private bool _m_requireConfirmation = false;
        
        [Header("输入配置")]
        /// <summary>双击检测时间间隔</summary>
        [SerializeField] private float _m_doubleClickInterval = 0.3f;
        
        /// <summary>长按检测时间</summary>
        [SerializeField] private float _m_longPressTime = 0.8f;
        
        /// <summary>拖拽最小距离</summary>
        [SerializeField] private float _m_dragThreshold = 10f;
        
        /// <summary>操作历史记录</summary>
        private List<PlayerAction> _m_actionHistory = new List<PlayerAction>();
        
        /// <summary>待确认的操作</summary>
        private PlayerAction _m_pendingAction = null;
        
        /// <summary>确认计时器</summary>
        private float _m_confirmationTimer = 0f;
        
        /// <summary>上次点击时间</summary>
        private float _m_lastClickTime = 0f;
        
        /// <summary>点击计数</summary>
        private int _m_clickCount = 0;
        
        /// <summary>当前拖拽的方块</summary>
        private GamePiece _m_draggingPiece = null;
        
        /// <summary>拖拽开始位置</summary>
        private Vector3 _m_dragStartPosition = Vector3.zero;
        
        /// <summary>是否正在拖拽</summary>
        private bool _m_isDragging = false;
        
        /// <summary>
        /// 组件更新
        /// 处理输入检测和确认计时
        /// </summary>
        protected virtual void Update()
        {
            // 只有当前回合玩家才处理输入
            if (!isCurrentPlayer)
                return;
                
            _handleInput();
            _updateConfirmationTimer();
        }
        
        /// <summary>
        /// 初始化人类玩家
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_playerName">玩家名称</param>
        /// <param name="_playerColor">玩家颜色</param>
        public override void initializePlayer(int _playerId, string _playerName, Color _playerColor)
        {
            base.initializePlayer(_playerId, _playerName, _playerColor);
            
            // 初始化操作历史
            _m_actionHistory.Clear();
            
            Debug.Log($"[HumanPlayer] 人类玩家 {_playerId} 初始化完成");
        }
        
        /// <summary>
        /// 使用PlayerData初始化人类玩家
        /// </summary>
        /// <param name="_playerData">玩家数据</param>
        public virtual void initialize(PlayerData _playerData)
        {
            if (_playerData == null)
            {
                Debug.LogError("[HumanPlayer] PlayerData不能为空");
                return;
            }
            
            initializePlayer(_playerData.playerId, _playerData.playerName, _playerData.playerColor);
            
            // 确保玩家类型为人类
            if (_playerData.playerType != PlayerType.Human)
            {
                Debug.LogWarning($"[HumanPlayer] 玩家 {playerId} 类型不是Human，但使用了HumanPlayer类");
            }
        }
        
        /// <summary>
        /// 处理方块点击
        /// </summary>
        /// <param name="_piece">被点击的方块</param>
        public virtual void onPieceClicked(GamePiece _piece)
        {
            if (!isCurrentPlayer)
            {
                Debug.LogWarning("[HumanPlayer] 不是当前回合，无法操作方块");
                return;
            }
            
            if (_piece == null || !availablePieces.Contains(_piece))
            {
                Debug.LogWarning("[HumanPlayer] 点击的方块无效或不属于当前玩家");
                return;
            }
            
            if (_piece.isPlaced)
            {
                Debug.LogWarning("[HumanPlayer] 不能操作已放置的方块");
                return;
            }
            
            // 检测双击
            float currentTime = Time.time;
            if (currentTime - _m_lastClickTime < _m_doubleClickInterval && _m_selectedPiece == _piece)
            {
                _m_clickCount++;
                if (_m_clickCount >= 2)
                {
                    _handleDoubleClick(_piece);
                    _m_clickCount = 0;
                }
            }
            else
            {
                _m_clickCount = 1;
                _handleSingleClick(_piece);
            }
            
            _m_lastClickTime = currentTime;
        }
        
        /// <summary>
        /// 处理方块拖拽开始
        /// </summary>
        /// <param name="_piece">开始拖拽的方块</param>
        /// <param name="_startPosition">拖拽开始位置</param>
        public virtual void onPieceDragStart(GamePiece _piece, Vector3 _startPosition)
        {
            if (!isCurrentPlayer || _piece == null || !availablePieces.Contains(_piece))
                return;
                
            if (_piece.isPlaced)
            {
                Debug.LogWarning("[HumanPlayer] 不能拖拽已放置的方块");
                return;
            }
            
            _m_draggingPiece = _piece;
            _m_dragStartPosition = _startPosition;
            _m_isDragging = true;
            
            // 自动选择被拖拽的方块
            selectPiece(_piece);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[HumanPlayer] 开始拖拽方块: {_piece.pieceId}");
            }
            
            // 触发拖拽开始事件
            GameEvents.instance.onPieceDragStart?.Invoke(_piece);
        }
        
        /// <summary>
        /// 处理方块拖拽中
        /// </summary>
        /// <param name="_currentPosition">当前拖拽位置</param>
        public virtual void onPieceDragging(Vector3 _currentPosition)
        {
            if (!_m_isDragging || _m_draggingPiece == null)
                return;
            
            // 触发拖拽中事件
            GameEvents.instance.onPieceDragging?.Invoke(_m_draggingPiece, _currentPosition);
        }
        
        /// <summary>
        /// 处理方块拖拽结束
        /// </summary>
        /// <param name="_endPosition">拖拽结束位置</param>
        /// <param name="_boardPosition">对应的棋盘位置</param>
        public virtual void onPieceDragEnd(Vector3 _endPosition, Vector2Int _boardPosition)
        {
            if (!_m_isDragging || _m_draggingPiece == null)
                return;
            
            // 检查拖拽距离
            float dragDistance = Vector3.Distance(_m_dragStartPosition, _endPosition);
            
            if (dragDistance > _m_dragThreshold)
            {
                // 尝试放置方块
                _attemptPiecePlacement(_m_draggingPiece, _boardPosition);
            }
            
            // 重置拖拽状态
            _m_isDragging = false;
            _m_draggingPiece = null;
            _m_dragStartPosition = Vector3.zero;
            
            // 触发拖拽结束事件
            GameEvents.instance.onPieceDragEnd?.Invoke(_m_draggingPiece, _endPosition);
        }
        
        /// <summary>
        /// 撤销上一个操作
        /// </summary>
        /// <returns>是否撤销成功</returns>
        public virtual bool undoLastAction()
        {
            if (!_m_enableUndo || _m_actionHistory.Count == 0)
            {
                Debug.LogWarning("[HumanPlayer] 没有可撤销的操作");
                return false;
            }
            
            if (!isCurrentPlayer)
            {
                Debug.LogWarning("[HumanPlayer] 不是当前回合，无法撤销操作");
                return false;
            }
            
            // 获取最后一个操作
            PlayerAction lastAction = _m_actionHistory[_m_actionHistory.Count - 1];
            _m_actionHistory.RemoveAt(_m_actionHistory.Count - 1);
            
            // 执行撤销
            bool success = _executeUndo(lastAction);
            
            if (success && _m_enableDetailedLogging)
            {
                Debug.Log($"[HumanPlayer] 撤销操作成功: {lastAction.actionType}");
            }
            
            return success;
        }
        
        /// <summary>
        /// 确认待确认的操作
        /// </summary>
        public virtual void confirmPendingAction()
        {
            if (_m_pendingAction == null)
            {
                Debug.LogWarning("[HumanPlayer] 没有待确认的操作");
                return;
            }
            
            // 执行操作
            _executePendingAction();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[HumanPlayer] 确认操作: {_m_pendingAction.actionType}");
            }
        }
        
        /// <summary>
        /// 取消待确认的操作
        /// </summary>
        public virtual void cancelPendingAction()
        {
            if (_m_pendingAction == null)
                return;
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[HumanPlayer] 取消操作: {_m_pendingAction.actionType}");
            }
            
            _m_pendingAction = null;
            _m_confirmationTimer = 0f;
        }
        
        /// <summary>
        /// 重置人类玩家状态
        /// </summary>
        public override void resetPlayer()
        {
            base.resetPlayer();
            
            // 重置操作历史
            _m_actionHistory.Clear();
            _m_pendingAction = null;
            _m_confirmationTimer = 0f;
            
            // 重置输入状态
            _m_isDragging = false;
            _m_draggingPiece = null;
            _m_dragStartPosition = Vector3.zero;
            _m_lastClickTime = 0f;
            _m_clickCount = 0;
        }
        
        /// <summary>
        /// 处理输入检测
        /// </summary>
        private void _handleInput()
        {
            // 检测键盘输入
            if (Input.GetKeyDown(KeyCode.R))
            {
                rotateSelectedPiece();
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                flipSelectedPiece();
            }
            else if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl))
            {
                undoLastAction();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                skipTurn();
            }
        }
        
        /// <summary>
        /// 更新确认计时器
        /// </summary>
        private void _updateConfirmationTimer()
        {
            if (_m_pendingAction == null)
                return;
            
            _m_confirmationTimer += Time.deltaTime;
            
            if (_m_confirmationTimer >= _m_confirmationDelay)
            {
                if (_m_requireConfirmation)
                {
                    // 需要手动确认，显示确认UI
                    _showConfirmationUI();
                }
                else
                {
                    // 自动确认
                    confirmPendingAction();
                }
            }
        }
        
        /// <summary>
        /// 处理单击
        /// </summary>
        /// <param name="_piece">被点击的方块</param>
        private void _handleSingleClick(GamePiece _piece)
        {
            selectPiece(_piece);
        }
        
        /// <summary>
        /// 处理双击
        /// </summary>
        /// <param name="_piece">被双击的方块</param>
        private void _handleDoubleClick(GamePiece _piece)
        {
            // 双击旋转方块
            if (_piece == _m_selectedPiece)
            {
                rotateSelectedPiece();
            }
        }
        
        /// <summary>
        /// 尝试放置方块
        /// </summary>
        /// <param name="_piece">方块</param>
        /// <param name="_position">位置</param>
        private void _attemptPiecePlacement(GamePiece _piece, Vector2Int _position)
        {
            if (_m_requireConfirmation)
            {
                // 创建待确认操作
                _m_pendingAction = new PlayerAction
                {
                    actionType = PlayerActionType.PlacePiece,
                    piece = _piece,
                    position = _position,
                    timestamp = Time.time
                };
                _m_confirmationTimer = 0f;
            }
            else
            {
                // 直接执行放置
                bool success = tryPlacePiece(_piece, _position);
                if (success)
                {
                    _recordAction(PlayerActionType.PlacePiece, _piece, _position);
                }
            }
        }
        
        /// <summary>
        /// 执行待确认操作
        /// </summary>
        private void _executePendingAction()
        {
            if (_m_pendingAction == null)
                return;
            
            bool success = false;
            
            switch (_m_pendingAction.actionType)
            {
                case PlayerActionType.PlacePiece:
                    success = tryPlacePiece(_m_pendingAction.piece, _m_pendingAction.position);
                    break;
                case PlayerActionType.RotatePiece:
                    success = rotateSelectedPiece();
                    break;
                case PlayerActionType.FlipPiece:
                    success = flipSelectedPiece();
                    break;
            }
            
            if (success)
            {
                _recordAction(_m_pendingAction.actionType, _m_pendingAction.piece, _m_pendingAction.position);
            }
            
            _m_pendingAction = null;
            _m_confirmationTimer = 0f;
        }
        
        /// <summary>
        /// 记录操作到历史
        /// </summary>
        /// <param name="_actionType">操作类型</param>
        /// <param name="_piece">相关方块</param>
        /// <param name="_position">相关位置</param>
        private void _recordAction(PlayerActionType _actionType, GamePiece _piece, Vector2Int _position)
        {
            if (!_m_enableUndo)
                return;
            
            var action = new PlayerAction
            {
                actionType = _actionType,
                piece = _piece,
                position = _position,
                timestamp = Time.time
            };
            
            _m_actionHistory.Add(action);
            
            // 限制历史记录数量
            while (_m_actionHistory.Count > _m_maxUndoSteps)
            {
                _m_actionHistory.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// 执行撤销操作
        /// </summary>
        /// <param name="_action">要撤销的操作</param>
        /// <returns>是否成功</returns>
        private bool _executeUndo(PlayerAction _action)
        {
            switch (_action.actionType)
            {
                case PlayerActionType.PlacePiece:
                    // 撤销方块放置
                    if (_action.piece != null && _action.piece.isPlaced)
                    {
                        _action.piece.resetToOriginalState();
                        return true;
                    }
                    break;
                    
                case PlayerActionType.RotatePiece:
                    // 撤销旋转（反向旋转3次）
                    if (_action.piece != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            _action.piece.rotate90Clockwise();
                        }
                        return true;
                    }
                    break;
                    
                case PlayerActionType.FlipPiece:
                    // 撤销翻转（再次翻转）
                    if (_action.piece != null)
                    {
                        _action.piece.flipHorizontal();
                        return true;
                    }
                    break;
            }
            
            return false;
        }
        
        /// <summary>
        /// 显示确认UI
        /// </summary>
        private void _showConfirmationUI()
        {
            // 这里应该显示确认UI，暂时用日志代替
            Debug.Log($"[HumanPlayer] 请确认操作: {_m_pendingAction?.actionType}");
            
            // 触发显示确认消息事件
            GameEvents.instance.onShowMessage?.Invoke(
                $"确认{_getActionDisplayName(_m_pendingAction?.actionType)}？", 
                MessageType.Info);
        }
        
        /// <summary>
        /// 获取操作显示名称
        /// </summary>
        /// <param name="_actionType">操作类型</param>
        /// <returns>显示名称</returns>
        private string _getActionDisplayName(PlayerActionType? _actionType)
        {
            switch (_actionType)
            {
                case PlayerActionType.PlacePiece: return "放置方块";
                case PlayerActionType.RotatePiece: return "旋转方块";
                case PlayerActionType.FlipPiece: return "翻转方块";
                case PlayerActionType.SelectPiece: return "选择方块";
                default: return "操作";
            }
        }
    }
    
    /// <summary>
    /// 玩家操作记录
    /// </summary>
    [System.Serializable]
    public class PlayerAction
    {
        /// <summary>操作类型</summary>
        public PlayerActionType actionType;
        
        /// <summary>相关方块</summary>
        public GamePiece piece;
        
        /// <summary>相关位置</summary>
        public Vector2Int position;
        
        /// <summary>操作时间戳</summary>
        public float timestamp;
    }
    
    /// <summary>
    /// 玩家操作类型枚举
    /// </summary>
    public enum PlayerActionType
    {
        /// <summary>选择方块</summary>
        SelectPiece,
        /// <summary>放置方块</summary>
        PlacePiece,
        /// <summary>旋转方块</summary>
        RotatePiece,
        /// <summary>翻转方块</summary>
        FlipPiece,
        /// <summary>跳过回合</summary>
        SkipTurn
    }
}