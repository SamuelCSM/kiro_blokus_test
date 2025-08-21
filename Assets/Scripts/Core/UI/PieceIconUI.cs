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
        
        /// <summary>方块形状可视化容器</summary>
        [SerializeField] private Transform _m_shapeContainer;
        
        /// <summary>方块格子预制体</summary>
        [SerializeField] private GameObject _m_blockPrefab;
        
        [Header("选中状态配置")]
        /// <summary>选中边框颜色</summary>
        [SerializeField] private Color _m_selectedBorderColor = Color.yellow;
        
        /// <summary>普通边框颜色</summary>
        [SerializeField] private Color _m_normalBorderColor = Color.white;
        
        /// <summary>不可用遮罩颜色</summary>
        [SerializeField] private Color _m_disabledMaskColor = new Color(0, 0, 0, 0.5f);
        
        [Header("方块可视化配置")]
        /// <summary>方块格子大小</summary>
        [SerializeField] private float _m_blockSize = 8f;
        
        /// <summary>方块间距</summary>
        [SerializeField] private float _m_blockSpacing = 1f;
        
        /// <summary>是否使用动态图标生成</summary>
        [SerializeField] private bool _m_useDynamicIcon = true;
        
        // 私有字段
        /// <summary>关联的方块数据</summary>
        private _IGamePiece _m_gamePiece;
        
        /// <summary>是否被选中</summary>
        private bool _m_isSelected = false;
        
        /// <summary>是否可用</summary>
        private bool _m_isEnabled = true;
        
        /// <summary>生成的方块格子列表</summary>
        private System.Collections.Generic.List<GameObject> _m_generatedBlocks = new System.Collections.Generic.List<GameObject>();
        
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
            
            // 更新方块格子的视觉效果
            _updateBlocksVisualState();
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
        
        /// <summary>
        /// 设置方块颜色
        /// </summary>
        /// <param name="_color">方块颜色</param>
        public void SetPieceColor(Color _color)
        {
            if (_m_gamePiece != null)
            {
                // 更新方块颜色并重新生成图标
                _updatePieceIcon();
                _updateBlocksColor(_color);
            }
        }
        
        /// <summary>
        /// 刷新方块显示
        /// </summary>
        public void RefreshDisplay()
        {
            if (_m_gamePiece != null)
            {
                _updatePieceIcon();
                _updatePieceSizeText();
            }
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
            if (_m_gamePiece == null)
                return;
            
            if (_m_useDynamicIcon && _m_shapeContainer != null)
            {
                // 使用动态生成的方块形状
                _generatePieceShape();
                
                // 隐藏静态图标
                if (_m_pieceIcon != null)
                {
                    _m_pieceIcon.gameObject.SetActive(false);
                }
            }
            else if (_m_pieceIcon != null)
            {
                // 使用静态图标
                _m_pieceIcon.gameObject.SetActive(true);
                _m_pieceIcon.color = _m_gamePiece.pieceColor;
                
                // 隐藏动态形状容器
                if (_m_shapeContainer != null)
                {
                    _m_shapeContainer.gameObject.SetActive(false);
                }
            }
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
        
        /// <summary>
        /// 生成方块形状的可视化表示
        /// </summary>
        private void _generatePieceShape()
        {
            if (_m_shapeContainer == null || _m_gamePiece == null)
                return;
            
            // 清除现有的方块格子
            _clearGeneratedBlocks();
            
            Vector2Int[] shape = _m_gamePiece.currentShape;
            if (shape == null || shape.Length == 0)
                return;
            
            // 计算形状的边界
            Vector2Int min = shape[0];
            Vector2Int max = shape[0];
            
            foreach (Vector2Int pos in shape)
            {
                if (pos.x < min.x) min.x = pos.x;
                if (pos.y < min.y) min.y = pos.y;
                if (pos.x > max.x) max.x = pos.x;
                if (pos.y > max.y) max.y = pos.y;
            }
            
            // 计算中心偏移
            Vector2 centerOffset = new Vector2(
                -(max.x + min.x) * (_m_blockSize + _m_blockSpacing) * 0.5f,
                -(max.y + min.y) * (_m_blockSize + _m_blockSpacing) * 0.5f
            );
            
            // 生成每个方块格子
            foreach (Vector2Int pos in shape)
            {
                _createBlock(pos, centerOffset);
            }
        }
        
        /// <summary>
        /// 创建单个方块格子
        /// </summary>
        /// <param name="_position">格子位置</param>
        /// <param name="_centerOffset">中心偏移</param>
        private void _createBlock(Vector2Int _position, Vector2 _centerOffset)
        {
            GameObject block;
            
            if (_m_blockPrefab != null)
            {
                // 使用预制体创建方块格子
                block = Instantiate(_m_blockPrefab, _m_shapeContainer);
            }
            else
            {
                // 创建简单的方块格子
                block = new GameObject("Block");
                block.transform.SetParent(_m_shapeContainer);
                
                var image = block.AddComponent<Image>();
                image.color = _m_gamePiece.pieceColor;
			}

            // 设置位置
            if (block != null)
            {
	            var rectTransform = block.GetComponent<RectTransform>();
	            if (rectTransform != null)
	            {
		            Vector2 localPos = new Vector2(
			            _position.x * (_m_blockSize + _m_blockSpacing),
			            _position.y * (_m_blockSize + _m_blockSpacing)
		            ) + _centerOffset;

		            rectTransform.anchoredPosition = localPos;
		            rectTransform.localScale = Vector3.one;
	            }

	            _m_generatedBlocks.Add(block);
			}
		}
        
        /// <summary>
        /// 清除生成的方块格子
        /// </summary>
        private void _clearGeneratedBlocks()
        {
            foreach (GameObject block in _m_generatedBlocks)
            {
                if (block != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(block);
                    }
                    else
                    {
                        DestroyImmediate(block);
                    }
                }
            }
            _m_generatedBlocks.Clear();
        }
        
        /// <summary>
        /// 更新方块格子的视觉状态
        /// </summary>
        private void _updateBlocksVisualState()
        {
            foreach (GameObject block in _m_generatedBlocks)
            {
                if (block == null) continue;
                
                var image = block.GetComponent<Image>();
                if (image != null)
                {
                    // 根据选中状态调整颜色
                    Color baseColor = _m_gamePiece?.pieceColor ?? Color.white;
                    if (_m_isSelected)
                    {
                        // 选中时稍微提亮
                        image.color = Color.Lerp(baseColor, Color.white, 0.3f);
                    }
                    else
                    {
                        image.color = baseColor;
                    }
                }
            }
        }
        
        /// <summary>
        /// 更新方块格子的颜色
        /// </summary>
        /// <param name="_color">新颜色</param>
        private void _updateBlocksColor(Color _color)
        {
            foreach (GameObject block in _m_generatedBlocks)
            {
                if (block == null) continue;
                
                var image = block.GetComponent<Image>();
                if (image != null)
                {
                    image.color = _color;
                }
            }
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 清理资源
        /// </summary>
        private void OnDestroy()
        {
            _clearGeneratedBlocks();
        }
        
        #endregion
    }
}