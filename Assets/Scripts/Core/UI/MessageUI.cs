using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using BlokusGame.Core.Data;
using BlokusGame.Core.Events;

namespace BlokusGame.Core.UI
{
    /// <summary>
    /// 消息数据结构
    /// </summary>
    [System.Serializable]
    public struct MessageData
    {
        /// <summary>消息内容</summary>
        public string message;
        
        /// <summary>消息类型</summary>
        public MessageType messageType;
        
        /// <summary>显示时长</summary>
        public float duration;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_message">消息内容</param>
        /// <param name="_messageType">消息类型</param>
        /// <param name="_duration">显示时长</param>
        public MessageData(string _message, MessageType _messageType, float _duration)
        {
            message = _message;
            messageType = _messageType;
            duration = _duration;
        }
    }

    /// <summary>
    /// 消息UI - 显示各种提示消息和通知
    /// 支持不同类型的消息显示和自动隐藏功能
    /// </summary>
    public class MessageUI : UIBase
    {
        [Header("消息UI配置")]
        /// <summary>消息文本组件</summary>
        [SerializeField] private Text _m_messageText;
        
        /// <summary>消息图标组件</summary>
        [SerializeField] private Image _m_messageIcon;
        
        /// <summary>消息背景组件</summary>
        [SerializeField] private Image _m_messageBackground;
        
        /// <summary>关闭按钮</summary>
        [SerializeField] private Button _m_closeButton;
        
        [Header("消息类型配置")]
        /// <summary>信息消息颜色</summary>
        [SerializeField] private Color _m_infoColor = Color.blue;
        
        /// <summary>成功消息颜色</summary>
        [SerializeField] private Color _m_successColor = Color.green;
        
        /// <summary>警告消息颜色</summary>
        [SerializeField] private Color _m_warningColor = Color.yellow;
        
        /// <summary>错误消息颜色</summary>
        [SerializeField] private Color _m_errorColor = Color.red;
        
        [Header("消息类型图标")]
        /// <summary>信息消息图标</summary>
        [SerializeField] private Sprite _m_infoIcon;
        
        /// <summary>成功消息图标</summary>
        [SerializeField] private Sprite _m_successIcon;
        
        /// <summary>警告消息图标</summary>
        [SerializeField] private Sprite _m_warningIcon;
        
        /// <summary>错误消息图标</summary>
        [SerializeField] private Sprite _m_errorIcon;
        
        // 私有字段
        /// <summary>当前消息类型</summary>
        private MessageType _m_currentMessageType;
        
        /// <summary>自动隐藏协程</summary>
        private Coroutine _m_autoHideCoroutine;
        
        /// <summary>默认自动隐藏时间</summary>
        private const float DEFAULT_AUTO_HIDE_DURATION = 3f;
        
        /// <summary>消息队列</summary>
        private System.Collections.Generic.Queue<MessageData> _m_messageQueue = new System.Collections.Generic.Queue<MessageData>();
        
        /// <summary>是否正在显示消息</summary>
        private bool _m_isShowingMessage = false;
        
        #region UIBase实现
        
        /// <summary>
        /// 初始化UI特定内容
        /// </summary>
        protected override void InitializeUIContent()
        {
            _setupMessageUI();
            _bindEvents();
        }
        
        /// <summary>
        /// 处理UI显示时的逻辑
        /// </summary>
        protected override void OnUIShown()
        {
            // 消息显示时的逻辑
        }
        
        /// <summary>
        /// 处理UI隐藏时的逻辑
        /// </summary>
        protected override void OnUIHidden()
        {
            _stopAutoHide();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="_message">消息内容</param>
        /// <param name="_messageType">消息类型</param>
        /// <param name="_duration">显示时长（秒），0表示不自动隐藏</param>
        public void ShowMessage(string _message, MessageType _messageType = MessageType.Info, float _duration = DEFAULT_AUTO_HIDE_DURATION)
        {
            if (string.IsNullOrEmpty(_message))
            {
                Debug.LogWarning("[MessageUI] 尝试显示空消息");
                return;
            }
            
            var messageData = new MessageData(_message, _messageType, _duration);
            
            // 如果当前正在显示消息，加入队列
            if (_m_isShowingMessage)
            {
                _m_messageQueue.Enqueue(messageData);
                
                if (_m_enableDetailedLogging)
                {
                    Debug.Log($"[MessageUI] 消息加入队列: {_message} (队列长度: {_m_messageQueue.Count})");
                }
                return;
            }
            
            // 立即显示消息
            _displayMessage(messageData);
        }
        
        /// <summary>
        /// 显示消息（立即显示，跳过队列）
        /// </summary>
        /// <param name="_message">消息内容</param>
        /// <param name="_messageType">消息类型</param>
        /// <param name="_duration">显示时长</param>
        public void ShowMessageImmediate(string _message, MessageType _messageType = MessageType.Info, float _duration = DEFAULT_AUTO_HIDE_DURATION)
        {
            if (string.IsNullOrEmpty(_message))
            {
                Debug.LogWarning("[MessageUI] 尝试显示空消息");
                return;
            }
            
            // 清空队列并立即显示
            _m_messageQueue.Clear();
            _displayMessage(new MessageData(_message, _messageType, _duration));
        }
        
        /// <summary>
        /// 隐藏消息
        /// </summary>
        public void HideMessage()
        {
            _stopAutoHide();
            Hide(true);
            _m_isShowingMessage = false;
            
            // 处理队列中的下一条消息
            _processNextMessage();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[MessageUI] 隐藏消息");
            }
        }
        
        /// <summary>
        /// 清空消息队列
        /// </summary>
        public void ClearMessageQueue()
        {
            _m_messageQueue.Clear();
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[MessageUI] 消息队列已清空");
            }
        }
        
        /// <summary>
        /// 获取队列中的消息数量
        /// </summary>
        /// <returns>队列中的消息数量</returns>
        public int GetQueuedMessageCount()
        {
            return _m_messageQueue.Count;
        }
        
        /// <summary>
        /// 显示信息消息
        /// </summary>
        /// <param name="_message">消息内容</param>
        /// <param name="_duration">显示时长</param>
        public void ShowInfo(string _message, float _duration = DEFAULT_AUTO_HIDE_DURATION)
        {
            ShowMessage(_message, MessageType.Info, _duration);
        }
        
        /// <summary>
        /// 显示成功消息
        /// </summary>
        /// <param name="_message">消息内容</param>
        /// <param name="_duration">显示时长</param>
        public void ShowSuccess(string _message, float _duration = DEFAULT_AUTO_HIDE_DURATION)
        {
            ShowMessage(_message, MessageType.Success, _duration);
        }
        
        /// <summary>
        /// 显示警告消息
        /// </summary>
        /// <param name="_message">消息内容</param>
        /// <param name="_duration">显示时长</param>
        public void ShowWarning(string _message, float _duration = DEFAULT_AUTO_HIDE_DURATION)
        {
            ShowMessage(_message, MessageType.Warning, _duration);
        }
        
        /// <summary>
        /// 显示错误消息
        /// </summary>
        /// <param name="_message">消息内容</param>
        /// <param name="_duration">显示时长</param>
        public void ShowError(string _message, float _duration = DEFAULT_AUTO_HIDE_DURATION)
        {
            ShowMessage(_message, MessageType.Error, _duration);
        }
        
        #endregion
        
        #region 私有方法 - 初始化
        
        /// <summary>
        /// 设置消息UI
        /// </summary>
        private void _setupMessageUI()
        {
            // 初始状态隐藏
            Hide(false);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[MessageUI] 消息UI设置完成");
            }
        }
        
        /// <summary>
        /// 绑定事件
        /// </summary>
        private void _bindEvents()
        {
            if (_m_closeButton != null)
            {
                _m_closeButton.onClick.AddListener(HideMessage);
            }
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log("[MessageUI] 事件绑定完成");
            }
        }
        
        #endregion
        
        #region 私有方法 - 样式设置
        
        /// <summary>
        /// 应用消息样式
        /// </summary>
        /// <param name="_messageType">消息类型</param>
        private void _applyMessageStyle(MessageType _messageType)
        {
            Color messageColor = _getMessageColor(_messageType);
            Sprite messageIcon = _getMessageIcon(_messageType);
            
            // 设置背景颜色
            if (_m_messageBackground != null)
            {
                _m_messageBackground.color = messageColor;
            }
            
            // 设置图标
            if (_m_messageIcon != null && messageIcon != null)
            {
                _m_messageIcon.sprite = messageIcon;
                _m_messageIcon.gameObject.SetActive(true);
            }
            else if (_m_messageIcon != null)
            {
                _m_messageIcon.gameObject.SetActive(false);
            }
            
            // 设置文本颜色（如果需要）
            if (_m_messageText != null)
            {
                // 根据背景颜色调整文本颜色以确保可读性
                _m_messageText.color = _getContrastColor(messageColor);
            }
        }
        
        /// <summary>
        /// 获取消息类型对应的颜色
        /// </summary>
        /// <param name="_messageType">消息类型</param>
        /// <returns>对应颜色</returns>
        private Color _getMessageColor(MessageType _messageType)
        {
            switch (_messageType)
            {
                case MessageType.Info:
                    return _m_infoColor;
                case MessageType.Success:
                    return _m_successColor;
                case MessageType.Warning:
                    return _m_warningColor;
                case MessageType.Error:
                    return _m_errorColor;
                default:
                    return _m_infoColor;
            }
        }
        
        /// <summary>
        /// 获取消息类型对应的图标
        /// </summary>
        /// <param name="_messageType">消息类型</param>
        /// <returns>对应图标</returns>
        private Sprite _getMessageIcon(MessageType _messageType)
        {
            switch (_messageType)
            {
                case MessageType.Info:
                    return _m_infoIcon;
                case MessageType.Success:
                    return _m_successIcon;
                case MessageType.Warning:
                    return _m_warningIcon;
                case MessageType.Error:
                    return _m_errorIcon;
                default:
                    return _m_infoIcon;
            }
        }
        
        /// <summary>
        /// 获取对比色（用于文本颜色）
        /// </summary>
        /// <param name="_backgroundColor">背景颜色</param>
        /// <returns>对比色</returns>
        private Color _getContrastColor(Color _backgroundColor)
        {
            // 计算亮度
            float brightness = (_backgroundColor.r * 0.299f + _backgroundColor.g * 0.587f + _backgroundColor.b * 0.114f);
            
            // 根据亮度选择黑色或白色文本
            return brightness > 0.5f ? Color.black : Color.white;
        }
        
        #endregion
        
        #region 私有方法 - 自动隐藏
        
        /// <summary>
        /// 开始自动隐藏计时
        /// </summary>
        /// <param name="_duration">隐藏延迟时间</param>
        private void _startAutoHide(float _duration)
        {
            _stopAutoHide();
            _m_autoHideCoroutine = StartCoroutine(_autoHideCoroutine(_duration));
        }
        
        /// <summary>
        /// 停止自动隐藏计时
        /// </summary>
        private void _stopAutoHide()
        {
            if (_m_autoHideCoroutine != null)
            {
                StopCoroutine(_m_autoHideCoroutine);
                _m_autoHideCoroutine = null;
            }
        }
        
        /// <summary>
        /// 自动隐藏协程
        /// </summary>
        /// <param name="_duration">等待时间</param>
        /// <returns>协程枚举器</returns>
        private IEnumerator _autoHideCoroutine(float _duration)
        {
            yield return new WaitForSeconds(_duration);
            HideMessage();
            _m_autoHideCoroutine = null;
        }
        
        #endregion
        
        #region 私有方法 - 消息队列处理
        
        /// <summary>
        /// 显示消息（内部方法）
        /// </summary>
        /// <param name="_messageData">消息数据</param>
        private void _displayMessage(MessageData _messageData)
        {
            _m_isShowingMessage = true;
            _m_currentMessageType = _messageData.messageType;
            
            // 设置消息内容
            if (_m_messageText != null)
            {
                _m_messageText.text = _messageData.message;
            }
            
            // 设置消息样式
            _applyMessageStyle(_messageData.messageType);
            
            // 显示UI
            Show(true);
            
            // 设置自动隐藏
            if (_messageData.duration > 0f)
            {
                _startAutoHide(_messageData.duration);
            }
            
            // 播放消息音效
            _playMessageSound(_messageData.messageType);
            
            if (_m_enableDetailedLogging)
            {
                Debug.Log($"[MessageUI] 显示{_messageData.messageType}消息: {_messageData.message}");
            }
        }
        
        /// <summary>
        /// 处理队列中的下一条消息
        /// </summary>
        private void _processNextMessage()
        {
            if (_m_messageQueue.Count > 0)
            {
                var nextMessage = _m_messageQueue.Dequeue();
                
                // 延迟一小段时间再显示下一条消息
                StartCoroutine(_delayedShowMessage(nextMessage, 0.5f));
            }
        }
        
        /// <summary>
        /// 延迟显示消息协程
        /// </summary>
        /// <param name="_messageData">消息数据</param>
        /// <param name="_delay">延迟时间</param>
        /// <returns>协程枚举器</returns>
        private IEnumerator _delayedShowMessage(MessageData _messageData, float _delay)
        {
            yield return new WaitForSeconds(_delay);
            _displayMessage(_messageData);
        }
        
        /// <summary>
        /// 播放消息音效
        /// </summary>
        /// <param name="_messageType">消息类型</param>
        private void _playMessageSound(MessageType _messageType)
        {
            string soundName = "";
            
            switch (_messageType)
            {
                case MessageType.Info:
                    soundName = "MessageInfo";
                    break;
                case MessageType.Success:
                    soundName = "MessageSuccess";
                    break;
                case MessageType.Warning:
                    soundName = "MessageWarning";
                    break;
                case MessageType.Error:
                    soundName = "MessageError";
                    break;
            }
            
            if (!string.IsNullOrEmpty(soundName))
            {
                GameEvents.instance.onPlaySound?.Invoke(soundName);
            }
        }
        
        #endregion
        
        #region 公共方法 - 扩展功能
        
        /// <summary>
        /// 检查是否正在显示消息
        /// </summary>
        /// <returns>是否正在显示消息</returns>
        public bool IsShowingMessage()
        {
            return _m_isShowingMessage;
        }
        
        /// <summary>
        /// 获取当前消息类型
        /// </summary>
        /// <returns>当前消息类型</returns>
        public MessageType GetCurrentMessageType()
        {
            return _m_currentMessageType;
        }
        
        /// <summary>
        /// 获取当前消息内容
        /// </summary>
        /// <returns>当前消息内容</returns>
        public string GetCurrentMessage()
        {
            return _m_messageText?.text ?? "";
        }
        
        /// <summary>
        /// 设置消息颜色配置
        /// </summary>
        /// <param name="_infoColor">信息颜色</param>
        /// <param name="_successColor">成功颜色</param>
        /// <param name="_warningColor">警告颜色</param>
        /// <param name="_errorColor">错误颜色</param>
        public void SetMessageColors(Color _infoColor, Color _successColor, Color _warningColor, Color _errorColor)
        {
            _m_infoColor = _infoColor;
            _m_successColor = _successColor;
            _m_warningColor = _warningColor;
            _m_errorColor = _errorColor;
        }
        
        #endregion
    }
}