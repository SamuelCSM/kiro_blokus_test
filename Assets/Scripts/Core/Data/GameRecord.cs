using UnityEngine;
using System;
using System.Collections.Generic;
using BlokusGame.Core.Scoring;

namespace BlokusGame.Core.Data
{
    /// <summary>
    /// 游戏记录数据类 - 存储单局游戏的完整记录
    /// 包含游戏配置、玩家信息、游戏过程和最终结果
    /// 支持序列化保存和加载
    /// </summary>
    [System.Serializable]
    public class GameRecord
    {
        [Header("游戏基本信息")]
        /// <summary>游戏记录唯一ID</summary>
        public string gameId;
        
        /// <summary>游戏开始时间</summary>
        public DateTime gameStartTime;
        
        /// <summary>游戏结束时间</summary>
        public DateTime gameEndTime;
        
        /// <summary>游戏持续时间（秒）</summary>
        public float gameDuration;
        
        /// <summary>游戏模式</summary>
        public GameMode gameMode;
        
        /// <summary>玩家数量</summary>
        public int playerCount;
        
        [Header("玩家信息")]
        /// <summary>玩家记录列表</summary>
        public List<PlayerRecord> playerRecords;
        
        /// <summary>获胜者ID</summary>
        public int winnerId;
        
        /// <summary>获胜者名称</summary>
        public string winnerName;
        
        [Header("游戏统计")]
        /// <summary>总回合数</summary>
        public int totalTurns;
        
        /// <summary>总放置方块数</summary>
        public int totalPiecesPlaced;
        
        /// <summary>平均每回合时间（秒）</summary>
        public float averageTurnTime;
        
        /// <summary>最高分数</summary>
        public int highestScore;
        
        /// <summary>最低分数</summary>
        public int lowestScore;
        
        [Header("游戏过程记录")]
        /// <summary>回合记录列表</summary>
        public List<TurnRecord> turnRecords;
        
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public GameRecord()
        {
            gameId = System.Guid.NewGuid().ToString();
            gameStartTime = DateTime.Now;
            playerRecords = new List<PlayerRecord>();
            turnRecords = new List<TurnRecord>();
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_gameMode">游戏模式</param>
        /// <param name="_playerCount">玩家数量</param>
        public GameRecord(GameMode _gameMode, int _playerCount) : this()
        {
            gameMode = _gameMode;
            playerCount = _playerCount;
        }
        
        /// <summary>
        /// 完成游戏记录
        /// </summary>
        /// <param name="_gameResults">游戏结果</param>
        /// <param name="_playerScores">玩家分数列表</param>
        public void completeGameRecord(GameResults _gameResults, List<ScoreSystem.PlayerScore> _playerScores)
        {
            gameEndTime = DateTime.Now;
            gameDuration = (float)(gameEndTime - gameStartTime).TotalSeconds;
            
            if (_gameResults != null)
            {
                winnerId = _gameResults.winnerId;
            }
            
            // 更新玩家记录
            if (_playerScores != null)
            {
                foreach (var playerScore in _playerScores)
                {
                    var playerRecord = playerRecords.Find(p => p.playerId == playerScore.playerId);
                    if (playerRecord != null)
                    {
                        playerRecord.finalScore = playerScore.totalScore;
                        playerRecord.rank = playerScore.rank;
                        playerRecord.piecesPlaced = playerScore.placedPiecesCount;
                        playerRecord.piecesRemaining = playerScore.remainingPiecesCount;
                        playerRecord.completedAllPieces = playerScore.completedAllPieces;
                        
                        if (playerScore.rank == 1)
                        {
                            winnerName = playerScore.playerName;
                        }
                    }
                }
            }
            
            // 计算统计数据
            _calculateStatistics();
        }
        
        /// <summary>
        /// 添加玩家记录
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_playerName">玩家名称</param>
        /// <param name="_isAI">是否为AI玩家</param>
        public void addPlayerRecord(int _playerId, string _playerName, bool _isAI = false)
        {
            var playerRecord = new PlayerRecord
            {
                playerId = _playerId,
                playerName = _playerName,
                isAI = _isAI,
                gameStartTime = gameStartTime
            };
            
            playerRecords.Add(playerRecord);
        }
        
        /// <summary>
        /// 添加回合记录
        /// </summary>
        /// <param name="_turnRecord">回合记录</param>
        public void addTurnRecord(TurnRecord _turnRecord)
        {
            if (_turnRecord != null)
            {
                turnRecords.Add(_turnRecord);
                totalTurns = turnRecords.Count;
            }
        }
        
        /// <summary>
        /// 获取玩家记录
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>玩家记录</returns>
        public PlayerRecord getPlayerRecord(int _playerId)
        {
            return playerRecords.Find(p => p.playerId == _playerId);
        }
        
        /// <summary>
        /// 计算统计数据
        /// </summary>
        private void _calculateStatistics()
        {
            if (playerRecords.Count == 0) return;
            
            // 计算分数统计
            highestScore = int.MinValue;
            lowestScore = int.MaxValue;
            totalPiecesPlaced = 0;
            
            foreach (var playerRecord in playerRecords)
            {
                if (playerRecord.finalScore > highestScore)
                    highestScore = playerRecord.finalScore;
                
                if (playerRecord.finalScore < lowestScore)
                    lowestScore = playerRecord.finalScore;
                
                totalPiecesPlaced += playerRecord.piecesPlaced;
            }
            
            // 计算平均回合时间
            if (turnRecords.Count > 0)
            {
                float totalTurnTime = 0f;
                foreach (var turnRecord in turnRecords)
                {
                    totalTurnTime += turnRecord.turnDuration;
                }
                averageTurnTime = totalTurnTime / turnRecords.Count;
            }
        }
        
        /// <summary>
        /// 转换为JSON字符串
        /// </summary>
        /// <returns>JSON字符串</returns>
        public string toJson()
        {
            return JsonUtility.ToJson(this, true);
        }
        
        /// <summary>
        /// 从JSON字符串创建游戏记录
        /// </summary>
        /// <param name="_json">JSON字符串</param>
        /// <returns>游戏记录</returns>
        public static GameRecord fromJson(string _json)
        {
            try
            {
                return JsonUtility.FromJson<GameRecord>(_json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameRecord] JSON解析失败: {e.Message}");
                return null;
            }
        }
    }
    
    /// <summary>
    /// 玩家记录数据类
    /// </summary>
    [System.Serializable]
    public class PlayerRecord
    {
        /// <summary>玩家ID</summary>
        public int playerId;
        
        /// <summary>玩家名称</summary>
        public string playerName;
        
        /// <summary>是否为AI玩家</summary>
        public bool isAI;
        
        /// <summary>最终分数</summary>
        public int finalScore;
        
        /// <summary>排名</summary>
        public int rank;
        
        /// <summary>已放置方块数量</summary>
        public int piecesPlaced;
        
        /// <summary>剩余方块数量</summary>
        public int piecesRemaining;
        
        /// <summary>是否完成所有方块</summary>
        public bool completedAllPieces;
        
        /// <summary>总游戏时间（秒）</summary>
        public float totalGameTime;
        
        /// <summary>平均回合时间（秒）</summary>
        public float averageTurnTime;
        
        /// <summary>游戏开始时间</summary>
        public DateTime gameStartTime;
        
        /// <summary>回合统计</summary>
        public PlayerTurnStats turnStats;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public PlayerRecord()
        {
            turnStats = new PlayerTurnStats();
        }
    }
    
    /// <summary>
    /// 回合记录数据类
    /// </summary>
    [System.Serializable]
    public class TurnRecord
    {
        /// <summary>回合编号</summary>
        public int turnNumber;
        
        /// <summary>玩家ID</summary>
        public int playerId;
        
        /// <summary>回合开始时间</summary>
        public DateTime turnStartTime;
        
        /// <summary>回合结束时间</summary>
        public DateTime turnEndTime;
        
        /// <summary>回合持续时间（秒）</summary>
        public float turnDuration;
        
        /// <summary>放置的方块ID</summary>
        public int placedPieceId;
        
        /// <summary>放置位置</summary>
        public Vector2Int placedPosition;
        
        /// <summary>是否跳过回合</summary>
        public bool skippedTurn;
        
        /// <summary>回合结束原因</summary>
        public TurnEndReason endReason;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_turnNumber">回合编号</param>
        /// <param name="_playerId">玩家ID</param>
        public TurnRecord(int _turnNumber, int _playerId)
        {
            turnNumber = _turnNumber;
            playerId = _playerId;
            turnStartTime = DateTime.Now;
        }
        
        /// <summary>
        /// 结束回合
        /// </summary>
        /// <param name="_endReason">结束原因</param>
        public void endTurn(TurnEndReason _endReason)
        {
            turnEndTime = DateTime.Now;
            turnDuration = (float)(turnEndTime - turnStartTime).TotalSeconds;
            endReason = _endReason;
        }
    }
    
    /// <summary>
    /// 玩家回合统计数据
    /// </summary>
    [System.Serializable]
    public class PlayerTurnStats
    {
        /// <summary>总回合数</summary>
        public int totalTurns;
        
        /// <summary>成功放置回合数</summary>
        public int successfulTurns;
        
        /// <summary>跳过回合数</summary>
        public int skippedTurns;
        
        /// <summary>最快回合时间（秒）</summary>
        public float fastestTurnTime = float.MaxValue;
        
        /// <summary>最慢回合时间（秒）</summary>
        public float slowestTurnTime = 0f;
        
        /// <summary>
        /// 更新统计数据
        /// </summary>
        /// <param name="_turnDuration">回合时间</param>
        /// <param name="_successful">是否成功</param>
        public void updateStats(float _turnDuration, bool _successful)
        {
            totalTurns++;
            
            if (_successful)
            {
                successfulTurns++;
            }
            else
            {
                skippedTurns++;
            }
            
            if (_turnDuration < fastestTurnTime)
                fastestTurnTime = _turnDuration;
            
            if (_turnDuration > slowestTurnTime)
                slowestTurnTime = _turnDuration;
        }
    }
    
    /// <summary>
    /// 回合结束原因枚举
    /// </summary>
    public enum TurnEndReason
    {
        /// <summary>成功放置方块</summary>
        PiecePlaced,
        /// <summary>玩家跳过</summary>
        PlayerSkipped,
        /// <summary>无法放置</summary>
        NoValidMoves,
        /// <summary>时间超时</summary>
        TimeOut,
        /// <summary>游戏结束</summary>
        GameEnded
    }
}