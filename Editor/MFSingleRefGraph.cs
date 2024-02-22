using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Moonflow;
using Moonflow.MFAssetTools.MFRefLink;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tools.Editor.MFAssetTools.MFRefLink.Editor
{
    public class MFSingleRefGraph : EditorWindow
    {
        public MFRefLinkData center;
        public Object[] refs;
        public Object[] deps;

        private List<string> errorGUID;
        private Object obj;
        private int pageL;
        private int pageR;
        
        //menu item of right click on project view
        [MenuItem("Assets/Show Ref Graph")]
        public static void ShowRefGraph()
        {
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject));
            var data = MFRefLinkCache.LoadCache().GetRefLinkDataByGUID(guid);
            if (data != null)
            {
                Open(data);
            }
        }
        
        public static void Open(MFRefLinkData data)
        {
            var window = GetWindow<MFSingleRefGraph>();
            window.Init(data);
        }
        
        public void Init(MFRefLinkData data)
        {
            center = data;
            LoadAssets();
        }
        
        //load refs and deps assets by MFRefLinkData
        private void LoadAssets()
        {
            //load object of center by guid to obj
            obj = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(center.guid));
            errorGUID = new List<string>();
            int i = 0;
            if (center.refGUID != null)
            {
                refs = new Object[center.refGUID.Count];
                foreach (var guid in center.refGUID)
                {
                    refs[i] = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid));
                    if(refs[i] == null) errorGUID.Add(guid);
                    i++;
                }
            }

            if (center.depGUID != null)
            {
                deps = new Object[center.depGUID.Count];
                i = 0;
                foreach (var guid in center.depGUID)
                {
                    deps[i] = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid));
                    if(deps[i] == null) errorGUID.Add(guid);
                    i++;
                }
            }
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                //draw three vertical list, left is deps, middle is center, right is refs
                using (new EditorGUILayout.HorizontalScope(GUILayout.Width(900)))
                {
                    EditorGUILayout.LabelField("Deps", EditorStyles.boldLabel, GUILayout.Width(297));
                    EditorGUILayout.LabelField("Obj", EditorStyles.boldLabel, GUILayout.Width(297));
                    EditorGUILayout.LabelField("Refs", EditorStyles.boldLabel, GUILayout.Width(297));
                }

                MFEditorUI.DivideLine(Color.white);

                int depCount = deps?.Length ?? 1;
                int refCount = refs?.Length ?? 1;
                using (new EditorGUILayout.HorizontalScope(GUILayout.Width(891),
                           GUILayout.Height(Mathf.Max(depCount, refCount) * 20 + 20)))
                {
                    float normalLabelWidth = EditorGUIUtility.labelWidth;
                    //Left
                    using (new EditorGUILayout.VerticalScope("box", GUILayout.Width(297)))
                    {
                        EditorGUIUtility.labelWidth = 20;
                        if (deps is { Length: > 0 })
                        {
                            MFEditorUI.DrawFlipList<Object>(DrawAsset, deps.ToList(), ref pageL, 20);
                        }
                        else
                        {
                            EditorGUILayout.LabelField("No Deps");
                        }
                        EditorGUIUtility.labelWidth = normalLabelWidth;
                    }

                    //Mid
                    using (new EditorGUILayout.VerticalScope("box", GUILayout.Width(297)))
                    {
                        // GUILayout.Label(center.path);
                        EditorGUILayout.ObjectField(obj, typeof(object), true);
                        using (new EditorGUILayout.HorizontalScope("box", GUILayout.Width(297)))
                        {
                            EditorGUIUtility.labelWidth = 50;
                            EditorGUILayout.LabelField("GUID", center.guid, GUILayout.Width(247));
                            if (GUILayout.Button("Copy", GUILayout.Width(50)))
                            {
                                EditorGUIUtility.systemCopyBuffer = center.guid;
                            }
                            EditorGUIUtility.labelWidth = normalLabelWidth;
                        }

                        float cap = center.capacity;
                        var suffix = CapacityUnitConversion(ref cap);
                        EditorGUILayout.LabelField("Capacity: ", $"{cap} {suffix}");
                        using (new EditorGUILayout.HorizontalScope(GUILayout.Width(297)))
                        {
                            EditorGUIUtility.labelWidth = 50;
                            EditorGUILayout.LabelField("Estimate Ref Capacity: ");
                            if (center.totalCapacity == 0)
                            {
                                if (GUILayout.Button("Calculate"))
                                {
                                    center.totalCapacity = TotalCapacitySum(center);
                                }
                            }
                            else
                            {
                                var trc = center.totalCapacity;
                                var trcSuffix = CapacityUnitConversion(ref trc);
                                EditorGUILayout.LabelField($"{trc} {trcSuffix}");
                                if (GUILayout.Button("Re-Calculate"))
                                {
                                    center.totalCapacity = TotalCapacitySum(center);
                                }
                            }
                            EditorGUIUtility.labelWidth = normalLabelWidth;
                        }
                        

                        EditorGUILayout.LabelField("ErrorGUID", EditorStyles.boldLabel, GUILayout.Width(297));
                        MFEditorUI.DivideLine(Color.gray);
                        if (errorGUID is { Count: > 0 })
                        {
                            using (new EditorGUILayout.VerticalScope("box", GUILayout.Width(297),
                                       GUILayout.Height(Mathf.Max(depCount, refCount) * 20 + 20)))
                            {
                                foreach (var guid in errorGUID)
                                {
                                    using (new EditorGUILayout.HorizontalScope("box", GUILayout.Width(297)))
                                    {
                                        EditorGUIUtility.labelWidth = 20;
                                        EditorGUILayout.LabelField(guid, GUILayout.Width(247));
                                        if (GUILayout.Button("Copy", GUILayout.Width(50)))
                                            EditorGUIUtility.systemCopyBuffer = guid;
                                        EditorGUIUtility.labelWidth = normalLabelWidth;
                                    }
                                }
                            }
                        }
                    }

                    //Right
                    using (new EditorGUILayout.VerticalScope("box", GUILayout.Width(297),
                               GUILayout.Height(Mathf.Max(depCount, refCount) * 20 + 20)))
                    {
                        EditorGUIUtility.labelWidth = 20;
                        if (refs is { Length: > 0 })
                        {
                            MFEditorUI.DrawFlipList<Object>(DrawAsset, refs.ToList(), ref pageR, 20);
                        }
                        else
                        {
                            EditorGUILayout.LabelField("No Refs");
                        }
                        EditorGUIUtility.labelWidth = normalLabelWidth;
                    }

                }
            }
        }

        private float TotalCapacitySum(MFRefLinkData mfRefLinkData)
        {
            float cap = 0;
            if (mfRefLinkData.depGUID == null) return 0;
            foreach (var t in mfRefLinkData.depGUID)
            {
                var depData = MFRefLinkCache.LoadCache().GetRefLinkDataByGUID(t);
                if(depData == null || depData.memType == MFRefLinkData.AssetMemoryType.Static) continue;
                if (depData.memType == MFRefLinkData.AssetMemoryType.Art)
                {
                    cap += depData.capacity;
                }
                else
                {
                    cap += TotalCapacitySum(depData);
                }
            }

            return cap;
        }

        private static string CapacityUnitConversion(ref float cap)
        {
            String suffix = "B";
            if (cap > 1024)
            {
                cap /= 1024.0f;
                suffix = "kB";
            }

            if (cap > 1024)
            {
                cap /= 1024.0f;
                suffix = "MB";
            }

            return suffix;
        }


        private void DrawAsset(Object asset, int index)
        {
            using (new EditorGUILayout.HorizontalScope("box", GUILayout.Width(200)))
            {
                EditorGUILayout.ObjectField(index.ToString(), asset, typeof(object), true, GUILayout.Width(150));
                if (GUILayout.Button("Show", GUILayout.Width(50)))
                {
                    var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
                    var data = MFRefLinkCache.LoadCache().GetRefLinkDataByGUID(guid);
                    if (data != null)
                    {
                        Open(data);
                    }
                }
            }
            
        }
    }
}