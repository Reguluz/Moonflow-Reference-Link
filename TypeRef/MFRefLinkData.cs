using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Moonflow.MFAssetTools.MFRefLink
{
    public abstract class MFRefLinkData : ScriptableObject
    {
        //base asset data
        public string path;
        public string guid;
        public bool hasSub = false;
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
                    break;
                case ".hlsl":
                    data = CreateInstance<MFRefHLSLHeaderData>();
                    break;
                case ".cginc":
                    data = CreateInstance<MFRefCGHeaderData>();
                    break;
                case ".mat":
                    data = CreateInstance<MFRefMaterialData>();
                    break;
                case ".prefab":
                    data = CreateInstance<MFRefPrefabData>();
                    break;
                case ".unity":
                    data = CreateInstance<MFRefSceneData>();
                    break;
                case ".tga":
                case ".jpg":
                case ".jpeg":
                case ".png":
                    data = CreateInstance<MFRefTextureData>();
                    break;
                case ".cs":
                    data = CreateInstance<MFRefCSharpData>();
                    break;
                case ".anim":
                    data = CreateInstance<MFRefAnimationData>();
                    break;
                case ".fbx":
                    data = CreateInstance<MFRefModelData>();
                    data.hasSub = true;
                    break;
                case ".asset":
                    data = CreateInstance<MFRefAssetData>();
                    break;
                
                default:
                    Debug.LogWarning("Unknow type of asset: " + extension);
                    break;
            }

            if (data != null)
            {
                data.path = path;
                data.guid = AssetDatabase.AssetPathToGUID(data.path);
                //set data name as asset name
                data.name = Path.GetFileNameWithoutExtension(data.path);
            }

            return data;
        }

        public void UpdateData(Object o)
        {
            throw new System.NotImplementedException();
        }
    }
}