using UnityEngine;
using UnityEngine.UI;

namespace BlokusGame.Core.UI
{
    /// <summary>
    /// 玩家信息UI组件
    /// 显示单个玩家的基本信息和状态
    /// </summary>
    public class PlayerInfoUI : MonoBehaviour
    {
        [Header("玩家信息UI组件")]
        /// <summary>玩家名称文本</summary>
        [SerializeField] private Text _m_playerNameText;
        
        /// <summary>玩家分数文本</summary>
        [SerializeField] private Text _m_playerScoreText;
        
        /// <summary>剩余方块数量文本</summary>
        [SerializeField] private Text _m_remainingPiecesText;
        
        /// <summary>玩家颜色指示器</summary>
        [SerializeField] private Image _m_playerColorIndicator;
        
        /// <summary>高亮背景</summary>
        [SerializeField] private Image _m_highlightBackground;
        
        /// <summary>玩家头像</summary>
        [SerializeField] private Image _m_playerAvatar;
        
        /// <summary>AI标识</summary>
        [SerializeField] private GameObject _m_aiIndicator;
        
        [Header("高亮配置")]
        /// <summary>高亮颜色</summary>
        [SerializeField] private Color _m_highlightColor = Color.yellow;
        
        /// <summary>普通背景颜色</summary>
        [SerializeField] private Color _m_normalColor = Color.white;
        
        // 私有字段
        /// <summary>玩家ID</summary>
        private int _m_playerId = -1;
        
        /// <summary>是否高亮显示</summary>
        private bool _m_isHighlighted = false;
        
        #region 公共方法
        
        /// <summary>
        /// 初始化玩家信息UI
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_playerName">玩家名称</param>
        /// <param name="_playerColor">玩家颜色</param>
        /// <param name="_initialScore">初始分数</param>
        /// <param name="_initialPieces">初始方块数量</param>
        /// <param name="_isAI">是否为AI玩家</param>
        public void Initialize(int _playerId, string _playerName, Color _playerColor, int _initialScore = 0, int _initialPieces = 21, bool _isAI = false)
        {
            _m_playerId = _playerId;
            
            // 设置玩家名称
            if (_m_playerNameText != null)
            {
                _m_playerNameText.text = _playerName;
            }
            
            // 设置玩家颜色
            if (_m_playerColorIndicator != null)
            {
                _m_playerColorIndicator.color = _playerColor;
            }
            
            // 设置初始分数
            UpdateScore(_initialScore);
            
            // 设置初始方块数量
            UpdateRemainingPieces(_initialPieces);
            
            // 设置AI标识
            if (_m_aiIndicator != null)
            {
                _m_aiIndicator.SetActive(_isAI);
            }
            
            // 设置初始高亮状态
            SetHighlight(false);
        }
        
        /// <summary>
        /// 更新玩家分数
        /// </summary>
        /// <param name="_score">新分数</param>
        public void UpdateScore(int _score)
        {
            if (_m_playerScoreText != null)
            {
                _m_playerScoreText.text = $"分数: {_score}";
            }
        }
        
        /// <summary>
        /// 更新剩余方块数量
        /// </summary>
        /// <param name="_remainingPieces">剩余方块数量</param>
        public void UpdateRemainingPieces(int _remainingPieces)
        {
            if (_m_remainingPiecesText != null)
            {
                _m_remainingPiecesText.text = $"剩余: {_remainingPieces}";
            }
        }
        
        /// <summary>
        /// 设置高亮状态
        /// </summary>
        /// <param name="_highlighted">是否高亮</param>
        public void SetHighlight(bool _highlighted)
        {
            _m_isHighlighted = _highlighted;
            
            if (_m_highlightBackground != null)
            {
                _m_highlightBackground.color = _highlighted ? _m_highlightColor : _m_normalColor;
            }
        }
        
        /// <summary>
        /// 设置玩家头像
        /// </summary>
        /// <param name="_avatarSprite">头像精灵</param>
        public void SetPlayerAvatar(Sprite _avatarSprite)
        {
            if (_m_playerAvatar != null)
            {
                _m_playerAvatar.sprite = _avatarSprite;
                _m_playerAvatar.gameObject.SetActive(_avatarSprite != null);
            }
        }
        
        /// <summary>
        /// 设置玩家状态（活跃/非活跃）
        /// </summary>
        /// <param name="_active">是否活跃</param>
        public void SetPlayerActive(bool _active)
        {
            // 调整透明度来表示活跃状态
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = _active ? 1f : 0.5f;
        }
        
        /// <summary>
        /// 获取玩家ID
        /// </summary>
        /// <returns>玩家ID</returns>
        public int GetPlayerId()
        {
            return _m_playerId;
        }
        
        /// <summary>
        /// 获取是否高亮状态
        /// </summary>
        /// <returns>是否高亮</returns>
        public bool IsHighlighted()
        {
            return _m_isHighlighted;
        }
        
        #endregion
    }
}