using System.Collections;
using UnityEngine;

namespace BlokusGame.Core.Interfaces
{
    /// <summary>
    /// AI玩家接口 - 定义AI玩家的特殊行为和决策能力
    /// 继承自_IPlayer接口，添加AI特有的决策和难度控制功能
    /// </summary>
    public interface _IAIPlayer : _IPlayer
    {
        /// <summary>AI难度等级枚举</summary>
        public enum AIDifficulty
        {
            /// <summary>简单 - 随机选择有效移动</summary>
            Easy,
            /// <summary>中等 - 基于启发式算法</summary>
            Medium,
            /// <summary>困难 - 使用前瞻搜索算法</summary>
            Hard
        }
        
        /// <summary>当前AI难度等级</summary>
        AIDifficulty difficulty { get; }
        
        /// <summary>AI思考时间（秒）</summary>
        float thinkingTime { get; }
        
        /// <summary>AI是否正在思考中</summary>
        bool isThinking { get; }
        
        /// <summary>
        /// 设置AI难度等级
        /// </summary>
        /// <param name="_difficulty">难度等级</param>
        void setDifficulty(AIDifficulty _difficulty);
        
        /// <summary>
        /// 设置AI思考时间
        /// </summary>
        /// <param name="_thinkingTime">思考时间（秒）</param>
        void setThinkingTime(float _thinkingTime);
        
        /// <summary>
        /// AI进行决策并执行移动
        /// 这是一个协程方法，会根据难度等级进行不同复杂度的计算
        /// </summary>
        /// <param name="_gameBoard">游戏棋盘引用</param>
        /// <returns>协程枚举器</returns>
        IEnumerator makeMove(_IGameBoard _gameBoard);
        
        /// <summary>
        /// 评估指定移动的价值
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <param name="_gameBoard">游戏棋盘引用</param>
        /// <returns>移动价值评分</returns>
        float evaluateMove(_IGamePiece _piece, Vector2Int _position, _IGameBoard _gameBoard);
        
        /// <summary>
        /// 获取AI推荐的最佳移动
        /// </summary>
        /// <param name="_gameBoard">游戏棋盘引用</param>
        /// <returns>最佳移动信息（方块和位置）</returns>
        (_IGamePiece piece, Vector2Int position) getBestMove(_IGameBoard _gameBoard);
        
        /// <summary>
        /// 停止AI思考过程
        /// </summary>
        void stopThinking();
    }
}