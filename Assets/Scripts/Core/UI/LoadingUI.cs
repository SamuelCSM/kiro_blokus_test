using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BlokusGame.Core.UI
{
    /// <summary>
    /// 加载UI - 显示加载进度和状态
    /// 支持进度条显示和加载文本更新
    /// </summary>
    public class LoadingUI : UIBase
    {
        [Header("加载UI配置")]
        /// <summary>加载文本组件</summary>
        [SerializeField] private Text _m_loadingText;
        
        /// <summary>进度文本组件</summary>
        [SerializeField] private Text _m_progressText;
        
        /// <summary>进度条组件</summary>
        [SerializeField] private Slider _m_progressBar;
        
        /// <summary>加载动画组件</summary>
        [SerializeField] private Image _m_loadingSpinner;
        
        /// <summary>背景遮罩组件</summary>
        [SerializeField] private Image _m_backgroundMask;
        
        [Header("动画配置")]
        /// <summary>旋转动画速度</summary>
        [SerializeField] private float _m_spinnerRotationSpeed = 360f;
        
        /// <summary>是否启用旋转动画</summary>
        [SerializeField] private bool _m_enableSpinnerAnimation = true;
        
        // 私有字段
        /// <summary>当前进度值 (0-1)</summary>
        private float _m_currentProgress = 0f;
        
        /// <summary>是否显示进度条</summary>
        private bool _m_showProgressBar = false;
        
        /// <summary>旋转动画协程</summary>
        private Coroutine _m_spinnerCoroutine;
        
        /// <summary>加载步骤列表</summary>
        private System.Collections.Generic.List<string> _m_loadingSteps = new System.Collections.Generic.List<string>();
        
        /// <summary>当前步骤索引</summary>
        private int _m_currentStepIndex = 0;
        
        /// <summary>步骤切换协程</summary>
        private Coroutine _m_stepSwitchCoroutine;
        
        #region UIBase实现
        
        /// <summary>
        /// 初始化UI特定内容
        /// </summary>
        protected override void InitializeUIContent()
        {
            _setupLoadingUI();
        }
        
        /// <summary>
        /// 处理UI显示时的逻辑
        /// </summary>
        protected override void OnUIShown()
        {
            _startSpinnerAnimation();
        }
        
        /// <summary>
        /// 处理UI隐藏时的逻辑
        /// </summary>
        protected override void OnUIHidden()
        {
            _stopSpinnerAnimation();
            _stopStepSwitching();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 显示加载界面
        /// </summary>
        /// <param name="_loadingText">加载文本</param>
        /// <param name="_showProgress">是否显示进度条</param>
        public void ShowLoading(string _loadingText = "加载中...", bool _showProgress = false)
        {
            _m_showProgressBar = _showProgress;
            
            // 设置加载文本
            if (_m_loadingText != null)
            {
                _m_loadingText.text = _loadingText;
            }
            
            // 设置进度条显示状态
            if (_m_progressBar != null)
            {
                _m_progressBar.gameObject.SetActive(_showProgress);
                if (_showProgress)
                {
                    _m_progressBar.value = 0f;
                }
            }
            
            // 设置进度文本显示状态
            if (_m_progressText != null)
            {
                _m_progressText.gameObject.SetActive(_showProgress);
                if (_showProgress)
                {
                    _m_progressText.text = "0%";
                }
            }
            
            // 重置进度
            _m_currentProgress = 0f;
            
            // 显示UI
            Show(true);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[LoadingUI] 显示加载界面: {_loadingText}, 显示进度: {_showProgress}");
            }
        }
        
        /// <summary>
        /// 隐藏加载界面
        /// </summary>
        public void HideLoading()
        {
            Hide(true);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[LoadingUI] 隐藏加载界面");
            }
        }
        
        /// <summary>
        /// 更新加载进度
        /// </summary>
        /// <param name="_progress">进度值 (0-1)</param>
        /// <param name="_progressText">进度文本（可选）</param>
        public void UpdateProgress(float _progress, string _progressText = "")
        {
            _m_currentProgress = Mathf.Clamp01(_progress);
            
            // 更新进度条
            if (_m_progressBar != null && _m_showProgressBar)
            {
                _m_progressBar.value = _m_currentProgress;
            }
            
            // 更新进度文本
            if (_m_progressText != null && _m_showProgressBar)
            {
                if (!string.IsNullOrEmpty(_progressText))
                {
                    _m_progressText.text = _progressText;
                }
                else
                {
                    _m_progressText.text = $"{Mathf.RoundToInt(_m_currentProgress * 100)}%";
                }
            }
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[LoadingUI] 更新进度: {_m_currentProgress:F2} ({_progressText})");
            }
        }
        
        /// <summary>
        /// 设置加载文本
        /// </summary>
        /// <param name="_text">加载文本</param>
        public void SetLoadingText(string _text)
        {
            if (_m_loadingText != null)
            {
                _m_loadingText.text = _text;
            }
        }
        
        /// <summary>
        /// 设置背景遮罩透明度
        /// </summary>
        /// <param name="_alpha">透明度 (0-1)</param>
        public void SetBackgroundAlpha(float _alpha)
        {
            if (_m_backgroundMask != null)
            {
                var color = _m_backgroundMask.color;
                color.a = Mathf.Clamp01(_alpha);
                _m_backgroundMask.color = color;
            }
        }
        
        #endregion
        
        #region 私有方法 - 初始化
        
        /// <summary>
        /// 设置加载UI
        /// </summary>
        private void _setupLoadingUI()
        {
            // 设置进度条初始状态
            if (_m_progressBar != null)
            {
                _m_progressBar.minValue = 0f;
                _m_progressBar.maxValue = 1f;
                _m_progressBar.value = 0f;
                _m_progressBar.gameObject.SetActive(false);
            }
            
            // 设置进度文本初始状态
            if (_m_progressText != null)
            {
                _m_progressText.gameObject.SetActive(false);
            }
            
            // 设置默认加载文本
            if (_m_loadingText != null)
            {
                _m_loadingText.text = "加载中...";
            }
            
            // 设置背景遮罩
            if (_m_backgroundMask != null)
            {
                SetBackgroundAlpha(0.8f); // 半透明背景
            }
            
            // 初始状态隐藏
            Hide(false);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[LoadingUI] 加载UI设置完成");
            }
        }
        
        #endregion
        
        #region 私有方法 - 动画控制
        
        /// <summary>
        /// 开始旋转动画
        /// </summary>
        private void _startSpinnerAnimation()
        {
            if (_m_loadingSpinner != null && _m_enableSpinnerAnimation)
            {
                _stopSpinnerAnimation();
                _m_spinnerCoroutine = StartCoroutine(_spinnerAnimationCoroutine());
            }
        }
        
        /// <summary>
        /// 停止旋转动画
        /// </summary>
        private void _stopSpinnerAnimation()
        {
            if (_m_spinnerCoroutine != null)
            {
                StopCoroutine(_m_spinnerCoroutine);
                _m_spinnerCoroutine = null;
            }
        }
        
        /// <summary>
        /// 旋转动画协程
        /// </summary>
        /// <returns>协程枚举器</returns>
        private IEnumerator _spinnerAnimationCoroutine()
        {
            while (_m_loadingSpinner != null)
            {
                float rotationAmount = _m_spinnerRotationSpeed * Time.unscaledDeltaTime;
                _m_loadingSpinner.transform.Rotate(0f, 0f, -rotationAmount);
                yield return null;
            }
        }
        
        #endregion
        
        #region 公共方法 - 便利方法
        
        /// <summary>
        /// 显示简单加载（无进度条）
        /// </summary>
        /// <param name="_text">加载文本</param>
        public void ShowSimpleLoading(string _text = "加载中...")
        {
            ShowLoading(_text, false);
        }
        
        /// <summary>
        /// 显示进度加载（带进度条）
        /// </summary>
        /// <param name="_text">加载文本</param>
        public void ShowProgressLoading(string _text = "加载中...")
        {
            ShowLoading(_text, true);
        }
        
        /// <summary>
        /// 设置进度百分比
        /// </summary>
        /// <param name="_percentage">百分比 (0-100)</param>
        public void SetProgressPercentage(int _percentage)
        {
            float progress = Mathf.Clamp(_percentage, 0, 100) / 100f;
            UpdateProgress(progress);
        }
        
        /// <summary>
        /// 增加进度
        /// </summary>
        /// <param name="_increment">增加的进度值</param>
        public void IncrementProgress(float _increment)
        {
            UpdateProgress(_m_currentProgress + _increment);
        }
        
        /// <summary>
        /// 完成加载（进度设为100%然后隐藏）
        /// </summary>
        /// <param name="_delay">完成后延迟隐藏时间</param>
        public void CompleteLoading(float _delay = 0.5f)
        {
            UpdateProgress(1f, "完成");
            StartCoroutine(_completeLoadingCoroutine(_delay));
        }
        
        /// <summary>
        /// 完成加载协程
        /// </summary>
        /// <param name="_delay">延迟时间</param>
        /// <returns>协程枚举器</returns>
        private IEnumerator _completeLoadingCoroutine(float _delay)
        {
            yield return new WaitForSeconds(_delay);
            HideLoading();
        }
        
        /// <summary>
        /// 设置加载步骤
        /// </summary>
        /// <param name="_steps">加载步骤列表</param>
        /// <param name="_stepDuration">每个步骤的显示时长</param>
        public void SetLoadingSteps(System.Collections.Generic.List<string> _steps, float _stepDuration = 2f)
        {
            _m_loadingSteps = new System.Collections.Generic.List<string>(_steps);
            _m_currentStepIndex = 0;
            
            if (_m_loadingSteps.Count > 0)
            {
                _startStepSwitching(_stepDuration);
            }
        }
        
        /// <summary>
        /// 设置加载步骤（数组版本）
        /// </summary>
        /// <param name="_steps">加载步骤数组</param>
        /// <param name="_stepDuration">每个步骤的显示时长</param>
        public void SetLoadingSteps(string[] _steps, float _stepDuration = 2f)
        {
            SetLoadingSteps(new System.Collections.Generic.List<string>(_steps), _stepDuration);
        }
        
        /// <summary>
        /// 跳转到下一个加载步骤
        /// </summary>
        public void NextLoadingStep()
        {
            if (_m_loadingSteps.Count > 0 && _m_currentStepIndex < _m_loadingSteps.Count - 1)
            {
                _m_currentStepIndex++;
                SetLoadingText(_m_loadingSteps[_m_currentStepIndex]);
                
                // 更新进度（基于步骤）
                if (_m_showProgressBar)
                {
                    float stepProgress = (float)(_m_currentStepIndex + 1) / _m_loadingSteps.Count;
                    UpdateProgress(stepProgress);
                }
            }
        }
        
        /// <summary>
        /// 跳转到指定的加载步骤
        /// </summary>
        /// <param name="_stepIndex">步骤索引</param>
        public void GoToLoadingStep(int _stepIndex)
        {
            if (_m_loadingSteps.Count > 0 && _stepIndex >= 0 && _stepIndex < _m_loadingSteps.Count)
            {
                _m_currentStepIndex = _stepIndex;
                SetLoadingText(_m_loadingSteps[_m_currentStepIndex]);
                
                // 更新进度（基于步骤）
                if (_m_showProgressBar)
                {
                    float stepProgress = (float)(_m_currentStepIndex + 1) / _m_loadingSteps.Count;
                    UpdateProgress(stepProgress);
                }
            }
        }
        
        /// <summary>
        /// 开始步骤自动切换
        /// </summary>
        /// <param name="_stepDuration">每个步骤的显示时长</param>
        private void _startStepSwitching(float _stepDuration)
        {
            _stopStepSwitching();
            _m_stepSwitchCoroutine = StartCoroutine(_stepSwitchCoroutine(_stepDuration));
        }
        
        /// <summary>
        /// 停止步骤自动切换
        /// </summary>
        private void _stopStepSwitching()
        {
            if (_m_stepSwitchCoroutine != null)
            {
                StopCoroutine(_m_stepSwitchCoroutine);
                _m_stepSwitchCoroutine = null;
            }
        }
        
        /// <summary>
        /// 步骤切换协程
        /// </summary>
        /// <param name="_stepDuration">每个步骤的显示时长</param>
        /// <returns>协程枚举器</returns>
        private IEnumerator _stepSwitchCoroutine(float _stepDuration)
        {
            while (_m_currentStepIndex < _m_loadingSteps.Count)
            {
                SetLoadingText(_m_loadingSteps[_m_currentStepIndex]);
                
                // 更新进度（基于步骤）
                if (_m_showProgressBar)
                {
                    float stepProgress = (float)(_m_currentStepIndex + 1) / _m_loadingSteps.Count;
                    UpdateProgress(stepProgress);
                }
                
                yield return new WaitForSeconds(_stepDuration);
                _m_currentStepIndex++;
            }
            
            _m_stepSwitchCoroutine = null;
        }
        
        /// <summary>
        /// 获取当前步骤信息
        /// </summary>
        /// <returns>当前步骤信息</returns>
        public string GetCurrentStepInfo()
        {
            if (_m_loadingSteps.Count > 0 && _m_currentStepIndex < _m_loadingSteps.Count)
            {
                return $"步骤 {_m_currentStepIndex + 1}/{_m_loadingSteps.Count}: {_m_loadingSteps[_m_currentStepIndex]}";
            }
            return "";
        }
        
        /// <summary>
        /// 清除加载步骤
        /// </summary>
        public void ClearLoadingSteps()
        {
            _stopStepSwitching();
            _m_loadingSteps.Clear();
            _m_currentStepIndex = 0;
        }
        
        #endregion
    }
}