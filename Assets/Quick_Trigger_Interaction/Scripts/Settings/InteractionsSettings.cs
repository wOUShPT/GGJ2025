// Copyright (c) AstralShift. All rights reserved.

using AstralShift.Helpers;
using UnityEditor;
using UnityEngine;

namespace AstralShift.QTI.Settings
{
    [CreateAssetMenu(fileName = "InteractionsSettings", menuName = "ScriptableObjects/InteractionsSettings", order = 2)]
    public class InteractionsSettings : ScriptableObject
    {
        public string PrioritiesEnumFolder => prioritiesEnumFolder;
        public string PrioritiesEnumAssetName => prioritiesEnumAssetName;

        public bool ForceInputTriggerLayer => forceInputTriggerLayer;
        public bool ForceCollisionTriggerLayer => forceCollisionTriggerLayer;

        public int InputTriggerLayer => inputTriggerLayer;
        public int CollisionTriggerLayer => collisionTriggerLayer;

        public static PrioritiesEnumSelector dynamicEnumSelector = new PrioritiesEnumSelector();

        private static InteractionsSettings instance;

        public static InteractionsSettings Instance
        {
            get
            {
#if UNITY_EDITOR
                if (instance == null)
                {
                    instance = LoadInstance();
                }
#endif
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        [Header("Priorities")]
        [SerializeField]
        string prioritiesEnumFolder = "Assets/Quick_Trigger_Interaction/Enums"; // Specify the folder to search for the DynamicEnum asset

        [SerializeField] string prioritiesEnumAssetName = "PrioritiesEnum"; // Specify the name of the DynamicEnum asset

        [Header("Physics Triggers")]
        [SerializeField]
        bool forceInputTriggerLayer;

        [LayerSelector][SerializeField] int inputTriggerLayer;
        [SerializeField] bool forceCollisionTriggerLayer;
        [LayerSelector][SerializeField] int collisionTriggerLayer;

#if UNITY_EDITOR
        public void Init()
        {
            LoadDynamicEnum();
        }

        private static InteractionsSettings LoadInstance()
        {
            string[] guids = AssetDatabase.FindAssets(nameof(InteractionsSettings),
                new[] { "Assets/Quick_Trigger_Interaction/Settings" });
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<InteractionsSettings>(path);
            }
            else
            {
                string path = "Assets/Quick_Trigger_Interaction/Settings/InteractionsSettings.asset";
                InteractionsSettings newSettings = CreateInstance<InteractionsSettings>();
                AssetDatabase.CreateAsset(newSettings, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return newSettings;
            }
        }

        private void LoadDynamicEnum()
        {
            // Find the DynamicEnum asset in the specified folder
            string[] guids = AssetDatabase.FindAssets(PrioritiesEnumAssetName, new[] { PrioritiesEnumFolder });
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                dynamicEnumSelector.dynamicEnum = AssetDatabase.LoadAssetAtPath<PrioritiesEnum>(path);
            }
            else
            {
                string path = PrioritiesEnumFolder + "/" + prioritiesEnumAssetName + ".asset";
                PrioritiesEnum newPrioritiesEnum = CreateInstance<PrioritiesEnum>();
                AssetDatabase.CreateAsset(newPrioritiesEnum, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                dynamicEnumSelector.dynamicEnum = newPrioritiesEnum;
            }
        }

        public PrioritiesEnumSelector GetPriorities()
        {
            if (dynamicEnumSelector == null || dynamicEnumSelector.dynamicEnum == null)
            {
                LoadDynamicEnum();
            }
            return dynamicEnumSelector;
        }

#endif

        public int GetCollisionTriggerLayer(int layer)
        {
            if (ForceCollisionTriggerLayer)
                return CollisionTriggerLayer;
            return layer;
        }

        public int GetInputTriggerLayer(int layer)
        {
            if (ForceInputTriggerLayer)
                return InputTriggerLayer;
            return layer;
        }

        public LayerMask AssignInputTriggerLayerMask(LayerMask layerMask)
        {
            if (ForceInputTriggerLayer)
            {
                return 1 << InputTriggerLayer;
            }
            else return layerMask;
        }
    }
}