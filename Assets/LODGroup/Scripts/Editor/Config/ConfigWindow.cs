using Chess.LODGroupIJob.Utils;
using UnityEditor;
using UnityEngine;
namespace Chess.LODGroupIJob.Config
{
    public class ConfigWindow : EditorWindow
    {
        [MenuItem("Chess/LODGroupConfig")]
        public static void Init()
        {
            Rect wr = new Rect(0, 0, 200, 80);
            var windows = (ConfigWindow)EditorWindow.GetWindowWithRect(typeof(ConfigWindow), wr, true, "LODGroupConfig");
            windows.Show();
        }
        private void OnGUI()
        {
            var config = SystemConfig.Instance.Config;
            config.asynLoadNum = EditorGUILayout.IntField("同时异步加载数量", config.asynLoadNum);
            config.cullInterval = EditorGUILayout.FloatField("间隔时常计算屏占比", config.cullInterval);
            EditorGUI.BeginChangeCheck();
            config.editorStream = EditorGUILayout.Toggle("编辑器下启动流式加载", config.editorStream);
            if(EditorGUI.EndChangeCheck())
            {
                if (config.editorStream)
                    return;

                //关闭编辑器下流式，将流式加载的资源全部删除
                var lodGroups = GameObject.FindObjectsOfType<LODGroup>();
                if (lodGroups == null)
                    return;
                foreach(var g in lodGroups)
                {
                    foreach(var lod in g.GetLODs())
                    {
                        if(lod.Handle != null && lod.Handle.Result != null)
                            GameObject.DestroyImmediate(lod.Handle.Result);
                    }
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}