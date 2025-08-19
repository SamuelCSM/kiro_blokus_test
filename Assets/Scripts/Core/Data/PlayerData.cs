using UnityEngine;
using System.Collections.Generic;

namespace BlokusGame.Core.Data
{
    /// <summary>
    /// 玩家数据类
    /// 存储玩家的基本信息、游戏状态和统计数据
    /// 支持序列化以便保存和加载游戏进度
    /// </summary>
    [System.Serializable]
    public class PlayerData
    {
        [Header("基本信息")]
        /// <summary>玩家唯一标识符</summary>
        [SerializeField] private int _m_playerId;
        
        /// <summary>玩家显示名称</summary>
        [SerializeField] private string _m_playerName;
        
        /// <summary>玩家颜色</summary>
        [SerializeField] private Color _m_playerColor;
        
        /// <summary>玩家类型（人类/AI）</summary>
        [SerializeField] private PlayerType _m_playerType;
        
        [Header("游戏状态")]
        /// <summary>当前游戏状态</summary>
        [SerializeField] private PlayerGameState _m_gameState;
        
        /// <summary>当前分数</summary>
        [SerializeField] private int _m_currentScore;
        
        /// <summary>剩余方块数量</summary>
        [SerializeField] private int _m_remainingPieces;
        
        /// <summary>已放置方块数量</summary>
        [SerializeField] private int _m_placedPieces;
        
        [Header("统计数据")]
        /// <summary>总游戏时间（秒）</summary>
        [SerializeField] private float _m_totalGameTime;
        
        /// <summary>平均每回合时间（秒）</summary>
        [SerializeField] private float _m_averageTurnTime;
        
        /// <summary>跳过的回合数</summary>
        [SerializeField] private int _m_skippedTurns;
        
        /// <summary>最后活动时间</summary>
        [SerializeField] private float _m_lastActivityTime;
        
        /// <summary>玩家ID属性</summary>
        public int playerId => _m_playerId;
        
        /// <summary>玩家名称属性</summary>
        public string playerName => _m_playerName;
        
        /// <summary>玩家颜色属性</summary>
        public Color playerColor => _m_playerColor;
        
        /// <summary>玩家类型属性</summary>
        public PlayerType playerType => _m_playerType;
        
        /// <summary>游戏状态属性</summary>
        public PlayerGameState gameState => _m_gameState;
        
        /// <summary>当前分数属性</summary>
        public int currentScore => _m_currentScore;
        
        /// <summary>剩余方块数量属性</summary>
        public int remainingPieces => _m_remainingPieces;
        
        /// <summary>已放置方块数量属性</summary>
        public int placedPieces => _m_placedPieces;
        
        /// <summary>
        /// 构造函数
        /// 创建新的玩家数据实例
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_playerName">玩家名称</param>
        /// <param name="_playerColor">玩家颜色</param>
        /// <param name="_playerType">玩家类型</param>
        public PlayerData(int _playerId, string _playerName, Color _playerColor, PlayerType _playerType)
        {
            _m_playerId = _playerId;
            _m_playerName = _playerName;
            _m_playerColor = _playerColor;
            _m_playerType = _playerType;
            
            // 初始化游戏状态
            _m_gameState = PlayerGameState.Active;
            _m_currentScore = 0;
            _m_remainingPieces = 0;
            _m_placedPieces = 0;
            
            // 初始化统计数据
            _m_totalGameTime = 0f;
            _m_averageTurnTime = 0f;
            _m_skippedTurns = 0;
            _m_lastActivityTime = Time.time;
        }
        
        /// <summary>
        /// 更新游戏状态
        /// </summary>
        /// <param name="_newState">新的游戏状态</param>
        public void updateGameState(PlayerGameState _newState)
        {
            _m_gameState = _newState;
            _m_lastActivityTime = Time.time;
        }
        
        /// <summary>
        /// 更新分数
        /// </summary>
        /// <param name="_newScore">新分数</param>
        public void updateScore(int _newScore)
        {
            _m_currentScore = _newScore;
        }
        
        /// <summary>
        /// 更新方块统计
        /// </summary>
        /// <param name="_remainingCount">剩余方块数</param>
        /// <param name="_placedCount">已放置方块数</param>
        public void updatePieceStats(int _remainingCount, int _placedCount)
        {
            _m_remainingPieces = _remainingCount;
            _m_placedPieces = _placedCount;
        }
        
        /// <summary>
        /// 记录回合时间
        /// </summary>
        /// <param name="_turnTime">本回合用时</param>
        public void recordTurnTime(float _turnTime)
        {
            _m_totalGameTime += _turnTime;
            
            // 计算平均回合时间
            int totalTurns = _m_placedPieces + _m_skippedTurns;
            if (totalTurns > 0)
            {
                _m_averageTurnTime = _m_totalGameTime / totalTurns;
            }
        }
        
        /// <summary>
        /// 增加跳过回合计数
        /// </summary>
        public void incrementSkippedTurns()
        {
            _m_skippedTurns++;
            _m_lastActivityTime = Time.time;
        }
        
        /// <summary>
        /// 重置玩家数据到初始状态
        /// </summary>
        public void reset()
        {
            _m_gameState = PlayerGameState.Active;
            _m_currentScore = 0;
            _m_remainingPieces = 0;
            _m_placedPieces = 0;
            _m_totalGameTime = 0f;
            _m_averageTurnTime = 0f;
            _m_skippedTurns = 0;
            _m_lastActivityTime = Time.time;
        }
        
        /// <summary>
        /// 获取玩家活跃度（基于最后活动时间）
        /// </summary>
        /// <returns>距离最后活动的时间（秒）</returns>
        public float getInactiveTime()
        {
            return Time.time - _m_lastActivityTime;
        }
        
        /// <summary>
        /// 检查玩家是否活跃
        /// </summary>
        /// <param name="_timeoutSeconds">超时时间（秒）</param>
        /// <returns>是否活跃</returns>
        public bool isActive(float _timeoutSeconds = 300f)
        {
            return getInactiveTime() < _timeoutSeconds;
        }
        
        /// <summary>
        /// 获取玩家游戏进度百分比
        /// </summary>
        /// <param name="_totalPieces">总方块数</param>
        /// <returns>进度百分比 (0.0 - 1.0)</returns>
        public float getProgress(int _totalPieces)
        {
            if (_totalPieces <= 0) return 0f;
            return (float)_m_placedPieces / _totalPieces;
        }
        
        /// <summary>
        /// 转换为字符串表示
        /// </summary>
        /// <returns>玩家数据的字符串描述</returns>
        public override string ToString()
        {
            return $"Player {_m_playerId}: {_m_playerName} ({_m_playerType}) - Score: {_m_currentScore}, " +
                   $"Pieces: {_m_placedPieces}/{_m_remainingPieces + _m_placedPieces}, State: {_m_gameState}";
        }
    }
    
    /// <summary>
    /// 玩家类型枚举
    /// 定义不同类型的玩家
    /// </summary>
    public enum PlayerType
    {
        /// <summary>人类玩家</summary>
        Human,
        /// <summary>AI玩家</summary>
        AI,
        /// <summary>网络玩家</summary>
        Network,
        /// <summary>观察者</summary>
        Observer
    }
}