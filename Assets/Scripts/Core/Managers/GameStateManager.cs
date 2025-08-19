using UnityEngine;
using System.Collections.Generic;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Data;
using BlokusGame.Core.Events;
using BlokusGame.Core.Rules;

namespace BlokusGame.Core.Managers
{
    /// <summary>
    /// 游戏状态管理器
    /// 负责管理游戏的整体状态转换、回合控制和游戏流程
    /// 协调各个系统之间的状态同步
    /// </summary>
    public class GameStateManager : MonoBehaviour, _IGameStateManager
    {
        [Header("游戏配置")]
        /// <summary>最大玩家数量</summary>
        [SerializeField] private int _m_maxPlayers = 4;
        
        /// <summary>最小玩家数量</summary>
        [SerializeField] private int _m_minPlayers = 2;
        
        /// <summary>每回合最大时间限制（秒）</summary>
        [SerializeField] private float _m_turnTimeLimit = 60f;
        
        /// <summary>是否启用回合时间限制</summary>
        [SerializeField] private bool _m_enableTurnTimer = true;
        
        [Header("组件引用")]
        /// <summary>规则引擎引用</summary>
        [SerializeField] private RuleEngine _m_ruleEngine;
        
        /// <summary>方块管理器引用</summary>
        [SerializeField] private PieceManager _m_pieceManager;
        
        /// <summary>玩家管理器引用</summary>
        [SerializeField] private PlayerManager _m_playerManager;
        
        /// <summary>当前游戏状态</summary>
        public GameState currentState { get; private set; } = GameState.MainMenu;
        
        /// <summary>当前回合的玩家ID</summary>
        public int currentPlayerId { get; private set; } = 0;
        
        /// <summary>游戏回合数</summary>
        public int turnNumber { get; private set; } = 0;
        
        /// <summary>参与游戏的玩家数量</summary>
        private int _m_playerCount = 0;
        
        /// <summary>每个玩家的游戏状态</summary>
        private Dictionary<int, PlayerGameState> _m_playerStates = new Dictionary<int, PlayerGameState>();
        
        /// <summary>当前回合开始时间</summary>
        private float _m_turnStartTime = 0f;
        
        /// <summary>游戏开始时间</summary>
        private float _m_gameStartTime = 0f;
        
        /// <summary>
        /// 组件初始化
        /// 获取必要的组件引用并设置初始状态
        /// </summary>
        private void Awake()
        {
            // 自动获取组件引用
            if (_m_ruleEngine == null)
                _m_ruleEngine = FindObjectOfType<RuleEngine>();
            if (_m_pieceManager == null)
                _m_pieceManager = FindObjectOfType<PieceManager>();
            if (_m_playerManager == null)
                _m_playerManager = FindObjectOfType<PlayerManager>();
                
            // 初始化状态
            currentState = GameState.MainMenu;
        }
        
        /// <summary>
        /// 每帧更新
        /// 处理回合计时和状态检查
        /// </summary>
        private void Update()
        {
            // 处理回合计时
            if (currentState == GameState.GamePlaying && _m_enableTurnTimer)
            {
                _handleTurnTimer();
            }
        }
        
        /// <summary>
        /// 开始新游戏
        /// </summary>
        /// <param name="_playerCount">参与游戏的玩家数量</param>
        public void startNewGame(int _playerCount)
        {
            if (_playerCount < _m_minPlayers || _playerCount > _m_maxPlayers)
            {
                Debug.LogError($"[GameStateManager] 无效的玩家数量: {_playerCount}，有效范围: {_m_minPlayers}-{_m_maxPlayers}");
                return;
            }
            
            Debug.Log($"[GameStateManager] 开始新游戏，玩家数量: {_playerCount}");
            
            // 设置游戏参数
            _m_playerCount = _playerCount;
            currentPlayerId = 0;
            turnNumber = 1;
            _m_gameStartTime = Time.time;
            
            // 初始化玩家状态
            _initializePlayerStates();
            
            // 切换到初始化状态
            _changeState(GameState.GameInitializing);
            
            // 初始化各个系统
            _initializeGameSystems();
            
            // 开始游戏
            _changeState(GameState.GamePlaying);
            _startTurn();
            
            // 触发游戏开始事件
            GameEvents.onGameStarted?.Invoke(_playerCount);
        }
        
        /// <summary>
        /// 切换到下一个玩家的回合
        /// </summary>
        public void nextTurn()
        {
            if (currentState != GameState.GamePlaying)
            {
                Debug.LogWarning("[GameStateManager] 只能在游戏进行中切换回合");
                return;
            }
            
            if (!canAdvanceTurn())
            {
                Debug.LogWarning("[GameStateManager] 当前无法切换回合");
                return;
            }
            
            // 结束当前回合
            _endTurn();
            
            // 寻找下一个活跃玩家
            int nextPlayerId = _findNextActivePlayer();
            
            if (nextPlayerId == -1)
            {
                // 没有活跃玩家，游戏结束
                endGame();
                return;
            }
            
            // 切换到下一个玩家
            currentPlayerId = nextPlayerId;
            
            // 如果回到第一个玩家，增加回合数
            if (currentPlayerId == 0)
            {
                turnNumber++;
            }
            
            // 检查游戏结束条件
            if (_m_ruleEngine != null && _m_ruleEngine.isGameOver())
            {
                endGame();
                return;
            }
            
            // 开始新回合
            _startTurn();
            
            Debug.Log($"[GameStateManager] 切换到玩家 {currentPlayerId} 的回合，第 {turnNumber} 轮");
        }
        
        /// <summary>
        /// 跳过当前玩家的回合
        /// </summary>
        public void skipCurrentPlayer()
        {
            if (currentState != GameState.GamePlaying)
            {
                Debug.LogWarning("[GameStateManager] 只能在游戏进行中跳过回合");
                return;
            }
            
            Debug.Log($"[GameStateManager] 玩家 {currentPlayerId} 跳过回合");
            
            // 设置玩家状态为跳过
            setPlayerState(currentPlayerId, PlayerGameState.Skipped);
            
            // 触发玩家跳过事件
            GameEvents.onPlayerSkipped?.Invoke(currentPlayerId);
            
            // 切换到下一回合
            nextTurn();
        }
        
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void pauseGame()
        {
            if (currentState != GameState.GamePlaying)
            {
                Debug.LogWarning("[GameStateManager] 只能在游戏进行中暂停");
                return;
            }
            
            Debug.Log("[GameStateManager] 游戏暂停");
            _changeState(GameState.GamePaused);
            
            // 触发游戏暂停事件
            GameEvents.onGamePaused?.Invoke();
        }
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void resumeGame()
        {
            if (currentState != GameState.GamePaused)
            {
                Debug.LogWarning("[GameStateManager] 只能从暂停状态恢复游戏");
                return;
            }
            
            Debug.Log("[GameStateManager] 游戏恢复");
            _changeState(GameState.GamePlaying);
            
            // 重置回合计时
            _m_turnStartTime = Time.time;
            
            // 触发游戏恢复事件
            GameEvents.onGameResumed?.Invoke();
        }
        
        /// <summary>
        /// 结束游戏
        /// </summary>
        public void endGame()
        {
            if (currentState == GameState.GameEnded)
            {
                Debug.LogWarning("[GameStateManager] 游戏已经结束");
                return;
            }
            
            Debug.Log("[GameStateManager] 游戏结束");
            _changeState(GameState.GameEnded);
            
            // 计算最终得分
            var finalScores = _calculateFinalScores();
            
            // 触发游戏结束事件
            GameEvents.onGameEnded?.Invoke(finalScores);
            
            // 切换到结果显示状态
            _changeState(GameState.ShowingResults);
        }
        
        /// <summary>
        /// 重置游戏
        /// </summary>
        public void resetGame()
        {
            Debug.Log("[GameStateManager] 重置游戏");
            
            // 重置状态
            currentState = GameState.MainMenu;
            currentPlayerId = 0;
            turnNumber = 0;
            _m_playerCount = 0;
            _m_playerStates.Clear();
            
            // 重置各个系统
            if (_m_pieceManager != null)
            {
                for (int i = 0; i < _m_maxPlayers; i++)
                {
                    _m_pieceManager.resetPlayerPieces(i);
                }
            }
            
            // 触发游戏重置事件
            GameEvents.onGameReset?.Invoke();
        }
        
        /// <summary>
        /// 检查是否可以切换到下一回合
        /// </summary>
        /// <returns>是否可以切换回合</returns>
        public bool canAdvanceTurn()
        {
            return currentState == GameState.GamePlaying;
        }
        
        /// <summary>
        /// 获取游戏进度百分比
        /// </summary>
        /// <returns>游戏进度 (0.0 - 1.0)</returns>
        public float getGameProgress()
        {
            if (_m_pieceManager == null || _m_playerCount == 0)
                return 0f;
            
            int totalPieces = 0;
            int placedPieces = 0;
            
            for (int i = 0; i < _m_playerCount; i++)
            {
                var playerPieces = _m_pieceManager.getPlayerPieces(i);
                totalPieces += playerPieces.Count;
                placedPieces += _m_pieceManager.getPlacedPieceCount(i);
            }
            
            return totalPieces > 0 ? (float)placedPieces / totalPieces : 0f;
        }
        
        /// <summary>
        /// 获取玩家的游戏状态
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>玩家游戏状态</returns>
        public PlayerGameState getPlayerState(int _playerId)
        {
            if (_m_playerStates.ContainsKey(_playerId))
            {
                return _m_playerStates[_playerId];
            }
            
            return PlayerGameState.Active; // 默认状态
        }
        
        /// <summary>
        /// 设置玩家的游戏状态
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_state">新的游戏状态</param>
        public void setPlayerState(int _playerId, PlayerGameState _state)
        {
            _m_playerStates[_playerId] = _state;
            
            Debug.Log($"[GameStateManager] 玩家 {_playerId} 状态变更为: {_state}");
            
            // 触发玩家状态变更事件
            GameEvents.onPlayerStateChanged?.Invoke(_playerId, _state);
        }
        
        /// <summary>
        /// 初始化玩家状态
        /// </summary>
        private void _initializePlayerStates()
        {
            _m_playerStates.Clear();
            
            for (int i = 0; i < _m_playerCount; i++)
            {
                _m_playerStates[i] = PlayerGameState.Active;
            }
        }
        
        /// <summary>
        /// 初始化游戏系统
        /// </summary>
        private void _initializeGameSystems()
        {
            // 初始化方块系统
            if (_m_pieceManager != null)
            {
                for (int i = 0; i < _m_playerCount; i++)
                {
                    _m_pieceManager.createPlayerPieces(i);
                }
            }
        }
        
        /// <summary>
        /// 改变游戏状态
        /// </summary>
        /// <param name="_newState">新状态</param>
        private void _changeState(GameState _newState)
        {
            GameState oldState = currentState;
            currentState = _newState;
            
            Debug.Log($"[GameStateManager] 状态变更: {oldState} -> {_newState}");
            
            // 触发状态变更事件
            GameEvents.instance.onGameStateChanged?.Invoke(oldState, _newState);
        }
        
        /// <summary>
        /// 开始回合
        /// </summary>
        private void _startTurn()
        {
            _m_turnStartTime = Time.time;
            
            // 触发回合开始事件
            GameEvents.onTurnStarted?.Invoke(currentPlayerId, turnNumber);
        }
        
        /// <summary>
        /// 结束回合
        /// </summary>
        private void _endTurn()
        {
            // 触发回合结束事件
            GameEvents.onTurnEnded?.Invoke(currentPlayerId, turnNumber);
        }
        
        /// <summary>
        /// 寻找下一个活跃玩家
        /// </summary>
        /// <returns>下一个活跃玩家ID，如果没有则返回-1</returns>
        private int _findNextActivePlayer()
        {
            int startPlayerId = currentPlayerId;
            int nextPlayerId = (currentPlayerId + 1) % _m_playerCount;
            
            // 循环查找活跃玩家
            while (nextPlayerId != startPlayerId)
            {
                PlayerGameState state = getPlayerState(nextPlayerId);
                if (state == PlayerGameState.Active)
                {
                    return nextPlayerId;
                }
                
                nextPlayerId = (nextPlayerId + 1) % _m_playerCount;
            }
            
            // 检查当前玩家是否还活跃
            if (getPlayerState(currentPlayerId) == PlayerGameState.Active)
            {
                return currentPlayerId;
            }
            
            return -1; // 没有活跃玩家
        }
        
        /// <summary>
        /// 处理回合计时
        /// </summary>
        private void _handleTurnTimer()
        {
            float elapsedTime = Time.time - _m_turnStartTime;
            
            if (elapsedTime >= _m_turnTimeLimit)
            {
                Debug.Log($"[GameStateManager] 玩家 {currentPlayerId} 回合超时，自动跳过");
                skipCurrentPlayer();
            }
        }
        
        /// <summary>
        /// 计算最终得分
        /// </summary>
        /// <returns>玩家得分字典</returns>
        private Dictionary<int, int> _calculateFinalScores()
        {
            var scores = new Dictionary<int, int>();
            
            if (_m_pieceManager == null)
                return scores;
            
            for (int i = 0; i < _m_playerCount; i++)
            {
                // 计算剩余方块的总格子数作为负分
                int remainingBlocks = 0;
                var remainingPieces = _m_pieceManager.getPlayerPieces(i);
                
                foreach (var piece in remainingPieces)
                {
                    if (!piece.isPlaced)
                    {
                        remainingBlocks += piece.currentShape.Length;
                    }
                }
                
                scores[i] = -remainingBlocks; // 剩余格子越少得分越高
            }
            
            return scores;
        }
    }
}