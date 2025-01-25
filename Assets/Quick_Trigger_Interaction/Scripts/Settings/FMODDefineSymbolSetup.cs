// Copyright (c) AstralShift. All rights reserved.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace AstralShift.QTI.Interactions.FMODIntegration
{
    [InitializeOnLoad]
    public class FMODDefineSymbolSetup
    {
        static FMODDefineSymbolSetup()
        {
            AddFMODDefine();
        }

        private static void AddFMODDefine()
        {
#if UNITY_6000_0_OR_NEWER
            string defineSymbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));
#else
            string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
#endif
            if (!defineSymbols.Contains("FMOD_PRESENT"))
            {
                // Check if FMOD is present in the project
                if (System.Type.GetType("FMODUnity.RuntimeManager, FMODUnity") != null)
                {
                    defineSymbols += ";FMOD_PRESENT";
#if UNITY_6000_0_OR_NEWER
                    PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup),
                        defineSymbols);
#else
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                        defineSymbols);
#endif
                    Debug.Log("FMOD_PRESENT define symbol added.");
                }
            }
        }
    }
}
#endif