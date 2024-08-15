using System;
using System.Linq;
using Moonflow.MFAssetTools.MFRefLink;
using UnityEditor;
using UnityEngine;

namespace Tools.Editor.MFAssetTools.MFRefLink.Editor
{
    public class MFTypeRefSearcher : EditorWindow
    {
        //create window
        [MenuItem("Tools/Moonflow/Tools/Type Ref Searcher")]
        public static void ShowWindow()
        {
            var _ins = GetWindow<MFTypeRefSearcher>(MFToolsLang.isCN?"类型引用搜索":"Type Ref Searcher");
            _ins.minSize = new UnityEngine.Vector2(400, 30);
            _ins.maxSize = new UnityEngine.Vector2(400, 30);
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // 使用反射获得MFRefLinkData的所有子类
                var types = typeof(MFRefLinkData).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(MFRefLinkData))).ToArray();
                //获得types内所有类的“assetType”重载方法的返回结果
                var typeNames = types.Select(t => t.GetMethod("assetType").Invoke(null, null) as string).ToArray();

                var type = EditorGUILayout.Popup(MFToolsLang.isCN?"类型":"Type", 0, typeNames);
                if (GUILayout.Button(MFToolsLang.isCN?"搜索":"Search"))
                {
                    var objs = MFRefLinkCache.LoadCache().GetRefLinkDataByType(typeNames[type]);
                    if (objs != null && objs.Count > 0)
                    {
                        EditorGUIUtility.PingObject(objs[0]);
                    }
                }
            }
        }
    }
}