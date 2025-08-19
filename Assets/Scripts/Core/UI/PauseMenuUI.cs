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
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[PauseMenuUI] 事件绑定完成");
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
            // TODO: 显示确认对话框
            GameManager.instance?.resetGame();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[PauseMenuUI] 重新开始游戏");
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
            // TODO: 显示确认对话框
            GameManager.instance?.exitToMainMenu();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[PauseMenuUI] 退出到主菜单");
            }
        }
        
        #endregion
    }
}