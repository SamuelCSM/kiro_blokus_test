namespace BlokusGame.Core.Data
{
    /// <summary>
    /// 游戏状态枚举
    /// 定义游戏的各种状态，用于状态机管理
    /// </summary>
    public enum GameState
    {
        /// <summary>主菜单状态</summary>
        MainMenu,
        /// <summary>游戏模式选择状态</summary>
        ModeSelection,
        /// <summary>游戏设置状态</summary>
        Settings,
        /// <summary>游戏初始化状态</summary>
        GameInitializing,
        /// <summary>游戏进行中状态</summary>
        GamePlaying,
        /// <summary>游戏暂停状态</summary>
        GamePaused,
        /// <summary>游戏结束状态</summary>
        GameEnded,
        /// <summary>加载状态</summary>
        Loading
    }

    /// <summary>
    /// 游戏模式枚举
    /// 定义支持的游戏模式类型
    /// </summary>
    public enum GameMode
    {
        /// <summary>单人对战AI模式</summary>
        SinglePlayerVsAI,
        /// <summary>本地多人模式</summary>
        LocalMultiplayer,
        /// <summary>在线多人模式</summary>
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
}