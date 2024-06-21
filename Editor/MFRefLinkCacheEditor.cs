using System;
using Moonflow;
using Moonflow.MFAssetTools.MFRefLink;
using UnityEditor;

namespace Tools.Editor.MFAssetTools.MFRefLink.Editor
{
    [CustomEditor(typeof(MFRefLinkCache))]
    public class MFRefLinkCacheEditor : UnityEditor.Editor
    {
        private MFRefLinkCache _target;
        private int index;
        private void OnEnable()
        {
            _target = target as MFRefLinkCache;
        }

        public override void OnInspectorGUI()
        {
            MFEditorUI.DrawFlipList(_target.refLinkDict,ref index, 50);
        }
    }
}