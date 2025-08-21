using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Events;
using BlokusGame.Core.Data;

namespace BlokusGame.Core.Scoring
{
    /// <summary>
    /// 计分系统 - 负责Blokus游戏的计分逻辑和统计
    /// 实现标准Blokus计分规则：每个方块格子1分，剩余方块扣分
    /// 提供实时分数计算、排名统计和游戏结果分析功能
    /// </summary>
    public class ScoreSystem : MonoBehaviour
    {
        [Header("计分配置")]
        /// <summary>每个放置方块格子的得分</summary>
        [SerializeField] private int _m_pointsPerSquare = 1;
        
        /// <summary>剩余方块的扣分倍率</summary>
        [SerializeField] private float _m_remainingPiecePenalty = 1f;
        
        /// <summary>完成所有方块的奖励分数</summary>
        [SerializeField] private int _m_completionBonus = 15;
        
        /// <summary>最后一个方块是单格方块的额外奖励</summary>
        [SerializeField] private int _m_singleSquareBonus = 5;
        
        /// <summary>是否启用详细日志</summary>
        [SerializeField] private bool _m_enableDetailedLogging = false;
        
        // 私有字段
        /// <summary>玩家分数字典</summary>
        private Dictionary<int, PlayerScore> _m_playerScores = new Dictionary<int, PlayerScore>();
        
        /// <summary>游戏结果数据</summary>
        private GameResults _m_gameResults;
        
        /// <summary>是否正在计算分数</summary>
        private bool _m_isCalculatingScores = false;
        
        /// <summary>单例实例</summary>
        public static ScoreSystem instance { get; private set; }
        
        /// <summary>
        /// 玩家分数数据结构
        /// </summary>
        [System.Serializable]
        public class PlayerScore
        {
            /// <summary>玩家ID</summary>
            public int playerId;
            
            /// <summary>玩家名称</summary>
            public string playerName;
            
            /// <summary>已放置方块的总分数</summary>
            public int placedPiecesScore;
            
            /// <summary>剩余方块的扣分</summary>
            public int remainingPiecesPenalty;
            
            /// <summary>奖励分数</summary>
            public int bonusScore;
            
            /// <summary>最终总分</summary>
            public int totalScore;
            
            /// <summary>排名</summary>
            public int rank;
            
            /// <summary>已放置的方块数量</summary>
            public int placedPiecesCount;
            
            /// <summary>剩余方块数量</summary>
            public int remainingPiecesCount;
            
            /// <summary>是否完成所有方块</summary>
            public bool completedAllPieces;
            
            /// <summary>最后放置的方块是否为单格方块</summary>
            public bool lastPieceWasSingle;
            
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="_playerId">玩家ID</param>
            /// <param name="_playerName">玩家名称</param>
            public PlayerScore(int _playerId, string _playerName)
            {
                playerId = _playerId;
                playerName = _playerName;
                placedPiecesScore = 0;
                remainingPiecesPenalty = 0;
                bonusScore = 0;
                totalScore = 0;
                rank = 0;
                placedPiecesCount = 0;
                remainingPiecesCount = 0;
                completedAllPieces = false;
                lastPieceWasSingle = false;
            }
        }
        
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
                _initializeScoreSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Unity Start方法 - 订阅事件
        /// </summary>
        private void Start()
        {
            _subscribeToEvents();
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 清理资源
        /// </summary>
        private void OnDestroy()
        {
            _unsubscribeFromEvents();
            
            if (instance == this)
            {
                instance = null;
            }
        }
        
        #endregion      
  
        #region 公共方法
        
        /// <summary>
        /// 初始化玩家分数
        /// </summary>
        /// <param name="_playerIds">玩家ID列表</param>
        /// <param name="_playerNames">玩家名称列表</param>
        public void initializePlayerScores(List<int> _playerIds, List<string> _playerNames)
        {
            if (_playerIds == null || _playerNames == null || _playerIds.Count != _playerNames.Count)
            {
                Debug.LogError("[ScoreSystem] 玩家ID和名称列表不匹配");
                return;
            }
            
            _m_playerScores.Clear();
            
            for (int i = 0; i < _playerIds.Count; i++)
            {
                int playerId = _playerIds[i];
                string playerName = _playerNames[i];
                
                _m_playerScores[playerId] = new PlayerScore(playerId, playerName);
            }
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[ScoreSystem] 初始化了 {_m_playerScores.Count} 个玩家的分数");
            }
        }
        
        /// <summary>
        /// 计算指定玩家的当前分数
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_player">玩家实例</param>
        /// <returns>当前分数</returns>
        public int calculatePlayerScore(int _playerId, _IPlayer _player)
        {
            if (_player == null)
            {
                Debug.LogError($"[ScoreSystem] 玩家 {_playerId} 实例为空");
                return 0;
            }
            
            if (!_m_playerScores.ContainsKey(_playerId))
            {
                Debug.LogError($"[ScoreSystem] 玩家 {_playerId} 未初始化");
                return 0;
            }
            
            PlayerScore playerScore = _m_playerScores[_playerId];
            
            // 计算已放置方块的分数
            int placedScore = _calculatePlacedPiecesScore(_player);
            
            // 计算剩余方块的扣分
            int remainingPenalty = _calculateRemainingPiecesPenalty(_player);
            
            // 更新玩家分数数据
            playerScore.placedPiecesScore = placedScore;
            playerScore.remainingPiecesPenalty = remainingPenalty;
            playerScore.totalScore = placedScore - remainingPenalty;
            
            // 更新统计数据
            _updatePlayerStatistics(playerScore, _player);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[ScoreSystem] 玩家 {_playerId} 分数: 放置={placedScore}, 扣分={remainingPenalty}, 总分={playerScore.totalScore}");
            }
            
            return playerScore.totalScore;
        }
        
        /// <summary>
        /// 计算游戏结束时的最终分数和排名
        /// </summary>
        /// <param name="_players">所有玩家列表</param>
        /// <returns>游戏结果</returns>
        public GameResults calculateFinalScores(List<_IPlayer> _players)
        {
            if (_players == null || _players.Count == 0)
            {
                Debug.LogError("[ScoreSystem] 玩家列表为空");
                return null;
            }
            
            _m_isCalculatingScores = true;
            
            // 计算每个玩家的最终分数
            foreach (var player in _players)
            {
                if (player != null)
                {
                    calculatePlayerScore(player.playerId, player);
                    _calculateBonusScores(player.playerId, player);
                }
            }
            
            // 计算排名
            _calculateRankings();
            
            // 创建游戏结果
            _m_gameResults = _createGameResults();
            
            _m_isCalculatingScores = false;
            
            // 触发分数计算完成事件
            GameEvents.instance.onScoreCalculated?.Invoke(_m_gameResults);
            
            if (_m_enableDetailedLogging)
            {
                _logFinalResults();
            }
            
            return _m_gameResults;
        }
        
        /// <summary>
        /// 获取指定玩家的分数信息
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>玩家分数信息</returns>
        public PlayerScore getPlayerScore(int _playerId)
        {
            if (_m_playerScores.ContainsKey(_playerId))
            {
                return _m_playerScores[_playerId];
            }
            
            Debug.LogWarning($"[ScoreSystem] 玩家 {_playerId} 分数信息不存在");
            return null;
        }
        
        /// <summary>
        /// 获取所有玩家的分数信息
        /// </summary>
        /// <returns>所有玩家分数信息</returns>
        public List<PlayerScore> getAllPlayerScores()
        {
            return _m_playerScores.Values.ToList();
        }
        
        /// <summary>
        /// 获取排名列表
        /// </summary>
        /// <returns>按排名排序的玩家分数列表</returns>
        public List<PlayerScore> getRankings()
        {
            return _m_playerScores.Values.OrderBy(p => p.rank).ToList();
        }
        
        /// <summary>
        /// 获取获胜者
        /// </summary>
        /// <returns>获胜者的玩家分数信息</returns>
        public PlayerScore getWinner()
        {
            return _m_playerScores.Values.Where(p => p.rank == 1).FirstOrDefault();
        }
        
        /// <summary>
        /// 获取玩家分数字典（兼容性方法）
        /// </summary>
        /// <returns>玩家分数字典</returns>
        public Dictionary<int, int> playerScores()
        {
            var scores = new Dictionary<int, int>();
            foreach (var playerScore in _m_playerScores.Values)
            {
                scores[playerScore.playerId] = playerScore.totalScore;
            }
            return scores;
        }
        
        /// <summary>
        /// 获取玩家排名字典（兼容性方法）
        /// </summary>
        /// <returns>玩家排名字典</returns>
        public Dictionary<int, int> playerRankings()
        {
            var rankings = new Dictionary<int, int>();
            foreach (var playerScore in _m_playerScores.Values)
            {
                rankings[playerScore.playerId] = playerScore.rank;
            }
            return rankings;
        }
        
        /// <summary>
        /// 重置分数系统
        /// </summary>
        public void resetScores()
        {
            _m_playerScores.Clear();
            _m_gameResults = null;
            _m_isCalculatingScores = false;
            
            Debug.Log("[ScoreSystem] 分数系统已重置");
        }
        
        #endregion        

        #region 私有方法 - 初始化和事件
        
        /// <summary>
        /// 初始化分数系统
        /// </summary>
        private void _initializeScoreSystem()
        {
            _m_playerScores = new Dictionary<int, PlayerScore>();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[ScoreSystem] 分数系统初始化完成");
            }
        }
        
        /// <summary>
        /// 订阅游戏事件
        /// </summary>
        private void _subscribeToEvents()
        {
            if (GameEvents.instance != null)
            {
                GameEvents.onPiecePlaced += _onPiecePlaced;
                GameEvents.onGameEnded += _onGameEnded;
            }
        }
        
        /// <summary>
        /// 取消订阅游戏事件
        /// </summary>
        private void _unsubscribeFromEvents()
        {
            if (GameEvents.instance != null)
            {
                GameEvents.onPiecePlaced -= _onPiecePlaced;
                GameEvents.onGameEnded -= _onGameEnded;
            }
        }
        
        #endregion
        
        #region 私有方法 - 分数计算
        
        /// <summary>
        /// 计算已放置方块的分数
        /// </summary>
        /// <param name="_player">玩家实例</param>
        /// <returns>已放置方块分数</returns>
        private int _calculatePlacedPiecesScore(_IPlayer _player)
        {
            int totalScore = 0;
            
            foreach (var piece in _player.availablePieces)
            {
                if (piece.isPlaced)
                {
                    // 每个方块格子得1分
                    int pieceSquares = piece.currentShape.Length;
                    totalScore += pieceSquares * _m_pointsPerSquare;
                }
            }
            
            return totalScore;
        }
        
        /// <summary>
        /// 计算剩余方块的扣分
        /// </summary>
        /// <param name="_player">玩家实例</param>
        /// <returns>剩余方块扣分</returns>
        private int _calculateRemainingPiecesPenalty(_IPlayer _player)
        {
            int totalPenalty = 0;
            
            foreach (var piece in _player.availablePieces)
            {
                if (!piece.isPlaced)
                {
                    // 剩余方块按格子数扣分
                    int pieceSquares = piece.currentShape.Length;
                    totalPenalty += Mathf.RoundToInt(pieceSquares * _m_remainingPiecePenalty);
                }
            }
            
            return totalPenalty;
        }
        
        /// <summary>
        /// 计算奖励分数
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_player">玩家实例</param>
        private void _calculateBonusScores(int _playerId, _IPlayer _player)
        {
            if (!_m_playerScores.ContainsKey(_playerId)) return;
            
            PlayerScore playerScore = _m_playerScores[_playerId];
            int bonusScore = 0;
            
            // 检查是否完成所有方块
            bool completedAllPieces = _player.availablePieces.All(p => p.isPlaced);
            playerScore.completedAllPieces = completedAllPieces;
            
            if (completedAllPieces)
            {
                bonusScore += _m_completionBonus;
                
                // 检查最后一个方块是否为单格方块
                var lastPiece = _getLastPlacedPiece(_player);
                if (lastPiece != null && lastPiece.currentShape.Length == 1)
                {
                    bonusScore += _m_singleSquareBonus;
                    playerScore.lastPieceWasSingle = true;
                }
            }
            
            playerScore.bonusScore = bonusScore;
            playerScore.totalScore += bonusScore;
        }
        
        /// <summary>
        /// 获取最后放置的方块
        /// </summary>
        /// <param name="_player">玩家实例</param>
        /// <returns>最后放置的方块</returns>
        private _IGamePiece _getLastPlacedPiece(_IPlayer _player)
        {
            // 这里需要根据实际的方块放置时间戳来确定
            // 暂时返回第一个单格方块作为示例
            return _player.availablePieces.FirstOrDefault(p => p.isPlaced && p.currentShape.Length == 1);
        }
        
        /// <summary>
        /// 更新玩家统计数据
        /// </summary>
        /// <param name="_playerScore">玩家分数对象</param>
        /// <param name="_player">玩家实例</param>
        private void _updatePlayerStatistics(PlayerScore _playerScore, _IPlayer _player)
        {
            _playerScore.placedPiecesCount = _player.availablePieces.Count(p => p.isPlaced);
            _playerScore.remainingPiecesCount = _player.availablePieces.Count(p => !p.isPlaced);
        }
        
        /// <summary>
        /// 计算排名
        /// </summary>
        private void _calculateRankings()
        {
            var sortedScores = _m_playerScores.Values.OrderByDescending(p => p.totalScore).ToList();
            
            for (int i = 0; i < sortedScores.Count; i++)
            {
                sortedScores[i].rank = i + 1;
            }
        }
        
        /// <summary>
        /// 创建游戏结果对象
        /// </summary>
        /// <returns>游戏结果</returns>
        private GameResults _createGameResults()
        {
            var results = new GameResults();
            results.playerScores = new Dictionary<int, int>();
            results.playerRankings = new Dictionary<int, int>();
            
            foreach (var playerScore in _m_playerScores.Values)
            {
                results.playerScores[playerScore.playerId] = playerScore.totalScore;
                results.playerRankings[playerScore.playerId] = playerScore.rank;
            }
            
            var winner = getWinner();
            results.winnerId = winner?.playerId ?? 0;
            results.gameEndTime = System.DateTime.Now;
            
            return results;
        }
        
        #endregion   
     
        #region 私有方法 - 事件处理
        
        /// <summary>
        /// 方块放置事件处理
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_piece">放置的方块</param>
        /// <param name="_position">放置位置</param>
        private void _onPiecePlaced(int _playerId, _IGamePiece _piece, Vector2Int _position)
        {
            // 实时更新玩家分数
            if (_m_playerScores.ContainsKey(_playerId))
            {
                // 这里可以触发分数更新事件
                GameEvents.instance.onPlayerScoreUpdated?.Invoke(_playerId, _m_playerScores[_playerId].totalScore);
            }
        }
        
        /// <summary>
        /// 游戏结束事件处理
        /// </summary>
        /// <param name="_finalScores">最终分数</param>
        private void _onGameEnded(Dictionary<int, int> _finalScores)
        {
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[ScoreSystem] 游戏结束，最终分数已计算");
            }
        }
        
        #endregion
        
        #region 私有方法 - 调试和日志
        
        /// <summary>
        /// 记录最终结果
        /// </summary>
        private void _logFinalResults()
        {
            Debug.Log("=== 最终游戏结果 ===");
            
            var rankings = getRankings();
            foreach (var playerScore in rankings)
            {
                Debug.Log($"排名 {playerScore.rank}: {playerScore.playerName} (ID: {playerScore.playerId})");
                Debug.Log($"  放置方块分数: {playerScore.placedPiecesScore}");
                Debug.Log($"  剩余方块扣分: {playerScore.remainingPiecesPenalty}");
                Debug.Log($"  奖励分数: {playerScore.bonusScore}");
                Debug.Log($"  最终总分: {playerScore.totalScore}");
                Debug.Log($"  完成所有方块: {playerScore.completedAllPieces}");
                Debug.Log($"  最后方块为单格: {playerScore.lastPieceWasSingle}");
                Debug.Log("---");
            }
            
            var winner = getWinner();
            if (winner != null)
            {
                Debug.Log($"🏆 获胜者: {winner.playerName} (总分: {winner.totalScore})");
            }
        }
        
        #endregion
    }
}