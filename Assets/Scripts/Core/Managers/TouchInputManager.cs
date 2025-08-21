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
        
        /// <summary>最小缩放级别</summary>
        [SerializeField] private float _m_minZoomLevel = 0.5f;
        
        /// <summary>最大缩放级别</summary>
        [SerializeField] private float _m_maxZoomLevel = 3.0f;
        
        /// <summary>缩放平滑度</summary>
        [SerializeField] private float _m_zoomSmoothness = 0.1f;
        
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
        
        /// <summary>防误触模式开始时间</summary>
        private float _m_antiMistouchStartTime;
        
        /// <summary>触摸频率统计（用于异常检测）</summary>
        private Queue<float> _m_touchTimestamps = new Queue<float>();
        
        /// <summary>当前手势类型</summary>
        private TouchState _m_currentGestureType = TouchState.None;
        
        /// <summary>手势开始时间</summary>
        private float _m_gestureStartTime;
        
        /// <summary>手掌误触检测数据</summary>
        private List<Vector2> _m_palmTouchPositions = new List<Vector2>();
        
        /// <summary>性能监控计时器</summary>
        private float _m_performanceMonitorTimer;
        
        /// <summary>帧率历史记录</summary>
        private Queue<float> _m_frameRateHistory = new Queue<float>();
        
        /// <summary>当前平均帧率</summary>
        private float _m_currentAverageFrameRate = 60f;
        
        /// <summary>性能级别</summary>
        private PerformanceLevel _m_currentPerformanceLevel = PerformanceLevel.High;
        
        /// <summary>触摸事件批处理队列</summary>
        private Queue<TouchEventBatch> _m_touchEventBatchQueue = new Queue<TouchEventBatch>();
        
        /// <summary>对象池管理器</summary>
        private Dictionary<System.Type, Queue<object>> _m_objectPools = new Dictionary<System.Type, Queue<object>>();
        
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
        
        /// <summary>手势冲突检测时间窗口</summary>
        [SerializeField] private float _m_gestureConflictWindow = 0.2f;
        
        /// <summary>异常触摸检测阈值（每秒触摸次数）</summary>
        [SerializeField] private int _m_abnormalTouchThreshold = 20;
        
        /// <summary>手掌误触检测面积阈值</summary>
        [SerializeField] private float _m_palmTouchAreaThreshold = 100f;
        
        /// <summary>防误触恢复时间</summary>
        [SerializeField] private float _m_antiMistouchRecoveryTime = 1f;
        
        [Header("性能优化配置")]
        /// <summary>触摸事件处理频率限制（每秒最大次数）</summary>
        [SerializeField] private int _m_maxTouchEventsPerSecond = 60;
        
        /// <summary>是否启用触摸事件缓存</summary>
        [SerializeField] private bool _m_enableTouchEventCaching = true;
        
        /// <summary>是否启用自适应性能调整</summary>
        [SerializeField] private bool _m_enableAdaptivePerformance = true;
        
        /// <summary>性能监控间隔（秒）</summary>
        [SerializeField] private float _m_performanceMonitorInterval = 1f;
        
        /// <summary>低性能阈值（FPS）</summary>
        [SerializeField] private float _m_lowPerformanceThreshold = 30f;
        
        /// <summary>触摸事件批处理大小</summary>
        [SerializeField] private int _m_touchEventBatchSize = 5;
        
        /// <summary>对象池预分配大小</summary>
        [SerializeField] private int _m_objectPoolPreallocationSize = 20;
        
        // 注意：使用TouchFeedbackSystem.FeedbackType替代本地TouchFeedbackType枚举
        // 避免重复定义，统一使用TouchFeedbackSystem中的FeedbackType
        
        /// <summary>触摸状态和手势类型统一枚举</summary>
        public enum TouchState
        {
            /// <summary>无触摸</summary>
            None,
            /// <summary>单点触摸/点击</summary>
            Tap,
            /// <summary>拖拽中</summary>
            Dragging,
            /// <summary>长按</summary>
            LongPress,
            /// <summary>双击</summary>
            DoubleTap,
            /// <summary>多点触摸</summary>
            MultiTouch,
            /// <summary>缩放手势</summary>
            Pinching,
            /// <summary>旋转手势</summary>
            Rotation,
            /// <summary>平移手势</summary>
            Pan,
            /// <summary>边缘滑动</summary>
            EdgeSwipe
        }
        
        /// <summary>性能级别枚举</summary>
        public enum PerformanceLevel
        {
            /// <summary>低性能</summary>
            Low,
            /// <summary>中等性能</summary>
            Medium,
            /// <summary>高性能</summary>
            High
        }
        
        /// <summary>触摸事件批处理结构</summary>
        private struct TouchEventBatch
        {
            /// <summary>批处理中的事件列表</summary>
            public List<TouchEventData> events;
            /// <summary>批处理时间戳</summary>
            public float timestamp;
            /// <summary>批处理优先级</summary>
            public int priority;
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
            
            // 初始化对象池
            _initializeObjectPools();
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
            
            // 性能监控
            if (_m_enableAdaptivePerformance)
            {
                _monitorPerformance();
            }
            
            // 重置帧计数器
            _m_currentFrameTouchEvents = 0;
            
            // 根据性能级别调整处理策略
            if (_m_currentPerformanceLevel == PerformanceLevel.Low)
            {
                _handleInputLowPerformance();
            }
            else
            {
                _handleInput();
            }
            
            _updateTouchState();
            _processAntiMistouch();
            
            // 处理缓存的触摸事件
            if (_m_enableTouchEventCaching)
            {
                _processCachedTouchEvents();
            }
            
            // 处理批处理事件
            _processBatchedTouchEvents();
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
        
        /// <summary>
        /// 设置触摸灵敏度
        /// </summary>
        /// <param name="_sensitivity">触摸灵敏度值</param>
        public void SetTouchSensitivity(float _sensitivity)
        {
            // 调整拖拽阈值和触摸半径
            _m_dragThreshold = Mathf.Lerp(10f, 40f, 1f / _sensitivity);
            _m_touchRadius = Mathf.Lerp(30f, 80f, _sensitivity);
            
            Debug.Log($"[TouchInputManager] 触摸灵敏度设置为: {_sensitivity:F1}");
        }
        
        /// <summary>
        /// 设置双击间隔时间
        /// </summary>
        /// <param name="_interval">双击间隔时间（秒）</param>
        public void SetDoubleTapInterval(float _interval)
        {
            _m_doubleTapInterval = Mathf.Clamp(_interval, 0.1f, 1f);
            
            Debug.Log($"[TouchInputManager] 双击间隔设置为: {_m_doubleTapInterval:F2}s");
        }
        
        /// <summary>
        /// 设置触觉反馈开关
        /// </summary>
        /// <param name="_enabled">是否启用触觉反馈</param>
        public void SetHapticFeedbackEnabled(bool _enabled)
        {
            _m_enableHapticFeedback = _enabled;
            
            Debug.Log($"[TouchInputManager] 触觉反馈设置为: {_enabled}");
        }
        
        /// <summary>
        /// 获取当前是否在防误触模式
        /// </summary>
        /// <returns>是否在防误触模式</returns>
        public bool isInAntiMistouchMode()
        {
            return _m_isInAntiMistouchMode;
        }
        
        /// <summary>
        /// 设置防误触敏感度
        /// </summary>
        /// <param name="_sensitivity">敏感度（0.1-2.0）</param>
        public void setAntiMistouchSensitivity(float _sensitivity)
        {
            _sensitivity = Mathf.Clamp(_sensitivity, 0.1f, 2.0f);
            
            // 调整相关参数
            _m_abnormalTouchThreshold = Mathf.RoundToInt(20 / _sensitivity);
            _m_palmTouchAreaThreshold = 100f / _sensitivity;
            _m_gestureConflictWindow = 0.2f / _sensitivity;
            
            Debug.Log($"[TouchInputManager] 防误触敏感度设置为: {_sensitivity}");
        }
        
        /// <summary>
        /// 强制退出防误触模式
        /// </summary>
        public void forceExitAntiMistouchMode()
        {
            if (_m_isInAntiMistouchMode)
            {
                _m_isInAntiMistouchMode = false;
                resetInput();
                
                Debug.Log("[TouchInputManager] 强制退出防误触模式");
            }
        }
        
        /// <summary>
        /// 获取当前手势类型
        /// </summary>
        /// <returns>当前手势类型</returns>
        public TouchState getCurrentGestureType()
        {
            return _m_currentGestureType;
        }
        
        /// <summary>
        /// 设置性能级别
        /// </summary>
        /// <param name="_level">性能级别</param>
        public void setPerformanceLevel(PerformanceLevel _level)
        {
            _m_currentPerformanceLevel = _level;
            _applyPerformanceOptimizations();
            
            Debug.Log($"[TouchInputManager] 手动设置性能级别为: {_level}");
        }
        
        /// <summary>
        /// 获取当前性能级别
        /// </summary>
        /// <returns>当前性能级别</returns>
        public PerformanceLevel getCurrentPerformanceLevel()
        {
            return _m_currentPerformanceLevel;
        }
        
        /// <summary>
        /// 设置自适应性能调整开关
        /// </summary>
        /// <param name="_enabled">是否启用</param>
        public void setAdaptivePerformanceEnabled(bool _enabled)
        {
            _m_enableAdaptivePerformance = _enabled;
            
            Debug.Log($"[TouchInputManager] 自适应性能调整设置为: {_enabled}");
        }
        
        /// <summary>
        /// 清理对象池
        /// </summary>
        public void clearObjectPools()
        {
            foreach (var pool in _m_objectPools.Values)
            {
                pool.Clear();
            }
            
            Debug.Log("[TouchInputManager] 对象池已清理");
        }
        
        /// <summary>
        /// 强制垃圾回收（仅在必要时使用）
        /// </summary>
        public void forceGarbageCollection()
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            
            Debug.Log("[TouchInputManager] 强制垃圾回收完成");
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
            
            // 如果状态没有被具体手势更新，则设置为多点触摸
            if (_m_currentTouchState != TouchState.Pinching && 
                _m_currentTouchState != TouchState.Rotation && 
                _m_currentTouchState != TouchState.Pan)
            {
                _m_currentTouchState = TouchState.MultiTouch;
            }
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
                
                // 如果平移距离足够大，设置为平移状态
                if (panDelta.magnitude > _m_dragThreshold * 0.5f)
                {
                    _m_currentTouchState = TouchState.Pan;
                }
                
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
                    float panSensitivity = 0.02f; // 平移敏感度
                    Vector3 worldPanDelta = new Vector3(-_panDelta.x * panSensitivity, 0, -_panDelta.y * panSensitivity);
                    
                    // 获取当前平移偏移并应用增量
                    Vector3 currentPanOffset = _getCurrentPanOffset(boardVisualizer);
                    Vector3 newPanOffset = currentPanOffset + worldPanDelta;
                    
                    // 应用新的平移偏移
                    boardVisualizer.setPanOffset(newPanOffset);
                    
                    Debug.Log($"[TouchInputManager] 棋盘平移: {worldPanDelta}, 新偏移: {newPanOffset}");
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
                    _m_currentTouchState = TouchState.Rotation;
                    
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
                _showVisualFeedback(_touch.position, TouchFeedbackSystem.FeedbackType.Success);
            }
            else
            {
                // 显示普通触摸反馈
                _showVisualFeedback(_touch.position, TouchFeedbackSystem.FeedbackType.Light);
            }
            
            _m_currentTouchState = TouchState.Tap;
            
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
            if (_m_currentTouchState == TouchState.Tap)
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
                _m_currentTouchState = TouchState.LongPress;
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
            else if (_m_currentTouchState == TouchState.Tap)
            {
                // 检查是否是边缘滑动
                if (_isEdgeSwipe(_touch))
                {
                    _m_currentTouchState = TouchState.EdgeSwipe;
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
                _m_currentTouchState = TouchState.DoubleTap;
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
            
            // 计算两个触摸点之间的距离
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);
            
            // 初始化上一帧距离
            if (_m_lastPinchDistance <= 0)
            {
                _m_lastPinchDistance = currentDistance;
                return;
            }
            
            // 计算距离变化
            float deltaDistance = currentDistance - _m_lastPinchDistance;
            
            // 检查是否达到缩放阈值
            if (Mathf.Abs(deltaDistance) > _m_pinchThreshold)
            {
                _m_currentTouchState = TouchState.Pinching;
                
                // 计算缩放因子（基于距离变化的百分比）
                float scaleFactor = currentDistance / _m_lastPinchDistance;
                
                // 应用缩放敏感度调整
                float adjustedScaleFactor = 1f + (scaleFactor - 1f) * _m_pinchSensitivity;
                
                // 计算缩放中心点（两个触摸点的中心）
                Vector2 pinchCenter = (touch1.position + touch2.position) * 0.5f;
                
                // 触发棋盘缩放
                _triggerBoardZoomAtPoint(adjustedScaleFactor, pinchCenter);
                
                Debug.Log($"[TouchInputManager] 缩放手势 - 距离变化: {deltaDistance:F1}, 缩放因子: {adjustedScaleFactor:F3}");
                
                // 触发触觉反馈
                if (_m_feedbackSystem != null && _m_enableHapticFeedback)
                {
                    _m_feedbackSystem.playHapticFeedback(TouchFeedbackSystem.FeedbackType.Light);
                }
                
                // 显示缩放反馈效果
                if (_m_feedbackSystem != null && _m_enableVisualFeedback)
                {
                    _m_feedbackSystem.showScaleEffect(pinchCenter, adjustedScaleFactor);
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
                    float currentZoom = _getCurrentZoomLevel(boardVisualizer);
                    float newZoom = Mathf.Clamp(currentZoom * _scaleFactor, _m_minZoomLevel, _m_maxZoomLevel);
                    boardVisualizer.setZoomLevel(newZoom);
                    
                    Debug.Log($"[TouchInputManager] 棋盘缩放: {_scaleFactor:F3}, 新缩放级别: {newZoom:F3}");
                }
            }
        }
        
        /// <summary>
        /// 在指定点触发棋盘缩放
        /// </summary>
        /// <param name="_scaleFactor">缩放因子</param>
        /// <param name="_centerPoint">缩放中心点（屏幕坐标）</param>
        private void _triggerBoardZoomAtPoint(float _scaleFactor, Vector2 _centerPoint)
        {
            // 获取棋盘控制器并应用缩放
            var boardController = FindObjectOfType<BoardController>();
            if (boardController != null)
            {
                var boardVisualizer = boardController.getBoardVisualizer();
                if (boardVisualizer != null)
                {
                    // 获取当前缩放级别
                    float currentZoom = _getCurrentZoomLevel(boardVisualizer);
                    float newZoom = Mathf.Clamp(currentZoom * _scaleFactor, _m_minZoomLevel, _m_maxZoomLevel);
                    
                    // 计算缩放前后的世界坐标差异，用于调整平移偏移
                    Vector3 worldPoint = _screenToWorldPoint(_centerPoint);
                    Vector3 panAdjustment = worldPoint * (1f - newZoom / currentZoom);
                    
                    // 应用缩放
                    boardVisualizer.setZoomLevel(newZoom);
                    
                    // 调整平移偏移以保持缩放中心点不变
                    Vector3 currentPanOffset = _getCurrentPanOffset(boardVisualizer);
                    boardVisualizer.setPanOffset(currentPanOffset + panAdjustment);
                    
                    Debug.Log($"[TouchInputManager] 在点 {_centerPoint} 缩放: {_scaleFactor:F3}, 新缩放级别: {newZoom:F3}");
                }
            }
        }
        
        /// <summary>
        /// 获取当前缩放级别
        /// </summary>
        /// <param name="_boardVisualizer">棋盘可视化组件</param>
        /// <returns>当前缩放级别</returns>
        private float _getCurrentZoomLevel(BoardVisualizer _boardVisualizer)
        {
            if (_boardVisualizer == null) return 1.0f;
            return _boardVisualizer.getCurrentZoomLevel();
        }
        
        /// <summary>
        /// 获取当前平移偏移
        /// </summary>
        /// <param name="_boardVisualizer">棋盘可视化组件</param>
        /// <returns>当前平移偏移</returns>
        private Vector3 _getCurrentPanOffset(BoardVisualizer _boardVisualizer)
        {
            if (_boardVisualizer == null) return Vector3.zero;
            return _boardVisualizer.getCurrentPanOffset();
        }
        
        /// <summary>
        /// 屏幕坐标转换为世界坐标
        /// </summary>
        /// <param name="_screenPoint">屏幕坐标</param>
        /// <returns>世界坐标</returns>
        private Vector3 _screenToWorldPoint(Vector2 _screenPoint)
        {
            if (_m_camera == null) return Vector3.zero;
            
            // 将屏幕坐标转换为世界坐标（假设在Y=0平面上）
            Ray ray = _m_camera.ScreenPointToRay(_screenPoint);
            if (ray.direction.y != 0)
            {
                float distance = -ray.origin.y / ray.direction.y;
                return ray.origin + ray.direction * distance;
            }
            
            return Vector3.zero;
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
        private void _showVisualFeedback(Vector2 _position, TouchFeedbackSystem.FeedbackType _feedbackType)
        {
            if (!_m_enableVisualFeedback || _m_feedbackSystem == null) return;
            
            // 根据反馈类型选择不同的视觉效果
            switch (_feedbackType)
            {
                case TouchFeedbackSystem.FeedbackType.Light:
                    _m_feedbackSystem.showRippleEffect(_position, 0.5f);
                    break;
                    
                case TouchFeedbackSystem.FeedbackType.Medium:
                    _m_feedbackSystem.showRippleEffect(_position, 0.7f);
                    break;
                    
                case TouchFeedbackSystem.FeedbackType.Strong:
                    _m_feedbackSystem.showRippleEffect(_position, 1.0f);
                    break;
                    
                case TouchFeedbackSystem.FeedbackType.Success:
                    _m_feedbackSystem.showRippleEffect(_position, 1.0f);
                    // 可以添加成功的颜色效果
                    break;
                    
                case TouchFeedbackSystem.FeedbackType.Error:
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
        private void _createTouchEffect(Vector2 _position, TouchFeedbackSystem.FeedbackType _feedbackType)
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
                case TouchFeedbackSystem.FeedbackType.Light:
                    effectMaterial.color = Color.white;
                    break;
                case TouchFeedbackSystem.FeedbackType.Medium:
                    effectMaterial.color = Color.cyan;
                    break;
                case TouchFeedbackSystem.FeedbackType.Strong:
                    effectMaterial.color = Color.yellow;
                    break;
                case TouchFeedbackSystem.FeedbackType.Success:
                    effectMaterial.color = Color.green;
                    break;
                case TouchFeedbackSystem.FeedbackType.Error:
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
            // 如果在防误触模式中，更严格的验证
            if (_m_isInAntiMistouchMode)
            {
                return _isValidTouchInAntiMistouchMode(_touch);
            }
            
            // 基础位置验证
            if (!_isTouchPositionValid(_touch.position))
            {
                return false;
            }
            
            // 触摸压力验证（如果设备支持）
            if (_touch.pressure > 0 && _touch.pressure < _m_pressureThreshold)
            {
                Debug.Log($"[TouchInputManager] 触摸压力过低: {_touch.pressure}");
                return false;
            }
            
            // 同时触摸数量验证
            if (UnityEngine.Input.touchCount > _m_maxSimultaneousTouches)
            {
                Debug.LogWarning($"[TouchInputManager] 触摸数量过多: {UnityEngine.Input.touchCount}");
                return false;
            }
            
            // 触摸频率验证
            if (!_isValidTouchFrequency())
            {
                return false;
            }
            
            // 手势冲突检测
            if (_hasGestureConflict(_touch))
            {
                return false;
            }
            
            // 手掌误触检测
            if (_isPalmTouch(_touch))
            {
                return false;
            }
            
            // 触摸间隔验证
            float currentTime = Time.time;
            if (currentTime - _m_lastValidTouchTime < _m_minTouchInterval)
            {
                return false;
            }
            
            _m_lastValidTouchTime = currentTime;
            _recordTouchTimestamp(currentTime);
            
            return true;
        }
        
        /// <summary>
        /// 防误触模式下的触摸验证
        /// </summary>
        /// <param name="_touch">触摸数据</param>
        /// <returns>是否为有效触摸</returns>
        private bool _isValidTouchInAntiMistouchMode(Touch _touch)
        {
            // 在防误触模式下，只允许单点触摸
            if (UnityEngine.Input.touchCount > 1)
            {
                return false;
            }
            
            // 更严格的位置验证
            if (!_isTouchPositionValidStrict(_touch.position))
            {
                return false;
            }
            
            // 检查是否可以退出防误触模式
            float timeSinceAntiMistouch = Time.time - _m_antiMistouchStartTime;
            if (timeSinceAntiMistouch > _m_antiMistouchRecoveryTime)
            {
                _m_isInAntiMistouchMode = false;
                Debug.Log("[TouchInputManager] 防误触模式恢复正常");
            }
            
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
        /// 严格的触摸位置验证（防误触模式）
        /// </summary>
        /// <param name="_position">触摸位置</param>
        /// <returns>是否有效</returns>
        private bool _isTouchPositionValidStrict(Vector2 _position)
        {
            // 更大的边界区域
            float strictBorder = _m_invalidTouchBorder * 2f;
            
            if (_position.x < strictBorder || 
                _position.x > Screen.width - strictBorder ||
                _position.y < strictBorder || 
                _position.y > Screen.height - strictBorder)
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 验证触摸频率是否正常
        /// </summary>
        /// <returns>是否为正常频率</returns>
        private bool _isValidTouchFrequency()
        {
            float currentTime = Time.time;
            
            // 清理过期的时间戳
            while (_m_touchTimestamps.Count > 0 && currentTime - _m_touchTimestamps.Peek() > 1f)
            {
                _m_touchTimestamps.Dequeue();
            }
            
            // 检查每秒触摸次数是否异常
            if (_m_touchTimestamps.Count >= _m_abnormalTouchThreshold)
            {
                Debug.LogWarning($"[TouchInputManager] 检测到异常触摸频率: {_m_touchTimestamps.Count}/秒");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 记录触摸时间戳
        /// </summary>
        /// <param name="_timestamp">时间戳</param>
        private void _recordTouchTimestamp(float _timestamp)
        {
            _m_touchTimestamps.Enqueue(_timestamp);
            
            // 限制队列大小
            while (_m_touchTimestamps.Count > _m_abnormalTouchThreshold * 2)
            {
                _m_touchTimestamps.Dequeue();
            }
        }
        
        /// <summary>
        /// 检测手势冲突
        /// </summary>
        /// <param name="_touch">触摸数据</param>
        /// <returns>是否有手势冲突</returns>
        private bool _hasGestureConflict(Touch _touch)
        {
            float currentTime = Time.time;
            
            // 检测新手势
            TouchState newGestureType = _detectGestureType(_touch);
            
            // 如果当前没有手势，直接设置新手势
            if (_m_currentGestureType == TouchState.None)
            {
                _m_currentGestureType = newGestureType;
                _m_gestureStartTime = currentTime;
                return false;
            }
            
            // 检查手势切换是否过于频繁
            if (newGestureType != _m_currentGestureType && 
                currentTime - _m_gestureStartTime < _m_gestureConflictWindow)
            {
                Debug.LogWarning($"[TouchInputManager] 手势冲突: {_m_currentGestureType} -> {newGestureType}");
                return true;
            }
            
            // 更新当前手势
            if (newGestureType != _m_currentGestureType)
            {
                _m_currentGestureType = newGestureType;
                _m_gestureStartTime = currentTime;
            }
            
            return false;
        }
        
        /// <summary>
        /// 检测手势类型
        /// </summary>
        /// <param name="_touch">触摸数据</param>
        /// <returns>手势类型</returns>
        private TouchState _detectGestureType(Touch _touch)
        {
            int touchCount = UnityEngine.Input.touchCount;
            
            if (touchCount == 1)
            {
                if (_touch.phase == TouchPhase.Moved)
                {
                    float distance = Vector2.Distance(_touch.position, _touch.position - _touch.deltaPosition);
                    return distance > _m_dragThreshold ? TouchState.Dragging : TouchState.Tap;
                }
                return TouchState.Tap;
            }
            else if (touchCount == 2)
            {
                // 根据双指操作判断手势类型
                Touch touch1 = UnityEngine.Input.GetTouch(0);
                Touch touch2 = UnityEngine.Input.GetTouch(1);
                
                float currentDistance = Vector2.Distance(touch1.position, touch2.position);
                float lastDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, 
                                                     touch2.position - touch2.deltaPosition);
                
                if (Mathf.Abs(currentDistance - lastDistance) > _m_pinchThreshold)
                {
                    return TouchState.Pinching;
                }
                
                Vector2 direction1 = touch1.deltaPosition.normalized;
                Vector2 direction2 = touch2.deltaPosition.normalized;
                float dotProduct = Vector2.Dot(direction1, direction2);
                
                if (dotProduct > 0.8f) // 同方向移动
                {
                    return TouchState.Pan;
                }
                else if (dotProduct < -0.8f) // 反方向移动
                {
                    return TouchState.Rotation;
                }
            }
            
            return TouchState.None;
        }
        
        /// <summary>
        /// 检测是否为手掌误触
        /// </summary>
        /// <param name="_touch">触摸数据</param>
        /// <returns>是否为手掌误触</returns>
        private bool _isPalmTouch(Touch _touch)
        {
            // 如果设备支持触摸面积检测
            if (_touch.radius > 0)
            {
                float touchArea = Mathf.PI * _touch.radius * _touch.radius;
                if (touchArea > _m_palmTouchAreaThreshold)
                {
                    Debug.Log($"[TouchInputManager] 检测到手掌误触，面积: {touchArea}");
                    return true;
                }
            }
            
            // 基于多点触摸的手掌检测
            if (UnityEngine.Input.touchCount >= 3)
            {
                _m_palmTouchPositions.Clear();
                for (int i = 0; i < UnityEngine.Input.touchCount; i++)
                {
                    _m_palmTouchPositions.Add(UnityEngine.Input.GetTouch(i).position);
                }
                
                // 计算触摸点的分布面积
                float area = _calculateTouchArea(_m_palmTouchPositions);
                if (area > _m_palmTouchAreaThreshold)
                {
                    Debug.Log($"[TouchInputManager] 检测到多点手掌误触，面积: {area}");
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 计算触摸点分布面积
        /// </summary>
        /// <param name="_positions">触摸点位置列表</param>
        /// <returns>分布面积</returns>
        private float _calculateTouchArea(List<Vector2> _positions)
        {
            if (_positions.Count < 3) return 0f;
            
            // 简单的包围盒面积计算
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            
            foreach (Vector2 pos in _positions)
            {
                minX = Mathf.Min(minX, pos.x);
                maxX = Mathf.Max(maxX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxY = Mathf.Max(maxY, pos.y);
            }
            
            return (maxX - minX) * (maxY - minY);
        }
        
        /// <summary>
        /// 处理防误触逻辑
        /// </summary>
        private void _processAntiMistouch()
        {
            float currentTime = Time.time;
            
            // 检测异常触摸模式
            bool shouldEnterAntiMistouchMode = false;
            
            // 1. 检测同时触摸数量异常
            if (UnityEngine.Input.touchCount > _m_maxSimultaneousTouches)
            {
                shouldEnterAntiMistouchMode = true;
                Debug.LogWarning($"[TouchInputManager] 触摸数量异常: {UnityEngine.Input.touchCount}");
            }
            
            // 2. 检测触摸频率异常
            if (_m_touchTimestamps.Count >= _m_abnormalTouchThreshold)
            {
                shouldEnterAntiMistouchMode = true;
                Debug.LogWarning($"[TouchInputManager] 触摸频率异常: {_m_touchTimestamps.Count}/秒");
            }
            
            // 3. 检测手掌误触
            if (UnityEngine.Input.touchCount >= 3)
            {
                bool hasPalmTouch = false;
                for (int i = 0; i < UnityEngine.Input.touchCount; i++)
                {
                    if (_isPalmTouch(UnityEngine.Input.GetTouch(i)))
                    {
                        hasPalmTouch = true;
                        break;
                    }
                }
                
                if (hasPalmTouch)
                {
                    shouldEnterAntiMistouchMode = true;
                    Debug.LogWarning("[TouchInputManager] 检测到手掌误触");
                }
            }
            
            // 进入防误触模式
            if (shouldEnterAntiMistouchMode && !_m_isInAntiMistouchMode)
            {
                _m_isInAntiMistouchMode = true;
                _m_antiMistouchStartTime = currentTime;
                Debug.LogWarning("[TouchInputManager] 启用防误触模式");
                
                // 重置所有触摸状态
                resetInput();
                
                // 清理触摸频率统计
                _m_touchTimestamps.Clear();
                
                // 重置手势状态
                _m_currentGestureType = TouchState.None;
                
                // 触发防误触反馈
                if (_m_feedbackSystem != null && _m_enableHapticFeedback)
                {
                    _m_feedbackSystem.playHapticFeedback(TouchFeedbackSystem.FeedbackType.Error);
                }
            }
            
            // 检查是否可以退出防误触模式
            if (_m_isInAntiMistouchMode)
            {
                bool canExitAntiMistouchMode = false;
                
                // 1. 没有触摸输入
                if (UnityEngine.Input.touchCount == 0)
                {
                    canExitAntiMistouchMode = true;
                }
                
                // 2. 超过恢复时间且只有单点触摸
                if (currentTime - _m_antiMistouchStartTime > _m_antiMistouchRecoveryTime && 
                    UnityEngine.Input.touchCount <= 1)
                {
                    canExitAntiMistouchMode = true;
                }
                
                if (canExitAntiMistouchMode)
                {
                    _m_isInAntiMistouchMode = false;
                    Debug.Log("[TouchInputManager] 退出防误触模式");
                    
                    // 触发恢复反馈
                    if (_m_feedbackSystem != null && _m_enableHapticFeedback)
                    {
                        _m_feedbackSystem.playHapticFeedback(TouchFeedbackSystem.FeedbackType.Success);
                    }
                }
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
        /// 性能监控
        /// </summary>
        private void _monitorPerformance()
        {
            _m_performanceMonitorTimer += Time.deltaTime;
            
            if (_m_performanceMonitorTimer >= _m_performanceMonitorInterval)
            {
                _m_performanceMonitorTimer = 0f;
                
                // 计算当前帧率
                float currentFrameRate = 1f / Time.deltaTime;
                _m_frameRateHistory.Enqueue(currentFrameRate);
                
                // 保持历史记录在合理范围内
                while (_m_frameRateHistory.Count > 10)
                {
                    _m_frameRateHistory.Dequeue();
                }
                
                // 计算平均帧率
                float totalFrameRate = 0f;
                foreach (float frameRate in _m_frameRateHistory)
                {
                    totalFrameRate += frameRate;
                }
                _m_currentAverageFrameRate = totalFrameRate / _m_frameRateHistory.Count;
                
                // 更新性能级别
                _updatePerformanceLevel();
                
                // 应用性能优化
                _applyPerformanceOptimizations();
            }
        }
        
        /// <summary>
        /// 更新性能级别
        /// </summary>
        private void _updatePerformanceLevel()
        {
            PerformanceLevel newLevel;
            
            if (_m_currentAverageFrameRate < _m_lowPerformanceThreshold)
            {
                newLevel = PerformanceLevel.Low;
            }
            else if (_m_currentAverageFrameRate < _m_lowPerformanceThreshold * 1.5f)
            {
                newLevel = PerformanceLevel.Medium;
            }
            else
            {
                newLevel = PerformanceLevel.High;
            }
            
            if (newLevel != _m_currentPerformanceLevel)
            {
                _m_currentPerformanceLevel = newLevel;
                Debug.Log($"[TouchInputManager] 性能级别变更为: {newLevel}");
            }
        }
        
        /// <summary>
        /// 应用性能优化
        /// </summary>
        private void _applyPerformanceOptimizations()
        {
            switch (_m_currentPerformanceLevel)
            {
                case PerformanceLevel.Low:
                    _m_maxTouchEventsPerSecond = 30;
                    _m_enableVisualFeedback = false;
                    _m_touchEventBatchSize = 10; // 增加批处理大小
                    break;
                    
                case PerformanceLevel.Medium:
                    _m_maxTouchEventsPerSecond = 45;
                    _m_enableVisualFeedback = true;
                    _m_touchEventBatchSize = 5;
                    break;
                    
                case PerformanceLevel.High:
                    _m_maxTouchEventsPerSecond = 60;
                    _m_enableVisualFeedback = true;
                    _m_touchEventBatchSize = 3;
                    break;
            }
            
            // 根据触摸数量动态调整
            if (UnityEngine.Input.touchCount > 2)
            {
                _m_maxTouchEventsPerSecond = Mathf.RoundToInt(_m_maxTouchEventsPerSecond * 0.8f);
            }
        }
        
        /// <summary>
        /// 低性能模式下的输入处理
        /// </summary>
        private void _handleInputLowPerformance()
        {
            // 降低处理频率，每隔一帧处理一次
            if (Time.frameCount % 2 != 0) return;
            
            // 只处理最重要的触摸事件
            if (_m_enableTouchInput && UnityEngine.Input.touchCount > 0)
            {
                // 只处理第一个触摸点
                _handleSingleTouch(UnityEngine.Input.GetTouch(0));
            }
            else if (_m_enableMouseInput && _shouldHandleMouseInput())
            {
                _handleMouseInput();
            }
        }
        
        /// <summary>
        /// 处理批处理的触摸事件
        /// </summary>
        private void _processBatchedTouchEvents()
        {
            if (_m_touchEventBatchQueue.Count == 0) return;
            
            int processedBatches = 0;
            int maxBatchesPerFrame = _m_currentPerformanceLevel == PerformanceLevel.Low ? 1 : 3;
            
            while (_m_touchEventBatchQueue.Count > 0 && processedBatches < maxBatchesPerFrame)
            {
                TouchEventBatch batch = _m_touchEventBatchQueue.Dequeue();
                
                // 处理批处理中的事件
                foreach (TouchEventData eventData in batch.events)
                {
                    _processSingleTouchEvent(eventData);
                }
                
                // 回收事件列表到对象池
                _returnToObjectPool(batch.events);
                
                processedBatches++;
            }
        }
        
        /// <summary>
        /// 处理单个触摸事件
        /// </summary>
        /// <param name="_eventData">触摸事件数据</param>
        private void _processSingleTouchEvent(TouchEventData _eventData)
        {
            // 根据事件类型处理
            switch (_eventData.eventType)
            {
                case TouchEventType.Began:
                    // 处理触摸开始
                    break;
                case TouchEventType.Moved:
                    // 处理触摸移动
                    break;
                case TouchEventType.Ended:
                    // 处理触摸结束
                    break;
                case TouchEventType.Canceled:
                    // 处理触摸取消
                    break;
            }
        }
        
        /// <summary>
        /// 初始化对象池
        /// </summary>
        private void _initializeObjectPools()
        {
            // 初始化触摸事件数据对象池
            var touchEventDataPool = new Queue<object>();
            for (int i = 0; i < _m_objectPoolPreallocationSize; i++)
            {
                touchEventDataPool.Enqueue(new TouchEventData());
            }
            _m_objectPools[typeof(TouchEventData)] = touchEventDataPool;
            
            // 初始化触摸事件列表对象池
            var touchEventListPool = new Queue<object>();
            for (int i = 0; i < _m_objectPoolPreallocationSize / 2; i++)
            {
                touchEventListPool.Enqueue(new List<TouchEventData>());
            }
            _m_objectPools[typeof(List<TouchEventData>)] = touchEventListPool;
            
            Debug.Log($"[TouchInputManager] 对象池初始化完成，预分配大小: {_m_objectPoolPreallocationSize}");
        }
        
        /// <summary>
        /// 从对象池获取对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>对象实例</returns>
        private T _getFromObjectPool<T>() where T : class, new()
        {
            System.Type type = typeof(T);
            
            if (_m_objectPools.ContainsKey(type) && _m_objectPools[type].Count > 0)
            {
                return _m_objectPools[type].Dequeue() as T;
            }
            
            // 池中没有对象，创建新的
            return new T();
        }
        
        /// <summary>
        /// 将对象返回到对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="_obj">要返回的对象</param>
        private void _returnToObjectPool<T>(T _obj) where T : class
        {
            if (_obj == null) return;
            
            System.Type type = typeof(T);
            
            if (!_m_objectPools.ContainsKey(type))
            {
                _m_objectPools[type] = new Queue<object>();
            }
            
            // 清理对象状态
            if (_obj is List<TouchEventData> list)
            {
                list.Clear();
            }
            
            // 限制池大小
            if (_m_objectPools[type].Count < _m_objectPoolPreallocationSize * 2)
            {
                _m_objectPools[type].Enqueue(_obj);
            }
        }
        
        /// <summary>
        /// 创建触摸事件批处理
        /// </summary>
        /// <param name="_events">事件列表</param>
        /// <param name="_priority">优先级</param>
        private void _createTouchEventBatch(List<TouchEventData> _events, int _priority = 0)
        {
            if (_events == null || _events.Count == 0) return;
            
            TouchEventBatch batch = new TouchEventBatch
            {
                events = _events,
                timestamp = Time.time,
                priority = _priority
            };
            
            _m_touchEventBatchQueue.Enqueue(batch);
        }
        
        /// <summary>
        /// 获取触摸性能统计信息
        /// </summary>
        /// <returns>性能统计字符串</returns>
        public string getTouchPerformanceStats()
        {
            int totalPooledObjects = 0;
            foreach (var pool in _m_objectPools.Values)
            {
                totalPooledObjects += pool.Count;
            }
            
            return $"触摸事件/秒: {_m_currentFrameTouchEvents * 60}, " +
                   $"缓存事件: {(_m_touchEventCache?.Count ?? 0)}, " +
                   $"批处理队列: {_m_touchEventBatchQueue.Count}, " +
                   $"对象池: {totalPooledObjects}, " +
                   $"平均帧率: {_m_currentAverageFrameRate:F1}, " +
                   $"性能级别: {_m_currentPerformanceLevel}, " +
                   $"防误触模式: {_m_isInAntiMistouchMode}";
        }
        
        /// <summary>
        /// 获取详细的性能报告
        /// </summary>
        /// <returns>详细性能报告</returns>
        public string getDetailedPerformanceReport()
        {
            System.Text.StringBuilder report = new System.Text.StringBuilder();
            
            report.AppendLine("=== TouchInputManager 性能报告 ===");
            report.AppendLine($"当前帧率: {1f / Time.deltaTime:F1} FPS");
            report.AppendLine($"平均帧率: {_m_currentAverageFrameRate:F1} FPS");
            report.AppendLine($"性能级别: {_m_currentPerformanceLevel}");
            report.AppendLine($"触摸事件处理频率: {_m_maxTouchEventsPerSecond}/秒");
            report.AppendLine($"当前帧触摸事件: {_m_currentFrameTouchEvents}");
            report.AppendLine($"触摸事件缓存: {(_m_touchEventCache?.Count ?? 0)}");
            report.AppendLine($"批处理队列: {_m_touchEventBatchQueue.Count}");
            report.AppendLine($"批处理大小: {_m_touchEventBatchSize}");
            
            report.AppendLine("\n=== 对象池状态 ===");
            foreach (var kvp in _m_objectPools)
            {
                report.AppendLine($"{kvp.Key.Name}: {kvp.Value.Count} 个对象");
            }
            
            report.AppendLine($"\n=== 防误触状态 ===");
            report.AppendLine($"防误触模式: {_m_isInAntiMistouchMode}");
            report.AppendLine($"当前手势: {_m_currentGestureType}");
            report.AppendLine($"触摸频率统计: {_m_touchTimestamps.Count}/秒");
            
            return report.ToString();
        }
        
        #endregion
    }
}