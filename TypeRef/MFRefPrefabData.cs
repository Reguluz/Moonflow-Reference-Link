namespace Moonflow.MFAssetTools.MFRefLink
{
    public class MFRefPrefabData : MFRefLinkData
    {
        public override string assetType => MFToolsLang.isCN?"预制体":"Prefab";
        public override bool refInMeta => false;
    }
}