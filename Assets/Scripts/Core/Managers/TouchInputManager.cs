using UnityEngine;
using System.Collections.Generic;
using BlokusGame.Core.Events;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Pieces;
using BlokusGame.Core.InputSystem;
using BlokusGame.Core.Board;

namespace BlokusGame.Core.Managers
{
    /// <summary>
    /// 触摸输入管理器 - 处理移动设备的触摸输入
    /// 支持单点触摸拖拽、双击旋转、长按翻转等操作
    /// 提供触摸反馈和视觉提示功能
    /// 兼容鼠标输入用于编辑器测试
    /// </summary>
    public class TouchInputManager : MonoBehaviour
    {
        [Header("触摸配置")]
        /// <summary>是否启用触摸输入</summary>
        [SerializeField] private bool _m_enableTouchInput = true;
        
        /// <summary>是否启用鼠标输入（编辑器测试用）</summary>
        [SerializeField] private bool _m_enableMouseInput = true;
        
        /// <summary>双击检测的时间间隔（秒）</summary>
        [SerializeField] private float _m_doubleTapInterval = 0.3f;
        
        /// <summary>长按检测的时间（秒）</summary>
        [SerializeField] private float _m_longPressTime = 0.8f;
        
        /// <summary>拖拽开始的最小距离（像素）</summary>
        [SerializeField] private float _m_dragThreshold = 20f;
        
        /// <summary>触摸有效检测半径（像素）</summary>
        [SerializeField] private float _m_touchRadius = 50f;
        
        [Header("多点触摸配置")]
        /// <summary>是否启用多点触摸</summary>
        [SerializeField] private bool _m_enableMultiTouch = true;
        
        /// <summary>缩放手势的最小距离变化</summary>
        [SerializeField] private float _m_pinchThreshold = 10f;
        
        /// <summary>缩放敏感度</summary>
        [SerializeField] private float _m_pinchSensitivity = 1f;
        
        [Header("反馈配置")]
        /// <summary>是否启用触觉反馈</summary>
        [SerializeField] private bool _m_enableHapticFeedback = true;
        
        /// <summary>是否启用视觉反馈</summary>
        [SerializeField] private bool _m_enableVisualFeedback = true;
        
        /// <summary>触摸反馈持续时间</summary>
        [SerializeField] private float _m_feedbackDuration = 0.1f;
        
        /// <summary>触摸反馈系统组件</summary>
        [SerializeField] private TouchFeedbackSystem _m_feedbackSystem;
        
        // 私有字段
        /// <summary>摄像机引用</summary>
        private Camera _m_camera;
        
        /// <summary>当前触摸状态</summary>
        private TouchState _m_currentTouchState = TouchState.None;
        
        /// <summary>当前选中的方块</summary>
        private GamePiece _m_selectedPiece;
        
        /// <summary>当前拖拽的方块</summary>
        private GamePiece _m_draggingPiece;
        
        /// <summary>触摸开始位置</summary>
        private Vector2 _m_touchStartPosition;
        
        /// <summary>触摸开始时间</summary>
        private float _m_touchStartTime;
        
        /// <summary>上次点击时间</summary>
        private float _m_lastTapTime;
        
        /// <summary>上次点击的方块</summary>
        private GamePiece _m_lastTappedPiece;
        
        /// <summary>是否正在长按</summary>
        private bool _m_isLongPressing;
        
        /// <summary>长按开始时间</summary>
        private float _m_longPressStartTime;
        
        /// <summary>多点触摸数据</summary>
        private Dictionary<int, TouchData> _m_activeTouches = new Dictionary<int, TouchData>();
        
        /// <summary>上一帧的双指距离（用于缩放检测）</summary>
        private float _m_lastPinchDistance;
        
        /// <summary>上一帧的双指角度（用于旋转检测）</summary>
        private float _m_lastPinchAngle;
        
        /// <summary>触摸游戏逻辑集成系统</summary>
        private TouchGameplayIntegration _m_gameplayIntegration;
        
        /// <summary>上次触摸事件处理时间</summary>
        private float _m_lastTouchEventTime;
        
        /// <summary>当前帧触摸事件计数</summary>
        private int _m_currentFrameTouchEvents;
        
        /// <summary>触摸事件缓存队列</summary>
        private Queue<TouchEventData> _m_touchEventCache;
        
        /// <summary>防误触状态标记</summary>
        private bool _m_isInAntiMistouchMode = false;
        
        /// <summary>上次有效触摸时间</summary>
        private float _m_lastValidTouchTime;
        
        /// <summary>旋转手势的最小角度变化</summary>
        [SerializeField] private float _m_rotationThreshold = 5f;
        
        /// <summary>旋转敏感度</summary>
        [SerializeField] private float _m_rotationSensitivity = 1f;
        
        /// <summary>边缘滑动检测区域宽度（像素）</summary>
        [SerializeField] private float _m_edgeSwipeZone = 100f;
        
        /// <summary>边缘滑动的最小距离</summary>
        [SerializeField] private float _m_edgeSwipeThreshold = 50f;
        
        [Header("防误触配置")]
        /// <summary>同时触摸的最大数量</summary>
        [SerializeField] private int _m_maxSimultaneousTouches = 5;
        
        /// <summary>触摸间隔最小时间（防止过快连续触摸）</summary>
        [SerializeField] private float _m_minTouchInterval = 0.05f;
        
        /// <summary>无效触摸区域边界（屏幕边缘像素）</summary>
        [SerializeField] private float _m_invalidTouchBorder = 20f;
        
        /// <summary>触摸压力阈值（如果设备支持）</summary>
        [SerializeField] private float _m_pressureThreshold = 0.1f;
        
        [Header("性能优化配置")]
        /// <summary>触摸事件处理频率限制（每秒最大次数）</summary>
        [SerializeField] private int _m_maxTouchEventsPerSecond = 60;
        
        /// <summary>是否启用触摸事件缓存</summary>
        [SerializeField] private bool _m_enableTouchEventCaching = true;
        
        /// <summary>
        /// 触摸反馈类型枚举
        /// </summary>
        private enum TouchFeedbackType
        {
            /// <summary>普通点击</summary>
            Tap,
            /// <summary>成功操作</summary>
            Success,
            /// <summary>错误操作</summary>
            Error
        }
        
        /// <summary>触摸状态枚举</summary>
        public enum TouchState
        {
            /// <summary>无触摸</summary>
            None,
            /// <summary>单点触摸</summary>
            SingleTouch,
            /// <summary>拖拽中</summary>
            Dragging,
            /// <summary>多点触摸</summary>
            MultiTouch,
            /// <summary>缩放手势</summary>
            Pinching
        }
        
        /// <summary>
        /// 触摸数据结构
        /// </summary>
        private struct TouchData
        {
            /// <summary>触摸ID</summary>
            public int fingerId;
            /// <summary>触摸位置</summary>
            public Vector2 position;
            /// <summary>触摸开始位置</summary>
            public Vector2 startPosition;
            /// <summary>触摸开始时间</summary>
            public float startTime;
            /// <summary>触摸的方块</summary>
            public GamePiece touchedPiece;
            /// <summary>触摸压力（如果支持）</summary>
            public float pressure;
            /// <summary>是否为有效触摸</summary>
            public bool isValid;
        }
        
        /// <summary>
        /// 触摸事件数据结构（用于缓存）
        /// </summary>
        private struct TouchEventData
        {
            /// <summary>事件类型</summary>
            public TouchEventType eventType;
            /// <summary>触摸数据</summary>
            public TouchData touchData;
            /// <summary>事件时间戳</summary>
            public float timestamp;
        }
        
        /// <summary>
        /// 触摸事件类型枚举
        /// </summary>
        private enum TouchEventType
        {
            /// <summary>触摸开始</summary>
            Began,
            /// <summary>触摸移动</summary>
            Moved,
            /// <summary>触摸结束</summary>
            Ended,
            /// <summary>触摸取消</summary>
            Canceled
        }
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法 - 初始化组件引用
        /// </summary>
        private void Awake()
        {
            _m_camera = Camera.main;
            if (_m_camera == null)
            {
                _m_camera = FindObjectOfType<Camera>();
            }
            
            if (_m_camera == null)
            {
                Debug.LogError("[TouchInputManager] 未找到摄像机，触摸输入可能无法正常工作");
            }
            
            // 初始化反馈系统
            if (_m_feedbackSystem == null)
            {
                _m_feedbackSystem = GetComponent<TouchFeedbackSystem>();
                if (_m_feedbackSystem == null)
                {
                    _m_feedbackSystem = gameObject.AddComponent<TouchFeedbackSystem>();
                }
            }
            
            // 初始化游戏逻辑集成系统
            _m_gameplayIntegration = GetComponent<TouchGameplayIntegration>();
            if (_m_gameplayIntegration == null)
            {
                _m_gameplayIntegration = gameObject.AddComponent<TouchGameplayIntegration>();
            }
            
            // 初始化触摸事件缓存
            if (_m_enableTouchEventCaching)
            {
                _m_touchEventCache = new Queue<TouchEventData>();
            }
        }
        
        /// <summary>
        /// Unity Start方法 - 初始化触摸输入系统
        /// </summary>
        private void Start()
        {
            _initializeTouchInput();
        }
        
        /// <summary>
        /// Unity Update方法 - 处理触摸输入
        /// </summary>
        private void Update()
        {
            if (!_m_enableTouchInput && !_m_enableMouseInput) return;
            
            // 重置帧计数器
            _m_currentFrameTouchEvents = 0;
            
            _handleInput();
            _updateTouchState();
            _processAntiMistouch();
            
            // 处理缓存的触摸事件
            if (_m_enableTouchEventCaching)
            {
                _processCachedTouchEvents();
            }
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 清理资源
        /// </summary>
        private void OnDestroy()
        {
            _cleanup();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 设置输入是否启用
        /// </summary>
        /// <param name="_enabled">是否启用输入</param>
        public void setInputEnabled(bool _enabled)
        {
            _m_enableTouchInput = _enabled;
            _m_enableMouseInput = _enabled;
            
            if (!_enabled)
            {
                _resetCurrentTouch();
            }
        }
        
        /// <summary>
        /// 设置触摸输入是否启用
        /// </summary>
        /// <param name="_enabled">是否启用触摸输入</param>
        public void setTouchInputEnabled(bool _enabled)
        {
            _m_enableTouchInput = _enabled;
            
            if (!_enabled)
            {
                _resetCurrentTouch();
            }
        }
        
        /// <summary>
        /// 设置鼠标输入是否启用
        /// </summary>
        /// <param name="_enabled">是否启用鼠标输入</param>
        public void setMouseInputEnabled(bool _enabled)
        {
            _m_enableMouseInput = _enabled;
        }
        
        /// <summary>
        /// 重置输入状态
        /// </summary>
        public void resetInput()
        {
            _resetCurrentTouch();
            _m_activeTouches.Clear();
            _m_currentTouchState = TouchState.None;
        }
        
        /// <summary>
        /// 获取当前触摸状态
        /// </summary>
        /// <returns>当前触摸状态</returns>
        public TouchState getCurrentTouchState()
        {
            return _m_currentTouchState;
        }
        
        /// <summary>
        /// 获取当前选中的方块
        /// </summary>
        /// <returns>当前选中的方块</returns>
        public GamePiece getSelectedPiece()
        {
            return _m_selectedPiece;
        }
        
        #endregion
        
        #region 私有方法 - 初始化和清理
        
        /// <summary>
        /// 初始化触摸输入系统
        /// </summary>
        private void _initializeTouchInput()
        {
            // 设置多点触摸
            UnityEngine.Input.multiTouchEnabled = _m_enableMultiTouch;
            
            Debug.Log("[TouchInputManager] 触摸输入系统初始化完成");
            Debug.Log($"[TouchInputManager] 多点触摸: {_m_enableMultiTouch}");
            Debug.Log($"[TouchInputManager] 触摸输入: {_m_enableTouchInput}");
            Debug.Log($"[TouchInputManager] 鼠标输入: {_m_enableMouseInput}");
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        private void _cleanup()
        {
            _resetCurrentTouch();
            _m_activeTouches.Clear();
        }
        
        #endregion
        
        #region 私有方法 - 输入处理
        
        /// <summary>
        /// 处理输入
        /// </summary>
        private void _handleInput()
        {
            // 处理触摸输入
            if (_m_enableTouchInput && UnityEngine.Input.touchCount > 0)
            {
                _handleTouchInput();
            }
            // 处理鼠标输入（编辑器和PC平台）
            else if (_m_enableMouseInput && _shouldHandleMouseInput())
            {
                _handleMouseInput();
            }
        }
        
        /// <summary>
        /// 判断是否应该处理鼠标输入
        /// </summary>
        /// <returns>是否处理鼠标输入</returns>
        private bool _shouldHandleMouseInput()
        {
            return Application.isEditor || 
                   Application.platform == RuntimePlatform.WindowsPlayer ||
                   Application.platform == RuntimePlatform.OSXPlayer ||
                   Application.platform == RuntimePlatform.LinuxPlayer;
        }
        
        /// <summary>
        /// 处理触摸输入
        /// </summary>
        private void _handleTouchInput()
        {
            // 处理单点触摸
            if (UnityEngine.Input.touchCount == 1)
            {
                _handleSingleTouch(UnityEngine.Input.GetTouch(0));
            }
            // 处理多点触摸
            else if (UnityEngine.Input.touchCount > 1 && _m_enableMultiTouch)
            {
                _handleMultiTouch();
            }
        }
        
        /// <summary>
        /// 处理鼠标输入
        /// </summary>
        private void _handleMouseInput()
        {
            // 模拟触摸输入
            Touch simulatedTouch = new Touch();
            
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                simulatedTouch.phase = TouchPhase.Began;
                simulatedTouch.position = UnityEngine.Input.mousePosition;
                simulatedTouch.fingerId = 0;
                _handleSingleTouch(simulatedTouch);
            }
            else if (UnityEngine.Input.GetMouseButton(0))
            {
                simulatedTouch.phase = TouchPhase.Moved;
                simulatedTouch.position = UnityEngine.Input.mousePosition;
                simulatedTouch.fingerId = 0;
                _handleSingleTouch(simulatedTouch);
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                simulatedTouch.phase = TouchPhase.Ended;
                simulatedTouch.position = UnityEngine.Input.mousePosition;
                simulatedTouch.fingerId = 0;
                _handleSingleTouch(simulatedTouch);
            }
        }
        
        /// <summary>
        /// 处理单点触摸
        /// </summary>
        /// <param name="_touch">触摸数据</param>
        private void _handleSingleTouch(Touch _touch)
        {
            // 性能限制检查
            if (!_canProcessTouchEvent()) return;
            
            // 防误触检查
            if (!_isValidTouch(_touch)) return;
            
            switch (_touch.phase)
            {
                case TouchPhase.Began:
                    _onTouchBegan(_touch);
                    break;
                    
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    _onTouchMoved(_touch);
                    break;
                    
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    _onTouchEnded(_touch);
                    break;
            }
            
            _m_currentFrameTouchEvents++;
        }
        
        /// <summary>
        /// 处理多点触摸
        /// </summary>
        private void _handleMultiTouch()
        {
            if (UnityEngine.Input.touchCount == 2)
            {
                _handlePinchGesture();
                _handleRotationGesture();
                _handlePanGesture();
            }
            
            _m_currentTouchState = TouchState.MultiTouch;
        }
        
        /// <summary>
        /// 处理平移手势
        /// </summary>
        private void _handlePanGesture()
        {
            Touch touch1 = UnityEngine.Input.GetTouch(0);
            Touch touch2 = UnityEngine.Input.GetTouch(1);
            
            // 计算两个触摸点的中心位置
            Vector2 currentCenter = (touch1.position + touch2.position) * 0.5f;
            
            // 如果有存储的中心位置，计算平移
            if (_m_activeTouches.ContainsKey(-1)) // 使用-1作为双指中心的特殊ID
            {
                Vector2 lastCenter = _m_activeTouches[-1].position;
                Vector2 panDelta = currentCenter - lastCenter;
                
                // 触发棋盘平移
                _triggerBoardPan(panDelta);
            }
            
            // 更新中心位置
            TouchData centerData = new TouchData
            {
                fingerId = -1,
                position = currentCenter,
                startPosition = currentCenter,
                startTime = Time.time
            };
            _m_activeTouches[-1] = centerData;
        }
        
        /// <summary>
        /// 触发棋盘平移
        /// </summary>
        /// <param name="_panDelta">平移增量</param>
        private void _triggerBoardPan(Vector2 _panDelta)
        {
            // 获取棋盘控制器并应用平移
            var boardController = FindObjectOfType<BoardController>();
            if (boardController != null)
            {
                var boardVisualizer = boardController.getBoardVisualizer();
                if (boardVisualizer != null)
                {
                    // 将屏幕坐标转换为世界坐标偏移
                    Vector3 worldPanDelta = new Vector3(-_panDelta.x * 0.01f, 0, -_panDelta.y * 0.01f);
                    
                    // 应用平移（这里需要BoardVisualizer支持获取和设置平移偏移）
                    Debug.Log($"[TouchInputManager] 棋盘平移: {worldPanDelta}");
                }
            }
        }
        
        /// <summary>
        /// 处理旋转手势
        /// </summary>
        private void _handleRotationGesture()
        {
            Touch touch1 = UnityEngine.Input.GetTouch(0);
            Touch touch2 = UnityEngine.Input.GetTouch(1);
            
            // 计算两个触摸点之间的角度
            Vector2 direction = touch2.position - touch1.position;
            float currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            if (_m_lastPinchAngle != 0)
            {
                float deltaAngle = Mathf.DeltaAngle(_m_lastPinchAngle, currentAngle);
                
                if (Mathf.Abs(deltaAngle) > _m_rotationThreshold)
                {
                    // 触发棋盘旋转
                    _triggerBoardRotation(deltaAngle * _m_rotationSensitivity);
                    
                    Debug.Log($"[TouchInputManager] 旋转手势，角度变化: {deltaAngle}");
                }
            }
            
            _m_lastPinchAngle = currentAngle;
        }
        
        /// <summary>
        /// 触发棋盘旋转
        /// </summary>
        /// <param name="_deltaAngle">角度变化</param>
        private void _triggerBoardRotation(float _deltaAngle)
        {
            // 获取棋盘控制器并应用旋转
            var boardController = FindObjectOfType<BoardController>();
            if (boardController != null)
            {
                // 这里可以实现棋盘旋转功能
                Debug.Log($"[TouchInputManager] 棋盘旋转: {_deltaAngle} 度");
            }
        }
        
        #endregion
        
        #region 私有方法 - 触摸事件处理
        
        /// <summary>
        /// 触摸开始事件处理
        /// </summary>
        /// <param name="_touch">触摸数据</param>
        private void _onTouchBegan(Touch _touch)
        {
            _m_touchStartPosition = _touch.position;
            _m_touchStartTime = Time.time;
            _m_isLongPressing = true;
            _m_longPressStartTime = Time.time;
            
            // 检测触摸的方块
            GamePiece touchedPiece = _getTouchedPiece(_touch.position);
            
            if (touchedPiece != null)
            {
                _handlePieceTouch(touchedPiece, _touch);
                // 显示成功触摸反馈
                _showVisualFeedback(_touch.position, TouchFeedbackType.Success);
            }
            else
            {
                // 显示普通触摸反馈
                _showVisualFeedback(_touch.position, TouchFeedbackType.Tap);
            }
            
            _m_currentTouchState = TouchState.SingleTouch;
            
            // 播放触觉反馈和视觉反馈
            if (_m_feedbackSystem != null)
            {
                if (_m_enableHapticFeedback)
                {
                    _m_feedbackSystem.playHapticFeedback(TouchFeedbackSystem.FeedbackType.Light);
                }
                
                if (_m_enableVisualFeedback)
                {
                    _m_feedbackSystem.showTouchPoint(_touch.position, _touch.fingerId);
                    _m_feedbackSystem.showRippleEffect(_touch.position, 1f);
                }
            }
        }
        
        /// <summary>
        /// 触摸移动事件处理
        /// </summary>
        /// <param name="_touch">触摸数据</param>
        private void _onTouchMoved(Touch _touch)
        {
            if (_m_currentTouchState == TouchState.SingleTouch)
            {
                float dragDistance = Vector2.Distance(_touch.position, _m_touchStartPosition);
                
                // 检查是否达到拖拽阈值
                if (dragDistance > _m_dragThreshold && _m_selectedPiece != null)
                {
                    _startDragging(_m_selectedPiece, _touch);
                }
            }
            else if (_m_currentTouchState == TouchState.Dragging)
            {
                _updateDragging(_touch);
            }
            
            // 检查长按
            if (_m_isLongPressing && Time.time - _m_longPressStartTime >= _m_longPressTime)
            {
                _onLongPress();
                _m_isLongPressing = false;
            }
        }
        
        /// <summary>
        /// 触摸结束事件处理
        /// </summary>
        /// <param name="_touch">触摸数据</param>
        private void _onTouchEnded(Touch _touch)
        {
            _m_isLongPressing = false;
            
            if (_m_currentTouchState == TouchState.Dragging)
            {
                _endDragging(_touch);
            }
            else if (_m_currentTouchState == TouchState.SingleTouch)
            {
                // 检查是否是边缘滑动
                if (_isEdgeSwipe(_touch))
                {
                    _handleEdgeSwipe(_touch);
                }
                else
                {
                    _handleTap(_touch);
                }
            }
            
            _m_currentTouchState = TouchState.None;
        }
        
        /// <summary>
        /// 检查是否是边缘滑动
        /// </summary>
        /// <param name="_touch">触摸数据</param>
        /// <returns>是否是边缘滑动</returns>
        private bool _isEdgeSwipe(Touch _touch)
        {
            // 检查触摸开始位置是否在边缘区域
            bool startAtEdge = _m_touchStartPosition.x < _m_edgeSwipeZone || 
                              _m_touchStartPosition.x > Screen.width - _m_edgeSwipeZone ||
                              _m_touchStartPosition.y < _m_edgeSwipeZone || 
                              _m_touchStartPosition.y > Screen.height - _m_edgeSwipeZone;
            
            // 检查滑动距离是否足够
            float swipeDistance = Vector2.Distance(_touch.position, _m_touchStartPosition);
            
            return startAtEdge && swipeDistance > _m_edgeSwipeThreshold;
        }
        
        /// <summary>
        /// 处理边缘滑动
        /// </summary>
        /// <param name="_touch">触摸数据</param>
        private void _handleEdgeSwipe(Touch _touch)
        {
            Vector2 swipeDirection = (_touch.position - _m_touchStartPosition).normalized;
            
            // 判断滑动方向
            if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
            {
                // 水平滑动
                if (swipeDirection.x > 0)
                {
                    Debug.Log("[TouchInputManager] 右边缘滑动 - 可以用于切换UI面板");
                    // 触发右滑事件
                }
                else
                {
                    Debug.Log("[TouchInputManager] 左边缘滑动 - 可以用于返回上一级");
                    // 触发左滑事件
                }
            }
            else
            {
                // 垂直滑动
                if (swipeDirection.y > 0)
                {
                    Debug.Log("[TouchInputManager] 上边缘滑动 - 可以用于显示菜单");
                    // 触发上滑事件
                }
                else
                {
                    Debug.Log("[TouchInputManager] 下边缘滑动 - 可以用于隐藏UI");
                    // 触发下滑事件
                }
            }
        }
        
        #endregion
        
        #region 私有方法 - 方块交互
        
        /// <summary>
        /// 获取触摸位置的方块
        /// </summary>
        /// <param name="_screenPosition">屏幕坐标</param>
        /// <returns>触摸到的方块，如果没有则返回null</returns>
        private GamePiece _getTouchedPiece(Vector2 _screenPosition)
        {
            if (_m_camera == null) return null;
            
            Ray ray = _m_camera.ScreenPointToRay(_screenPosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                GamePiece piece = hit.collider.GetComponent<GamePiece>();
                if (piece == null)
                {
                    piece = hit.collider.GetComponentInParent<GamePiece>();
                }
                
                return piece;
            }
            
            return null;
        }
        
        /// <summary>
        /// 处理方块触摸
        /// </summary>
        /// <param name="_piece">被触摸的方块</param>
        /// <param name="_touch">触摸数据</param>
        private void _handlePieceTouch(GamePiece _piece, Touch _touch)
        {
            // 检查方块是否可以交互
            if (_piece.isPlaced)
            {
                Debug.Log($"[TouchInputManager] 方块 {_piece.pieceId} 已放置，无法交互");
                return;
            }
            
            // 选中方块
            _selectPiece(_piece);
            
            // 检测双击
            float timeSinceLastTap = Time.time - _m_lastTapTime;
            if (_m_lastTappedPiece == _piece && timeSinceLastTap <= _m_doubleTapInterval)
            {
                _onDoubleTap(_piece);
            }
            
            _m_lastTapTime = Time.time;
            _m_lastTappedPiece = _piece;
        }
        
        /// <summary>
        /// 选中方块
        /// </summary>
        /// <param name="_piece">要选中的方块</param>
        private void _selectPiece(GamePiece _piece)
        {
            if (_m_selectedPiece == _piece) return;
            
            // 取消之前选中的方块
            if (_m_selectedPiece != null)
            {
                _deselectPiece(_m_selectedPiece);
            }
            
            _m_selectedPiece = _piece;
            
            // 触发选择事件
            GameEvents.instance.onPieceSelected?.Invoke(_piece, _piece.playerId);
            
            Debug.Log($"[TouchInputManager] 选中方块 {_piece.pieceId}");
        }
        
        /// <summary>
        /// 取消选中方块
        /// </summary>
        /// <param name="_piece">要取消选中的方块</param>
        private void _deselectPiece(GamePiece _piece)
        {
            // 这里可以添加取消选中的视觉效果
            Debug.Log($"[TouchInputManager] 取消选中方块 {_piece.pieceId}");
        }
        
        /// <summary>
        /// 开始拖拽
        /// </summary>
        /// <param name="_piece">要拖拽的方块</param>
        /// <param name="_touch">触摸数据</param>
        private void _startDragging(GamePiece _piece, Touch _touch)
        {
            _m_draggingPiece = _piece;
            _m_currentTouchState = TouchState.Dragging;
            
            // 开始拖拽轨迹
            if (_m_feedbackSystem != null && _m_enableVisualFeedback)
            {
                _m_feedbackSystem.startDragTrail(_touch.position, _touch.fingerId);
            }
            
            // 触发拖拽开始事件
            GameEvents.instance.onPieceDragStart?.Invoke(_piece);
            
            Debug.Log($"[TouchInputManager] 开始拖拽方块 {_piece.pieceId}");
        }
        
        /// <summary>
        /// 更新拖拽
        /// </summary>
        /// <param name="_touch">触摸数据</param>
        private void _updateDragging(Touch _touch)
        {
            if (_m_draggingPiece == null) return;
            
            // 更新拖拽轨迹
            if (_m_feedbackSystem != null && _m_enableVisualFeedback)
            {
                _m_feedbackSystem.updateDragTrail(_touch.position, _touch.fingerId);
            }
            
            // 将屏幕坐标转换为世界坐标
            Vector3 worldPosition = _screenToWorldPosition(_touch.position);
            
            // 触发拖拽中事件
            GameEvents.instance.onPieceDragging?.Invoke(_m_draggingPiece, worldPosition);
        }
        
        /// <summary>
        /// 结束拖拽
        /// </summary>
        /// <param name="_touch">触摸数据</param>
        private void _endDragging(Touch _touch)
        {
            if (_m_draggingPiece == null) return;
            
            // 结束拖拽轨迹
            if (_m_feedbackSystem != null && _m_enableVisualFeedback)
            {
                _m_feedbackSystem.endDragTrail(_touch.fingerId);
            }
            
            Vector3 worldPosition = _screenToWorldPosition(_touch.position);
            
            // 触发拖拽结束事件
            GameEvents.instance.onPieceDragEnd?.Invoke(_m_draggingPiece, worldPosition);
            
            Debug.Log($"[TouchInputManager] 结束拖拽方块 {_m_draggingPiece.pieceId}");
            
            _m_draggingPiece = null;
        }
        
        #endregion
        
        #region 私有方法 - 手势处理
        
        /// <summary>
        /// 处理点击
        /// </summary>
        /// <param name="_touch">触摸数据</param>
        private void _handleTap(Touch _touch)
        {
            GamePiece tappedPiece = _getTouchedPiece(_touch.position);
            
            if (tappedPiece != null)
            {
                // 触发方块点击事件
                GameEvents.instance.onPieceClicked?.Invoke(tappedPiece);
            }
        }
        
        /// <summary>
        /// 处理双击
        /// </summary>
        /// <param name="_piece">被双击的方块</param>
        private void _onDoubleTap(GamePiece _piece)
        {
            if (_piece == null) return;
            
            // 旋转方块
            _piece.rotate90Clockwise();
            
            // 触发旋转事件
            GameEvents.instance.onPieceRotated?.Invoke(_piece, _piece.playerId);
            
            // 播放触觉反馈
            if (_m_feedbackSystem != null && _m_enableHapticFeedback)
            {
                _m_feedbackSystem.playHapticFeedback(TouchFeedbackSystem.FeedbackType.Medium);
            }
            
            Debug.Log($"[TouchInputManager] 双击旋转方块 {_piece.pieceId}");
        }
        
        /// <summary>
        /// 处理长按
        /// </summary>
        private void _onLongPress()
        {
            if (_m_selectedPiece == null) return;
            
            // 翻转方块
            _m_selectedPiece.flipHorizontal();
            
            // 触发翻转事件
            GameEvents.instance.onPieceFlipped?.Invoke(_m_selectedPiece, _m_selectedPiece.playerId);
            
            // 播放触觉反馈
            if (_m_feedbackSystem != null && _m_enableHapticFeedback)
            {
                _m_feedbackSystem.playHapticFeedback(TouchFeedbackSystem.FeedbackType.Strong);
            }
            
            Debug.Log($"[TouchInputManager] 长按翻转方块 {_m_selectedPiece.pieceId}");
        }
        
        /// <summary>
        /// 处理缩放手势
        /// </summary>
        private void _handlePinchGesture()
        {
            Touch touch1 = UnityEngine.Input.GetTouch(0);
            Touch touch2 = UnityEngine.Input.GetTouch(1);
            
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);
            
            if (_m_lastPinchDistance > 0)
            {
                float deltaDistance = currentDistance - _m_lastPinchDistance;
                
                if (Mathf.Abs(deltaDistance) > _m_pinchThreshold)
                {
                    _m_currentTouchState = TouchState.Pinching;
                    
                    // 计算缩放因子
                    float scaleFactor = deltaDistance * _m_pinchSensitivity * 0.01f;
                    
                    // 触发棋盘缩放事件
                    _triggerBoardZoom(scaleFactor);
                    
                    Debug.Log($"[TouchInputManager] 缩放手势，缩放因子: {scaleFactor}");
                }
            }
            
            _m_lastPinchDistance = currentDistance;
        }
        
        /// <summary>
        /// 触发棋盘缩放
        /// </summary>
        /// <param name="_scaleFactor">缩放因子</param>
        private void _triggerBoardZoom(float _scaleFactor)
        {
            // 获取棋盘控制器并应用缩放
            var boardController = FindObjectOfType<BoardController>();
            if (boardController != null)
            {
                var boardVisualizer = boardController.getBoardVisualizer();
                if (boardVisualizer != null)
                {
                    // 获取当前缩放级别并应用变化
                    float currentZoom = 1.0f; // 这里需要从BoardVisualizer获取当前缩放
                    float newZoom = Mathf.Clamp(currentZoom + _scaleFactor, 0.5f, 3.0f);
                    boardVisualizer.setZoomLevel(newZoom);
                    
                    Debug.Log($"[TouchInputManager] 棋盘缩放: {_scaleFactor}, 新缩放级别: {newZoom}");
                }
            }
        }
        
        #endregion
        
        #region 私有方法 - 辅助功能
        
        /// <summary>
        /// 更新触摸状态
        /// </summary>
        private void _updateTouchState()
        {
            // 如果没有触摸输入，重置状态
            if (UnityEngine.Input.touchCount == 0 && !UnityEngine.Input.GetMouseButton(0))
            {
                if (_m_currentTouchState != TouchState.None)
                {
                    _m_currentTouchState = TouchState.None;
                    _m_lastPinchDistance = 0;
                }
            }
        }
        
        /// <summary>
        /// 重置当前触摸状态
        /// </summary>
        private void _resetCurrentTouch()
        {
            _m_selectedPiece = null;
            _m_draggingPiece = null;
            _m_isLongPressing = false;
            _m_currentTouchState = TouchState.None;
            _m_lastPinchDistance = 0;
            _m_lastPinchAngle = 0;
        }
        
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
        /// 显示视觉反馈效果
        /// </summary>
        /// <param name="_position">屏幕位置</param>
        /// <param name="_feedbackType">反馈类型</param>
        private void _showVisualFeedback(Vector2 _position, TouchFeedbackType _feedbackType)
        {
            if (!_m_enableVisualFeedback || _m_feedbackSystem == null) return;
            
            // 根据反馈类型选择不同的视觉效果
            switch (_feedbackType)
            {
                case TouchFeedbackType.Tap:
                    _m_feedbackSystem.showRippleEffect(_position, 0.5f);
                    break;
                    
                case TouchFeedbackType.Success:
                    _m_feedbackSystem.showRippleEffect(_position, 1.0f);
                    // 可以添加成功的颜色效果
                    break;
                    
                case TouchFeedbackType.Error:
                    _m_feedbackSystem.showRippleEffect(_position, 1.0f);
                    // 可以添加错误的颜色效果（红色）
                    break;
            }
        }
        
        /// <summary>
        /// 创建触摸效果
        /// </summary>
        /// <param name="_position">触摸位置</param>
        /// <param name="_feedbackType">反馈类型</param>
        private void _createTouchEffect(Vector2 _position, TouchFeedbackType _feedbackType)
        {
            // 将屏幕坐标转换为世界坐标
            Vector3 worldPosition = _screenToWorldPosition(_position);
            
            // 创建触摸效果对象
            GameObject effectObj = new GameObject("TouchEffect");
            effectObj.transform.position = worldPosition;
            
            // 添加视觉组件（例如粒子系统或简单的圆圈）
            var renderer = effectObj.AddComponent<MeshRenderer>();
            var meshFilter = effectObj.AddComponent<MeshFilter>();
            
            // 创建简单的圆形网格
            meshFilter.mesh = _createCircleMesh();
            
            // 设置材质颜色
            Material effectMaterial = new Material(Shader.Find("Sprites/Default"));
            switch (_feedbackType)
            {
                case TouchFeedbackType.Tap:
                    effectMaterial.color = Color.white;
                    break;
                case TouchFeedbackType.Success:
                    effectMaterial.color = Color.green;
                    break;
                case TouchFeedbackType.Error:
                    effectMaterial.color = Color.red;
                    break;
            }
            renderer.material = effectMaterial;
            
            // 启动效果动画
            StartCoroutine(_animateTouchEffect(effectObj));
        }
        
        /// <summary>
        /// 创建圆形网格
        /// </summary>
        /// <returns>圆形网格</returns>
        private Mesh _createCircleMesh()
        {
            Mesh mesh = new Mesh();
            int segments = 16;
            float radius = 0.1f;
            
            Vector3[] vertices = new Vector3[segments + 1];
            int[] triangles = new int[segments * 3];
            
            // 中心点
            vertices[0] = Vector3.zero;
            
            // 圆周点
            for (int i = 0; i < segments; i++)
            {
                float angle = i * 2f * Mathf.PI / segments;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            }
            
            // 三角形
            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % segments + 1;
            }
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            
            return mesh;
        }
        
        /// <summary>
        /// 触摸效果动画协程
        /// </summary>
        /// <param name="_effectObj">效果对象</param>
        /// <returns>协程</returns>
        private System.Collections.IEnumerator _animateTouchEffect(GameObject _effectObj)
        {
            float duration = _m_feedbackDuration;
            float elapsedTime = 0f;
            
            Vector3 startScale = Vector3.zero;
            Vector3 endScale = Vector3.one;
            
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                
                // 缩放动画
                _effectObj.transform.localScale = Vector3.Lerp(startScale, endScale, t);
                
                // 透明度动画
                var renderer = _effectObj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    Color color = renderer.material.color;
                    color.a = 1f - t;
                    renderer.material.color = color;
                }
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 销毁效果对象
            Destroy(_effectObj);
        }
        
        #endregion
        
        #region 私有方法 - 防误触和性能优化
        
        /// <summary>
        /// 检查是否可以处理触摸事件（性能限制）
        /// </summary>
        /// <returns>是否可以处理</returns>
        private bool _canProcessTouchEvent()
        {
            // 检查帧率限制
            if (_m_currentFrameTouchEvents >= _m_maxTouchEventsPerSecond / 60) // 假设60FPS
            {
                return false;
            }
            
            // 检查时间间隔
            float currentTime = Time.time;
            if (currentTime - _m_lastTouchEventTime < _m_minTouchInterval)
            {
                return false;
            }
            
            _m_lastTouchEventTime = currentTime;
            return true;
        }
        
        /// <summary>
        /// 验证触摸是否有效（防误触）
        /// </summary>
        /// <param name="_touch">触摸数据</param>
        /// <returns>是否为有效触摸</returns>
        private bool _isValidTouch(Touch _touch)
        {
            // 检查触摸位置是否在有效区域内
            if (!_isTouchPositionValid(_touch.position))
            {
                return false;
            }
            
            // 检查触摸压力（如果设备支持）
            if (_touch.pressure > 0 && _touch.pressure < _m_pressureThreshold)
            {
                return false;
            }
            
            // 检查同时触摸数量
            if (UnityEngine.Input.touchCount > _m_maxSimultaneousTouches)
            {
                return false;
            }
            
            // 检查触摸间隔
            float currentTime = Time.time;
            if (currentTime - _m_lastValidTouchTime < _m_minTouchInterval)
            {
                return false;
            }
            
            _m_lastValidTouchTime = currentTime;
            return true;
        }
        
        /// <summary>
        /// 检查触摸位置是否有效
        /// </summary>
        /// <param name="_position">触摸位置</param>
        /// <returns>是否有效</returns>
        private bool _isTouchPositionValid(Vector2 _position)
        {
            // 检查是否在屏幕边缘的无效区域
            if (_position.x < _m_invalidTouchBorder || 
                _position.x > Screen.width - _m_invalidTouchBorder ||
                _position.y < _m_invalidTouchBorder || 
                _position.y > Screen.height - _m_invalidTouchBorder)
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 处理防误触逻辑
        /// </summary>
        private void _processAntiMistouch()
        {
            // 检测异常触摸模式
            if (UnityEngine.Input.touchCount > _m_maxSimultaneousTouches)
            {
                if (!_m_isInAntiMistouchMode)
                {
                    _m_isInAntiMistouchMode = true;
                    Debug.LogWarning("[TouchInputManager] 检测到异常触摸模式，启用防误触模式");
                    
                    // 重置所有触摸状态
                    resetInput();
                }
            }
            else if (_m_isInAntiMistouchMode && UnityEngine.Input.touchCount == 0)
            {
                // 退出防误触模式
                _m_isInAntiMistouchMode = false;
                Debug.Log("[TouchInputManager] 退出防误触模式");
            }
        }
        
        /// <summary>
        /// 处理缓存的触摸事件
        /// </summary>
        private void _processCachedTouchEvents()
        {
            if (_m_touchEventCache == null) return;
            
            int processedEvents = 0;
            int maxEventsPerFrame = 5; // 每帧最多处理5个缓存事件
            
            while (_m_touchEventCache.Count > 0 && processedEvents < maxEventsPerFrame)
            {
                TouchEventData eventData = _m_touchEventCache.Dequeue();
                
                // 检查事件是否过期（超过100ms的事件丢弃）
                if (Time.time - eventData.timestamp > 0.1f)
                {
                    continue;
                }
                
                // 处理缓存的事件
                _processCachedTouchEvent(eventData);
                processedEvents++;
            }
        }
        
        /// <summary>
        /// 处理单个缓存的触摸事件
        /// </summary>
        /// <param name="_eventData">事件数据</param>
        private void _processCachedTouchEvent(TouchEventData _eventData)
        {
            // 根据事件类型处理
            switch (_eventData.eventType)
            {
                case TouchEventType.Began:
                    // 处理延迟的触摸开始事件
                    break;
                    
                case TouchEventType.Moved:
                    // 处理延迟的触摸移动事件
                    break;
                    
                case TouchEventType.Ended:
                    // 处理延迟的触摸结束事件
                    break;
            }
        }
        
        /// <summary>
        /// 将触摸事件添加到缓存
        /// </summary>
        /// <param name="_eventType">事件类型</param>
        /// <param name="_touchData">触摸数据</param>
        private void _cacheTouchEvent(TouchEventType _eventType, TouchData _touchData)
        {
            if (!_m_enableTouchEventCaching || _m_touchEventCache == null) return;
            
            TouchEventData eventData = new TouchEventData
            {
                eventType = _eventType,
                touchData = _touchData,
                timestamp = Time.time
            };
            
            _m_touchEventCache.Enqueue(eventData);
            
            // 限制缓存大小
            while (_m_touchEventCache.Count > 50)
            {
                _m_touchEventCache.Dequeue();
            }
        }
        
        /// <summary>
        /// 优化触摸处理性能
        /// </summary>
        private void _optimizeTouchPerformance()
        {
            // 根据设备性能调整处理频率
            if (Application.targetFrameRate > 0 && Application.targetFrameRate < 30)
            {
                // 低性能设备，降低处理频率
                _m_maxTouchEventsPerSecond = Mathf.Min(_m_maxTouchEventsPerSecond, 30);
            }
            
            // 根据触摸数量动态调整
            if (UnityEngine.Input.touchCount > 2)
            {
                // 多点触摸时降低处理频率
                _m_maxTouchEventsPerSecond = Mathf.Min(_m_maxTouchEventsPerSecond, 40);
            }
        }
        
        /// <summary>
        /// 获取触摸性能统计信息
        /// </summary>
        /// <returns>性能统计字符串</returns>
        public string getTouchPerformanceStats()
        {
            return $"触摸事件/秒: {_m_currentFrameTouchEvents * 60}, " +
                   $"缓存事件: {(_m_touchEventCache?.Count ?? 0)}, " +
                   $"防误触模式: {_m_isInAntiMistouchMode}";
        }
        
        #endregion
    }
}