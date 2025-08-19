using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BlokusGame.Core.Managers;
using BlokusGame.Core.Data;
using BlokusGame.Core.Events;

namespace BlokusGame.Core.UI
{
    /// <summary>
    /// 游戏结束UI - 显示游戏结果和统计信息
    /// 提供重新开始、返回主菜单等选项
    /// </summary>
    public class GameOverUI : UIBase
    {
        [Header("游戏结束UI配置")]
        /// <summary>标题文本</summary>
        [SerializeField] private Text _m_titleText;
        
        /// <summary>获胜者文本</summary>
        [SerializeField] private Text _m_winnerText;
        
        /// <summary>游戏时长文本</summary>
        [SerializeField] private Text _m_gameTimeText;
        
        [Header("玩家结果显示")]
        /// <summary>玩家结果容器</summary>
        [SerializeField] private Transform _m_playerResultsContainer;
        
        /// <summary>玩家结果项预制体</summary>
        [SerializeField] private GameObject _m_playerResultPrefab;
        
        [Header("按钮")]
        /// <summary>重新开始按钮</summary>
        [SerializeField] private Button _m_playAgainButton;
        
        /// <summary>返回主菜单按钮</summary>
        [SerializeField] private Button _m_mainMenuButton;
        
        /// <summary>分享结果按钮</summary>
        [SerializeField] private Button _m_shareButton;
        
        // 私有字段
        /// <summary>玩家结果UI列表</summary>
        private List<PlayerResultUI> _m_playerResultUIs = new List<PlayerResultUI>();
        
        #region UIBase实现
        
        /// <summary>
        /// 初始化UI特定内容
        /// </summary>
        protected override void InitializeUIContent()
        {
            _setupGameOverUI();
            _bindEvents();
        }
        
        /// <summary>
        /// 处理UI显示时的逻辑
        /// </summary>
        protected override void OnUIShown()
        {
            // 游戏结束时的逻辑
        }
        
        /// <summary>
        /// 处理UI隐藏时的逻辑
        /// </summary>
        protected override void OnUIHidden()
        {
            _clearPlayerResults();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 显示游戏结果
        /// </summary>
        /// <param name="_gameResults">游戏结果数据</param>
        public void ShowGameResults(GameResults _gameResults)
        {
            if (_gameResults == null)
            {
                Debug.LogError("[GameOverUI] 游戏结果数据为空");
                return;
            }
            
            // 设置标题
            if (_m_titleText != null)
            {
                _m_titleText.text = "游戏结束";
            }
            
            // 设置获胜者信息
            _displayWinner(_gameResults);
            
            // 设置游戏时长
            _displayGameTime(_gameResults.gameDuration);
            
            // 显示玩家结果
            _displayPlayerResults(_gameResults.playerResults);
            
            // 显示UI
            Show(true);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[GameOverUI] 显示游戏结果，获胜者: {_gameResults.winnerId}");
            }
        }
        
        #endregion
        
        #region 私有方法 - 初始化
        
        /// <summary>
        /// 设置游戏结束UI
        /// </summary>
        private void _setupGameOverUI()
        {
            // 清除现有结果
            _clearPlayerResults();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[GameOverUI] 游戏结束UI设置完成");
            }
        }
        
        /// <summary>
        /// 绑定事件
        /// </summary>
        private void _bindEvents()
        {
            if (_m_playAgainButton != null)
                _m_playAgainButton.onClick.AddListener(_onPlayAgainClicked);
            
            if (_m_mainMenuButton != null)
                _m_mainMenuButton.onClick.AddListener(_onMainMenuClicked);
            
            if (_m_shareButton != null)
                _m_shareButton.onClick.AddListener(_onShareClicked);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[GameOverUI] 事件绑定完成");
            }
        }
        
        #endregion
        
        #region 私有方法 - 结果显示
        
        /// <summary>
        /// 显示获胜者信息
        /// </summary>
        /// <param name="_gameResults">游戏结果</param>
        private void _displayWinner(GameResults _gameResults)
        {
            if (_m_winnerText == null) return;
            
            if (_gameResults.winnerId >= 0)
            {
                var winnerResult = _gameResults.playerResults.Find(p => p.playerId == _gameResults.winnerId);
                if (winnerResult != null)
                {
                    _m_winnerText.text = $"🎉 {winnerResult.playerName} 获胜！";
                    _m_winnerText.color = winnerResult.playerColor;
                }
                else
                {
                    _m_winnerText.text = $"🎉 玩家 {_gameResults.winnerId + 1} 获胜！";
                }
            }
            else
            {
                _m_winnerText.text = "🤝 平局！";
            }
        }
        
        /// <summary>
        /// 显示游戏时长
        /// </summary>
        /// <param name="_gameDuration">游戏时长（秒）</param>
        private void _displayGameTime(float _gameDuration)
        {
            if (_m_gameTimeText == null) return;
            
            int minutes = Mathf.FloorToInt(_gameDuration / 60f);
            int seconds = Mathf.FloorToInt(_gameDuration % 60f);
            _m_gameTimeText.text = $"游戏时长: {minutes:00}:{seconds:00}";
        }
        
        /// <summary>
        /// 显示玩家结果
        /// </summary>
        /// <param name="_playerResults">玩家结果列表</param>
        private void _displayPlayerResults(List<PlayerResult> _playerResults)
        {
            _clearPlayerResults();
            
            if (_m_playerResultsContainer == null || _m_playerResultPrefab == null)
            {
                Debug.LogWarning("[GameOverUI] 玩家结果容器或预制体未配置");
                return;
            }
            
            // 按分数排序
            var sortedResults = new List<PlayerResult>(_playerResults);
            sortedResults.Sort((a, b) => b.finalScore.CompareTo(a.finalScore));
            
            // 创建结果UI
            for (int i = 0; i < sortedResults.Count; i++)
            {
                var resultObj = Instantiate(_m_playerResultPrefab, _m_playerResultsContainer);
                var resultUI = resultObj.GetComponent<PlayerResultUI>();
                
                if (resultUI != null)
                {
                    resultUI.Initialize(sortedResults[i], i + 1);
                    _m_playerResultUIs.Add(resultUI);
                }
            }
        }
        
        /// <summary>
        /// 清除玩家结果显示
        /// </summary>
        private void _clearPlayerResults()
        {
            foreach (var resultUI in _m_playerResultUIs)
            {
                if (resultUI != null)
                {
                    Destroy(resultUI.gameObject);
                }
            }
            _m_playerResultUIs.Clear();
        }
        
        #endregion
        
        #region 事件处理
        
        /// <summary>
        /// 处理再玩一次按钮点击
        /// </summary>
        private void _onPlayAgainClicked()
        {
            GameManager.instance?.resetGame();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[GameOverUI] 再玩一次");
            }
        }
        
        /// <summary>
        /// 处理返回主菜单按钮点击
        /// </summary>
        private void _onMainMenuClicked()
        {
            GameManager.instance?.exitToMainMenu();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[GameOverUI] 返回主菜单");
            }
        }
        
        /// <summary>
        /// 处理分享按钮点击
        /// </summary>
        private void _onShareClicked()
        {
            // TODO: 实现分享功能
            UIManager.instance?.ShowMessage("分享功能即将推出", MessageType.Info);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[GameOverUI] 分享结果");
            }
        }
        
        #endregion
    }
}