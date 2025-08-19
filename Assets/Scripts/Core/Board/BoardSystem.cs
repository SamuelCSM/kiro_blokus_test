using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Events;

namespace BlokusGame.Core.Board
{
    /// <summary>
    /// 棋盘系统核心类 - 实现Blokus游戏的20x20棋盘管理
    /// 负责棋盘状态管理、方块放置验证、规则检查和状态序列化
    /// 实现完整的Blokus游戏规则：角接触、边不相接、起始位置验证等
    /// </summary>
    [System.Serializable]
    public class BoardSystem : _IGameBoard
    {
        [Header("棋盘配置")]
        /// <summary>棋盘尺寸，标准Blokus为20x20</summary>
        [SerializeField] private int _m_boardSize = 20;
        
        /// <summary>棋盘状态数组，0表示空位，1-4表示玩家ID</summary>
        private int[,] _m_boardGrid;
        
        /// <summary>每个玩家的起始角落位置</summary>
        private readonly Vector2Int[] _m_startingCorners = new Vector2Int[]
        {
            new Vector2Int(0, 0),      // 玩家1：左下角
            new Vector2Int(19, 0),     // 玩家2：右下角
            new Vector2Int(19, 19),    // 玩家3：右上角
            new Vector2Int(0, 19)      // 玩家4：左上角
        };
        
        /// <summary>记录每个玩家是否已经放置了第一个方块</summary>
        private bool[] _m_playerFirstPiecePlaced;
        
        /// <summary>缓存每个玩家的可连接位置，用于优化性能</summary>
        private Dictionary<int, HashSet<Vector2Int>> _m_playerConnectablePositions;
        
        /// <summary>四个方向的偏移向量（上下左右）</summary>
        private readonly Vector2Int[] _m_adjacentDirections = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };
        
        /// <summary>四个对角方向的偏移向量</summary>
        private readonly Vector2Int[] _m_diagonalDirections = new Vector2Int[]
        {
            new Vector2Int(1, 1), new Vector2Int(1, -1), 
            new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };
        
        // 接口属性实现
        /// <summary>棋盘尺寸</summary>
        public int boardSize => _m_boardSize;
        
        /// <summary>
        /// 初始化棋盘系统
        /// 创建空白棋盘并初始化所有必要的数据结构
        /// </summary>
        public void initializeBoard()
        {
            // 初始化棋盘网格
            _m_boardGrid = new int[_m_boardSize, _m_boardSize];
            
            // 初始化玩家状态
            _m_playerFirstPiecePlaced = new bool[4]; // 支持最多4个玩家
            
            // 初始化可连接位置缓存
            _m_playerConnectablePositions = new Dictionary<int, HashSet<Vector2Int>>();
            for (int i = 1; i <= 4; i++)
            {
                _m_playerConnectablePositions[i] = new HashSet<Vector2Int>();
                // 初始时，每个玩家只能在自己的起始角落放置
                _m_playerConnectablePositions[i].Add(_m_startingCorners[i - 1]);
            }
            
            Debug.Log("[BoardSystem] 棋盘系统初始化完成，棋盘尺寸：" + _m_boardSize + "x" + _m_boardSize);
        }
        
        /// <summary>
        /// 检查指定位置是否可以放置方块
        /// 验证Blokus游戏的所有放置规则
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置的基准位置</param>
        /// <param name="_playerId">玩家ID（1-4）</param>
        /// <returns>是否可以放置</returns>
        public bool isValidPlacement(_IGamePiece _piece, Vector2Int _position, int _playerId)
        {
            // 参数验证
            if (_piece == null)
            {
                Debug.LogError("[BoardSystem] 方块参数为空，无法验证放置");
                return false;
            }
            
            if (_playerId < 1 || _playerId > 4)
            {
                Debug.LogError($"[BoardSystem] 玩家ID {_playerId} 无效，必须在1-4范围内");
                return false;
            }
            
            // 获取方块占用的所有位置
            List<Vector2Int> occupiedPositions = _piece.getOccupiedPositions(_position);
            
            // 检查所有位置是否在棋盘范围内
            foreach (Vector2Int pos in occupiedPositions)
            {
                if (!isPositionValid(pos))
                {
                    return false;
                }
            }
            
            // 检查所有位置是否为空
            foreach (Vector2Int pos in occupiedPositions)
            {
                if (_m_boardGrid[pos.x, pos.y] != 0)
                {
                    return false; // 位置已被占用
                }
            }
            
            // 检查是否违反边相接规则（不能与同一玩家的方块在边上相接）
            if (_hasAdjacentSamePlayerPiece(occupiedPositions, _playerId))
            {
                return false;
            }
            
            // 检查角接触规则
            if (_m_playerFirstPiecePlaced[_playerId - 1])
            {
                // 非首次放置：必须与已有方块在角上相接
                if (!_hasCornerConnection(occupiedPositions, _playerId))
                {
                    return false;
                }
            }
            else
            {
                // 首次放置：必须包含起始角落位置
                Vector2Int startingCorner = _m_startingCorners[_playerId - 1];
                if (!occupiedPositions.Contains(startingCorner))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 在棋盘上放置方块
        /// 更新棋盘状态并维护可连接位置缓存
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置的基准位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否放置成功</returns>
        public bool placePiece(_IGamePiece _piece, Vector2Int _position, int _playerId)
        {
            // 验证放置是否有效
            if (!isValidPlacement(_piece, _position, _playerId))
            {
                Debug.LogWarning($"[BoardSystem] 玩家 {_playerId} 无法在位置 {_position} 放置方块");
                return false;
            }
            
            // 获取方块占用的所有位置
            List<Vector2Int> occupiedPositions = _piece.getOccupiedPositions(_position);
            
            // 在棋盘上标记这些位置
            foreach (Vector2Int pos in occupiedPositions)
            {
                _m_boardGrid[pos.x, pos.y] = _playerId;
            }
            
            // 更新玩家首次放置状态
            if (!_m_playerFirstPiecePlaced[_playerId - 1])
            {
                _m_playerFirstPiecePlaced[_playerId - 1] = true;
            }
            
            // 更新可连接位置缓存
            _updateConnectablePositions(_playerId, occupiedPositions);
            
            // 设置方块为已放置状态
            _piece.setPlacedState(true);
            
            Debug.Log($"[BoardSystem] 玩家 {_playerId} 成功在位置 {_position} 放置方块，占用 {occupiedPositions.Count} 个格子");
            
            return true;
        }
        
        /// <summary>
        /// 获取指定玩家对特定方块的所有有效放置位置
        /// 使用优化算法，只检查可能的连接位置
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>有效放置位置列表</returns>
        public List<Vector2Int> getValidPlacements(_IGamePiece _piece, int _playerId)
        {
            List<Vector2Int> validPlacements = new List<Vector2Int>();
            
            if (_piece == null || _playerId < 1 || _playerId > 4)
            {
                return validPlacements;
            }
            
            // 获取玩家当前的可连接位置
            HashSet<Vector2Int> connectablePositions = _m_playerConnectablePositions[_playerId];
            
            // 对于每个可连接位置，尝试将方块的每个格子放在该位置
            foreach (Vector2Int connectPos in connectablePositions)
            {
                Vector2Int[] pieceShape = _piece.currentShape;
                
                // 尝试方块的每个格子作为连接点
                for (int i = 0; i < pieceShape.Length; i++)
                {
                    // 计算方块基准位置，使得第i个格子位于连接位置
                    Vector2Int basePosition = connectPos - pieceShape[i];
                    
                    // 检查这个放置是否有效
                    if (isValidPlacement(_piece, basePosition, _playerId))
                    {
                        // 避免重复添加相同的位置
                        if (!validPlacements.Contains(basePosition))
                        {
                            validPlacements.Add(basePosition);
                        }
                    }
                }
            }
            
            return validPlacements;
        }
        
        /// <summary>
        /// 获取指定位置的占用状态
        /// </summary>
        /// <param name="_position">棋盘位置</param>
        /// <returns>占用该位置的玩家ID，0表示空位</returns>
        public int getPositionOwner(Vector2Int _position)
        {
            if (!isPositionValid(_position))
            {
                return -1; // 无效位置
            }
            
            return _m_boardGrid[_position.x, _position.y];
        }
        
        /// <summary>
        /// 检查位置是否在棋盘范围内
        /// </summary>
        /// <param name="_position">要检查的位置</param>
        /// <returns>是否在有效范围内</returns>
        public bool isPositionValid(Vector2Int _position)
        {
            return _position.x >= 0 && _position.x < _m_boardSize && 
                   _position.y >= 0 && _position.y < _m_boardSize;
        }
        
        /// <summary>
        /// 获取指定玩家的起始角落位置
        /// </summary>
        /// <param name="_playerId">玩家ID（1-4）</param>
        /// <returns>起始角落位置</returns>
        public Vector2Int getStartingCorner(int _playerId)
        {
            if (_playerId < 1 || _playerId > 4)
            {
                Debug.LogError($"[BoardSystem] 玩家ID {_playerId} 无效，返回默认位置");
                return Vector2Int.zero;
            }
            
            return _m_startingCorners[_playerId - 1];
        }
        
        /// <summary>
        /// 检查玩家是否还有有效移动
        /// 遍历玩家的所有可用方块，检查是否存在有效放置位置
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_availablePieces">玩家可用方块列表</param>
        /// <returns>是否还有有效移动</returns>
        public bool hasValidMoves(int _playerId, List<_IGamePiece> _availablePieces)
        {
            if (_availablePieces == null || _availablePieces.Count == 0)
            {
                return false;
            }
            
            // 检查每个可用方块是否有有效放置位置
            foreach (_IGamePiece piece in _availablePieces)
            {
                if (piece.isPlaced) continue; // 跳过已放置的方块
                
                // 尝试方块的所有旋转和翻转状态
                for (int rotation = 0; rotation < 4; rotation++)
                {
                    for (int flip = 0; flip < 2; flip++)
                    {
                        // 临时应用变换
                        _IGamePiece tempPiece = _createTempPiece(piece, rotation, flip == 1);
                        
                        // 检查是否有有效放置位置
                        List<Vector2Int> validPlacements = getValidPlacements(tempPiece, _playerId);
                        if (validPlacements.Count > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 清空棋盘，重置所有位置和状态
        /// </summary>
        public void clearBoard()
        {
            // 重置棋盘网格
            for (int x = 0; x < _m_boardSize; x++)
            {
                for (int y = 0; y < _m_boardSize; y++)
                {
                    _m_boardGrid[x, y] = 0;
                }
            }
            
            // 重置玩家状态
            for (int i = 0; i < _m_playerFirstPiecePlaced.Length; i++)
            {
                _m_playerFirstPiecePlaced[i] = false;
            }
            
            // 重置可连接位置缓存
            for (int i = 1; i <= 4; i++)
            {
                _m_playerConnectablePositions[i].Clear();
                _m_playerConnectablePositions[i].Add(_m_startingCorners[i - 1]);
            }
            
            Debug.Log("[BoardSystem] 棋盘已清空并重置");
        }
        
        /// <summary>
        /// 获取棋盘当前状态的副本
        /// </summary>
        /// <returns>棋盘状态数组的副本</returns>
        public int[,] getBoardState()
        {
            int[,] stateCopy = new int[_m_boardSize, _m_boardSize];
            
            for (int x = 0; x < _m_boardSize; x++)
            {
                for (int y = 0; y < _m_boardSize; y++)
                {
                    stateCopy[x, y] = _m_boardGrid[x, y];
                }
            }
            
            return stateCopy;
        }      
  
        // ==================== 私有辅助方法 ====================
        
        /// <summary>
        /// 检查方块是否与同一玩家的已有方块在边上相接
        /// Blokus规则：同一玩家的方块不能在边上相接
        /// </summary>
        /// <param name="_occupiedPositions">方块占用的位置列表</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否存在边相接</returns>
        private bool _hasAdjacentSamePlayerPiece(List<Vector2Int> _occupiedPositions, int _playerId)
        {
            foreach (Vector2Int pos in _occupiedPositions)
            {
                // 检查四个相邻方向
                foreach (Vector2Int direction in _m_adjacentDirections)
                {
                    Vector2Int adjacentPos = pos + direction;
                    
                    // 如果相邻位置在棋盘内且被同一玩家占用，则违反规则
                    if (isPositionValid(adjacentPos) && 
                        _m_boardGrid[adjacentPos.x, adjacentPos.y] == _playerId)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 检查方块是否与同一玩家的已有方块在角上相接
        /// Blokus规则：非首次放置的方块必须与已有方块在角上相接
        /// </summary>
        /// <param name="_occupiedPositions">方块占用的位置列表</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否存在角相接</returns>
        private bool _hasCornerConnection(List<Vector2Int> _occupiedPositions, int _playerId)
        {
            foreach (Vector2Int pos in _occupiedPositions)
            {
                // 检查四个对角方向
                foreach (Vector2Int direction in _m_diagonalDirections)
                {
                    Vector2Int diagonalPos = pos + direction;
                    
                    // 如果对角位置在棋盘内且被同一玩家占用，则满足角接触规则
                    if (isPositionValid(diagonalPos) && 
                        _m_boardGrid[diagonalPos.x, diagonalPos.y] == _playerId)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 更新玩家的可连接位置缓存
        /// 在放置方块后，计算新的可连接位置并更新缓存
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_newOccupiedPositions">新放置方块占用的位置</param>
        private void _updateConnectablePositions(int _playerId, List<Vector2Int> _newOccupiedPositions)
        {
            HashSet<Vector2Int> connectablePositions = _m_playerConnectablePositions[_playerId];
            
            // 移除被新方块占用的可连接位置
            foreach (Vector2Int pos in _newOccupiedPositions)
            {
                connectablePositions.Remove(pos);
            }
            
            // 添加新方块周围的对角位置作为可连接位置
            foreach (Vector2Int pos in _newOccupiedPositions)
            {
                foreach (Vector2Int direction in _m_diagonalDirections)
                {
                    Vector2Int diagonalPos = pos + direction;
                    
                    // 检查对角位置是否可以作为连接点
                    if (_isValidConnectablePosition(diagonalPos, _playerId))
                    {
                        connectablePositions.Add(diagonalPos);
                    }
                }
            }
        }
        
        /// <summary>
        /// 检查位置是否可以作为玩家的连接点
        /// 连接点必须：1.在棋盘内 2.为空 3.不与同玩家方块边相接
        /// </summary>
        /// <param name="_position">要检查的位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否可以作为连接点</returns>
        private bool _isValidConnectablePosition(Vector2Int _position, int _playerId)
        {
            // 位置必须在棋盘范围内
            if (!isPositionValid(_position))
            {
                return false;
            }
            
            // 位置必须为空
            if (_m_boardGrid[_position.x, _position.y] != 0)
            {
                return false;
            }
            
            // 位置不能与同一玩家的方块在边上相接
            foreach (Vector2Int direction in _m_adjacentDirections)
            {
                Vector2Int adjacentPos = _position + direction;
                
                if (isPositionValid(adjacentPos) && 
                    _m_boardGrid[adjacentPos.x, adjacentPos.y] == _playerId)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 创建临时方块用于测试不同的变换状态
        /// 这是一个简化的实现，实际项目中可能需要更完整的方块克隆
        /// </summary>
        /// <param name="_originalPiece">原始方块</param>
        /// <param name="_rotationCount">旋转次数</param>
        /// <param name="_isFlipped">是否翻转</param>
        /// <returns>变换后的临时方块</returns>
        private _IGamePiece _createTempPiece(_IGamePiece _originalPiece, int _rotationCount, bool _isFlipped)
        {
            // 注意：这里需要根据实际的方块实现来创建临时副本
            // 目前返回原方块，实际使用时需要实现方块的深拷贝和变换
            // TODO: 实现完整的方块克隆和变换功能
            return _originalPiece;
        }
        
        // ==================== 序列化相关方法 ====================
        
        /// <summary>
        /// 序列化棋盘状态为JSON字符串
        /// 用于保存游戏进度或网络传输
        /// </summary>
        /// <returns>序列化后的JSON字符串</returns>
        public string serializeBoardState()
        {
            BoardStateData stateData = new BoardStateData
            {
                boardSize = _m_boardSize,
                gridData = _flattenGrid(),
                playerFirstPiecePlaced = (bool[])_m_playerFirstPiecePlaced.Clone(),
                connectablePositions = _serializeConnectablePositions()
            };
            
            return JsonUtility.ToJson(stateData, true);
        }
        
        /// <summary>
        /// 从JSON字符串反序列化棋盘状态
        /// 用于加载游戏进度或接收网络数据
        /// </summary>
        /// <param name="_jsonData">JSON格式的状态数据</param>
        /// <returns>是否反序列化成功</returns>
        public bool deserializeBoardState(string _jsonData)
        {
            try
            {
                BoardStateData stateData = JsonUtility.FromJson<BoardStateData>(_jsonData);
                
                // 验证数据有效性
                if (stateData.boardSize != _m_boardSize)
                {
                    Debug.LogError($"[BoardSystem] 棋盘尺寸不匹配：期望 {_m_boardSize}，实际 {stateData.boardSize}");
                    return false;
                }
                
                // 恢复棋盘网格
                _restoreGrid(stateData.gridData);
                
                // 恢复玩家状态
                _m_playerFirstPiecePlaced = (bool[])stateData.playerFirstPiecePlaced.Clone();
                
                // 恢复可连接位置
                _deserializeConnectablePositions(stateData.connectablePositions);
                
                Debug.Log("[BoardSystem] 棋盘状态反序列化成功");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BoardSystem] 棋盘状态反序列化失败：{ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 将二维网格数据扁平化为一维数组
        /// </summary>
        /// <returns>扁平化的网格数据</returns>
        private int[] _flattenGrid()
        {
            int[] flatData = new int[_m_boardSize * _m_boardSize];
            
            for (int x = 0; x < _m_boardSize; x++)
            {
                for (int y = 0; y < _m_boardSize; y++)
                {
                    flatData[x * _m_boardSize + y] = _m_boardGrid[x, y];
                }
            }
            
            return flatData;
        }
        
        /// <summary>
        /// 从一维数组恢复二维网格数据
        /// </summary>
        /// <param name="_flatData">扁平化的网格数据</param>
        private void _restoreGrid(int[] _flatData)
        {
            if (_flatData.Length != _m_boardSize * _m_boardSize)
            {
                Debug.LogError("[BoardSystem] 网格数据长度不匹配");
                return;
            }
            
            for (int x = 0; x < _m_boardSize; x++)
            {
                for (int y = 0; y < _m_boardSize; y++)
                {
                    _m_boardGrid[x, y] = _flatData[x * _m_boardSize + y];
                }
            }
        }
        
        /// <summary>
        /// 序列化可连接位置数据
        /// </summary>
        /// <returns>序列化的可连接位置数据</returns>
        private SerializableConnectablePositions _serializeConnectablePositions()
        {
            SerializableConnectablePositions data = new SerializableConnectablePositions();
            
            for (int playerId = 1; playerId <= 4; playerId++)
            {
                if (_m_playerConnectablePositions.ContainsKey(playerId))
                {
                    Vector2Int[] positions = _m_playerConnectablePositions[playerId].ToArray();
                    
                    switch (playerId)
                    {
                        case 1: data.player1Positions = positions; break;
                        case 2: data.player2Positions = positions; break;
                        case 3: data.player3Positions = positions; break;
                        case 4: data.player4Positions = positions; break;
                    }
                }
            }
            
            return data;
        }
        
        /// <summary>
        /// 反序列化可连接位置数据
        /// </summary>
        /// <param name="_data">序列化的可连接位置数据</param>
        private void _deserializeConnectablePositions(SerializableConnectablePositions _data)
        {
            _m_playerConnectablePositions.Clear();
            
            _m_playerConnectablePositions[1] = new HashSet<Vector2Int>(_data.player1Positions ?? new Vector2Int[0]);
            _m_playerConnectablePositions[2] = new HashSet<Vector2Int>(_data.player2Positions ?? new Vector2Int[0]);
            _m_playerConnectablePositions[3] = new HashSet<Vector2Int>(_data.player3Positions ?? new Vector2Int[0]);
            _m_playerConnectablePositions[4] = new HashSet<Vector2Int>(_data.player4Positions ?? new Vector2Int[0]);
        }
    }
    
    // ==================== 序列化数据结构 ====================
    
    /// <summary>
    /// 棋盘状态序列化数据结构
    /// 用于JSON序列化和反序列化
    /// </summary>
    [System.Serializable]
    public class BoardStateData
    {
        /// <summary>棋盘尺寸</summary>
        public int boardSize;
        
        /// <summary>扁平化的网格数据</summary>
        public int[] gridData;
        
        /// <summary>玩家首次放置状态</summary>
        public bool[] playerFirstPiecePlaced;
        
        /// <summary>可连接位置数据</summary>
        public SerializableConnectablePositions connectablePositions;
    }
    
    /// <summary>
    /// 可连接位置序列化数据结构
    /// </summary>
    [System.Serializable]
    public class SerializableConnectablePositions
    {
        public Vector2Int[] player1Positions;
        public Vector2Int[] player2Positions;
        public Vector2Int[] player3Positions;
        public Vector2Int[] player4Positions;
    }
}