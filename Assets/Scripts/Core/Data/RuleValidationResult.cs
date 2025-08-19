using UnityEngine;

namespace BlokusGame.Core.Data
{
    /// <summary>
    /// 规则验证结果数据类
    /// 包含方块放置验证的详细结果信息
    /// 提供验证失败的具体原因，便于调试和用户反馈
    /// </summary>
    [System.Serializable]
    public class RuleValidationResult
    {
        /// <summary>验证是否通过</summary>
        public bool isValid { get; private set; }
        
        /// <summary>验证失败的原因描述</summary>
        public string failureReason { get; private set; }
        
        /// <summary>失败的规则类型</summary>
        public RuleType violatedRule { get; private set; }
        
        /// <summary>相关的位置信息（如冲突位置）</summary>
        public Vector2Int[] conflictPositions { get; private set; }
        
        /// <summary>
        /// 创建验证成功的结果
        /// </summary>
        /// <returns>表示验证通过的结果对象</returns>
        public static RuleValidationResult createSuccess()
        {
            return new RuleValidationResult
            {
                isValid = true,
                failureReason = string.Empty,
                violatedRule = RuleType.None,
                conflictPositions = new Vector2Int[0]
            };
        }
        
        /// <summary>
        /// 创建验证失败的结果
        /// </summary>
        /// <param name="_reason">失败原因描述</param>
        /// <param name="_ruleType">违反的规则类型</param>
        /// <param name="_conflictPositions">冲突位置数组</param>
        /// <returns>表示验证失败的结果对象</returns>
        public static RuleValidationResult createFailure(string _reason, RuleType _ruleType, Vector2Int[] _conflictPositions = null)
        {
            return new RuleValidationResult
            {
                isValid = false,
                failureReason = _reason,
                violatedRule = _ruleType,
                conflictPositions = _conflictPositions ?? new Vector2Int[0]
            };
        }
        
        /// <summary>
        /// 获取用户友好的错误信息
        /// 根据规则类型返回本地化的错误描述
        /// </summary>
        /// <returns>用户可理解的错误信息</returns>
        public string getUserFriendlyMessage()
        {
            if (isValid)
                return "放置合法";
                
            switch (violatedRule)
            {
                case RuleType.FirstPlacementCorner:
                    return "首次放置必须占据角落位置";
                case RuleType.CornerContact:
                    return "方块必须与你已有方块的角相接触";
                case RuleType.EdgeContact:
                    return "方块不能与你已有方块的边相接触";
                case RuleType.Overlap:
                    return "方块不能放置在已被占据的位置";
                case RuleType.OutOfBounds:
                    return "方块不能超出棋盘边界";
                default:
                    return failureReason;
            }
        }
    }
    
    /// <summary>
    /// 规则类型枚举
    /// 定义Blokus游戏中的各种规则类型
    /// </summary>
    public enum RuleType
    {
        /// <summary>无规则违反</summary>
        None,
        /// <summary>首次放置角落规则</summary>
        FirstPlacementCorner,
        /// <summary>角接触规则</summary>
        CornerContact,
        /// <summary>边不相接规则</summary>
        EdgeContact,
        /// <summary>重叠规则</summary>
        Overlap,
        /// <summary>边界规则</summary>
        OutOfBounds
    }
}