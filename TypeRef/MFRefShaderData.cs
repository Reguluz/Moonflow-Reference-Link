using System.Collections.Generic;

namespace Moonflow.MFAssetTools.MFRefLink
{
    public class MFRefShaderData : MFRefLinkData
    {
        public HashSet<string> matRef;
        public override string assetType => MFToolsLang.isCN?"着色器":"Shader";
        public override bool refInMeta => true;
    }
}