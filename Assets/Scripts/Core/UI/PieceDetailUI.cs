using UnityEngine;
using UnityEngine.UI;
using BlokusGame.Core.Interfaces;

namespace BlokusGame.Core.UI
{
    /// <summary>
    /// 方块详情UI组件
    /// 显示选中方块的详细信息和操作选项
    /// </summary>
    public class PieceDetailUI : MonoBehaviour
    {
        [Header("方块详情显示")]
        /// <summary>方块名称文本</summary>
        [SerializeField] private Text _m_pieceNameText;
        
        /// <summary>方块ID文本</summary>
        [SerializeField] private Text _m_pieceIdText;
        
        /// <summary>方块大小文本</summary>
        [SerializeField] private Text _m_pieceSizeText;
        
        /// <summary>方块状态文本</summary>
        [SerializeField] private Text _m_pieceStatusText;
        
        /// <summary>方块形状预览容器</summary>
        [SerializeField] private Transform _m_shapePreviewContainer;
        
        /// <summary>方块颜色指示器</summary>
        [SerializeField] private Image _m_pieceColorIndicator;
        
        [Header("方块操作按钮")]
        /// <summary>选择方块按钮</summary>
        [SerializeField] private Button _m_selectPieceButton;
        
        /// <summary>旋转方块按钮</summary>
        [SerializeField] private Button _m_rotatePieceButton;
        
        /// <summary>翻转方块按钮</summary>
        [SerializeField] private Button _m_flipPieceButton;
        
        /// <summary>重置方块按钮</summary>
        [SerializeField] private Button _m_resetPieceButton;
        
        /// <summary>关闭详情按钮</summary>
        [SerializeField] private Button _m_closeButton;
        
        [Header("形状预览配置")]
        /// <summary>预览格子大小</summary>
        [SerializeField] private float _m_previewBlockSize = 25f;
        
        /// <summary>预览格子间距</summary>
        [SerializeField] private float _m_previewBlockSpacing = 2f;
        
        /// <summary>预览格子预制体</summary>
        [SerializeField] private GameObject _m_previewBlockPrefab;
        
        [Header("动画配置")]
        /// <summary>是否启用动画</summary>
        [SerializeField] private bool _m_enableAnimations = true;
        
        /// <summary>动画持续时间</summary>
        [SerializeField] private float _m_animationDuration = 0.3f;
        
        // 私有字段
        /// <summary>当前显示的方块</summary>
        private _IGamePiece _m_currentPiece;
        
        /// <summary>生成的预览格子列表</summary>
        private System.Collections.Generic.List<GameObject> _m_previewBlocks = new System.Collections.Generic.List<GameObject>();
        
        // 事件
        /// <summary>方块选择事件</summary>
        public System.Action<_IGamePiece> onPieceSelected;
        
        /// <summary>方块操作事件</summary>
        public System.Action<_IGamePiece, PieceOperation> onPieceOperation;
        
        /// <summary>关闭详情事件</summary>
        public System.Action onDetailClosed;
        
        /// <summary>方块操作类型枚举</summary>
        public enum PieceOperation
        {
            Rotate,     // 旋转
            Flip,       // 翻转
            Reset       // 重置
        }
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法 - 初始化组件
        /// </summary>
        private void Awake()
        {
            _bindEvents();
            _initializeUI();
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 清理资源
        /// </summary>
        private void OnDestroy()
        {
            _unbindEvents();
            _clearPreviewBlocks();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 显示方块详情
        /// </summary>
        /// <param name="_piece">要显示的方块</param>
        public void ShowPieceDetail(_IGamePiece _piece)
        {
            _m_currentPiece = _piece;
            
            if (_piece == null)
            {
                Hide();
                return;
            }
            
            _updatePieceInfo();
            _updateShapePreview();
            _updateOperationButtons();
            
            // 显示面板
            gameObject.SetActive(true);
            
            // 播放入场动画
            if (_m_enableAnimations)
            {
                StartCoroutine(_playShowAnimation());
            }
        }
        
        /// <summary>
        /// 隐藏方块详情
        /// </summary>
        public void Hide()
        {
            if (_m_enableAnimations && gameObject.activeInHierarchy)
            {
                StartCoroutine(_playHideAnimation());
            }
            else
            {
                gameObject.SetActive(false);
            }
            
            _m_currentPiece = null;
        }
        
        /// <summary>
        /// 刷新显示内容
        /// </summary>
        public void RefreshDisplay()
        {
            if (_m_currentPiece != null)
            {
                _updatePieceInfo();
                _updateShapePreview();
                _updateOperationButtons();
            }
        }
        
        /// <summary>
        /// 获取当前显示的方块
        /// </summary>
        /// <returns>当前方块</returns>
        public _IGamePiece GetCurrentPiece()
        {
            return _m_currentPiece;
        }
        
        #endregion
        
        #region 私有方法 - 初始化
        
        /// <summary>
        /// 初始化UI
        /// </summary>
        private void _initializeUI()
        {
            // 初始状态隐藏面板
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 绑定事件
        /// </summary>
        private void _bindEvents()
        {
            if (_m_selectPieceButton != null)
                _m_selectPieceButton.onClick.AddListener(_onSelectPieceClicked);
            
            if (_m_rotatePieceButton != null)
                _m_rotatePieceButton.onClick.AddListener(_onRotatePieceClicked);
            
            if (_m_flipPieceButton != null)
                _m_flipPieceButton.onClick.AddListener(_onFlipPieceClicked);
            
            if (_m_resetPieceButton != null)
                _m_resetPieceButton.onClick.AddListener(_onResetPieceClicked);
            
            if (_m_closeButton != null)
                _m_closeButton.onClick.AddListener(_onCloseClicked);
        }
        
        /// <summary>
        /// 取消事件绑定
        /// </summary>
        private void _unbindEvents()
        {
            if (_m_selectPieceButton != null)
                _m_selectPieceButton.onClick.RemoveAllListeners();
            
            if (_m_rotatePieceButton != null)
                _m_rotatePieceButton.onClick.RemoveAllListeners();
            
            if (_m_flipPieceButton != null)
                _m_flipPieceButton.onClick.RemoveAllListeners();
            
            if (_m_resetPieceButton != null)
                _m_resetPieceButton.onClick.RemoveAllListeners();
            
            if (_m_closeButton != null)
                _m_closeButton.onClick.RemoveAllListeners();
        }
        
        #endregion
        
        #region 私有方法 - 显示更新
        
        /// <summary>
        /// 更新方块信息显示
        /// </summary>
        private void _updatePieceInfo()
        {
            if (_m_currentPiece == null) return;
            
            // 更新方块名称
            if (_m_pieceNameText != null)
            {
                _m_pieceNameText.text = $"方块 #{_m_currentPiece.pieceId}";
            }
            
            // 更新方块ID
            if (_m_pieceIdText != null)
            {
                _m_pieceIdText.text = $"ID: {_m_currentPiece.pieceId}";
            }
            
            // 更新方块大小
            if (_m_pieceSizeText != null)
            {
                int size = _m_currentPiece.currentShape?.Length ?? 0;
                _m_pieceSizeText.text = $"大小: {size} 格";
            }
            
            // 更新方块状态
            if (_m_pieceStatusText != null)
            {
                string status = _m_currentPiece.isPlaced ? "已放置" : "可用";
                _m_pieceStatusText.text = $"状态: {status}";
                
                // 根据状态设置颜色
                _m_pieceStatusText.color = _m_currentPiece.isPlaced ? Color.red : Color.green;
            }
            
            // 更新颜色指示器
            if (_m_pieceColorIndicator != null)
            {
                _m_pieceColorIndicator.color = _m_currentPiece.pieceColor;
            }
        }
        
        /// <summary>
        /// 更新形状预览
        /// </summary>
        private void _updateShapePreview()
        {
            if (_m_shapePreviewContainer == null || _m_currentPiece == null)
                return;
            
            // 清除现有预览
            _clearPreviewBlocks();
            
            // 生成新预览
            _generateShapePreview();
        }
        
        /// <summary>
        /// 生成形状预览
        /// </summary>
        private void _generateShapePreview()
        {
            Vector2Int[] shape = _m_currentPiece.currentShape;
            if (shape == null || shape.Length == 0) return;
            
            // 计算边界
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
                -(max.x + min.x) * (_m_previewBlockSize + _m_previewBlockSpacing) * 0.5f,
                -(max.y + min.y) * (_m_previewBlockSize + _m_previewBlockSpacing) * 0.5f
            );
            
            // 创建每个格子
            foreach (Vector2Int pos in shape)
            {
                _createPreviewBlock(pos, centerOffset);
            }
        }
        
        /// <summary>
        /// 创建预览格子
        /// </summary>
        /// <param name="_position">格子位置</param>
        /// <param name="_centerOffset">中心偏移</param>
        private void _createPreviewBlock(Vector2Int _position, Vector2 _centerOffset)
        {
            GameObject block;
            
            if (_m_previewBlockPrefab != null)
            {
                // 使用预制体
                block = Instantiate(_m_previewBlockPrefab, _m_shapePreviewContainer);
            }
            else
            {
                // 创建简单格子
                block = new GameObject("PreviewBlock");
                block.transform.SetParent(_m_shapePreviewContainer);
                
                var image = block.AddComponent<Image>();
                image.color = _m_currentPiece.pieceColor;
            }
            
            // 设置位置
            var rectTransform = block.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 localPos = new Vector2(
                    _position.x * (_m_previewBlockSize + _m_previewBlockSpacing),
                    _position.y * (_m_previewBlockSize + _m_previewBlockSpacing)
                ) + _centerOffset;
                
                rectTransform.anchoredPosition = localPos;
                rectTransform.localScale = Vector3.one;
            }
            
            _m_previewBlocks.Add(block);
        }
        
        /// <summary>
        /// 清除预览格子
        /// </summary>
        private void _clearPreviewBlocks()
        {
            foreach (GameObject block in _m_previewBlocks)
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
            _m_previewBlocks.Clear();
        }
        
        /// <summary>
        /// 更新操作按钮状态
        /// </summary>
        private void _updateOperationButtons()
        {
            bool canOperate = _m_currentPiece != null && !_m_currentPiece.isPlaced;
            
            if (_m_selectPieceButton != null)
                _m_selectPieceButton.interactable = _m_currentPiece != null;
            
            if (_m_rotatePieceButton != null)
                _m_rotatePieceButton.interactable = canOperate;
            
            if (_m_flipPieceButton != null)
                _m_flipPieceButton.interactable = canOperate;
            
            if (_m_resetPieceButton != null)
                _m_resetPieceButton.interactable = canOperate;
        }
        
        #endregion
        
        #region 私有方法 - 事件处理
        
        /// <summary>
        /// 处理选择方块按钮点击
        /// </summary>
        private void _onSelectPieceClicked()
        {
            if (_m_currentPiece != null)
            {
                onPieceSelected?.Invoke(_m_currentPiece);
            }
        }
        
        /// <summary>
        /// 处理旋转方块按钮点击
        /// </summary>
        private void _onRotatePieceClicked()
        {
            if (_m_currentPiece != null)
            {
                onPieceOperation?.Invoke(_m_currentPiece, PieceOperation.Rotate);
                
                // 刷新显示
                RefreshDisplay();
            }
        }
        
        /// <summary>
        /// 处理翻转方块按钮点击
        /// </summary>
        private void _onFlipPieceClicked()
        {
            if (_m_currentPiece != null)
            {
                onPieceOperation?.Invoke(_m_currentPiece, PieceOperation.Flip);
                
                // 刷新显示
                RefreshDisplay();
            }
        }
        
        /// <summary>
        /// 处理重置方块按钮点击
        /// </summary>
        private void _onResetPieceClicked()
        {
            if (_m_currentPiece != null)
            {
                onPieceOperation?.Invoke(_m_currentPiece, PieceOperation.Reset);
                
                // 刷新显示
                RefreshDisplay();
            }
        }
        
        /// <summary>
        /// 处理关闭按钮点击
        /// </summary>
        private void _onCloseClicked()
        {
            Hide();
            onDetailClosed?.Invoke();
        }
        
        #endregion
        
        #region 私有方法 - 动画
        
        /// <summary>
        /// 播放显示动画
        /// </summary>
        /// <returns>协程枚举器</returns>
        private System.Collections.IEnumerator _playShowAnimation()
        {
            // 获取或添加CanvasGroup
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
        /// 播放隐藏动画
        /// </summary>
        /// <returns>协程枚举器</returns>
        private System.Collections.IEnumerator _playHideAnimation()
        {
            // 获取CanvasGroup
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // 执行动画
            float elapsedTime = 0f;
            Vector3 startScale = transform.localScale;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsedTime < _m_animationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / _m_animationDuration;
                
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
                
                yield return null;
            }
            
            // 隐藏面板
            gameObject.SetActive(false);
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