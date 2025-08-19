using UnityEngine;
using BlokusGame.Core.Data;

namespace BlokusGame.Core.Pieces
{
    /// <summary>
    /// 方块配置组件 - 用于在预制体中配置方块的默认属性
    /// 提供Inspector界面来设置方块的视觉和交互参数
    /// 在运行时被PieceManager用来初始化方块实例
    /// </summary>
    public class PieceConfiguration : MonoBehaviour
    {
        [Header("方块数据")]
        /// <summary>方块数据引用，定义方块的形状和基本属性</summary>
        [SerializeField] private PieceData _m_pieceData;
        
        [Header("视觉设置")]
        /// <summary>方块格子的大小</summary>
        [SerializeField] private float _m_cellSize = 1.0f;
        
        /// <summary>方块的高度</summary>
        [SerializeField] private float _m_pieceHeight = 0.2f;
        
        /// <summary>方块边缘的倒角大小</summary>
        [SerializeField] private float _m_bevelSize = 0.05f;
        
        [Header("材质配置")]
        /// <summary>方块的基础材质</summary>
        [SerializeField] private Material _m_baseMaterial;
        
        /// <summary>方块高亮时的材质</summary>
        [SerializeField] private Material _m_highlightMaterial;
        
        /// <summary>方块预览时的材质</summary>
        [SerializeField] private Material _m_previewMaterial;
        
        /// <summary>方块无效放置时的材质</summary>
        [SerializeField] private Material _m_invalidMaterial;
        
        [Header("交互设置")]
        /// <summary>是否启用拖拽功能</summary>
        [SerializeField] private bool _m_enableDrag = true;
        
        /// <summary>是否启用旋转功能</summary>
        [SerializeField] private bool _m_enableRotation = true;
        
        /// <summary>双击旋转的时间间隔</summary>
        [SerializeField] private float _m_doubleClickTime = 0.3f;
        
        /// <summary>拖拽的最小距离阈值</summary>
        [SerializeField] private float _m_dragThreshold = 10f;
        
        /// <summary>悬停时是否高亮</summary>
        [SerializeField] private bool _m_highlightOnHover = true;
        
        /// <summary>拖拽时的高度偏移</summary>
        [SerializeField] private float _m_dragHeightOffset = 1.0f;
        
        /// <summary>拖拽时的缩放倍数</summary>
        [SerializeField] private float _m_dragScaleMultiplier = 1.2f;
        
        [Header("动画设置")]
        /// <summary>方块放置动画的持续时间</summary>
        [SerializeField] private float _m_placementAnimationDuration = 0.5f;
        
        /// <summary>方块旋转动画的持续时间</summary>
        [SerializeField] private float _m_rotationAnimationDuration = 0.3f;
        
        /// <summary>方块缩放动画的持续时间</summary>
        [SerializeField] private float _m_scaleAnimationDuration = 0.2f;
        
        // 公共属性访问器
        /// <summary>方块数据</summary>
        public PieceData pieceData => _m_pieceData;
        
        /// <summary>格子大小</summary>
        public float cellSize => _m_cellSize;
        
        /// <summary>方块高度</summary>
        public float pieceHeight => _m_pieceHeight;
        
        /// <summary>倒角大小</summary>
        public float bevelSize => _m_bevelSize;
        
        /// <summary>基础材质</summary>
        public Material baseMaterial => _m_baseMaterial;
        
        /// <summary>高亮材质</summary>
        public Material highlightMaterial => _m_highlightMaterial;
        
        /// <summary>预览材质</summary>
        public Material previewMaterial => _m_previewMaterial;
        
        /// <summary>无效材质</summary>
        public Material invalidMaterial => _m_invalidMaterial;
        
        /// <summary>启用拖拽</summary>
        public bool enableDrag => _m_enableDrag;
        
        /// <summary>启用旋转</summary>
        public bool enableRotation => _m_enableRotation;
        
        /// <summary>双击时间</summary>
        public float doubleClickTime => _m_doubleClickTime;
        
        /// <summary>拖拽阈值</summary>
        public float dragThreshold => _m_dragThreshold;
        
        /// <summary>悬停高亮</summary>
        public bool highlightOnHover => _m_highlightOnHover;
        
        /// <summary>拖拽高度偏移</summary>
        public float dragHeightOffset => _m_dragHeightOffset;
        
        /// <summary>拖拽缩放倍数</summary>
        public float dragScaleMultiplier => _m_dragScaleMultiplier;
        
        /// <summary>放置动画时长</summary>
        public float placementAnimationDuration => _m_placementAnimationDuration;
        
        /// <summary>旋转动画时长</summary>
        public float rotationAnimationDuration => _m_rotationAnimationDuration;
        
        /// <summary>缩放动画时长</summary>
        public float scaleAnimationDuration => _m_scaleAnimationDuration;
        
        /// <summary>
        /// 应用配置到方块组件
        /// </summary>
        /// <param name="_gamePiece">游戏方块组件</param>
        /// <param name="_visualizer">可视化组件</param>
        /// <param name="_interactionController">交互控制器</param>
        public void applyConfiguration(GamePiece _gamePiece, PieceVisualizer _visualizer, PieceInteractionController _interactionController)
        {
            if (_gamePiece == null || _visualizer == null || _interactionController == null)
            {
                Debug.LogError("[PieceConfiguration] 无法应用配置：组件引用为空");
                return;
            }
            
            // 应用可视化设置
            _applyVisualizationSettings(_visualizer);
            
            // 应用交互设置
            _applyInteractionSettings(_interactionController);
            
            Debug.Log($"[PieceConfiguration] 配置已应用到方块 {_gamePiece.pieceId}");
        }
        
        /// <summary>
        /// 验证配置的完整性
        /// </summary>
        /// <returns>配置是否有效</returns>
        public bool validateConfiguration()
        {
            bool isValid = true;
            
            if (_m_pieceData == null)
            {
                Debug.LogError("[PieceConfiguration] 方块数据未设置");
                isValid = false;
            }
            
            if (_m_baseMaterial == null)
            {
                Debug.LogWarning("[PieceConfiguration] 基础材质未设置，将使用默认材质");
            }
            
            if (_m_cellSize <= 0)
            {
                Debug.LogError("[PieceConfiguration] 格子大小必须大于0");
                isValid = false;
            }
            
            if (_m_pieceHeight <= 0)
            {
                Debug.LogError("[PieceConfiguration] 方块高度必须大于0");
                isValid = false;
            }
            
            if (_m_dragThreshold < 0)
            {
                Debug.LogWarning("[PieceConfiguration] 拖拽阈值不应为负数");
                _m_dragThreshold = 0;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// 重置配置到默认值
        /// </summary>
        public void resetToDefaults()
        {
            _m_cellSize = 1.0f;
            _m_pieceHeight = 0.2f;
            _m_bevelSize = 0.05f;
            _m_enableDrag = true;
            _m_enableRotation = true;
            _m_doubleClickTime = 0.3f;
            _m_dragThreshold = 10f;
            _m_highlightOnHover = true;
            _m_dragHeightOffset = 1.0f;
            _m_dragScaleMultiplier = 1.2f;
            _m_placementAnimationDuration = 0.5f;
            _m_rotationAnimationDuration = 0.3f;
            _m_scaleAnimationDuration = 0.2f;
            
            Debug.Log("[PieceConfiguration] 配置已重置到默认值");
        }
        
        /// <summary>
        /// 应用可视化设置
        /// </summary>
        /// <param name="_visualizer">可视化组件</param>
        private void _applyVisualizationSettings(PieceVisualizer _visualizer)
        {
            // 通过反射或公共方法设置可视化参数
            // 这里需要PieceVisualizer提供相应的设置方法
            
            // 示例：如果PieceVisualizer有设置方法
            // _visualizer.setCellSize(_m_cellSize);
            // _visualizer.setPieceHeight(_m_pieceHeight);
            // _visualizer.setMaterials(_m_baseMaterial, _m_highlightMaterial, _m_previewMaterial, _m_invalidMaterial);
        }
        
        /// <summary>
        /// 应用交互设置
        /// </summary>
        /// <param name="_interactionController">交互控制器</param>
        private void _applyInteractionSettings(PieceInteractionController _interactionController)
        {
            _interactionController.setDragEnabled(_m_enableDrag);
            _interactionController.setRotationEnabled(_m_enableRotation);
            
            // 其他设置需要通过公共方法或属性设置
            // 这里需要PieceInteractionController提供相应的设置方法
        }
        
        /// <summary>
        /// Unity OnValidate方法 - Inspector中数据变化时调用
        /// </summary>
        private void OnValidate()
        {
            // 确保数值在合理范围内
            _m_cellSize = Mathf.Max(0.1f, _m_cellSize);
            _m_pieceHeight = Mathf.Max(0.01f, _m_pieceHeight);
            _m_bevelSize = Mathf.Clamp(_m_bevelSize, 0f, _m_cellSize * 0.4f);
            _m_doubleClickTime = Mathf.Max(0.1f, _m_doubleClickTime);
            _m_dragThreshold = Mathf.Max(0f, _m_dragThreshold);
            _m_dragHeightOffset = Mathf.Max(0f, _m_dragHeightOffset);
            _m_dragScaleMultiplier = Mathf.Max(0.1f, _m_dragScaleMultiplier);
            _m_placementAnimationDuration = Mathf.Max(0.1f, _m_placementAnimationDuration);
            _m_rotationAnimationDuration = Mathf.Max(0.1f, _m_rotationAnimationDuration);
            _m_scaleAnimationDuration = Mathf.Max(0.1f, _m_scaleAnimationDuration);
        }
    }
}