using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Pieces;
using BlokusGame.Core.Data;
using BlokusGame.Core.Managers;

namespace BlokusGame.Core.Rules
{
    /// <summary>
    /// Blokus游戏规则引擎
    /// 实现所有游戏规则的验证逻辑，确保游戏按照标准规则进行
    /// 负责验证方块放置的合法性、管理游戏状态转换
    /// </summary>
    public class RuleEngine : MonoBehaviour, _IRuleEngine
    {
        [Header("规则配置")]
        /// <summary>棋盘大小，标准Blokus为20x20</summary>
        [SerializeField] private int _m_boardSize = 20;
        
        /// <summary>玩家数量，标准为4人游戏</summary>
        [SerializeField] private int _m_playerCount = 4;
        
        /// <summary>是否启用详细的规则验证日志</summary>
        [SerializeField] private bool _m_enableDetailedLogging = false;
        
        [Header("组件引用")]
        /// <summary>棋盘管理器引用</summary>
        [SerializeField] private BoardManager _m_boardManager;
        
        /// <summary>方块管理器引用</summary>
        [SerializeField] private PieceManager _m_pieceManager;
        
        /// <summary>玩家管理器引用</summary>
        [SerializeField] private PlayerManager _m_playerManager;
        
        /// <summary>每个玩家的起始角落位置</summary>
        private readonly Vector2Int[] _m_playerStartCorners = new Vector2Int[]
        {
            new Vector2Int(0, 0),      // 玩家0：左下角
            new Vector2Int(19, 0),     // 玩家1：右下角
            new Vector2Int(19, 19),    // 玩家2：右上角
            new Vector2Int(0, 19)      // 玩家3：左上角
        };
        
        /// <summary>
        /// 组件初始化
        /// 获取必要的管理器组件引用
        /// </summary>
        private void Awake()
        {
            // 自动获取组件引用
            if (_m_boardManager == null)
                _m_boardManager = FindObjectOfType<BoardManager>();
            if (_m_pieceManager == null)
                _m_pieceManager = FindObjectOfType<PieceManager>();
            if (_m_playerManager == null)
                _m_playerManager = FindObjectOfType<PlayerManager>();
        }
        
        /// <summary>
        /// 验证方块放置是否合法
        /// 按照Blokus规则检查所有约束条件
        /// </summary>
        /// <param name="_piece">要放置的游戏方块</param>
        /// <param name="_position">目标放置位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>放置是否合法</returns>
        public bool isValidPlacement(GamePiece _piece, Vector2Int _position, int _playerId)
        {
            var result = validatePlacement(_piece, _position, _playerId);
            
            if (_m_enableDetailedLogging && !result.isValid)
            {
                Debug.LogWarning($"[RuleEngine] 方块放置验证失败: {result.getUserFriendlyMessage()}");
            }
            
            return result.isValid;
        }
        
        /// <summary>
        /// 详细的方块放置验证
        /// 返回包含失败原因的详细结果
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>详细的验证结果</returns>
        public RuleValidationResult validatePlacement(GamePiece _piece, Vector2Int _position, int _playerId)
        {
            // 参数验证
            if (_piece == null)
                return RuleValidationResult.createFailure("方块不能为空", RuleType.None);
            
            if (_playerId < 0 || _playerId >= _m_playerCount)
                return RuleValidationResult.createFailure("无效的玩家ID", RuleType.None);
            
            // 1. 检查边界
            if (!_isWithinBounds(_piece, _position))
                return RuleValidationResult.createFailure("方块超出棋盘边界", RuleType.OutOfBounds);
            
            // 2. 检查重叠
            if (hasOverlap(_piece, _position))
            {
                var conflictPositions = _getConflictPositions(_piece, _position);
                return RuleValidationResult.createFailure("方块与已有方块重叠", RuleType.Overlap, conflictPositions);
            }
            
            // 3. 检查首次放置规则
            if (isFirstPlacement(_playerId))
            {
                if (!isValidCornerPlacement(_piece, _position, _playerId))
                    return RuleValidationResult.createFailure("首次放置必须占据角落位置", RuleType.FirstPlacementCorner);
            }
            else
            {
                // 4. 检查角接触规则
                if (!hasCornerContact(_piece, _position, _playerId))
                    return RuleValidationResult.createFailure("方块必须与已有方块的角相接触", RuleType.CornerContact);
                
                // 5. 检查边不相接规则
                if (hasEdgeContact(_piece, _position, _playerId))
                    return RuleValidationResult.createFailure("方块不能与已有方块的边相接触", RuleType.EdgeContact);
            }
            
            return RuleValidationResult.createSuccess();
        }
        
        /// <summary>
        /// 验证是否为玩家的首次放置
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否为首次放置</returns>
        public bool isFirstPlacement(int _playerId)
        {
            if (_m_pieceManager == null)
            {
                Debug.LogError("[RuleEngine] PieceManager引用为空");
                return true; // 安全起见，假设是首次放置
            }
            
            return _m_pieceManager.getPlacedPieceCount(_playerId) == 0;
        }
        
        /// <summary>
        /// 验证首次放置的角落位置规则
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否符合角落位置规则</returns>
        public bool isValidCornerPlacement(GamePiece _piece, Vector2Int _position, int _playerId)
        {
            Vector2Int startCorner = getPlayerStartCorner(_playerId);
            
            // 检查方块的任意一个格子是否占据起始角落
            foreach (Vector2Int blockPos in _piece.currentShape)
            {
                Vector2Int worldPos = _position + blockPos;
                if (worldPos == startCorner)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 验证角接触规则
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否满足角接触规则</returns>
        public bool hasCornerContact(GamePiece _piece, Vector2Int _position, int _playerId)
        {
            if (_m_boardManager == null)
            {
                Debug.LogError("[RuleEngine] BoardManager引用为空");
                return false;
            }
            
            // 获取方块的所有位置
            Vector2Int[] piecePositions = _piece.currentShape;
            
            // 对于方块的每个格子，检查其对角位置
            foreach (Vector2Int blockPos in piecePositions)
            {
                Vector2Int worldPos = _position + blockPos;
                
                // 检查四个对角方向
                Vector2Int[] cornerDirections = {
                    new Vector2Int(1, 1),   // 右上
                    new Vector2Int(1, -1),  // 右下
                    new Vector2Int(-1, 1),  // 左上
                    new Vector2Int(-1, -1)  // 左下
                };
                
                foreach (Vector2Int cornerDir in cornerDirections)
                {
                    Vector2Int cornerPos = worldPos + cornerDir;
                    
                    // 检查该位置是否被同一玩家的方块占据
                    if (_m_boardManager.isPositionOccupied(cornerPos) && 
                        _m_boardManager.getPlayerAtPosition(cornerPos) == _playerId)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 验证边不相接规则
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否违反边相接规则</returns>
        public bool hasEdgeContact(GamePiece _piece, Vector2Int _position, int _playerId)
        {
            if (_m_boardManager == null)
            {
                Debug.LogError("[RuleEngine] BoardManager引用为空");
                return false;
            }
            
            // 获取方块的所有位置
            Vector2Int[] piecePositions = _piece.currentShape;
            
            // 对于方块的每个格子，检查其四个边的相邻位置
            foreach (Vector2Int blockPos in piecePositions)
            {
                Vector2Int worldPos = _position + blockPos;
                
                // 检查四个边方向
                Vector2Int[] edgeDirections = {
                    Vector2Int.up,      // 上
                    Vector2Int.down,    // 下
                    Vector2Int.left,    // 左
                    Vector2Int.right    // 右
                };
                
                foreach (Vector2Int edgeDir in edgeDirections)
                {
                    Vector2Int edgePos = worldPos + edgeDir;
                    
                    // 检查该位置是否被同一玩家的方块占据
                    if (_m_boardManager.isPositionOccupied(edgePos) && 
                        _m_boardManager.getPlayerAtPosition(edgePos) == _playerId)
                    {
                        return true; // 发现边相接，违反规则
                    }
                }
            }
            
            return false; // 没有边相接，符合规则
        }
        
        /// <summary>
        /// 检查方块是否与其他方块重叠
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <returns>是否存在重叠</returns>
        public bool hasOverlap(GamePiece _piece, Vector2Int _position)
        {
            if (_m_boardManager == null)
            {
                Debug.LogError("[RuleEngine] BoardManager引用为空");
                return true; // 安全起见，假设有重叠
            }
            
            // 检查方块的每个格子是否已被占据
            foreach (Vector2Int blockPos in _piece.currentShape)
            {
                Vector2Int worldPos = _position + blockPos;
                if (_m_boardManager.isPositionOccupied(worldPos))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 检查玩家是否还能继续放置方块
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>玩家是否还能继续游戏</returns>
        public bool canPlayerContinue(int _playerId)
        {
            if (_m_pieceManager == null)
            {
                Debug.LogError("[RuleEngine] PieceManager引用为空");
                return false;
            }
            
            // 获取玩家剩余的方块
            List<GamePiece> remainingPieces = _m_pieceManager.getPlayerPieces(_playerId)
                .Where(piece => !piece.isPlaced).ToList();
            
            if (remainingPieces.Count == 0)
                return false;
            
            // 检查是否有任何方块可以放置在棋盘上
            for (int x = 0; x < _m_boardSize; x++)
            {
                for (int y = 0; y < _m_boardSize; y++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    
                    foreach (GamePiece piece in remainingPieces)
                    {
                        // 尝试所有旋转状态
                        for (int rotation = 0; rotation < 4; rotation++)
                        {
                            if (isValidPlacement(piece, position, _playerId))
                            {
                                return true;
                            }
                            piece.rotate90Clockwise(); // 旋转方块尝试下一个状态
                        }
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 检查游戏是否结束
        /// </summary>
        /// <returns>游戏是否结束</returns>
        public bool isGameOver()
        {
            // 检查是否所有玩家都无法继续
            for (int playerId = 0; playerId < _m_playerCount; playerId++)
            {
                if (canPlayerContinue(playerId))
                {
                    return false; // 还有玩家可以继续
                }
            }
            
            return true; // 所有玩家都无法继续，游戏结束
        }
        
        /// <summary>
        /// 获取玩家的起始角落位置
        /// </summary>
        /// <param name="_playerId">玩家ID (0-3)</param>
        /// <returns>角落位置坐标</returns>
        public Vector2Int getPlayerStartCorner(int _playerId)
        {
            if (_playerId < 0 || _playerId >= _m_playerStartCorners.Length)
            {
                Debug.LogError($"[RuleEngine] 无效的玩家ID: {_playerId}");
                return Vector2Int.zero;
            }
            
            return _m_playerStartCorners[_playerId];
        }
        
        /// <summary>
        /// 检查方块是否在棋盘边界内
        /// </summary>
        /// <param name="_piece">要检查的方块</param>
        /// <param name="_position">放置位置</param>
        /// <returns>是否在边界内</returns>
        private bool _isWithinBounds(GamePiece _piece, Vector2Int _position)
        {
            foreach (Vector2Int blockPos in _piece.currentShape)
            {
                Vector2Int worldPos = _position + blockPos;
                
                if (worldPos.x < 0 || worldPos.x >= _m_boardSize ||
                    worldPos.y < 0 || worldPos.y >= _m_boardSize)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取冲突位置列表
        /// </summary>
        /// <param name="_piece">方块</param>
        /// <param name="_position">位置</param>
        /// <returns>冲突位置数组</returns>
        private Vector2Int[] _getConflictPositions(GamePiece _piece, Vector2Int _position)
        {
            List<Vector2Int> conflicts = new List<Vector2Int>();
            
            foreach (Vector2Int blockPos in _piece.currentShape)
            {
                Vector2Int worldPos = _position + blockPos;
                if (_m_boardManager != null && _m_boardManager.isPositionOccupied(worldPos))
                {
                    conflicts.Add(worldPos);
                }
            }
            
            return conflicts.ToArray();
        }
    }
}