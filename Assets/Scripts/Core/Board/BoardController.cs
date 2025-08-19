using UnityEngine;
using System.Collections.Generic;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Events;

namespace BlokusGame.Core.Board
{
    /// <summary>
    /// 棋盘控制器 - 整合棋盘逻辑和视觉系统的统一控制器
    /// 作为棋盘系统的主要入口点，协调BoardSystem和BoardVisualizer
    /// 提供完整的棋盘功能，包括逻辑验证、视觉反馈和用户交互
    /// </summary>
    public class BoardController : MonoBehaviour, _IGameBoard
    {
        [Header("棋盘组件引用")]
        /// <summary>棋盘可视化组件</summary>
        [SerializeField] private BoardVisualizer _m_boardVisualizer;
        
        [Header("交互配置")]
        /// <summary>是否启用触摸交互</summary>
        [SerializeField] private bool _m_enableTouchInteraction = true;
        
        /// <summary>是否显示有效位置高亮</summary>
        [SerializeField] private bool _m_showValidPositionHighlights = true;
        
        /// <summary>是否显示方块预览</summary>
        [SerializeField] private bool _m_showPiecePreview = true;
        
        /// <summary>射线检测的摄像机</summary>
        [SerializeField] private Camera _m_raycastCamera;
        
        // 私有字段
        /// <summary>棋盘系统核心实例</summary>
        private BoardSystem _m_boardSystem;
        
        /// <summary>当前选中的方块</summary>
        private _IGamePiece _m_selectedPiece;
        
        /// <summary>当前操作的玩家ID</summary>
        private int _m_currentPlayerId = 1;
        
        /// <summary>鼠标/触摸上一帧的位置</summary>
        private Vector3 _m_lastInputPosition;
        
        /// <summary>是否正在拖拽方块</summary>
        private bool _m_isDraggingPiece = false;
        
        // 接口属性实现
        /// <summary>棋盘尺寸</summary>
        public int boardSize => _m_boardSystem?.boardSize ?? 20;
        
        /// <summary>
        /// Unity生命周期 - 初始化
        /// </summary>
        private void Awake()
        {
            // 创建棋盘系统实例
            _m_boardSystem = new BoardSystem();
            
            // 获取摄像机引用
            if (_m_raycastCamera == null)
            {
                _m_raycastCamera = Camera.main;
                if (_m_raycastCamera == null)
                {
                    _m_raycastCamera = FindObjectOfType<Camera>();
                }
            }
            
            // 获取可视化组件
            if (_m_boardVisualizer == null)
            {
                _m_boardVisualizer = GetComponent<BoardVisualizer>();
                if (_m_boardVisualizer == null)
                {
                    _m_boardVisualizer = gameObject.AddComponent<BoardVisualizer>();
                }
            }
        }
        
        /// <summary>
        /// Unity生命周期 - 开始
        /// </summary>
        private void Start()
        {
            // 初始化棋盘系统
            initializeBoard();
            
            // 设置可视化组件的棋盘系统引用
            _m_boardVisualizer.setBoardSystem(_m_boardSystem);
            
            // 订阅事件
            _subscribeToEvents();
        }
        
        /// <summary>
        /// Unity生命周期 - 更新
        /// </summary>
        private void Update()
        {
            if (_m_enableTouchInteraction)
            {
                _handleInput();
            }
        }
        
        /// <summary>
        /// Unity生命周期 - 清理
        /// </summary>
        private void OnDestroy()
        {
            _unsubscribeFromEvents();
        }
        
        // ==================== 公共接口实现 ====================
        
        /// <summary>
        /// 初始化棋盘
        /// </summary>
        public void initializeBoard()
        {
            _m_boardSystem.initializeBoard();
            Debug.Log("[BoardController] 棋盘控制器初始化完成");
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
            return _m_boardSystem.isValidPlacement(_piece, _position, _playerId);
        }
        
        /// <summary>
        /// 在棋盘上放置方块
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置的基准位置</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否放置成功</returns>
        public bool placePiece(_IGamePiece _piece, Vector2Int _position, int _playerId)
        {
            bool success = _m_boardSystem.placePiece(_piece, _position, _playerId);
            
            if (success)
            {
                // 触发视觉效果
                _m_boardVisualizer.playPiecePlacementAnimation(_piece, _position);
                
                // 清除选中状态
                _clearSelection();
            }
            
            return success;
        }
        
        /// <summary>
        /// 获取指定玩家的所有有效放置位置
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>有效放置位置列表</returns>
        public List<Vector2Int> getValidPlacements(_IGamePiece _piece, int _playerId)
        {
            return _m_boardSystem.getValidPlacements(_piece, _playerId);
        }
        
        /// <summary>
        /// 获取指定位置的占用状态
        /// </summary>
        /// <param name="_position">棋盘位置</param>
        /// <returns>占用该位置的玩家ID，0表示空位</returns>
        public int getPositionOwner(Vector2Int _position)
        {
            return _m_boardSystem.getPositionOwner(_position);
        }
        
        /// <summary>
        /// 检查位置是否在棋盘范围内
        /// </summary>
        /// <param name="_position">要检查的位置</param>
        /// <returns>是否在有效范围内</returns>
        public bool isPositionValid(Vector2Int _position)
        {
            return _m_boardSystem.isPositionValid(_position);
        }
        
        /// <summary>
        /// 获取指定玩家的起始角落位置
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>起始角落位置</returns>
        public Vector2Int getStartingCorner(int _playerId)
        {
            return _m_boardSystem.getStartingCorner(_playerId);
        }
        
        /// <summary>
        /// 检查玩家是否还有有效移动
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_availablePieces">玩家可用方块列表</param>
        /// <returns>是否还有有效移动</returns>
        public bool hasValidMoves(int _playerId, List<_IGamePiece> _availablePieces)
        {
            return _m_boardSystem.hasValidMoves(_playerId, _availablePieces);
        }
        
        /// <summary>
        /// 清空棋盘，重置所有位置
        /// </summary>
        public void clearBoard()
        {
            _m_boardSystem.clearBoard();
            _clearSelection();
        }
        
        /// <summary>
        /// 获取棋盘当前状态的副本
        /// </summary>
        /// <returns>棋盘状态数组</returns>
        public int[,] getBoardState()
        {
            return _m_boardSystem.getBoardState();
        }
        
        // ==================== 控制器特有功能 ====================
        
        /// <summary>
        /// 选择方块进行放置
        /// </summary>
        /// <param name="_piece">要选择的方块</param>
        /// <param name="_playerId">操作的玩家ID</param>
        public void selectPiece(_IGamePiece _piece, int _playerId)
        {
            _m_selectedPiece = _piece;
            _m_currentPlayerId = _playerId;
            
            if (_m_showValidPositionHighlights && _piece != null)
            {
                // 显示有效位置高亮
                List<Vector2Int> validPositions = getValidPlacements(_piece, _playerId);
                _m_boardVisualizer.highlightValidPositions(validPositions, _playerId);
            }
            
            Debug.Log($"[BoardController] 玩家 {_playerId} 选择了方块 {_piece?.pieceId}");
        }
        
        /// <summary>
        /// 取消选择方块
        /// </summary>
        public void deselectPiece()
        {
            _clearSelection();
        }
        
        /// <summary>
        /// 设置当前操作的玩家
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        public void setCurrentPlayer(int _playerId)
        {
            _m_currentPlayerId = _playerId;
        }
        
        /// <summary>
        /// 获取当前选中的方块
        /// </summary>
        /// <returns>当前选中的方块</returns>
        public _IGamePiece getSelectedPiece()
        {
            return _m_selectedPiece;
        }
        
        /// <summary>
        /// 获取棋盘系统实例
        /// </summary>
        /// <returns>棋盘系统实例</returns>
        public BoardSystem getBoardSystem()
        {
            return _m_boardSystem;
        }
        
        /// <summary>
        /// 获取棋盘可视化组件
        /// </summary>
        /// <returns>棋盘可视化组件</returns>
        public BoardVisualizer getBoardVisualizer()
        {
            return _m_boardVisualizer;
        }
        
        // ==================== 私有方法 ====================
        
        /// <summary>
        /// 订阅游戏事件
        /// </summary>
        private void _subscribeToEvents()
        {
            // 可以在这里订阅需要的事件
        }
        
        /// <summary>
        /// 取消事件订阅
        /// </summary>
        private void _unsubscribeFromEvents()
        {
            // 取消事件订阅
        }
        
        /// <summary>
        /// 处理输入（鼠标和触摸）
        /// </summary>
        private void _handleInput()
        {
            Vector3 inputPosition = Vector3.zero;
            bool inputPressed = false;
            bool inputHeld = false;
            bool inputReleased = false;
            
            // 处理鼠标输入（编辑器和PC）
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer || 
                Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.LinuxPlayer)
            {
                inputPosition = Input.mousePosition;
                inputPressed = Input.GetMouseButtonDown(0);
                inputHeld = Input.GetMouseButton(0);
                inputReleased = Input.GetMouseButtonUp(0);
            }
            // 处理触摸输入（移动设备）
            else if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                inputPosition = touch.position;
                inputPressed = touch.phase == TouchPhase.Began;
                inputHeld = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
                inputReleased = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
            }
            
            // 处理输入事件
            if (inputPressed)
            {
                _onInputPressed(inputPosition);
            }
            else if (inputHeld)
            {
                _onInputHeld(inputPosition);
            }
            else if (inputReleased)
            {
                _onInputReleased(inputPosition);
            }
        }
        
        /// <summary>
        /// 输入按下事件处理
        /// </summary>
        /// <param name="_inputPosition">输入位置（屏幕坐标）</param>
        private void _onInputPressed(Vector3 _inputPosition)
        {
            _m_lastInputPosition = _inputPosition;
            
            // 如果有选中的方块，开始拖拽
            if (_m_selectedPiece != null)
            {
                _m_isDraggingPiece = true;
            }
        }
        
        /// <summary>
        /// 输入持续事件处理
        /// </summary>
        /// <param name="_inputPosition">输入位置（屏幕坐标）</param>
        private void _onInputHeld(Vector3 _inputPosition)
        {
            if (_m_isDraggingPiece && _m_selectedPiece != null && _m_showPiecePreview)
            {
                // 将屏幕坐标转换为棋盘位置
                Vector2Int boardPosition = _screenToBoardPosition(_inputPosition);
                
                // 检查位置是否有效
                bool isValid = isValidPlacement(_m_selectedPiece, boardPosition, _m_currentPlayerId);
                
                // 显示预览
                _m_boardVisualizer.showPiecePreview(_m_selectedPiece, boardPosition, isValid);
            }
            
            _m_lastInputPosition = _inputPosition;
        }
        
        /// <summary>
        /// 输入释放事件处理
        /// </summary>
        /// <param name="_inputPosition">输入位置（屏幕坐标）</param>
        private void _onInputReleased(Vector3 _inputPosition)
        {
            if (_m_isDraggingPiece && _m_selectedPiece != null)
            {
                // 尝试在释放位置放置方块
                Vector2Int boardPosition = _screenToBoardPosition(_inputPosition);
                
                if (isValidPlacement(_m_selectedPiece, boardPosition, _m_currentPlayerId))
                {
                    placePiece(_m_selectedPiece, boardPosition, _m_currentPlayerId);
                }
                else
                {
                    // 放置失败，隐藏预览
                    _m_boardVisualizer.hidePiecePreview();
                }
            }
            
            _m_isDraggingPiece = false;
        }
        
        /// <summary>
        /// 屏幕坐标转换为棋盘坐标
        /// </summary>
        /// <param name="_screenPosition">屏幕坐标</param>
        /// <returns>棋盘坐标</returns>
        private Vector2Int _screenToBoardPosition(Vector3 _screenPosition)
        {
            if (_m_raycastCamera == null)
            {
                return Vector2Int.zero;
            }
            
            // 从屏幕坐标发射射线
            Ray ray = _m_raycastCamera.ScreenPointToRay(_screenPosition);
            
            // 与Y=0平面相交
            if (ray.direction.y != 0)
            {
                float distance = -ray.origin.y / ray.direction.y;
                Vector3 hitPoint = ray.origin + ray.direction * distance;
                
                // 转换为棋盘坐标
                int x = Mathf.RoundToInt(hitPoint.x);
                int z = Mathf.RoundToInt(hitPoint.z);
                
                return new Vector2Int(x, z);
            }
            
            return Vector2Int.zero;
        }
        
        /// <summary>
        /// 清除选择状态
        /// </summary>
        private void _clearSelection()
        {
            _m_selectedPiece = null;
            _m_isDraggingPiece = false;
            _m_boardVisualizer.clearHighlights();
            _m_boardVisualizer.hidePiecePreview();
        }
    }
}