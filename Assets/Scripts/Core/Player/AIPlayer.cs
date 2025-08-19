using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Data;
using BlokusGame.Core.Events;

namespace BlokusGame.Core.Player
{
    /// <summary>
    /// AI玩家类 - 实现_IAIPlayer接口的AI玩家实现
    /// 继承自Player类，添加AI决策和自动游戏功能
    /// 支持不同难度等级的AI算法
    /// </summary>
    public class AIPlayer : Player, _IAIPlayer
    {
        [Header("AI配置")]
        /// <summary>AI难度等级</summary>
        [SerializeField] private AIDifficulty _m_difficulty = AIDifficulty.Medium;
        
        /// <summary>AI思考时间（秒）</summary>
        [SerializeField] private float _m_thinkingTime = 2.0f;
        
        /// <summary>AI是否正在思考中</summary>
        [SerializeField] private bool _m_isThinking = false;
        
        [Header("AI行为参数")]
        /// <summary>随机性因子，影响AI决策的随机程度</summary>
        [SerializeField] [Range(0f, 1f)] private float _m_randomnessFactor = 0.1f;
        
        /// <summary>防守权重，影响AI对阻挡对手的重视程度</summary>
        [SerializeField] [Range(0f, 2f)] private float _m_defensiveWeight = 0.5f;
        
        /// <summary>进攻权重，影响AI对扩展自己领域的重视程度</summary>
        [SerializeField] [Range(0f, 2f)] private float _m_aggressiveWeight = 1.0f;
        
        [Header("调试设置")]
        /// <summary>是否启用详细日志输出</summary>
        [SerializeField] private bool _m_enableDetailedLogging = false;
        
        // 私有字段
        /// <summary>当前思考协程</summary>
        private Coroutine _m_thinkingCoroutine;
        
        /// <summary>移动评估缓存</summary>
        private Dictionary<string, float> _m_moveEvaluationCache = new Dictionary<string, float>();
        
        // 接口属性实现
        /// <summary>当前AI难度等级</summary>
        public AIDifficulty difficulty => _m_difficulty;
        
        /// <summary>AI思考时间（秒）</summary>
        public float thinkingTime => _m_thinkingTime;
        
        /// <summary>AI是否正在思考中</summary>
        public bool isThinking => _m_isThinking;
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法 - 初始化AI组件
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            _initializeAI();
        }
        
        /// <summary>
        /// Unity Start方法 - 开始时设置
        /// </summary>
        protected override void Start()
        {
            base.Start();
            _subscribeToAIEvents();
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 清理AI资源
        /// </summary>
        protected override void OnDestroy()
        {
            _unsubscribeFromAIEvents();
            stopThinking();
            base.OnDestroy();
        }
        
        #endregion
        
        #region 接口实现
        
        /// <summary>
        /// 初始化AI玩家
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_playerName">玩家名称</param>
        /// <param name="_playerColor">玩家颜色</param>
        public override void initializePlayer(int _playerId, string _playerName, Color _playerColor)
        {
            base.initializePlayer(_playerId, _playerName, _playerColor);
            
            // 初始化AI特有设置
            _initializeAI();
            
            Debug.Log($"[AIPlayer] AI玩家 {_playerId} 初始化完成，难度: {_m_difficulty}");
        }
        
        /// <summary>
        /// 设置AI难度等级
        /// </summary>
        /// <param name="_difficulty">难度等级</param>
        public void setDifficulty(AIDifficulty _difficulty)
        {
            _m_difficulty = _difficulty;
            
            // 根据难度调整参数
            switch (_difficulty)
            {
                case AIDifficulty.Easy:
                    _m_randomnessFactor = 0.4f;
                    _m_thinkingTime = 1.0f;
                    break;
                case AIDifficulty.Medium:
                    _m_randomnessFactor = 0.2f;
                    _m_thinkingTime = 2.0f;
                    break;
                case AIDifficulty.Hard:
                    _m_randomnessFactor = 0.05f;
                    _m_thinkingTime = 3.0f;
                    break;
            }
            
            Debug.Log($"[AIPlayer] AI玩家 {playerId} 难度设置为: {_difficulty}");
        }
        
        /// <summary>
        /// 设置AI思考时间
        /// </summary>
        /// <param name="_thinkingTime">思考时间（秒）</param>
        public void setThinkingTime(float _thinkingTime)
        {
            _m_thinkingTime = Mathf.Max(0.5f, _thinkingTime);
            Debug.Log($"[AIPlayer] AI玩家 {playerId} 思考时间设置为: {_m_thinkingTime}秒");
        }
        
        /// <summary>
        /// AI进行决策并执行移动
        /// 这是一个协程方法，会根据难度等级进行不同复杂度的计算
        /// </summary>
        /// <param name="_gameBoard">游戏棋盘引用</param>
        /// <returns>协程枚举器</returns>
        public IEnumerator makeMove(_IGameBoard _gameBoard)
        {
            if (_m_isThinking || _gameBoard == null)
            {
                Debug.LogWarning($"[AIPlayer] AI玩家 {playerId} 无法开始思考");
                yield break;
            }
            
            _m_isThinking = true;
            
            // 触发AI开始思考事件
            GameEvents.instance.onAIThinkingStarted?.Invoke(playerId);
            
            Debug.Log($"[AIPlayer] AI玩家 {playerId} 开始思考...");
            
            // 模拟思考时间
            yield return new WaitForSeconds(_m_thinkingTime);
            
            // 获取最佳移动
            var bestMove = getBestMove(_gameBoard);
            
            _m_isThinking = false;
            
            if (bestMove.piece != null)
            {
                // 触发AI完成思考事件
                GameEvents.instance.onAIThinkingCompleted?.Invoke(playerId, bestMove.piece, bestMove.position);
                
                // 执行移动
                bool success = _gameBoard.placePiece(bestMove.piece, bestMove.position, playerId);
                
                if (success)
                {
                    usePiece(bestMove.piece);
                    Debug.Log($"[AIPlayer] AI玩家 {playerId} 执行移动: 方块{bestMove.piece.pieceId} 位置{bestMove.position}");
                }
                else
                {
                    Debug.LogError($"[AIPlayer] AI玩家 {playerId} 移动执行失败");
                }
            }
            else
            {
                Debug.Log($"[AIPlayer] AI玩家 {playerId} 无法找到有效移动，跳过回合");
                GameEvents.onPlayerSkipped?.Invoke(playerId);
            }
        }
        
        /// <summary>
        /// 评估指定移动的价值
        /// </summary>
        /// <param name="_piece">要放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <param name="_gameBoard">游戏棋盘引用</param>
        /// <returns>移动价值评分</returns>
        public float evaluateMove(_IGamePiece _piece, Vector2Int _position, _IGameBoard _gameBoard)
        {
            if (_piece == null || _gameBoard == null)
                return 0f;
            
            // 生成缓存键
            string cacheKey = $"{_piece.pieceId}_{_position.x}_{_position.y}";
            if (_m_moveEvaluationCache.ContainsKey(cacheKey))
            {
                return _m_moveEvaluationCache[cacheKey];
            }
            
            float score = 0f;
            
            // 基础分数：方块大小
            score += _piece.currentShape.Length * 10f;
            
            // 位置分数：靠近中心的位置更有价值
            Vector2 center = new Vector2(_gameBoard.boardSize / 2f, _gameBoard.boardSize / 2f);
            float distanceToCenter = Vector2.Distance(_position, center);
            score += (20f - distanceToCenter) * 2f;
            
            // 扩展性分数：评估放置后能创造多少新的连接点
            score += _evaluateExpansionPotential(_piece, _position, _gameBoard) * _m_aggressiveWeight;
            
            // 防守分数：评估是否阻挡了对手
            score += _evaluateDefensiveValue(_piece, _position, _gameBoard) * _m_defensiveWeight;
            
            // 角落奖励：占据角落位置有额外价值
            if (_isCornerPosition(_position, _gameBoard))
            {
                score += 15f;
            }
            
            // 边缘惩罚：过于靠近边缘的位置价值较低
            if (_isEdgePosition(_position, _gameBoard))
            {
                score -= 5f;
            }
            
            // 添加随机性
            if (_m_randomnessFactor > 0f)
            {
                score += Random.Range(-_m_randomnessFactor * 20f, _m_randomnessFactor * 20f);
            }
            
            // 缓存结果
            _m_moveEvaluationCache[cacheKey] = score;
            
            return score;
        }
        
        /// <summary>
        /// 获取AI推荐的最佳移动
        /// </summary>
        /// <param name="_gameBoard">游戏棋盘引用</param>
        /// <returns>最佳移动信息（方块和位置）</returns>
        public (_IGamePiece piece, Vector2Int position) getBestMove(_IGameBoard _gameBoard)
        {
            if (_gameBoard == null || availablePieces.Count == 0)
            {
                return (null, Vector2Int.zero);
            }
            
            // 清理缓存
            _m_moveEvaluationCache.Clear();
            
            _IGamePiece bestPiece = null;
            Vector2Int bestPosition = Vector2Int.zero;
            float bestScore = float.MinValue;
            
            // 根据难度等级选择不同的搜索策略
            switch (_m_difficulty)
            {
                case AIDifficulty.Easy:
                    return _getRandomMove(_gameBoard);
                    
                case AIDifficulty.Medium:
                    return _getGreedyMove(_gameBoard);
                    
                case AIDifficulty.Hard:
                    return _getOptimalMove(_gameBoard);
                    
                default:
                    return _getGreedyMove(_gameBoard);
            }
        }
        
        /// <summary>
        /// 停止AI思考过程
        /// </summary>
        public void stopThinking()
        {
            if (_m_thinkingCoroutine != null)
            {
                StopCoroutine(_m_thinkingCoroutine);
                _m_thinkingCoroutine = null;
            }
            
            _m_isThinking = false;
            Debug.Log($"[AIPlayer] AI玩家 {playerId} 停止思考");
        }
        
        #endregion
        
        #region 私有方法 - AI算法
        
        /// <summary>
        /// 初始化AI组件
        /// </summary>
        private void _initializeAI()
        {
            // 初始化移动评估缓存
            _m_moveEvaluationCache = new Dictionary<string, float>();
            
            // 根据难度设置初始参数
            setDifficulty(_m_difficulty);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[AIPlayer] AI组件初始化完成，难度: {_m_difficulty}");
            }
        }
        
        /// <summary>
        /// 获取随机移动（简单难度）
        /// </summary>
        /// <param name="_gameBoard">游戏棋盘</param>
        /// <returns>随机选择的移动</returns>
        private (_IGamePiece piece, Vector2Int position) _getRandomMove(_IGameBoard _gameBoard)
        {
            var validMoves = new List<(_IGamePiece piece, Vector2Int position)>();
            
            // 收集所有有效移动
            foreach (var piece in availablePieces)
            {
                if (piece.isPlaced) continue;
                
                var validPositions = _gameBoard.getValidPlacements(piece, playerId);
                foreach (var position in validPositions)
                {
                    validMoves.Add((piece, position));
                }
            }
            
            // 随机选择一个
            if (validMoves.Count > 0)
            {
                int randomIndex = Random.Range(0, validMoves.Count);
                return validMoves[randomIndex];
            }
            
            return (null, Vector2Int.zero);
        }
        
        /// <summary>
        /// 获取贪心移动（中等难度）
        /// </summary>
        /// <param name="_gameBoard">游戏棋盘</param>
        /// <returns>贪心算法选择的移动</returns>
        private (_IGamePiece piece, Vector2Int position) _getGreedyMove(_IGameBoard _gameBoard)
        {
            _IGamePiece bestPiece = null;
            Vector2Int bestPosition = Vector2Int.zero;
            float bestScore = float.MinValue;
            
            // 评估所有可能的移动
            foreach (var piece in availablePieces)
            {
                if (piece.isPlaced) continue;
                
                var validPositions = _gameBoard.getValidPlacements(piece, playerId);
                foreach (var position in validPositions)
                {
                    float score = evaluateMove(piece, position, _gameBoard);
                    
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestPiece = piece;
                        bestPosition = position;
                    }
                }
            }
            
            return (bestPiece, bestPosition);
        }
        
        /// <summary>
        /// 获取最优移动（困难难度）
        /// </summary>
        /// <param name="_gameBoard">游戏棋盘</param>
        /// <returns>最优算法选择的移动</returns>
        private (_IGamePiece piece, Vector2Int position) _getOptimalMove(_IGameBoard _gameBoard)
        {
            // 困难难度使用更复杂的前瞻搜索
            // 目前先使用贪心算法，后续可以实现minimax等算法
            return _getGreedyMove(_gameBoard);
        }
        
        /// <summary>
        /// 评估扩展潜力
        /// </summary>
        /// <param name="_piece">方块</param>
        /// <param name="_position">位置</param>
        /// <param name="_gameBoard">棋盘</param>
        /// <returns>扩展潜力分数</returns>
        private float _evaluateExpansionPotential(_IGamePiece _piece, Vector2Int _position, _IGameBoard _gameBoard)
        {
            float potential = 0f;
            
            // 计算放置后能创造的新连接点数量
            var occupiedPositions = _piece.getOccupiedPositions(_position);
            
            foreach (var pos in occupiedPositions)
            {
                // 检查对角位置
                Vector2Int[] diagonals = {
                    new Vector2Int(1, 1), new Vector2Int(1, -1),
                    new Vector2Int(-1, 1), new Vector2Int(-1, -1)
                };
                
                foreach (var diagonal in diagonals)
                {
                    Vector2Int checkPos = pos + diagonal;
                    
                    if (_gameBoard.isPositionValid(checkPos) && 
                        _gameBoard.getPositionOwner(checkPos) == 0)
                    {
                        potential += 3f; // 每个新连接点3分
                    }
                }
            }
            
            return potential;
        }
        
        /// <summary>
        /// 评估防守价值
        /// </summary>
        /// <param name="_piece">方块</param>
        /// <param name="_position">位置</param>
        /// <param name="_gameBoard">棋盘</param>
        /// <returns>防守价值分数</returns>
        private float _evaluateDefensiveValue(_IGamePiece _piece, Vector2Int _position, _IGameBoard _gameBoard)
        {
            float defensiveValue = 0f;
            
            // 检查是否阻挡了对手的扩展
            var occupiedPositions = _piece.getOccupiedPositions(_position);
            
            foreach (var pos in occupiedPositions)
            {
                // 检查周围是否有对手的方块
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        
                        Vector2Int checkPos = pos + new Vector2Int(dx, dy);
                        
                        if (_gameBoard.isPositionValid(checkPos))
                        {
                            int owner = _gameBoard.getPositionOwner(checkPos);
                            if (owner != 0 && owner != playerId)
                            {
                                defensiveValue += 2f; // 阻挡对手2分
                            }
                        }
                    }
                }
            }
            
            return defensiveValue;
        }
        
        /// <summary>
        /// 检查是否为角落位置
        /// </summary>
        /// <param name="_position">位置</param>
        /// <param name="_gameBoard">棋盘</param>
        /// <returns>是否为角落</returns>
        private bool _isCornerPosition(Vector2Int _position, _IGameBoard _gameBoard)
        {
            int size = _gameBoard.boardSize;
            return (_position.x == 0 || _position.x == size - 1) && 
                   (_position.y == 0 || _position.y == size - 1);
        }
        
        /// <summary>
        /// 检查是否为边缘位置
        /// </summary>
        /// <param name="_position">位置</param>
        /// <param name="_gameBoard">棋盘</param>
        /// <returns>是否为边缘</returns>
        private bool _isEdgePosition(Vector2Int _position, _IGameBoard _gameBoard)
        {
            int size = _gameBoard.boardSize;
            return _position.x == 0 || _position.x == size - 1 || 
                   _position.y == 0 || _position.y == size - 1;
        }
        
        /// <summary>
        /// 订阅AI相关事件
        /// </summary>
        private void _subscribeToAIEvents()
        {
            // 可以订阅需要的AI事件
        }
        
        /// <summary>
        /// 取消AI事件订阅
        /// </summary>
        private void _unsubscribeFromAIEvents()
        {
            // 取消AI事件订阅
        }
        
        #endregion
    }
}