using UnityEngine;
using System.Collections.Generic;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Data;

namespace BlokusGame.Core.Pieces
{
    /// <summary>
    /// 方块可视化组件 - 负责方块的3D渲染和视觉效果
    /// 管理方块的网格生成、材质应用、动画效果等视觉表现
    /// 支持拖拽预览、选择高亮、放置动画等交互反馈
    /// </summary>
    public class PieceVisualizer : MonoBehaviour
    {
        [Header("可视化配置")]
        /// <summary>方块单元格的大小（世界坐标）</summary>
        [SerializeField] private float _m_cellSize = 1.0f;
        
        /// <summary>方块的高度</summary>
        [SerializeField] private float _m_pieceHeight = 0.2f;
        
        /// <summary>方块边缘的圆角半径</summary>
        [SerializeField] private float _m_cornerRadius = 0.1f;
        
        [Header("材质配置")]
        /// <summary>方块的默认材质</summary>
        [SerializeField] private Material _m_defaultMaterial;
        
        /// <summary>方块选中时的材质</summary>
        [SerializeField] private Material _m_selectedMaterial;
        
        /// <summary>方块预览时的材质</summary>
        [SerializeField] private Material _m_previewMaterial;
        
        /// <summary>方块无效放置时的材质</summary>
        [SerializeField] private Material _m_invalidMaterial;
        
        [Header("动画配置")]
        /// <summary>选择动画的持续时间</summary>
        [SerializeField] private float _m_selectionAnimationDuration = 0.3f;
        
        /// <summary>放置动画的持续时间</summary>
        [SerializeField] private float _m_placementAnimationDuration = 0.5f;
        
        /// <summary>旋转动画的持续时间</summary>
        [SerializeField] private float _m_rotationAnimationDuration = 0.2f;
        
        // 私有字段
        /// <summary>关联的游戏方块组件</summary>
        private GamePiece _m_gamePiece;
        
        /// <summary>方块的网格渲染器列表</summary>
        private List<MeshRenderer> _m_meshRenderers = new List<MeshRenderer>();
        
        /// <summary>方块的碰撞器列表</summary>
        private List<Collider> _m_colliders = new List<Collider>();
        
        /// <summary>方块的子对象列表（每个格子一个）</summary>
        private List<GameObject> _m_cellObjects = new List<GameObject>();
        
        /// <summary>当前的可视化状态</summary>
        private PieceVisualState _m_currentState = PieceVisualState.Normal;
        
        /// <summary>原始位置（用于动画）</summary>
        private Vector3 _m_originalPosition;
        
        /// <summary>原始旋转（用于动画）</summary>
        private Quaternion _m_originalRotation;
        
        /// <summary>原始缩放（用于动画）</summary>
        private Vector3 _m_originalScale;
        
        /// <summary>当前正在播放的动画协程</summary>
        private Coroutine _m_currentAnimation;
        
        /// <summary>方块可视化状态枚举</summary>
        public enum PieceVisualState
        {
            /// <summary>正常状态</summary>
            Normal,
            /// <summary>选中状态</summary>
            Selected,
            /// <summary>预览状态（有效位置）</summary>
            PreviewValid,
            /// <summary>预览状态（无效位置）</summary>
            PreviewInvalid,
            /// <summary>已放置状态</summary>
            Placed
        }
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法 - 初始化组件
        /// </summary>
        private void Awake()
        {
            _m_gamePiece = GetComponent<GamePiece>();
            if (_m_gamePiece == null)
            {
                Debug.LogError("[PieceVisualizer] 未找到GamePiece组件");
            }
            
            // 保存原始变换信息
            _m_originalPosition = transform.localPosition;
            _m_originalRotation = transform.localRotation;
            _m_originalScale = transform.localScale;
        }
        
        /// <summary>
        /// Unity Start方法 - 初始化可视化
        /// </summary>
        private void Start()
        {
            if (_m_gamePiece != null)
            {
                _generatePieceVisual();
            }
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 清理资源
        /// </summary>
        private void OnDestroy()
        {
            if (_m_currentAnimation != null)
            {
                StopCoroutine(_m_currentAnimation);
            }
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 初始化方块可视化
        /// </summary>
        /// <param name="_gamePiece">关联的游戏方块</param>
        public void initialize(GamePiece _gamePiece)
        {
            _m_gamePiece = _gamePiece;
            _generatePieceVisual();
        }
        
        /// <summary>
        /// 设置方块的可视化状态
        /// </summary>
        /// <param name="_state">新的可视化状态</param>
        /// <param name="_animated">是否使用动画过渡</param>
        public void setVisualState(PieceVisualState _state, bool _animated = true)
        {
            if (_m_currentState == _state) return;
            
            _m_currentState = _state;
            
            if (_animated)
            {
                _playStateTransitionAnimation(_state);
            }
            else
            {
                _applyVisualState(_state);
            }
        }
        
        /// <summary>
        /// 更新方块的颜色
        /// </summary>
        /// <param name="_color">新的颜色</param>
        public void updateColor(Color _color)
        {
            foreach (var renderer in _m_meshRenderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.color = _color;
                }
            }
        }
        
        /// <summary>
        /// 播放方块旋转动画
        /// </summary>
        /// <param name="_targetRotation">目标旋转角度</param>
        public void playRotationAnimation(Quaternion _targetRotation)
        {
            if (_m_currentAnimation != null)
            {
                StopCoroutine(_m_currentAnimation);
            }
            
            _m_currentAnimation = StartCoroutine(_animateRotation(_targetRotation));
        }
        
        /// <summary>
        /// 播放方块放置动画
        /// </summary>
        /// <param name="_targetPosition">目标位置</param>
        public void playPlacementAnimation(Vector3 _targetPosition)
        {
            if (_m_currentAnimation != null)
            {
                StopCoroutine(_m_currentAnimation);
            }
            
            _m_currentAnimation = StartCoroutine(_animatePlacement(_targetPosition));
        }
        
        /// <summary>
        /// 重置方块到原始状态
        /// </summary>
        public void resetToOriginalState()
        {
            if (_m_currentAnimation != null)
            {
                StopCoroutine(_m_currentAnimation);
                _m_currentAnimation = null;
            }
            
            transform.localPosition = _m_originalPosition;
            transform.localRotation = _m_originalRotation;
            transform.localScale = _m_originalScale;
            
            setVisualState(PieceVisualState.Normal, false);
        }
        
        /// <summary>
        /// 设置方块的交互性（是否可以点击）
        /// </summary>
        /// <param name="_interactive">是否可交互</param>
        public void setInteractive(bool _interactive)
        {
            foreach (var collider in _m_colliders)
            {
                if (collider != null)
                {
                    collider.enabled = _interactive;
                }
            }
        }
        
        #endregion
        
        #region 私有方法 - 可视化生成
        
        /// <summary>
        /// 生成方块的3D可视化
        /// 根据方块形状创建对应的3D网格
        /// </summary>
        private void _generatePieceVisual()
        {
            if (_m_gamePiece == null) return;
            
            // 清理现有的可视化对象
            _clearExistingVisual();
            
            // 获取方块当前形状
            Vector2Int[] shape = _m_gamePiece.currentShape;
            if (shape == null || shape.Length == 0) return;
            
            // 为每个格子创建3D对象
            foreach (Vector2Int cellPos in shape)
            {
                GameObject cellObj = _createCellObject(cellPos);
                _m_cellObjects.Add(cellObj);
            }
            
            Debug.Log($"[PieceVisualizer] 为方块 {_m_gamePiece.pieceId} 生成了 {_m_cellObjects.Count} 个格子的可视化");
        }
        
        /// <summary>
        /// 创建单个格子的3D对象
        /// </summary>
        /// <param name="_cellPosition">格子的相对位置</param>
        /// <returns>创建的格子对象</returns>
        private GameObject _createCellObject(Vector2Int _cellPosition)
        {
            // 创建格子对象
            GameObject cellObj = new GameObject($"Cell_{_cellPosition.x}_{_cellPosition.y}");
            cellObj.transform.SetParent(transform);
            
            // 设置位置
            Vector3 worldPos = new Vector3(
                _cellPosition.x * _m_cellSize,
                0,
                _cellPosition.y * _m_cellSize
            );
            cellObj.transform.localPosition = worldPos;
            
            // 添加网格组件
            MeshFilter meshFilter = cellObj.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = cellObj.AddComponent<MeshRenderer>();
            
            // 生成立方体网格
            meshFilter.mesh = _createCellMesh();
            
            // 设置材质
            meshRenderer.material = _m_defaultMaterial;
            if (_m_gamePiece != null)
            {
                meshRenderer.material.color = _m_gamePiece.pieceColor;
            }
            
            // 添加碰撞器
            BoxCollider collider = cellObj.AddComponent<BoxCollider>();
            collider.size = new Vector3(_m_cellSize, _m_pieceHeight, _m_cellSize);
            collider.center = new Vector3(0, _m_pieceHeight * 0.5f, 0);
            
            // 保存引用
            _m_meshRenderers.Add(meshRenderer);
            _m_colliders.Add(collider);
            
            return cellObj;
        }
        
        /// <summary>
        /// 创建格子的网格数据
        /// </summary>
        /// <returns>生成的网格</returns>
        private Mesh _createCellMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "CellMesh";
            
            // 创建立方体顶点
            Vector3[] vertices = new Vector3[24]; // 6面 x 4顶点
            Vector3[] normals = new Vector3[24];
            Vector2[] uvs = new Vector2[24];
            int[] triangles = new int[36]; // 6面 x 2三角形 x 3顶点
            
            float halfSize = _m_cellSize * 0.5f;
            float height = _m_pieceHeight;
            
            // 定义立方体的6个面
            Vector3[] faceNormals = {
                Vector3.up, Vector3.down, Vector3.forward, Vector3.back, Vector3.right, Vector3.left
            };
            
            int vertexIndex = 0;
            int triangleIndex = 0;
            
            // 为每个面生成顶点和三角形
            for (int face = 0; face < 6; face++)
            {
                Vector3 normal = faceNormals[face];
                Vector3[] faceVertices = _getFaceVertices(normal, halfSize, height);
                
                // 添加顶点
                for (int i = 0; i < 4; i++)
                {
                    vertices[vertexIndex + i] = faceVertices[i];
                    normals[vertexIndex + i] = normal;
                    uvs[vertexIndex + i] = new Vector2(i % 2, i / 2);
                }
                
                // 添加三角形（两个三角形组成一个面）
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + 1;
                triangles[triangleIndex + 2] = vertexIndex + 2;
                
                triangles[triangleIndex + 3] = vertexIndex + 2;
                triangles[triangleIndex + 4] = vertexIndex + 1;
                triangles[triangleIndex + 5] = vertexIndex + 3;
                
                vertexIndex += 4;
                triangleIndex += 6;
            }
            
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            
            return mesh;
        }
        
        /// <summary>
        /// 获取指定面的四个顶点
        /// </summary>
        /// <param name="_normal">面的法向量</param>
        /// <param name="_halfSize">格子的半尺寸</param>
        /// <param name="_height">格子的高度</param>
        /// <returns>面的四个顶点</returns>
        private Vector3[] _getFaceVertices(Vector3 _normal, float _halfSize, float _height)
        {
            Vector3[] vertices = new Vector3[4];
            
            if (_normal == Vector3.up)
            {
                vertices[0] = new Vector3(-_halfSize, _height, -_halfSize);
                vertices[1] = new Vector3(_halfSize, _height, -_halfSize);
                vertices[2] = new Vector3(-_halfSize, _height, _halfSize);
                vertices[3] = new Vector3(_halfSize, _height, _halfSize);
            }
            else if (_normal == Vector3.down)
            {
                vertices[0] = new Vector3(-_halfSize, 0, _halfSize);
                vertices[1] = new Vector3(_halfSize, 0, _halfSize);
                vertices[2] = new Vector3(-_halfSize, 0, -_halfSize);
                vertices[3] = new Vector3(_halfSize, 0, -_halfSize);
            }
            else if (_normal == Vector3.forward)
            {
                vertices[0] = new Vector3(-_halfSize, 0, _halfSize);
                vertices[1] = new Vector3(_halfSize, 0, _halfSize);
                vertices[2] = new Vector3(-_halfSize, _height, _halfSize);
                vertices[3] = new Vector3(_halfSize, _height, _halfSize);
            }
            else if (_normal == Vector3.back)
            {
                vertices[0] = new Vector3(_halfSize, 0, -_halfSize);
                vertices[1] = new Vector3(-_halfSize, 0, -_halfSize);
                vertices[2] = new Vector3(_halfSize, _height, -_halfSize);
                vertices[3] = new Vector3(-_halfSize, _height, -_halfSize);
            }
            else if (_normal == Vector3.right)
            {
                vertices[0] = new Vector3(_halfSize, 0, _halfSize);
                vertices[1] = new Vector3(_halfSize, 0, -_halfSize);
                vertices[2] = new Vector3(_halfSize, _height, _halfSize);
                vertices[3] = new Vector3(_halfSize, _height, -_halfSize);
            }
            else if (_normal == Vector3.left)
            {
                vertices[0] = new Vector3(-_halfSize, 0, -_halfSize);
                vertices[1] = new Vector3(-_halfSize, 0, _halfSize);
                vertices[2] = new Vector3(-_halfSize, _height, -_halfSize);
                vertices[3] = new Vector3(-_halfSize, _height, _halfSize);
            }
            
            return vertices;
        }
        
        /// <summary>
        /// 清理现有的可视化对象
        /// </summary>
        private void _clearExistingVisual()
        {
            foreach (GameObject cellObj in _m_cellObjects)
            {
                if (cellObj != null)
                {
                    DestroyImmediate(cellObj);
                }
            }
            
            _m_cellObjects.Clear();
            _m_meshRenderers.Clear();
            _m_colliders.Clear();
        }
        
        #endregion
        
        #region 私有方法 - 动画和状态
        
        /// <summary>
        /// 应用可视化状态
        /// </summary>
        /// <param name="_state">要应用的状态</param>
        private void _applyVisualState(PieceVisualState _state)
        {
            Material targetMaterial = _getStateMaterial(_state);
            
            foreach (var renderer in _m_meshRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = targetMaterial;
                    if (_m_gamePiece != null)
                    {
                        renderer.material.color = _m_gamePiece.pieceColor;
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取状态对应的材质
        /// </summary>
        /// <param name="_state">可视化状态</param>
        /// <returns>对应的材质</returns>
        private Material _getStateMaterial(PieceVisualState _state)
        {
            switch (_state)
            {
                case PieceVisualState.Selected:
                    return _m_selectedMaterial ?? _m_defaultMaterial;
                case PieceVisualState.PreviewValid:
                    return _m_previewMaterial ?? _m_defaultMaterial;
                case PieceVisualState.PreviewInvalid:
                    return _m_invalidMaterial ?? _m_defaultMaterial;
                default:
                    return _m_defaultMaterial;
            }
        }
        
        /// <summary>
        /// 播放状态转换动画
        /// </summary>
        /// <param name="_state">目标状态</param>
        private void _playStateTransitionAnimation(PieceVisualState _state)
        {
            if (_m_currentAnimation != null)
            {
                StopCoroutine(_m_currentAnimation);
            }
            
            _m_currentAnimation = StartCoroutine(_animateStateTransition(_state));
        }
        
        /// <summary>
        /// 状态转换动画协程
        /// </summary>
        /// <param name="_targetState">目标状态</param>
        /// <returns>协程</returns>
        private System.Collections.IEnumerator _animateStateTransition(PieceVisualState _targetState)
        {
            float duration = _m_selectionAnimationDuration;
            float elapsedTime = 0f;
            
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = _targetState == PieceVisualState.Selected ? startScale * 1.1f : _m_originalScale;
            
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            transform.localScale = targetScale;
            _applyVisualState(_targetState);
            
            _m_currentAnimation = null;
        }
        
        /// <summary>
        /// 旋转动画协程
        /// </summary>
        /// <param name="_targetRotation">目标旋转</param>
        /// <returns>协程</returns>
        private System.Collections.IEnumerator _animateRotation(Quaternion _targetRotation)
        {
            float duration = _m_rotationAnimationDuration;
            float elapsedTime = 0f;
            
            Quaternion startRotation = transform.localRotation;
            
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                transform.localRotation = Quaternion.Lerp(startRotation, _targetRotation, t);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            transform.localRotation = _targetRotation;
            _m_currentAnimation = null;
        }
        
        /// <summary>
        /// 放置动画协程
        /// </summary>
        /// <param name="_targetPosition">目标位置</param>
        /// <returns>协程</returns>
        private System.Collections.IEnumerator _animatePlacement(Vector3 _targetPosition)
        {
            float duration = _m_placementAnimationDuration;
            float elapsedTime = 0f;
            
            Vector3 startPosition = transform.position;
            Vector3 highPoint = Vector3.Lerp(startPosition, _targetPosition, 0.5f) + Vector3.up * 2f;
            
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                
                // 使用抛物线轨迹
                Vector3 currentPos;
                if (t < 0.5f)
                {
                    currentPos = Vector3.Lerp(startPosition, highPoint, t * 2f);
                }
                else
                {
                    currentPos = Vector3.Lerp(highPoint, _targetPosition, (t - 0.5f) * 2f);
                }
                
                transform.position = currentPos;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            transform.position = _targetPosition;
            setVisualState(PieceVisualState.Placed, false);
            
            _m_currentAnimation = null;
        }
        
        #endregion
    }
}