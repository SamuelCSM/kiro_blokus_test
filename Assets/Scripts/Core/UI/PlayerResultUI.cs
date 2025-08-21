using UnityEngine;
using UnityEngine.UI;
using BlokusGame.Core.Data;

namespace BlokusGame.Core.UI
{
    /// <summary>
    /// 玩家结果UI组件
    /// 显示游戏结束时单个玩家的详细结果信息
    /// </summary>
    public class PlayerResultUI : MonoBehaviour
    {
        [Header("玩家结果UI组件")]
        /// <summary>排名文本</summary>
        [SerializeField] private Text _m_rankText;
        
        /// <summary>玩家名称文本</summary>
        [SerializeField] private Text _m_playerNameText;
        
        /// <summary>最终分数文本</summary>
        [SerializeField] private Text _m_finalScoreText;
        
        /// <summary>已放置方块数量文本</summary>
        [SerializeField] private Text _m_placedPiecesText;
        
        /// <summary>剩余方块数量文本</summary>
        [SerializeField] private Text _m_remainingPiecesText;
        
        /// <summary>玩家颜色指示器</summary>
        [SerializeField] private Image _m_playerColorIndicator;
        
        /// <summary>排名图标</summary>
        [SerializeField] private Image _m_rankIcon;
        
        /// <summary>AI标识</summary>
        [SerializeField] private GameObject _m_aiIndicator;
        
        /// <summary>获胜者标识</summary>
        [SerializeField] private GameObject _m_winnerIndicator;
        
        [Header("排名图标配置")]
        /// <summary>第一名图标</summary>
        [SerializeField] private Sprite _m_firstPlaceIcon;
        
        /// <summary>第二名图标</summary>
        [SerializeField] private Sprite _m_secondPlaceIcon;
        
        /// <summary>第三名图标</summary>
        [SerializeField] private Sprite _m_thirdPlaceIcon;
        
        [Header("颜色配置")]
        /// <summary>获胜者文本颜色</summary>
        [SerializeField] private Color _m_winnerTextColor = Color.yellow;
        
        /// <summary>普通文本颜色</summary>
        [SerializeField] private Color _m_normalTextColor = Color.white;
        
        [Header("动画配置")]
        /// <summary>是否启用入场动画</summary>
        [SerializeField] private bool _m_enableEntranceAnimation = true;
        
        /// <summary>动画持续时间</summary>
        [SerializeField] private float _m_animationDuration = 0.5f;
        
        /// <summary>动画延迟时间</summary>
        [SerializeField] private float _m_animationDelay = 0f;
        
        [Header("详细统计显示")]
        /// <summary>游戏时长文本</summary>
        [SerializeField] private Text _m_gameTimeText;
        
        /// <summary>平均每回合时间文本</summary>
        [SerializeField] private Text _m_avgTurnTimeText;
        
        /// <summary>最大连续放置文本</summary>
        [SerializeField] private Text _m_maxStreakText;
        
        // 私有字段
        /// <summary>玩家结果数据</summary>
        private PlayerResult _m_playerResult;
        
        /// <summary>排名</summary>
        private int _m_rank;
        
        #region 公共方法
        
        /// <summary>
        /// 初始化玩家结果UI
        /// </summary>
        /// <param name="_playerResult">玩家结果数据</param>
        /// <param name="_rank">排名</param>
        public void Initialize(PlayerResult _playerResult, int _rank)
        {
            _m_playerResult = _playerResult;
            _m_rank = _rank;
            
            if (_playerResult == null)
            {
                Debug.LogError("[PlayerResultUI] 玩家结果数据为空");
                return;
            }
            
            // 设置排名
            _setRank(_rank);
            
            // 设置玩家信息
            _setPlayerInfo(_playerResult);
            
            // 设置分数信息
            _setScoreInfo(_playerResult);
            
            // 设置方块信息
            _setPieceInfo(_playerResult);
            
            // 设置特殊标识
            _setSpecialIndicators(_playerResult, _rank);
            
            // 设置详细统计信息
            _setDetailedStats(_playerResult);
        }
        
        /// <summary>
        /// 更新玩家结果数据
        /// </summary>
        /// <param name="_playerResult">新的玩家结果数据</param>
        /// <param name="_rank">新的排名</param>
        public void UpdateResult(PlayerResult _playerResult, int _rank)
        {
            Initialize(_playerResult, _rank);
        }
        
        /// <summary>
        /// 获取玩家结果数据
        /// </summary>
        /// <returns>玩家结果数据</returns>
        public PlayerResult GetPlayerResult()
        {
            return _m_playerResult;
        }
        
        /// <summary>
        /// 获取排名
        /// </summary>
        /// <returns>排名</returns>
        public int GetRank()
        {
            return _m_rank;
        }
        
        /// <summary>
        /// 播放入场动画
        /// </summary>
        public void PlayEntranceAnimation()
        {
            if (!_m_enableEntranceAnimation)
                return;
            
            StartCoroutine(_playEntranceAnimationCoroutine());
        }
        
        /// <summary>
        /// 高亮显示结果（用于获胜者等特殊情况）
        /// </summary>
        /// <param name="_highlight">是否高亮</param>
        public void SetHighlight(bool _highlight)
        {
            if (_highlight)
            {
                StartCoroutine(_playHighlightAnimation());
            }
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 设置排名显示
        /// </summary>
        /// <param name="_rank">排名</param>
        private void _setRank(int _rank)
        {
            // 设置排名文本
            if (_m_rankText != null)
            {
                _m_rankText.text = $"#{_rank}";
            }
            
            // 设置排名图标
            if (_m_rankIcon != null)
            {
                Sprite rankSprite = _getRankIcon(_rank);
                if (rankSprite != null)
                {
                    _m_rankIcon.sprite = rankSprite;
                    _m_rankIcon.gameObject.SetActive(true);
                }
                else
                {
                    _m_rankIcon.gameObject.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// 设置玩家信息
        /// </summary>
        /// <param name="_playerResult">玩家结果数据</param>
        private void _setPlayerInfo(PlayerResult _playerResult)
        {
            // 设置玩家名称
            if (_m_playerNameText != null)
            {
                _m_playerNameText.text = _playerResult.playerName;
                
                // 根据是否获胜设置文本颜色
                bool isWinner = _m_rank == 1;
                _m_playerNameText.color = isWinner ? _m_winnerTextColor : _m_normalTextColor;
            }
            
            // 设置玩家颜色指示器
            if (_m_playerColorIndicator != null)
            {
                _m_playerColorIndicator.color = _playerResult.playerColor;
            }
        }
        
        /// <summary>
        /// 设置分数信息
        /// </summary>
        /// <param name="_playerResult">玩家结果数据</param>
        private void _setScoreInfo(PlayerResult _playerResult)
        {
            if (_m_finalScoreText != null)
            {
                _m_finalScoreText.text = $"{_playerResult.finalScore} 分";
                
                // 根据是否获胜设置文本颜色
                bool isWinner = _m_rank == 1;
                _m_finalScoreText.color = isWinner ? _m_winnerTextColor : _m_normalTextColor;
            }
        }
        
        /// <summary>
        /// 设置方块信息
        /// </summary>
        /// <param name="_playerResult">玩家结果数据</param>
        private void _setPieceInfo(PlayerResult _playerResult)
        {
            // 设置已放置方块数量
            if (_m_placedPiecesText != null)
            {
                _m_placedPiecesText.text = $"已放置: {_playerResult.placedPieces}";
            }
            
            // 设置剩余方块数量
            if (_m_remainingPiecesText != null)
            {
                _m_remainingPiecesText.text = $"剩余: {_playerResult.remainingPieces}";
            }
        }
        
        /// <summary>
        /// 设置特殊标识
        /// </summary>
        /// <param name="_playerResult">玩家结果数据</param>
        /// <param name="_rank">排名</param>
        private void _setSpecialIndicators(PlayerResult _playerResult, int _rank)
        {
            // 设置AI标识
            if (_m_aiIndicator != null)
            {
                _m_aiIndicator.SetActive(_playerResult.isAI);
            }
            
            // 设置获胜者标识
            if (_m_winnerIndicator != null)
            {
                _m_winnerIndicator.SetActive(_rank == 1);
            }
        }
        
        /// <summary>
        /// 设置详细统计信息
        /// </summary>
        /// <param name="_playerResult">玩家结果数据</param>
        private void _setDetailedStats(PlayerResult _playerResult)
        {
            // 设置游戏时长
            if (_m_gameTimeText != null)
            {
                float gameTimeMinutes = _playerResult.totalPlayTime / 60f;
                _m_gameTimeText.text = $"游戏时长: {gameTimeMinutes:F1}分钟";
            }
            
            // 设置平均每回合时间
            if (_m_avgTurnTimeText != null)
            {
                _m_avgTurnTimeText.text = $"平均回合: {_playerResult.averageTurnTime:F1}秒";
            }
            
            // 设置放置效率统计
            if (_m_maxStreakText != null)
            {
                float efficiency = _playerResult.remainingPieces > 0 ? 
                    (float)_playerResult.placedPieces / 21f * 100f : 100f;
                _m_maxStreakText.text = $"完成度: {efficiency:F0}%";
            }
        }
        
        /// <summary>
        /// 获取排名对应的图标
        /// </summary>
        /// <param name="_rank">排名</param>
        /// <returns>排名图标</returns>
        private Sprite _getRankIcon(int _rank)
        {
            switch (_rank)
            {
                case 1:
                    return _m_firstPlaceIcon;
                case 2:
                    return _m_secondPlaceIcon;
                case 3:
                    return _m_thirdPlaceIcon;
                default:
                    return null;
            }
        }
        
        /// <summary>
        /// 入场动画协程
        /// </summary>
        /// <returns>协程枚举器</returns>
        private System.Collections.IEnumerator _playEntranceAnimationCoroutine()
        {
            // 等待延迟时间
            if (_m_animationDelay > 0f)
            {
                yield return new WaitForSeconds(_m_animationDelay);
            }
            
            // 获取或添加CanvasGroup组件
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // 设置初始状态
            canvasGroup.alpha = 0f;
            transform.localScale = Vector3.zero;
            
            // 执行动画
            float elapsedTime = 0f;
            while (elapsedTime < _m_animationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / _m_animationDuration;
                
                // 使用缓动函数
                float easedProgress = _easeOutBack(progress);
                
                canvasGroup.alpha = progress;
                transform.localScale = Vector3.one * easedProgress;
                
                yield return null;
            }
            
            // 确保最终状态
            canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;
        }
        
        /// <summary>
        /// 高亮动画协程
        /// </summary>
        /// <returns>协程枚举器</returns>
        private System.Collections.IEnumerator _playHighlightAnimation()
        {
            // 获取或添加CanvasGroup组件
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // 闪烁效果
            for (int i = 0; i < 3; i++)
            {
                // 变亮
                float elapsedTime = 0f;
                float duration = 0.2f;
                
                while (elapsedTime < duration)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    float progress = elapsedTime / duration;
                    
                    canvasGroup.alpha = Mathf.Lerp(1f, 1.5f, progress);
                    
                    yield return null;
                }
                
                // 变暗
                elapsedTime = 0f;
                while (elapsedTime < duration)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    float progress = elapsedTime / duration;
                    
                    canvasGroup.alpha = Mathf.Lerp(1.5f, 1f, progress);
                    
                    yield return null;
                }
            }
            
            // 确保最终状态
            canvasGroup.alpha = 1f;
        }
        
        /// <summary>
        /// 缓动函数：回弹效果
        /// </summary>
        /// <param name="_t">时间参数 (0-1)</param>
        /// <returns>缓动后的值</returns>
        private float _easeOutBack(float _t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            
            return 1f + c3 * Mathf.Pow(_t - 1f, 3f) + c1 * Mathf.Pow(_t - 1f, 2f);
        }
        
        #endregion
    }
}