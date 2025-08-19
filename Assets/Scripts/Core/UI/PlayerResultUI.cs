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
        
        #endregion
    }
}