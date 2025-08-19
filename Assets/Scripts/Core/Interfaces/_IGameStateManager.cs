using UnityEngine;
using BlokusGame.Core.Data;

namespace BlokusGame.Core.Interfaces
{
    /// <summary>
    /// 游戏状态管理器接口
    /// 定义游戏状态转换和回合管理的核心功能
    /// 负责协调游戏流程和状态同步
    /// </summary>
    public interface _IGameStateManager
    {
        /// <summary>当前游戏状态</summary>
        GameState currentState { get; }
        
        /// <summary>当前回合的玩家ID</summary>
        int currentPlayerId { get; }
        
        /// <summary>游戏回合数</summary>
        int turnNumber { get; }
        
        /// <summary>
        /// 开始新游戏
        /// 初始化游戏状态，重置所有玩家数据
        /// </summary>
        /// <param name="_playerCount">参与游戏的玩家数量</param>
        void startNewGame(int _playerCount);
        
        /// <summary>
        /// 切换到下一个玩家的回合
        /// 更新当前玩家ID，检查游戏结束条件
        /// </summary>
        void nextTurn();
        
        /// <summary>
        /// 跳过当前玩家的回合
        /// 当玩家无法放置任何方块时调用
        /// </summary>
        void skipCurrentPlayer();
        
        /// <summary>
        /// 暂停游戏
        /// 保存当前游戏状态，允许稍后恢复
        /// </summary>
        void pauseGame();
        
        /// <summary>
        /// 恢复游戏
        /// 从暂停状态恢复到正常游戏流程
        /// </summary>
        void resumeGame();
        
        /// <summary>
        /// 结束游戏
        /// 计算最终得分，触发游戏结束事件
        /// </summary>
        void endGame();
        
        /// <summary>
        /// 重置游戏
        /// 清除所有游戏数据，返回到初始状态
        /// </summary>
        void resetGame();
        
        /// <summary>
        /// 检查是否可以切换到下一回合
        /// 验证当前玩家是否完成了回合操作
        /// </summary>
        /// <returns>是否可以切换回合</returns>
        bool canAdvanceTurn();
        
        /// <summary>
        /// 获取游戏进度百分比
        /// 基于已放置方块数量计算游戏进度
        /// </summary>
        /// <returns>游戏进度 (0.0 - 1.0)</returns>
        float getGameProgress();
        
        /// <summary>
        /// 获取玩家的游戏状态
        /// 返回玩家是否还在游戏中、是否跳过等信息
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>玩家游戏状态</returns>
        PlayerGameState getPlayerState(int _playerId);
        
        /// <summary>
        /// 设置玩家的游戏状态
        /// 更新玩家的参与状态（活跃、跳过、退出等）
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_state">新的游戏状态</param>
        void setPlayerState(int _playerId, PlayerGameState _state);
    }
}