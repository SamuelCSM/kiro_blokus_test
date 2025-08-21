using BlokusGame.Core.Data;
using BlokusGame.Core.Events;
using BlokusGame.Core.Scoring;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlokusGame.Core.Managers
{
    /// <summary>
    /// 游戏记录管理器 - 负责游戏记录的保存、加载和管理
    /// 提供游戏历史查询、统计分析和数据持久化功能
    /// 支持本地存储和云端同步（预留接口）
    /// </summary>
    public class GameRecordManager : MonoBehaviour
    {
        [Header("记录管理配置")]
        /// <summary>是否启用游戏记录</summary>
        [SerializeField] private bool _m_enableGameRecording = true;
        
        /// <summary>最大保存记录数量</summary>
        [SerializeField] private int _m_maxRecordsToKeep = 100;
        
        /// <summary>是否自动保存记录</summary>
        [SerializeField] private bool _m_autoSaveRecords = true;
        
        /// <summary>记录保存路径</summary>
        [SerializeField] private string _m_recordSavePath = "GameRecords";
        
        /// <summary>是否启用详细日志</summary>
        [SerializeField] private bool _m_enableDetailedLogging = false;
        
        // 私有字段
        /// <summary>当前游戏记录</summary>
        private GameRecord _m_currentGameRecord;
        
        /// <summary>游戏记录列表</summary>
        private List<GameRecord> _m_gameRecords = new List<GameRecord>();
        
        /// <summary>当前回合记录</summary>
        private TurnRecord _m_currentTurnRecord;
        
        /// <summary>记录文件完整路径</summary>
        private string _m_fullRecordPath;
        
        /// <summary>单例实例</summary>
        public static GameRecordManager instance { get; private set; }
        
        /// <summary>当前游戏记录</summary>
        public GameRecord currentGameRecord => _m_currentGameRecord;
        
        /// <summary>游戏记录总数</summary>
        public int totalRecordsCount => _m_gameRecords.Count;
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法 - 初始化单例
        /// </summary>
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                _initializeRecordManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Unity Start方法 - 加载记录和订阅事件
        /// </summary>
        private void Start()
        {
            _loadGameRecords();
            _subscribeToEvents();
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 保存记录和清理资源
        /// </summary>
        private void OnDestroy()
        {
            _unsubscribeFromEvents();
            
            if (_m_autoSaveRecords)
            {
                _saveGameRecords();
            }
            
            if (instance == this)
            {
                instance = null;
            }
        }
        
        /// <summary>
        /// Unity OnApplicationPause方法 - 应用暂停时保存记录
        /// </summary>
        /// <param name="_pauseStatus">暂停状态</param>
        private void OnApplicationPause(bool _pauseStatus)
        {
            if (_pauseStatus && _m_autoSaveRecords)
            {
                _saveGameRecords();
            }
        }
        
        /// <summary>
        /// Unity OnApplicationFocus方法 - 应用失去焦点时保存记录
        /// </summary>
        /// <param name="_hasFocus">是否有焦点</param>
        private void OnApplicationFocus(bool _hasFocus)
        {
            if (!_hasFocus && _m_autoSaveRecords)
            {
                _saveGameRecords();
            }
        }
        
        #endregion
        
        #region 公共方法 - 游戏记录管理
        
        /// <summary>
        /// 开始新的游戏记录
        /// </summary>
        /// <param name="_gameMode">游戏模式</param>
        /// <param name="_playerCount">玩家数量</param>
        /// <returns>新的游戏记录</returns>
        public GameRecord startNewGameRecord(GameMode _gameMode, int _playerCount)
        {
            if (!_m_enableGameRecording) return null;
            
            _m_currentGameRecord = new GameRecord(_gameMode, _playerCount);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[GameRecordManager] 开始新游戏记录: {_m_currentGameRecord.gameId}");
            }
            
            return _m_currentGameRecord;
        }
        
        /// <summary>
        /// 添加玩家到当前游戏记录
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_playerName">玩家名称</param>
        /// <param name="_isAI">是否为AI玩家</param>
        public void addPlayerToCurrentRecord(int _playerId, string _playerName, bool _isAI = false)
        {
            if (_m_currentGameRecord == null) return;
            
            _m_currentGameRecord.addPlayerRecord(_playerId, _playerName, _isAI);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[GameRecordManager] 添加玩家到记录: {_playerName} (ID: {_playerId})");
            }
        }
        
        /// <summary>
        /// 开始新回合记录
        /// </summary>
        /// <param name="_turnNumber">回合编号</param>
        /// <param name="_playerId">玩家ID</param>
        public void startTurnRecord(int _turnNumber, int _playerId)
        {
            if (_m_currentGameRecord == null) return;
            
            _m_currentTurnRecord = new TurnRecord(_turnNumber, _playerId);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[GameRecordManager] 开始回合记录: 回合{_turnNumber}, 玩家{_playerId}");
            }
        }

        /// <summary>
        /// 结束当前回合记录
        /// </summary>
        /// <param name="_endReason">结束原因</param>
        /// <param name="_placedPieceId">放置的方块ID（如果有）</param>
        /// <param name="_placedPosition">放置位置（如果有）</param>
        public void endTurnRecord(TurnEndReason _endReason, int _placedPieceId, Vector2Int _placedPosition = default)
        {
	        if (_m_currentTurnRecord == null || _m_currentGameRecord == null) return;

	        _m_currentTurnRecord.endTurn(_endReason);


	        _m_currentTurnRecord.placedPieceId = _placedPieceId;
	        
	        _m_currentTurnRecord.placedPosition = _placedPosition;

	        if (_endReason == TurnEndReason.PlayerSkipped || _endReason == TurnEndReason.NoValidMoves)
	        {
		        _m_currentTurnRecord.skippedTurn = true;
	        }

	        _m_currentGameRecord.addTurnRecord(_m_currentTurnRecord);

	        // 更新玩家统计
	        _updatePlayerTurnStats(_m_currentTurnRecord);

	        _m_currentTurnRecord = null;

	        if (_m_enableDetailedLogging)
	        {
		        Debug.Log($"[GameRecordManager] 结束回合记录: {_endReason}");
	        }
        }

        /// <summary>
        /// 完成当前游戏记录
        /// </summary>
        /// <param name="_gameResults">游戏结果</param>
        /// <param name="_playerScores">玩家分数列表</param>
        public void completeCurrentGameRecord(GameResults _gameResults, List<ScoreSystem.PlayerScore> _playerScores)
        {
            if (_m_currentGameRecord == null) return;
            
            _m_currentGameRecord.completeGameRecord(_gameResults, _playerScores);
            
            // 添加到记录列表
            _m_gameRecords.Add(_m_currentGameRecord);
            
            // 限制记录数量
            _limitRecordsCount();
            
            // 自动保存
            if (_m_autoSaveRecords)
            {
                _saveGameRecords();
            }
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[GameRecordManager] 完成游戏记录: {_m_currentGameRecord.gameId}");
                Debug.Log($"[GameRecordManager] 获胜者: {_m_currentGameRecord.winnerName}");
            }
            
            _m_currentGameRecord = null;
        }
        
        #endregion
        
        #region 公共方法 - 记录查询
        
        /// <summary>
        /// 获取所有游戏记录
        /// </summary>
        /// <returns>游戏记录列表</returns>
        public List<GameRecord> getAllGameRecords()
        {
            return new List<GameRecord>(_m_gameRecords);
        }
        
        /// <summary>
        /// 获取最近的游戏记录
        /// </summary>
        /// <param name="_count">记录数量</param>
        /// <returns>最近的游戏记录列表</returns>
        public List<GameRecord> getRecentGameRecords(int _count = 10)
        {
            return _m_gameRecords
                .OrderByDescending(r => r.gameStartTime)
                .Take(_count)
                .ToList();
        }
        
        /// <summary>
        /// 根据游戏模式获取记录
        /// </summary>
        /// <param name="_gameMode">游戏模式</param>
        /// <returns>指定模式的游戏记录列表</returns>
        public List<GameRecord> getRecordsByGameMode(GameMode _gameMode)
        {
            return _m_gameRecords
                .Where(r => r.gameMode == _gameMode)
                .OrderByDescending(r => r.gameStartTime)
                .ToList();
        }
        
        /// <summary>
        /// 获取玩家的游戏记录
        /// </summary>
        /// <param name="_playerName">玩家名称</param>
        /// <returns>玩家的游戏记录列表</returns>
        public List<GameRecord> getPlayerRecords(string _playerName)
        {
            return _m_gameRecords
                .Where(r => r.playerRecords.Any(p => p.playerName == _playerName))
                .OrderByDescending(r => r.gameStartTime)
                .ToList();
        }
        
        /// <summary>
        /// 获取获胜记录
        /// </summary>
        /// <param name="_playerName">玩家名称</param>
        /// <returns>获胜记录列表</returns>
        public List<GameRecord> getWinRecords(string _playerName)
        {
            return _m_gameRecords
                .Where(r => r.winnerName == _playerName)
                .OrderByDescending(r => r.gameStartTime)
                .ToList();
        }
        
        /// <summary>
        /// 获取游戏统计数据
        /// </summary>
        /// <returns>游戏统计数据</returns>
        public GameStatistics getGameStatistics()
        {
            var stats = new GameStatistics();
            
            if (_m_gameRecords.Count == 0) return stats;
            
            stats.totalGamesPlayed = _m_gameRecords.Count;
            stats.totalGameTime = _m_gameRecords.Sum(r => r.gameDuration);
            stats.averageGameTime = stats.totalGameTime / stats.totalGamesPlayed;
            
            stats.totalTurnsPlayed = _m_gameRecords.Sum(r => r.totalTurns);
            stats.averageTurnsPerGame = (float)stats.totalTurnsPlayed / stats.totalGamesPlayed;
            
            stats.totalPiecesPlaced = _m_gameRecords.Sum(r => r.totalPiecesPlaced);
            stats.averagePiecesPerGame = (float)stats.totalPiecesPlaced / stats.totalGamesPlayed;
            
            stats.highestScore = _m_gameRecords.Max(r => r.highestScore);
            stats.lowestScore = _m_gameRecords.Min(r => r.lowestScore);
            
            // 按游戏模式统计
            stats.gamesByMode = new Dictionary<GameMode, int>();
            foreach (GameMode mode in System.Enum.GetValues(typeof(GameMode)))
            {
                stats.gamesByMode[mode] = _m_gameRecords.Count(r => r.gameMode == mode);
            }
            
            return stats;
        }
        
        #endregion
        
        #region 公共方法 - 数据管理
        
        /// <summary>
        /// 手动保存游戏记录
        /// </summary>
        /// <returns>是否保存成功</returns>
        public bool saveGameRecords()
        {
            return _saveGameRecords();
        }
        
        /// <summary>
        /// 手动加载游戏记录
        /// </summary>
        /// <returns>是否加载成功</returns>
        public bool loadGameRecords()
        {
            return _loadGameRecords();
        }
        
        /// <summary>
        /// 清除所有游戏记录
        /// </summary>
        /// <param name="_saveAfterClear">清除后是否保存</param>
        public void clearAllRecords(bool _saveAfterClear = true)
        {
            _m_gameRecords.Clear();
            
            if (_saveAfterClear)
            {
                _saveGameRecords();
            }
            
            Debug.Log("[GameRecordManager] 所有游戏记录已清除");
        }
        
        /// <summary>
        /// 删除指定游戏记录
        /// </summary>
        /// <param name="_gameId">游戏ID</param>
        /// <returns>是否删除成功</returns>
        public bool deleteGameRecord(string _gameId)
        {
            var recordToDelete = _m_gameRecords.FirstOrDefault(r => r.gameId == _gameId);
            if (recordToDelete != null)
            {
                _m_gameRecords.Remove(recordToDelete);
                
                if (_m_autoSaveRecords)
                {
                    _saveGameRecords();
                }
                
                Debug.Log($"[GameRecordManager] 删除游戏记录: {_gameId}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 导出游戏记录为JSON
        /// </summary>
        /// <param name="_filePath">导出文件路径</param>
        /// <returns>是否导出成功</returns>
        public bool exportRecordsToJson(string _filePath)
        {
            try
            {
                var recordsData = new GameRecordsData
                {
                    records = _m_gameRecords,
                    exportTime = System.DateTime.Now,
                    totalRecords = _m_gameRecords.Count
                };
                
                string json = JsonUtility.ToJson(recordsData, true);
                File.WriteAllText(_filePath, json);
                
                Debug.Log($"[GameRecordManager] 游戏记录已导出到: {_filePath}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameRecordManager] 导出记录失败: {e.Message}");
                return false;
            }
        }
        
        #endregion  
      
        #region 私有方法 - 初始化和事件
        
        /// <summary>
        /// 初始化记录管理器
        /// </summary>
        private void _initializeRecordManager()
        {
            // 设置保存路径
            _m_fullRecordPath = Path.Combine(Application.persistentDataPath, _m_recordSavePath);
            
            // 确保目录存在
            if (!Directory.Exists(_m_fullRecordPath))
            {
                Directory.CreateDirectory(_m_fullRecordPath);
            }
            
            _m_gameRecords = new List<GameRecord>();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[GameRecordManager] 记录管理器初始化完成，保存路径: {_m_fullRecordPath}");
            }
        }
        
        /// <summary>
        /// 订阅游戏事件
        /// </summary>
        private void _subscribeToEvents()
        {
            if (GameEvents.instance != null)
            {
                GameEvents.onGameStarted += _onGameStarted;
                GameEvents.onGameEnded += _onGameEnded;
                GameEvents.onTurnStarted += _onTurnStarted;
                GameEvents.onTurnEnded += _onTurnEnded;
                GameEvents.onPiecePlaced += _onPiecePlaced;
                GameEvents.onPlayerSkipped += _onPlayerSkipped;
            }
        }
        
        /// <summary>
        /// 取消订阅游戏事件
        /// </summary>
        private void _unsubscribeFromEvents()
        {
            if (GameEvents.instance != null)
            {
                GameEvents.onGameStarted -= _onGameStarted;
                GameEvents.onGameEnded -= _onGameEnded;
                GameEvents.onTurnStarted -= _onTurnStarted;
                GameEvents.onTurnEnded -= _onTurnEnded;
                GameEvents.onPiecePlaced -= _onPiecePlaced;
                GameEvents.onPlayerSkipped -= _onPlayerSkipped;
            }
        }
        
        #endregion
        
        #region 私有方法 - 数据持久化
        
        /// <summary>
        /// 保存游戏记录到文件
        /// </summary>
        /// <returns>是否保存成功</returns>
        private bool _saveGameRecords()
        {
            try
            {
                var recordsData = new GameRecordsData
                {
                    records = _m_gameRecords,
                    exportTime = System.DateTime.Now,
                    totalRecords = _m_gameRecords.Count
                };
                
                string json = JsonUtility.ToJson(recordsData, true);
                string filePath = Path.Combine(_m_fullRecordPath, "game_records.json");
                
                File.WriteAllText(filePath, json);
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[GameRecordManager] 游戏记录已保存，共{_m_gameRecords.Count}条记录");
                }
                
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameRecordManager] 保存游戏记录失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 从文件加载游戏记录
        /// </summary>
        /// <returns>是否加载成功</returns>
        private bool _loadGameRecords()
        {
            try
            {
                string filePath = Path.Combine(_m_fullRecordPath, "game_records.json");
                
                if (!File.Exists(filePath))
                {
                    if (_m_enableDetailedLogging)
                    {
                        Debug.Log("[GameRecordManager] 游戏记录文件不存在，创建新的记录列表");
                    }
                    return true;
                }
                
                string json = File.ReadAllText(filePath);
                var recordsData = JsonUtility.FromJson<GameRecordsData>(json);
                
                if (recordsData != null && recordsData.records != null)
                {
                    _m_gameRecords = recordsData.records;
                    
                    if (_m_enableDetailedLogging)
                    {
                        Debug.Log($"[GameRecordManager] 游戏记录已加载，共{_m_gameRecords.Count}条记录");
                    }
                }
                
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameRecordManager] 加载游戏记录失败: {e.Message}");
                _m_gameRecords = new List<GameRecord>();
                return false;
            }
        }
        
        /// <summary>
        /// 限制记录数量
        /// </summary>
        private void _limitRecordsCount()
        {
            if (_m_gameRecords.Count > _m_maxRecordsToKeep)
            {
                // 按时间排序，保留最新的记录
                _m_gameRecords = _m_gameRecords
                    .OrderByDescending(r => r.gameStartTime)
                    .Take(_m_maxRecordsToKeep)
                    .ToList();
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[GameRecordManager] 记录数量已限制为{_m_maxRecordsToKeep}条");
                }
            }
        }
        
        #endregion
        
        #region 私有方法 - 统计更新
        
        /// <summary>
        /// 更新玩家回合统计
        /// </summary>
        /// <param name="_turnRecord">回合记录</param>
        private void _updatePlayerTurnStats(TurnRecord _turnRecord)
        {
            if (_m_currentGameRecord == null) return;
            
            var playerRecord = _m_currentGameRecord.getPlayerRecord(_turnRecord.playerId);
            if (playerRecord != null)
            {
                bool successful = _turnRecord.endReason == TurnEndReason.PiecePlaced;
                playerRecord.turnStats.updateStats(_turnRecord.turnDuration, successful);
            }
        }
        
        #endregion
        
        #region 私有方法 - 事件处理
        
        /// <summary>
        /// 游戏开始事件处理
        /// </summary>
        /// <param name="_playerCount">玩家数量</param>
        private void _onGameStarted(int _playerCount)
        {
            // 游戏开始时，GameManager会调用startNewGameRecord
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[GameRecordManager] 游戏开始事件，玩家数量: {_playerCount}");
            }
        }
        
        /// <summary>
        /// 游戏结束事件处理
        /// </summary>
        /// <param name="_finalScores">最终分数</param>
        private void _onGameEnded(Dictionary<int, int> _finalScores)
        {
            // 结束当前回合记录（如果有）
            if (_m_currentTurnRecord != null)
            {
                endTurnRecord(TurnEndReason.GameEnded, -1, Vector2Int.one * -1); 
            }
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[GameRecordManager] 游戏结束事件");
            }
        }
        
        /// <summary>
        /// 回合开始事件处理
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_turnNumber">回合数</param>
        private void _onTurnStarted(int _playerId, int _turnNumber)
        {
            startTurnRecord(_turnNumber, _playerId);
        }
        
        /// <summary>
        /// 回合结束事件处理
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_turnNumber">回合数</param>
        private void _onTurnEnded(int _playerId, int _turnNumber)
        {
            // 回合结束的具体原因会在其他事件中处理
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[GameRecordManager] 回合结束事件: 玩家{_playerId}, 回合{_turnNumber}");
            }
        }
        
        /// <summary>
        /// 方块放置事件处理
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_piece">放置的方块</param>
        /// <param name="_position">放置位置</param>
        private void _onPiecePlaced(int _playerId, BlokusGame.Core.Interfaces._IGamePiece _piece, Vector2Int _position)
        {
            if (_m_currentTurnRecord != null && _m_currentTurnRecord.playerId == _playerId)
            {
                endTurnRecord(TurnEndReason.PiecePlaced, _piece.pieceId, _position);
            }
        }
        
        /// <summary>
        /// 玩家跳过事件处理
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        private void _onPlayerSkipped(int _playerId)
        {
            if (_m_currentTurnRecord != null && _m_currentTurnRecord.playerId == _playerId)
            {
                endTurnRecord(TurnEndReason.PlayerSkipped, -1, Vector2Int.one * -1); 
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 游戏记录数据容器类
    /// </summary>
    [System.Serializable]
    public class GameRecordsData
    {
        /// <summary>游戏记录列表</summary>
        public List<GameRecord> records;
        
        /// <summary>导出时间</summary>
        public System.DateTime exportTime;
        
        /// <summary>记录总数</summary>
        public int totalRecords;
    }
    
    /// <summary>
    /// 游戏统计数据类
    /// </summary>
    [System.Serializable]
    public class GameStatistics
    {
        /// <summary>总游戏数</summary>
        public int totalGamesPlayed;
        
        /// <summary>总游戏时间（秒）</summary>
        public float totalGameTime;
        
        /// <summary>平均游戏时间（秒）</summary>
        public float averageGameTime;
        
        /// <summary>总回合数</summary>
        public int totalTurnsPlayed;
        
        /// <summary>平均每局回合数</summary>
        public float averageTurnsPerGame;
        
        /// <summary>总放置方块数</summary>
        public int totalPiecesPlaced;
        
        /// <summary>平均每局方块数</summary>
        public float averagePiecesPerGame;
        
        /// <summary>历史最高分</summary>
        public int highestScore;
        
        /// <summary>历史最低分</summary>
        public int lowestScore;
        
        /// <summary>按游戏模式统计</summary>
        public Dictionary<GameMode, int> gamesByMode;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public GameStatistics()
        {
            gamesByMode = new Dictionary<GameMode, int>();
        }
    }
}