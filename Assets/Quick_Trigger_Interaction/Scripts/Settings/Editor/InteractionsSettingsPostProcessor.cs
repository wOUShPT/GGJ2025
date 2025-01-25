// Copyright (c) AstralShift. All rights reserved.

using UnityEditor;

namespace AstralShift.QTI.Settings
{
    public class InteractionSettingsPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            InteractionsSettings.Instance.Init();
        }
    }
}


