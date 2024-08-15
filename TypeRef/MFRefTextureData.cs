namespace Moonflow.MFAssetTools.MFRefLink
{
    public class MFRefTextureData : MFRefLinkData
    {
        public override string assetType => MFToolsLang.isCN?"贴图":"Texture";
        public override bool refInMeta => true;
    }
}