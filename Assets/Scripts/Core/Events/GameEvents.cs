using UnityEngine;
using System;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Data;

namespace BlokusGame.Core.Events
{
    /// <summary>
    /// 游戏事件系统 - 单例模式
    /// 定义Blokus游戏中的所有事件，提供统一的事件管理接口
    /// 使用单例模式确保事件系统的唯一性和可管理性
    /// </summary>
    public class GameEvents : MonoBehaviour
    {
        /// <summary>单例实例</summary>
        private static GameEvents _s_instance;
        
        /// <summary>获取单例实例</summary>
        public static GameEvents instance
        {
            get
            {
                if (_s_instance == null)
                {
                    _s_instance = FindObjectOfType<GameEvents>();
                    if (_s_instance == null)
                    {
                        GameObject eventSystemObject = new GameObject("GameEvents");
                        _s_instance = eventSystemObject.AddComponent<GameEvents>();
                        DontDestroyOnLoad(eventSystemObject);
                    }
                }
                return _s_instance;
            }
        }

        /// <summary>
        /// Unity生命周期 - Awake
        /// 确保单例唯一性
        /// </summary>
        private void Awake()
        {
            if (_s_instance == null)
            {
                _s_instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_s_instance != this)
            {
                Destroy(gameObject);
            }
        }

        // 配置系统事件
        
        /// <summary>配置加载完成事件</summary>
        public System.Action onConfigLoaded;
        
        /// <summary>游戏状态改变事件</summary>
        public System.Action<GameState, GameState> onGameStateChanged;
        
        /// <summary>游戏模式改变事件</summary>
        public System.Action<GameMode> onGameModeChanged;
        
        // 游戏状态事件
        
        /// <summary>
        /// 游戏开始事件
        /// </summary>
        /// <param name="_playerCount">玩家数量</param>
        /// <param name="_gameMode">游戏模式</param>
        public System.Action<int, GameMode> onGameStarted;
        
        /// <summary>
        /// 游戏结束事件
        /// </summary>
        /// <param name="_winnerId">获胜玩家ID</param>
        /// <param name="_finalScores">所有玩家的最终分数</param>
        public System.Action<int, int[]> onGameEnded;
        
        /// <summary>
        /// 游戏暂停事件
        /// </summary>
        public System.Action onGamePaused;
        
        /// <summary>
        /// 游戏恢复事件
        /// </summary>
        public System.Action onGameResumed;
        
        // 回合管理事件
        
        /// <summary>
        /// 回合开始事件
        /// </summary>
        /// <param name="_playerId">当前回合玩家ID</param>
        public System.Action<int> onTurnStarted;
        
        /// <summary>
        /// 回合结束事件
        /// </summary>
        /// <param name="_playerId">结束回合的玩家ID</param>
        public System.Action<int> onTurnEnded;
        
        /// <summary>
        /// 玩家跳过回合事件
        /// </summary>
        /// <param name="_playerId">跳过回合的玩家ID</param>
        /// <param name="_reason">跳过原因</param>
        public System.Action<int, string> onTurnSkipped;
        
        // 方块操作事件
        
        /// <summary>
        /// 方块选择事件
        /// </summary>
        /// <param name="_piece">被选择的方块</param>
        /// <param name="_playerId">选择方块的玩家ID</param>
        public System.Action<_IGamePiece, int> onPieceSelected;
        
        /// <summary>
        /// 方块放置成功事件（实例版本）
        /// </summary>
        /// <param name="_piece">放置的方块</param>
        /// <param name="_position">放置位置</param>
        /// <param name="_playerId">放置方块的玩家ID</param>
        public System.Action<_IGamePiece, Vector2Int, int> onPiecePlacedInstance;
        
        /// <summary>
        /// 方块放置失败事件
        /// </summary>
        /// <param name="_piece">尝试放置的方块</param>
        /// <param name="_position">尝试放置的位置</param>
        /// <param name="_playerId">尝试放置的玩家ID</param>
        /// <param name="_reason">失败原因</param>
        public System.Action<_IGamePiece, Vector2Int, int, string> onPiecePlacementFailed;
        
        /// <summary>
        /// 方块旋转事件
        /// </summary>
        /// <param name="_piece">旋转的方块</param>
        /// <param name="_playerId">操作的玩家ID</param>
        public System.Action<_IGamePiece, int> onPieceRotated;
        
        /// <summary>
        /// 方块翻转事件
        /// </summary>
        /// <param name="_piece">翻转的方块</param>
        /// <param name="_playerId">操作的玩家ID</param>
        public System.Action<_IGamePiece, int> onPieceFlipped;
        
        /// <summary>
        /// 方块拖拽开始事件
        /// </summary>
        /// <param name="_piece">开始拖拽的方块</param>
        public System.Action<_IGamePiece> onPieceDragStart;
        
        /// <summary>
        /// 方块拖拽中事件
        /// </summary>
        /// <param name="_piece">正在拖拽的方块</param>
        /// <param name="_position">当前位置</param>
        public System.Action<_IGamePiece, Vector3> onPieceDragging;
        
        /// <summary>
        /// 方块拖拽结束事件
        /// </summary>
        /// <param name="_piece">结束拖拽的方块</param>
        /// <param name="_position">最终位置</param>
        public System.Action<_IGamePiece, Vector3> onPieceDragEnd;
        
        /// <summary>
        /// 方块点击事件
        /// </summary>
        /// <param name="_piece">被点击的方块</param>
        public System.Action<_IGamePiece> onPieceClicked;
        
        // 棋盘事件
        
        /// <summary>
        /// 棋盘初始化完成事件
        /// </summary>
        public static System.Action onBoardInitialized;
        
        /// <summary>
        /// 棋盘清空事件
        /// </summary>
        public static System.Action onBoardCleared;
        
        /// <summary>
        /// 棋盘状态更新事件
        /// </summary>
        /// <param name="_boardState">更新后的棋盘状态</param>
        public System.Action<int[,]> onBoardStateUpdated;
        
        /// <summary>
        /// 方块放置成功事件（静态版本，用于BoardManager）
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_piece">放置的方块</param>
        /// <param name="_position">放置位置</param>
        public static System.Action<int, _IGamePiece, Vector2Int> onPiecePlaced;
        
        /// <summary>
        /// 有效位置高亮事件
        /// </summary>
        /// <param name="_validPositions">有效位置列表</param>
        /// <param name="_playerId">玩家ID</param>
        public System.Action<Vector2Int[], int> onValidPositionsHighlighted;
        
        /// <summary>
        /// 清除高亮事件
        /// </summary>
        public System.Action onHighlightCleared;
        
        // 玩家事件
        
        /// <summary>
        /// 玩家分数更新事件
        /// </summary>
        /// <param name="_playerId">玩家ID</param>
        /// <param name="_newScore">新分数</param>
        public System.Action<int, int> onPlayerScoreUpdated;
        
        /// <summary>
        /// 玩家淘汰事件
        /// </summary>
        /// <param name="_playerId">被淘汰的玩家ID</param>
        public System.Action<int> onPlayerEliminated;
        
        // AI事件
        
        /// <summary>
        /// AI开始思考事件
        /// </summary>
        /// <param name="_aiPlayerId">AI玩家ID</param>
        public System.Action<int> onAIThinkingStarted;
        
        /// <summary>
        /// AI完成思考事件
        /// </summary>
        /// <param name="_aiPlayerId">AI玩家ID</param>
        /// <param name="_selectedPiece">AI选择的方块</param>
        /// <param name="_selectedPosition">AI选择的位置</param>
        public System.Action<int, _IGamePiece, Vector2Int> onAIThinkingCompleted;
        
        // UI事件
        
        /// <summary>
        /// 显示消息事件
        /// </summary>
        /// <param name="_message">要显示的消息</param>
        /// <param name="_messageType">消息类型</param>
        public System.Action<string, MessageType> onShowMessage;
        
        /// <summary>
        /// 更新UI事件
        /// </summary>
        public System.Action onUpdateUI;
        
        // 音效事件
        
        /// <summary>
        /// 播放音效事件
        /// </summary>
        /// <param name="_soundName">音效名称</param>
        public System.Action<string> onPlaySound;
        
        /// <summary>
        /// 播放音乐事件
        /// </summary>
        /// <param name="_musicName">音乐名称</param>
        public System.Action<string> onPlayMusic;
        
        /// <summary>
        /// 停止音乐事件
        /// </summary>
        public System.Action onStopMusic;
        
        /// <summary>
        /// Unity生命周期 - OnDestroy
        /// 清理所有事件订阅
        /// </summary>
        private void OnDestroy()
        {
            if (_s_instance == this)
            {
                // 清理配置系统事件
                onConfigLoaded = null;
                onGameStateChanged = null;
                onGameModeChanged = null;
                
                // 清理游戏状态事件
                onGameStarted = null;
                onGameEnded = null;
                onGamePaused = null;
                onGameResumed = null;
                
                // 清理回合管理事件
                onTurnStarted = null;
                onTurnEnded = null;
                onTurnSkipped = null;
                
                // 清理方块操作事件
                onPieceSelected = null;
                onPiecePlacedInstance = null;
                onPiecePlacementFailed = null;
                onPieceRotated = null;
                onPieceFlipped = null;
                
                // 清理棋盘事件
                onBoardInitialized = null;
                onBoardCleared = null;
                onBoardStateUpdated = null;
                onPiecePlaced = null;
                onValidPositionsHighlighted = null;
                onHighlightCleared = null;
                
                // 清理玩家事件
                onPlayerScoreUpdated = null;
                onPlayerEliminated = null;
                
                // 清理AI事件
                onAIThinkingStarted = null;
                onAIThinkingCompleted = null;
                
                // 清理UI事件
                onShowMessage = null;
                onUpdateUI = null;
                
                // 清理音效事件
                onPlaySound = null;
                onPlayMusic = null;
                onStopMusic = null;
            }
        }
    }
    
    /// <summary>
    /// 消息类型枚举
    /// </summary>
    public enum MessageType
    {
        /// <summary>信息消息</summary>
        Info,
        /// <summary>警告消息</summary>
        Warning,
        /// <summary>错误消息</summary>
        Error,
        /// <summary>成功消息</summary>
        Success
    }
}