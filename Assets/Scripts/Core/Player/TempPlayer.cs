using System.Collections.Generic;
using UnityEngine;
using BlokusGame.Core.Interfaces;

namespace BlokusGame.Core.Player
{
    /// <summary>
    /// 临时玩家类 - 实现_IPlayer接口的占位类
    /// 后续将实现完整的玩家功能
    /// </summary>
    public class TempPlayer : _IPlayer
    {
        /// <summary>玩家的唯一标识符</summary>
        public int playerId { get; private set; }
        
        /// <summary>玩家名称</summary>
        public string playerName { get; private set; }
        
        /// <summary>玩家颜色</summary>
        public Color playerColor { get; private set; }
        
        /// <summary>玩家当前分数</summary>
        public int currentScore { get; private set; }
        
        /// <summary>玩家是否还在游戏中（未被淘汰）</summary>
        public bool isActive { get; private set; } = true;
        
        /// <summary>玩家可用的方块列表</summary>
        public List<_IGamePiece> availablePieces { get; private set; }
        
        /// <summary>玩家已使用的方块列表</summary>
        public List<_IGamePiece> usedPieces { get; private set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public TempPlayer()
        {
            availablePieces = new List<_IGamePiece>();
            usedPieces = new List<_IGamePiece>();
        }
        
        /// <summary>
        /// 初始化玩家数据
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_playerName">玩家名称</param>
        /// <param name="_playerColor">玩家颜色</param>
        public void initializePlayer(int _playerId, string _playerName, Color _playerColor)
        {
            playerId = _playerId;
            playerName = _playerName;
            playerColor = _playerColor;
            currentScore = 0;
            isActive = true;
            
            Debug.Log($"[TempPlayer] 初始化玩家 {_playerId}: {_playerName} - 临时实现");
        }
        
        /// <summary>
        /// 使用指定的方块
        /// </summary>
        /// <param name="_piece">要使用的方块</param>
        /// <returns>是否使用成功</returns>
        public bool usePiece(_IGamePiece _piece)
        {
            Debug.Log($"[TempPlayer] 玩家 {playerId} 使用方块 - 临时实现，返回true");
            return true; // 临时返回true
        }
        
        /// <summary>
        /// 检查玩家是否拥有指定方块
        /// </summary>
        /// <param name="_pieceId">方块ID</param>
        /// <returns>是否拥有该方块</returns>
        public bool hasPiece(int _pieceId)
        {
            Debug.Log($"[TempPlayer] 检查玩家 {playerId} 是否拥有方块 {_pieceId} - 临时实现，返回true");
            return true; // 临时返回true
        }
        
        /// <summary>
        /// 获取指定ID的方块
        /// </summary>
        /// <param name="_pieceId">方块ID</param>
        /// <returns>方块实例，如果不存在返回null</returns>
        public _IGamePiece getPiece(int _pieceId)
        {
            Debug.Log($"[TempPlayer] 获取玩家 {playerId} 的方块 {_pieceId} - 临时实现，返回null");
            return null; // 临时返回null
        }
        
        /// <summary>
        /// 计算玩家当前分数
        /// 分数 = 已使用方块的总格数
        /// </summary>
        /// <returns>当前分数</returns>
        public int calculateScore()
        {
            Debug.Log($"[TempPlayer] 计算玩家 {playerId} 分数 - 临时实现，返回0");
            return 0; // 临时返回0
        }
        
        /// <summary>
        /// 检查玩家是否还能继续游戏
        /// </summary>
        /// <param name="_gameBoard">游戏棋盘引用</param>
        /// <returns>是否还能继续游戏</returns>
        public bool canContinueGame(_IGameBoard _gameBoard)
        {
            Debug.Log($"[TempPlayer] 检查玩家 {playerId} 是否能继续游戏 - 临时实现，返回true");
            return true; // 临时返回true
        }
        
        /// <summary>
        /// 设置玩家的活跃状态
        /// </summary>
        /// <param name="_active">是否活跃</param>
        public void setActiveState(bool _active)
        {
            isActive = _active;
            Debug.Log($"[TempPlayer] 设置玩家 {playerId} 活跃状态为 {_active}");
        }
        
        /// <summary>
        /// 重置玩家状态到游戏开始时
        /// </summary>
        public void resetPlayer()
        {
            currentScore = 0;
            isActive = true;
            availablePieces.Clear();
            usedPieces.Clear();
            Debug.Log($"[TempPlayer] 重置玩家 {playerId} 状态");
        }
    }
}