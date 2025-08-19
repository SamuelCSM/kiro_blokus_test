using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Data;
using BlokusGame.Core.Pieces;
using BlokusGame.Core.Events;

namespace BlokusGame.Core.Managers
{
    /// <summary>
    /// 方块管理器 - 负责管理游戏中所有方块的创建、销毁和状态管理
    /// 提供方块实例化、玩家方块库存管理、方块查找等核心功能
    /// 支持对象池优化和方块状态同步
    /// </summary>
    public class PieceManager : MonoBehaviour
    {
        [Header("方块配置")]
        /// <summary>所有方块数据的引用数组，包含21种标准Blokus方块</summary>
        [SerializeField] private PieceData[] _m_allPieceData;
        
        /// <summary>方块预制体，用于实例化方块对象</summary>
        [SerializeField] private GameObject _m_piecePrefab;
        
        /// <summary>方块父对象，用于组织场景层次结构</summary>
        [SerializeField] private Transform _m_pieceParent;
        
        [Header("玩家颜色配置")]
        /// <summary>四个玩家的颜色配置</summary>
        [SerializeField] private Color[] _m_playerColors = new Color[]
        {
            Color.blue,    // 玩家1 - 蓝色
            Color.yellow,  // 玩家2 - 黄色  
            Color.red,     // 玩家3 - 红色
            Color.green    // 玩家4 - 绿色
        };
        
        // 方块管理数据结构
        /// <summary>所有活跃方块的字典，键为方块实例ID，值为方块对象</summary>
        private Dictionary<int, GamePiece> _m_activePieces = new Dictionary<int, GamePiece>();
        
        /// <summary>每个玩家的方块库存，键为玩家ID，值为方块列表</summary>
        private Dictionary<int, List<GamePiece>> _m_playerPieces = new Dictionary<int, List<GamePiece>>();
        
        /// <summary>已放置的方块列表</summary>
        private List<GamePiece> _m_placedPieces = new List<GamePiece>();
        
        /// <summary>方块对象池，用于复用方块对象</summary>
        private Queue<GameObject> _m_piecePool = new Queue<GameObject>();
        
        /// <summary>下一个方块实例的唯一ID</summary>
        private int _m_nextPieceInstanceId = 1;
        
        // 单例模式
        /// <summary>PieceManager单例实例</summary>
        public static PieceManager instance { get; private set; }
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法 - 初始化单例和基础设置
        /// </summary>
        private void Awake()
        {
            // 单例模式设置
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                _initializeManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Unity Start方法 - 初始化方块数据
        /// </summary>
        private void Start()
        {
            _validateConfiguration();
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 清理资源
        /// </summary>
        private void OnDestroy()
        {
            if (instance == this)
            {
                _cleanupManager();
                instance = null;
            }
        }
        
        #endregion
        
        #region 公共方法 - 方块创建和管理
        
        /// <summary>
        /// 为指定玩家创建完整的方块库存
        /// </summary>
        /// <param name="_playerId">玩家ID（0-3）</param>
        /// <returns>创建是否成功</returns>
        public bool createPlayerPieces(int _playerId)
        {
            if (_playerId < 0 || _playerId >= _m_playerColors.Length)
            {
                Debug.LogError($"[PieceManager] 无效的玩家ID: {_playerId}");
                return false;
            }
            
            if (_m_allPieceData == null || _m_allPieceData.Length == 0)
            {
                Debug.LogError("[PieceManager] 方块数据未配置");
                return false;
            }
            
            // 清理该玩家现有的方块
            _clearPlayerPieces(_playerId);
            
            List<GamePiece> playerPieces = new List<GamePiece>();
            Color playerColor = _m_playerColors[_playerId];
            
            // 为每种方块类型创建一个实例
            foreach (PieceData pieceData in _m_allPieceData)
            {
                if (pieceData == null) continue;
                
                GamePiece piece = _createPieceInstance(pieceData, _playerId, playerColor);
                if (piece != null)
                {
                    playerPieces.Add(piece);
                    _m_activePieces[piece.GetInstanceID()] = piece;
                }
            }
            
            _m_playerPieces[_playerId] = playerPieces;
            
            Debug.Log($"[PieceManager] 为玩家 {_playerId} 创建了 {playerPieces.Count} 个方块");
            return true;
        }
        
        /// <summary>
        /// 获取指定玩家的所有可用方块
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>玩家的方块列表</returns>
        public List<GamePiece> getPlayerPieces(int _playerId)
        {
            if (_m_playerPieces.ContainsKey(_playerId))
            {
                return _m_playerPieces[_playerId].Where(p => !p.isPlaced).ToList();
            }
            
            return new List<GamePiece>();
        }
        
        /// <summary>
        /// 获取指定玩家的所有方块（包括已放置的）
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>玩家的所有方块列表</returns>
        public List<GamePiece> getAllPlayerPieces(int _playerId)
        {
            if (_m_playerPieces.ContainsKey(_playerId))
            {
                return new List<GamePiece>(_m_playerPieces[_playerId]);
            }
            
            return new List<GamePiece>();
        }
        
        /// <summary>
        /// 根据方块ID获取指定玩家的方块
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_pieceId">方块ID</param>
        /// <returns>找到的方块，如果不存在返回null</returns>
        public GamePiece getPlayerPiece(int _playerId, int _pieceId)
        {
            if (!_m_playerPieces.ContainsKey(_playerId))
                return null;
            
            return _m_playerPieces[_playerId].FirstOrDefault(p => p.pieceId == _pieceId);
        }
        
        /// <summary>
        /// 标记方块为已放置状态
        /// </summary>
        /// <param name="_piece">要标记的方块</param>
        /// <param name="_position">放置位置</param>
        public void markPieceAsPlaced(GamePiece _piece, Vector2Int _position)
        {
            if (_piece == null)
            {
                Debug.LogError("[PieceManager] 尝试标记空方块为已放置");
                return;
            }
            
            _piece.setPlacedState(true);
            
            if (!_m_placedPieces.Contains(_piece))
            {
                _m_placedPieces.Add(_piece);
            }
            
            // 触发方块放置事件
            GameEvents.onPiecePlaced?.Invoke(_piece.playerId, _piece, _position);
            
            Debug.Log($"[PieceManager] 方块 {_piece.pieceId} 被玩家 {_piece.playerId} 放置在位置 {_position}");
        }
        
        /// <summary>
        /// 获取所有已放置的方块
        /// </summary>
        /// <returns>已放置方块的列表</returns>
        public List<GamePiece> getPlacedPieces()
        {
            return new List<GamePiece>(_m_placedPieces);
        }
        
        /// <summary>
        /// 获取指定玩家已放置的方块数量
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>已放置的方块数量</returns>
        public int getPlacedPieceCount(int _playerId)
        {
            return _m_placedPieces.Count(p => p.playerId == _playerId);
        }
        
        /// <summary>
        /// 获取指定玩家剩余的方块数量
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>剩余方块数量</returns>
        public int getRemainingPieceCount(int _playerId)
        {
            if (!_m_playerPieces.ContainsKey(_playerId))
                return 0;
            
            return _m_playerPieces[_playerId].Count(p => !p.isPlaced);
        }
        
        #endregion
        
        #region 公共方法 - 游戏状态管理
        
        /// <summary>
        /// 重置所有方块到初始状态
        /// </summary>
        public void resetAllPieces()
        {
            foreach (var piece in _m_activePieces.Values)
            {
                piece.resetToOriginalState();
            }
            
            _m_placedPieces.Clear();
            
            Debug.Log("[PieceManager] 所有方块已重置到初始状态");
        }
        
        /// <summary>
        /// 重置指定玩家的所有方块
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        public void resetPlayerPieces(int _playerId)
        {
            if (!_m_playerPieces.ContainsKey(_playerId))
                return;
            
            foreach (var piece in _m_playerPieces[_playerId])
            {
                piece.resetToOriginalState();
                _m_placedPieces.Remove(piece);
            }
            
            Debug.Log($"[PieceManager] 玩家 {_playerId} 的所有方块已重置");
        }
        
        /// <summary>
        /// 清理所有方块数据
        /// </summary>
        public void clearAllPieces()
        {
            // 销毁所有方块对象
            foreach (var piece in _m_activePieces.Values)
            {
                if (piece != null && piece.gameObject != null)
                {
                    _returnPieceToPool(piece.gameObject);
                }
            }
            
            _m_activePieces.Clear();
            _m_playerPieces.Clear();
            _m_placedPieces.Clear();
            
            Debug.Log("[PieceManager] 所有方块数据已清理");
        }
        
        #endregion
        
        #region 私有方法 - 初始化和配置
        
        /// <summary>
        /// 初始化管理器
        /// </summary>
        private void _initializeManager()
        {
            // 创建方块父对象
            if (_m_pieceParent == null)
            {
                GameObject parentObj = new GameObject("Pieces");
                parentObj.transform.SetParent(transform);
                _m_pieceParent = parentObj.transform;
            }
            
            // 预热对象池
            _preWarmPool(10);
            
            Debug.Log("[PieceManager] 管理器初始化完成");
        }
        
        /// <summary>
        /// 验证配置
        /// </summary>
        private void _validateConfiguration()
        {
            if (_m_allPieceData == null || _m_allPieceData.Length == 0)
            {
                Debug.LogError("[PieceManager] 方块数据配置为空，请在Inspector中设置");
                return;
            }
            
            if (_m_piecePrefab == null)
            {
                Debug.LogError("[PieceManager] 方块预制体未设置，请在Inspector中设置");
                return;
            }
            
            if (_m_playerColors.Length < 4)
            {
                Debug.LogWarning("[PieceManager] 玩家颜色配置不足4个，可能影响多人游戏");
            }
            
            Debug.Log($"[PieceManager] 配置验证完成，共有 {_m_allPieceData.Length} 种方块类型");
        }
        
        /// <summary>
        /// 清理管理器资源
        /// </summary>
        private void _cleanupManager()
        {
            clearAllPieces();
            
            // 清理对象池
            while (_m_piecePool.Count > 0)
            {
                var pooledObj = _m_piecePool.Dequeue();
                if (pooledObj != null)
                {
                    DestroyImmediate(pooledObj);
                }
            }
            
            Debug.Log("[PieceManager] 管理器资源清理完成");
        }
        
        #endregion
        
        #region 私有方法 - 方块创建和对象池
        
        /// <summary>
        /// 创建方块实例
        /// </summary>
        /// <param name="_pieceData">方块数据</param>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_color">方块颜色</param>
        /// <returns>创建的方块实例</returns>
        private GamePiece _createPieceInstance(PieceData _pieceData, int _playerId, Color _color)
        {
            GameObject pieceObj = _getPieceFromPool();
            if (pieceObj == null)
            {
                Debug.LogError("[PieceManager] 无法从对象池获取方块对象");
                return null;
            }
            
            pieceObj.transform.SetParent(_m_pieceParent);
            pieceObj.name = $"Piece_{_pieceData.pieceId}_Player{_playerId}_{_m_nextPieceInstanceId}";
            
            GamePiece gamePiece = pieceObj.GetComponent<GamePiece>();
            if (gamePiece == null)
            {
                gamePiece = pieceObj.AddComponent<GamePiece>();
            }
            
            gamePiece.initialize(_pieceData, _playerId, _color);
            
            _m_nextPieceInstanceId++;
            
            return gamePiece;
        }
        
        /// <summary>
        /// 从对象池获取方块对象
        /// </summary>
        /// <returns>方块游戏对象</returns>
        private GameObject _getPieceFromPool()
        {
            if (_m_piecePool.Count > 0)
            {
                GameObject pooledObj = _m_piecePool.Dequeue();
                pooledObj.SetActive(true);
                return pooledObj;
            }
            
            // 对象池为空，创建新对象
            if (_m_piecePrefab != null)
            {
                return Instantiate(_m_piecePrefab);
            }
            
            // 如果没有预制体，创建空对象
            return new GameObject("GamePiece");
        }
        
        /// <summary>
        /// 将方块对象返回到对象池
        /// </summary>
        /// <param name="_pieceObj">要返回的方块对象</param>
        private void _returnPieceToPool(GameObject _pieceObj)
        {
            if (_pieceObj == null) return;
            
            _pieceObj.SetActive(false);
            _pieceObj.transform.SetParent(transform);
            _m_piecePool.Enqueue(_pieceObj);
        }
        
        /// <summary>
        /// 预热对象池
        /// </summary>
        /// <param name="_count">预创建的对象数量</param>
        private void _preWarmPool(int _count)
        {
            for (int i = 0; i < _count; i++)
            {
                GameObject obj = _getPieceFromPool();
                _returnPieceToPool(obj);
            }
            
            Debug.Log($"[PieceManager] 对象池预热完成，创建了 {_count} 个对象");
        }
        
        /// <summary>
        /// 清理指定玩家的方块
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        private void _clearPlayerPieces(int _playerId)
        {
            if (!_m_playerPieces.ContainsKey(_playerId))
                return;
            
            foreach (var piece in _m_playerPieces[_playerId])
            {
                if (piece != null && piece.gameObject != null)
                {
                    _m_activePieces.Remove(piece.GetInstanceID());
                    _m_placedPieces.Remove(piece);
                    _returnPieceToPool(piece.gameObject);
                }
            }
            
            _m_playerPieces[_playerId].Clear();
        }
        
        #endregion
    }
}