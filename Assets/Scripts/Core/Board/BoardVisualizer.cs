using UnityEngine;
using System.Collections.Generic;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Events;

namespace BlokusGame.Core.Board
{
    /// <summary>
    /// 棋盘可视化组件 - 负责Blokus棋盘的视觉渲染和交互反馈
    /// 管理棋盘网格显示、方块放置预览、有效位置高亮等视觉效果
    /// 支持触摸缩放、平移等移动端交互功能
    /// </summary>
    public class BoardVisualizer : MonoBehaviour
    {
        [Header("棋盘网格配置")]
        /// <summary>网格线材质</summary>
        [SerializeField] private Material _m_gridLineMaterial;
        
        /// <summary>网格单元大小（世界坐标）</summary>
        [SerializeField] private float _m_cellSize = 1.0f;
        
        /// <summary>网格线宽度</summary>
        [SerializeField] private float _m_gridLineWidth = 0.02f;
        
        /// <summary>棋盘背景材质</summary>
        [SerializeField] private Material _m_boardBackgroundMaterial;
        
        [Header("高亮显示配置")]
        /// <summary>有效位置高亮材质</summary>
        [SerializeField] private Material _m_validPositionMaterial;
        
        /// <summary>无效位置高亮材质</summary>
        [SerializeField] private Material _m_invalidPositionMaterial;
        
        /// <summary>预览方块材质</summary>
        [SerializeField] private Material _m_previewPieceMaterial;
        
        /// <summary>高亮显示的透明度</summary>
        [SerializeField] [Range(0.1f, 1.0f)] private float _m_highlightAlpha = 0.5f;
        
        [Header("缩放和平移配置")]
        /// <summary>最小缩放比例</summary>
        [SerializeField] private float _m_minZoom = 0.5f;
        
        /// <summary>最大缩放比例</summary>
        [SerializeField] private float _m_maxZoom = 3.0f;
        
        /// <summary>缩放速度</summary>
        [SerializeField] private float _m_zoomSpeed = 2.0f;
        
        /// <summary>平移速度</summary>
        [SerializeField] private float _m_panSpeed = 5.0f;
        
        /// <summary>平移边界限制</summary>
        [SerializeField] private Vector2 _m_panBounds = new Vector2(10f, 10f);
        
        [Header("动画配置")]
        /// <summary>高亮动画持续时间</summary>
        [SerializeField] private float _m_highlightAnimationDuration = 0.3f;
        
        /// <summary>方块放置动画持续时间</summary>
        [SerializeField] private float _m_placementAnimationDuration = 0.5f;
        
        // 私有字段
        /// <summary>棋盘系统引用</summary>
        private BoardSystem _m_boardSystem;
        
        /// <summary>网格线渲染器列表</summary>
        private List<LineRenderer> _m_gridLines;
        
        /// <summary>棋盘背景渲染器</summary>
        private MeshRenderer _m_backgroundRenderer;
        
        /// <summary>高亮显示对象池</summary>
        private Queue<GameObject> _m_highlightPool;
        
        /// <summary>当前激活的高亮显示对象</summary>
        private List<GameObject> _m_activeHighlights;
        
        /// <summary>预览方块对象</summary>
        private GameObject _m_previewPiece;
        
        /// <summary>摄像机引用</summary>
        private Camera _m_camera;
        
        /// <summary>当前缩放级别</summary>
        private float _m_currentZoom = 1.0f;
        
        /// <summary>当前平移偏移</summary>
        private Vector3 _m_currentPanOffset = Vector3.zero;
        
        /// <summary>是否正在进行触摸操作</summary>
        private bool _m_isTouching = false;
        
        /// <summary>上一帧的触摸位置</summary>
        private Vector2 _m_lastTouchPosition;
        
        /// <summary>上一帧的双指距离（用于缩放）</summary>
        private float _m_lastTouchDistance;
        
        /// <summary>棋盘根变换对象</summary>
        private Transform _m_boardRoot;
        
        /// <summary>初始棋盘位置</summary>
        private Vector3 _m_initialPosition;
        
        /// <summary>初始棋盘缩放</summary>
        private Vector3 _m_initialScale;
        
        /// <summary>
        /// Unity生命周期 - 初始化组件
        /// </summary>
        private void Awake()
        {
            // 获取摄像机引用
            _m_camera = Camera.main;
            if (_m_camera == null)
            {
                _m_camera = FindObjectOfType<Camera>();
            }
            
            // 初始化对象池和列表
            _m_gridLines = new List<LineRenderer>();
            _m_highlightPool = new Queue<GameObject>();
            _m_activeHighlights = new List<GameObject>();
            
            // 预创建高亮对象池
            _initializeHighlightPool();
        }
        
        /// <summary>
        /// Unity生命周期 - 开始时初始化
        /// </summary>
        private void Start()
        {
            // 订阅事件
            _subscribeToEvents();
            
            // 创建棋盘视觉元素
            _createBoardVisuals();
        }
        
        /// <summary>
        /// Unity生命周期 - 每帧更新
        /// </summary>
        private void Update()
        {
            // 处理触摸输入
            _handleTouchInput();
        }
        
        /// <summary>
        /// Unity生命周期 - 清理资源
        /// </summary>
        private void OnDestroy()
        {
            // 取消事件订阅
            _unsubscribeFromEvents();
        }
        
        /// <summary>
        /// 设置棋盘系统引用
        /// </summary>
        /// <param name="_boardSystem">棋盘系统实例</param>
        public void setBoardSystem(BoardSystem _boardSystem)
        {
            _m_boardSystem = _boardSystem;
        }
        
        /// <summary>
        /// 高亮显示有效放置位置
        /// </summary>
        /// <param name="_validPositions">有效位置列表</param>
        /// <param name="_playerId">玩家ID</param>
        public void highlightValidPositions(List<Vector2Int> _validPositions, int _playerId)
        {
            // 清除之前的高亮
            clearHighlights();
            
            // 为每个有效位置创建高亮显示
            foreach (Vector2Int position in _validPositions)
            {
                GameObject highlight = _getHighlightFromPool();
                if (highlight != null)
                {
                    // 设置位置
                    Vector3 worldPos = _boardPositionToWorldPosition(position);
                    highlight.transform.position = worldPos;
                    
                    // 设置材质和颜色
                    MeshRenderer renderer = highlight.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.material = _m_validPositionMaterial;
                        Color color = _getPlayerColor(_playerId);
                        color.a = _m_highlightAlpha;
                        renderer.material.color = color;
                    }
                    
                    // 激活对象
                    highlight.SetActive(true);
                    _m_activeHighlights.Add(highlight);
                    
                    // 播放高亮动画
                    _playHighlightAnimation(highlight);
                }
            }
        }
        
        /// <summary>
        /// 显示方块放置预览
        /// </summary>
        /// <param name="_piece">要预览的方块</param>
        /// <param name="_position">预览位置</param>
        /// <param name="_isValid">位置是否有效</param>
        public void showPiecePreview(_IGamePiece _piece, Vector2Int _position, bool _isValid)
        {
            if (_piece == null)
            {
                hidePiecePreview();
                return;
            }
            
            // 创建或更新预览对象
            if (_m_previewPiece == null)
            {
                _m_previewPiece = _createPreviewPiece();
            }
            
            // 设置预览位置
            Vector3 worldPos = _boardPositionToWorldPosition(_position);
            _m_previewPiece.transform.position = worldPos;
            
            // 更新预览方块的形状和颜色
            _updatePreviewPieceShape(_piece, _isValid);
            
            // 激活预览对象
            _m_previewPiece.SetActive(true);
        }
        
        /// <summary>
        /// 隐藏方块放置预览
        /// </summary>
        public void hidePiecePreview()
        {
            if (_m_previewPiece != null)
            {
                _m_previewPiece.SetActive(false);
            }
        }
        
        /// <summary>
        /// 清除所有高亮显示
        /// </summary>
        public void clearHighlights()
        {
            foreach (GameObject highlight in _m_activeHighlights)
            {
                _returnHighlightToPool(highlight);
            }
            _m_activeHighlights.Clear();
        }
        
        /// <summary>
        /// 播放方块放置动画
        /// </summary>
        /// <param name="_piece">放置的方块</param>
        /// <param name="_position">放置位置</param>
        public void playPiecePlacementAnimation(_IGamePiece _piece, Vector2Int _position)
        {
            // 创建临时动画对象
            GameObject animationPiece = _createAnimationPiece(_piece);
            Vector3 worldPos = _boardPositionToWorldPosition(_position);
            
            // 设置起始位置（稍微偏上）
            animationPiece.transform.position = worldPos + Vector3.up * 2f;
            
            // 播放下落动画
            StartCoroutine(_animatePiecePlacement(animationPiece, worldPos));
        }
        
        /// <summary>
        /// 设置棋盘缩放级别
        /// </summary>
        /// <param name="_zoomLevel">缩放级别</param>
        public void setZoomLevel(float _zoomLevel)
        {
            _m_currentZoom = Mathf.Clamp(_zoomLevel, _m_minZoom, _m_maxZoom);
            _applyCameraTransform();
        }
        
        /// <summary>
        /// 设置棋盘平移偏移
        /// </summary>
        /// <param name="_panOffset">平移偏移</param>
        public void setPanOffset(Vector3 _panOffset)
        {
            // 限制平移范围
            _panOffset.x = Mathf.Clamp(_panOffset.x, -_m_panBounds.x, _m_panBounds.x);
            _panOffset.z = Mathf.Clamp(_panOffset.z, -_m_panBounds.y, _m_panBounds.y);
            
            _m_currentPanOffset = _panOffset;
            _applyCameraTransform();
        }
        
        /// <summary>
        /// 重置摄像机视角到默认位置
        /// </summary>
        public void resetCameraView()
        {
            _m_currentZoom = 1.0f;
            _m_currentPanOffset = Vector3.zero;
            _applyCameraTransform();
        }
        
        /// <summary>
        /// 获取当前缩放级别
        /// </summary>
        /// <returns>当前缩放级别</returns>
        public float getCurrentZoomLevel()
        {
            return _m_currentZoom;
        }
        
        /// <summary>
        /// 获取当前平移偏移
        /// </summary>
        /// <returns>当前平移偏移</returns>
        public Vector3 getCurrentPanOffset()
        {
            return _m_currentPanOffset;
        }       
 
        // ==================== 私有方法实现 ====================
        
        /// <summary>
        /// 订阅游戏事件
        /// </summary>
        private void _subscribeToEvents()
        {
            GameEvents.onBoardInitialized += _onBoardInitialized;
            GameEvents.onBoardCleared += _onBoardCleared;
            GameEvents.onPiecePlaced += _onPiecePlaced;
        }
        
        /// <summary>
        /// 取消事件订阅
        /// </summary>
        private void _unsubscribeFromEvents()
        {
            GameEvents.onBoardInitialized -= _onBoardInitialized;
            GameEvents.onBoardCleared -= _onBoardCleared;
            GameEvents.onPiecePlaced -= _onPiecePlaced;
        }
        
        /// <summary>
        /// 初始化高亮对象池
        /// </summary>
        private void _initializeHighlightPool()
        {
            // 预创建50个高亮对象
            for (int i = 0; i < 50; i++)
            {
                GameObject highlight = _createHighlightObject();
                highlight.SetActive(false);
                _m_highlightPool.Enqueue(highlight);
            }
        }
        
        /// <summary>
        /// 创建棋盘视觉元素
        /// </summary>
        private void _createBoardVisuals()
        {
            // 创建棋盘背景
            _createBoardBackground();
            
            // 创建网格线
            _createGridLines();
        }
        
        /// <summary>
        /// 创建棋盘背景
        /// </summary>
        private void _createBoardBackground()
        {
            GameObject backgroundObj = new GameObject("BoardBackground");
            backgroundObj.transform.SetParent(transform);
            
            // 创建平面网格
            MeshFilter meshFilter = backgroundObj.AddComponent<MeshFilter>();
            _m_backgroundRenderer = backgroundObj.AddComponent<MeshRenderer>();
            
            // 创建20x20的平面网格
            meshFilter.mesh = _createPlaneMesh(20, 20, _m_cellSize);
            _m_backgroundRenderer.material = _m_boardBackgroundMaterial;
            
            // 设置位置（棋盘中心）
            backgroundObj.transform.position = new Vector3(9.5f * _m_cellSize, 0, 9.5f * _m_cellSize);
        }
        
        /// <summary>
        /// 创建网格线
        /// </summary>
        private void _createGridLines()
        {
            GameObject gridParent = new GameObject("GridLines");
            gridParent.transform.SetParent(transform);
            
            // 创建垂直线（21条，从0到20）
            for (int x = 0; x <= 20; x++)
            {
                GameObject lineObj = new GameObject($"VerticalLine_{x}");
                lineObj.transform.SetParent(gridParent.transform);
                
                LineRenderer line = lineObj.AddComponent<LineRenderer>();
                _setupGridLine(line);
                
                // 设置线的起点和终点
                Vector3 start = new Vector3(x * _m_cellSize, 0.01f, 0);
                Vector3 end = new Vector3(x * _m_cellSize, 0.01f, 20 * _m_cellSize);
                line.SetPosition(0, start);
                line.SetPosition(1, end);
                
                _m_gridLines.Add(line);
            }
            
            // 创建水平线（21条，从0到20）
            for (int z = 0; z <= 20; z++)
            {
                GameObject lineObj = new GameObject($"HorizontalLine_{z}");
                lineObj.transform.SetParent(gridParent.transform);
                
                LineRenderer line = lineObj.AddComponent<LineRenderer>();
                _setupGridLine(line);
                
                // 设置线的起点和终点
                Vector3 start = new Vector3(0, 0.01f, z * _m_cellSize);
                Vector3 end = new Vector3(20 * _m_cellSize, 0.01f, z * _m_cellSize);
                line.SetPosition(0, start);
                line.SetPosition(1, end);
                
                _m_gridLines.Add(line);
            }
        }
        
        /// <summary>
        /// 设置网格线属性
        /// </summary>
        /// <param name="_line">线渲染器</param>
        private void _setupGridLine(LineRenderer _line)
        {
            _line.material = _m_gridLineMaterial;
            _line.startWidth = _m_gridLineWidth;
            _line.endWidth = _m_gridLineWidth;
            _line.positionCount = 2;
            _line.useWorldSpace = true;
        }
        
        /// <summary>
        /// 创建平面网格
        /// </summary>
        /// <param name="_width">宽度格数</param>
        /// <param name="_height">高度格数</param>
        /// <param name="_cellSize">单元格大小</param>
        /// <returns>生成的网格</returns>
        private Mesh _createPlaneMesh(int _width, int _height, float _cellSize)
        {
            Mesh mesh = new Mesh();
            
            // 计算顶点
            Vector3[] vertices = new Vector3[(_width + 1) * (_height + 1)];
            Vector2[] uvs = new Vector2[vertices.Length];
            
            for (int y = 0; y <= _height; y++)
            {
                for (int x = 0; x <= _width; x++)
                {
                    int index = y * (_width + 1) + x;
                    vertices[index] = new Vector3(x * _cellSize - _width * _cellSize * 0.5f, 0, 
                                                 y * _cellSize - _height * _cellSize * 0.5f);
                    uvs[index] = new Vector2((float)x / _width, (float)y / _height);
                }
            }
            
            // 计算三角形
            int[] triangles = new int[_width * _height * 6];
            int triangleIndex = 0;
            
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int bottomLeft = y * (_width + 1) + x;
                    int bottomRight = bottomLeft + 1;
                    int topLeft = bottomLeft + (_width + 1);
                    int topRight = topLeft + 1;
                    
                    // 第一个三角形
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = bottomRight;
                    
                    // 第二个三角形
                    triangles[triangleIndex++] = bottomRight;
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = topRight;
                }
            }
            
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            
            return mesh;
        }
        
        /// <summary>
        /// 创建高亮对象
        /// </summary>
        /// <returns>高亮对象</returns>
        private GameObject _createHighlightObject()
        {
            GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            highlight.name = "PositionHighlight";
            highlight.transform.SetParent(transform);
            
            // 设置大小和位置
            highlight.transform.localScale = new Vector3(_m_cellSize * 0.9f, 0.1f, _m_cellSize * 0.9f);
            
            // 移除碰撞器
            Collider collider = highlight.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }
            
            return highlight;
        }
        
        /// <summary>
        /// 从对象池获取高亮对象
        /// </summary>
        /// <returns>高亮对象</returns>
        private GameObject _getHighlightFromPool()
        {
            if (_m_highlightPool.Count > 0)
            {
                return _m_highlightPool.Dequeue();
            }
            else
            {
                // 池中没有对象，创建新的
                return _createHighlightObject();
            }
        }
        
        /// <summary>
        /// 将高亮对象返回到对象池
        /// </summary>
        /// <param name="_highlight">高亮对象</param>
        private void _returnHighlightToPool(GameObject _highlight)
        {
            _highlight.SetActive(false);
            _m_highlightPool.Enqueue(_highlight);
        }
        
        /// <summary>
        /// 创建预览方块对象
        /// </summary>
        /// <returns>预览方块对象</returns>
        private GameObject _createPreviewPiece()
        {
            GameObject preview = new GameObject("PiecePreview");
            preview.transform.SetParent(transform);
            return preview;
        }
        
        /// <summary>
        /// 更新预览方块的形状和颜色
        /// </summary>
        /// <param name="_piece">方块数据</param>
        /// <param name="_isValid">位置是否有效</param>
        private void _updatePreviewPieceShape(_IGamePiece _piece, bool _isValid)
        {
            if (_m_previewPiece == null || _piece == null) return;
            
            // 清除之前的子对象
            foreach (Transform child in _m_previewPiece.transform)
            {
                DestroyImmediate(child.gameObject);
            }
            
            // 为方块的每个格子创建立方体
            Vector2Int[] shape = _piece.currentShape;
            Material material = _isValid ? _m_previewPieceMaterial : _m_invalidPositionMaterial;
            
            foreach (Vector2Int offset in shape)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(_m_previewPiece.transform);
                cube.transform.localPosition = new Vector3(offset.x * _m_cellSize, 0.2f, offset.y * _m_cellSize);
                cube.transform.localScale = new Vector3(_m_cellSize * 0.8f, 0.3f, _m_cellSize * 0.8f);
                
                // 设置材质和颜色
                MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
                renderer.material = material;
                
                Color color = _isValid ? _piece.pieceColor : Color.red;
                color.a = _m_highlightAlpha;
                renderer.material.color = color;
                
                // 移除碰撞器
                Collider collider = cube.GetComponent<Collider>();
                if (collider != null)
                {
                    DestroyImmediate(collider);
                }
            }
        }
        
        /// <summary>
        /// 创建动画方块对象
        /// </summary>
        /// <param name="_piece">方块数据</param>
        /// <returns>动画方块对象</returns>
        private GameObject _createAnimationPiece(_IGamePiece _piece)
        {
            GameObject animationPiece = new GameObject("AnimationPiece");
            animationPiece.transform.SetParent(transform);
            
            // 为方块的每个格子创建立方体
            Vector2Int[] shape = _piece.currentShape;
            
            foreach (Vector2Int offset in shape)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(animationPiece.transform);
                cube.transform.localPosition = new Vector3(offset.x * _m_cellSize, 0, offset.y * _m_cellSize);
                cube.transform.localScale = new Vector3(_m_cellSize * 0.9f, 0.2f, _m_cellSize * 0.9f);
                
                // 设置颜色
                MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
                renderer.material.color = _piece.pieceColor;
                
                // 移除碰撞器
                Collider collider = cube.GetComponent<Collider>();
                if (collider != null)
                {
                    DestroyImmediate(collider);
                }
            }
            
            return animationPiece;
        }
        
        /// <summary>
        /// 棋盘坐标转换为世界坐标
        /// </summary>
        /// <param name="_boardPosition">棋盘坐标</param>
        /// <returns>世界坐标</returns>
        private Vector3 _boardPositionToWorldPosition(Vector2Int _boardPosition)
        {
            return new Vector3(_boardPosition.x * _m_cellSize, 0, _boardPosition.y * _m_cellSize);
        }
        
        /// <summary>
        /// 世界坐标转换为棋盘坐标
        /// </summary>
        /// <param name="_worldPosition">世界坐标</param>
        /// <returns>棋盘坐标</returns>
        private Vector2Int _worldPositionToBoardPosition(Vector3 _worldPosition)
        {
            int x = Mathf.RoundToInt(_worldPosition.x / _m_cellSize);
            int z = Mathf.RoundToInt(_worldPosition.z / _m_cellSize);
            return new Vector2Int(x, z);
        }
        
        /// <summary>
        /// 获取玩家颜色
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>玩家颜色</returns>
        private Color _getPlayerColor(int _playerId)
        {
            switch (_playerId)
            {
                case 1: return Color.red;
                case 2: return Color.blue;
                case 3: return Color.green;
                case 4: return Color.yellow;
                default: return Color.white;
            }
        }
        
        /// <summary>
        /// 播放高亮动画
        /// </summary>
        /// <param name="_highlight">高亮对象</param>
        private void _playHighlightAnimation(GameObject _highlight)
        {
            // 简单的缩放动画
            StartCoroutine(_animateHighlight(_highlight));
        }
        
        /// <summary>
        /// 应用摄像机变换（缩放和平移）
        /// </summary>
        private void _applyCameraTransform()
        {
            if (_m_camera == null) return;
            
            // 计算摄像机位置
            Vector3 basePosition = new Vector3(10f, 15f, 10f); // 棋盘中心上方
            Vector3 targetPosition = basePosition + _m_currentPanOffset;
            
            // 应用缩放（通过调整摄像机距离）
            targetPosition.y = basePosition.y / _m_currentZoom;
            
            _m_camera.transform.position = targetPosition;
            _m_camera.transform.LookAt(new Vector3(10f, 0, 10f) + _m_currentPanOffset);
        }
        
        /// <summary>
        /// 处理触摸输入
        /// </summary>
        private void _handleTouchInput()
        {
            // 处理单点触摸（平移）
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                
                if (touch.phase == TouchPhase.Began)
                {
                    _m_isTouching = true;
                    _m_lastTouchPosition = touch.position;
                }
                else if (touch.phase == TouchPhase.Moved && _m_isTouching)
                {
                    Vector2 deltaPosition = touch.position - _m_lastTouchPosition;
                    
                    // 将屏幕坐标转换为世界坐标偏移
                    Vector3 panDelta = new Vector3(-deltaPosition.x, 0, -deltaPosition.y) * _m_panSpeed * Time.deltaTime;
                    setPanOffset(_m_currentPanOffset + panDelta);
                    
                    _m_lastTouchPosition = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    _m_isTouching = false;
                }
            }
            // 处理双点触摸（缩放）
            else if (Input.touchCount == 2)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);
                
                float currentDistance = Vector2.Distance(touch1.position, touch2.position);
                
                if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
                {
                    _m_lastTouchDistance = currentDistance;
                }
                else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
                {
                    float deltaDistance = currentDistance - _m_lastTouchDistance;
                    float zoomDelta = deltaDistance * _m_zoomSpeed * Time.deltaTime * 0.01f;
                    
                    setZoomLevel(_m_currentZoom + zoomDelta);
                    
                    _m_lastTouchDistance = currentDistance;
                }
            }
            else
            {
                _m_isTouching = false;
            }
        }
        
        // ==================== 事件处理方法 ====================
        
        /// <summary>
        /// 棋盘初始化事件处理
        /// </summary>
        private void _onBoardInitialized()
        {
            Debug.Log("[BoardVisualizer] 棋盘初始化完成，重新创建视觉元素");
            _createBoardVisuals();
        }
        
        /// <summary>
        /// 棋盘清空事件处理
        /// </summary>
        private void _onBoardCleared()
        {
            Debug.Log("[BoardVisualizer] 棋盘已清空，清除所有高亮");
            clearHighlights();
            hidePiecePreview();
        }
        
        /// <summary>
        /// 方块放置事件处理
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_piece">放置的方块</param>
        /// <param name="_position">放置位置</param>
        private void _onPiecePlaced(int _playerId, _IGamePiece _piece, Vector2Int _position)
        {
            Debug.Log($"[BoardVisualizer] 玩家 {_playerId} 放置方块，播放动画");
            playPiecePlacementAnimation(_piece, _position);
            clearHighlights();
            hidePiecePreview();
        }
        
        // ==================== 协程方法 ====================
        
        /// <summary>
        /// 高亮动画协程
        /// </summary>
        /// <param name="_highlight">高亮对象</param>
        /// <returns>协程</returns>
        private System.Collections.IEnumerator _animateHighlight(GameObject _highlight)
        {
            Vector3 originalScale = _highlight.transform.localScale;
            Vector3 targetScale = originalScale * 1.2f;
            
            float elapsedTime = 0f;
            
            // 放大动画
            while (elapsedTime < _m_highlightAnimationDuration * 0.5f)
            {
                float t = elapsedTime / (_m_highlightAnimationDuration * 0.5f);
                _highlight.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            elapsedTime = 0f;
            
            // 缩小动画
            while (elapsedTime < _m_highlightAnimationDuration * 0.5f)
            {
                float t = elapsedTime / (_m_highlightAnimationDuration * 0.5f);
                _highlight.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            _highlight.transform.localScale = originalScale;
        }
        
        /// <summary>
        /// 方块放置动画协程
        /// </summary>
        /// <param name="_animationPiece">动画方块对象</param>
        /// <param name="_targetPosition">目标位置</param>
        /// <returns>协程</returns>
        private System.Collections.IEnumerator _animatePiecePlacement(GameObject _animationPiece, Vector3 _targetPosition)
        {
            Vector3 startPosition = _animationPiece.transform.position;
            float elapsedTime = 0f;
            
            while (elapsedTime < _m_placementAnimationDuration)
            {
                float t = elapsedTime / _m_placementAnimationDuration;
                
                // 使用缓动函数创建更自然的下落效果
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                
                _animationPiece.transform.position = Vector3.Lerp(startPosition, _targetPosition, easedT);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            _animationPiece.transform.position = _targetPosition;
            
            // 动画完成后销毁对象
            Destroy(_animationPiece);
        }
    }
}