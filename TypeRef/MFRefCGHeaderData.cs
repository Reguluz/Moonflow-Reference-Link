namespace Moonflow.MFAssetTools.MFRefLink
{
    public class MFRefCGHeaderData: MFRefLinkData
    {
        public override string assetType => MFToolsLang.isCN?"CG语言头文件":"CGHeader";
        public override bool refInMeta => true;
    }
}