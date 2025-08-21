using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Data;

namespace BlokusGame.Core.UI
{
    /// <summary>
    /// 方块库存UI组件
    /// 管理和显示玩家的方块库存，提供详细的可视化和交互功能
    /// </summary>
    public class PieceInventoryUI : MonoBehaviour
    {
        [Header("库存UI组件")]
        /// <summary>方块图标容器</summary>
        [SerializeField] private Transform _m_pieceIconContainer;
        
        /// <summary>方块图标预制体</summary>
        [SerializeField] private GameObject _m_pieceIconPrefab;
        
        /// <summary>滚动视图组件</summary>
        [SerializeField] private ScrollRect _m_scrollRect;
        
        /// <summary>网格布局组件</summary>
        [SerializeField] private GridLayoutGroup _m_gridLayout;
        
        [Header("库存信息显示")]
        /// <summary>剩余方块数量文本</summary>
        [SerializeField] private Text _m_remainingCountText;
        
        /// <summary>剩余格子数量文本</summary>
        [SerializeField] private Text _m_remainingBlocksText;
        
        /// <summary>库存完成度进度条</summary>
        [SerializeField] private Slider _m_completionProgressBar;
        
        /// <summary>库存完成度文本</summary>
        [SerializeField] private Text _m_completionPercentText;
        
        [Header("筛选和排序")]
        /// <summary>按大小排序按钮</summary>
        [SerializeField] private Button _m_sortBySizeButton;
        
        /// <summary>按ID排序按钮</summary>
        [SerializeField] private Button _m_sortByIdButton;
        
        /// <summary>只显示可用方块切换</summary>
        [SerializeField] private Toggle _m_showAvailableOnlyToggle;
        
        /// <summary>大小筛选下拉菜单</summary>
        [SerializeField] private Dropdown _m_sizeFilterDropdown;
        
        [Header("选中方块详情")]
        /// <summary>选中方块详情面板</summary>
        [SerializeField] private GameObject _m_selectedPiecePanel;
        
        /// <summary>选中方块预览</summary>
        [SerializeField] private Transform _m_selectedPiecePreview;
        
        /// <summary>选中方块信息文本</summary>
        [SerializeField] private Text _m_selectedPieceInfoText;
        
        /// <summary>方块操作按钮容器</summary>
        [SerializeField] private Transform _m_pieceActionButtons;
        
        [Header("可视化配置")]
        /// <summary>方块图标大小</summary>
        [SerializeField] private Vector2 _m_iconSize = new Vector2(60f, 60f);
        
        /// <summary>图标间距</summary>
        [SerializeField] private Vector2 _m_iconSpacing = new Vector2(5f, 5f);
        
        /// <summary>每行图标数量</summary>
        [SerializeField] private int _m_iconsPerRow = 4;
        
        /// <summary>是否启用动画</summary>
        [SerializeField] private bool _m_enableAnimations = true;
        
        // 私有字段
        /// <summary>当前玩家ID</summary>
        private int _m_currentPlayerId = -1;
        
        /// <summary>所有方块数据</summary>
        private List<_IGamePiece> _m_allPieces = new List<_IGamePiece>();
        
        /// <summary>当前显示的方块</summary>
        private List<_IGamePiece> _m_displayedPieces = new List<_IGamePiece>();
        
        /// <summary>方块图标UI映射</summary>
        private Dictionary<_IGamePiece, PieceIconUI> _m_pieceIconMap = new Dictionary<_IGamePiece, PieceIconUI>();
        
        /// <summary>当前选中的方块</summary>
        private _IGamePiece _m_selectedPiece;
        
        /// <summary>当前排序方式</summary>
        private SortMode _m_currentSortMode = SortMode.ById;
        
        /// <summary>当前大小筛选</summary>
        private int _m_currentSizeFilter = 0; // 0表示显示所有大小
        
        // 事件
        /// <summary>方块选择事件</summary>
        public System.Action<_IGamePiece> onPieceSelected;
        
        /// <summary>方块双击事件</summary>
        public System.Action<_IGamePiece> onPieceDoubleClicked;
        
        /// <summary>库存更新事件</summary>
        public System.Action<int, int> onInventoryUpdated; // 剩余方块数, 剩余格子数
        
        /// <summary>排序方式枚举</summary>
        public enum SortMode
        {
            ById,       // 按ID排序
            BySize,     // 按大小排序
            ByAvailable // 按可用性排序
        }
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法 - 初始化组件
        /// </summary>
        private void Awake()
        {
            _initializeUI();
            _bindEvents();
        }
        
        /// <summary>
        /// Unity Start方法 - 设置初始状态
        /// </summary>
        private void Start()
        {
            _setupGridLayout();
            _initializeFilters();
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 清理资源
        /// </summary>
        private void OnDestroy()
        {
            _unbindEvents();
            _clearPieceIcons();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 初始化方块库存UI
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_pieces">方块列表</param>
        public void Initialize(int _playerId, List<_IGamePiece> _pieces)
        {
            _m_currentPlayerId = _playerId;
            _m_allPieces = new List<_IGamePiece>(_pieces);
            
            _refreshInventoryDisplay();
            _updateInventoryStats();
            
            Debug.Log($"[PieceInventoryUI] 为玩家{_playerId}初始化库存，方块数量: {_pieces.Count}");
        }
        
        /// <summary>
        /// 更新方块库存
        /// </summary>
        /// <param name="_pieces">新的方块列表</param>
        public void UpdateInventory(List<_IGamePiece> _pieces)
        {
            _m_allPieces = new List<_IGamePiece>(_pieces);
            
            _refreshInventoryDisplay();
            _updateInventoryStats();
            
            // 触发库存更新事件
            int remainingPieces = _m_allPieces.Count(p => !p.isPlaced);
            int remainingBlocks = _m_allPieces.Where(p => !p.isPlaced).Sum(p => p.currentShape?.Length ?? 0);
            onInventoryUpdated?.Invoke(remainingPieces, remainingBlocks);
        }
        
        /// <summary>
        /// 设置选中的方块
        /// </summary>
        /// <param name="_piece">要选中的方块</param>
        public void SetSelectedPiece(_IGamePiece _piece)
        {
            // 取消之前的选中状态
            if (_m_selectedPiece != null && _m_pieceIconMap.ContainsKey(_m_selectedPiece))
            {
                _m_pieceIconMap[_m_selectedPiece].SetSelected(false);
            }
            
            _m_selectedPiece = _piece;
            
            // 设置新的选中状态
            if (_m_selectedPiece != null && _m_pieceIconMap.ContainsKey(_m_selectedPiece))
            {
                _m_pieceIconMap[_m_selectedPiece].SetSelected(true);
                _updateSelectedPieceDetails();
            }
            
            // 显示或隐藏详情面板
            if (_m_selectedPiecePanel != null)
            {
                _m_selectedPiecePanel.SetActive(_m_selectedPiece != null);
            }
            
            // 触发选择事件
            onPieceSelected?.Invoke(_m_selectedPiece);
        }
        
        /// <summary>
        /// 获取当前选中的方块
        /// </summary>
        /// <returns>选中的方块</returns>
        public _IGamePiece GetSelectedPiece()
        {
            return _m_selectedPiece;
        }
        
        /// <summary>
        /// 设置方块的可用状态
        /// </summary>
        /// <param name="_piece">方块</param>
        /// <param name="_available">是否可用</param>
        public void SetPieceAvailable(_IGamePiece _piece, bool _available)
        {
            if (_m_pieceIconMap.ContainsKey(_piece))
            {
                _m_pieceIconMap[_piece].SetEnabled(_available);
            }
        }
        
        /// <summary>
        /// 滚动到指定方块
        /// </summary>
        /// <param name="_piece">目标方块</param>
        public void ScrollToPiece(_IGamePiece _piece)
        {
            if (!_m_pieceIconMap.ContainsKey(_piece) || _m_scrollRect == null)
                return;
            
            var pieceIcon = _m_pieceIconMap[_piece];
            var pieceRect = pieceIcon.GetComponent<RectTransform>();
            
            if (pieceRect != null)
            {
                StartCoroutine(_scrollToPieceCoroutine(pieceRect));
            }
        }
        
        #endregion
        
        #region 私有方法 - 初始化
        
        /// <summary>
        /// 初始化UI组件
        /// </summary>
        private void _initializeUI()
        {
            // 隐藏选中方块详情面板
            if (_m_selectedPiecePanel != null)
            {
                _m_selectedPiecePanel.SetActive(false);
            }
            
            // 初始化进度条
            if (_m_completionProgressBar != null)
            {
                _m_completionProgressBar.value = 0f;
            }
        }
        
        /// <summary>
        /// 绑定事件
        /// </summary>
        private void _bindEvents()
        {
            if (_m_sortBySizeButton != null)
                _m_sortBySizeButton.onClick.AddListener(() => _setSortMode(SortMode.BySize));
            
            if (_m_sortByIdButton != null)
                _m_sortByIdButton.onClick.AddListener(() => _setSortMode(SortMode.ById));
            
            if (_m_showAvailableOnlyToggle != null)
                _m_showAvailableOnlyToggle.onValueChanged.AddListener(_onShowAvailableOnlyChanged);
            
            if (_m_sizeFilterDropdown != null)
                _m_sizeFilterDropdown.onValueChanged.AddListener(_onSizeFilterChanged);
        }
        
        /// <summary>
        /// 取消事件绑定
        /// </summary>
        private void _unbindEvents()
        {
            if (_m_sortBySizeButton != null)
                _m_sortBySizeButton.onClick.RemoveAllListeners();
            
            if (_m_sortByIdButton != null)
                _m_sortByIdButton.onClick.RemoveAllListeners();
            
            if (_m_showAvailableOnlyToggle != null)
                _m_showAvailableOnlyToggle.onValueChanged.RemoveAllListeners();
            
            if (_m_sizeFilterDropdown != null)
                _m_sizeFilterDropdown.onValueChanged.RemoveAllListeners();
        }
        
        /// <summary>
        /// 设置网格布局
        /// </summary>
        private void _setupGridLayout()
        {
            if (_m_gridLayout == null) return;
            
            _m_gridLayout.cellSize = _m_iconSize;
            _m_gridLayout.spacing = _m_iconSpacing;
            _m_gridLayout.constraintCount = _m_iconsPerRow;
        }
        
        /// <summary>
        /// 初始化筛选器
        /// </summary>
        private void _initializeFilters()
        {
            if (_m_sizeFilterDropdown == null) return;
            
            // 设置大小筛选选项
            _m_sizeFilterDropdown.ClearOptions();
            var options = new List<string>
            {
                "所有大小",
                "1格方块",
                "2格方块", 
                "3格方块",
                "4格方块",
                "5格方块"
            };
            _m_sizeFilterDropdown.AddOptions(options);
        }
        
        #endregion
        
        #region 私有方法 - 显示更新
        
        /// <summary>
        /// 刷新库存显示
        /// </summary>
        private void _refreshInventoryDisplay()
        {
            // 应用筛选和排序
            _m_displayedPieces = _getFilteredAndSortedPieces();
            
            // 清除现有图标
            _clearPieceIcons();
            
            // 创建新图标
            foreach (var piece in _m_displayedPieces)
            {
                _createPieceIcon(piece);
            }
            
            // 更新布局
            if (_m_enableAnimations)
            {
                StartCoroutine(_animateLayoutUpdate());
            }
        }
        
        /// <summary>
        /// 获取筛选和排序后的方块列表
        /// </summary>
        /// <returns>筛选排序后的方块列表</returns>
        private List<_IGamePiece> _getFilteredAndSortedPieces()
        {
            var filteredPieces = _m_allPieces.AsEnumerable();
            
            // 应用可用性筛选
            if (_m_showAvailableOnlyToggle != null && _m_showAvailableOnlyToggle.isOn)
            {
                filteredPieces = filteredPieces.Where(p => !p.isPlaced);
            }
            
            // 应用大小筛选
            if (_m_currentSizeFilter > 0)
            {
                filteredPieces = filteredPieces.Where(p => (p.currentShape?.Length ?? 0) == _m_currentSizeFilter);
            }
            
            // 应用排序
            switch (_m_currentSortMode)
            {
                case SortMode.ById:
                    filteredPieces = filteredPieces.OrderBy(p => p.pieceId);
                    break;
                case SortMode.BySize:
                    filteredPieces = filteredPieces.OrderBy(p => p.currentShape?.Length ?? 0).ThenBy(p => p.pieceId);
                    break;
                case SortMode.ByAvailable:
                    filteredPieces = filteredPieces.OrderBy(p => p.isPlaced).ThenBy(p => p.pieceId);
                    break;
            }
            
            return filteredPieces.ToList();
        }
        
        /// <summary>
        /// 创建方块图标
        /// </summary>
        /// <param name="_piece">方块数据</param>
        private void _createPieceIcon(_IGamePiece _piece)
        {
            if (_m_pieceIconContainer == null || _m_pieceIconPrefab == null)
                return;
            
            var iconObj = Instantiate(_m_pieceIconPrefab, _m_pieceIconContainer);
            var pieceIconUI = iconObj.GetComponent<PieceIconUI>();
            
            if (pieceIconUI != null)
            {
                pieceIconUI.Initialize(_piece);
                pieceIconUI.SetEnabled(!_piece.isPlaced);
                pieceIconUI.onPieceClicked += _onPieceIconClicked;
                
                _m_pieceIconMap[_piece] = pieceIconUI;
            }
        }
        
        /// <summary>
        /// 清除方块图标
        /// </summary>
        private void _clearPieceIcons()
        {
            foreach (var kvp in _m_pieceIconMap)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.onPieceClicked -= _onPieceIconClicked;
                    
                    if (Application.isPlaying)
                    {
                        Destroy(kvp.Value.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(kvp.Value.gameObject);
                    }
                }
            }
            _m_pieceIconMap.Clear();
        }
        
        /// <summary>
        /// 更新库存统计信息
        /// </summary>
        private void _updateInventoryStats()
        {
            int totalPieces = _m_allPieces.Count;
            int remainingPieces = _m_allPieces.Count(p => !p.isPlaced);
            int totalBlocks = _m_allPieces.Sum(p => p.currentShape?.Length ?? 0);
            int remainingBlocks = _m_allPieces.Where(p => !p.isPlaced).Sum(p => p.currentShape?.Length ?? 0);
            
            // 更新文本显示
            if (_m_remainingCountText != null)
            {
                _m_remainingCountText.text = $"剩余方块: {remainingPieces}/{totalPieces}";
            }
            
            if (_m_remainingBlocksText != null)
            {
                _m_remainingBlocksText.text = $"剩余格子: {remainingBlocks}/{totalBlocks}";
            }
            
            // 更新进度条
            if (_m_completionProgressBar != null)
            {
                float completion = totalPieces > 0 ? (float)(totalPieces - remainingPieces) / totalPieces : 0f;
                _m_completionProgressBar.value = completion;
            }
            
            // 更新完成度文本
            if (_m_completionPercentText != null)
            {
                float completionPercent = totalPieces > 0 ? (float)(totalPieces - remainingPieces) / totalPieces * 100f : 0f;
                _m_completionPercentText.text = $"{completionPercent:F0}%";
            }
        }
        
        /// <summary>
        /// 更新选中方块详情
        /// </summary>
        private void _updateSelectedPieceDetails()
        {
            if (_m_selectedPiece == null) return;
            
            // 更新信息文本
            if (_m_selectedPieceInfoText != null)
            {
                string info = $"方块 #{_m_selectedPiece.pieceId}\n";
                info += $"大小: {_m_selectedPiece.currentShape?.Length ?? 0} 格\n";
                info += $"状态: {(_m_selectedPiece.isPlaced ? "已放置" : "可用")}";
                
                _m_selectedPieceInfoText.text = info;
            }
            
            // 更新预览显示
            _updateSelectedPiecePreview();
        }
        
        /// <summary>
        /// 更新选中方块预览
        /// </summary>
        private void _updateSelectedPiecePreview()
        {
            if (_m_selectedPiecePreview == null || _m_selectedPiece == null)
                return;
            
            // 清除现有预览
            foreach (Transform child in _m_selectedPiecePreview)
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
            
            // 创建新预览
            _createPiecePreview(_m_selectedPiece, _m_selectedPiecePreview);
        }
        
        /// <summary>
        /// 创建方块预览
        /// </summary>
        /// <param name="_piece">方块数据</param>
        /// <param name="_container">容器</param>
        private void _createPiecePreview(_IGamePiece _piece, Transform _container)
        {
            if (_piece.currentShape == null) return;
            
            float blockSize = 20f;
            float spacing = 2f;
            
            // 计算边界
            Vector2Int min = _piece.currentShape[0];
            Vector2Int max = _piece.currentShape[0];
            
            foreach (Vector2Int pos in _piece.currentShape)
            {
                if (pos.x < min.x) min.x = pos.x;
                if (pos.y < min.y) min.y = pos.y;
                if (pos.x > max.x) max.x = pos.x;
                if (pos.y > max.y) max.y = pos.y;
            }
            
            // 计算中心偏移
            Vector2 centerOffset = new Vector2(
                -(max.x + min.x) * (blockSize + spacing) * 0.5f,
                -(max.y + min.y) * (blockSize + spacing) * 0.5f
            );
            
            // 创建每个格子
            foreach (Vector2Int pos in _piece.currentShape)
            {
                var blockObj = new GameObject("PreviewBlock");
                blockObj.transform.SetParent(_container);
                
                var image = blockObj.AddComponent<Image>();
                image.color = _piece.pieceColor;
                
                var rectTransform = blockObj.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(blockSize, blockSize);
                
                Vector2 localPos = new Vector2(
                    pos.x * (blockSize + spacing),
                    pos.y * (blockSize + spacing)
                ) + centerOffset;
                
                rectTransform.anchoredPosition = localPos;
                rectTransform.localScale = Vector3.one;
            }
        }
        
        #endregion
        
        #region 私有方法 - 事件处理
        
        /// <summary>
        /// 处理方块图标点击事件
        /// </summary>
        /// <param name="_piece">点击的方块</param>
        private void _onPieceIconClicked(_IGamePiece _piece)
        {
            SetSelectedPiece(_piece);
        }
        
        /// <summary>
        /// 处理排序方式变更
        /// </summary>
        /// <param name="_sortMode">新的排序方式</param>
        private void _setSortMode(SortMode _sortMode)
        {
            if (_m_currentSortMode == _sortMode) return;
            
            _m_currentSortMode = _sortMode;
            _refreshInventoryDisplay();
            
            // 更新按钮状态
            _updateSortButtonStates();
        }
        
        /// <summary>
        /// 处理"只显示可用"切换
        /// </summary>
        /// <param name="_showAvailableOnly">是否只显示可用方块</param>
        private void _onShowAvailableOnlyChanged(bool _showAvailableOnly)
        {
            _refreshInventoryDisplay();
        }
        
        /// <summary>
        /// 处理大小筛选变更
        /// </summary>
        /// <param name="_filterIndex">筛选索引</param>
        private void _onSizeFilterChanged(int _filterIndex)
        {
            _m_currentSizeFilter = _filterIndex; // 0表示所有大小，1-5表示对应格子数
            _refreshInventoryDisplay();
        }
        
        /// <summary>
        /// 更新排序按钮状态
        /// </summary>
        private void _updateSortButtonStates()
        {
            if (_m_sortBySizeButton != null)
            {
                var colors = _m_sortBySizeButton.colors;
                colors.normalColor = _m_currentSortMode == SortMode.BySize ? Color.yellow : Color.white;
                _m_sortBySizeButton.colors = colors;
            }
            
            if (_m_sortByIdButton != null)
            {
                var colors = _m_sortByIdButton.colors;
                colors.normalColor = _m_currentSortMode == SortMode.ById ? Color.yellow : Color.white;
                _m_sortByIdButton.colors = colors;
            }
        }
        
        #endregion
        
        #region 私有方法 - 动画
        
        /// <summary>
        /// 布局更新动画协程
        /// </summary>
        /// <returns>协程枚举器</returns>
        private System.Collections.IEnumerator _animateLayoutUpdate()
        {
            // 简单的淡入动画
            var canvasGroup = _m_pieceIconContainer.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = _m_pieceIconContainer.gameObject.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = 0f;
            
            float duration = 0.3f;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                canvasGroup.alpha = elapsedTime / duration;
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
        
        /// <summary>
        /// 滚动到方块协程
        /// </summary>
        /// <param name="_targetRect">目标方块的RectTransform</param>
        /// <returns>协程枚举器</returns>
        private System.Collections.IEnumerator _scrollToPieceCoroutine(RectTransform _targetRect)
        {
            if (_m_scrollRect == null) yield break;
            
            // 计算目标位置
            var contentRect = _m_scrollRect.content;
            var viewportRect = _m_scrollRect.viewport;
            
            Vector2 targetPos = (Vector2)_m_scrollRect.transform.InverseTransformPoint(contentRect.position) 
                              - (Vector2)_m_scrollRect.transform.InverseTransformPoint(_targetRect.position);
            
            // 平滑滚动
            float duration = 0.5f;
            float elapsedTime = 0f;
            Vector2 startPos = contentRect.anchoredPosition;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                
                contentRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, progress);
                
                yield return null;
            }
            
            contentRect.anchoredPosition = targetPos;
        }
        
        #endregion
    }
}