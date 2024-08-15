
using Moonflow.MFAssetTools.MFRefLink;

public class MFRefMaterialData : MFRefLinkData
{
    public string shaderRef;

    public override string assetType => MFToolsLang.isCN?"材质":"Material";
    public override bool refInMeta => false;
}
