using UnityEngine;
using System.Collections.Generic;

namespace BlokusGame.Core.Data
{
    /// <summary>
    /// 游戏结果数据类
    /// 存储游戏结束时的所有结果信息
    /// </summary>
    [System.Serializable]
    public class GameResults
    {
        /// <summary>获胜者玩家ID，-1表示平局</summary>
        public int winnerId = -1;
        
        /// <summary>游戏时长（秒）</summary>
        public float gameDuration = 0f;
        
        /// <summary>游戏开始时间</summary>
        public System.DateTime gameStartTime;
        
        /// <summary>游戏结束时间</summary>
        public System.DateTime gameEndTime;
        
        /// <summary>游戏模式</summary>
        public GameMode gameMode = GameMode.SinglePlayerVsAI;
        
        /// <summary>总回合数</summary>
        public int totalTurns = 0;
        
        /// <summary>玩家结果列表</summary>
        public List<PlayerResult> playerResults = new List<PlayerResult>();
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public GameResults()
        {
            gameStartTime = System.DateTime.Now;
            gameEndTime = System.DateTime.Now;
        }
        
        /// <summary>
        /// 添加玩家结果
        /// </summary>
        /// <param name="_playerResult">玩家结果</param>
        public void AddPlayerResult(PlayerResult _playerResult)
        {
            if (_playerResult != null)
            {
                playerResults.Add(_playerResult);
            }
        }
        
        /// <summary>
        /// 获取指定玩家的结果
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>玩家结果</returns>
        public PlayerResult GetPlayerResult(int _playerId)
        {
            return playerResults.Find(p => p.playerId == _playerId);
        }
        
        /// <summary>
        /// 计算游戏时长
        /// </summary>
        public void CalculateGameDuration()
        {
            gameDuration = (float)(gameEndTime - gameStartTime).TotalSeconds;
        }
        
        /// <summary>
        /// 确定获胜者
        /// </summary>
        public void DetermineWinner()
        {
            if (playerResults.Count == 0)
            {
                winnerId = -1;
                return;
            }
            
            // 按分数排序
            playerResults.Sort((a, b) => b.finalScore.CompareTo(a.finalScore));
            
            // 检查是否有平局
            int highestScore = playerResults[0].finalScore;
            var winners = playerResults.FindAll(p => p.finalScore == highestScore);
            
            if (winners.Count > 1)
            {
                winnerId = -1; // 平局
            }
            else
            {
                winnerId = winners[0].playerId;
            }
        }
        
        /// <summary>
        /// 转换为JSON字符串
        /// </summary>
        /// <returns>JSON字符串</returns>
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }
        
        /// <summary>
        /// 从JSON字符串创建游戏结果
        /// </summary>
        /// <param name="_json">JSON字符串</param>
        /// <returns>游戏结果对象</returns>
        public static GameResults FromJson(string _json)
        {
            if (string.IsNullOrEmpty(_json))
                return new GameResults();
            
            try
            {
                return JsonUtility.FromJson<GameResults>(_json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameResults] 解析JSON失败: {ex.Message}");
                return new GameResults();
            }
        }
    }
    
    /// <summary>
    /// 玩家结果数据类
    /// 存储单个玩家的游戏结果信息
    /// </summary>
    [System.Serializable]
    public class PlayerResult
    {
        /// <summary>玩家ID</summary>
        public int playerId = 0;
        
        /// <summary>玩家名称</summary>
        public string playerName = "";
        
        /// <summary>玩家颜色</summary>
        public Color playerColor = Color.white;
        
        /// <summary>最终分数</summary>
        public int finalScore = 0;
        
        /// <summary>已放置的方块数量</summary>
        public int placedPieces = 0;
        
        /// <summary>剩余方块数量</summary>
        public int remainingPieces = 21;
        
        /// <summary>已放置的总格子数</summary>
        public int placedBlocks = 0;
        
        /// <summary>剩余的总格子数</summary>
        public int remainingBlocks = 89; // 21个方块的总格子数
        
        /// <summary>游戏排名（1表示第一名）</summary>
        public int rank = 1;
        
        /// <summary>是否为AI玩家</summary>
        public bool isAI = false;
        
        /// <summary>玩家总游戏时间（秒）</summary>
        public float totalPlayTime = 0f;
        
        /// <summary>平均回合时间（秒）</summary>
        public float averageTurnTime = 0f;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public PlayerResult()
        {
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_playerName">玩家名称</param>
        /// <param name="_playerColor">玩家颜色</param>
        public PlayerResult(int _playerId, string _playerName, Color _playerColor)
        {
            playerId = _playerId;
            playerName = _playerName;
            playerColor = _playerColor;
        }
        
        /// <summary>
        /// 计算最终分数
        /// 根据Blokus规则：已放置格子数 - 剩余格子数
        /// </summary>
        public void CalculateFinalScore()
        {
            finalScore = placedBlocks - remainingBlocks;
        }
        
        /// <summary>
        /// 计算平均回合时间
        /// </summary>
        /// <param name="_totalTurns">总回合数</param>
        public void CalculateAverageTurnTime(int _totalTurns)
        {
            if (_totalTurns > 0)
            {
                averageTurnTime = totalPlayTime / _totalTurns;
            }
        }
        
        /// <summary>
        /// 获取结果摘要字符串
        /// </summary>
        /// <returns>结果摘要</returns>
        public string GetSummary()
        {
            return $"{playerName}: {finalScore}分 (已放置: {placedPieces}个方块, 剩余: {remainingPieces}个方块)";
        }
        
        /// <summary>
        /// 获取详细统计字符串
        /// </summary>
        /// <returns>详细统计</returns>
        public string GetDetailedStats()
        {
            return $"{playerName} - 排名: {rank}, 分数: {finalScore}, " +
                   $"已放置方块: {placedPieces}/{placedPieces + remainingPieces}, " +
                   $"已放置格子: {placedBlocks}, 剩余格子: {remainingBlocks}, " +
                   $"平均回合时间: {averageTurnTime:F1}秒";
        }
    }
}