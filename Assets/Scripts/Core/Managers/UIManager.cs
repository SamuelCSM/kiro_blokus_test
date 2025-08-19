using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using BlokusGame.Core.Events;
using BlokusGame.Core.UI;
using BlokusGame.Core.Data;

namespace BlokusGame.Core.Managers
{
    /// <summary>
    /// UI管理器 - 统一管理所有UI面板和界面切换
    /// 负责UI面板的显示、隐藏、切换和状态管理
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI管理器配置")]
        /// <summary>是否启用详细日志</summary>
        [SerializeField] private bool _m_enableDetailedLogging = false;
        
        /// <summary>UI面板切换的默认动画时长</summary>
        [SerializeField] private float _m_defaultTransitionDuration = 0.3f;
        
        /// <summary>是否在启动时自动初始化</summary>
        [SerializeField] private bool _m_autoInitializeOnStart = true;
        
        [Header("UI面板引用")]
        /// <summary>主菜单UI面板</summary>
        [SerializeField] private MainMenuUI _m_mainMenuUI;
        
        /// <summary>游戏内UI面板</summary>
        [SerializeField] private GameplayUI _m_gameplayUI;
        
        /// <summary>设置UI面板</summary>
        [SerializeField] private UIBase _m_settingsUI;
        
        /// <summary>暂停菜单UI面板</summary>
        [SerializeField] private UIBase _m_pauseMenuUI;
        
        /// <summary>游戏结束UI面板</summary>
        [SerializeField] private UIBase _m_gameOverUI;
        
        /// <summary>加载界面UI面板</summary>
        [SerializeField] private UIBase _m_loadingUI;
        
        /// <summary>消息提示UI面板</summary>
        [SerializeField] private UIBase _m_messageUI;
        
        // 私有字段
        /// <summary>所有注册的UI面板字典</summary>
        private Dictionary<System.Type, UIBase> _m_uiPanels = new Dictionary<System.Type, UIBase>();
        
        /// <summary>当前活跃的UI面板</summary>
        private UIBase _m_currentActivePanel;
        
        /// <summary>UI面板切换历史栈</summary>
        private Stack<UIBase> _m_panelHistory = new Stack<UIBase>();
        
        /// <summary>UI管理器单例实例</summary>
        public static UIManager instance { get; private set; }
        
        /// <summary>当前活跃的UI面板类型</summary>
        public System.Type currentActivePanelType => _m_currentActivePanel?.GetType();
        
        /// <summary>是否有UI面板正在执行切换动画</summary>
        public bool isTransitioning { get; private set; } = false;
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法 - 初始化单例和基础组件
        /// </summary>
        private void Awake()
        {
            _initializeSingleton();
            _initializeUIManager();
        }
        
        /// <summary>
        /// Unity Start方法 - 自动初始化和事件订阅
        /// </summary>
        private void Start()
        {
            if (_m_autoInitializeOnStart)
            {
                _autoDiscoverUIPanels();
                _subscribeToEvents();
                _showInitialUI();
            }
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 清理资源和取消事件订阅
        /// </summary>
        private void OnDestroy()
        {
            _unsubscribeFromEvents();
            _cleanup();
        }
        
        #endregion
        
        #region 公共方法 - UI面板管理
        
        /// <summary>
        /// 显示指定类型的UI面板
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <param name="_animated">是否播放切换动画</param>
        /// <param name="_addToHistory">是否添加到历史栈</param>
        public void ShowPanel<T>(bool _animated = true, bool _addToHistory = true) where T : UIBase
        {
            ShowPanel(typeof(T), _animated, _addToHistory);
        }
        
        /// <summary>
        /// 显示指定类型的UI面板
        /// </summary>
        /// <param name="_panelType">UI面板类型</param>
        /// <param name="_animated">是否播放切换动画</param>
        /// <param name="_addToHistory">是否添加到历史栈</param>
        public void ShowPanel(System.Type _panelType, bool _animated = true, bool _addToHistory = true)
        {
            if (!_m_uiPanels.ContainsKey(_panelType))
            {
                Debug.LogError($"[UIManager] 未找到类型为 {_panelType.Name} 的UI面板");
                return;
            }
            
            var targetPanel = _m_uiPanels[_panelType];
            
            if (_m_currentActivePanel == targetPanel)
            {
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[UIManager] UI面板 {_panelType.Name} 已经是当前活跃面板");
                }
                return;
            }
            
            StartCoroutine(_switchToPanel(targetPanel, _animated, _addToHistory));
        }
        
        /// <summary>
        /// 隐藏指定类型的UI面板
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <param name="_animated">是否播放动画</param>
        public void HidePanel<T>(bool _animated = true) where T : UIBase
        {
            HidePanel(typeof(T), _animated);
        }
        
        /// <summary>
        /// 隐藏指定类型的UI面板
        /// </summary>
        /// <param name="_panelType">UI面板类型</param>
        /// <param name="_animated">是否播放动画</param>
        public void HidePanel(System.Type _panelType, bool _animated = true)
        {
            if (!_m_uiPanels.ContainsKey(_panelType))
            {
                Debug.LogError($"[UIManager] 未找到类型为 {_panelType.Name} 的UI面板");
                return;
            }
            
            var targetPanel = _m_uiPanels[_panelType];
            targetPanel.Hide(_animated);
            
            if (_m_currentActivePanel == targetPanel)
            {
                _m_currentActivePanel = null;
            }
        }
        
        /// <summary>
        /// 返回到上一个UI面板
        /// </summary>
        /// <param name="_animated">是否播放动画</param>
        /// <returns>是否成功返回</returns>
        public bool GoBack(bool _animated = true)
        {
            if (_m_panelHistory.Count == 0)
            {
                if (_m_enableDetailedLogging)
                {
                    Debug.Log("[UIManager] 没有可返回的UI面板历史");
                }
                return false;
            }
            
            var previousPanel = _m_panelHistory.Pop();
            StartCoroutine(_switchToPanel(previousPanel, _animated, false));
            return true;
        }
        
        /// <summary>
        /// 清空UI面板历史栈
        /// </summary>
        public void ClearHistory()
        {
            _m_panelHistory.Clear();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[UIManager] UI面板历史栈已清空");
            }
        }
        
        /// <summary>
        /// 隐藏所有UI面板
        /// </summary>
        /// <param name="_animated">是否播放动画</param>
        public void HideAllPanels(bool _animated = true)
        {
            foreach (var panel in _m_uiPanels.Values)
            {
                if (panel.isVisible)
                {
                    panel.Hide(_animated);
                }
            }
            
            _m_currentActivePanel = null;
            _m_panelHistory.Clear();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[UIManager] 所有UI面板已隐藏");
            }
        }
        
        #endregion
        
        #region 公共方法 - 特殊UI功能
        
        /// <summary>
        /// 显示消息提示
        /// </summary>
        /// <param name="_message">消息内容</param>
        /// <param name="_messageType">消息类型</param>
        /// <param name="_duration">显示时长（秒），0表示需要手动关闭</param>
        public void ShowMessage(string _message, MessageType _messageType = MessageType.Info, float _duration = 3f)
        {
            // TODO: 实现消息UI显示逻辑
            Debug.Log($"[UIManager] 显示消息: {_message} (类型: {_messageType})");
        }
        
        /// <summary>
        /// 显示加载界面
        /// </summary>
        /// <param name="_loadingText">加载文本</param>
        /// <param name="_showProgress">是否显示进度条</param>
        public void ShowLoading(string _loadingText = "加载中...", bool _showProgress = false)
        {
            // TODO: 实现加载UI显示逻辑
            Debug.Log($"[UIManager] 显示加载界面: {_loadingText}");
        }
        
        /// <summary>
        /// 隐藏加载界面
        /// </summary>
        public void HideLoading()
        {
            // TODO: 实现加载UI隐藏逻辑
            Debug.Log("[UIManager] 隐藏加载界面");
        }
        
        /// <summary>
        /// 更新加载进度
        /// </summary>
        /// <param name="_progress">进度值 (0-1)</param>
        /// <param name="_progressText">进度文本</param>
        public void UpdateLoadingProgress(float _progress, string _progressText = "")
        {
            // TODO: 实现加载进度更新逻辑
            Debug.Log($"[UIManager] 更新加载进度: {_progress:P0} - {_progressText}");
        }
        
        #endregion
        
        #region 公共方法 - UI状态查询
        
        /// <summary>
        /// 检查指定类型的UI面板是否可见
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <returns>是否可见</returns>
        public bool IsPanelVisible<T>() where T : UIBase
        {
            return IsPanelVisible(typeof(T));
        }
        
        /// <summary>
        /// 检查指定类型的UI面板是否可见
        /// </summary>
        /// <param name="_panelType">UI面板类型</param>
        /// <returns>是否可见</returns>
        public bool IsPanelVisible(System.Type _panelType)
        {
            if (_m_uiPanels.ContainsKey(_panelType))
            {
                return _m_uiPanels[_panelType].isVisible;
            }
            return false;
        }
        
        /// <summary>
        /// 获取指定类型的UI面板实例
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <returns>UI面板实例</returns>
        public T GetPanel<T>() where T : UIBase
        {
            var panelType = typeof(T);
            if (_m_uiPanels.ContainsKey(panelType))
            {
                return _m_uiPanels[panelType] as T;
            }
            return null;
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
                Debug.LogWarning("[UIManager] 检测到重复的UIManager实例，销毁当前实例");
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 初始化UI管理器
        /// </summary>
        private void _initializeUIManager()
        {
            _m_uiPanels.Clear();
            _m_panelHistory.Clear();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[UIManager] UI管理器初始化完成");
            }
        }
        
        /// <summary>
        /// 自动发现和注册UI面板
        /// </summary>
        private void _autoDiscoverUIPanels()
        {
            // 注册手动配置的UI面板
            _registerPanel(_m_mainMenuUI);
            _registerPanel(_m_gameplayUI);
            _registerPanel(_m_settingsUI);
            _registerPanel(_m_pauseMenuUI);
            _registerPanel(_m_gameOverUI);
            _registerPanel(_m_loadingUI);
            _registerPanel(_m_messageUI);
            
            // 自动发现子对象中的UI面板
            var uiPanels = GetComponentsInChildren<UIBase>(true);
            foreach (var panel in uiPanels)
            {
                _registerPanel(panel);
            }
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[UIManager] 自动发现并注册了 {_m_uiPanels.Count} 个UI面板");
            }
        }
        
        /// <summary>
        /// 注册UI面板
        /// </summary>
        /// <param name="_panel">要注册的UI面板</param>
        private void _registerPanel(UIBase _panel)
        {
            if (_panel == null) return;
            
            var panelType = _panel.GetType();
            if (!_m_uiPanels.ContainsKey(panelType))
            {
                _m_uiPanels[panelType] = _panel;
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[UIManager] 注册UI面板: {panelType.Name}");
                }
            }
        }
        
        /// <summary>
        /// 显示初始UI
        /// </summary>
        private void _showInitialUI()
        {
            // 默认显示主菜单
            if (_m_mainMenuUI != null)
            {
                ShowPanel<MainMenuUI>(false, false);
            }
        }
        
        #endregion
        
        #region 私有方法 - UI切换
        
        /// <summary>
        /// 切换到指定UI面板的协程
        /// </summary>
        /// <param name="_targetPanel">目标UI面板</param>
        /// <param name="_animated">是否播放动画</param>
        /// <param name="_addToHistory">是否添加到历史栈</param>
        /// <returns>协程枚举器</returns>
        private IEnumerator _switchToPanel(UIBase _targetPanel, bool _animated, bool _addToHistory)
        {
            if (isTransitioning)
            {
                yield break;
            }
            
            isTransitioning = true;
            
            // 将当前面板添加到历史栈
            if (_addToHistory && _m_currentActivePanel != null)
            {
                _m_panelHistory.Push(_m_currentActivePanel);
            }
            
            // 隐藏当前面板
            if (_m_currentActivePanel != null && _m_currentActivePanel != _targetPanel)
            {
                _m_currentActivePanel.Hide(_animated);
                
                if (_animated)
                {
                    yield return new WaitForSeconds(_m_defaultTransitionDuration);
                }
            }
            
            // 显示目标面板
            _m_currentActivePanel = _targetPanel;
            _targetPanel.Show(_animated);
            
            if (_animated)
            {
                yield return new WaitForSeconds(_m_defaultTransitionDuration);
            }
            
            isTransitioning = false;
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[UIManager] 切换到UI面板: {_targetPanel.GetType().Name}");
            }
        }
        
        #endregion
        
        #region 私有方法 - 事件处理
        
        /// <summary>
        /// 订阅游戏事件
        /// </summary>
        private void _subscribeToEvents()
        {
            GameEvents.instance.onGameStateChanged += _onGameStateChanged;
            GameEvents.instance.onShowMessage += _onShowMessage;
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[UIManager] 游戏事件订阅完成");
            }
        }
        
        /// <summary>
        /// 取消游戏事件订阅
        /// </summary>
        private void _unsubscribeFromEvents()
        {
            GameEvents.instance.onGameStateChanged -= _onGameStateChanged;
            GameEvents.instance.onShowMessage -= _onShowMessage;
        }
        
        /// <summary>
        /// 处理游戏状态变更事件
        /// </summary>
        /// <param name="_oldState">旧的游戏状态</param>
        /// <param name="_newState">新的游戏状态</param>
        private void _onGameStateChanged(GameState _oldState, GameState _newState)
        {
            switch (_newState)
            {
                case GameState.MainMenu:
                    ShowPanel<MainMenuUI>();
                    break;
                case GameState.GamePlaying:
                    ShowPanel<GameplayUI>();
                    break;
                case GameState.GamePaused:
                    // TODO: 显示暂停菜单
                    Debug.Log("[UIManager] 游戏暂停，显示暂停菜单");
                    break;
                case GameState.GameEnded:
                    // TODO: 显示游戏结束界面
                    Debug.Log("[UIManager] 游戏结束，显示结束界面");
                    break;
                case GameState.Loading:
                    ShowLoading();
                    break;
            }
        }
        
        /// <summary>
        /// 处理显示消息事件
        /// </summary>
        /// <param name="_message">消息内容</param>
        /// <param name="_messageType">消息类型</param>
        private void _onShowMessage(string _message, MessageType _messageType)
        {
            ShowMessage(_message, _messageType);
        }
        
        #endregion
        
        #region 私有方法 - 清理
        
        /// <summary>
        /// 清理资源
        /// </summary>
        private void _cleanup()
        {
            StopAllCoroutines();
            _m_uiPanels.Clear();
            _m_panelHistory.Clear();
            
            if (instance == this)
            {
                instance = null;
            }
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[UIManager] UI管理器资源清理完成");
            }
        }
        
        #endregion
    }
}