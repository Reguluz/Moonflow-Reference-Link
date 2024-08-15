namespace Moonflow.MFAssetTools.MFRefLink
{
    public class MFRefCSharpData : MFRefLinkData
    {
        public override string assetType => MFToolsLang.isCN?"C#脚本":"CSharp";
        public override bool refInMeta => true;
    }
}