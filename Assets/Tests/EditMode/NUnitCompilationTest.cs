using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using BlokusGame.Core.Managers;
using BlokusGame.Core.Data;
using BlokusGame.Core.Rules;
using BlokusGame.Core.Pieces;

namespace BlokusGame.Editor.Tests
{
    /// <summary>
    /// NUnit编译测试 - 验证所有NUnit引用是否正确
    /// </summary>
    public class NUnitCompilationTest
    {
        /// <summary>
        /// 验证NUnit属性是否可以正常使用
        /// </summary>
        [Test]
        public void testNUnitAttributesWork()
        {
            Debug.Log("[NUnitCompilationTest] NUnit属性测试通过");
            Assert.IsTrue(true, "基本断言测试");
        }
        
        /// <summary>
        /// 验证SetUp属性是否可用
        /// </summary>
        [SetUp]
        public void setUp()
        {
            Debug.Log("[NUnitCompilationTest] SetUp方法执行");
        }
        
        /// <summary>
        /// 验证TearDown属性是否可用
        /// </summary>
        [TearDown]
        public void tearDown()
        {
            Debug.Log("[NUnitCompilationTest] TearDown方法执行");
        }
        
        /// <summary>
        /// 验证GameManager类型是否可以正常访问
        /// </summary>
        [Test]
        public void testGameManagerTypeAccess()
        {
            var gameObject = new GameObject("TestGameManager");
            var gameManager = gameObject.AddComponent<GameManager>();
            
            Assert.IsNotNull(gameManager, "GameManager应该可以正常创建");
            
            Object.DestroyImmediate(gameObject);
            Debug.Log("[NUnitCompilationTest] GameManager类型访问测试通过");
        }
        
        /// <summary>
        /// 验证RuleEngine类型是否可以正常访问
        /// </summary>
        [Test]
        public void testRuleEngineTypeAccess()
        {
            var gameObject = new GameObject("TestRuleEngine");
            var ruleEngine = gameObject.AddComponent<RuleEngine>();
            
            Assert.IsNotNull(ruleEngine, "RuleEngine应该可以正常创建");
            
            Object.DestroyImmediate(gameObject);
            Debug.Log("[NUnitCompilationTest] RuleEngine类型访问测试通过");
        }
    }
}