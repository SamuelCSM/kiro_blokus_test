using UnityEngine;
using UnityEngine.UI;
using BlokusGame.Core.Managers;

namespace BlokusGame.Core.UI
{
    /// <summary>
    /// 暂停菜单UI - 游戏暂停时显示的菜单界面
    /// 提供继续游戏、设置、退出等选项
    /// </summary>
    public class PauseMenuUI : UIBase
    {
        [Header("暂停菜单UI配置")]
        /// <summary>标题文本</summary>
        [SerializeField] private Text _m_titleText;
        
        /// <summary>继续游戏按钮</summary>
        [SerializeField] private Button _m_resumeButton;
        
        /// <summary>重新开始按钮</summary>
        [SerializeField] private Button _m_restartButton;
        
        /// <summary>设置按钮</summary>
        [SerializeField] private Button _m_settingsButton;
        
        /// <summary>退出到主菜单按钮</summary>
        [SerializeField] private Button _m_exitToMenuButton;
        
        [Header("确认对话框")]
        /// <summary>确认对话框面板</summary>
        [SerializeField] private GameObject _m_confirmationPanel;
        
        /// <summary>确认对话框标题文本</summary>
        [SerializeField] private Text _m_confirmationTitleText;
        
        /// <summary>确认对话框内容文本</summary>
        [SerializeField] private Text _m_confirmationContentText;
        
        /// <summary>确认按钮</summary>
        [SerializeField] private Button _m_confirmButton;
        
        /// <summary>取消按钮</summary>
        [SerializeField] private Button _m_cancelButton;
        
        // 私有字段
        /// <summary>当前确认操作的回调</summary>
        private System.Action _m_currentConfirmAction;
        
        #region UIBase实现
        
        /// <summary>
        /// 初始化UI特定内容
        /// </summary>
        protected override void InitializeUIContent()
        {
            _setupPauseMenuUI();
            _bindEvents();
        }
        
        /// <summary>
        /// 处理UI显示时的逻辑
        /// </summary>
        protected override void OnUIShown()
        {
            // 暂停游戏时间
            Time.timeScale = 0f;
        }
        
        /// <summary>
        /// 处理UI隐藏时的逻辑
        /// </summary>
        protected override void OnUIHidden()
        {
            // 恢复游戏时间
            Time.timeScale = 1f;
        }
        
        #endregion
        
        #region 私有方法 - 初始化
        
        /// <summary>
        /// 设置暂停菜单UI
        /// </summary>
        private void _setupPauseMenuUI()
        {
            if (_m_titleText != null)
            {
                _m_titleText.text = "游戏暂停";
            }
            
            _initializeConfirmationDialog();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[PauseMenuUI] 暂停菜单UI设置完成");
            }
        }
        
        /// <summary>
        /// 绑定事件
        /// </summary>
        private void _bindEvents()
        {
            if (_m_resumeButton != null)
                _m_resumeButton.onClick.AddListener(_onResumeClicked);
            
            if (_m_restartButton != null)
                _m_restartButton.onClick.AddListener(_onRestartClicked);
            
            if (_m_settingsButton != null)
                _m_settingsButton.onClick.AddListener(_onSettingsClicked);
            
            if (_m_exitToMenuButton != null)
                _m_exitToMenuButton.onClick.AddListener(_onExitToMenuClicked);
            
            // 绑定确认对话框事件
            if (_m_confirmButton != null)
                _m_confirmButton.onClick.AddListener(_onConfirmClicked);
            
            if (_m_cancelButton != null)
                _m_cancelButton.onClick.AddListener(_onCancelClicked);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[PauseMenuUI] 事件绑定完成");
            }
        }
        
        /// <summary>
        /// 初始化确认对话框
        /// </summary>
        private void _initializeConfirmationDialog()
        {
            if (_m_confirmationPanel != null)
            {
                _m_confirmationPanel.SetActive(false);
            }
        }
        
        #endregion
        
        #region 事件处理
        
        /// <summary>
        /// 处理继续游戏按钮点击
        /// </summary>
        private void _onResumeClicked()
        {
            GameManager.instance?.resumeGame();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[PauseMenuUI] 继续游戏");
            }
        }
        
        /// <summary>
        /// 处理重新开始按钮点击
        /// </summary>
        private void _onRestartClicked()
        {
            _showConfirmationDialog(
                "重新开始游戏",
                "确定要重新开始当前游戏吗？当前进度将会丢失。",
                () => {
                    GameManager.instance?.resetGame();
                    Hide(true);
                }
            );
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[PauseMenuUI] 请求重新开始游戏");
            }
        }
        
        /// <summary>
        /// 处理设置按钮点击
        /// </summary>
        private void _onSettingsClicked()
        {
            UIManager.instance?.ShowPanel<SettingsUI>();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[PauseMenuUI] 打开设置");
            }
        }
        
        /// <summary>
        /// 处理退出到主菜单按钮点击
        /// </summary>
        private void _onExitToMenuClicked()
        {
            _showConfirmationDialog(
                "退出游戏",
                "确定要退出到主菜单吗？当前游戏进度将会丢失。",
                () => {
                    GameManager.instance?.exitToMainMenu();
                    Hide(true);
                }
            );
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[PauseMenuUI] 请求退出到主菜单");
            }
        }
        
        /// <summary>
        /// 处理确认按钮点击
        /// </summary>
        private void _onConfirmClicked()
        {
            _hideConfirmationDialog();
            _m_currentConfirmAction?.Invoke();
            _m_currentConfirmAction = null;
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[PauseMenuUI] 确认操作");
            }
        }
        
        /// <summary>
        /// 处理取消按钮点击
        /// </summary>
        private void _onCancelClicked()
        {
            _hideConfirmationDialog();
            _m_currentConfirmAction = null;
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[PauseMenuUI] 取消操作");
            }
        }
        
        #endregion
        
        #region 确认对话框方法
        
        /// <summary>
        /// 显示确认对话框
        /// </summary>
        /// <param name="_title">对话框标题</param>
        /// <param name="_content">对话框内容</param>
        /// <param name="_confirmAction">确认时执行的操作</param>
        private void _showConfirmationDialog(string _title, string _content, System.Action _confirmAction)
        {
            if (_m_confirmationPanel == null) return;
            
            _m_currentConfirmAction = _confirmAction;
            
            if (_m_confirmationTitleText != null)
                _m_confirmationTitleText.text = _title;
            
            if (_m_confirmationContentText != null)
                _m_confirmationContentText.text = _content;
            
            _m_confirmationPanel.SetActive(true);
            
            // 禁用主菜单按钮
            _setMainButtonsInteractable(false);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[PauseMenuUI] 显示确认对话框: {_title}");
            }
        }
        
        /// <summary>
        /// 隐藏确认对话框
        /// </summary>
        private void _hideConfirmationDialog()
        {
            if (_m_confirmationPanel != null)
            {
                _m_confirmationPanel.SetActive(false);
            }
            
            // 重新启用主菜单按钮
            _setMainButtonsInteractable(true);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[PauseMenuUI] 隐藏确认对话框");
            }
        }
        
        /// <summary>
        /// 设置主菜单按钮的交互状态
        /// </summary>
        /// <param name="_interactable">是否可交互</param>
        private void _setMainButtonsInteractable(bool _interactable)
        {
            if (_m_resumeButton != null)
                _m_resumeButton.interactable = _interactable;
            
            if (_m_restartButton != null)
                _m_restartButton.interactable = _interactable;
            
            if (_m_settingsButton != null)
                _m_settingsButton.interactable = _interactable;
            
            if (_m_exitToMenuButton != null)
                _m_exitToMenuButton.interactable = _interactable;
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 快速恢复游戏（外部调用）
        /// </summary>
        public void QuickResume()
        {
            _onResumeClicked();
        }
        
        /// <summary>
        /// 检查是否有确认对话框正在显示
        /// </summary>
        /// <returns>是否有确认对话框显示</returns>
        public bool IsConfirmationDialogShowing()
        {
            return _m_confirmationPanel != null && _m_confirmationPanel.activeInHierarchy;
        }
        
        #endregion
    }
}