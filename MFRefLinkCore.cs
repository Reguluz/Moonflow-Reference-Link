using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Moonflow.MFAssetTools.MFRefLink;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class MFRefLinkCore
{
    private static MFRefLinkCache _cache;

    public static MFRefLinkData GetRefData(string guid)
    {
        return _cache.GetRefLinkDataByGUID(guid);
    }
    public static MFRefLinkData GetRefData(Object obj)
    {
        var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
        return GetRefData(guid);
    }

    [MenuItem("Moonflow/Utility/RefLink/Collect")]
    public static void InitLink()
    {
        //check if cache exists
        _cache = MFRefLinkCache.LoadCache();
        if (_cache == null)
        {
            _cache = ScriptableObject.CreateInstance<MFRefLinkCache>();
            _cache.CleanCache();
            CacheCollection();
            MFRefLinkCache.SaveCache(_cache);
        }
        
    }

    public static void CacheCollection(bool UpdateMode = false)
    {
        //editor progress bar
        EditorUtility.DisplayCancelableProgressBar("Cache Collection", "Collecting...", 0);
        //get all assets
        var assets = AssetDatabase.FindAssets("t:object", new[] {"Assets", "Packages"});
        ListCollection(UpdateMode, assets);
        var subMat = AssetDatabase.FindAssets("t:Material", new[] {"Assets", "Packages"});
        var subMesh = AssetDatabase.FindAssets("t:Mesh", new[] {"Assets", "Packages"});
        ListCollection(UpdateMode, subMat);
        ListCollection(UpdateMode, subMesh);
        EditorUtility.ClearProgressBar();
    }

    private static void ListCollection(bool UpdateMode, string[] assets)
    {
        for (var index = 0; index < assets.Length; index++)
        {
            var asset = assets[index];
            var path = AssetDatabase.GUIDToAssetPath(asset);
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (UpdateMode)
            {
                var data = _cache.refLinkDict.Find(x => x.path == path);
                if (data == null)
                {
                    data = MFRefLinkData.CreateData(obj);
                    if (data != null)
                        // if (sub)
                        // {
                        //     _cache.subDict.Add(data);
                        // }
                        // else
                        // {
                        _cache.refLinkDict.Add(data);
                        // }
                }
                else
                {
                    //skip if data exists in dict
                    // data.UpdateData(obj);
                }
            }
            else
            {
                var data = MFRefLinkData.CreateData(obj);
                if (data != null)
                {
                    // if(sub)
                    //     _cache.subDict.Add(data);
                    // else
                        _cache.refLinkDict.Add(data);
                    // AssetDatabase.AddObjectToAsset(data, _cache);
                }
            }

            EditorUtility.DisplayCancelableProgressBar("Cache Collection",
                $"({index}/{assets.Length.ToString()})Collecting...{assets[index]}", (float)index / assets.Length);
        }
    }

    public static HashSet<MFRefLinkData> GetFilterCache(string type)
    {
        if (_cache == null)
        {
            if(EditorUtility.DisplayDialog("Cache not found", "Cache not found, collect cache now?", "Yes", "No"))
                InitLink();
            else
                return null;
        }
        HashSet<MFRefLinkData> filterData = new HashSet<MFRefLinkData>();
        for (int i = 0; i < _cache.refLinkDict.Count; i++)
        {
            if(_cache.refLinkDict[i].assetType == type)
                filterData.Add(_cache.refLinkDict[i]);
        }
        return filterData;
    }

    #region LinkAction

    [MenuItem("Moonflow/Utility/RefLink/LinkAction/ShaderMatLink")]
    public static void ShaderMaterialLink()
    {
        var shaderCache = GetFilterCache("Shader");
        if (shaderCache == null)
            return;
        var matCache = GetFilterCache("Material");
        if (matCache == null)
            return;
        
        foreach (var shaderData in shaderCache)
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderData.path);
            foreach (var matData in matCache)
            {
                var path = AssetDatabase.GUIDToAssetPath(matData.guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material.shader == shader)
                {
                    var shaderRefData = shaderData as MFRefShaderData;
                    if (shaderRefData.matRef == null)
                        shaderRefData.matRef = new HashSet<string>();
                    if(shaderRefData.refGUID == null)
                        shaderRefData.refGUID = new HashSet<string>();
                    shaderRefData.matRef.Add(matData.guid);
                    shaderRefData.refGUID.Add(matData.guid);
                    
                    var matRefData = matData as MFRefMaterialData;
                    if (matRefData.depGUID == null)
                        matRefData.depGUID = new HashSet<string>();
                    matRefData.shaderRef = shaderData.guid;
                    matRefData.depGUID.Add(shaderData.guid);
                }
            }
        }
        EditorUtility.SetDirty(_cache);
        AssetDatabase.SaveAssetIfDirty(_cache);
        // MFRefLinkCache.SaveCache(_cache);
    }

    [MenuItem("Moonflow/Utility/RefLink/LinkAction/TotalLink(Force)")]
    public static void TotalLinkForce()
    {
        if(_cache == null || _cache.refLinkDict == null)
            InitLink();
        if (_cache.refLinkDict != null)
            for (var index = 0; index < _cache.refLinkDict.Count; index++)
            {
                _cache.refLinkDict[index].depGUID = new HashSet<string>();
                _cache.refLinkDict[index].refGUID = new HashSet<string>();
                EditorUtility.DisplayProgressBar("Link Data Reference", $"({index}/{_cache.refLinkDict.Count.ToString()})Clear Linking...{_cache.refLinkDict[index].name}",
                    (float)index / _cache.refLinkDict.Count);
            }

        TotalLink();
    }
    
    [MenuItem("Moonflow/Utility/RefLink/LinkAction/TotalLink")]
    public static void TotalLink()
    {
        if(_cache == null || _cache.refLinkDict == null /*|| _cache.subDict == null*/)
            InitLink();
        //editor progress bar
        EditorUtility.DisplayProgressBar("Link Data Reference", "Linking...", 0);

        RefListLink(ref _cache.refLinkDict);
        // RefListLink(ref _cache.subDict);

        EditorUtility.ClearProgressBar();
    }

    private static void RefListLink(ref List<MFRefLinkData> list)
    {
        if (list == null) return;
        for (var index = 0; index < list.Count; index++)
        {
            var linkData = list[index];
            var path = AssetDatabase.GUIDToAssetPath(linkData.guid);
            //get absolute path
            var absolutePath = Application.dataPath.Replace("Assets", "") + path;
            //read meta file, which has the same path and same name with asset but with .meta extension
            var metaPath = absolutePath + ".meta";
            //read meta file as text
            var text = System.IO.File.ReadAllText(linkData.refInMeta ? metaPath : absolutePath);
            //find guids by regex
            var guids = Regex.Matches(text, @"guid: ([0-9a-fA-F]{32})");
            //add all guids to depGUID
            if (linkData.depGUID == null)
                linkData.depGUID = new HashSet<string>();
            foreach (Match guid in guids)
            {
                string id = guid.Groups[1].Value /*.Replace("guid: ", "");*/;
                if (id != linkData.guid)
                    linkData.depGUID.Add(id);
            }

            //find all dependency assets from dict and add current to their refGUID
            foreach (var depGuid in linkData.depGUID)
            {
                var depData = list.Find(x => x.guid == depGuid);
                if (depData == null)
                {
                    Debug.LogError($"{linkData.name}可能引用了空资源, GUID: {depGuid}");
                    continue;
                }

                if (depData.refGUID == null)
                    depData.refGUID = new HashSet<string>();
                if (depData.guid != linkData.guid)
                    depData.refGUID.Add(linkData.guid);
            }

            //editor progress bar
            EditorUtility.DisplayProgressBar("Link Data Reference",
                $"({index}/{list.Count.ToString()})Linking...{list[index].name}",
                (float)index / list.Count);
        }
    }

    #endregion
    
}
