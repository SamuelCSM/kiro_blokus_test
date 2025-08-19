namespace BlokusGame.Core.Data
{
    /// <summary>
    /// 游戏状态枚举
    /// 定义Blokus游戏的各种状态，用于状态机管理
    /// 整合了所有游戏状态，确保与现有代码兼容
    /// </summary>
    public enum GameState
    {
        /// <summary>主菜单状态，游戏未开始</summary>
        MainMenu,
        /// <summary>游戏模式选择状态</summary>
        ModeSelection,
        /// <summary>游戏设置状态</summary>
        Settings,
        /// <summary>游戏初始化中</summary>
        GameInitializing,
        /// <summary>等待玩家加入</summary>
        WaitingForPlayers,
        /// <summary>游戏进行中</summary>
        GamePlaying,
        /// <summary>游戏暂停</summary>
        GamePaused,
        /// <summary>游戏结束</summary>
        GameEnded,
        /// <summary>显示结果</summary>
        ShowingResults,
        /// <summary>加载状态</summary>
        Loading
    }

    /// <summary>
    /// 游戏模式枚举
    /// 定义Blokus游戏支持的不同游戏模式
    /// </summary>
    public enum GameMode
    {
        /// <summary>单人对战AI模式</summary>
        SinglePlayerVsAI,
        /// <summary>本地多人游戏模式</summary>
        LocalMultiplayer,
        /// <summary>在线多人游戏模式</summary>
        OnlineMultiplayer,
        /// <summary>教程模式</summary>
        Tutorial
    }

    /// <summary>
    /// AI难度等级枚举
    /// 定义AI玩家的难度级别
    /// </summary>
    public enum AIDifficulty
    {
        /// <summary>简单难度 - 随机策略</summary>
        Easy,
        /// <summary>中等难度 - 启发式算法</summary>
        Medium,
        /// <summary>困难难度 - 高级算法</summary>
        Hard
    }

    /// <summary>
    /// 玩家游戏状态枚举
    /// 定义单个玩家在游戏中的状态
    /// </summary>
    public enum PlayerGameState
    {
        /// <summary>玩家活跃，正常参与游戏</summary>
        Active,
        /// <summary>玩家跳过当前回合</summary>
        Skipped,
        /// <summary>玩家无法继续游戏（无可放置方块）</summary>
        Finished,
        /// <summary>玩家退出游戏</summary>
        Quit,
        /// <summary>玩家断线</summary>
        Disconnected
    }
}