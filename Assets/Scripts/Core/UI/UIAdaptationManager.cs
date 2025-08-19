using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace BlokusGame.Core.UI
{
    /// <summary>
    /// UI适配管理器 - 处理不同屏幕尺寸和方向的UI适配
    /// 负责安全区域适配、分辨率适配和屏幕方向变化处理
    /// 确保UI在各种移动设备上都能正确显示
    /// </summary>
    public class UIAdaptationManager : MonoBehaviour
    {
        [Header("适配配置")]
        /// <summary>是否启用安全区域适配</summary>
        [SerializeField] private bool _m_enableSafeAreaAdaptation = true;
        
        /// <summary>是否启用屏幕方向适配</summary>
        [SerializeField] private bool _m_enableOrientationAdaptation = true;
        
        /// <summary>是否启用分辨率适配</summary>
        [SerializeField] private bool _m_enableResolutionAdaptation = true;
        
        /// <summary>参考分辨率</summary>
        [SerializeField] private Vector2 _m_referenceResolution = new Vector2(1920, 1080);
        
        /// <summary>是否启用详细日志</summary>
        [SerializeField] private bool _m_enableDetailedLogging = false;
        
        [Header("安全区域适配")]
        /// <summary>需要适配安全区域的UI元素列表</summary>
        [SerializeField] private List<RectTransform> _m_safeAreaElements = new List<RectTransform>();
        
        /// <summary>安全区域边距</summary>
        [SerializeField] private Vector4 _m_safeAreaMargins = Vector4.zero;
        
        // 私有字段
        /// <summary>当前屏幕方向</summary>
        private ScreenOrientation _m_currentOrientation;
        
        /// <summary>当前屏幕分辨率</summary>
        private Vector2 _m_currentResolution;
        
        /// <summary>当前安全区域</summary>
        private Rect _m_currentSafeArea;
        
        /// <summary>Canvas缩放器组件</summary>
        private CanvasScaler _m_canvasScaler;
        
        /// <summary>单例实例</summary>
        public static UIAdaptationManager instance { get; private set; }
        
        /// <summary>当前屏幕宽高比</summary>
        public float currentAspectRatio => (float)Screen.width / Screen.height;
        
        /// <summary>是否为竖屏模式</summary>
        public bool isPortrait => Screen.height > Screen.width;
        
        /// <summary>是否为横屏模式</summary>
        public bool isLandscape => Screen.width > Screen.height;
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法 - 初始化单例和基础设置
        /// </summary>
        private void Awake()
        {
            _initializeSingleton();
            _initializeAdaptationManager();
        }
        
        /// <summary>
        /// Unity Start方法 - 开始适配处理
        /// </summary>
        private void Start()
        {
            _performInitialAdaptation();
        }
        
        /// <summary>
        /// Unity Update方法 - 检测屏幕变化
        /// </summary>
        private void Update()
        {
            _checkForScreenChanges();
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 清理资源
        /// </summary>
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 添加需要安全区域适配的UI元素
        /// </summary>
        /// <param name="_element">UI元素的RectTransform</param>
        public void addSafeAreaElement(RectTransform _element)
        {
            if (_element != null && !_m_safeAreaElements.Contains(_element))
            {
                _m_safeAreaElements.Add(_element);
                _applySafeAreaToElement(_element);
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[UIAdaptationManager] 添加安全区域适配元素: {_element.name}");
                }
            }
        }
        
        /// <summary>
        /// 移除安全区域适配的UI元素
        /// </summary>
        /// <param name="_element">UI元素的RectTransform</param>
        public void removeSafeAreaElement(RectTransform _element)
        {
            if (_element != null && _m_safeAreaElements.Contains(_element))
            {
                _m_safeAreaElements.Remove(_element);
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[UIAdaptationManager] 移除安全区域适配元素: {_element.name}");
                }
            }
        }
        
        /// <summary>
        /// 强制执行UI适配
        /// </summary>
        public void forceAdaptation()
        {
            _performFullAdaptation();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[UIAdaptationManager] 强制执行UI适配");
            }
        }
        
        /// <summary>
        /// 获取安全区域相对于屏幕的比例
        /// </summary>
        /// <returns>安全区域比例 (x, y, width, height)</returns>
        public Vector4 getSafeAreaRatio()
        {
            Rect safeArea = Screen.safeArea;
            Vector4 ratio = new Vector4(
                safeArea.x / Screen.width,
                safeArea.y / Screen.height,
                safeArea.width / Screen.width,
                safeArea.height / Screen.height
            );
            
            return ratio;
        }
        
        /// <summary>
        /// 获取当前屏幕信息
        /// </summary>
        /// <returns>屏幕信息字符串</returns>
        public string getScreenInfo()
        {
            return $"分辨率: {Screen.width}x{Screen.height}, " +
                   $"方向: {Screen.orientation}, " +
                   $"安全区域: {Screen.safeArea}, " +
                   $"DPI: {Screen.dpi}";
        }
        
        #endregion
        
        #region 私有方法 - 初始化
        
        /// <summary>
        /// 初始化单例
        /// </summary>
        private void _initializeSingleton()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Debug.LogWarning("[UIAdaptationManager] 检测到重复的UIAdaptationManager实例，销毁当前实例");
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 初始化适配管理器
        /// </summary>
        private void _initializeAdaptationManager()
        {
            // 获取Canvas缩放器组件
            _m_canvasScaler = GetComponentInParent<CanvasScaler>();
            if (_m_canvasScaler == null)
            {
                _m_canvasScaler = FindObjectOfType<CanvasScaler>();
            }
            
            // 记录初始状态
            _m_currentOrientation = Screen.orientation;
            _m_currentResolution = new Vector2(Screen.width, Screen.height);
            _m_currentSafeArea = Screen.safeArea;
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[UIAdaptationManager] 适配管理器初始化完成 - {getScreenInfo()}");
            }
        }
        
        /// <summary>
        /// 执行初始适配
        /// </summary>
        private void _performInitialAdaptation()
        {
            _performFullAdaptation();
        }
        
        #endregion
        
        #region 私有方法 - 适配处理
        
        /// <summary>
        /// 检测屏幕变化
        /// </summary>
        private void _checkForScreenChanges()
        {
            bool needsAdaptation = false;
            
            // 检测方向变化
            if (_m_enableOrientationAdaptation && Screen.orientation != _m_currentOrientation)
            {
                _m_currentOrientation = Screen.orientation;
                needsAdaptation = true;
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[UIAdaptationManager] 屏幕方向变化: {_m_currentOrientation}");
                }
            }
            
            // 检测分辨率变化
            Vector2 currentResolution = new Vector2(Screen.width, Screen.height);
            if (_m_enableResolutionAdaptation && currentResolution != _m_currentResolution)
            {
                _m_currentResolution = currentResolution;
                needsAdaptation = true;
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[UIAdaptationManager] 分辨率变化: {_m_currentResolution}");
                }
            }
            
            // 检测安全区域变化
            if (_m_enableSafeAreaAdaptation && Screen.safeArea != _m_currentSafeArea)
            {
                _m_currentSafeArea = Screen.safeArea;
                needsAdaptation = true;
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[UIAdaptationManager] 安全区域变化: {_m_currentSafeArea}");
                }
            }
            
            if (needsAdaptation)
            {
                _performFullAdaptation();
            }
        }
        
        /// <summary>
        /// 执行完整的UI适配
        /// </summary>
        private void _performFullAdaptation()
        {
            if (_m_enableResolutionAdaptation)
            {
                _adaptResolution();
            }
            
            if (_m_enableSafeAreaAdaptation)
            {
                _adaptSafeArea();
            }
            
            if (_m_enableOrientationAdaptation)
            {
                _adaptOrientation();
            }
        }
        
        /// <summary>
        /// 适配分辨率
        /// </summary>
        private void _adaptResolution()
        {
            if (_m_canvasScaler == null) return;
            
            // 根据屏幕宽高比调整匹配模式
            float screenAspect = currentAspectRatio;
            float referenceAspect = _m_referenceResolution.x / _m_referenceResolution.y;
            
            if (screenAspect > referenceAspect)
            {
                // 屏幕更宽，匹配高度
                _m_canvasScaler.matchWidthOrHeight = 1f;
            }
            else
            {
                // 屏幕更高，匹配宽度
                _m_canvasScaler.matchWidthOrHeight = 0f;
            }
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[UIAdaptationManager] 分辨率适配完成，匹配值: {_m_canvasScaler.matchWidthOrHeight}");
            }
        }
        
        /// <summary>
        /// 适配安全区域
        /// </summary>
        private void _adaptSafeArea()
        {
            foreach (var element in _m_safeAreaElements)
            {
                if (element != null)
                {
                    _applySafeAreaToElement(element);
                }
            }
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[UIAdaptationManager] 安全区域适配完成，适配了 {_m_safeAreaElements.Count} 个元素");
            }
        }
        
        /// <summary>
        /// 将安全区域应用到指定UI元素
        /// </summary>
        /// <param name="_element">UI元素</param>
        private void _applySafeAreaToElement(RectTransform _element)
        {
            if (_element == null) return;
            
            Rect safeArea = Screen.safeArea;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            
            // 计算安全区域的相对位置和大小
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            
            anchorMin.x /= screenSize.x;
            anchorMin.y /= screenSize.y;
            anchorMax.x /= screenSize.x;
            anchorMax.y /= screenSize.y;
            
            // 应用边距
            anchorMin.x += _m_safeAreaMargins.x / screenSize.x;
            anchorMin.y += _m_safeAreaMargins.y / screenSize.y;
            anchorMax.x -= _m_safeAreaMargins.z / screenSize.x;
            anchorMax.y -= _m_safeAreaMargins.w / screenSize.y;
            
            // 设置锚点
            _element.anchorMin = anchorMin;
            _element.anchorMax = anchorMax;
            
            // 重置偏移
            _element.offsetMin = Vector2.zero;
            _element.offsetMax = Vector2.zero;
        }
        
        /// <summary>
        /// 适配屏幕方向
        /// </summary>
        private void _adaptOrientation()
        {
            // 根据屏幕方向调整UI布局
            // 这里可以添加特定的方向适配逻辑
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[UIAdaptationManager] 屏幕方向适配完成: {Screen.orientation}");
            }
        }
        
        #endregion
        
        #region 调试方法
        
        /// <summary>
        /// 在编辑器中绘制调试信息
        /// </summary>
        private void OnGUI()
        {
            if (!_m_enableDetailedLogging) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("UI适配调试信息", GUI.skin.box);
            GUILayout.Label($"屏幕分辨率: {Screen.width} x {Screen.height}");
            GUILayout.Label($"屏幕方向: {Screen.orientation}");
            GUILayout.Label($"安全区域: {Screen.safeArea}");
            GUILayout.Label($"宽高比: {currentAspectRatio:F2}");
            GUILayout.Label($"DPI: {Screen.dpi}");
            GUILayout.Label($"适配元素数量: {_m_safeAreaElements.Count}");
            GUILayout.EndArea();
        }
        
        #endregion
    }
}