using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
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
        
        [Header("统计信息")]
        /// <summary>总回合数文本</summary>
        [SerializeField] private Text _m_totalTurnsText;
        
        /// <summary>最高分文本</summary>
        [SerializeField] private Text _m_highestScoreText;
        
        /// <summary>平均分文本</summary>
        [SerializeField] private Text _m_averageScoreText;
        
        [Header("动画配置")]
        /// <summary>结果显示动画延迟</summary>
        [SerializeField] private float _m_resultAnimationDelay = 0.2f;
        
        /// <summary>是否启用结果动画</summary>
        [SerializeField] private bool _m_enableResultAnimation = true;
        
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
            
            // 显示统计信息
            _displayGameStatistics(_gameResults);
            
            // 显示玩家结果
            if (_m_enableResultAnimation)
            {
                StartCoroutine(_displayPlayerResultsAnimated(_gameResults.playerResults));
            }
            else
            {
                _displayPlayerResults(_gameResults.playerResults);
            }
            
            // 显示UI
            Show(true);
            
            // 播放游戏结束音效
            GameEvents.instance.onPlaySound?.Invoke("GameOver");
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[GameOverUI] 显示游戏结果，获胜者: {_gameResults.winnerId}");
            }
        }
        
        /// <summary>
        /// 显示简化的游戏结果（用于快速显示）
        /// </summary>
        /// <param name="_winnerId">获胜者ID</param>
        /// <param name="_playerResults">玩家结果</param>
        public void ShowSimpleResults(int _winnerId, List<PlayerResult> _playerResults)
        {
            var gameResults = new GameResults
            {
                winnerId = _winnerId,
                playerResults = _playerResults,
                gameDuration = 0f,
                totalTurns = 0
            };
            
            ShowGameResults(gameResults);
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
        /// 显示游戏统计信息
        /// </summary>
        /// <param name="_gameResults">游戏结果</param>
        private void _displayGameStatistics(GameResults _gameResults)
        {
            // 显示总回合数
            if (_m_totalTurnsText != null)
            {
                _m_totalTurnsText.text = $"总回合数: {_gameResults.totalTurns}";
            }
            
            // 计算并显示最高分
            if (_m_highestScoreText != null && _gameResults.playerResults.Count > 0)
            {
                int highestScore = _gameResults.playerResults.Max(p => p.finalScore);
                _m_highestScoreText.text = $"最高分: {highestScore}";
            }
            
            // 计算并显示平均分
            if (_m_averageScoreText != null && _gameResults.playerResults.Count > 0)
            {
                double averageScore = _gameResults.playerResults.Average(p => p.finalScore);
                _m_averageScoreText.text = $"平均分: {averageScore:F1}";
            }
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
        /// 带动画的显示玩家结果协程
        /// </summary>
        /// <param name="_playerResults">玩家结果列表</param>
        /// <returns>协程枚举器</returns>
        private System.Collections.IEnumerator _displayPlayerResultsAnimated(List<PlayerResult> _playerResults)
        {
            _clearPlayerResults();
            
            if (_m_playerResultsContainer == null || _m_playerResultPrefab == null)
            {
                Debug.LogWarning("[GameOverUI] 玩家结果容器或预制体未配置");
                yield break;
            }
            
            // 按分数排序
            var sortedResults = new List<PlayerResult>(_playerResults);
            sortedResults.Sort((a, b) => b.finalScore.CompareTo(a.finalScore));
            
            // 逐个创建结果UI，带动画效果
            for (int i = 0; i < sortedResults.Count; i++)
            {
                var resultObj = Instantiate(_m_playerResultPrefab, _m_playerResultsContainer);
                var resultUI = resultObj.GetComponent<PlayerResultUI>();
                
                if (resultUI != null)
                {
                    // 初始化为透明
                    var canvasGroup = resultObj.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = resultObj.AddComponent<CanvasGroup>();
                    }
                    canvasGroup.alpha = 0f;
                    
                    resultUI.Initialize(sortedResults[i], i + 1);
                    _m_playerResultUIs.Add(resultUI);
                    
                    // 淡入动画
                    StartCoroutine(_fadeInResult(canvasGroup));
                    
                    // 播放排名音效
                    if (i == 0)
                    {
                        GameEvents.instance.onPlaySound?.Invoke("FirstPlace");
                    }
                    else
                    {
                        GameEvents.instance.onPlaySound?.Invoke("PlayerRank");
                    }
                }
                
                // 等待动画延迟
                yield return new WaitForSeconds(_m_resultAnimationDelay);
            }
        }
        
        /// <summary>
        /// 结果淡入动画协程
        /// </summary>
        /// <param name="_canvasGroup">要淡入的CanvasGroup</param>
        /// <returns>协程枚举器</returns>
        private System.Collections.IEnumerator _fadeInResult(CanvasGroup _canvasGroup)
        {
            float duration = 0.5f;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
                yield return null;
            }
            
            _canvasGroup.alpha = 1f;
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
            _shareGameResults();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[GameOverUI] 分享结果");
            }
        }
        
        /// <summary>
        /// 分享游戏结果
        /// </summary>
        private void _shareGameResults()
        {
            try
            {
                // 生成分享文本
                string shareText = _generateShareText();
                
                // 在移动平台上使用原生分享
                #if UNITY_ANDROID || UNITY_IOS
                    // TODO: 实现原生分享功能
                    UIManager.instance?.ShowMessage("分享功能即将推出", MessageType.Info);
                #else
                    // 在其他平台上复制到剪贴板
                    GUIUtility.systemCopyBuffer = shareText;
                    UIManager.instance?.ShowMessage("游戏结果已复制到剪贴板", MessageType.Success);
                #endif
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameOverUI] 分享失败: {ex.Message}");
                UIManager.instance?.ShowMessage("分享失败", MessageType.Error);
            }
        }
        
        /// <summary>
        /// 生成分享文本
        /// </summary>
        /// <returns>分享文本</returns>
        private string _generateShareText()
        {
            var text = new System.Text.StringBuilder();
            text.AppendLine("🎮 Blokus游戏结果 🎮");
            text.AppendLine();
            
            if (_m_winnerText != null)
            {
                text.AppendLine(_m_winnerText.text);
            }
            
            if (_m_gameTimeText != null)
            {
                text.AppendLine(_m_gameTimeText.text);
            }
            
            text.AppendLine();
            text.AppendLine("📊 玩家排名:");
            
            for (int i = 0; i < _m_playerResultUIs.Count; i++)
            {
                var resultUI = _m_playerResultUIs[i];
                if (resultUI != null)
                {
	                PlayerResult result = resultUI.GetPlayerResult();

                    if (result != null)
						text.AppendLine($"{i + 1}. {result.playerName} - {result.finalScore}分");
                }
            }
            
            return text.ToString();
        }
        
        #endregion
        
        #region 公共方法 - 扩展功能
        
        /// <summary>
        /// 设置按钮的可见性
        /// </summary>
        /// <param name="_showPlayAgain">是否显示再玩一次按钮</param>
        /// <param name="_showMainMenu">是否显示主菜单按钮</param>
        /// <param name="_showShare">是否显示分享按钮</param>
        public void SetButtonVisibility(bool _showPlayAgain = true, bool _showMainMenu = true, bool _showShare = true)
        {
            if (_m_playAgainButton != null)
                _m_playAgainButton.gameObject.SetActive(_showPlayAgain);
            
            if (_m_mainMenuButton != null)
                _m_mainMenuButton.gameObject.SetActive(_showMainMenu);
            
            if (_m_shareButton != null)
                _m_shareButton.gameObject.SetActive(_showShare);
        }
        
        /// <summary>
        /// 获取获胜者信息
        /// </summary>
        /// <returns>获胜者文本</returns>
        public string GetWinnerText()
        {
            return _m_winnerText?.text ?? "";
        }
        
        /// <summary>
        /// 获取游戏时长文本
        /// </summary>
        /// <returns>游戏时长文本</returns>
        public string GetGameTimeText()
        {
            return _m_gameTimeText?.text ?? "";
        }
        
        /// <summary>
        /// 获取玩家结果数量
        /// </summary>
        /// <returns>玩家结果数量</returns>
        public int GetPlayerResultCount()
        {
            return _m_playerResultUIs.Count;
        }
        
        /// <summary>
        /// 设置动画配置
        /// </summary>
        /// <param name="_enableAnimation">是否启用动画</param>
        /// <param name="_animationDelay">动画延迟时间</param>
        public void SetAnimationConfig(bool _enableAnimation, float _animationDelay = 0.2f)
        {
            _m_enableResultAnimation = _enableAnimation;
            _m_resultAnimationDelay = Mathf.Max(0f, _animationDelay);
        }
        
        #endregion
    }
}