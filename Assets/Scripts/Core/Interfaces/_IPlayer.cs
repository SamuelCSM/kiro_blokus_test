using System.Collections.Generic;
using UnityEngine;

namespace BlokusGame.Core.Interfaces
{
    /// <summary>
    /// 玩家接口 - 定义Blokus游戏中玩家的基本行为
    /// 管理玩家的方块库存、分数计算和游戏状态
    /// </summary>
    public interface _IPlayer
    {
        /// <summary>玩家的唯一标识符</summary>
        int playerId { get; }
        
        /// <summary>玩家名称</summary>
        string playerName { get; }
        
        /// <summary>玩家颜色</summary>
        Color playerColor { get; }
        
        /// <summary>玩家当前分数</summary>
        int currentScore { get; }
        
        /// <summary>玩家是否还在游戏中（未被淘汰）</summary>
        bool isActive { get; }
        
        /// <summary>玩家可用的方块列表</summary>
        List<_IGamePiece> availablePieces { get; }
        
        /// <summary>玩家已使用的方块列表</summary>
        List<_IGamePiece> usedPieces { get; }
        
        /// <summary>
        /// 初始化玩家数据
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_playerName">玩家名称</param>
        /// <param name="_playerColor">玩家颜色</param>
        void initializePlayer(int _playerId, string _playerName, Color _playerColor);
        
        /// <summary>
        /// 使用指定的方块
        /// </summary>
        /// <param name="_piece">要使用的方块</param>
        /// <returns>是否使用成功</returns>
        bool usePiece(_IGamePiece _piece);
        
        /// <summary>
        /// 检查玩家是否拥有指定方块
        /// </summary>
        /// <param name="_pieceId">方块ID</param>
        /// <returns>是否拥有该方块</returns>
        bool hasPiece(int _pieceId);
        
        /// <summary>
        /// 获取指定ID的方块
        /// </summary>
        /// <param name="_pieceId">方块ID</param>
        /// <returns>方块实例，如果不存在返回null</returns>
        _IGamePiece getPiece(int _pieceId);
        
        /// <summary>
        /// 计算玩家当前分数
        /// 分数 = 已使用方块的总格数
        /// </summary>
        /// <returns>当前分数</returns>
        int calculateScore();
        
        /// <summary>
        /// 检查玩家是否还能继续游戏
        /// </summary>
        /// <param name="_gameBoard">游戏棋盘引用</param>
        /// <returns>是否还能继续游戏</returns>
        bool canContinueGame(_IGameBoard _gameBoard);
        
        /// <summary>
        /// 设置玩家的活跃状态
        /// </summary>
        /// <param name="_active">是否活跃</param>
        void setActiveState(bool _active);
        
        /// <summary>
        /// 重置玩家状态到游戏开始时
        /// </summary>
        void resetPlayer();
    }
}