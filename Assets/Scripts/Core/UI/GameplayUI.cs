using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BlokusGame.Core.Managers;
using BlokusGame.Core.Data;
using BlokusGame.Core.Events;
using BlokusGame.Core.Interfaces;

namespace BlokusGame.Core.UI
{
    /// <summary>
    /// 游戏内UI - 显示游戏进行时的所有界面元素
    /// 包括玩家信息、方块库存、游戏控制和状态显示
    /// </summary>
    public class GameplayUI : UIBase
    {
        [Header("游戏状态显示")]
        /// <summary>当前回合玩家文本</summary>
        [SerializeField] private Text _m_currentPlayerText;
        
        /// <summary>回合计时器文本</summary>
        [SerializeField] private Text _m_turnTimerText;
        
        /// <summary>游戏时间文本</summary>
        [SerializeField] private Text _m_gameTimeText;
        
        /// <summary>游戏状态文本</summary>
        [SerializeField] private Text _m_gameStatusText;
        
        [Header("玩家信息面板")]
        /// <summary>玩家信息面板容器</summary>
        [SerializeField] private Transform _m_playerInfoContainer;
        
        /// <summary>玩家信息面板预制体</summary>
        [SerializeField] private GameObject _m_playerInfoPrefab;
        
        [Header("方块库存显示")]
        /// <summary>方块库存面板</summary>
        [SerializeField] private Transform _m_pieceInventoryPanel;
        
        /// <summary>方块图标预制体</summary>
        [SerializeField] private GameObject _m_pieceIconPrefab;
        
        /// <summary>选中方块显示区域</summary>
        [SerializeField] private Transform _m_selectedPieceDisplay;
        
        [Header("游戏控制按钮")]
        /// <summary>暂停游戏按钮</summary>
        [SerializeField] private Button _m_pauseButton;
        
        /// <summary>跳过回合按钮</summary>
        [SerializeField] private Button _m_skipTurnButton;
        
        /// <summary>撤销操作按钮</summary>
        [SerializeField] private Button _m_undoButton;
        
        /// <summary>旋转方块按钮</summary>
        [SerializeField] private Button _m_rotateButton;
        
        /// <summary>翻转方块按钮</summary>
        [SerializeField] private Button _m_flipButton;
        
        /// <summary>确认放置按钮</summary>
        [SerializeField] private Button _m_confirmButton;
        
        [Header("提示和帮助")]
        /// <summary>提示文本</summary>
        [SerializeField] private Text _m_hintText;
        
        /// <summary>帮助按钮</summary>
        [SerializeField] private Button _m_helpButton;
        
        /// <summary>规则说明按钮</summary>
        [SerializeField] private Button _m_rulesButton;
        
        // 私有字段
        /// <summary>玩家信息UI列表</summary>
        private List<PlayerInfoUI> _m_playerInfoUIs = new List<PlayerInfoUI>();
        
        /// <summary>方块图标UI列表</summary>
        private List<PieceIconUI> _m_pieceIconUIs = new List<PieceIconUI>();
        
        /// <summary>当前选中的方块</summary>
        private _IGamePiece _m_selectedPiece;
        
        /// <summary>游戏开始时间</summary>
        private float _m_gameStartTime;
        
        /// <summary>当前回合开始时间</summary>
        private float _m_turnStartTime;
        
        #region UIBase实现
        
        /// <summary>
        /// 初始化UI特定内容
        /// </summary>
        protected override void InitializeUIContent()
        {
            _setupUI();
            _bindEvents();
        }
        
        /// <summary>
        /// 处理UI显示时的逻辑
        /// </summary>
        protected override void OnUIShown()
        {
            _initializeGameplayUI();
            _subscribeToGameEvents();
        }
        
        /// <summary>
        /// 处理UI隐藏时的逻辑
        /// </summary>
        protected override void OnUIHidden()
        {
            _unsubscribeFromGameEvents();
        }
        
        #endregion
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Update方法 - 更新计时器和状态
        /// </summary>
        private void Update()
        {
            if (isVisible)
            {
                _updateTimers();
                _updateGameStatus();
            }
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 初始化游戏UI
        /// </summary>
        /// <param name="_playerCount">玩家数量</param>
        public void InitializeForGame(int _playerCount)
        {
            _createPlayerInfoUIs(_playerCount);
            _initializePieceInventory();
            _resetTimers();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[GameplayUI] 为{_playerCount}人游戏初始化UI");
            }
        }
        
        /// <summary>
        /// 更新当前玩家显示
        /// </summary>
        /// <param name="_playerId">当前玩家ID</param>
        /// <param name="_playerName">玩家名称</param>
        /// <param name="_playerColor">玩家颜色</param>
        public void UpdateCurrentPlayer(int _playerId, string _playerName, Color _playerColor)
        {
            if (_m_currentPlayerText != null)
            {
                _m_currentPlayerText.text = $"当前玩家: {_playerName}";
                _m_currentPlayerText.color = _playerColor;
            }
            
            // 高亮当前玩家的信息面板
            _highlightPlayerInfo(_playerId);
            
            // 重置回合计时器
            _m_turnStartTime = Time.time;
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[GameplayUI] 更新当前玩家: {_playerName} (ID: {_playerId})");
            }
        }
        
        /// <summary>
        /// 更新玩家分数
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_score">新分数</param>
        public void UpdatePlayerScore(int _playerId, int _score)
        {
            if (_playerId >= 0 && _playerId < _m_playerInfoUIs.Count)
            {
                _m_playerInfoUIs[_playerId].UpdateScore(_score);
            }
        }
        
        /// <summary>
        /// 更新玩家剩余方块数量
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_remainingPieces">剩余方块数量</param>
        public void UpdatePlayerRemainingPieces(int _playerId, int _remainingPieces)
        {
            if (_playerId >= 0 && _playerId < _m_playerInfoUIs.Count)
            {
                _m_playerInfoUIs[_playerId].UpdateRemainingPieces(_remainingPieces);
            }
        }
        
        /// <summary>
        /// 更新方块库存显示
        /// </summary>
        /// <param name="_availablePieces">可用方块列表</param>
        public void UpdatePieceInventory(List<_IGamePiece> _availablePieces)
        {
            // 清除现有的方块图标
            _clearPieceInventory();
            
            // 创建新的方块图标
            foreach (var piece in _availablePieces)
            {
                _createPieceIcon(piece);
            }
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[GameplayUI] 更新方块库存，可用方块数量: {_availablePieces.Count}");
            }
        }
        
        /// <summary>
        /// 设置选中的方块
        /// </summary>
        /// <param name="_piece">选中的方块</param>
        public void SetSelectedPiece(_IGamePiece _piece)
        {
            _m_selectedPiece = _piece;
            _updateSelectedPieceDisplay();
            _updateControlButtons();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[GameplayUI] 选中方块: {_piece?.pieceId}");
            }
        }
        
        /// <summary>
        /// 显示提示信息
        /// </summary>
        /// <param name="_hint">提示内容</param>
        public void ShowHint(string _hint)
        {
            if (_m_hintText != null)
            {
                _m_hintText.text = _hint;
                _m_hintText.gameObject.SetActive(true);
            }
        }
        
        /// <summary>
        /// 隐藏提示信息
        /// </summary>
        public void HideHint()
        {
            if (_m_hintText != null)
            {
                _m_hintText.gameObject.SetActive(false);
            }
        }
        
        #endregion
        
        #region 私有方法 - 初始化
        
        /// <summary>
        /// 设置UI元素
        /// </summary>
        private void _setupUI()
        {
            // 初始化显示状态
            if (_m_hintText != null)
            {
                _m_hintText.gameObject.SetActive(false);
            }
            
            // 设置按钮初始状态
            _updateControlButtons();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[GameplayUI] UI元素设置完成");
            }
        }
        
        /// <summary>
        /// 绑定按钮事件
        /// </summary>
        private void _bindEvents()
        {
            if (_m_pauseButton != null)
                _m_pauseButton.onClick.AddListener(_onPauseClicked);
            
            if (_m_skipTurnButton != null)
                _m_skipTurnButton.onClick.AddListener(_onSkipTurnClicked);
            
            if (_m_undoButton != null)
                _m_undoButton.onClick.AddListener(_onUndoClicked);
            
            if (_m_rotateButton != null)
                _m_rotateButton.onClick.AddListener(_onRotateClicked);
            
            if (_m_flipButton != null)
                _m_flipButton.onClick.AddListener(_onFlipClicked);
            
            if (_m_confirmButton != null)
                _m_confirmButton.onClick.AddListener(_onConfirmClicked);
            
            if (_m_helpButton != null)
                _m_helpButton.onClick.AddListener(_onHelpClicked);
            
            if (_m_rulesButton != null)
                _m_rulesButton.onClick.AddListener(_onRulesClicked);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[GameplayUI] 按钮事件绑定完成");
            }
        }
        
        /// <summary>
        /// 初始化游戏UI
        /// </summary>
        private void _initializeGameplayUI()
        {
            _m_gameStartTime = Time.time;
            _m_turnStartTime = Time.time;
            
            // 清除之前的数据
            _clearPlayerInfoUIs();
            _clearPieceInventory();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[GameplayUI] 游戏UI初始化完成");
            }
        }
        
        #endregion
        
        #region 私有方法 - 玩家信息UI
        
        /// <summary>
        /// 创建玩家信息UI
        /// </summary>
        /// <param name="_playerCount">玩家数量</param>
        private void _createPlayerInfoUIs(int _playerCount)
        {
            _clearPlayerInfoUIs();
            
            if (_m_playerInfoContainer == null || _m_playerInfoPrefab == null)
            {
                Debug.LogWarning("[GameplayUI] 玩家信息容器或预制体未配置");
                return;
            }
            
            for (int i = 0; i < _playerCount; i++)
            {
                var playerInfoObj = Instantiate(_m_playerInfoPrefab, _m_playerInfoContainer);
                var playerInfoUI = playerInfoObj.GetComponent<PlayerInfoUI>();
                
                if (playerInfoUI != null)
                {
                    playerInfoUI.Initialize(i, $"玩家{i + 1}", Color.white, 0, 21);
                    _m_playerInfoUIs.Add(playerInfoUI);
                }
            }
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[GameplayUI] 创建了{_playerCount}个玩家信息UI");
            }
        }
        
        /// <summary>
        /// 清除玩家信息UI
        /// </summary>
        private void _clearPlayerInfoUIs()
        {
            foreach (var playerInfoUI in _m_playerInfoUIs)
            {
                if (playerInfoUI != null)
                {
                    Destroy(playerInfoUI.gameObject);
                }
            }
            _m_playerInfoUIs.Clear();
        }
        
        /// <summary>
        /// 高亮指定玩家的信息面板
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        private void _highlightPlayerInfo(int _playerId)
        {
            for (int i = 0; i < _m_playerInfoUIs.Count; i++)
            {
                _m_playerInfoUIs[i].SetHighlight(i == _playerId);
            }
        }
        
        #endregion
        
        #region 私有方法 - 方块库存UI
        
        /// <summary>
        /// 初始化方块库存
        /// </summary>
        private void _initializePieceInventory()
        {
            _clearPieceInventory();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[GameplayUI] 方块库存初始化完成");
            }
        }
        
        /// <summary>
        /// 创建方块图标
        /// </summary>
        /// <param name="_piece">方块数据</param>
        private void _createPieceIcon(_IGamePiece _piece)
        {
            if (_m_pieceInventoryPanel == null || _m_pieceIconPrefab == null)
            {
                Debug.LogWarning("[GameplayUI] 方块库存面板或预制体未配置");
                return;
            }
            
            var pieceIconObj = Instantiate(_m_pieceIconPrefab, _m_pieceInventoryPanel);
            var pieceIconUI = pieceIconObj.GetComponent<PieceIconUI>();
            
            if (pieceIconUI != null)
            {
                pieceIconUI.Initialize(_piece);
                pieceIconUI.onPieceClicked += _onPieceIconClicked;
                _m_pieceIconUIs.Add(pieceIconUI);
            }
        }
        
        /// <summary>
        /// 清除方块库存
        /// </summary>
        private void _clearPieceInventory()
        {
            foreach (var pieceIconUI in _m_pieceIconUIs)
            {
                if (pieceIconUI != null)
                {
                    pieceIconUI.onPieceClicked -= _onPieceIconClicked;
                    Destroy(pieceIconUI.gameObject);
                }
            }
            _m_pieceIconUIs.Clear();
        }
        
        /// <summary>
        /// 更新选中方块显示
        /// </summary>
        private void _updateSelectedPieceDisplay()
        {
            if (_m_selectedPieceDisplay == null) return;
            
            // 清除现有显示
            foreach (Transform child in _m_selectedPieceDisplay)
            {
                Destroy(child.gameObject);
            }
            
            // 显示选中的方块
            if (_m_selectedPiece != null)
            {
                // TODO: 创建选中方块的3D预览
                // 这里需要根据方块数据创建可视化表示
            }
        }
        
        #endregion
        
        #region 私有方法 - 状态更新
        
        /// <summary>
        /// 更新计时器显示
        /// </summary>
        private void _updateTimers()
        {
            // 更新游戏时间
            if (_m_gameTimeText != null)
            {
                float gameTime = Time.time - _m_gameStartTime;
                _m_gameTimeText.text = $"游戏时间: {_formatTime(gameTime)}";
            }
            
            // 更新回合时间
            if (_m_turnTimerText != null)
            {
                float turnTime = Time.time - _m_turnStartTime;
                _m_turnTimerText.text = $"回合时间: {_formatTime(turnTime)}";
            }
        }
        
        /// <summary>
        /// 更新游戏状态显示
        /// </summary>
        private void _updateGameStatus()
        {
            if (_m_gameStatusText == null) return;
            
            if (GameManager.instance != null)
            {
                var gameState = GameManager.instance.currentGameState;
                _m_gameStatusText.text = _getGameStateDisplayText(gameState);
            }
        }
        
        /// <summary>
        /// 更新控制按钮状态
        /// </summary>
        private void _updateControlButtons()
        {
            bool hasPieceSelected = _m_selectedPiece != null;
            bool isCurrentPlayerTurn = GameManager.instance != null && GameManager.instance.isCurrentPlayerTurn;
            
            if (_m_rotateButton != null)
                _m_rotateButton.interactable = hasPieceSelected && isCurrentPlayerTurn;
            
            if (_m_flipButton != null)
                _m_flipButton.interactable = hasPieceSelected && isCurrentPlayerTurn;
            
            if (_m_confirmButton != null)
                _m_confirmButton.interactable = hasPieceSelected && isCurrentPlayerTurn;
            
            if (_m_skipTurnButton != null)
                _m_skipTurnButton.interactable = isCurrentPlayerTurn;
            
            if (_m_undoButton != null)
                _m_undoButton.interactable = isCurrentPlayerTurn; // TODO: 检查是否有可撤销的操作
        }
        
        #endregion
        
        #region 私有方法 - 事件处理
        
        /// <summary>
        /// 订阅游戏事件
        /// </summary>
        private void _subscribeToGameEvents()
        {
            GameEvents.onTurnStarted += _onTurnStarted;
            GameEvents.instance.onPlayerScoreUpdated += _onPlayerScoreUpdated;
            GameEvents.instance.onPieceSelected += _onPieceSelected;
            GameEvents.instance.onGameStateChanged += _onGameStateChanged;
        }
        
        /// <summary>
        /// 取消游戏事件订阅
        /// </summary>
        private void _unsubscribeFromGameEvents()
        {
            GameEvents.onTurnStarted -= _onTurnStarted;
            GameEvents.instance.onPlayerScoreUpdated -= _onPlayerScoreUpdated;
            GameEvents.instance.onPieceSelected -= _onPieceSelected;
            GameEvents.instance.onGameStateChanged -= _onGameStateChanged;
        }
        
        /// <summary>
        /// 处理回合开始事件
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_turnNumber">回合数</param>
        private void _onTurnStarted(int _playerId, int _turnNumber)
        {
            // TODO: 从PlayerManager获取玩家信息
            UpdateCurrentPlayer(_playerId, $"玩家{_playerId + 1}", Color.white);
        }
        
        /// <summary>
        /// 处理玩家分数更新事件
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_newScore">新分数</param>
        private void _onPlayerScoreUpdated(int _playerId, int _newScore)
        {
            UpdatePlayerScore(_playerId, _newScore);
        }
        
        /// <summary>
        /// 处理方块选择事件
        /// </summary>
        /// <param name="_piece">选中的方块</param>
        /// <param name="_playerId">玩家ID</param>
        private void _onPieceSelected(_IGamePiece _piece, int _playerId)
        {
            SetSelectedPiece(_piece);
        }
        
        /// <summary>
        /// 处理游戏状态变更事件
        /// </summary>
        /// <param name="_oldState">旧的游戏状态</param>
        /// <param name="_newState">新的游戏状态</param>
        private void _onGameStateChanged(GameState _oldState, GameState _newState)
        {
            _updateControlButtons();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[GameplayUI] 游戏状态变更: {_oldState} -> {_newState}");
            }
        }
        
        /// <summary>
        /// 处理方块图标点击事件
        /// </summary>
        /// <param name="_piece">点击的方块</param>
        private void _onPieceIconClicked(_IGamePiece _piece)
        {
            SetSelectedPiece(_piece);
            
            // 通知游戏管理器方块被选中
            GameEvents.instance.onPieceSelected?.Invoke(_piece, 0); // TODO: 获取正确的玩家ID
        }
        
        /// <summary>
        /// 处理暂停按钮点击
        /// </summary>
        private void _onPauseClicked()
        {
            GameManager.instance?.pauseGame();
        }
        
        /// <summary>
        /// 处理跳过回合按钮点击
        /// </summary>
        private void _onSkipTurnClicked()
        {
            GameManager.instance?.skipCurrentPlayer();
        }
        
        /// <summary>
        /// 处理撤销按钮点击
        /// </summary>
        private void _onUndoClicked()
        {
            // TODO: 实现撤销功能
            UIManager.instance?.ShowMessage("撤销功能暂未实现", MessageType.Info);
        }
        
        /// <summary>
        /// 处理旋转按钮点击
        /// </summary>
        private void _onRotateClicked()
        {
            if (_m_selectedPiece != null)
            {
                _m_selectedPiece.rotate90Clockwise();
                _updateSelectedPieceDisplay();
            }
        }
        
        /// <summary>
        /// 处理翻转按钮点击
        /// </summary>
        private void _onFlipClicked()
        {
            if (_m_selectedPiece != null)
            {
                _m_selectedPiece.flipHorizontal();
                _updateSelectedPieceDisplay();
            }
        }
        
        /// <summary>
        /// 处理确认按钮点击
        /// </summary>
        private void _onConfirmClicked()
        {
            // TODO: 实现确认放置功能
            UIManager.instance?.ShowMessage("确认放置功能暂未实现", MessageType.Info);
        }
        
        /// <summary>
        /// 处理帮助按钮点击
        /// </summary>
        private void _onHelpClicked()
        {
            // TODO: 显示帮助界面
            UIManager.instance?.ShowMessage("帮助功能暂未实现", MessageType.Info);
        }
        
        /// <summary>
        /// 处理规则按钮点击
        /// </summary>
        private void _onRulesClicked()
        {
            // TODO: 显示规则说明界面
            UIManager.instance?.ShowMessage("规则说明功能暂未实现", MessageType.Info);
        }
        
        #endregion
        
        #region 私有方法 - 辅助功能
        
        /// <summary>
        /// 重置计时器
        /// </summary>
        private void _resetTimers()
        {
            _m_gameStartTime = Time.time;
            _m_turnStartTime = Time.time;
        }
        
        /// <summary>
        /// 格式化时间显示
        /// </summary>
        /// <param name="_timeInSeconds">时间（秒）</param>
        /// <returns>格式化的时间字符串</returns>
        private string _formatTime(float _timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(_timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(_timeInSeconds % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
        
        /// <summary>
        /// 获取游戏状态显示文本
        /// </summary>
        /// <param name="_gameState">游戏状态</param>
        /// <returns>显示文本</returns>
        private string _getGameStateDisplayText(GameState _gameState)
        {
            switch (_gameState)
            {
                case GameState.GamePlaying:
                    return "游戏进行中";
                case GameState.GamePaused:
                    return "游戏已暂停";
                case GameState.GameEnded:
                    return "游戏结束";
                case GameState.WaitingForPlayers:
                    return "等待玩家";
                default:
                    return "未知状态";
            }
        }
        
        #endregion
    }
}