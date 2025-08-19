using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using BlokusGame.Core.Data;
using BlokusGame.Core.Interfaces;
using BlokusGame.Core.Player;

namespace BlokusGame.Editor
{
    /// <summary>
    /// AIDifficulty引用验证编辑器脚本
    /// 验证所有AIDifficulty枚举引用都正确使用
    /// 确保没有错误的_IAIPlayer.AIDifficulty引用
    /// </summary>
    public class AIDifficultyReferenceVerification
    {
        /// <summary>
        /// 验证AIDifficulty枚举定义
        /// 确保枚举在正确的命名空间中定义
        /// </summary>
        [Test]
        public void testAIDifficultyEnumDefinition()
        {
            // 验证AIDifficulty枚举存在于正确的命名空间
            var enumType = typeof(AIDifficulty);
            Assert.IsNotNull(enumType, "AIDifficulty枚举应该存在");
            Assert.AreEqual("BlokusGame.Core.Data", enumType.Namespace, "AIDifficulty应该在BlokusGame.Core.Data命名空间中");
            
            // 验证枚举值
            var enumValues = System.Enum.GetValues(typeof(AIDifficulty));
            Assert.AreEqual(3, enumValues.Length, "AIDifficulty应该有3个值");
            
            // 验证具体的枚举值
            Assert.IsTrue(System.Enum.IsDefined(typeof(AIDifficulty), AIDifficulty.Easy), "Easy难度应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(AIDifficulty), AIDifficulty.Medium), "Medium难度应该存在");
            Assert.IsTrue(System.Enum.IsDefined(typeof(AIDifficulty), AIDifficulty.Hard), "Hard难度应该存在");
            
            Debug.Log("[AIDifficultyReferenceVerification] AIDifficulty枚举定义验证通过");
        }
        
        /// <summary>
        /// 验证_IAIPlayer接口正确引用AIDifficulty
        /// 确保接口中使用的是正确的枚举类型
        /// </summary>
        [Test]
        public void testIAIPlayerInterfaceReference()
        {
            var interfaceType = typeof(_IAIPlayer);
            Assert.IsNotNull(interfaceType, "_IAIPlayer接口应该存在");
            
            // 获取difficulty属性
            var difficultyProperty = interfaceType.GetProperty("difficulty");
            Assert.IsNotNull(difficultyProperty, "difficulty属性应该存在");
            Assert.AreEqual(typeof(AIDifficulty), difficultyProperty.PropertyType, "difficulty属性类型应该是AIDifficulty");
            
            // 获取setDifficulty方法
            var setDifficultyMethod = interfaceType.GetMethod("setDifficulty");
            Assert.IsNotNull(setDifficultyMethod, "setDifficulty方法应该存在");
            
            var parameters = setDifficultyMethod.GetParameters();
            Assert.AreEqual(1, parameters.Length, "setDifficulty方法应该有1个参数");
            Assert.AreEqual(typeof(AIDifficulty), parameters[0].ParameterType, "setDifficulty参数类型应该是AIDifficulty");
            
            Debug.Log("[AIDifficultyReferenceVerification] _IAIPlayer接口引用验证通过");
        }
        
        /// <summary>
        /// 验证AIPlayer类正确实现AIDifficulty相关功能
        /// 确保实现类正确使用枚举类型
        /// </summary>
        [Test]
        public void testAIPlayerImplementation()
        {
            // 创建测试AI玩家
            GameObject testObj = new GameObject("TestAIPlayer");
            AIPlayer aiPlayer = testObj.AddComponent<AIPlayer>();
            
            try
            {
                // 验证默认难度
                Assert.AreEqual(AIDifficulty.Medium, aiPlayer.difficulty, "默认AI难度应该为Medium");
                
                // 测试设置不同难度
                aiPlayer.setDifficulty(AIDifficulty.Easy);
                Assert.AreEqual(AIDifficulty.Easy, aiPlayer.difficulty, "AI难度应该正确设置为Easy");
                
                aiPlayer.setDifficulty(AIDifficulty.Hard);
                Assert.AreEqual(AIDifficulty.Hard, aiPlayer.difficulty, "AI难度应该正确设置为Hard");
                
                aiPlayer.setDifficulty(AIDifficulty.Medium);
                Assert.AreEqual(AIDifficulty.Medium, aiPlayer.difficulty, "AI难度应该正确设置为Medium");
                
                // 验证接口转换
                _IAIPlayer aiInterface = aiPlayer as _IAIPlayer;
                Assert.IsNotNull(aiInterface, "AIPlayer应该可以转换为_IAIPlayer接口");
                Assert.AreEqual(AIDifficulty.Medium, aiInterface.difficulty, "接口访问的难度应该正确");
                
                Debug.Log("[AIDifficultyReferenceVerification] AIPlayer实现验证通过");
            }
            finally
            {
                // 清理测试对象
                Object.DestroyImmediate(testObj);
            }
        }
        
        /// <summary>
        /// 验证枚举值的正确性
        /// 确保枚举值符合预期的设计
        /// </summary>
        [Test]
        public void testEnumValueCorrectness()
        {
            // 验证枚举值的数值
            Assert.AreEqual(0, (int)AIDifficulty.Easy, "Easy难度的数值应该为0");
            Assert.AreEqual(1, (int)AIDifficulty.Medium, "Medium难度的数值应该为1");
            Assert.AreEqual(2, (int)AIDifficulty.Hard, "Hard难度的数值应该为2");
            
            // 验证枚举值的字符串表示
            Assert.AreEqual("Easy", AIDifficulty.Easy.ToString(), "Easy难度的字符串表示应该正确");
            Assert.AreEqual("Medium", AIDifficulty.Medium.ToString(), "Medium难度的字符串表示应该正确");
            Assert.AreEqual("Hard", AIDifficulty.Hard.ToString(), "Hard难度的字符串表示应该正确");
            
            // 验证枚举解析
            Assert.AreEqual(AIDifficulty.Easy, System.Enum.Parse(typeof(AIDifficulty), "Easy"), "Easy难度应该可以正确解析");
            Assert.AreEqual(AIDifficulty.Medium, System.Enum.Parse(typeof(AIDifficulty), "Medium"), "Medium难度应该可以正确解析");
            Assert.AreEqual(AIDifficulty.Hard, System.Enum.Parse(typeof(AIDifficulty), "Hard"), "Hard难度应该可以正确解析");
            
            Debug.Log("[AIDifficultyReferenceVerification] 枚举值正确性验证通过");
        }
        
        /// <summary>
        /// 验证编译时类型安全
        /// 确保所有类型引用都是编译时安全的
        /// </summary>
        [Test]
        public void testCompileTimeTypeSafety()
        {
            // 这个测试主要验证编译时的类型安全性
            // 如果有错误的类型引用，编译时就会失败
            
            // 验证可以正确创建和使用枚举值
            AIDifficulty testDifficulty = AIDifficulty.Medium;
            Assert.AreEqual(AIDifficulty.Medium, testDifficulty, "枚举值应该可以正确赋值和比较");
            
            // 验证可以在switch语句中使用
            string difficultyName = testDifficulty switch
            {
                AIDifficulty.Easy => "简单",
                AIDifficulty.Medium => "中等",
                AIDifficulty.Hard => "困难",
                _ => "未知"
            };
            
            Assert.AreEqual("中等", difficultyName, "switch表达式应该正确工作");
            
            // 验证可以在方法参数中使用
            Assert.DoesNotThrow(() => {
                _testDifficultyParameter(AIDifficulty.Easy);
                _testDifficultyParameter(AIDifficulty.Medium);
                _testDifficultyParameter(AIDifficulty.Hard);
            }, "枚举应该可以作为方法参数正确传递");
            
            Debug.Log("[AIDifficultyReferenceVerification] 编译时类型安全验证通过");
        }
        
        /// <summary>
        /// 测试用的方法，验证枚举参数传递
        /// </summary>
        /// <param name="_difficulty">AI难度参数</param>
        private void _testDifficultyParameter(AIDifficulty _difficulty)
        {
            // 验证参数可以正确接收和使用
            Assert.IsTrue(System.Enum.IsDefined(typeof(AIDifficulty), _difficulty), "传入的难度值应该是有效的");
        }
        
        /// <summary>
        /// 运行所有验证测试的菜单项
        /// </summary>
        [MenuItem("Blokus/验证/AIDifficulty引用验证")]
        public static void runAIDifficultyVerification()
        {
            Debug.Log("=== 开始AIDifficulty引用验证 ===");
            
            var verification = new AIDifficultyReferenceVerification();
            
            try
            {
                verification.testAIDifficultyEnumDefinition();
                verification.testIAIPlayerInterfaceReference();
                verification.testAIPlayerImplementation();
                verification.testEnumValueCorrectness();
                verification.testCompileTimeTypeSafety();
                
                Debug.Log("=== AIDifficulty引用验证全部通过 ===");
                EditorUtility.DisplayDialog("验证成功", "所有AIDifficulty引用验证都通过了！", "确定");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"AIDifficulty引用验证失败: {ex.Message}");
                EditorUtility.DisplayDialog("验证失败", $"AIDifficulty引用验证失败:\n{ex.Message}", "确定");
            }
        }
    }
}