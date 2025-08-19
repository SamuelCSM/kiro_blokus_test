using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BlokusGame.Core.Interfaces;

namespace BlokusGame.Core.UI
{
    /// <summary>
    /// 方块图标UI组件
    /// 显示方块的缩略图并处理点击事件
    /// </summary>
    public class PieceIconUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("方块图标UI组件")]
        /// <summary>方块图标图像</summary>
        [SerializeField] private Image _m_pieceIcon;
        
        /// <summary>选中状态边框</summary>
        [SerializeField] private Image _m_selectionBorder;
        
        /// <summary>方块大小文本</summary>
        [SerializeField] private Text _m_pieceSizeText;
        
        /// <summary>不可用遮罩</summary>
        [SerializeField] private Image _m_disabledMask;
        
        [Header("选中状态配置")]
        /// <summary>选中边框颜色</summary>
        [SerializeField] private Color _m_selectedBorderColor = Color.yellow;
        
        /// <summary>普通边框颜色</summary>
        [SerializeField] private Color _m_normalBorderColor = Color.white;
        
        /// <summary>不可用遮罩颜色</summary>
        [SerializeField] private Color _m_disabledMaskColor = new Color(0, 0, 0, 0.5f);
        
        // 私有字段
        /// <summary>关联的方块数据</summary>
        private _IGamePiece _m_gamePiece;
        
        /// <summary>是否被选中</summary>
        private bool _m_isSelected = false;
        
        /// <summary>是否可用</summary>
        private bool _m_isEnabled = true;
        
        // 事件
        /// <summary>方块点击事件</summary>
        public System.Action<_IGamePiece> onPieceClicked;
        
        #region 公共方法
        
        /// <summary>
        /// 初始化方块图标UI
        /// </summary>
        /// <param name="_gamePiece">方块数据</param>
        public void Initialize(_IGamePiece _gamePiece)
        {
            _m_gamePiece = _gamePiece;
            
            if (_gamePiece == null)
            {
                Debug.LogError("[PieceIconUI] 方块数据为空");
                return;
            }
            
            // 设置方块图标
            _updatePieceIcon();
            
            // 设置方块大小文本
            _updatePieceSizeText();
            
            // 设置初始状态
            SetSelected(false);
            SetEnabled(true);
        }
        
        /// <summary>
        /// 设置选中状态
        /// </summary>
        /// <param name="_selected">是否选中</param>
        public void SetSelected(bool _selected)
        {
            _m_isSelected = _selected;
            
            if (_m_selectionBorder != null)
            {
                _m_selectionBorder.color = _selected ? _m_selectedBorderColor : _m_normalBorderColor;
                _m_selectionBorder.gameObject.SetActive(_selected);
            }
        }
        
        /// <summary>
        /// 设置可用状态
        /// </summary>
        /// <param name="_enabled">是否可用</param>
        public void SetEnabled(bool _enabled)
        {
            _m_isEnabled = _enabled;
            
            if (_m_disabledMask != null)
            {
                _m_disabledMask.gameObject.SetActive(!_enabled);
                if (!_enabled)
                {
                    _m_disabledMask.color = _m_disabledMaskColor;
                }
            }
            
            // 调整整体透明度
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = _enabled ? 1f : 0.6f;
            canvasGroup.interactable = _enabled;
        }
        
        /// <summary>
        /// 获取关联的方块数据
        /// </summary>
        /// <returns>方块数据</returns>
        public _IGamePiece GetGamePiece()
        {
            return _m_gamePiece;
        }
        
        /// <summary>
        /// 获取是否选中状态
        /// </summary>
        /// <returns>是否选中</returns>
        public bool IsSelected()
        {
            return _m_isSelected;
        }
        
        /// <summary>
        /// 获取是否可用状态
        /// </summary>
        /// <returns>是否可用</returns>
        public bool IsEnabled()
        {
            return _m_isEnabled;
        }
        
        #endregion
        
        #region 事件处理
        
        /// <summary>
        /// 处理点击事件
        /// </summary>
        /// <param name="_eventData">事件数据</param>
        public void OnPointerClick(PointerEventData _eventData)
        {
            if (!_m_isEnabled || _m_gamePiece == null)
                return;
            
            // 触发点击事件
            onPieceClicked?.Invoke(_m_gamePiece);
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 更新方块图标
        /// </summary>
        private void _updatePieceIcon()
        {
            if (_m_pieceIcon == null || _m_gamePiece == null)
                return;
            
            // TODO: 根据方块形状生成或加载对应的图标
            // 这里需要根据方块的形状数据创建可视化表示
            // 暂时使用占位符颜色
            _m_pieceIcon.color = Color.white; // 可以根据玩家颜色设置
        }
        
        /// <summary>
        /// 更新方块大小文本
        /// </summary>
        private void _updatePieceSizeText()
        {
            if (_m_pieceSizeText == null || _m_gamePiece == null)
                return;
            
            // 显示方块包含的格子数量
            int blockCount = _m_gamePiece.currentShape?.Length ?? 0;
            _m_pieceSizeText.text = blockCount.ToString();
        }
        
        #endregion
    }
}