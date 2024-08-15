using System;
using UnityEditor;
using UnityEngine;

namespace Tools.Editor.MFAssetTools.MFRefLink.Editor
{
    public class MFGuidSearcher : EditorWindow
    {
        private string _guid = "";
        //create window
        [MenuItem("Tools/Moonflow/Tools/Guid Searcher")]
        public static void ShowWindow()
        {
            var _ins = GetWindow<MFGuidSearcher>(MFToolsLang.isCN?"GUID搜索":"Guid Searcher");
            _ins.minSize = new Vector2(400, 30);
            _ins.maxSize = new Vector2(400, 30);
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _guid = EditorGUILayout.TextField("Guid", _guid);
                if (GUILayout.Button(MFToolsLang.isCN?"搜索":"Search"))
                {
                    var path = AssetDatabase.GUIDToAssetPath(_guid);
                    if (path != null)
                    {
                        var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                        EditorGUIUtility.PingObject(obj);
                    }
                }
            }
        }
    }
}