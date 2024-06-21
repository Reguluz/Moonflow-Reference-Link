using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Moonflow.Core;
using Moonflow.MFAssetTools.MFRefLink;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using Directory = System.IO.Directory;
using Object = UnityEngine.Object;

public class MFRefLinkCore
{
    private static int updateCount;
    private static MFRefLinkCache _cache;
    private static bool _onRefProcessing;

    public static MFRefLinkData GetRefData(string guid)
    {
        return _cache.GetRefLinkDataByGUID(guid);
    }
    public static MFRefLinkData GetRefData(Object obj)
    {
        var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
        return GetRefData(guid);
    }

    [MenuItem("Moonflow/Utility/RefLink/Setting/Enable Auto Update")]
    public static void EnableAutoUpdate()
    {
        //unity editor callback: after asset import
        AssetDatabase.importPackageCompleted += AutoUpdate;
    }
    [MenuItem("Moonflow/Utility/RefLink/Setting/Disable Auto Update")]
    public static void DisableAutoUpdate()
    {
        AssetDatabase.importPackageCompleted -= AutoUpdate;
        AssetDatabase.importPackageCompleted -= AutoUpdate;
        AssetDatabase.importPackageCompleted -= AutoUpdate;
        AssetDatabase.importPackageCompleted -= AutoUpdate;
        AssetDatabase.importPackageCompleted -= AutoUpdate;
    }
    
    public static void AutoUpdate(string packageName)
    {
        ManualCollect();
    }

    [MenuItem("Moonflow/Utility/RefLink/Collect")]
    public static void ManualCollect()
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
        else
        {
            updateCount = 0;
            CacheCollection(true);
            Debug.Log($"UpdateCount: {updateCount.ToString()}");
        }
        // TotalLinkForce();
    }

    public static void CacheCollection(bool UpdateMode = false)
    {
        //get all assets
        var assets = AssetDatabase.FindAssets("t:object", new[] {"Assets", "Packages"});
        var subMat = AssetDatabase.FindAssets("t:Material", new[] {"Assets", "Packages"});
        var subMesh = AssetDatabase.FindAssets("t:Mesh", new[] {"Assets", "Packages"});
        List<string> total = new List<string>();
        total.AddRange(assets);
        total.AddRange(subMat);
        total.AddRange(subMesh);

        // ListCollection(UpdateMode, total.ToArray());
        AssetDatabase.StartAssetEditing();
        int index = 0;
        int length = total.Count;
        EditorApplication.update = delegate()
        {

            var asset = total[index];
            ListCollection(UpdateMode, asset);
            _onRefProcessing = EditorUtility.DisplayCancelableProgressBar("Cache Collection",
                $"({index}/{length.ToString()})Collecting...{asset}", (float)index / length);
            index++;
            if (_onRefProcessing || index >= length)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                index = 0;
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
        };
        // EditorUtility.DisplayCancelableProgressBar("Folder Path Collection", "Collecting...", 95);
        // FolderCollection();
        EditorUtility.ClearProgressBar();
    }
    
    // private static void FolderCollection()
    // {
    //     _cache.folderRoot = new MFRefLinkCache.DirectoryNest();
    //     SubFolderSearch("Assets", ref _cache.folderRoot);
    // }
    //
    // private static void SubFolderSearch(string path, ref MFRefLinkCache.DirectoryNest parentNest)
    // {
    //     parentNest.localPath = path;
    //     var dateTime = Directory.GetLastWriteTime(Application.dataPath.Replace("Assets", "") + path);
    //     parentNest.lastTime = dateTime.Ticks;
    //     var sub = AssetDatabase.GetSubFolders(path);
    //     if (sub != null && sub.Length > 0)
    //     {
    //         parentNest.subFolder = new MFRefLinkCache.DirectoryNest[sub.Length];
    //         for (int i = 0; i < sub.Length; i++)
    //         {
    //             SubFolderSearch(sub[i], ref parentNest.subFolder[i]);
    //         }
    //     }
    // }
    //
    // [MenuItem("Test/Update")]
    // public static void UpdateT()
    // {
    //     UpdateTimeStamp("Assets", ref _cache.folderRoot);
    // }
    // private static void UpdateTimeStamp(string path, ref MFRefLinkCache.DirectoryNest parentNest)
    // {
    //     if (parentNest.lastTime <= _cache.timeStamp) return;
    //     var assets = AssetDatabase.FindAssets("t:object", new[] { path });
    // }

    private static void ListCollection(bool UpdateMode, string[] assets)
    {
        
    }

    private static void ListCollection(bool UpdateMode, string asset)
    {
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
                var lastTime = Directory.GetLastWriteTime(path);
                if (lastTime.Ticks > _cache.timeStamp)
                {
                    MFDebug.Log($"UpdateLink {data.path}");
                    //TODO: UpdateLink
                    UpdateLink(data);
                    updateCount++;
                }
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
    }

    public static HashSet<MFRefLinkData> GetFilterCache(string type)
    {
        if (_cache == null)
        {
            if(EditorUtility.DisplayDialog("Cache not found", "Cache not found, collect cache now?", "Yes", "No"))
                ManualCollect();
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

    // [MenuItem("Moonflow/Utility/RefLink/LinkAction/ShaderMatLink")]
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
            ManualCollect();
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
    
    // [MenuItem("Moonflow/Utility/RefLink/LinkAction/TotalLink")]
    public static void TotalLink()
    {
        if(_cache == null || _cache.refLinkDict == null /*|| _cache.subDict == null*/)
            ManualCollect();
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
            UpdateLink(ref linkData, list);

            //editor progress bar
            EditorUtility.DisplayProgressBar("Link Data Reference",
                $"({index}/{list.Count.ToString()})Linking...{list[index].name}",
                (float)index / list.Count);
        }
    }

    private static void UpdateLink(ref MFRefLinkData linkData, List<MFRefLinkData> list, bool forceRefreshDeps = false)
    {
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
        if (linkData.depGUID == null || forceRefreshDeps)
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
                MFDebug.LogError($"{linkData.name}可能引用了空资源, GUID: {depGuid}");
                continue;
            }

            if (depData.refGUID == null)
                depData.refGUID = new HashSet<string>();
            if (depData.guid != linkData.guid)
                depData.refGUID.Add(linkData.guid);
        }
    }

    public static void UpdateLink(MFRefLinkData data)
    {
        var olddeps = data.depGUID;
        UpdateLink(ref data, _cache.refLinkDict, true);
        var newdeps = data.depGUID;
        if(newdeps == null || newdeps.Count == 0)
            return;
        var added = olddeps != null && olddeps.Count != 0 ? newdeps.Except(olddeps) : newdeps;
        foreach (var guid in added)
        {
            var depData = _cache.refLinkDict.Find(x => x.guid == guid);
            if (depData == null)
            {
                MFDebug.LogError($"{data.name}可能引用了空资源, GUID: {guid}");
                continue;
            }

            if (depData.refGUID == null)
                depData.refGUID = new HashSet<string>();
            if (depData.guid != data.guid)
                depData.refGUID.Add(data.guid);
        }
        if(olddeps == null || olddeps.Count == 0)
            return;
        var deleted = olddeps.Except(newdeps);
        foreach (var guid in deleted)
        {
            var depData = _cache.refLinkDict.Find(x => x.guid == guid);
            if (depData == null)
            {
                MFDebug.LogError($"{data.name}可能引用了空资源, GUID: {guid}");
                continue;
            }

            if (depData.refGUID == null)
                depData.refGUID = new HashSet<string>();
            if (depData.guid != data.guid)
                depData.refGUID.Remove(data.guid);
        }
    }
    #endregion
    
}
