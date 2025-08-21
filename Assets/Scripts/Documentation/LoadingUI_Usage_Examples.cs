using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BlokusGame.Core.UI;
using BlokusGame.Core.Managers;

namespace BlokusGame.Documentation
{
    /// <summary>
    /// LoadingUI使用示例
    /// 展示LoadingUI的各种使用方法和最佳实践
    /// </summary>
    public class LoadingUI_Usage_Examples : MonoBehaviour
    {
        [Header("示例配置")]
        /// <summary>是否在启动时运行示例</summary>
        [SerializeField] private bool _m_runExamplesOnStart = false;
        
        /// <summary>示例间隔时间</summary>
        [SerializeField] private float _m_exampleInterval = 3f;
        
        /// <summary>LoadingUI引用</summary>
        private LoadingUI _m_loadingUI;
        
        /// <summary>UIManager引用</summary>
        private UIManager _m_uiManager;
        
        #region Unity生命周期
        
        private void Start()
        {
            _initializeReferences();
            
            if (_m_runExamplesOnStart)
            {
                StartCoroutine(_runAllExamples());
            }
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化引用
        /// </summary>
        private void _initializeReferences()
        {
            _m_loadingUI = FindObjectOfType<LoadingUI>();
            _m_uiManager = UIManager.instance;
            
            if (_m_loadingUI == null)
            {
                Debug.LogWarning("[LoadingUI示例] 未找到LoadingUI组件");
            }
            
            if (_m_uiManager == null)
            {
                Debug.LogWarning("[LoadingUI示例] 未找到UIManager实例");
            }
        }
        
        #endregion
        
        #region 示例运行器
        
        /// <summary>
        /// 运行所有示例
        /// </summary>
        /// <returns>协程枚举器</returns>
        private IEnumerator _runAllExamples()
        {
            Debug.Log("=== LoadingUI使用示例开始 ===");
            
            yield return StartCoroutine(_example1_BasicLoading());
            yield return new WaitForSeconds(_m_exampleInterval);
            
            yield return StartCoroutine(_example2_ProgressLoading());
            yield return new WaitForSeconds(_m_exampleInterval);
            
            yield return StartCoroutine(_example3_StepLoading());
            yield return new WaitForSeconds(_m_exampleInterval);
            
            yield return StartCoroutine(_example4_UIManagerIntegration());
            yield return new WaitForSeconds(_m_exampleInterval);
            
            yield return StartCoroutine(_example5_CustomizationExample());
            
            Debug.Log("=== LoadingUI使用示例结束 ===");
        }
        
        #endregion
        
        #region 示例1：基础加载
        
        /// <summary>
        /// 示例1：基础加载界面使用
        /// </summary>
        /// <returns>协程枚举器</returns>
        private IEnumerator _example1_BasicLoading()
        {
            Debug.Log("--- 示例1：基础加载界面 ---");
            
            if (_m_loadingUI == null) yield break;
            
            // 显示简单加载界面
            _m_loadingUI.ShowSimpleLoading("正在初始化游戏...");
            Debug.Log("显示简单加载界面");
            
            yield return new WaitForSeconds(2f);
            
            // 更新加载文本
            _m_loadingUI.SetLoadingText("正在加载资源...");
            Debug.Log("更新加载文本");
            
            yield return new WaitForSeconds(2f);
            
            // 隐藏加载界面
            _m_loadingUI.HideLoading();
            Debug.Log("隐藏加载界面");
        }
        
        #endregion
        
        #region 示例2：进度加载
        
        /// <summary>
        /// 示例2：带进度条的加载界面
        /// </summary>
        /// <returns>协程枚举器</returns>
        private IEnumerator _example2_ProgressLoading()
        {
            Debug.Log("--- 示例2：进度加载界面 ---");
            
            if (_m_loadingUI == null) yield break;
            
            // 显示进度加载界面
            _m_loadingUI.ShowProgressLoading("正在加载游戏数据...");
            Debug.Log("显示进度加载界面");
            
            // 模拟加载进度
            for (int i = 0; i <= 100; i += 10)
            {
                float progress = i / 100f;
                _m_loadingUI.UpdateProgress(progress, $"加载中... {i}%");
                Debug.Log($"更新进度: {i}%");
                yield return new WaitForSeconds(0.3f);
            }
            
            // 完成加载
            _m_loadingUI.CompleteLoading(1f);
            Debug.Log("完成加载");
        }
        
        #endregion
        
        #region 示例3：步骤加载
        
        /// <summary>
        /// 示例3：分步骤加载界面
        /// </summary>
        /// <returns>协程枚举器</returns>
        private IEnumerator _example3_StepLoading()
        {
            Debug.Log("--- 示例3：步骤加载界面 ---");
            
            if (_m_loadingUI == null) yield break;
            
            // 定义加载步骤
            var loadingSteps = new List<string>
            {
                "初始化游戏引擎...",
                "加载游戏资源...",
                "初始化UI系统...",
                "加载游戏数据...",
                "准备游戏场景..."
            };
            
            // 显示进度加载并设置步骤
            _m_loadingUI.ShowProgressLoading("正在启动游戏...");
            _m_loadingUI.SetLoadingSteps(loadingSteps, 1.5f);
            Debug.Log("开始步骤加载");
            
            // 等待所有步骤完成
            yield return new WaitForSeconds(loadingSteps.Count * 1.5f + 1f);
            
            // 手动完成加载
            _m_loadingUI.CompleteLoading();
            Debug.Log("步骤加载完成");
        }
        
        #endregion
        
        #region 示例4：UIManager集成
        
        /// <summary>
        /// 示例4：通过UIManager使用LoadingUI
        /// </summary>
        /// <returns>协程枚举器</returns>
        private IEnumerator _example4_UIManagerIntegration()
        {
            Debug.Log("--- 示例4：UIManager集成 ---");
            
            if (_m_uiManager == null) yield break;
            
            // 通过UIManager显示简单加载
            _m_uiManager.ShowSimpleLoading("通过UIManager加载...");
            Debug.Log("UIManager显示简单加载");
            
            yield return new WaitForSeconds(2f);
            
            // 切换到进度加载
            _m_uiManager.ShowProgressLoading("切换到进度加载...");
            Debug.Log("切换到进度加载");
            
            // 更新进度
            for (int i = 0; i <= 100; i += 20)
            {
                float progress = i / 100f;
                _m_uiManager.UpdateLoadingProgress(progress, $"进度: {i}%");
                Debug.Log($"UIManager更新进度: {i}%");
                yield return new WaitForSeconds(0.5f);
            }
            
            // 完成加载
            _m_uiManager.CompleteLoading();
            Debug.Log("UIManager完成加载");
        }
        
        #endregion
        
        #region 示例5：自定义样式
        
        /// <summary>
        /// 示例5：自定义加载界面样式
        /// </summary>
        /// <returns>协程枚举器</returns>
        private IEnumerator _example5_CustomizationExample()
        {
            Debug.Log("--- 示例5：自定义样式 ---");
            
            if (_m_loadingUI == null) yield break;
            
            // 显示加载界面
            _m_loadingUI.ShowProgressLoading("自定义样式加载...");
            
            // 设置半透明背景
            _m_loadingUI.SetBackgroundAlpha(0.5f);
            Debug.Log("设置背景透明度");
            
            yield return new WaitForSeconds(1f);
            
            // 模拟加载过程中的文本变化
            string[] loadingTexts = {
                "正在连接服务器...",
                "正在验证用户信息...",
                "正在下载更新...",
                "正在应用更新...",
                "准备完成..."
            };
            
            for (int i = 0; i < loadingTexts.Length; i++)
            {
                _m_loadingUI.SetLoadingText(loadingTexts[i]);
                float progress = (i + 1) / (float)loadingTexts.Length;
                _m_loadingUI.UpdateProgress(progress);
                Debug.Log($"更新文本和进度: {loadingTexts[i]} ({progress:P0})");
                yield return new WaitForSeconds(1f);
            }
            
            // 恢复背景透明度并完成
            _m_loadingUI.SetBackgroundAlpha(0.8f);
            _m_loadingUI.CompleteLoading();
            Debug.Log("自定义样式示例完成");
        }
        
        #endregion
        
        #region 公共方法 - 手动触发示例
        
        /// <summary>
        /// 手动运行基础加载示例
        /// </summary>
        [ContextMenu("运行基础加载示例")]
        public void RunBasicLoadingExample()
        {
            StartCoroutine(_example1_BasicLoading());
        }
        
        /// <summary>
        /// 手动运行进度加载示例
        /// </summary>
        [ContextMenu("运行进度加载示例")]
        public void RunProgressLoadingExample()
        {
            StartCoroutine(_example2_ProgressLoading());
        }
        
        /// <summary>
        /// 手动运行步骤加载示例
        /// </summary>
        [ContextMenu("运行步骤加载示例")]
        public void RunStepLoadingExample()
        {
            StartCoroutine(_example3_StepLoading());
        }
        
        /// <summary>
        /// 手动运行UIManager集成示例
        /// </summary>
        [ContextMenu("运行UIManager集成示例")]
        public void RunUIManagerIntegrationExample()
        {
            StartCoroutine(_example4_UIManagerIntegration());
        }
        
        /// <summary>
        /// 手动运行自定义样式示例
        /// </summary>
        [ContextMenu("运行自定义样式示例")]
        public void RunCustomizationExample()
        {
            StartCoroutine(_example5_CustomizationExample());
        }
        
        /// <summary>
        /// 运行所有示例
        /// </summary>
        [ContextMenu("运行所有示例")]
        public void RunAllExamples()
        {
            StartCoroutine(_runAllExamples());
        }
        
        #endregion
        
        #region 实用工具方法
        
        /// <summary>
        /// 模拟游戏启动加载
        /// </summary>
        [ContextMenu("模拟游戏启动加载")]
        public void SimulateGameStartupLoading()
        {
            StartCoroutine(_simulateGameStartup());
        }
        
        /// <summary>
        /// 模拟游戏启动加载协程
        /// </summary>
        /// <returns>协程枚举器</returns>
        private IEnumerator _simulateGameStartup()
        {
            if (_m_uiManager == null) yield break;
            
            // 定义启动步骤
            var startupSteps = new List<string>
            {
                "初始化游戏引擎...",
                "加载核心资源...",
                "初始化音频系统...",
                "加载UI资源...",
                "初始化网络连接...",
                "加载游戏数据...",
                "准备游戏场景...",
                "启动完成！"
            };
            
            _m_uiManager.ShowProgressLoading("正在启动Blokus游戏...");
            _m_uiManager.SetLoadingSteps(startupSteps, 1f);
            
            Debug.Log("开始模拟游戏启动加载");
            
            // 等待加载完成
            yield return new WaitForSeconds(startupSteps.Count * 1f + 1f);
            
            Debug.Log("游戏启动加载完成");
        }
        
        /// <summary>
        /// 模拟关卡加载
        /// </summary>
        /// <param name="_levelName">关卡名称</param>
        public void SimulateLevelLoading(string _levelName = "关卡1")
        {
            StartCoroutine(_simulateLevelLoading(_levelName));
        }
        
        /// <summary>
        /// 模拟关卡加载协程
        /// </summary>
        /// <param name="_levelName">关卡名称</param>
        /// <returns>协程枚举器</returns>
        private IEnumerator _simulateLevelLoading(string _levelName)
        {
            if (_m_uiManager == null) yield break;
            
            _m_uiManager.ShowProgressLoading($"正在加载{_levelName}...");
            
            // 模拟加载过程
            string[] loadingStages = {
                "加载地图数据...",
                "初始化游戏对象...",
                "加载AI配置...",
                "准备游戏界面...",
                "完成加载"
            };
            
            for (int i = 0; i < loadingStages.Length; i++)
            {
                float progress = (i + 1) / (float)loadingStages.Length;
                _m_uiManager.UpdateLoadingProgress(progress, loadingStages[i]);
                Debug.Log($"关卡加载: {loadingStages[i]} ({progress:P0})");
                yield return new WaitForSeconds(0.8f);
            }
            
            _m_uiManager.CompleteLoading();
            Debug.Log($"{_levelName}加载完成");
        }
        
        #endregion
    }
}