namespace Moonflow.MFAssetTools.MFRefLink
{
    public class MFRefAssetData: MFRefLinkData
    {
        public override string assetType => MFToolsLang.isCN?"资产":"Asset";
        public override bool refInMeta => true;
    }
}