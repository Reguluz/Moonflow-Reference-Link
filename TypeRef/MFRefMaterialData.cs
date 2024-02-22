using Moonflow.MFAssetTools.MFRefLink;

public class MFRefMaterialData : MFRefLinkData
{
    public string shaderRef;

    public override string assetType => "Material";
    public override bool refInMeta => false;
}
