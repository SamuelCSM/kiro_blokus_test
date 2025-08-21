using UnityEngine;
using System.Collections.Generic;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Events;
using BlokusGame.Core.Board;

namespace BlokusGame.Core.Managers
{
    /// <summary>
    /// 棋盘管理器 - 管理Blokus游戏棋盘的高级管理器
    /// 作为棋盘系统的统一入口点，整合BoardController和相关功能
    /// 负责棋盘的初始化、状态管理和与其他游戏系统的集成
    /// </summary>
    public class BoardManager : MonoBehaviour, _IGameBoard
    {
        /// <summary>单例实例</summary>
        public static BoardManager instance { get; private set; }
    
        [Header("棋盘控制器配置")]
        /// <summary>棋盘控制器实例</summary>
        [SerializeField] private BoardController _m_boardController;
        
        /// <summary>是否在启动时自动初始化棋盘</summary>
        [SerializeField] private bool _m_autoInitialize = true;
        
        /// <summary>棋盘控制器预制体（如果需要动态创建）</summary>
        [SerializeField] private GameObject _m_boardControllerPrefab;
        
        // 接口属性实现
        /// <summary>棋盘大小</summary>
        public int boardSize => _m_boardController?.boardSize ?? 20;
        
        /// <summary>
        /// Unity生命周期 - 在对象激活时初始化
        /// </summary>
        private void Awake()
        {
            // 单例模式实现
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // 获取或创建棋盘控制器
            if (_m_boardController == null)
            {
                _m_boardController = GetComponent<BoardController>();
                
                if (_m_boardController == null)
                {
                    if (_m_boardControllerPrefab != null)
                    {
                        GameObject controllerObj = Instantiate(_m_boardControllerPrefab, transform);
                        _m_boardController = controllerObj.GetComponent<BoardController>();
                    }
                    else
                    {
                        _m_boardController = gameObject.AddComponent<BoardController>();
                    }
                }
            }
        }
        
        /// <summary>
        /// Unity生命周期 - 在第一帧开始前初始化
        /// </summary>
        private void Start()
        {
            if (_m_autoInitialize)
            {
                initializeBoard();
            }
        }
        
        /// <summary>
        /// 初始化棋盘系统
        /// </summary>
        public void initializeBoard()
        {
            if (_m_boardController == null)
            {
                Debug.LogError("[BoardManager] 棋盘控制器未找到，无法初始化");
                return;
            }
            
            _m_boardController.initializeBoard();
            Debug.Log("[BoardManager] 棋盘管理器初始化完成");
            
            // 触发棋盘初始化事件
            GameEvents.onBoardInitialized?.Invoke();
        }
        
        /// <summary>
        /// 清空棋盘并重置状态
        /// </summary>
        public void clearBoard()
        {
            _m_boardController?.clearBoard();
            Debug.Log("[BoardManager] 棋盘已清空");
            
            // 触发棋盘清空事件
            GameEvents.onBoardCleared?.Invoke();
        }
        
        /// <summary>
        /// 检查指定位置是否可以放置方块
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置的基准位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否可以放置</returns>
        public bool isValidPlacement(_IGamePiece _piece, Vector2Int _position, int _playerId)
        {
            if (_m_boardController == null)
            {
                Debug.LogError("[BoardManager] 棋盘控制器未初始化");
                return false;
            }
            
            return _m_boardController.isValidPlacement(_piece, _position, _playerId);
        }
        
        /// <summary>
        /// 尝试在指定位置放置方块（包含验证）
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否放置成功</returns>
        public bool tryPlacePiece(_IGamePiece _piece, Vector2Int _position, int _playerId)
        {
            if (_m_boardController == null)
            {
                Debug.LogError("[BoardManager] 棋盘控制器未初始化");
                return false;
            }
            
            // 首先验证放置是否有效
            if (!isValidPlacement(_piece, _position, _playerId))
            {
                Debug.LogWarning($"[BoardManager] 玩家 {_playerId} 无法在位置 {_position} 放置方块");
                return false;
            }
            
            // 执行放置
            return placePiece(_piece, _position, _playerId);
        }
        
        /// <summary>
        /// 在指定位置放置方块
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否放置成功</returns>
        public bool placePiece(_IGamePiece _piece, Vector2Int _position, int _playerId)
        {
            if (_m_boardController == null)
            {
                Debug.LogError("[BoardManager] 棋盘控制器未初始化");
                return false;
            }
            
            bool success = _m_boardController.placePiece(_piece, _position, _playerId);
            
            if (success)
            {
                // 触发方块放置事件
                GameEvents.onPiecePlaced?.Invoke(_playerId, _piece, _position);
            }
            
            return success;
        }
        
        /// <summary>
        /// 获取指定位置的占用状态
        /// </summary>
        /// <param name="_position">棋盘位置</param>
        /// <returns>占用该位置的玩家ID，0表示空位</returns>
        public int getPositionOwner(Vector2Int _position)
        {
            if (_m_boardController == null)
            {
                Debug.LogError("[BoardManager] 棋盘控制器未初始化");
                return -1;
            }
            
            return _m_boardController.getPositionOwner(_position);
        }
        
        /// <summary>
        /// 获取指定玩家的起始角落位置
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>起始角落位置</returns>
        public Vector2Int getStartingCorner(int _playerId)
        {
            if (_m_boardController == null)
            {
                Debug.LogError("[BoardManager] 棋盘控制器未初始化");
                return Vector2Int.zero;
            }
            
            return _m_boardController.getStartingCorner(_playerId);
        }
        
        /// <summary>
        /// 获取指定玩家的所有有效放置位置
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>有效放置位置列表</returns>
        public List<Vector2Int> getValidPlacements(_IGamePiece _piece, int _playerId)
        {
            if (_m_boardController == null)
            {
                Debug.LogError("[BoardManager] 棋盘控制器未初始化");
                return new List<Vector2Int>();
            }
            
            return _m_boardController.getValidPlacements(_piece, _playerId);
        }
        
        /// <summary>
        /// 检查位置是否在棋盘范围内
        /// </summary>
        /// <param name="_position">要检查的位置</param>
        /// <returns>是否在有效范围内</returns>
        public bool isPositionValid(Vector2Int _position)
        {
            if (_m_boardController == null)
            {
                // 如果控制器未初始化，使用默认逻辑
                return _position.x >= 0 && _position.x < 20 && _position.y >= 0 && _position.y < 20;
            }
            
            return _m_boardController.isPositionValid(_position);
        }
        
        /// <summary>
        /// 获取棋盘当前状态的副本
        /// </summary>
        /// <returns>棋盘状态数组</returns>
        public int[,] getBoardState()
        {
            if (_m_boardController == null)
            {
                Debug.LogError("[BoardManager] 棋盘控制器未初始化");
                return new int[20, 20];
            }
            
            return _m_boardController.getBoardState();
        }
        
        /// <summary>
        /// 获取棋盘系统实例
        /// </summary>
        /// <returns>棋盘系统实例</returns>
        public BoardSystem getBoardSystem()
        {
            if (_m_boardController == null)
            {
                Debug.LogError("[BoardManager] 棋盘控制器未初始化");
                return null;
            }
            
            return _m_boardController.getBoardSystem();
        }
        
        /// <summary>
        /// 检查玩家是否还有有效移动
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_availablePieces">玩家可用方块列表</param>
        /// <returns>是否还有有效移动</returns>
        public bool hasValidMoves(int _playerId, List<_IGamePiece> _availablePieces)
        {
            if (_m_boardController == null)
            {
                Debug.LogError("[BoardManager] 棋盘控制器未初始化");
                return false;
            }
            
            return _m_boardController.hasValidMoves(_playerId, _availablePieces);
        }
        
        // ==================== 额外的管理器功能 ====================
        
        /// <summary>
        /// 保存棋盘状态到JSON字符串
        /// </summary>
        /// <returns>序列化的棋盘状态</returns>
        public string saveBoardState()
        {
            if (_m_boardController?.getBoardSystem() == null)
            {
                Debug.LogError("[BoardManager] 棋盘系统未初始化");
                return string.Empty;
            }
            
            return _m_boardController.getBoardSystem().serializeBoardState();
        }
        
        /// <summary>
        /// 从JSON字符串加载棋盘状态
        /// </summary>
        /// <param name="_jsonData">序列化的棋盘状态</param>
        /// <returns>是否加载成功</returns>
        public bool loadBoardState(string _jsonData)
        {
            if (_m_boardController == null)
            {
                Debug.LogError("[BoardManager] 棋盘控制器未初始化");
                return false;
            }
            
            BoardSystem boardSystem = _m_boardController.getBoardSystem();
            if (boardSystem == null)
            {
                Debug.LogError("[BoardManager] 棋盘系统未找到");
                return false;
            }
            
            return boardSystem.deserializeBoardState(_jsonData);
        }
        
        /// <summary>
        /// 选择方块进行放置（管理器级别的便捷方法）
        /// </summary>
        /// <param name="_piece">要选择的方块</param>
        /// <param name="_playerId">操作的玩家ID</param>
        public void selectPiece(_IGamePiece _piece, int _playerId)
        {
            _m_boardController?.selectPiece(_piece, _playerId);
        }
        
        /// <summary>
        /// 取消选择方块
        /// </summary>
        public void deselectPiece()
        {
            _m_boardController?.deselectPiece();
        }
        
        /// <summary>
        /// 设置当前操作的玩家
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        public void setCurrentPlayer(int _playerId)
        {
            _m_boardController?.setCurrentPlayer(_playerId);
        }
        
        /// <summary>
        /// 获取棋盘控制器的直接引用（用于高级操作）
        /// </summary>
        /// <returns>棋盘控制器实例</returns>
        public BoardController getBoardController()
        {
            return _m_boardController;
        }
        
        /// <summary>
        /// 检查指定位置是否被占用
        /// </summary>
        /// <param name="_position">要检查的位置</param>
        /// <returns>位置是否被占用</returns>
        public bool isPositionOccupied(Vector2Int _position)
        {
            return getPositionOwner(_position) != 0;
        }
        
        /// <summary>
        /// 获取指定位置的玩家ID
        /// </summary>
        /// <param name="_position">要检查的位置</param>
        /// <returns>占用该位置的玩家ID，0表示空位</returns>
        public int getPlayerAtPosition(Vector2Int _position)
        {
            return getPositionOwner(_position);
        }
    }
}