using UnityEngine;
using System.Collections.Generic;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Events;
using BlokusGame.Core.Data;

namespace BlokusGame.Core.Managers
{
    /// <summary>
    /// 游戏管理器 - Blokus游戏的核心控制器
    /// 负责游戏状态管理、回合控制、游戏流程协调
    /// 这是整个游戏系统的中央控制点
    /// </summary>
    public class GameManager : MonoBehaviour, _IGameStateManager
    {
        [Header("游戏配置")]
        /// <summary>最大玩家数量</summary>
        [SerializeField] private int _m_maxPlayers = 4;
        
        /// <summary>最小玩家数量</summary>
        [SerializeField] private int _m_minPlayers = 2;
        
        /// <summary>游戏是否自动保存</summary>
        [SerializeField] private bool _m_autoSave = true;
        
        /// <summary>自动保存间隔（秒）</summary>
        [SerializeField] private float _m_autoSaveInterval = 30f;
        
        [Header("组件引用")]
        /// <summary>游戏状态管理器引用</summary>
        [SerializeField] private GameStateManager _m_gameStateManager;
        
        /// <summary>棋盘管理器引用</summary>
        [SerializeField] private BoardManager _m_boardManager;
        
        /// <summary>玩家管理器引用</summary>
        [SerializeField] private PlayerManager _m_playerManager;
        
        /// <summary>输入管理器引用</summary>
        [SerializeField] private InputManager _m_inputManager;
        
        /// <summary>UI管理器引用</summary>
        [SerializeField] private UIManager _m_uiManager;
        
        // 私有字段
        /// <summary>当前游戏状态</summary>
        private GameState _m_currentGameState = GameState.MainMenu;
        
        /// <summary>当前游戏模式</summary>
        private GameMode _m_currentGameMode;
        
        /// <summary>当前回合玩家ID</summary>
        private int _m_currentPlayerId = 1;
        
        /// <summary>当前游戏回合数</summary>
        private int _m_turnNumber = 1;
        
        /// <summary>游戏开始时间</summary>
        private float _m_gameStartTime;
        
        /// <summary>上次自动保存时间</summary>
        private float _m_lastAutoSaveTime;
        
        /// <summary>游戏是否暂停</summary>
        private bool _m_isPaused = false;
        
        /// <summary>单例实例</summary>
        public static GameManager instance { get; private set; }
        
        // 公共属性
        /// <summary>当前游戏状态</summary>
        public GameState currentGameState => _m_currentGameState;
        
        /// <summary>当前游戏状态（接口实现）</summary>
        public GameState currentState => _m_currentGameState;
        
        /// <summary>当前游戏模式</summary>
        public GameMode currentGameMode => _m_currentGameMode;
        
        /// <summary>当前回合玩家ID</summary>
        public int currentPlayerId => _m_currentPlayerId;
        
        /// <summary>当前游戏回合数</summary>
        public int turnNumber => _m_turnNumber;
        
        /// <summary>游戏是否正在进行中</summary>
        public bool isGameActive => _m_currentGameState == GameState.GamePlaying;
        
        /// <summary>游戏是否暂停</summary>
        public bool isPaused => _m_isPaused;
        
        /// <summary>游戏进行时间（秒）</summary>
        public float gameTime => Time.time - _m_gameStartTime;
        
        #region Unity生命周期
        
        /// <summary>
        /// Awake - 初始化单例和基本设置
        /// </summary>
        private void Awake()
        {
            // 单例模式实现
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                _initializeManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Start - 启动游戏管理器
        /// </summary>
        private void Start()
        {
            _subscribeToEvents();
            _setGameState(GameState.MainMenu);
        }
        
        /// <summary>
        /// Update - 每帧更新游戏逻辑
        /// </summary>
        private void Update()
        {
            if (_m_isPaused) return;
            
            // 处理自动保存
            if (_m_autoSave && isGameActive)
            {
                _handleAutoSave();
            }
            
            // 处理游戏状态更新
            _updateGameState();
        }
        
        /// <summary>
        /// OnDestroy - 清理资源和取消事件订阅
        /// </summary>
        private void OnDestroy()
        {
            _unsubscribeFromEvents();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 开始新游戏（接口实现）
        /// </summary>
        /// <param name="_playerCount">玩家数量</param>
        public void startNewGame(int _playerCount)
        {
            startNewGame(_playerCount, GameMode.LocalMultiplayer);
        }
        
        /// <summary>
        /// 开始新游戏
        /// </summary>
        /// <param name="_playerCount">玩家数量</param>
        /// <param name="_gameMode">游戏模式</param>
        public void startNewGame(int _playerCount, GameMode _gameMode)
        {
            // 参数验证
            if (_playerCount < _m_minPlayers || _playerCount > _m_maxPlayers)
            {
                Debug.LogError($"[GameManager] 无效的玩家数量: {_playerCount}，有效范围: {_m_minPlayers}-{_m_maxPlayers}");
                GameEvents.instance.onShowMessage?.Invoke($"玩家数量必须在{_m_minPlayers}-{_m_maxPlayers}之间", MessageType.Error);
                return;
            }
            
            Debug.Log($"[GameManager] 开始新游戏 - 玩家数量: {_playerCount}, 模式: {_gameMode}");
            
            // 设置游戏参数
            _m_currentGameMode = _gameMode;
            _m_gameStartTime = Time.time;
            _m_lastAutoSaveTime = Time.time;
            
            // 初始化游戏组件
            _m_boardManager?.initializeBoard();
            _m_playerManager?.initializePlayers(_playerCount, _gameMode);
            
            // 委托给GameStateManager处理游戏状态和回合管理
            if (_m_gameStateManager != null)
            {
                _m_gameStateManager.startNewGame(_playerCount);
            }
            else
            {
                // 备用方案：直接管理状态
                _m_currentPlayerId = 1;
                _setGameState(GameState.GamePlaying);
                GameEvents.onGameStarted?.Invoke(_playerCount);
                GameEvents.onTurnStarted?.Invoke(_m_currentPlayerId, _m_turnNumber);
            }
        }
        
        /// <summary>
        /// 切换到下一个玩家的回合（接口实现）
        /// </summary>
        public void nextTurn()
        {
            endCurrentTurn();
        }
        
        /// <summary>
        /// 结束当前回合
        /// </summary>
        public void endCurrentTurn()
        {
            if (!isGameActive)
            {
                Debug.LogWarning("[GameManager] 游戏未进行中，无法结束回合");
                return;
            }
            
            Debug.Log($"[GameManager] 结束玩家 {currentPlayerId} 的回合");
            
            // 委托给GameStateManager处理回合切换
            if (_m_gameStateManager != null)
            {
                _m_gameStateManager.nextTurn();
            }
            else
            {
                // 备用方案：直接管理回合
                GameEvents.onTurnEnded?.Invoke(_m_currentPlayerId, _m_turnNumber);
                
                if (_checkGameEndCondition())
                {
                    _endGame();
                    return;
                }
                
                _nextPlayer();
                GameEvents.onTurnStarted?.Invoke(_m_currentPlayerId, _m_turnNumber);
            }
        }
        
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void pauseGame()
        {
            if (!isGameActive)
            {
                Debug.LogWarning("[GameManager] 游戏未进行中，无法暂停");
                return;
            }
            
            // 委托给GameStateManager处理暂停
            if (_m_gameStateManager != null)
            {
                _m_gameStateManager.pauseGame();
            }
            else
            {
                // 备用方案：直接管理状态
                _setGameState(GameState.GamePaused);
                Debug.Log("[GameManager] 游戏已暂停");
                GameEvents.onGamePaused?.Invoke();
            }
        }
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void resumeGame()
        {
            if (currentGameState != GameState.GamePaused)
            {
                Debug.LogWarning("[GameManager] 游戏未暂停，无法恢复");
                return;
            }
            
            // 委托给GameStateManager处理恢复
            if (_m_gameStateManager != null)
            {
                _m_gameStateManager.resumeGame();
            }
            else
            {
                // 备用方案：直接管理状态
                _setGameState(GameState.GamePlaying);
                Debug.Log("[GameManager] 游戏已恢复");
                GameEvents.onGameResumed?.Invoke();
            }
        }
        
        /// <summary>
        /// 保存游戏状态
        /// </summary>
        public void saveGameState()
        {
            if (!isGameActive)
            {
                Debug.LogWarning("[GameManager] 游戏未进行中，无法保存");
                return;
            }
            
            try
            {
                // TODO: 实现游戏状态保存逻辑
                Debug.Log("[GameManager] 游戏状态已保存");
                _m_lastAutoSaveTime = Time.time;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameManager] 保存游戏状态失败: {ex.Message}");
                GameEvents.instance.onShowMessage?.Invoke("保存游戏失败", MessageType.Error);
            }
        }
        
        /// <summary>
        /// 加载游戏状态
        /// </summary>
        public void loadGameState()
        {
            try
            {
                // TODO: 实现游戏状态加载逻辑
                Debug.Log("[GameManager] 游戏状态已加载");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameManager] 加载游戏状态失败: {ex.Message}");
                GameEvents.instance.onShowMessage?.Invoke("加载游戏失败", MessageType.Error);
            }
        }
        
        /// <summary>
        /// 退出到主菜单
        /// </summary>
        public void exitToMainMenu()
        {
            Debug.Log("[GameManager] 退出到主菜单");
            
            // 重置游戏状态
            _resetGameState();
            
            // 设置为主菜单状态
            _setGameState(GameState.MainMenu);
        }
        
        /// <summary>
        /// 跳过当前玩家的回合
        /// 当玩家无法放置任何方块时调用
        /// </summary>
        public void skipCurrentPlayer()
        {
            if (!isGameActive)
            {
                Debug.LogWarning("[GameManager] 游戏未进行中，无法跳过回合");
                return;
            }
            
            Debug.Log($"[GameManager] 玩家 {_m_currentPlayerId} 跳过回合");
            
            // 触发玩家跳过事件
            GameEvents.onPlayerSkipped?.Invoke(_m_currentPlayerId);
            
            // 切换到下一回合
            endCurrentTurn();
        }
        
        /// <summary>
        /// 获取游戏进度百分比
        /// 基于已放置方块数量计算游戏进度
        /// </summary>
        /// <returns>游戏进度 (0.0 - 1.0)</returns>
        public float getGameProgress()
        {
            if (_m_playerManager == null)
                return 0f;
            
            int totalPieces = 0;
            int placedPieces = 0;
            int playerCount = _m_playerManager.getPlayerCount();
            
            for (int i = 1; i <= playerCount; i++)
            {
                var player = _m_playerManager.getPlayer(i);
                if (player != null)
                {
                    // TODO: 实现获取玩家方块数量的方法
                    // totalPieces += player.getTotalPieceCount();
                    // placedPieces += player.getPlacedPieceCount();
                }
            }
            
            return totalPieces > 0 ? (float)placedPieces / totalPieces : 0f;
        }
        
        /// <summary>
        /// 获取玩家的游戏状态
        /// 返回玩家是否还在游戏中、是否跳过等信息
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>玩家游戏状态</returns>
        public PlayerGameState getPlayerState(int _playerId)
        {
            if (_m_playerManager != null)
            {
                var player = _m_playerManager.getPlayer(_playerId);
                if (player != null)
                {
                    // TODO: 实现从Player获取状态的方法
                    // return player.getGameState();
                }
            }
            
            return PlayerGameState.Active; // 默认状态
        }
        
        /// <summary>
        /// 设置玩家的游戏状态
        /// 更新玩家的参与状态（活跃、跳过、退出等）
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_state">新的游戏状态</param>
        public void setPlayerState(int _playerId, PlayerGameState _state)
        {
            if (_m_playerManager != null)
            {
                var player = _m_playerManager.getPlayer(_playerId);
                if (player != null)
                {
                    // TODO: 实现设置Player状态的方法
                    // player.setGameState(_state);
                    Debug.Log($"[GameManager] 玩家 {_playerId} 状态变更为: {_state}");
                    
                    // 触发玩家状态变更事件
                    GameEvents.onPlayerStateChanged?.Invoke(_playerId, _state);
                }
            }
        }
        
        /// <summary>
        /// 检查是否可以切换到下一回合
        /// 验证当前玩家是否完成了回合操作
        /// </summary>
        /// <returns>是否可以切换回合</returns>
        public bool canAdvanceTurn()
        {
            return isGameActive;
        }
        
        /// <summary>
        /// 结束游戏
        /// 计算最终得分，触发游戏结束事件
        /// </summary>
        public void endGame()
        {
            if (currentState == GameState.GameEnded)
            {
                Debug.LogWarning("[GameManager] 游戏已经结束");
                return;
            }
            
            Debug.Log("[GameManager] 游戏结束");
            _endGame();
        }
        
        /// <summary>
        /// 重置游戏
        /// 清除所有游戏数据，返回到初始状态
        /// </summary>
        public void resetGame()
        {
            Debug.Log("[GameManager] 重置游戏");
            
            // 重置游戏状态
            _resetGameState();
            
            // 设置为主菜单状态
            _setGameState(GameState.MainMenu);
            
            // 触发游戏重置事件
            GameEvents.onGameReset?.Invoke();
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 初始化管理器
        /// </summary>
        private void _initializeManager()
        {
            Debug.Log("[GameManager] 初始化游戏管理器");
            
            // 自动获取组件引用
            if (_m_gameStateManager == null)
                _m_gameStateManager = FindObjectOfType<GameStateManager>();
            if (_m_boardManager == null)
                _m_boardManager = FindObjectOfType<BoardManager>();
            if (_m_playerManager == null)
                _m_playerManager = FindObjectOfType<PlayerManager>();
            if (_m_inputManager == null)
                _m_inputManager = FindObjectOfType<InputManager>();
            if (_m_uiManager == null)
                _m_uiManager = FindObjectOfType<UIManager>();
            
            // 验证组件引用
            if (_m_gameStateManager == null)
                Debug.LogError("[GameManager] GameStateManager 引用缺失");
            if (_m_boardManager == null)
                Debug.LogError("[GameManager] BoardManager 引用缺失");
            if (_m_playerManager == null)
                Debug.LogError("[GameManager] PlayerManager 引用缺失");
            if (_m_inputManager == null)
                Debug.LogError("[GameManager] InputManager 引用缺失");
            if (_m_uiManager == null)
                Debug.LogError("[GameManager] UIManager 引用缺失");
        }
        
        /// <summary>
        /// 订阅游戏事件
        /// </summary>
        private void _subscribeToEvents()
        {
            GameEvents.onPiecePlaced += _onPiecePlaced;
            GameEvents.instance.onPiecePlacementFailed += _onPiecePlacementFailed;
            GameEvents.instance.onPlayerEliminated += _onPlayerEliminated;
        }
        
        /// <summary>
        /// 取消事件订阅
        /// </summary>
        private void _unsubscribeFromEvents()
        {
            GameEvents.onPiecePlaced -= _onPiecePlaced;
            GameEvents.instance.onPiecePlacementFailed -= _onPiecePlacementFailed;
            GameEvents.instance.onPlayerEliminated -= _onPlayerEliminated;
        }
        
        /// <summary>
        /// 设置游戏状态
        /// </summary>
        /// <param name="_newState">新的游戏状态</param>
        private void _setGameState(GameState _newState)
        {
            if (_m_currentGameState == _newState) return;
            
            var oldState = _m_currentGameState;
            _m_currentGameState = _newState;
            
            Debug.Log($"[GameManager] 游戏状态变更: {oldState} -> {_newState}");
            
            // 根据新状态执行相应逻辑
            switch (_newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    _m_isPaused = false;
                    break;
                    
                case GameState.GamePlaying:
                    Time.timeScale = 1f;
                    _m_isPaused = false;
                    break;
                    
                case GameState.GamePaused:
                    Time.timeScale = 0f;
                    _m_isPaused = true;
                    break;
                    
                case GameState.GameEnded:
                    // 游戏结束处理
                    Time.timeScale = 1f;
                    _m_isPaused = false;
                    break;
            }
        }
        
        /// <summary>
        /// 更新游戏状态
        /// </summary>
        private void _updateGameState()
        {
            switch (_m_currentGameState)
            {
                case GameState.GamePlaying:
                    // 检查当前玩家是否还能继续游戏
                    if (_m_playerManager != null)
                    {
                        var currentPlayer = _m_playerManager.getCurrentPlayer();
                        if (currentPlayer != null && !currentPlayer.canContinueGame(_m_boardManager))
                        {
                            Debug.Log($"[GameManager] 玩家 {_m_currentPlayerId} 无法继续游戏，跳过回合");
                            GameEvents.onPlayerSkipped?.Invoke(_m_currentPlayerId);
                            endCurrentTurn();
                        }
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 切换到下一个玩家（备用方案，当GameStateManager不可用时使用）
        /// </summary>
        private void _nextPlayer()
        {
            int totalPlayers = _m_playerManager?.getPlayerCount() ?? 0;
            if (totalPlayers == 0) return;
            
            // 寻找下一个活跃玩家
            int nextPlayerId = _m_currentPlayerId;
            int attempts = 0;
            
            do
            {
                nextPlayerId = (nextPlayerId % totalPlayers) + 1;
                attempts++;
                
                // 防止无限循环
                if (attempts > totalPlayers)
                {
                    Debug.LogError("[GameManager] 无法找到活跃玩家，结束游戏");
                    _endGame();
                    return;
                }
            }
            while (!_m_playerManager.isPlayerActive(nextPlayerId));
            
            _m_currentPlayerId = nextPlayerId;
            
            // 如果回到第一个玩家，增加回合数
            if (_m_currentPlayerId == 1)
            {
                _m_turnNumber++;
            }
            
            Debug.Log($"[GameManager] 切换到玩家 {_m_currentPlayerId}，第 {_m_turnNumber} 回合");
        }
        
        /// <summary>
        /// 检查游戏结束条件
        /// </summary>
        /// <returns>游戏是否应该结束</returns>
        private bool _checkGameEndCondition()
        {
            if (_m_playerManager == null) return true;
            
            // 检查是否还有活跃玩家能够继续游戏
            int activePlayersWithMoves = 0;
            int totalPlayers = _m_playerManager.getPlayerCount();
            
            for (int i = 1; i <= totalPlayers; i++)
            {
                var player = _m_playerManager.getPlayer(i);
                if (player != null && player.isActive && player.canContinueGame(_m_boardManager))
                {
                    activePlayersWithMoves++;
                }
            }
            
            return activePlayersWithMoves == 0;
        }
        
        /// <summary>
        /// 结束游戏
        /// </summary>
        private void _endGame()
        {
            Debug.Log("[GameManager] 游戏结束");
            
            // 计算最终分数和获胜者
            var (winnerId, finalScores) = _calculateFinalResults();
            
            // 设置游戏状态
            _setGameState(GameState.GameEnded);
            
            // 触发游戏结束事件
            GameEvents.onGameEnded?.Invoke(finalScores);
            
            Debug.Log($"[GameManager] 获胜者: 玩家 {winnerId}");
        }
        
        /// <summary>
        /// 计算最终结果
        /// </summary>
        /// <returns>获胜者ID和所有玩家分数字典</returns>
        private (int winnerId, Dictionary<int, int> finalScores) _calculateFinalResults()
        {
            if (_m_playerManager == null)
                return (0, new Dictionary<int, int>());
            
            int playerCount = _m_playerManager.getPlayerCount();
            var scores = new Dictionary<int, int>();
            int winnerId = 1;
            int highestScore = -1;
            
            // 计算每个玩家的最终分数
            for (int i = 1; i <= playerCount; i++)
            {
                var player = _m_playerManager.getPlayer(i);
                if (player != null)
                {
                    int playerScore = player.calculateScore();
                    scores[i] = playerScore;
                    
                    // 找出最高分玩家
                    if (playerScore > highestScore)
                    {
                        highestScore = playerScore;
                        winnerId = i;
                    }
                }
            }
            
            return (winnerId, scores);
        }
        
        /// <summary>
        /// 重置游戏状态
        /// </summary>
        private void _resetGameState()
        {
            // 委托给GameStateManager重置状态
            if (_m_gameStateManager != null)
            {
                _m_gameStateManager.resetGame();
            }
            else
            {
                // 备用方案：直接重置
                _m_currentPlayerId = 1;
                _m_turnNumber = 1;
            }
            
            _m_gameStartTime = 0f;
            _m_lastAutoSaveTime = 0f;
            _m_isPaused = false;
            Time.timeScale = 1f;
            
            // 重置各个管理器
            _m_boardManager?.clearBoard();
            _m_playerManager?.resetAllPlayers();
        }
        
        /// <summary>
        /// 处理自动保存
        /// </summary>
        private void _handleAutoSave()
        {
            if (Time.time - _m_lastAutoSaveTime >= _m_autoSaveInterval)
            {
                saveGameState();
            }
        }
        
        #endregion
        
        #region 事件处理
        
        /// <summary>
        /// 处理方块放置成功事件
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_piece">放置的方块</param>
        /// <param name="_position">放置位置</param>
        private void _onPiecePlaced(int _playerId, _IGamePiece _piece, Vector2Int _position)
        {
            Debug.Log($"[GameManager] 玩家 {_playerId} 成功放置方块 {_piece.pieceId} 在位置 {_position}");
            
            // 更新玩家分数
            var player = _m_playerManager?.getPlayer(_playerId);
            if (player != null)
            {
                int newScore = player.calculateScore();
                GameEvents.instance.onPlayerScoreUpdated?.Invoke(_playerId, newScore);
            }
            
            // 结束当前回合
            endCurrentTurn();
        }
        
        /// <summary>
        /// 处理方块放置失败事件
        /// </summary>
        /// <param name="_piece">尝试放置的方块</param>
        /// <param name="_position">尝试放置的位置</param>
        /// <param name="_playerId">尝试放置的玩家ID</param>
        /// <param name="_reason">放置失败的原因</param>
        private void _onPiecePlacementFailed(_IGamePiece _piece, Vector2Int _position, int _playerId, string _reason)
        {
            Debug.LogWarning($"[GameManager] 玩家 {_playerId} 放置方块失败: {_reason}");
            GameEvents.instance.onShowMessage?.Invoke($"放置失败: {_reason}", MessageType.Warning);
        }
        
        /// <summary>
        /// 处理玩家淘汰事件
        /// </summary>
        /// <param name="_playerId">被淘汰的玩家ID</param>
        private void _onPlayerEliminated(int _playerId)
        {
            Debug.Log($"[GameManager] 玩家 {_playerId} 被淘汰");
            
            // 如果当前回合玩家被淘汰，切换到下一个玩家
            if (_playerId == _m_currentPlayerId)
            {
                endCurrentTurn();
            }
        }
        
        #endregion
    }
}