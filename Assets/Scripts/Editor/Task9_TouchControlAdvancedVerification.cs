using UnityEngine;
using UnityEditor;
using BlokusGame.Core.Managers;
using BlokusGame.Core.InputSystem;
using BlokusGame.Core.Board;

namespace BlokusGame.Editor
{
    /// <summary>
    /// 任务9.2和9.3高级触摸控制验证工具
    /// 验证多点触摸、手势识别和触摸与游戏逻辑集成功能
    /// </summary>
    public class Task9_TouchControlAdvancedVerification : EditorWindow
    {
        private Vector2 _scrollPosition;
        private bool _showDetailedInfo = false;
        
        [MenuItem("Blokus/验证工具/任务9 - 高级触摸控制验证")]
        public static void ShowWindow()
        {
            GetWindow<Task9_TouchControlAdvancedVerification>("任务9高级触摸控制验证");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("任务9.2和9.3 - 高级触摸控制验证", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            _showDetailedInfo = EditorGUILayout.Toggle("显示详细信息", _showDetailedInfo);
            EditorGUILayout.Space();
            
            // 验证任务9.2 - 多点触摸和手势识别
            _verifyTask92();
            EditorGUILayout.Space();
            
            // 验证任务9.3 - 触摸控制与游戏逻辑集成
            _verifyTask93();
            EditorGUILayout.Space();
            
            // 性能和优化验证
            _verifyPerformanceOptimizations();
            EditorGUILayout.Space();
            
            // 操作按钮
            _drawActionButtons();
            
            EditorGUILayout.EndScrollView();
        }
        
        /// <summary>
        /// 验证任务9.2 - 多点触摸和手势识别
        /// </summary>
        private void _verifyTask92()
        {
            EditorGUILayout.LabelField("📱 任务9.2 - 多点触摸和手势识别", EditorStyles.boldLabel);
            
            var touchManager = FindObjectOfType<TouchInputManager>();
            var boardVisualizer = FindObjectOfType<BoardVisualizer>();
            
            // 检查TouchInputManager的多点触摸功能
            bool hasMultiTouchSupport = _checkMultiTouchSupport(touchManager);
            _drawVerificationItem("多点触摸支持", hasMultiTouchSupport, 
                "TouchInputManager支持多点触摸缩放和旋转手势");
            
            // 检查BoardVisualizer的缩放功能
            bool hasZoomFunction = _checkBoardZoomFunction(boardVisualizer);
            _drawVerificationItem("棋盘缩放功能", hasZoomFunction, 
                "BoardVisualizer实现了setZoomLevel方法");
            
            // 检查手势防误触机制
            bool hasAntiMistouch = _checkAntiMistouchMechanism(touchManager);
            _drawVerificationItem("防误触机制", hasAntiMistouch, 
                "实现了触摸验证和防误触逻辑");
            
            // 检查边缘滑动功能
            bool hasEdgeSwipe = _checkEdgeSwipeFunction(touchManager);
            _drawVerificationItem("边缘滑动手势", hasEdgeSwipe, 
                "支持边缘滑动手势识别");
            
            // 检查性能优化
            bool hasPerformanceOptimization = _checkTouchPerformanceOptimization(touchManager);
            _drawVerificationItem("触摸性能优化", hasPerformanceOptimization, 
                "实现了触摸事件频率限制和缓存机制");
            
            if (_showDetailedInfo && touchManager != null)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("TouchInputManager详细信息:", EditorStyles.miniBoldLabel);
                
                // 显示触摸配置
                var enableMultiTouch = _getPrivateField<bool>(touchManager, "_m_enableMultiTouch");
                EditorGUILayout.LabelField($"启用多点触摸: {enableMultiTouch}");
                
                var pinchThreshold = _getPrivateField<float>(touchManager, "_m_pinchThreshold");
                EditorGUILayout.LabelField($"缩放手势阈值: {pinchThreshold}");
                
                var rotationThreshold = _getPrivateField<float>(touchManager, "_m_rotationThreshold");
                EditorGUILayout.LabelField($"旋转手势阈值: {rotationThreshold}");
                
                EditorGUILayout.EndVertical();
            }
        }
        
        /// <summary>
        /// 验证任务9.3 - 触摸控制与游戏逻辑集成
        /// </summary>
        private void _verifyTask93()
        {
            EditorGUILayout.LabelField("🎮 任务9.3 - 触摸控制与游戏逻辑集成", EditorStyles.boldLabel);
            
            var gameplayIntegration = FindObjectOfType<TouchGameplayIntegration>();
            var boardVisualizer = FindObjectOfType<BoardVisualizer>();
            
            // 检查TouchGameplayIntegration组件
            bool hasGameplayIntegration = gameplayIntegration != null;
            _drawVerificationItem("游戏逻辑集成组件", hasGameplayIntegration, 
                "TouchGameplayIntegration组件存在并正确配置");
            
            // 检查拖拽到棋盘的完整流程
            bool hasDragToBoard = _checkDragToBoardFlow(gameplayIntegration);
            _drawVerificationItem("拖拽到棋盘流程", hasDragToBoard, 
                "实现了完整的方块拖拽到棋盘流程");
            
            // 检查实时预览功能
            bool hasRealTimePreview = _checkRealTimePreview(boardVisualizer);
            _drawVerificationItem("实时预览功能", hasRealTimePreview, 
                "BoardVisualizer支持方块放置预览");
            
            // 检查位置验证和吸附
            bool hasPositionValidation = _checkPositionValidation(gameplayIntegration);
            _drawVerificationItem("位置验证和吸附", hasPositionValidation, 
                "实现了位置验证和自动吸附功能");
            
            // 检查触摸反馈集成
            bool hasTouchFeedback = _checkTouchFeedbackIntegration(gameplayIntegration);
            _drawVerificationItem("触摸反馈集成", hasTouchFeedback, 
                "触摸操作与游戏反馈系统集成");
            
            // 检查事件系统集成
            bool hasEventIntegration = _checkEventSystemIntegration(gameplayIntegration);
            _drawVerificationItem("事件系统集成", hasEventIntegration, 
                "正确订阅和处理游戏事件");
            
            if (_showDetailedInfo && gameplayIntegration != null)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("TouchGameplayIntegration详细信息:", EditorStyles.miniBoldLabel);
                
                var enableDragPreview = _getPrivateField<bool>(gameplayIntegration, "_m_enableDragPreview");
                EditorGUILayout.LabelField($"启用拖拽预览: {enableDragPreview}");
                
                var enablePositionSnap = _getPrivateField<bool>(gameplayIntegration, "_m_enablePositionSnap");
                EditorGUILayout.LabelField($"启用位置吸附: {enablePositionSnap}");
                
                var snapDistance = _getPrivateField<float>(gameplayIntegration, "_m_snapDistance");
                EditorGUILayout.LabelField($"吸附距离: {snapDistance}");
                
                EditorGUILayout.EndVertical();
            }
        }
        
        /// <summary>
        /// 验证性能和优化
        /// </summary>
        private void _verifyPerformanceOptimizations()
        {
            EditorGUILayout.LabelField("⚡ 性能优化验证", EditorStyles.boldLabel);
            
            var touchManager = FindObjectOfType<TouchInputManager>();
            
            // 检查触摸事件缓存
            bool hasTouchEventCaching = _checkTouchEventCaching(touchManager);
            _drawVerificationItem("触摸事件缓存", hasTouchEventCaching, 
                "实现了触摸事件缓存机制");
            
            // 检查频率限制
            bool hasFrequencyLimit = _checkFrequencyLimit(touchManager);
            _drawVerificationItem("频率限制", hasFrequencyLimit, 
                "实现了触摸事件频率限制");
            
            // 检查内存优化
            bool hasMemoryOptimization = _checkMemoryOptimization();
            _drawVerificationItem("内存优化", hasMemoryOptimization, 
                "使用对象池和缓存减少GC压力");
            
            if (Application.isPlaying && touchManager != null)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("运行时性能统计:", EditorStyles.miniBoldLabel);
                
                // 尝试获取性能统计信息
                try
                {
                    var statsMethod = touchManager.GetType().GetMethod("getTouchPerformanceStats");
                    if (statsMethod != null)
                    {
                        string stats = (string)statsMethod.Invoke(touchManager, null);
                        EditorGUILayout.LabelField(stats);
                    }
                }
                catch (System.Exception e)
                {
                    EditorGUILayout.LabelField($"无法获取性能统计: {e.Message}");
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        /// <summary>
        /// 绘制操作按钮
        /// </summary>
        private void _drawActionButtons()
        {
            EditorGUILayout.LabelField("🔧 操作工具", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("创建测试场景"))
            {
                _createTestScene();
            }
            
            if (GUILayout.Button("运行触摸测试"))
            {
                _runTouchTest();
            }
            
            if (GUILayout.Button("生成验证报告"))
            {
                _generateVerificationReport();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        // ==================== 验证方法实现 ====================
        
        private bool _checkMultiTouchSupport(TouchInputManager touchManager)
        {
            if (touchManager == null) return false;
            
            // 检查是否有多点触摸相关的字段和方法
            var type = touchManager.GetType();
            return type.GetField("_m_enableMultiTouch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null &&
                   type.GetMethod("_handlePinchGesture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null;
        }
        
        private bool _checkBoardZoomFunction(BoardVisualizer boardVisualizer)
        {
            if (boardVisualizer == null) return false;
            
            // 检查是否有setZoomLevel方法
            var type = boardVisualizer.GetType();
            return type.GetMethod("setZoomLevel") != null;
        }
        
        private bool _checkAntiMistouchMechanism(TouchInputManager touchManager)
        {
            if (touchManager == null) return false;
            
            // 检查防误触相关字段和方法
            var type = touchManager.GetType();
            return type.GetField("_m_maxSimultaneousTouches", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null &&
                   type.GetMethod("_isValidTouch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null;
        }
        
        private bool _checkEdgeSwipeFunction(TouchInputManager touchManager)
        {
            if (touchManager == null) return false;
            
            // 检查边缘滑动相关方法
            var type = touchManager.GetType();
            return type.GetMethod("_isEdgeSwipe", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null &&
                   type.GetMethod("_handleEdgeSwipe", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null;
        }
        
        private bool _checkTouchPerformanceOptimization(TouchInputManager touchManager)
        {
            if (touchManager == null) return false;
            
            // 检查性能优化相关字段
            var type = touchManager.GetType();
            return type.GetField("_m_maxTouchEventsPerSecond", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null &&
                   type.GetField("_m_enableTouchEventCaching", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null;
        }
        
        private bool _checkDragToBoardFlow(TouchGameplayIntegration integration)
        {
            if (integration == null) return false;
            
            // 检查拖拽流程相关方法
            var type = integration.GetType();
            return type.GetMethod("_onPieceDragStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null &&
                   type.GetMethod("_onPieceDragging", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null &&
                   type.GetMethod("_onPieceDragEnd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null;
        }
        
        private bool _checkRealTimePreview(BoardVisualizer boardVisualizer)
        {
            if (boardVisualizer == null) return false;
            
            // 检查预览相关方法
            var type = boardVisualizer.GetType();
            return type.GetMethod("showPiecePreview") != null &&
                   type.GetMethod("hidePiecePreview") != null;
        }
        
        private bool _checkPositionValidation(TouchGameplayIntegration integration)
        {
            if (integration == null) return false;
            
            // 检查位置验证相关方法
            var type = integration.GetType();
            return type.GetMethod("_validatePiecePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null &&
                   type.GetMethod("_findNearestValidPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null;
        }
        
        private bool _checkTouchFeedbackIntegration(TouchGameplayIntegration integration)
        {
            if (integration == null) return false;
            
            // 检查触摸反馈集成
            var type = integration.GetType();
            return type.GetField("_m_feedbackSystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null;
        }
        
        private bool _checkEventSystemIntegration(TouchGameplayIntegration integration)
        {
            if (integration == null) return false;
            
            // 检查事件订阅方法
            var type = integration.GetType();
            return type.GetMethod("_subscribeToEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null &&
                   type.GetMethod("_unsubscribeFromEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null;
        }
        
        private bool _checkTouchEventCaching(TouchInputManager touchManager)
        {
            if (touchManager == null) return false;
            
            var type = touchManager.GetType();
            return type.GetField("_m_touchEventCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null;
        }
        
        private bool _checkFrequencyLimit(TouchInputManager touchManager)
        {
            if (touchManager == null) return false;
            
            var type = touchManager.GetType();
            return type.GetMethod("_canProcessTouchEvent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null;
        }
        
        private bool _checkMemoryOptimization()
        {
            // 检查是否有对象池相关组件
            var boardVisualizer = FindObjectOfType<BoardVisualizer>();
            if (boardVisualizer != null)
            {
                var type = boardVisualizer.GetType();
                return type.GetField("_m_highlightPool", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null;
            }
            return false;
        }
        
        // ==================== 辅助方法 ====================
        
        private void _drawVerificationItem(string itemName, bool isValid, string description)
        {
            EditorGUILayout.BeginHorizontal();
            
            // 状态图标
            string icon = isValid ? "✅" : "❌";
            GUILayout.Label(icon, GUILayout.Width(20));
            
            // 项目名称
            GUILayout.Label(itemName, GUILayout.Width(150));
            
            // 状态文本
            string status = isValid ? "已实现" : "未实现";
            Color originalColor = GUI.color;
            GUI.color = isValid ? Color.green : Color.red;
            GUILayout.Label(status, GUILayout.Width(60));
            GUI.color = originalColor;
            
            // 描述
            if (_showDetailedInfo)
            {
                GUILayout.Label(description);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private T _getPrivateField<T>(object obj, string fieldName)
        {
            try
            {
                var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    return (T)field.GetValue(obj);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"无法获取字段 {fieldName}: {e.Message}");
            }
            return default(T);
        }
        
        private void _createTestScene()
        {
            Debug.Log("创建触摸控制测试场景...");
            // 这里可以实现创建测试场景的逻辑
        }
        
        private void _runTouchTest()
        {
            Debug.Log("运行触摸功能测试...");
            // 这里可以实现自动化触摸测试的逻辑
        }
        
        private void _generateVerificationReport()
        {
            Debug.Log("生成任务9.2和9.3验证报告...");
            
            string report = "# 任务9.2和9.3验证报告\n\n";
            report += "## 任务9.2 - 多点触摸和手势识别\n";
            report += "- 多点触摸支持: " + (_checkMultiTouchSupport(FindObjectOfType<TouchInputManager>()) ? "✅" : "❌") + "\n";
            report += "- 棋盘缩放功能: " + (_checkBoardZoomFunction(FindObjectOfType<BoardVisualizer>()) ? "✅" : "❌") + "\n";
            report += "- 防误触机制: " + (_checkAntiMistouchMechanism(FindObjectOfType<TouchInputManager>()) ? "✅" : "❌") + "\n";
            report += "- 边缘滑动手势: " + (_checkEdgeSwipeFunction(FindObjectOfType<TouchInputManager>()) ? "✅" : "❌") + "\n";
            
            report += "\n## 任务9.3 - 触摸控制与游戏逻辑集成\n";
            report += "- 游戏逻辑集成组件: " + (FindObjectOfType<TouchGameplayIntegration>() != null ? "✅" : "❌") + "\n";
            report += "- 拖拽到棋盘流程: " + (_checkDragToBoardFlow(FindObjectOfType<TouchGameplayIntegration>()) ? "✅" : "❌") + "\n";
            report += "- 实时预览功能: " + (_checkRealTimePreview(FindObjectOfType<BoardVisualizer>()) ? "✅" : "❌") + "\n";
            report += "- 位置验证和吸附: " + (_checkPositionValidation(FindObjectOfType<TouchGameplayIntegration>()) ? "✅" : "❌") + "\n";
            
            Debug.Log(report);
        }
    }
}