using System;
using System.Collections.Generic;
using System.IO;
using Moonflow.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Moonflow.MFAssetTools.MFRefLink
{
    public class MFRefLinkCache : ScriptableObject
    {
        [Serializable]
        public struct DirectoryNest
        {
            public string localPath;
            public long lastTime;
            public DirectoryNest[] subFolder;
        }
        public List<MFRefLinkData> refLinkDict;
        public DirectoryNest folderRoot;
        // public List<MFRefLinkData> subDict;
        [NonSerialized]public HashSet<MFRefLinkData> refLinkSet;
        // [NonSerialized]public HashSet<MFRefLinkData> subSet;
        [NonSerialized]public static MFRefLinkCache cache;
        //时间戳
        public long timeStamp;
        //static dictionary path, has the same hierarchy of Assets folder
        private static string dictPath = "Assets/Cache/RefLinkDict.asset";

        //clean Cache
        public void CleanCache()
        {
            if (refLinkDict != null)
                refLinkDict.Clear();
            else
                refLinkDict = new List<MFRefLinkData>();
            
            // if(subDict != null)
            //     subDict.Clear();
            // else
            //     subDict = new List<MFRefLinkData>();
        }

        //save Cache
        public static void SaveCache(MFRefLinkCache cache)
        {
            //chech if directory exists
            string dir = Path.GetDirectoryName(dictPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            MFRefLinkData[] tempDict = new MFRefLinkData[cache.refLinkDict.Count];
            cache.refLinkDict.CopyTo(tempDict);
            
            // MFRefLinkData[] subTempDict = new MFRefLinkData[cache.subDict.Count];
            // cache.subDict.CopyTo(subTempDict);

            //delete old asset
            if (File.Exists(dictPath))
            {
                AssetDatabase.DeleteAsset(dictPath);
            }

            MFRefLinkCache newCache = ScriptableObject.CreateInstance<MFRefLinkCache>();
            newCache.refLinkDict = new List<MFRefLinkData>(tempDict);
            newCache.folderRoot = cache.folderRoot;
            
            //set time stamp
            newCache.timeStamp = DateTime.Now.Ticks;
            // newCache.subDict = new List<MFRefLinkData>(subTempDict);
            //create asset
            AssetDatabase.CreateAsset(newCache, dictPath);
            EditorUtility.SetDirty(newCache);
            foreach (var data in newCache.refLinkDict)
            {
                data.hideFlags =/* HideFlags.HideInInspector | */HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(data, newCache);
            }
            
            // foreach (var data in newCache.subDict)
            // {
            //     data.hideFlags =/* HideFlags.HideInInspector | */HideFlags.HideInHierarchy;
            //     AssetDatabase.AddObjectToAsset(data, newCache);
            // }

            AssetDatabase.SaveAssets();
        }

        //load Cache
        public static MFRefLinkCache LoadCache()
        {
            if (cache == null)
            {
                //load asset
                cache = AssetDatabase.LoadAssetAtPath<MFRefLinkCache>(dictPath);
                if (cache == null)
                {
                    Debug.LogWarning("MFRefLinkCache.LoadCache: " + dictPath + " not found, create new one.");
                    return null;
                }
                //translate object to dictionary
                cache.refLinkSet = new HashSet<MFRefLinkData>(cache.refLinkDict);
                // cache.subSet = new HashSet<MFRefLinkData>(cache.subDict);
            }
            return cache;
        }
        
        public MFRefLinkData GetRefLinkDataByGUID(string guid)
        {
            var data = refLinkDict.Find(x => x.guid == guid);
            if (data != null)
            {
                return data;
            }
            return null;
        }
        
        public List<MFRefLinkData> GetRefLinkDataByType(string type)
        {
            var data = refLinkDict.FindAll(x => x.assetType == type);
            if (data.Count > 0)
            {
                return data;
            }
            return null;
        }
    }
}