using System.Collections.Generic;
using UnityEngine;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Events;
using BlokusGame.Core.Data;
using BlokusGame.Core.Player;

namespace BlokusGame.Core.Managers
{
    /// <summary>
    /// 玩家管理器 - 负责管理游戏中的所有玩家
    /// 提供玩家初始化、状态管理、回合控制等核心功能
    /// 支持人类玩家和AI玩家的统一管理，确保游戏流程的正确执行
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        [Header("玩家管理配置")]
        /// <summary>默认玩家颜色数组，用于为玩家分配颜色</summary>
        [SerializeField] private Color[] _m_defaultPlayerColors = { Color.red, Color.blue, Color.green, Color.yellow };
        
        /// <summary>是否启用调试日志输出</summary>
        [SerializeField] private bool _m_enableDebugLog = true;
        
        /// <summary>玩家列表，存储所有参与游戏的玩家实例</summary>
        private List<_IPlayer> _m_players;
        
        /// <summary>当前回合玩家的索引，用于回合制游戏控制</summary>
        private int _m_currentPlayerIndex = 0;
        
        /// <summary>玩家管理器是否已初始化的标志</summary>
        private bool _m_isInitialized = false;
        
        // 公共属性访问器
        /// <summary>获取玩家总数</summary>
        public int playerCount => _m_players?.Count ?? 0;
        
        /// <summary>获取当前回合玩家索引</summary>
        public int currentPlayerIndex => _m_currentPlayerIndex;
        
        /// <summary>获取是否已初始化</summary>
        public bool isInitialized => _m_isInitialized;
        
        /// <summary>
        /// Unity生命周期 - Awake
        /// 初始化玩家管理器的基础数据结构
        /// </summary>
        private void Awake()
        {
            _initializePlayerManager();
        }
        
        /// <summary>
        /// 初始化玩家管理器的基础数据结构
        /// 设置默认配置和准备玩家列表
        /// </summary>
        private void _initializePlayerManager()
        {
            _m_players = new List<_IPlayer>();
            _m_currentPlayerIndex = 0;
            _m_isInitialized = false;
            
            if (_m_enableDebugLog)
            {
                Debug.Log("[PlayerManager] 玩家管理器基础初始化完成");
            }
        }
        
        /// <summary>
        /// 初始化游戏玩家
        /// 根据指定的玩家数量和游戏模式创建相应的玩家实例
        /// </summary>
        /// <param name="_playerCount">玩家数量，必须在2-4之间</param>
        /// <param name="_gameMode">游戏模式，影响AI玩家的创建</param>
        public void initializePlayers(int _playerCount, GameMode _gameMode)
        {
            // 参数验证
            if (_playerCount < 2 || _playerCount > 4)
            {
                Debug.LogError($"[PlayerManager] 无效的玩家数量: {_playerCount}，有效范围: 2-4");
                return;
            }
            
            if (_m_enableDebugLog)
            {
                Debug.Log($"[PlayerManager] 开始初始化玩家 - 数量: {_playerCount}, 模式: {_gameMode}");
            }
            
            // 清空现有玩家
            _clearExistingPlayers();
            
            // 根据游戏模式创建玩家
            _createPlayersForMode(_playerCount, _gameMode);
            
            // 重置当前玩家索引
            _m_currentPlayerIndex = 0;
            _m_isInitialized = true;
            
            if (_m_enableDebugLog)
            {
                Debug.Log($"[PlayerManager] 玩家初始化完成，共创建 {_m_players.Count} 个玩家");
            }
            
            // 触发玩家初始化完成事件
            GameEvents.instance.onUpdateUI?.Invoke();
        }
        
        /// <summary>
        /// 清空现有玩家列表
        /// 在重新初始化前清理所有玩家数据
        /// </summary>
        private void _clearExistingPlayers()
        {
            if (_m_players != null && _m_players.Count > 0)
            {
                foreach (var player in _m_players)
                {
                    player?.resetPlayer();
                }
                _m_players.Clear();
                
                if (_m_enableDebugLog)
                {
                    Debug.Log("[PlayerManager] 已清空现有玩家列表");
                }
            }
        }
        
        /// <summary>
        /// 根据游戏模式创建玩家
        /// 不同模式下AI玩家的数量和配置不同
        /// </summary>
        /// <param name="_playerCount">总玩家数量</param>
        /// <param name="_gameMode">游戏模式</param>
        private void _createPlayersForMode(int _playerCount, GameMode _gameMode)
        {
            switch (_gameMode)
            {
                case GameMode.SinglePlayerVsAI:
                    _createSinglePlayerVsAI();
                    break;
                    
                case GameMode.LocalMultiplayer:
                    _createLocalMultiplayers(_playerCount);
                    break;
                    
                case GameMode.OnlineMultiplayer:
                    _createOnlineMultiplayers(_playerCount);
                    break;
                    
                case GameMode.Tutorial:
                    _createTutorialPlayers();
                    break;
                    
                default:
                    Debug.LogWarning($"[PlayerManager] 未知的游戏模式: {_gameMode}，使用默认配置");
                    _createLocalMultiplayers(_playerCount);
                    break;
            }
        }
        
        /// <summary>
        /// 创建单人对战AI模式的玩家
        /// 1个人类玩家 + 1个AI玩家
        /// </summary>
        private void _createSinglePlayerVsAI()
        {
            // 创建人类玩家
            GameObject humanPlayerObj = new GameObject("HumanPlayer_1");
            humanPlayerObj.transform.SetParent(transform);
            var humanPlayer = humanPlayerObj.AddComponent<BlokusGame.Core.Player.Player>();
            humanPlayer.initializePlayer(1, "玩家", _getPlayerColor(0));
            _m_players.Add(humanPlayer);
            
            // 创建AI玩家
            GameObject aiPlayerObj = new GameObject("AIPlayer_2");
            aiPlayerObj.transform.SetParent(transform);
            var aiPlayer = aiPlayerObj.AddComponent<AIPlayer>();
            aiPlayer.initializePlayer(2, "AI对手", _getPlayerColor(1));
            aiPlayer.setDifficulty(_IAIPlayer.AIDifficulty.Medium);
            _m_players.Add(aiPlayer);
            
            if (_m_enableDebugLog)
            {
                Debug.Log("[PlayerManager] 创建单人对战AI模式玩家完成");
            }
        }
        
        /// <summary>
        /// 创建本地多人模式的玩家
        /// 所有玩家都是人类玩家
        /// </summary>
        /// <param name="_playerCount">玩家数量</param>
        private void _createLocalMultiplayers(int _playerCount)
        {
            for (int i = 0; i < _playerCount; i++)
            {
                GameObject playerObj = new GameObject($"Player_{i + 1}");
                playerObj.transform.SetParent(transform);
                var player = playerObj.AddComponent<BlokusGame.Core.Player.Player>();
                player.initializePlayer(i + 1, $"玩家{i + 1}", _getPlayerColor(i));
                _m_players.Add(player);
            }
            
            if (_m_enableDebugLog)
            {
                Debug.Log($"[PlayerManager] 创建本地多人模式玩家完成，共 {_playerCount} 个玩家");
            }
        }
        
        /// <summary>
        /// 创建在线多人模式的玩家
        /// 目前与本地多人模式相同，后续扩展网络功能
        /// </summary>
        /// <param name="_playerCount">玩家数量</param>
        private void _createOnlineMultiplayers(int _playerCount)
        {
            // 目前与本地多人相同，后续添加网络玩家支持
            _createLocalMultiplayers(_playerCount);
            
            if (_m_enableDebugLog)
            {
                Debug.Log($"[PlayerManager] 创建在线多人模式玩家完成（临时实现）");
            }
        }
        
        /// <summary>
        /// 创建教程模式的玩家
        /// 1个人类玩家 + 1个简单AI用于教学
        /// </summary>
        private void _createTutorialPlayers()
        {
            // 创建学习者玩家
            GameObject learnerPlayerObj = new GameObject("LearnerPlayer_1");
            learnerPlayerObj.transform.SetParent(transform);
            var learnerPlayer = learnerPlayerObj.AddComponent<BlokusGame.Core.Player.Player>();
            learnerPlayer.initializePlayer(1, "学习者", _getPlayerColor(0));
            _m_players.Add(learnerPlayer);
            
            // 创建教学AI
            GameObject tutorAIObj = new GameObject("TutorAI_2");
            tutorAIObj.transform.SetParent(transform);
            var tutorAI = tutorAIObj.AddComponent<AIPlayer>();
            tutorAI.initializePlayer(2, "教学助手", _getPlayerColor(1));
            tutorAI.setDifficulty(_IAIPlayer.AIDifficulty.Easy);
            _m_players.Add(tutorAI);
            
            if (_m_enableDebugLog)
            {
                Debug.Log("[PlayerManager] 创建教程模式玩家完成");
            }
        }
        
        /// <summary>
        /// 获取指定索引的玩家颜色
        /// 如果索引超出范围，则循环使用颜色数组
        /// </summary>
        /// <param name="_index">玩家索引</param>
        /// <returns>玩家颜色</returns>
        private Color _getPlayerColor(int _index)
        {
            if (_m_defaultPlayerColors == null || _m_defaultPlayerColors.Length == 0)
            {
                Debug.LogWarning("[PlayerManager] 默认玩家颜色数组为空，使用白色");
                return Color.white;
            }
            
            return _m_defaultPlayerColors[_index % _m_defaultPlayerColors.Length];
        }
        
        /// <summary>
        /// 获取当前回合的玩家
        /// 返回当前轮到的玩家实例
        /// </summary>
        /// <returns>当前玩家实例，如果没有玩家则返回null</returns>
        public _IPlayer getCurrentPlayer()
        {
            if (!_m_isInitialized)
            {
                Debug.LogWarning("[PlayerManager] 玩家管理器未初始化");
                return null;
            }
            
            if (_m_players == null || _m_players.Count == 0)
            {
                Debug.LogWarning("[PlayerManager] 没有可用的玩家");
                return null;
            }
            
            if (_m_currentPlayerIndex < 0 || _m_currentPlayerIndex >= _m_players.Count)
            {
                Debug.LogError($"[PlayerManager] 当前玩家索引无效: {_m_currentPlayerIndex}");
                return null;
            }
            
            return _m_players[_m_currentPlayerIndex];
        }
        
        /// <summary>
        /// 获取玩家数量
        /// </summary>
        /// <returns>玩家数量</returns>
        public int getPlayerCount()
        {
            Debug.Log($"[PlayerManager] 获取玩家数量: {_m_players.Count} - 临时实现");
            return _m_players.Count;
        }
        
        /// <summary>
        /// 检查玩家是否活跃
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>是否活跃</returns>
        public bool isPlayerActive(int _playerId)
        {
            var player = getPlayer(_playerId);
            if (player != null)
            {
                Debug.Log($"[PlayerManager] 玩家 {_playerId} 活跃状态: {player.isActive}");
                return player.isActive;
            }
            
            Debug.LogWarning($"[PlayerManager] 玩家 {_playerId} 不存在");
            return false;
        }
        
        /// <summary>
        /// 获取指定玩家
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <returns>玩家实例</returns>
        public _IPlayer getPlayer(int _playerId)
        {
            if (_playerId < 1 || _playerId > _m_players.Count)
            {
                Debug.LogWarning($"[PlayerManager] 无效的玩家ID: {_playerId}");
                return null;
            }
            
            Debug.Log($"[PlayerManager] 获取玩家 {_playerId} - 临时实现");
            return _m_players[_playerId - 1];
        }
        
        /// <summary>
        /// 重置所有玩家
        /// </summary>
        public void resetAllPlayers()
        {
            Debug.Log("[PlayerManager] 重置所有玩家 - 临时实现");
            
            foreach (var player in _m_players)
            {
                player?.resetPlayer();
            }
            
            _m_currentPlayerIndex = 0;
        }
    }
}