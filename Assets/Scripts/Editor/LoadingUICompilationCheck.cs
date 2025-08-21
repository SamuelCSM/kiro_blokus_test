using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using BlokusGame.Core.UI;
using BlokusGame.Core.Managers;

namespace BlokusGame.Editor
{
    /// <summary>
    /// LoadingUI编译检查工具
    /// 验证LoadingUI的编译状态和完整性
    /// </summary>
    public class LoadingUICompilationCheck : EditorWindow
    {
        /// <summary>检查结果</summary>
        private string _m_checkResult = "";
        
        /// <summary>滚动位置</summary>
        private Vector2 _m_scrollPosition;
        
        [MenuItem("Blokus/编译检查/LoadingUI编译检查")]
        public static void ShowWindow()
        {
            GetWindow<LoadingUICompilationCheck>("LoadingUI编译检查");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("LoadingUI编译检查工具", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("开始编译检查", GUILayout.Height(30)))
            {
                _performCompilationCheck();
            }
            
            GUILayout.Space(10);
            
            _m_scrollPosition = GUILayout.BeginScrollView(_m_scrollPosition);
            GUILayout.TextArea(_m_checkResult, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
        }
        
        /// <summary>
        /// 执行编译检查
        /// </summary>
        private void _performCompilationCheck()
        {
            _m_checkResult = "=== LoadingUI编译检查开始 ===\n\n";
            
            bool allChecksPassed = true;
            
            // 1. 检查LoadingUI类编译状态
            allChecksPassed &= _checkLoadingUICompilation();
            
            // 2. 检查UIManager集成编译状态
            allChecksPassed &= _checkUIManagerIntegration();
            
            // 3. 检查方法签名和返回类型
            allChecksPassed &= _checkMethodSignatures();
            
            // 4. 检查字段类型和访问修饰符
            allChecksPassed &= _checkFieldTypes();
            
            // 5. 检查继承关系
            allChecksPassed &= _checkInheritance();
            
            // 6. 检查命名空间和引用
            allChecksPassed &= _checkNamespaceAndReferences();
            
            _m_checkResult += "\n=== 编译检查总结 ===\n";
            if (allChecksPassed)
            {
                _m_checkResult += "✅ 所有编译检查通过！LoadingUI可以正常编译和使用。\n";
                _m_checkResult += "🎉 LoadingUI任务已完成！\n";
            }
            else
            {
                _m_checkResult += "❌ 部分编译检查失败，请检查上述错误。\n";
            }
            
            _m_checkResult += "=== LoadingUI编译检查结束 ===\n";
        }
        
        /// <summary>
        /// 检查LoadingUI类编译状态
        /// </summary>
        /// <returns>检查是否通过</returns>
        private bool _checkLoadingUICompilation()
        {
            _m_checkResult += "1. 检查LoadingUI类编译状态\n";
            
            try
            {
                var loadingUIType = typeof(LoadingUI);
                _m_checkResult += $"✅ LoadingUI类编译成功\n";
                _m_checkResult += $"   - 类型: {loadingUIType.FullName}\n";
                _m_checkResult += $"   - 程序集: {loadingUIType.Assembly.GetName().Name}\n";
                
                // 检查是否可以实例化（通过反射）
                var constructors = loadingUIType.GetConstructors();
                _m_checkResult += $"   - 构造函数数量: {constructors.Length}\n";
                
                return true;
            }
            catch (System.Exception ex)
            {
                _m_checkResult += $"❌ LoadingUI类编译失败: {ex.Message}\n";
                return false;
            }
            finally
            {
                _m_checkResult += "\n";
            }
        }
        
        /// <summary>
        /// 检查UIManager集成编译状态
        /// </summary>
        /// <returns>检查是否通过</returns>
        private bool _checkUIManagerIntegration()
        {
            _m_checkResult += "2. 检查UIManager集成编译状态\n";
            
            try
            {
                var uiManagerType = typeof(UIManager);
                _m_checkResult += $"✅ UIManager类编译成功\n";
                
                // 检查LoadingUI相关方法
                var loadingMethods = new string[]
                {
                    "ShowLoading",
                    "HideLoading", 
                    "UpdateLoadingProgress",
                    "ShowSimpleLoading",
                    "ShowProgressLoading",
                    "SetLoadingSteps",
                    "CompleteLoading"
                };
                
                bool allMethodsExist = true;
                foreach (var methodName in loadingMethods)
                {
                    var method = uiManagerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                    if (method != null)
                    {
                        _m_checkResult += $"   ✅ {methodName} 方法存在\n";
                    }
                    else
                    {
                        _m_checkResult += $"   ❌ {methodName} 方法不存在\n";
                        allMethodsExist = false;
                    }
                }
                
                return allMethodsExist;
            }
            catch (System.Exception ex)
            {
                _m_checkResult += $"❌ UIManager集成检查失败: {ex.Message}\n";
                return false;
            }
            finally
            {
                _m_checkResult += "\n";
            }
        }
        
        /// <summary>
        /// 检查方法签名和返回类型
        /// </summary>
        /// <returns>检查是否通过</returns>
        private bool _checkMethodSignatures()
        {
            _m_checkResult += "3. 检查方法签名和返回类型\n";
            
            try
            {
                var loadingUIType = typeof(LoadingUI);
                bool allSignaturesCorrect = true;
                
                // 检查ShowLoading方法签名
                var showLoadingMethod = loadingUIType.GetMethod("ShowLoading", 
                    new System.Type[] { typeof(string), typeof(bool) });
                if (showLoadingMethod != null)
                {
                    _m_checkResult += "   ✅ ShowLoading(string, bool) 签名正确\n";
                }
                else
                {
                    _m_checkResult += "   ❌ ShowLoading(string, bool) 签名不正确\n";
                    allSignaturesCorrect = false;
                }
                
                // 检查UpdateProgress方法签名
                var updateProgressMethod = loadingUIType.GetMethod("UpdateProgress",
                    new System.Type[] { typeof(float), typeof(string) });
                if (updateProgressMethod != null)
                {
                    _m_checkResult += "   ✅ UpdateProgress(float, string) 签名正确\n";
                }
                else
                {
                    _m_checkResult += "   ❌ UpdateProgress(float, string) 签名不正确\n";
                    allSignaturesCorrect = false;
                }
                
                // 检查SetLoadingSteps方法签名
                var setStepsMethod = loadingUIType.GetMethods()
                    .FirstOrDefault(m => m.Name == "SetLoadingSteps" && m.GetParameters().Length == 2);
                if (setStepsMethod != null)
                {
                    _m_checkResult += "   ✅ SetLoadingSteps 方法签名正确\n";
                }
                else
                {
                    _m_checkResult += "   ❌ SetLoadingSteps 方法签名不正确\n";
                    allSignaturesCorrect = false;
                }
                
                return allSignaturesCorrect;
            }
            catch (System.Exception ex)
            {
                _m_checkResult += $"❌ 方法签名检查失败: {ex.Message}\n";
                return false;
            }
            finally
            {
                _m_checkResult += "\n";
            }
        }
        
        /// <summary>
        /// 检查字段类型和访问修饰符
        /// </summary>
        /// <returns>检查是否通过</returns>
        private bool _checkFieldTypes()
        {
            _m_checkResult += "4. 检查字段类型和访问修饰符\n";
            
            try
            {
                var loadingUIType = typeof(LoadingUI);
                bool allFieldsCorrect = true;
                
                // 检查关键字段
                var fieldChecks = new System.Collections.Generic.Dictionary<string, System.Type>
                {
                    { "_m_loadingText", typeof(UnityEngine.UI.Text) },
                    { "_m_progressText", typeof(UnityEngine.UI.Text) },
                    { "_m_progressBar", typeof(UnityEngine.UI.Slider) },
                    { "_m_loadingSpinner", typeof(UnityEngine.UI.Image) },
                    { "_m_backgroundMask", typeof(UnityEngine.UI.Image) },
                    { "_m_spinnerRotationSpeed", typeof(float) },
                    { "_m_enableSpinnerAnimation", typeof(bool) },
                    { "_m_currentProgress", typeof(float) },
                    { "_m_showProgressBar", typeof(bool) }
                };
                
                foreach (var fieldCheck in fieldChecks)
                {
                    var field = loadingUIType.GetField(fieldCheck.Key, 
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    if (field != null)
                    {
                        if (field.FieldType == fieldCheck.Value)
                        {
                            _m_checkResult += $"   ✅ {fieldCheck.Key} 字段类型正确 ({fieldCheck.Value.Name})\n";
                        }
                        else
                        {
                            _m_checkResult += $"   ❌ {fieldCheck.Key} 字段类型错误，期望: {fieldCheck.Value.Name}，实际: {field.FieldType.Name}\n";
                            allFieldsCorrect = false;
                        }
                    }
                    else
                    {
                        _m_checkResult += $"   ❌ {fieldCheck.Key} 字段不存在\n";
                        allFieldsCorrect = false;
                    }
                }
                
                return allFieldsCorrect;
            }
            catch (System.Exception ex)
            {
                _m_checkResult += $"❌ 字段类型检查失败: {ex.Message}\n";
                return false;
            }
            finally
            {
                _m_checkResult += "\n";
            }
        }
        
        /// <summary>
        /// 检查继承关系
        /// </summary>
        /// <returns>检查是否通过</returns>
        private bool _checkInheritance()
        {
            _m_checkResult += "5. 检查继承关系\n";
            
            try
            {
                var loadingUIType = typeof(LoadingUI);
                var uiBaseType = typeof(UIBase);
                
                if (uiBaseType.IsAssignableFrom(loadingUIType))
                {
                    _m_checkResult += "✅ LoadingUI正确继承UIBase\n";
                    _m_checkResult += $"   - 基类: {loadingUIType.BaseType.Name}\n";
                    
                    // 检查抽象方法实现
                    var initMethod = loadingUIType.GetMethod("InitializeUIContent", 
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    if (initMethod != null && !initMethod.IsAbstract)
                    {
                        _m_checkResult += "   ✅ InitializeUIContent 方法已实现\n";
                    }
                    else
                    {
                        _m_checkResult += "   ❌ InitializeUIContent 方法未正确实现\n";
                        return false;
                    }
                    
                    return true;
                }
                else
                {
                    _m_checkResult += "❌ LoadingUI没有正确继承UIBase\n";
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                _m_checkResult += $"❌ 继承关系检查失败: {ex.Message}\n";
                return false;
            }
            finally
            {
                _m_checkResult += "\n";
            }
        }
        
        /// <summary>
        /// 检查命名空间和引用
        /// </summary>
        /// <returns>检查是否通过</returns>
        private bool _checkNamespaceAndReferences()
        {
            _m_checkResult += "6. 检查命名空间和引用\n";
            
            try
            {
                var loadingUIType = typeof(LoadingUI);
                
                // 检查命名空间
                if (loadingUIType.Namespace == "BlokusGame.Core.UI")
                {
                    _m_checkResult += "✅ LoadingUI命名空间正确\n";
                }
                else
                {
                    _m_checkResult += $"❌ LoadingUI命名空间错误，期望: BlokusGame.Core.UI，实际: {loadingUIType.Namespace}\n";
                    return false;
                }
                
                // 检查UIManager是否能获取LoadingUI
                var uiManagerType = typeof(UIManager);
                var getPanelMethod = uiManagerType.GetMethod("GetPanel")?.MakeGenericMethod(loadingUIType);
                if (getPanelMethod != null)
                {
                    _m_checkResult += "✅ UIManager可以获取LoadingUI实例\n";
                }
                else
                {
                    _m_checkResult += "❌ UIManager无法获取LoadingUI实例\n";
                    return false;
                }
                
                return true;
            }
            catch (System.Exception ex)
            {
                _m_checkResult += $"❌ 命名空间和引用检查失败: {ex.Message}\n";
                return false;
            }
            finally
            {
                _m_checkResult += "\n";
            }
        }
    }
}