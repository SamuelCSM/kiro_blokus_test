using UnityEngine;
using System.Collections.Generic;

namespace BlokusGame.Core.Interfaces
{
    /// <summary>
    /// 游戏方块接口 - 定义Blokus方块的基本行为和属性
    /// 所有方块类都必须实现此接口以确保一致的操作方式
    /// </summary>
    public interface _IGamePiece
    {
        /// <summary>方块的唯一标识符</summary>
        int pieceId { get; }
        
        /// <summary>方块所属的玩家ID</summary>
        int playerId { get; }
        
        /// <summary>方块的当前形状数据（相对坐标数组）</summary>
        Vector2Int[] currentShape { get; }
        
        /// <summary>方块的颜色</summary>
        Color pieceColor { get; }
        
        /// <summary>方块是否已被放置在棋盘上</summary>
        bool isPlaced { get; }
        
        /// <summary>
        /// 顺时针旋转方块90度
        /// </summary>
        void rotate90Clockwise();
        
        /// <summary>
        /// 水平翻转方块
        /// </summary>
        void flipHorizontal();
        
        /// <summary>
        /// 获取方块在指定位置的所有占用坐标
        /// </summary>
        /// <param name="_position">方块放置的基准位置</param>
        /// <returns>方块占用的所有棋盘坐标</returns>
        List<Vector2Int> getOccupiedPositions(Vector2Int _position);
        
        /// <summary>
        /// 设置方块的放置状态
        /// </summary>
        /// <param name="_placed">是否已放置</param>
        void setPlacedState(bool _placed);
        
        /// <summary>
        /// 重置方块到初始状态（未旋转、未翻转）
        /// </summary>
        void resetToOriginalState();
    }
}