using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Data;
using BlokusGame.Core.Pieces;
using BlokusGame.Core.Managers;
using BlokusGame.Core.Events;
using BlokusGame.Core.Rules;

namespace BlokusGame.Core.Player
{
    /// <summary>
    /// 基础玩家类 - 实现_IPlayer接口的核心玩家功能
    /// 负责管理玩家的方块库存、分数计算、游戏状态和回合控制
    /// 提供完整的玩家操作接口，支持人类玩家和AI玩家的基础功能
    /// </summary>
    public class Player : MonoBehaviour, _IPlayer
    {
        [Header("玩家基本信息")]
        /// <summary>玩家的唯一标识符，用于区分不同玩家</summary>
        [SerializeField] private int _m_playerId;
        
        /// <summary>玩家显示名称，用于UI显示和调试</summary>
        [SerializeField] private string _m_playerName;
        
        /// <summary>玩家代表颜色，用于方块和UI显示</summary>
        [SerializeField] private Color _m_playerColor = Color.white;
        
        [Header("游戏状态")]
        /// <summary>玩家当前分数，基于已放置方块计算</summary>
        [SerializeField] private int _m_currentScore = 0;
        
        /// <summary>玩家是否还在游戏中，false表示已被淘汰</summary>
        [SerializeField] private bool _m_isActive = true;
        
        [Header("组件引用")]
        /// <summary>方块管理器引用，用于获取和管理玩家方块</summary>
        [SerializeField] private PieceManager _m_pieceManager;
        
        /// <summary>规则引擎引用，用于验证玩家操作的合法性</summary>
        [SerializeField] private RuleEngine _m_ruleEngine;
        
        /// <summary>游戏状态管理器引用，用于回合控制和状态同步</summary>
        [SerializeField] private GameStateManager _m_gameStateManager;
        
        [Header("调试设置")]
        /// <summary>是否启用详细日志输出，用于调试和开发</summary>
        [SerializeField] protected bool _m_enableDetailedLogging = false;
        
        // 私有字段
        /// <summary>玩家可用的方块列表，包含所有未使用的方块</summary>
        private List<_IGamePiece> _m_availablePieces;
        
        /// <summary>玩家已使用的方块列表，包含所有已放置的方块</summary>
        private List<_IGamePiece> _m_usedPieces;
        
        /// <summary>当前选中的方块，用于操作和放置</summary>
        protected _IGamePiece _m_selectedPiece;
        
        /// <summary>回合开始时间，用于计算回合用时</summary>
        private float _m_turnStartTime = 0f;
        
        // 接口属性实现
        /// <summary>玩家的唯一标识符</summary>
        public int playerId => _m_playerId;
        
        /// <summary>玩家名称</summary>
        public string playerName => _m_playerName;
        
        /// <summary>玩家颜色</summary>
        public Color playerColor => _m_playerColor;
        
        /// <summary>玩家当前分数</summary>
        public int currentScore => _m_currentScore;
        
        /// <summary>玩家是否还在游戏中（未被淘汰）</summary>
        public bool isActive => _m_isActive;
        
        /// <summary>玩家可用的方块列表</summary>
        public List<_IGamePiece> availablePieces => _m_availablePieces ?? new List<_IGamePiece>();
        
        /// <summary>玩家已使用的方块列表</summary>
        public List<_IGamePiece> usedPieces => _m_usedPieces ?? new List<_IGamePiece>();
        
        /// <summary>是否为当前回合玩家</summary>
        public bool isCurrentPlayer => _m_gameStateManager != null && _m_gameStateManager.currentPlayerId == playerId;
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法 - 组件初始化
        /// 在对象创建时立即执行，用于初始化基础数据和获取组件引用
        /// </summary>
        protected virtual void Awake()
        {
            _initializePlayer();
        }
        
        /// <summary>
        /// Unity Start方法 - 组件启动
        /// 在第一帧开始前执行，用于订阅事件和启动系统
        /// </summary>
        protected virtual void Start()
        {
            _subscribeToEvents();
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 组件销毁
        /// 在对象销毁时执行，用于清理资源和取消事件订阅
        /// </summary>
        protected virtual void OnDestroy()
        {
            _unsubscribeFromEvents();
        }
        
        #endregion
        
        #region 接口方法实现
        
        /// <summary>
        /// 初始化玩家数据
        /// 设置玩家的基本信息并初始化方块库存
        /// </summary>
        /// <param name="_playerId">玩家ID，必须唯一</param>
        /// <param name="_playerName">玩家名称，用于显示</param>
        /// <param name="_playerColor">玩家颜色，用于方块显示</param>
        public virtual void initializePlayer(int _playerId, string _playerName, Color _playerColor)
        {
            _m_playerId = _playerId;
            _m_playerName = _playerName;
            _m_playerColor = _playerColor;
            _m_currentScore = 0;
            _m_isActive = true;
            
            // 初始化方块库存
            _initializePlayerPieces();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[Player] 初始化玩家 {_playerId}: {_playerName}，颜色: {_playerColor}");
            }
        }
        
        /// <summary>
        /// 使用指定的方块
        /// 将方块从可用列表移动到已使用列表，并更新分数
        /// </summary>
        /// <param name="_piece">要使用的方块，不能为null</param>
        /// <returns>是否使用成功</returns>
        public virtual bool usePiece(_IGamePiece _piece)
        {
            if (_piece == null)
            {
                Debug.LogError("[Player] 尝试使用空方块");
                return false;
            }
            
            if (!_m_availablePieces.Contains(_piece))
            {
                Debug.LogWarning($"[Player] 玩家 {playerId} 不拥有方块 {_piece.pieceId}");
                return false;
            }
            
            // 从可用列表移除
            _m_availablePieces.Remove(_piece);
            
            // 添加到已使用列表
            _m_usedPieces.Add(_piece);
            
            // 标记方块为已放置
            _piece.setPlacedState(true);
            
            // 更新分数
            _updateScore();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[Player] 玩家 {playerId} 使用方块 {_piece.pieceId}，当前分数: {_m_currentScore}");
            }
            
            return true;
        }
        
        /// <summary>
        /// 检查玩家是否拥有指定方块
        /// </summary>
        /// <param name="_pieceId">方块ID</param>
        /// <returns>是否拥有该方块</returns>
        public virtual bool hasPiece(int _pieceId)
        {
            return _m_availablePieces.Any(piece => piece.pieceId == _pieceId);
        }
        
        /// <summary>
        /// 获取指定ID的方块
        /// </summary>
        /// <param name="_pieceId">方块ID</param>
        /// <returns>方块实例，如果不存在返回null</returns>
        public virtual _IGamePiece getPiece(int _pieceId)
        {
            return _m_availablePieces.FirstOrDefault(piece => piece.pieceId == _pieceId);
        }
        
        /// <summary>
        /// 计算玩家当前分数
        /// 根据Blokus规则：已放置格子数 - 剩余格子数
        /// </summary>
        /// <returns>当前分数</returns>
        public virtual int calculateScore()
        {
            int placedBlocks = 0;
            int remainingBlocks = 0;
            
            // 计算已使用方块的总格子数
            foreach (var piece in _m_usedPieces)
            {
                if (piece != null && piece.currentShape != null)
                {
                    placedBlocks += piece.currentShape.Length;
                }
            }
            
            // 计算剩余方块的总格子数
            foreach (var piece in _m_availablePieces)
            {
                if (piece != null && piece.currentShape != null)
                {
                    remainingBlocks += piece.currentShape.Length;
                }
            }
            
            // Blokus计分规则：已放置格子数 - 剩余格子数
            return placedBlocks - remainingBlocks;
        }
        
        /// <summary>
        /// 检查玩家是否还能继续游戏
        /// 通过规则引擎检查是否还有有效的移动选项
        /// </summary>
        /// <param name="_gameBoard">游戏棋盘引用</param>
        /// <returns>是否还能继续游戏</returns>
        public virtual bool canContinueGame(_IGameBoard _gameBoard)
        {
            if (!_m_isActive || _gameBoard == null)
            {
                return false;
            }
            
            // 检查是否还有可用方块
            if (_m_availablePieces.Count == 0)
            {
                return false;
            }
            
            // 使用规则引擎检查是否还有有效移动
            if (_m_ruleEngine != null)
            {
                return _m_ruleEngine.canPlayerContinue(playerId);
            }
            
            // 备用检查：遍历所有可用方块，看是否有有效放置位置
            foreach (var piece in _m_availablePieces)
            {
                var validPlacements = _gameBoard.getValidPlacements(piece, playerId);
                if (validPlacements.Count > 0)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 设置玩家的活跃状态
        /// 用于标记玩家是否还在游戏中
        /// </summary>
        /// <param name="_active">是否活跃</param>
        public virtual void setActiveState(bool _active)
        {
            _m_isActive = _active;
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[Player] 设置玩家 {playerId} 活跃状态为 {_active}");
            }
            
            // 触发状态变更事件
            GameEvents.onPlayerStateChanged?.Invoke(playerId, _active ? PlayerGameState.Active : PlayerGameState.Finished);
        }
        
        /// <summary>
        /// 重置玩家状态到游戏开始时
        /// 清除所有游戏数据，重新初始化方块库存
        /// </summary>
        public virtual void resetPlayer()
        {
            _m_currentScore = 0;
            _m_isActive = true;
            _m_selectedPiece = null;
            _m_turnStartTime = 0f;
            
            // 重置方块状态
            foreach (var piece in _m_usedPieces)
            {
                piece?.resetToOriginalState();
            }
            
            // 重新初始化方块库存
            _initializePlayerPieces();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[Player] 重置玩家 {playerId} 状态");
            }
        }
        
        #endregion
        
        #region 扩展功能方法
        
        /// <summary>
        /// 选择方块进行操作
        /// 设置当前选中的方块，并触发相应事件
        /// </summary>
        /// <param name="_piece">要选择的方块</param>
        /// <returns>是否选择成功</returns>
        public virtual bool selectPiece(_IGamePiece _piece)
        {
            if (_piece == null || !_m_availablePieces.Contains(_piece))
            {
                return false;
            }
            
            _m_selectedPiece = _piece;
            
            // 触发方块选择事件
            GameEvents.instance.onPieceSelected?.Invoke(_piece, playerId);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[Player] 玩家 {playerId} 选择了方块: {_piece.pieceId}");
            }
            
            return true;
        }
        
        /// <summary>
        /// 取消选择方块
        /// 清除当前选中的方块
        /// </summary>
        public virtual void deselectPiece()
        {
            if (_m_selectedPiece != null)
            {
                _m_selectedPiece = null;
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[Player] 玩家 {playerId} 取消选择方块");
                }
            }
        }
        
        /// <summary>
        /// 获取当前选中的方块
        /// </summary>
        /// <returns>当前选中的方块，如果没有选中返回null</returns>
        public virtual _IGamePiece getSelectedPiece()
        {
            return _m_selectedPiece;
        }
        
        /// <summary>
        /// 开始玩家回合
        /// 记录回合开始时间，用于计算回合用时
        /// </summary>
        public virtual void startTurn()
        {
            _m_turnStartTime = Time.time;
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[Player] 玩家 {playerId} 开始回合");
            }
        }
        
        /// <summary>
        /// 结束玩家回合
        /// 清除选中状态，准备下一回合
        /// </summary>
        public virtual void endTurn()
        {
            _m_selectedPiece = null;
            
            if (_m_enableDetailedLogging)
            {
                float turnDuration = _m_turnStartTime > 0 ? Time.time - _m_turnStartTime : 0f;
                Debug.Log($"[Player] 玩家 {playerId} 结束回合，用时: {turnDuration:F2}秒");
            }
        }
        
        /// <summary>
        /// 获取回合进行时间
        /// </summary>
        /// <returns>回合进行时间（秒）</returns>
        public virtual float getTurnTime()
        {
            return _m_turnStartTime > 0 ? Time.time - _m_turnStartTime : 0f;
        }
        
        /// <summary>
        /// 跳过当前回合
        /// 通知游戏状态管理器跳过当前玩家的回合
        /// </summary>
        public virtual void skipTurn()
        {
            if (_m_gameStateManager != null && _m_gameStateManager.currentPlayerId == playerId)
            {
                _m_gameStateManager.skipCurrentPlayer();
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[Player] 玩家 {playerId} 跳过回合");
                }
            }
            else
            {
                Debug.LogWarning($"[Player] 玩家 {playerId} 不是当前回合玩家，无法跳过回合");
            }
        }
        
        /// <summary>
        /// 获取玩家剩余方块数量
        /// </summary>
        /// <returns>剩余方块数量</returns>
        public virtual int getRemainingPieceCount()
        {
            return _m_availablePieces?.Count ?? 0;
        }
        
        /// <summary>
        /// 获取玩家已使用方块数量
        /// </summary>
        /// <returns>已使用方块数量</returns>
        public virtual int getUsedPieceCount()
        {
            return _m_usedPieces?.Count ?? 0;
        }
        
        /// <summary>
        /// 旋转选中的方块
        /// </summary>
        /// <returns>是否旋转成功</returns>
        public virtual bool rotateSelectedPiece()
        {
            if (_m_selectedPiece == null)
            {
                Debug.LogWarning("[Player] 没有选中的方块可以旋转");
                return false;
            }
            
            _m_selectedPiece.rotate90Clockwise();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[Player] 玩家 {playerId} 旋转了方块: {_m_selectedPiece.pieceId}");
            }
            
            // 触发方块旋转事件
            GameEvents.instance.onPieceRotated?.Invoke(_m_selectedPiece, playerId);
            
            return true;
        }
        
        /// <summary>
        /// 翻转选中的方块
        /// </summary>
        /// <returns>是否翻转成功</returns>
        public virtual bool flipSelectedPiece()
        {
            if (_m_selectedPiece == null)
            {
                Debug.LogWarning("[Player] 没有选中的方块可以翻转");
                return false;
            }
            
            _m_selectedPiece.flipHorizontal();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[Player] 玩家 {playerId} 翻转了方块: {_m_selectedPiece.pieceId}");
            }
            
            // 触发方块翻转事件
            GameEvents.instance.onPieceFlipped?.Invoke(_m_selectedPiece, playerId);
            
            return true;
        }
        
        /// <summary>
        /// 尝试放置方块
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <returns>是否放置成功</returns>
        public virtual bool tryPlacePiece(_IGamePiece _piece, Vector2Int _position)
        {
            if (_piece == null)
            {
                Debug.LogError("[Player] 尝试放置空方块");
                return false;
            }
            
            if (!_m_availablePieces.Contains(_piece))
            {
                Debug.LogError($"[Player] 方块不属于玩家 {playerId}");
                return false;
            }
            
            // 验证放置是否合法
            GamePiece gamePiece = _piece as GamePiece;
            if (gamePiece == null)
            {
                Debug.LogError("[Player] 方块类型转换失败，无法验证放置规则");
                return false;
            }
            
            if (_m_ruleEngine == null || !_m_ruleEngine.isValidPlacement(gamePiece, _position, playerId))
            {
                if (_m_enableDetailedLogging)
                {
                    Debug.LogWarning($"[Player] 玩家 {playerId} 在位置 {_position} 放置方块失败：不符合游戏规则");
                }
                
                // 触发放置失败事件
                GameEvents.instance.onPiecePlacementFailed?.Invoke(_piece, _position, playerId, "不符合游戏规则");
                return false;
            }
            
            // 执行放置
            return usePiece(_piece);
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 初始化玩家组件
        /// 获取必要的组件引用并初始化数据结构
        /// </summary>
        private void _initializePlayer()
        {
            // 初始化方块列表
            _m_availablePieces = new List<_IGamePiece>();
            _m_usedPieces = new List<_IGamePiece>();
            
            // 自动获取组件引用
            if (_m_pieceManager == null)
                _m_pieceManager = FindObjectOfType<PieceManager>();
            if (_m_ruleEngine == null)
                _m_ruleEngine = FindObjectOfType<RuleEngine>();
            if (_m_gameStateManager == null)
                _m_gameStateManager = FindObjectOfType<GameStateManager>();
        }
        
        /// <summary>
        /// 初始化玩家方块库存
        /// 从方块管理器获取玩家的所有方块
        /// </summary>
        private void _initializePlayerPieces()
        {
            _m_availablePieces.Clear();
            _m_usedPieces.Clear();
            
            if (_m_pieceManager != null)
            {
                // 从方块管理器获取玩家方块
                var playerPieces = _m_pieceManager.getPlayerPieces(playerId);
                
                // 将GamePiece转换为_IGamePiece
                foreach (var piece in playerPieces)
                {
                    if (piece != null)
                    {
                        _m_availablePieces.Add(piece);
                    }
                }
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[Player] 为玩家 {playerId} 初始化了 {_m_availablePieces.Count} 个方块");
                }
            }
            else
            {
                Debug.LogWarning("[Player] PieceManager未找到，无法初始化方块库存");
            }
        }
        
        /// <summary>
        /// 更新玩家分数
        /// 重新计算分数并触发分数更新事件
        /// </summary>
        private void _updateScore()
        {
            int newScore = calculateScore();
            if (newScore != _m_currentScore)
            {
                _m_currentScore = newScore;
                
                // 触发分数更新事件
                GameEvents.instance.onPlayerScoreUpdated?.Invoke(playerId, _m_currentScore);
            }
        }
        
        /// <summary>
        /// 订阅游戏事件
        /// 监听回合开始和结束事件
        /// </summary>
        private void _subscribeToEvents()
        {
            GameEvents.onTurnStarted += _onTurnStarted;
            GameEvents.onTurnEnded += _onTurnEnded;
        }
        
        /// <summary>
        /// 取消事件订阅
        /// 清理事件监听，防止内存泄漏
        /// </summary>
        private void _unsubscribeFromEvents()
        {
            GameEvents.onTurnStarted -= _onTurnStarted;
            GameEvents.onTurnEnded -= _onTurnEnded;
        }
        
        /// <summary>
        /// 处理回合开始事件
        /// 当轮到该玩家时自动开始回合
        /// </summary>
        /// <param name="_playerId">开始回合的玩家ID</param>
        /// <param name="_turnNumber">回合数</param>
        private void _onTurnStarted(int _playerId, int _turnNumber)
        {
            if (_playerId == playerId)
            {
                startTurn();
            }
        }
        
        /// <summary>
        /// 处理回合结束事件
        /// 当该玩家回合结束时自动执行清理
        /// </summary>
        /// <param name="_playerId">结束回合的玩家ID</param>
        /// <param name="_turnNumber">回合数</param>
        private void _onTurnEnded(int _playerId, int _turnNumber)
        {
            if (_playerId == playerId)
            {
                endTurn();
            }
        }
        
        #endregion
    }
}       