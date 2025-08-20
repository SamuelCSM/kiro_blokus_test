using UnityEngine;
using BlokusGame.Core.Events;
using BlokusGame.Core.Interfaces;

namespace BlokusGame.Core.Managers
{
    /// <summary>
    /// 输入管理器 - 统一管理游戏输入系统
    /// 协调触摸输入管理器和其他输入组件的工作
    /// 提供输入系统的统一接口和配置管理
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("输入组件引用")]
        /// <summary>触摸输入管理器组件</summary>
        [SerializeField] private TouchInputManager _m_touchInputManager;
        
        [Header("输入配置")]
        /// <summary>是否启用触摸输入</summary>
        [SerializeField] private bool _m_enableTouchInput = true;
        
        /// <summary>是否启用鼠标输入（用于编辑器测试）</summary>
        [SerializeField] private bool _m_enableMouseInput = true;
        
        /// <summary>输入系统是否已初始化</summary>
        private bool _m_isInitialized = false;
        
        #region Unity生命周期
        
        /// <summary>
        /// Unity Awake方法 - 初始化组件引用
        /// </summary>
        private void Awake()
        {
            _initializeComponents();
        }
        
        /// <summary>
        /// Unity Start方法 - 初始化输入管理器
        /// </summary>
        private void Start()
        {
            _initializeInputSystem();
        }
        
        /// <summary>
        /// Unity OnDestroy方法 - 清理资源
        /// </summary>
        private void OnDestroy()
        {
            _cleanup();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 启用或禁用输入系统
        /// </summary>
        /// <param name="_enabled">是否启用输入</param>
        public void setInputEnabled(bool _enabled)
        {
            if (_m_touchInputManager != null)
            {
                _m_touchInputManager.setInputEnabled(_enabled);
            }
            
            Debug.Log($"[InputManager] 输入系统已{(_enabled ? "启用" : "禁用")}");
        }
        
        /// <summary>
        /// 设置触摸输入是否启用
        /// </summary>
        /// <param name="_enabled">是否启用触摸输入</param>
        public void setTouchInputEnabled(bool _enabled)
        {
            _m_enableTouchInput = _enabled;
            
            if (_m_touchInputManager != null)
            {
                _m_touchInputManager.setTouchInputEnabled(_enabled);
            }
        }
        
        /// <summary>
        /// 设置鼠标输入是否启用（主要用于编辑器测试）
        /// </summary>
        /// <param name="_enabled">是否启用鼠标输入</param>
        public void setMouseInputEnabled(bool _enabled)
        {
            _m_enableMouseInput = _enabled;
            
            if (_m_touchInputManager != null)
            {
                _m_touchInputManager.setMouseInputEnabled(_enabled);
            }
        }
        
        /// <summary>
        /// 获取输入系统是否已初始化
        /// </summary>
        /// <returns>是否已初始化</returns>
        public bool isInitialized()
        {
            return _m_isInitialized;
        }
        
        /// <summary>
        /// 重置输入系统
        /// </summary>
        public void resetInputSystem()
        {
            if (_m_touchInputManager != null)
            {
                _m_touchInputManager.resetInput();
            }
            
            Debug.Log("[InputManager] 输入系统已重置");
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 初始化组件引用
        /// </summary>
        private void _initializeComponents()
        {
            // 查找或创建TouchInputManager组件
            if (_m_touchInputManager == null)
            {
                _m_touchInputManager = GetComponent<TouchInputManager>();
                
                if (_m_touchInputManager == null)
                {
                    _m_touchInputManager = gameObject.AddComponent<TouchInputManager>();
                    Debug.Log("[InputManager] 自动创建TouchInputManager组件");
                }
            }
        }
        
        /// <summary>
        /// 初始化输入系统
        /// </summary>
        private void _initializeInputSystem()
        {
            if (_m_isInitialized)
            {
                Debug.LogWarning("[InputManager] 输入系统已经初始化，跳过重复初始化");
                return;
            }
            
            // 配置触摸输入管理器
            if (_m_touchInputManager != null)
            {
                _m_touchInputManager.setTouchInputEnabled(_m_enableTouchInput);
                _m_touchInputManager.setMouseInputEnabled(_m_enableMouseInput);
            }
            
            _m_isInitialized = true;
            Debug.Log("[InputManager] 输入管理器初始化完成");
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        private void _cleanup()
        {
            if (_m_touchInputManager != null)
            {
                _m_touchInputManager.resetInput();
            }
            
            _m_isInitialized = false;
            Debug.Log("[InputManager] 输入管理器资源已清理");
        }
        
        #endregion
    }
}