using UnityEngine;
using System.Collections.Generic;

namespace BlokusGame.Core.Interfaces
{
    /// <summary>
    /// 游戏棋盘接口 - 定义Blokus棋盘的核心功能
    /// 负责管理20x20棋盘的状态、验证放置规则和提供查询功能
    /// </summary>
    public interface _IGameBoard
    {
        /// <summary>棋盘尺寸（20x20）</summary>
        int boardSize { get; }
        
        /// <summary>
        /// 检查指定位置是否可以放置方块
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置的基准位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否可以放置</returns>
        bool isValidPlacement(_IGamePiece _piece, Vector2Int _position, int _playerId);
        
        /// <summary>
        /// 在棋盘上放置方块
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置的基准位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否放置成功</returns>
        bool placePiece(_IGamePiece _piece, Vector2Int _position, int _playerId);
        
        /// <summary>
        /// 获取指定玩家的所有有效放置位置
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>有效放置位置列表</returns>
        List<Vector2Int> getValidPlacements(_IGamePiece _piece, int _playerId);
        
        /// <summary>
        /// 获取指定位置的占用状态
        /// </summary>
        /// <param name="_position">棋盘位置</param>
        /// <returns>占用该位置的玩家ID，0表示空位</returns>
        int getPositionOwner(Vector2Int _position);
        
        /// <summary>
        /// 检查位置是否在棋盘范围内
        /// </summary>
        /// <param name="_position">要检查的位置</param>
        /// <returns>是否在有效范围内</returns>
        bool isPositionValid(Vector2Int _position);
        
        /// <summary>
        /// 获取指定玩家的起始角落位置
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>起始角落位置</returns>
        Vector2Int getStartingCorner(int _playerId);
        
        /// <summary>
        /// 检查玩家是否还有有效移动
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_availablePieces">玩家可用方块列表</param>
        /// <returns>是否还有有效移动</returns>
        bool hasValidMoves(int _playerId, List<_IGamePiece> _availablePieces);
        
        /// <summary>
        /// 清空棋盘，重置所有位置
        /// </summary>
        void clearBoard();
        
        /// <summary>
        /// 获取棋盘当前状态的副本
        /// </summary>
        /// <returns>棋盘状态数组</returns>
        int[,] getBoardState();
    }
}