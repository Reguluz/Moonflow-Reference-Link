namespace Moonflow.MFAssetTools.MFRefLink
{
    public class MFRefSceneData : MFRefLinkData
    {
        public override string assetType => MFToolsLang.isCN?"场景":"Scene";
        public override bool refInMeta => false;
    }
}