using UnityEditor;

namespace Tools.Editor.MFAssetTools.MFRefLink.Editor
{
    public class MFRefLinkDefSetting
    {
        private const string DefineSymbol = "MFRefLink";
        
        [MenuItem("Tools/Moonflow/Utility/RefLink/Add Define Symbol")]
        public static void AddDefineSymbol()
        {
            //add define symbol to unity project
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (symbols.Contains(DefineSymbol))
            {
                return;
            }
            symbols += ";" + DefineSymbol;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
        }
        
        [MenuItem("Tools/Moonflow/Utility/RefLink/Remove Define Symbol")]
        public static void RemoveDefineSymbol()
        {
            //remove define symbol to unity project
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!symbols.Contains(DefineSymbol))
            {
                return;
            }
            symbols = symbols.Replace(DefineSymbol, "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
        }
    }
}