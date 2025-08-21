using UnityEngine;
using UnityEditor;

namespace BlokusGame.Editor
{
    /// <summary>
    /// UI编译检查工具
    /// </summary>
    public class UICompilationCheck : EditorWindow
    {
        [MenuItem("Blokus/编译检查/UI系统编译检查")]
        public static void ShowWindow()
        {
            GetWindow<UICompilationCheck>("UI编译检查");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("UI系统编译检查", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("检查编译状态", GUILayout.Height(30)))
            {
                CheckCompilation();
            }
        }
        
        private void CheckCompilation()
        {
            Debug.Log("=== UI系统编译检查开始 ===");
            
            // 检查关键类型是否存在
            CheckType("BlokusGame.Core.UI.SettingsUI");
            CheckType("BlokusGame.Core.UI.PauseMenuUI");
            CheckType("BlokusGame.Core.UI.GameOverUI");
            CheckType("BlokusGame.Core.UI.MessageUI");
            CheckType("BlokusGame.Core.UI.LoadingUI");
            CheckType("BlokusGame.Core.UI.MessageData");
            
            Debug.Log("=== UI系统编译检查完成 ===");
        }
        
        private void CheckType(string typeName)
        {
            var type = System.Type.GetType(typeName + ", Assembly-CSharp");
            if (type != null)
            {
                Debug.Log($"✅ {typeName} 编译成功");
            }
            else
            {
                Debug.LogError($"❌ {typeName} 编译失败或未找到");
            }
        }
    }
}