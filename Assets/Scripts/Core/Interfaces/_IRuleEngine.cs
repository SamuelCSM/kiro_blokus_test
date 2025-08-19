using UnityEngine;
using System.Collections.Generic;
using BlokusGame.Core.Data;
using BlokusGame.Core.Pieces;

namespace BlokusGame.Core.Interfaces
{
    /// <summary>
    /// 规则引擎接口 - 定义Blokus游戏规则验证的核心功能
    /// 负责验证方块放置的合法性、管理游戏规则和状态检查
    /// 确保游戏按照标准Blokus规则进行
    /// </summary>
    public interface _IRuleEngine
    {
        /// <summary>
        /// 验证方块放置是否合法
        /// 按照Blokus规则检查所有约束条件
        /// </summary>
        /// <param name="_piece">要放置的游戏方块</param>
        /// <param name="_position">目标放置位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>放置是否合法</returns>
        bool isValidPlacement(GamePiece _piece, Vector2Int _position, int _playerId);
        
        /// <summary>
        /// 详细的方块放置验证
        /// 返回包含失败原因的详细结果
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>详细的验证结果</returns>
        RuleValidationResult validatePlacement(GamePiece _piece, Vector2Int _position, int _playerId);
        
        /// <summary>
        /// 验证是否为玩家的首次放置
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否为首次放置</returns>
        bool isFirstPlacement(int _playerId);
        
        /// <summary>
        /// 验证首次放置的角落位置规则
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否符合角落位置规则</returns>
        bool isValidCornerPlacement(GamePiece _piece, Vector2Int _position, int _playerId);
        
        /// <summary>
        /// 验证角接触规则
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否满足角接触规则</returns>
        bool hasCornerContact(GamePiece _piece, Vector2Int _position, int _playerId);
        
        /// <summary>
        /// 验证边不相接规则
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否违反边相接规则</returns>
        bool hasEdgeContact(GamePiece _piece, Vector2Int _position, int _playerId);
        
        /// <summary>
        /// 检查方块是否与其他方块重叠
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <returns>是否存在重叠</returns>
        bool hasOverlap(GamePiece _piece, Vector2Int _position);
        
        /// <summary>
        /// 检查玩家是否还能继续放置方块
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>玩家是否还能继续游戏</returns>
        bool canPlayerContinue(int _playerId);
        
        /// <summary>
        /// 检查游戏是否结束
        /// </summary>
        /// <returns>游戏是否结束</returns>
        bool isGameOver();
        
        /// <summary>
        /// 获取玩家的起始角落位置
        /// </summary>
        /// <param name="_playerId">玩家ID (0-3)</param>
        /// <returns>角落位置坐标</returns>
        Vector2Int getPlayerStartCorner(int _playerId);
    }
}