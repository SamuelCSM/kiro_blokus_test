using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BlokusGame.Core.Events;
using BlokusGame.Core.Interfaces;

namespace BlokusGame.Core.InputSystem
{
    /// <summary>
    /// 触摸反馈系统 - 提供触摸操作的视觉和触觉反馈
    /// 管理触摸点的视觉效果、触觉反馈和音效提示
    /// 支持自定义反馈效果和动画
    /// </summary>
    public class TouchFeedbackSystem : MonoBehaviour
    {
        [Header("视觉反馈配置")]
        /// <summary>是否启用视觉反馈</summary>
        [SerializeField] private bool _m_enableVisualFeedback = true;
        
        /// <summary>触摸点预制体</summary>
        [SerializeField] private GameObject _m_touchPointPrefab;
        
        /// <summary>触摸涟漪效果预制体</summary>
        [SerializeField] private GameObject _m_rippleEffectPrefab;
        
        /// <summary>拖拽轨迹预制体</summary>
        [SerializeField] private GameObject _m_dragTrailPrefab;
        
        /// <summary>触摸点显示时间</summary>
        [SerializeField] private float _m_touchPointDuration = 0.3f;
        
        /// <summary>涟漪效果持续时间</summary>
        [SerializeField] private float _m_rippleEffectDuration = 0.5f;
        
        /// <summary>拖拽轨迹淡出时间</summary>
        [SerializeField] private float _m_trailFadeTime = 1f;
        
        [Header("触觉反馈配置")]
        /// <summary>是否启用触觉反馈</summary>
        [SerializeField] private bool _m_enableHapticFeedback = true;
        
        /// <summary>轻触反馈强度</summary>
        [SerializeField] [Range(0f, 1f)] private float _m_lightHapticIntensity = 0.3f;
        
        /// <summary>中等反馈强度</summary>
        [SerializeField] [Range(0f, 1f)] private float _m_mediumHapticIntensity = 0.6f;
        
        /// <summary>强烈反馈强度</summary>
        [SerializeField] [Range(0f, 1f)] private float _m_strongHapticIntensity = 1f;
        
        [Header("音效反馈配置")]
        /// <summary>是否启用音效反馈</summary>
        [SerializeField] private bool _m_enableAudioFeedback = true;
        
        /// <summary>触摸音效</summary>
        [SerializeField] private AudioClip _m_touchSound;
        
        /// <summary>拖拽开始音效</summary>
        [SerializeField] private AudioClip _m_dragStartSound;
        
        /// <summary>拖拽结束音效</summary>
        [SerializeField] private AudioClip _m_dragEndSound;
        
        /// <summary>双击音效</summary>
        [SerializeField] private AudioClip _m_doubleTapSound;
        
        /// <summary>长按音效</summary>
        [SerializeField] private AudioClip _m_longPressSound;
        
        // 私有字段
        /// <summary>摄像机引用</summary>
        private Camera _m_camera;
        
        /// <summary>音频源组件</summary>
        private AudioSource _m_audioSource;
        
        /// <summary>活跃的触摸点效果</summary>
        private Dictionary<int, GameObject> _m_activeTouchPoints = new Dictionary<int, GameObject>();
        
        /// <summary>活跃的涟漪效果</summary>
        private List<GameObject> _m_activeRipples = new List<GameObject>();
        
        /// <summary>活跃的拖拽轨迹</summary>
        private Dictionary<int, GameObject> _m_activeDragTrails = new Dictionary<int, GameObject>();
        
        /// <summary>对象池 - 触摸点</summary>
        private Queue<GameObject> _m_touchPointPool = new Queue<GameObject>();
        
        /// <summary>对象池 - 涟漪效果</summary>
        private Queue<GameObject> _m_ripplePool = new Queue<GameObject>();
        
        /// <summary>对象池 - 拖拽轨迹</summary>
        private Queue<GameObject> _m_trailPool = new Queue<GameObject>();
        
        /// <summary>反馈类型枚举</summary>
        public enum FeedbackType
        {
            /// <summary>轻触</summary>
            Light,
            /// <summary>中等</summary>
            Medium,
            /// <summary>强烈</summary>
            Strong
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
            
            _m_audioSource = GetComponent<AudioSource>();
            if (_m_audioSource == null)
            {
                _m_audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        /// <summary>
        /// Unity Start方法 - 初始化反馈系统
        /// </summary>
        private void Start()
        {
            _initializeFeedbackSystem();
            _subscribeToEvents();
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 清理资源
        /// </summary>
        private void OnDestroy()
        {
            _unsubscribeFromEvents();
            _cleanup();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 显示触摸点反馈
        /// </summary>
        /// <param name="_screenPosition">屏幕坐标</param>
        /// <param name="_fingerId">触摸ID</param>
        public void showTouchPoint(Vector2 _screenPosition, int _fingerId = 0)
        {
            if (!_m_enableVisualFeedback || _m_touchPointPrefab == null) return;
            
            Vector3 worldPosition = _screenToWorldPosition(_screenPosition);
            GameObject touchPoint = _getTouchPointFromPool();
            
            if (touchPoint != null)
            {
                touchPoint.transform.position = worldPosition;
                touchPoint.SetActive(true);
                
                _m_activeTouchPoints[_fingerId] = touchPoint;
                
                // 启动淡出协程
                StartCoroutine(_fadeTouchPoint(touchPoint, _fingerId));
            }
        }
        
        /// <summary>
        /// 隐藏触摸点反馈
        /// </summary>
        /// <param name="_fingerId">触摸ID</param>
        public void hideTouchPoint(int _fingerId = 0)
        {
            if (_m_activeTouchPoints.ContainsKey(_fingerId))
            {
                GameObject touchPoint = _m_activeTouchPoints[_fingerId];
                _returnTouchPointToPool(touchPoint);
                _m_activeTouchPoints.Remove(_fingerId);
            }
        }
        
        /// <summary>
        /// 显示涟漪效果
        /// </summary>
        /// <param name="_screenPosition">屏幕坐标</param>
        /// <param name="_intensity">效果强度</param>
        public void showRippleEffect(Vector2 _screenPosition, float _intensity = 1f)
        {
            if (!_m_enableVisualFeedback || _m_rippleEffectPrefab == null) return;
            
            Vector3 worldPosition = _screenToWorldPosition(_screenPosition);
            GameObject ripple = _getRippleFromPool();
            
            if (ripple != null)
            {
                ripple.transform.position = worldPosition;
                ripple.transform.localScale = Vector3.one * _intensity;
                ripple.SetActive(true);
                
                _m_activeRipples.Add(ripple);
                
                // 启动涟漪动画协程
                StartCoroutine(_animateRipple(ripple));
            }
        }
        
        /// <summary>
        /// 开始拖拽轨迹
        /// </summary>
        /// <param name="_screenPosition">屏幕坐标</param>
        /// <param name="_fingerId">触摸ID</param>
        public void startDragTrail(Vector2 _screenPosition, int _fingerId = 0)
        {
            if (!_m_enableVisualFeedback || _m_dragTrailPrefab == null) return;
            
            Vector3 worldPosition = _screenToWorldPosition(_screenPosition);
            GameObject trail = _getTrailFromPool();
            
            if (trail != null)
            {
                trail.transform.position = worldPosition;
                trail.SetActive(true);
                
                _m_activeDragTrails[_fingerId] = trail;
                
                // 初始化轨迹组件
                TrailRenderer trailRenderer = trail.GetComponent<TrailRenderer>();
                if (trailRenderer != null)
                {
                    trailRenderer.Clear();
                }
            }
        }
        
        /// <summary>
        /// 更新拖拽轨迹
        /// </summary>
        /// <param name="_screenPosition">屏幕坐标</param>
        /// <param name="_fingerId">触摸ID</param>
        public void updateDragTrail(Vector2 _screenPosition, int _fingerId = 0)
        {
            if (_m_activeDragTrails.ContainsKey(_fingerId))
            {
                GameObject trail = _m_activeDragTrails[_fingerId];
                Vector3 worldPosition = _screenToWorldPosition(_screenPosition);
                trail.transform.position = worldPosition;
            }
        }
        
        /// <summary>
        /// 结束拖拽轨迹
        /// </summary>
        /// <param name="_fingerId">触摸ID</param>
        public void endDragTrail(int _fingerId = 0)
        {
            if (_m_activeDragTrails.ContainsKey(_fingerId))
            {
                GameObject trail = _m_activeDragTrails[_fingerId];
                _m_activeDragTrails.Remove(_fingerId);
                
                // 启动轨迹淡出协程
                StartCoroutine(_fadeTrail(trail));
            }
        }
        
        /// <summary>
        /// 播放触觉反馈
        /// </summary>
        /// <param name="_feedbackType">反馈类型</param>
        public void playHapticFeedback(FeedbackType _feedbackType = FeedbackType.Light)
        {
            if (!_m_enableHapticFeedback) return;
            
            // 在移动设备上播放触觉反馈
            if (Application.isMobilePlatform)
            {
                switch (_feedbackType)
                {
                    case FeedbackType.Light:
                        Handheld.Vibrate();
                        break;
                    case FeedbackType.Medium:
                    case FeedbackType.Strong:
                        Handheld.Vibrate();
                        break;
                }
            }
        }
        
        /// <summary>
        /// 播放音效反馈
        /// </summary>
        /// <param name="_audioClip">音效片段</param>
        /// <param name="_volume">音量</param>
        public void playAudioFeedback(AudioClip _audioClip, float _volume = 1f)
        {
            if (!_m_enableAudioFeedback || _audioClip == null || _m_audioSource == null) return;
            
            _m_audioSource.PlayOneShot(_audioClip, _volume);
        }
        
        /// <summary>
        /// 设置视觉反馈是否启用
        /// </summary>
        /// <param name="_enabled">是否启用</param>
        public void setVisualFeedbackEnabled(bool _enabled)
        {
            _m_enableVisualFeedback = _enabled;
            
            if (!_enabled)
            {
                _clearAllVisualEffects();
            }
        }
        
        /// <summary>
        /// 设置触觉反馈是否启用
        /// </summary>
        /// <param name="_enabled">是否启用</param>
        public void setHapticFeedbackEnabled(bool _enabled)
        {
            _m_enableHapticFeedback = _enabled;
        }
        
        /// <summary>
        /// 设置音效反馈是否启用
        /// </summary>
        /// <param name="_enabled">是否启用</param>
        public void setAudioFeedbackEnabled(bool _enabled)
        {
            _m_enableAudioFeedback = _enabled;
        }
        
        #endregion
        
        #region 私有方法 - 初始化和清理
        
        /// <summary>
        /// 初始化反馈系统
        /// </summary>
        private void _initializeFeedbackSystem()
        {
            _initializeObjectPools();
            
            Debug.Log("[TouchFeedbackSystem] 触摸反馈系统初始化完成");
        }
        
        /// <summary>
        /// 初始化对象池
        /// </summary>
        private void _initializeObjectPools()
        {
            // 预创建触摸点对象
            if (_m_touchPointPrefab != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    GameObject touchPoint = Instantiate(_m_touchPointPrefab, transform);
                    touchPoint.SetActive(false);
                    _m_touchPointPool.Enqueue(touchPoint);
                }
            }
            
            // 预创建涟漪效果对象
            if (_m_rippleEffectPrefab != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    GameObject ripple = Instantiate(_m_rippleEffectPrefab, transform);
                    ripple.SetActive(false);
                    _m_ripplePool.Enqueue(ripple);
                }
            }
            
            // 预创建拖拽轨迹对象
            if (_m_dragTrailPrefab != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    GameObject trail = Instantiate(_m_dragTrailPrefab, transform);
                    trail.SetActive(false);
                    _m_trailPool.Enqueue(trail);
                }
            }
        }
        
        /// <summary>
        /// 订阅事件
        /// </summary>
        private void _subscribeToEvents()
        {
            // 这里可以订阅游戏事件来自动播放反馈
            if (GameEvents.instance != null)
            {
                GameEvents.instance.onPieceSelected += _onPieceSelected;
                GameEvents.instance.onPieceDragStart += _onPieceDragStart;
                GameEvents.instance.onPieceDragEnd += _onPieceDragEnd;
                GameEvents.instance.onPieceRotated += _onPieceRotated;
                GameEvents.instance.onPieceFlipped += _onPieceFlipped;
            }
        }
        
        /// <summary>
        /// 取消订阅事件
        /// </summary>
        private void _unsubscribeFromEvents()
        {
            if (GameEvents.instance != null)
            {
                GameEvents.instance.onPieceSelected -= _onPieceSelected;
                GameEvents.instance.onPieceDragStart -= _onPieceDragStart;
                GameEvents.instance.onPieceDragEnd -= _onPieceDragEnd;
                GameEvents.instance.onPieceRotated -= _onPieceRotated;
                GameEvents.instance.onPieceFlipped -= _onPieceFlipped;
            }
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        private void _cleanup()
        {
            _clearAllVisualEffects();
            
            // 清理对象池
            _clearObjectPool(_m_touchPointPool);
            _clearObjectPool(_m_ripplePool);
            _clearObjectPool(_m_trailPool);
        }
        
        /// <summary>
        /// 清理对象池
        /// </summary>
        /// <param name="_pool">对象池</param>
        private void _clearObjectPool(Queue<GameObject> _pool)
        {
            while (_pool.Count > 0)
            {
                GameObject obj = _pool.Dequeue();
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
        }
        
        #endregion
        
        #region 私有方法 - 对象池管理
        
        /// <summary>
        /// 从对象池获取触摸点
        /// </summary>
        /// <returns>触摸点对象</returns>
        private GameObject _getTouchPointFromPool()
        {
            if (_m_touchPointPool.Count > 0)
            {
                return _m_touchPointPool.Dequeue();
            }
            else if (_m_touchPointPrefab != null)
            {
                return Instantiate(_m_touchPointPrefab, transform);
            }
            
            return null;
        }
        
        /// <summary>
        /// 将触摸点返回对象池
        /// </summary>
        /// <param name="_touchPoint">触摸点对象</param>
        private void _returnTouchPointToPool(GameObject _touchPoint)
        {
            if (_touchPoint != null)
            {
                _touchPoint.SetActive(false);
                _m_touchPointPool.Enqueue(_touchPoint);
            }
        }
        
        /// <summary>
        /// 从对象池获取涟漪效果
        /// </summary>
        /// <returns>涟漪效果对象</returns>
        private GameObject _getRippleFromPool()
        {
            if (_m_ripplePool.Count > 0)
            {
                return _m_ripplePool.Dequeue();
            }
            else if (_m_rippleEffectPrefab != null)
            {
                return Instantiate(_m_rippleEffectPrefab, transform);
            }
            
            return null;
        }
        
        /// <summary>
        /// 将涟漪效果返回对象池
        /// </summary>
        /// <param name="_ripple">涟漪效果对象</param>
        private void _returnRippleToPool(GameObject _ripple)
        {
            if (_ripple != null)
            {
                _ripple.SetActive(false);
                _m_ripplePool.Enqueue(_ripple);
                _m_activeRipples.Remove(_ripple);
            }
        }
        
        /// <summary>
        /// 从对象池获取拖拽轨迹
        /// </summary>
        /// <returns>拖拽轨迹对象</returns>
        private GameObject _getTrailFromPool()
        {
            if (_m_trailPool.Count > 0)
            {
                return _m_trailPool.Dequeue();
            }
            else if (_m_dragTrailPrefab != null)
            {
                return Instantiate(_m_dragTrailPrefab, transform);
            }
            
            return null;
        }
        
        /// <summary>
        /// 将拖拽轨迹返回对象池
        /// </summary>
        /// <param name="_trail">拖拽轨迹对象</param>
        private void _returnTrailToPool(GameObject _trail)
        {
            if (_trail != null)
            {
                _trail.SetActive(false);
                _m_trailPool.Enqueue(_trail);
            }
        }
        
        #endregion
        
        #region 私有方法 - 动画和效果
        
        /// <summary>
        /// 触摸点淡出协程
        /// </summary>
        /// <param name="_touchPoint">触摸点对象</param>
        /// <param name="_fingerId">触摸ID</param>
        /// <returns>协程</returns>
        private IEnumerator _fadeTouchPoint(GameObject _touchPoint, int _fingerId)
        {
            yield return new WaitForSeconds(_m_touchPointDuration);
            
            _returnTouchPointToPool(_touchPoint);
            _m_activeTouchPoints.Remove(_fingerId);
        }
        
        /// <summary>
        /// 涟漪动画协程
        /// </summary>
        /// <param name="_ripple">涟漪对象</param>
        /// <returns>协程</returns>
        private IEnumerator _animateRipple(GameObject _ripple)
        {
            float elapsedTime = 0f;
            Vector3 startScale = _ripple.transform.localScale;
            Vector3 endScale = startScale * 2f;
            
            while (elapsedTime < _m_rippleEffectDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / _m_rippleEffectDuration;
                
                _ripple.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
                
                // 调整透明度
                Renderer renderer = _ripple.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    Color color = renderer.material.color;
                    color.a = 1f - progress;
                    renderer.material.color = color;
                }
                
                yield return null;
            }
            
            _returnRippleToPool(_ripple);
        }
        
        /// <summary>
        /// 轨迹淡出协程
        /// </summary>
        /// <param name="_trail">轨迹对象</param>
        /// <returns>协程</returns>
        private IEnumerator _fadeTrail(GameObject _trail)
        {
            yield return new WaitForSeconds(_m_trailFadeTime);
            _returnTrailToPool(_trail);
        }
        
        /// <summary>
        /// 清除所有视觉效果
        /// </summary>
        private void _clearAllVisualEffects()
        {
            // 清除触摸点
            foreach (var touchPoint in _m_activeTouchPoints.Values)
            {
                _returnTouchPointToPool(touchPoint);
            }
            _m_activeTouchPoints.Clear();
            
            // 清除涟漪效果
            foreach (var ripple in _m_activeRipples)
            {
                _returnRippleToPool(ripple);
            }
            _m_activeRipples.Clear();
            
            // 清除拖拽轨迹
            foreach (var trail in _m_activeDragTrails.Values)
            {
                _returnTrailToPool(trail);
            }
            _m_activeDragTrails.Clear();
        }
        
        #endregion
        
        #region 私有方法 - 事件处理
        
        /// <summary>
        /// 方块选择事件处理
        /// </summary>
        /// <param name="_piece">选中的方块</param>
        /// <param name="_playerId">玩家ID</param>
        private void _onPieceSelected(_IGamePiece _piece, int _playerId)
        {
            playHapticFeedback(FeedbackType.Light);
            playAudioFeedback(_m_touchSound);
        }
        
        /// <summary>
        /// 方块拖拽开始事件处理
        /// </summary>
        /// <param name="_piece">拖拽的方块</param>
        private void _onPieceDragStart(_IGamePiece _piece)
        {
            playHapticFeedback(FeedbackType.Medium);
            playAudioFeedback(_m_dragStartSound);
        }
        
        /// <summary>
        /// 方块拖拽结束事件处理
        /// </summary>
        /// <param name="_piece">拖拽的方块</param>
        /// <param name="_position">结束位置</param>
        private void _onPieceDragEnd(_IGamePiece _piece, Vector3 _position)
        {
            playHapticFeedback(FeedbackType.Light);
            playAudioFeedback(_m_dragEndSound);
        }
        
        /// <summary>
        /// 方块旋转事件处理
        /// </summary>
        /// <param name="_piece">旋转的方块</param>
        /// <param name="_playerId">玩家ID</param>
        private void _onPieceRotated(_IGamePiece _piece, int _playerId)
        {
            playHapticFeedback(FeedbackType.Medium);
            playAudioFeedback(_m_doubleTapSound);
        }
        
        /// <summary>
        /// 方块翻转事件处理
        /// </summary>
        /// <param name="_piece">翻转的方块</param>
        /// <param name="_playerId">玩家ID</param>
        private void _onPieceFlipped(_IGamePiece _piece, int _playerId)
        {
            playHapticFeedback(FeedbackType.Strong);
            playAudioFeedback(_m_longPressSound);
        }
        
        #endregion
        
        #region 私有方法 - 辅助功能
        
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
        
        #endregion
    }
}