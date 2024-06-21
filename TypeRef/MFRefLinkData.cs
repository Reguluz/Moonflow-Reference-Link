using System.Collections.Generic;
using System.IO;
using Moonflow.Core;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Moonflow.MFAssetTools.MFRefLink
{
    public abstract class MFRefLinkData : ScriptableObject
    {
        public enum AssetMemoryType
        {
            Static,
            Ref,
            Art
        }
        //base asset data
        public string path;
        public string guid;
        public bool hasSub = false;
        public AssetMemoryType memType;
        public float capacity;
        public float totalCapacity;
        public abstract string assetType { get; }
        public abstract bool refInMeta { get; }
        public HashSet<string> depGUID;
        public HashSet<string> refGUID;
        

        public static MFRefLinkData CreateData(Object o)
        {
            //get extension from o
            var path = AssetDatabase.GetAssetPath(o);
            var extension = Path.GetExtension(path);
            //create different type of MFRefLinkData by extension
            MFRefLinkData data = null;
            switch (extension)
            {
                case ".shader":
                    data = CreateInstance<MFRefShaderData>();
                    data.memType = AssetMemoryType.Static;
                    break;
                case ".hlsl":
                    data = CreateInstance<MFRefHLSLHeaderData>();
                    data.memType = AssetMemoryType.Static;
                    break;
                case ".cginc":
                    data = CreateInstance<MFRefCGHeaderData>();
                    data.memType = AssetMemoryType.Static;
                    break;
                case ".mat":
                    data = CreateInstance<MFRefMaterialData>();
                    data.memType = AssetMemoryType.Ref;
                    break;
                case ".prefab":
                    data = CreateInstance<MFRefPrefabData>();
                    data.memType = AssetMemoryType.Ref;
                    break;
                case ".unity":
                    data = CreateInstance<MFRefSceneData>();
                    data.memType = AssetMemoryType.Ref;
                    break;
                case ".tga":
                case ".jpg":
                case ".jpeg":
                case ".png":
                    data = CreateInstance<MFRefTextureData>();
                    data.memType = AssetMemoryType.Art;
                    break;
                case ".cs":
                    data = CreateInstance<MFRefCSharpData>();
                    data.memType = AssetMemoryType.Static;
                    break;
                case ".anim":
                    data = CreateInstance<MFRefAnimationData>();
                    data.memType = AssetMemoryType.Art;
                    break;
                case ".fbx":
                    data = CreateInstance<MFRefModelData>();
                    data.memType = AssetMemoryType.Art;
                    data.hasSub = true;
                    break;
                case ".asset":
                    data = CreateInstance<MFRefAssetData>();
                    data.memType = AssetMemoryType.Static;
                    break;
                
                default:
                    MFDebug.LogWarning("Unknow type of asset: " + extension);
                    break;
            }

            if (data != null)
            {
                data.path = path;
                data.guid = AssetDatabase.AssetPathToGUID(data.path);
                //set data name as asset name
                data.name = Path.GetFileNameWithoutExtension(data.path);
                data.hideFlags = HideFlags.HideInInspector;
                
                string aPath = Path.GetFullPath(path);
                FileInfo fi = new FileInfo(aPath);
                data.capacity = fi.Length;
            }

            return data;
        }


    }
}