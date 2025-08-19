using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BlokusGame.Core.UI
{
    /// <summary>
    /// UI基础类 - 所有UI面板的基类
    /// 提供通用的UI操作和生命周期管理
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        [Header("UI基础配置")]
        /// <summary>UI面板的Canvas组件</summary>
        [SerializeField] protected CanvasGroup _m_canvasGroup;
        
        /// <summary>UI面板是否在启动时显示</summary>
        [SerializeField] protected bool _m_showOnStart = false;
        
        /// <summary>UI面板显示/隐藏的动画时长</summary>
        [SerializeField] protected float _m_animationDuration = 0.3f;
        
        /// <summary>是否启用详细日志</summary>
        [SerializeField] protected bool _m_enableDetailedLogging = false;
        
        /// <summary>UI排序等级，用于控制显示层级，数值越大显示层级越高</summary>
        [SerializeField] protected int _m_sortingOrder = 0;
        
        /// <summary>UI面板当前是否可见</summary>
        public bool isVisible { get; private set; } = false;
        
        /// <summary>UI面板是否正在执行动画</summary>
        public bool isAnimating { get; private set; } = false;
        
        /// <summary>获取UI排序等级</summary>
        public virtual int sortingOrder => _m_sortingOrder;
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法 - 初始化UI组件
        /// </summary>
        protected virtual void Awake()
        {
            _initializeUI();
        }
        
        /// <summary>
        /// Unity Start方法 - 设置初始状态
        /// </summary>
        protected virtual void Start()
        {
            if (_m_showOnStart)
            {
                Show(false); // 启动时显示，不播放动画
            }
            else
            {
                Hide(false); // 启动时隐藏，不播放动画
            }
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 清理资源
        /// </summary>
        protected virtual void OnDestroy()
        {
            _cleanup();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 显示UI面板
        /// </summary>
        /// <param name="_animated">是否播放动画</param>
        public virtual void Show(bool _animated = true)
        {
            if (isVisible && !isAnimating)
                return;
            
            gameObject.SetActive(true);
            
            if (_animated && _m_animationDuration > 0f)
            {
                StartCoroutine(_showAnimated());
            }
            else
            {
                _setVisibility(true, 1f);
            }
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[{GetType().Name}] UI面板显示，动画: {_animated}");
            }
        }
        
        /// <summary>
        /// 隐藏UI面板
        /// </summary>
        /// <param name="_animated">是否播放动画</param>
        public virtual void Hide(bool _animated = true)
        {
            if (!isVisible && !isAnimating)
                return;
            
            if (_animated && _m_animationDuration > 0f)
            {
                StartCoroutine(_hideAnimated());
            }
            else
            {
                _setVisibility(false, 0f);
                gameObject.SetActive(false);
            }
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[{GetType().Name}] UI面板隐藏，动画: {_animated}");
            }
        }
        
        /// <summary>
        /// 切换UI面板的显示状态
        /// </summary>
        /// <param name="_animated">是否播放动画</param>
        public virtual void Toggle(bool _animated = true)
        {
            if (isVisible)
            {
                Hide(_animated);
            }
            else
            {
                Show(_animated);
            }
        }
        
        /// <summary>
        /// 设置UI面板的交互状态
        /// </summary>
        /// <param name="_interactable">是否可交互</param>
        public virtual void SetInteractable(bool _interactable)
        {
            if (_m_canvasGroup != null)
            {
                _m_canvasGroup.interactable = _interactable;
                _m_canvasGroup.blocksRaycasts = _interactable;
            }
        }
        
        /// <summary>
        /// 设置UI面板的透明度
        /// </summary>
        /// <param name="_alpha">透明度值 (0-1)</param>
        public virtual void SetAlpha(float _alpha)
        {
            if (_m_canvasGroup != null)
            {
                _m_canvasGroup.alpha = Mathf.Clamp01(_alpha);
            }
        }
        
        #endregion
        
        #region 抽象方法 - 子类必须实现
        
        /// <summary>
        /// 初始化UI特定内容
        /// 子类重写此方法来初始化特定的UI元素
        /// </summary>
        protected abstract void InitializeUIContent();
        
        /// <summary>
        /// 处理UI显示时的逻辑
        /// 子类重写此方法来处理显示时的特定逻辑
        /// </summary>
        protected virtual void OnUIShown() { }
        
        /// <summary>
        /// 处理UI隐藏时的逻辑
        /// 子类重写此方法来处理隐藏时的特定逻辑
        /// </summary>
        protected virtual void OnUIHidden() { }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 初始化UI基础组件
        /// </summary>
        private void _initializeUI()
        {
            // 自动获取CanvasGroup组件
            if (_m_canvasGroup == null)
            {
                _m_canvasGroup = GetComponent<CanvasGroup>();
                if (_m_canvasGroup == null)
                {
                    _m_canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            
            // 调用子类的初始化方法
            InitializeUIContent();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[{GetType().Name}] UI基础组件初始化完成");
            }
        }
        
        /// <summary>
        /// 设置UI可见性和透明度
        /// </summary>
        /// <param name="_visible">是否可见</param>
        /// <param name="_alpha">透明度</param>
        private void _setVisibility(bool _visible, float _alpha)
        {
            isVisible = _visible;
            
            if (_m_canvasGroup != null)
            {
                _m_canvasGroup.alpha = _alpha;
                _m_canvasGroup.interactable = _visible;
                _m_canvasGroup.blocksRaycasts = _visible;
            }
            
            if (_visible)
            {
                OnUIShown();
            }
            else
            {
                OnUIHidden();
            }
        }
        
        /// <summary>
        /// 显示动画协程
        /// </summary>
        /// <returns>协程枚举器</returns>
        private IEnumerator _showAnimated()
        {
            isAnimating = true;
            
            float elapsedTime = 0f;
            float startAlpha = _m_canvasGroup != null ? _m_canvasGroup.alpha : 0f;
            
            while (elapsedTime < _m_animationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / _m_animationDuration;
                float currentAlpha = Mathf.Lerp(startAlpha, 1f, progress);
                
                if (_m_canvasGroup != null)
                {
                    _m_canvasGroup.alpha = currentAlpha;
                }
                
                yield return null;
            }
            
            _setVisibility(true, 1f);
            isAnimating = false;
        }
        
        /// <summary>
        /// 隐藏动画协程
        /// </summary>
        /// <returns>协程枚举器</returns>
        private IEnumerator _hideAnimated()
        {
            isAnimating = true;
            
            float elapsedTime = 0f;
            float startAlpha = _m_canvasGroup != null ? _m_canvasGroup.alpha : 1f;
            
            while (elapsedTime < _m_animationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / _m_animationDuration;
                float currentAlpha = Mathf.Lerp(startAlpha, 0f, progress);
                
                if (_m_canvasGroup != null)
                {
                    _m_canvasGroup.alpha = currentAlpha;
                }
                
                yield return null;
            }
            
            _setVisibility(false, 0f);
            gameObject.SetActive(false);
            isAnimating = false;
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        private void _cleanup()
        {
            // 停止所有协程
            StopAllCoroutines();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[{GetType().Name}] UI资源清理完成");
            }
        }
        
        #endregion
    }
}