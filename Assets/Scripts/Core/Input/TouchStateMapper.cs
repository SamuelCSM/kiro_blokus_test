using BlokusGame.Core.Managers;
using UnityEngine;

namespace BlokusGame.Core.InputSystem
{
    /// <summary>
    /// 触摸状态映射工具类
    /// 负责Unity内置TouchPhase与自定义TouchState之间的转换
    /// 提供统一的触摸状态管理和映射功能
    /// </summary>
    public static class TouchStateMapper
    {
        /// <summary>
        /// 将Unity的TouchPhase转换为自定义的TouchState
        /// 这个方法处理基础的触摸阶段映射
        /// </summary>
        /// <param name="_touchPhase">Unity的触摸阶段</param>
        /// <returns>对应的自定义触摸状态</returns>
        public static TouchInputManager.TouchState mapTouchPhaseToState(TouchPhase _touchPhase)
        {
            switch (_touchPhase)
            {
                case TouchPhase.Began:
                    return TouchInputManager.TouchState.Tap;
                    
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    return TouchInputManager.TouchState.Dragging;
                    
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    return TouchInputManager.TouchState.None;
                    
                default:
                    return TouchInputManager.TouchState.None;
            }
        }
        
        /// <summary>
        /// 根据触摸数据判断具体的手势类型
        /// 考虑触摸时长、移动距离等因素进行高级手势识别
        /// </summary>
        /// <param name="_touchPhase">Unity触摸阶段</param>
        /// <param name="_touchDuration">触摸持续时间</param>
        /// <param name="_moveDistance">移动距离</param>
        /// <param name="_touchCount">触摸点数量</param>
        /// <returns>识别出的手势类型</returns>
        public static TouchInputManager.TouchState detectGestureType(
            TouchPhase _touchPhase, 
            float _touchDuration, 
            float _moveDistance, 
            int _touchCount)
        {
            // 多点触摸处理
            if (_touchCount > 1)
            {
                return TouchInputManager.TouchState.MultiTouch;
            }
            
            // 单点触摸手势识别
            switch (_touchPhase)
            {
                case TouchPhase.Began:
                    return TouchInputManager.TouchState.Tap;
                    
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    // 根据移动距离判断是拖拽还是静止
                    if (_moveDistance > 20f) // 拖拽阈值
                    {
                        return TouchInputManager.TouchState.Dragging;
                    }
                    else if (_touchDuration > 0.8f) // 长按阈值
                    {
                        return TouchInputManager.TouchState.LongPress;
                    }
                    return TouchInputManager.TouchState.Tap;
                    
                case TouchPhase.Ended:
                    // 根据触摸时长和移动距离判断最终手势
                    if (_touchDuration < 0.3f && _moveDistance < 10f)
                    {
                        return TouchInputManager.TouchState.Tap;
                    }
                    else if (_touchDuration > 0.8f && _moveDistance < 20f)
                    {
                        return TouchInputManager.TouchState.LongPress;
                    }
                    else if (_moveDistance > 20f)
                    {
                        return TouchInputManager.TouchState.Dragging;
                    }
                    return TouchInputManager.TouchState.None;
                    
                case TouchPhase.Canceled:
                    return TouchInputManager.TouchState.None;
                    
                default:
                    return TouchInputManager.TouchState.None;
            }
        }
        
        /// <summary>
        /// 检测双击手势
        /// 需要在外部调用时传入上次点击的时间和位置信息
        /// </summary>
        /// <param name="_currentTapTime">当前点击时间</param>
        /// <param name="_lastTapTime">上次点击时间</param>
        /// <param name="_currentPosition">当前点击位置</param>
        /// <param name="_lastPosition">上次点击位置</param>
        /// <param name="_doubleTapInterval">双击间隔阈值</param>
        /// <param name="_doubleTapDistance">双击距离阈值</param>
        /// <returns>是否为双击手势</returns>
        public static bool isDoubleTap(
            float _currentTapTime, 
            float _lastTapTime, 
            Vector2 _currentPosition, 
            Vector2 _lastPosition,
            float _doubleTapInterval = 0.3f,
            float _doubleTapDistance = 50f)
        {
            float timeDelta = _currentTapTime - _lastTapTime;
            float distanceDelta = Vector2.Distance(_currentPosition, _lastPosition);
            
            return timeDelta <= _doubleTapInterval && distanceDelta <= _doubleTapDistance;
        }
        
        /// <summary>
        /// 获取触摸状态的中文描述
        /// 用于调试和日志输出
        /// </summary>
        /// <param name="_touchState">触摸状态</param>
        /// <returns>中文描述</returns>
        public static string getTouchStateDescription(TouchInputManager.TouchState _touchState)
        {
            switch (_touchState)
            {
                case TouchInputManager.TouchState.None:
                    return "无触摸";
                case TouchInputManager.TouchState.Tap:
                    return "单点触摸";
                case TouchInputManager.TouchState.Dragging:
                    return "拖拽中";
                case TouchInputManager.TouchState.LongPress:
                    return "长按";
                case TouchInputManager.TouchState.DoubleTap:
                    return "双击";
                case TouchInputManager.TouchState.MultiTouch:
                    return "多点触摸";
                case TouchInputManager.TouchState.Pinching:
                    return "缩放手势";
                case TouchInputManager.TouchState.Rotation:
                    return "旋转手势";
                case TouchInputManager.TouchState.Pan:
                    return "平移手势";
                case TouchInputManager.TouchState.EdgeSwipe:
                    return "边缘滑动";
                default:
                    return "未知状态";
            }
        }
    }
}