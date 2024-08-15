namespace Moonflow.MFAssetTools.MFRefLink
{
    public class MFRefModelData : MFRefLinkData
    {
        public override string assetType => MFToolsLang.isCN?"模型":"Model";
        public override bool refInMeta => true;
    }
}