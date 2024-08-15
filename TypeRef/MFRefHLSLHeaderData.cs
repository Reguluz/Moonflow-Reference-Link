namespace Moonflow.MFAssetTools.MFRefLink
{
    public class MFRefHLSLHeaderData : MFRefLinkData
    {
        public override string assetType => MFToolsLang.isCN?"HLSL头文件":"HLSLHeader";
        public override bool refInMeta => true;
    }
}