namespace Moonflow.MFAssetTools.MFRefLink
{
    public class MFRefAnimationData : MFRefLinkData
    {
        public override string assetType => MFToolsLang.isCN?"动画":"Animation";
        public override bool refInMeta => false;
    }
}