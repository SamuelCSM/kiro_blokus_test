using UnityEngine;
using UnityEngine.UI;
using BlokusGame.Core.Managers;
using BlokusGame.Core.Data;
using BlokusGame.Core.Events;

namespace BlokusGame.Core.UI
{
    /// <summary>
    /// 主菜单UI - 游戏的主要入口界面
    /// 提供游戏模式选择、设置和退出功能
    /// </summary>
    public class MainMenuUI : UIBase
    {
        [Header("主菜单UI配置")]
        /// <summary>游戏标题文本</summary>
        [SerializeField] private Text _m_titleText;
        
        /// <summary>版本信息文本</summary>
        [SerializeField] private Text _m_versionText;
        
        [Header("菜单按钮")]
        /// <summary>单人游戏按钮</summary>
        [SerializeField] private Button _m_singlePlayerButton;
        
        /// <summary>多人游戏按钮</summary>
        [SerializeField] private Button _m_multiPlayerButton;
        
        /// <summary>教程按钮</summary>
        [SerializeField] private Button _m_tutorialButton;
        
        /// <summary>设置按钮</summary>
        [SerializeField] private Button _m_settingsButton;
        
        /// <summary>退出游戏按钮</summary>
        [SerializeField] private Button _m_exitButton;
        
        [Header("单人游戏选项")]
        /// <summary>单人游戏选项面板</summary>
        [SerializeField] private GameObject _m_singlePlayerPanel;
        
        /// <summary>简单AI按钮</summary>
        [SerializeField] private Button _m_easyAIButton;
        
        /// <summary>中等AI按钮</summary>
        [SerializeField] private Button _m_mediumAIButton;
        
        /// <summary>困难AI按钮</summary>
        [SerializeField] private Button _m_hardAIButton;
        
        /// <summary>返回主菜单按钮</summary>
        [SerializeField] private Button _m_backToMainButton;
        
        [Header("多人游戏选项")]
        /// <summary>多人游戏选项面板</summary>
        [SerializeField] private GameObject _m_multiPlayerPanel;
        
        /// <summary>本地多人按钮</summary>
        [SerializeField] private Button _m_localMultiPlayerButton;
        
        /// <summary>在线多人按钮</summary>
        [SerializeField] private Button _m_onlineMultiPlayerButton;
        
        /// <summary>返回主菜单按钮（多人）</summary>
        [SerializeField] private Button _m_backToMainButton2;
        
        /// <summary>当前显示的子面板</summary>
        private GameObject _m_currentSubPanel;
        
        #region UIBase实现
        
        /// <summary>
        /// 初始化UI特定内容
        /// </summary>
        protected override void InitializeUIContent()
        {
            _setupUI();
            _bindEvents();
        }
        
        /// <summary>
        /// 处理UI显示时的逻辑
        /// </summary>
        protected override void OnUIShown()
        {
            _updateUI();
            _showMainMenu();
        }
        
        /// <summary>
        /// 处理UI隐藏时的逻辑
        /// </summary>
        protected override void OnUIHidden()
        {
            _hideAllSubPanels();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 显示单人游戏选项
        /// </summary>
        public void ShowSinglePlayerOptions()
        {
            _showSubPanel(_m_singlePlayerPanel);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[MainMenuUI] 显示单人游戏选项");
            }
        }
        
        /// <summary>
        /// 显示多人游戏选项
        /// </summary>
        public void ShowMultiPlayerOptions()
        {
            _showSubPanel(_m_multiPlayerPanel);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[MainMenuUI] 显示多人游戏选项");
            }
        }
        
        /// <summary>
        /// 返回主菜单
        /// </summary>
        public void BackToMainMenu()
        {
            _showMainMenu();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[MainMenuUI] 返回主菜单");
            }
        }
        
        #endregion
        
        #region 私有方法 - 初始化
        
        /// <summary>
        /// 设置UI元素
        /// </summary>
        private void _setupUI()
        {
            // 设置标题和版本信息
            if (_m_titleText != null)
            {
                _m_titleText.text = "Blokus";
            }
            
            if (_m_versionText != null)
            {
                _m_versionText.text = $"版本 {Application.version}";
            }
            
            // 初始化子面板状态
            _hideAllSubPanels();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[MainMenuUI] UI元素设置完成");
            }
        }
        
        /// <summary>
        /// 绑定按钮事件
        /// </summary>
        private void _bindEvents()
        {
            // 主菜单按钮事件
            if (_m_singlePlayerButton != null)
                _m_singlePlayerButton.onClick.AddListener(ShowSinglePlayerOptions);
            
            if (_m_multiPlayerButton != null)
                _m_multiPlayerButton.onClick.AddListener(ShowMultiPlayerOptions);
            
            if (_m_tutorialButton != null)
                _m_tutorialButton.onClick.AddListener(_onTutorialClicked);
            
            if (_m_settingsButton != null)
                _m_settingsButton.onClick.AddListener(_onSettingsClicked);
            
            if (_m_exitButton != null)
                _m_exitButton.onClick.AddListener(_onExitClicked);
            
            // 单人游戏按钮事件
            if (_m_easyAIButton != null)
                _m_easyAIButton.onClick.AddListener(() => _startSinglePlayerGame(AIDifficulty.Easy));
            
            if (_m_mediumAIButton != null)
                _m_mediumAIButton.onClick.AddListener(() => _startSinglePlayerGame(AIDifficulty.Medium));
            
            if (_m_hardAIButton != null)
                _m_hardAIButton.onClick.AddListener(() => _startSinglePlayerGame(AIDifficulty.Hard));
            
            if (_m_backToMainButton != null)
                _m_backToMainButton.onClick.AddListener(BackToMainMenu);
            
            // 多人游戏按钮事件
            if (_m_localMultiPlayerButton != null)
                _m_localMultiPlayerButton.onClick.AddListener(_onLocalMultiPlayerClicked);
            
            if (_m_onlineMultiPlayerButton != null)
                _m_onlineMultiPlayerButton.onClick.AddListener(_onOnlineMultiPlayerClicked);
            
            if (_m_backToMainButton2 != null)
                _m_backToMainButton2.onClick.AddListener(BackToMainMenu);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[MainMenuUI] 按钮事件绑定完成");
            }
        }
        
        #endregion
        
        #region 私有方法 - UI控制
        
        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void _updateUI()
        {
            // 根据游戏状态更新按钮可用性
            bool canStartGame = GameManager.instance != null && !GameManager.instance.isGameActive;
            
            if (_m_singlePlayerButton != null)
                _m_singlePlayerButton.interactable = canStartGame;
            
            if (_m_multiPlayerButton != null)
                _m_multiPlayerButton.interactable = canStartGame;
            
            // 检查在线功能是否可用
            if (_m_onlineMultiPlayerButton != null)
            {
                _m_onlineMultiPlayerButton.interactable = canStartGame && _isOnlineFeatureAvailable();
            }
        }
        
        /// <summary>
        /// 显示主菜单
        /// </summary>
        private void _showMainMenu()
        {
            _hideAllSubPanels();
            _updateUI();
        }
        
        /// <summary>
        /// 显示子面板
        /// </summary>
        /// <param name="_panel">要显示的子面板</param>
        private void _showSubPanel(GameObject _panel)
        {
            if (_panel == null) return;
            
            _hideAllSubPanels();
            _panel.SetActive(true);
            _m_currentSubPanel = _panel;
        }
        
        /// <summary>
        /// 隐藏所有子面板
        /// </summary>
        private void _hideAllSubPanels()
        {
            if (_m_singlePlayerPanel != null)
                _m_singlePlayerPanel.SetActive(false);
            
            if (_m_multiPlayerPanel != null)
                _m_multiPlayerPanel.SetActive(false);
            
            _m_currentSubPanel = null;
        }
        
        #endregion
        
        #region 私有方法 - 事件处理
        
        /// <summary>
        /// 开始单人游戏
        /// </summary>
        /// <param name="_difficulty">AI难度</param>
        private void _startSinglePlayerGame(AIDifficulty _difficulty)
        {
            if (GameManager.instance == null)
            {
                Debug.LogError("[MainMenuUI] GameManager实例不存在，无法开始游戏");
                UIManager.instance?.ShowMessage("游戏管理器未初始化", MessageType.Error);
                return;
            }
            
            // 创建游戏配置
            var gameConfig = new GameConfiguration
            {
                gameMode = GameMode.SinglePlayerVsAI,
                playerCount = 2, // 玩家 vs AI
                aiDifficulty = _difficulty,
                enableTutorial = false
            };
            
            // 开始游戏
            GameManager.instance.startNewGame(gameConfig.playerCount, gameConfig.gameMode);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[MainMenuUI] 开始单人游戏，AI难度: {_difficulty}");
            }
        }
        
        /// <summary>
        /// 处理教程按钮点击
        /// </summary>
        private void _onTutorialClicked()
        {
            if (GameManager.instance == null)
            {
                Debug.LogError("[MainMenuUI] GameManager实例不存在，无法开始教程");
                return;
            }
            
            // 创建教程配置
            var tutorialConfig = new GameConfiguration
            {
                gameMode = GameMode.Tutorial,
                playerCount = 2,
                aiDifficulty = AIDifficulty.Easy,
                enableTutorial = true
            };
            
            // 开始教程
            GameManager.instance.startNewGame(tutorialConfig.playerCount, tutorialConfig.gameMode);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[MainMenuUI] 开始教程模式");
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
                Debug.Log("[MainMenuUI] 打开设置界面");
            }
        }
        
        /// <summary>
        /// 处理本地多人游戏按钮点击
        /// </summary>
        private void _onLocalMultiPlayerClicked()
        {
            if (GameManager.instance == null)
            {
                Debug.LogError("[MainMenuUI] GameManager实例不存在，无法开始游戏");
                return;
            }
            
            // 创建本地多人游戏配置
            var gameConfig = new GameConfiguration
            {
                gameMode = GameMode.LocalMultiplayer,
                playerCount = 4, // 默认4人游戏
                aiDifficulty = AIDifficulty.Medium,
                enableTutorial = false
            };
            
            // 开始游戏
            GameManager.instance.startNewGame(gameConfig.playerCount, gameConfig.gameMode);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[MainMenuUI] 开始本地多人游戏");
            }
        }
        
        /// <summary>
        /// 处理在线多人游戏按钮点击
        /// </summary>
        private void _onOnlineMultiPlayerClicked()
        {
            // TODO: 实现在线多人游戏功能
            UIManager.instance?.ShowMessage("在线多人功能即将推出", MessageType.Info);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[MainMenuUI] 在线多人游戏功能暂未实现");
            }
        }
        
        /// <summary>
        /// 处理退出游戏按钮点击
        /// </summary>
        private void _onExitClicked()
        {
            // 显示确认对话框
            UIManager.instance?.ShowMessage("确定要退出游戏吗？", MessageType.Warning);
            
            // TODO: 实现确认对话框功能
            // 暂时直接退出
            _exitGame();
        }
        
        /// <summary>
        /// 退出游戏
        /// </summary>
        private void _exitGame()
        {
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[MainMenuUI] 退出游戏");
            }
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        #endregion
        
        #region 私有方法 - 辅助功能
        
        /// <summary>
        /// 检查在线功能是否可用
        /// </summary>
        /// <returns>是否可用</returns>
        private bool _isOnlineFeatureAvailable()
        {
            // TODO: 实现在线功能检查
            // 检查网络连接、服务器状态等
            return false; // 暂时返回false
        }
        
        #endregion
    }

    /// <summary>
    /// 游戏配置类
    /// </summary>
    [System.Serializable]
    public class GameConfiguration
    {
        /// <summary>游戏模式</summary>
        public GameMode gameMode;
        
        /// <summary>玩家数量</summary>
        public int playerCount = 2;
        
        /// <summary>AI难度</summary>
        public AIDifficulty aiDifficulty = AIDifficulty.Medium;
        
        /// <summary>是否启用教程</summary>
        public bool enableTutorial = false;
        
        /// <summary>游戏时间限制（分钟，0表示无限制）</summary>
        public int timeLimit = 0;
        
        /// <summary>是否启用音效</summary>
        public bool enableSoundEffects = true;
        
        /// <summary>是否启用背景音乐</summary>
        public bool enableBackgroundMusic = true;
    }
}