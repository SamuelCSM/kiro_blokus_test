using UnityEngine;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Events;

namespace BlokusGame.Core.Pieces
{
    /// <summary>
    /// 方块交互控制器 - 处理方块的触摸和拖拽交互
    /// 管理方块的选择、拖拽、旋转、翻转等用户交互操作
    /// 支持触摸设备和鼠标输入，提供流畅的交互体验
    /// </summary>
    public class PieceInteractionController : MonoBehaviour
    {
        [Header("交互配置")]
        /// <summary>是否启用拖拽功能</summary>
        [SerializeField] private bool _m_enableDragging = true;
        
        /// <summary>是否启用旋转功能</summary>
        [SerializeField] private bool _m_enableRotation = true;
        
        /// <summary>是否启用翻转功能</summary>
        [SerializeField] private bool _m_enableFlipping = true;
        
        /// <summary>双击检测的时间间隔（秒）</summary>
        [SerializeField] private float _m_doubleTapInterval = 0.3f;
        
        /// <summary>长按检测的时间（秒）</summary>
        [SerializeField] private float _m_longPressTime = 0.8f;
        
        /// <summary>拖拽开始的最小距离（像素）</summary>
        [SerializeField] private float _m_dragThreshold = 10f;
        
        [Header("视觉反馈")]
        /// <summary>拖拽时的高度偏移</summary>
        [SerializeField] private float _m_dragHeightOffset = 1f;
        
        /// <summary>拖拽时的透明度</summary>
        [SerializeField] [Range(0.1f, 1f)] private float _m_dragAlpha = 0.7f;
        
        // 私有字段
        /// <summary>关联的游戏方块组件</summary>
        private GamePiece _m_gamePiece;
        
        /// <summary>方块可视化组件</summary>
        private PieceVisualizer _m_pieceVisualizer;
        
        /// <summary>方块的碰撞器</summary>
        private Collider _m_collider;
        
        /// <summary>摄像机引用</summary>
        private Camera _m_camera;
        
        /// <summary>当前交互状态</summary>
        private InteractionState _m_currentState = InteractionState.Idle;
        
        /// <summary>是否正在拖拽</summary>
        private bool _m_isDragging = false;
        
        /// <summary>拖拽开始的屏幕位置</summary>
        private Vector2 _m_dragStartScreenPos;
        
        /// <summary>拖拽开始的世界位置</summary>
        private Vector3 _m_dragStartWorldPos;
        
        /// <summary>上次点击的时间</summary>
        private float _m_lastTapTime = 0f;
        
        /// <summary>长按开始的时间</summary>
        private float _m_longPressStartTime = 0f;
        
        /// <summary>是否正在长按</summary>
        private bool _m_isLongPressing = false;
        
        /// <summary>原始材质颜色（用于透明度调整）</summary>
        private Color _m_originalColor;
        
        /// <summary>交互状态枚举</summary>
        public enum InteractionState
        {
            /// <summary>空闲状态</summary>
            Idle,
            /// <summary>选中状态</summary>
            Selected,
            /// <summary>拖拽状态</summary>
            Dragging,
            /// <summary>已放置状态</summary>
            Placed
        }
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法 - 初始化组件引用
        /// </summary>
        private void Awake()
        {
            _m_gamePiece = GetComponent<GamePiece>();
            _m_pieceVisualizer = GetComponent<PieceVisualizer>();
            _m_collider = GetComponent<Collider>();
            
            if (_m_collider == null)
            {
                _m_collider = GetComponentInChildren<Collider>();
            }
            
            // 获取摄像机引用
            _m_camera = Camera.main;
            if (_m_camera == null)
            {
                _m_camera = FindObjectOfType<Camera>();
            }
        }
        
        /// <summary>
        /// Unity Start方法 - 初始化交互设置
        /// </summary>
        private void Start()
        {
            if (_m_gamePiece != null)
            {
                _m_originalColor = _m_gamePiece.pieceColor;
            }
        }
        
        /// <summary>
        /// Unity Update方法 - 处理输入检测
        /// </summary>
        private void Update()
        {
            if (_m_gamePiece != null && _m_gamePiece.isPlaced)
            {
                // 已放置的方块不响应交互
                return;
            }
            
            _handleInput();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 设置交互状态
        /// </summary>
        /// <param name="_state">新的交互状态</param>
        public void setInteractionState(InteractionState _state)
        {
            if (_m_currentState == _state) return;
            
            var oldState = _m_currentState;
            _m_currentState = _state;
            
            _onStateChanged(oldState, _state);
        }
        
        /// <summary>
        /// 启用或禁用交互
        /// </summary>
        /// <param name="_enabled">是否启用交互</param>
        public void setInteractionEnabled(bool _enabled)
        {
            if (_m_collider != null)
            {
                _m_collider.enabled = _enabled;
            }
            
            if (_m_pieceVisualizer != null)
            {
                _m_pieceVisualizer.setInteractive(_enabled);
            }
        }
        
        /// <summary>
        /// 强制停止当前交互
        /// </summary>
        public void stopInteraction()
        {
            if (_m_isDragging)
            {
                _endDrag();
            }
            
            _m_isLongPressing = false;
            setInteractionState(InteractionState.Idle);
        }
        
        /// <summary>
        /// 设置拖拽功能是否启用
        /// </summary>
        /// <param name="_enabled">是否启用拖拽</param>
        public void setDragEnabled(bool _enabled)
        {
            _m_enableDragging = _enabled;
        }
        
        /// <summary>
        /// 设置旋转功能是否启用
        /// </summary>
        /// <param name="_enabled">是否启用旋转</param>
        public void setRotationEnabled(bool _enabled)
        {
            _m_enableRotation = _enabled;
        }
        
        #endregion
        
        #region 私有方法 - 输入处理
        
        /// <summary>
        /// 处理输入检测
        /// </summary>
        private void _handleInput()
        {
            // 处理鼠标输入（编辑器和PC）
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer || 
                Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.LinuxPlayer)
            {
                _handleMouseInput();
            }
            // 处理触摸输入（移动设备）
            else if (Input.touchCount > 0)
            {
                _handleTouchInput();
            }
        }
        
        /// <summary>
        /// 处理鼠标输入
        /// </summary>
        private void _handleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _onPointerDown(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                _onPointerDrag(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _onPointerUp(Input.mousePosition);
            }
        }
        
        /// <summary>
        /// 处理触摸输入
        /// </summary>
        private void _handleTouchInput()
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _onPointerDown(touch.position);
                    break;
                    
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    _onPointerDrag(touch.position);
                    break;
                    
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    _onPointerUp(touch.position);
                    break;
            }
        }
        
        /// <summary>
        /// 指针按下事件处理
        /// </summary>
        /// <param name="_screenPosition">屏幕坐标</param>
        private void _onPointerDown(Vector2 _screenPosition)
        {
            // 检查是否点击到了这个方块
            if (!_isPointerOverPiece(_screenPosition)) return;
            
            _m_dragStartScreenPos = _screenPosition;
            _m_dragStartWorldPos = transform.position;
            _m_longPressStartTime = Time.time;
            _m_isLongPressing = true;
            
            // 检测双击
            float timeSinceLastTap = Time.time - _m_lastTapTime;
            if (timeSinceLastTap <= _m_doubleTapInterval)
            {
                _onDoubleTap();
            }
            
            _m_lastTapTime = Time.time;
            
            // 选中方块
            if (_m_currentState == InteractionState.Idle)
            {
                setInteractionState(InteractionState.Selected);
                _selectPiece();
            }
        }
        
        /// <summary>
        /// 指针拖拽事件处理
        /// </summary>
        /// <param name="_screenPosition">屏幕坐标</param>
        private void _onPointerDrag(Vector2 _screenPosition)
        {
            if (_m_currentState != InteractionState.Selected && _m_currentState != InteractionState.Dragging)
                return;
            
            // 检查是否达到拖拽阈值
            float dragDistance = Vector2.Distance(_screenPosition, _m_dragStartScreenPos);
            
            if (!_m_isDragging && dragDistance > _m_dragThreshold && _m_enableDragging)
            {
                _startDrag();
            }
            
            if (_m_isDragging)
            {
                _updateDrag(_screenPosition);
            }
            
            // 检查长按
            if (_m_isLongPressing && Time.time - _m_longPressStartTime >= _m_longPressTime)
            {
                _onLongPress();
                _m_isLongPressing = false;
            }
        }
        
        /// <summary>
        /// 指针抬起事件处理
        /// </summary>
        /// <param name="_screenPosition">屏幕坐标</param>
        private void _onPointerUp(Vector2 _screenPosition)
        {
            _m_isLongPressing = false;
            
            if (_m_isDragging)
            {
                _endDrag();
            }
            else if (_m_currentState == InteractionState.Selected)
            {
                // 简单点击，保持选中状态
            }
        }
        
        #endregion
        
        #region 私有方法 - 交互逻辑
        
        /// <summary>
        /// 检查指针是否在方块上
        /// </summary>
        /// <param name="_screenPosition">屏幕坐标</param>
        /// <returns>是否在方块上</returns>
        private bool _isPointerOverPiece(Vector2 _screenPosition)
        {
            if (_m_camera == null || _m_collider == null) return false;
            
            Ray ray = _m_camera.ScreenPointToRay(_screenPosition);
            return _m_collider.bounds.IntersectRay(ray);
        }
        
        /// <summary>
        /// 选中方块
        /// </summary>
        private void _selectPiece()
        {
            if (_m_gamePiece == null) return;
            
            // 更新可视化状态
            if (_m_pieceVisualizer != null)
            {
                _m_pieceVisualizer.setVisualState(PieceVisualizer.PieceVisualState.Selected);
            }
            
            // 触发选择事件
            GameEvents.instance.onPieceSelected?.Invoke(_m_gamePiece, _m_gamePiece.playerId);
            
            Debug.Log($"[PieceInteractionController] 选中方块 {_m_gamePiece.pieceId}");
        }
        
        /// <summary>
        /// 开始拖拽
        /// </summary>
        private void _startDrag()
        {
            _m_isDragging = true;
            setInteractionState(InteractionState.Dragging);
            
            // 调整方块位置和透明度
            Vector3 currentPos = transform.position;
            transform.position = new Vector3(currentPos.x, currentPos.y + _m_dragHeightOffset, currentPos.z);
            
            // 调整透明度
            if (_m_pieceVisualizer != null)
            {
                Color dragColor = _m_originalColor;
                dragColor.a = _m_dragAlpha;
                _m_pieceVisualizer.updateColor(dragColor);
            }
            
            Debug.Log($"[PieceInteractionController] 开始拖拽方块 {_m_gamePiece?.pieceId}");
        }
        
        /// <summary>
        /// 更新拖拽位置
        /// </summary>
        /// <param name="_screenPosition">屏幕坐标</param>
        private void _updateDrag(Vector2 _screenPosition)
        {
            if (_m_camera == null) return;
            
            // 将屏幕坐标转换为世界坐标
            Vector3 worldPosition = _screenToWorldPosition(_screenPosition);
            worldPosition.y = _m_dragStartWorldPos.y + _m_dragHeightOffset;
            
            transform.position = worldPosition;
        }
        
        /// <summary>
        /// 结束拖拽
        /// </summary>
        private void _endDrag()
        {
            _m_isDragging = false;
            
            // 恢复透明度
            if (_m_pieceVisualizer != null)
            {
                _m_pieceVisualizer.updateColor(_m_originalColor);
            }
            
            // 尝试放置方块
            Vector3 currentPos = transform.position;
            Vector2Int boardPosition = _worldToBoardPosition(currentPos);
            
            // 这里应该调用棋盘管理器来验证和放置方块
            // 暂时回到原位置
            transform.position = _m_dragStartWorldPos;
            setInteractionState(InteractionState.Selected);
            
            Debug.Log($"[PieceInteractionController] 结束拖拽方块 {_m_gamePiece?.pieceId}，尝试放置在 {boardPosition}");
        }
        
        /// <summary>
        /// 双击事件处理
        /// </summary>
        private void _onDoubleTap()
        {
            if (!_m_enableRotation || _m_gamePiece == null) return;
            
            // 旋转方块
            _m_gamePiece.rotate90Clockwise();
            
            // 播放旋转动画
            if (_m_pieceVisualizer != null)
            {
                Quaternion targetRotation = transform.localRotation * Quaternion.Euler(0, 90, 0);
                _m_pieceVisualizer.playRotationAnimation(targetRotation);
            }
            
            // 触发旋转事件
            GameEvents.instance.onPieceRotated?.Invoke(_m_gamePiece, _m_gamePiece.playerId);
            
            Debug.Log($"[PieceInteractionController] 旋转方块 {_m_gamePiece.pieceId}");
        }
        
        /// <summary>
        /// 长按事件处理
        /// </summary>
        private void _onLongPress()
        {
            if (!_m_enableFlipping || _m_gamePiece == null) return;
            
            // 翻转方块
            _m_gamePiece.flipHorizontal();
            
            // 触发翻转事件
            GameEvents.instance.onPieceFlipped?.Invoke(_m_gamePiece, _m_gamePiece.playerId);
            
            Debug.Log($"[PieceInteractionController] 翻转方块 {_m_gamePiece.pieceId}");
        }
        
        /// <summary>
        /// 状态变化处理
        /// </summary>
        /// <param name="_oldState">旧状态</param>
        /// <param name="_newState">新状态</param>
        private void _onStateChanged(InteractionState _oldState, InteractionState _newState)
        {
            Debug.Log($"[PieceInteractionController] 方块 {_m_gamePiece?.pieceId} 状态变化: {_oldState} -> {_newState}");
            
            // 根据状态变化更新可视化
            if (_m_pieceVisualizer != null)
            {
                PieceVisualizer.PieceVisualState visualState = PieceVisualizer.PieceVisualState.Normal;
                
                switch (_newState)
                {
                    case InteractionState.Selected:
                        visualState = PieceVisualizer.PieceVisualState.Selected;
                        break;
                    case InteractionState.Placed:
                        visualState = PieceVisualizer.PieceVisualState.Placed;
                        break;
                }
                
                _m_pieceVisualizer.setVisualState(visualState);
            }
        }
        
        #endregion
        
        #region 私有方法 - 坐标转换
        
        /// <summary>
        /// 屏幕坐标转换为世界坐标
        /// </summary>
        /// <param name="_screenPosition">屏幕坐标</param>
        /// <returns>世界坐标</returns>
        private Vector3 _screenToWorldPosition(Vector2 _screenPosition)
        {
            if (_m_camera == null) return Vector3.zero;
            
            // 使用射线投射到Y=0平面
            Ray ray = _m_camera.ScreenPointToRay(_screenPosition);
            
            if (ray.direction.y != 0)
            {
                float distance = -ray.origin.y / ray.direction.y;
                return ray.origin + ray.direction * distance;
            }
            
            return Vector3.zero;
        }
        
        /// <summary>
        /// 世界坐标转换为棋盘坐标
        /// </summary>
        /// <param name="_worldPosition">世界坐标</param>
        /// <returns>棋盘坐标</returns>
        private Vector2Int _worldToBoardPosition(Vector3 _worldPosition)
        {
            // 假设棋盘格子大小为1单位
            int x = Mathf.RoundToInt(_worldPosition.x);
            int z = Mathf.RoundToInt(_worldPosition.z);
            
            return new Vector2Int(x, z);
        }
        
        #endregion
    }
}