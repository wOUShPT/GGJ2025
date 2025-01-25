// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Helpers;
using AstralShift.QTI.Interactions;
using AstralShift.QTI.Interactions.Audio;
using AstralShift.QTI.Interactions.DebugInteractions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Interaction = AstralShift.QTI.Interactions.Interaction;
using InteractionTrigger = AstralShift.QTI.Triggers.InteractionTrigger;

namespace AstralShift.QTI.NodeEditor
{
    public enum ConnectionPath
    {
        Curvy,
        Straight,
        Angled,
        ShaderLab
    }

    public enum ConnectionStroke
    {
        Full,
        Dashed
    }

    public static class NodeEditorPreferences
    {
        /// <summary> The last editor we checked. This should be the one we modify </summary>
        private static NodeGraphEditor lastEditor;

        /// <summary> The last key we checked. This should be the one we modify </summary>
        private static string lastKey = "QTIGraphView.Settings";

        private static Dictionary<Type, Color> _currentTypeColors = new Dictionary<Type, Color>();
        private static Dictionary<string, Settings> settings = new Dictionary<string, Settings>();

        [Serializable]
        public class Settings : ISerializationCallbackReceiver
        {
            public Settings()
            {
                CreateInteractionTypeColors();
            }

            [SerializeField] private Color32 bgPrimaryColorDark = new Color32(88, 88, 88, 255);

            public Color32 BGPrimaryColorDark
            {
                get { return bgPrimaryColorDark; }
                set
                {
                    bgPrimaryColorDark = value;
                    _gridTextureDark = null;
                }
            }

            [SerializeField] private Color32 bgSecondaryColorDark = new Color32(100, 100, 100, 255);

            public Color32 BGSecondaryColorDark
            {
                get { return bgSecondaryColorDark; }
                set
                {
                    bgSecondaryColorDark = value;
                    _gridTextureDark =
                        QTIEditorResources.GraphViewResources.GenerateGridTexture(BGSecondaryColorDark,
                            BGPrimaryColorDark);
                    _crossTextureDark =
                        QTIEditorResources.GraphViewResources.GenerateCrossTexture(BGSecondaryColorDark);
                }
            }

            [SerializeField] private Color32 bgPrimaryColorLight = new Color32(93, 93, 93, 255);

            public Color32 BGPrimaryColorLight
            {
                get { return bgPrimaryColorLight; }
                set
                {
                    bgPrimaryColorLight = value;
                    _gridTextureLight =
                        QTIEditorResources.GraphViewResources.GenerateGridTexture(BGSecondaryColorLight,
                            BGPrimaryColorLight);
                }
            }

            [SerializeField] private Color32 bgSecondaryColorLight = new Color32(71, 71, 71, 255);

            public Color32 BGSecondaryColorLight
            {
                get { return bgSecondaryColorLight; }
                set
                {
                    bgSecondaryColorLight = value;
                    _gridTextureLight =
                        QTIEditorResources.GraphViewResources.GenerateGridTexture(BGSecondaryColorLight,
                            BGPrimaryColorLight);
                    _crossTextureLight =
                        QTIEditorResources.GraphViewResources.GenerateCrossTexture(BGSecondaryColorLight);
                }
            }

            public float maxZoom = 5f;
            public float minZoom = 1f;
            public int nodePaddingX = 150;
            public int nodePaddingY = 150;
            public Color32 darkBodyColor = new Color32(56, 56, 56, 255);
            public Color32 lightBodyColor = new Color32(200, 200, 200, 255);
            public Color32 highlightColor = new Color32(255, 255, 255, 255);
            public bool gridSnap = true;
            public bool autoSave = true;
            public bool openOnCreate = true;
            [HideInInspector] public bool dragToCreate = true;
            public bool createFilter = true;
            public bool zoomToMouse = true;
            [HideInInspector] public bool portTooltips = true;
            [SerializeField] private string typeColorsData = "";
            [NonSerialized] public Dictionary<string, Color> typeColors = new Dictionary<string, Color>();
            public ConnectionPath connectionPath = ConnectionPath.Curvy;
            public float connectionThickness = 2f;

            public ConnectionStroke connectionStroke = ConnectionStroke.Full;

            private Texture2D _gridTextureDark;

            public Texture2D GridTextureDark
            {
                get
                {
                    if (_gridTextureDark == null)
                    {
                        _gridTextureDark =
                            QTIEditorResources.GraphViewResources.GenerateGridTexture(BGSecondaryColorDark,
                                BGPrimaryColorDark);
                    }

                    return _gridTextureDark;
                }
            }

            private Texture2D _crossTextureDark;

            public Texture2D CrossTextureDark
            {
                get
                {
                    if (_crossTextureDark == null)
                    {
                        _crossTextureDark =
                            QTIEditorResources.GraphViewResources.GenerateCrossTexture(BGSecondaryColorDark);
                    }

                    return _crossTextureDark;
                }
            }

            private Texture2D _gridTextureLight;

            public Texture2D GridTextureLight
            {
                get
                {
                    if (_gridTextureLight == null)
                    {
                        _gridTextureLight =
                            QTIEditorResources.GraphViewResources.GenerateGridTexture(BGSecondaryColorLight,
                                BGPrimaryColorLight);
                    }

                    return _gridTextureLight;
                }
            }

            private Texture2D _crossTextureLight;

            public Texture2D CrossTextureLight
            {
                get
                {
                    if (_crossTextureLight == null)
                    {
                        _crossTextureLight =
                            QTIEditorResources.GraphViewResources.GenerateCrossTexture(BGSecondaryColorLight);
                    }

                    return _crossTextureLight;
                }
            }

            public void CreateInteractionTypeColors()
            {
                List<Type> types = typeof(InteractionTrigger).GetDerivedTypes().ToList();
                types.AddRange(typeof(Interaction).GetDerivedTypes().ToList());
                foreach (var type in types)
                {
                    Color color = new Color32(38, 38, 38, 255);

                    switch (type)
                    {
                        // Triggers

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Physics.InputTrigger):

                            color = new Color32(0, 79, 142, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.OnDistanceTrigger):

                            color = new Color32(90, 79, 142, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.OnDistance2DTrigger):

                            color = new Color32(90, 79, 142, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Physics.StepOnTrigger):

                            color = new Color32(0, 144, 77, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Physics.StepOffTrigger):

                            color = new Color32(144, 0, 0, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Physics.StayOnTrigger):

                            color = new Color32(144, 139, 0, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Physics2D.StepOn2DTrigger):

                            color = new Color32(0, 144, 77, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Physics2D.StepOff2DTrigger):

                            color = new Color32(144, 0, 0, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Physics2D.StayOn2DTrigger):

                            color = new Color32(144, 139, 0, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Physics.CollisionEnterTrigger):

                            color = new Color32(121, 158, 0, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Physics.CollisionExitTrigger):

                            color = new Color32(144, 57, 0, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Physics.CollisionStayTrigger):

                            color = new Color32(185, 173, 0, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Physics2D.CollisionEnter2DTrigger):

                            color = new Color32(121, 158, 0, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Physics2D.CollisionExit2DTrigger):

                            color = new Color32(144, 57, 0, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Physics2D.CollisionStay2DTrigger):

                            color = new Color32(185, 173, 0, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Lifecycle.OnAwakeTrigger):

                            color = new Color32(209, 209, 0, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Lifecycle.OnEnableTrigger):

                            color = new Color32(192, 115, 0, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Lifecycle.OnStartTrigger):

                            color = new Color32(192, 0, 0, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Lifecycle.UpdateTrigger):

                            color = new Color32(192, 30, 100, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Lifecycle.FixedUpdateTrigger):

                            color = new Color32(192, 30, 100, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Lifecycle.LateUpdateTrigger):

                            color = new Color32(192, 30, 100, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Lifecycle.OnDestroyTrigger):

                            color = new Color32(0, 30, 150, 255);
                            break;

                        case var _ when type == typeof(AstralShift.QTI.Triggers.Lifecycle.OnDisableTrigger):

                            color = new Color32(0, 30, 150, 255);
                            break;

                        // Interactions

                        case var _ when type == typeof(ActivationInteraction):

                            color = new Color32(134, 209, 144, 255);
                            break;

                        case var _ when type == typeof(AnimationInteraction):

                            color = new Color32(209, 134, 171, 255);
                            break;

                        case var _ when type == typeof(InstantiateInteraction):

                            color = new Color32(255, 111, 111, 255);
                            break;

                        case var _ when type == typeof(UnityEventInteraction):

                            color = new Color32(111, 116, 255, 255);
                            break;

                        case var _ when type == typeof(DestroyInteraction):

                            color = new Color32(209, 202, 134, 255);
                            break;

                        case var _ when type == typeof(DestroyComponentInteraction):

                            color = new Color32(134, 184, 209, 255);
                            break;

                        case var _ when type == typeof(ConditionInteraction):

                            color = new Color32(150, 200, 90, 255);
                            break;

                        case var _ when type == typeof(DelayInteraction):

                            color = new Color32(124, 0, 144, 255);
                            break;

                        case var _ when type == typeof(AudioSourceInteraction):

                            color = new Color32(187, 168, 212, 255);
                            break;

                        case var _ when type == typeof(AudioPlayOneShotInteraction):

                            color = new Color32(134, 151, 209, 255);
                            break;

                        case var _ when type == typeof(DebugLogInteraction):

                            color = new Color32(210, 165, 134, 255);
                            break;

                        case var _ when type == typeof(SetPositionInteraction):

                            color = new Color32(30, 130, 240, 255);
                            break;

                        case var _ when type == typeof(SetRotationInteraction):

                            color = new Color32(30, 130, 240, 255);
                            break;

                        case var _ when type == typeof(SetScaleInteraction):

                            color = new Color32(30, 130, 240, 255);
                            break;

                        case var _ when type == typeof(AddForceInteraction):

                            color = new Color32(30, 190, 100, 255);
                            break;

                        case var _ when type == typeof(InstantiateInteraction):

                            color = new Color32(130, 40, 50, 255);
                            break;
                    }

                    typeColors.TryAdd(type.Name, color);
                    _currentTypeColors.TryAdd(type, color);
                }
            }

            private void DeserializeTypeColors()
            {
                typeColors = new Dictionary<string, Color>();
                string[] data = typeColorsData.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < data.Length; i += 2)
                {
                    if (ColorUtility.TryParseHtmlString("#" + data[i + 1], out Color color))
                    {
                        typeColors.Add(data[i], color);
                    }
                }
            }

            private void SerializeTypeColors()
            {
                // Serialize typeColors
                typeColorsData = "";
                foreach (var item in typeColors)
                {
                    typeColorsData += item.Key + "," + ColorUtility.ToHtmlStringRGB(item.Value) + ",";
                }
            }

            public void OnAfterDeserialize()
            {
                DeserializeTypeColors();
            }

            public void OnBeforeSerialize()
            {
                SerializeTypeColors();
            }
        }

        /// <summary> Get settings of current active editor </summary>
        public static Settings GetSettings()
        {
            if (NodeEditorWindow.current == null)
            {
                return new Settings();
            }

            if (lastEditor != NodeEditorWindow.current.graphEditor)
            {
                object[] attribs = NodeEditorWindow.current.graphEditor.GetType()
                    .GetCustomAttributes(typeof(NodeGraphEditor.CustomNodeGraphEditorAttribute), true);
                if (attribs.Length == 1)
                {
                    NodeGraphEditor.CustomNodeGraphEditorAttribute attrib =
                        attribs[0] as NodeGraphEditor.CustomNodeGraphEditorAttribute;
                    lastEditor = NodeEditorWindow.current.graphEditor;
                    lastKey = attrib.editorPrefsKey;
                }
                else
                {
                    return null;
                }
            }

            if (!settings.ContainsKey(lastKey))
            {
                VerifyLoaded();
            }

            return settings[lastKey];
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider("Preferences/QTI Graph View", SettingsScope.User)
            {
                guiHandler = searchContext => { PreferencesGUI(); },
                keywords = new HashSet<string>(new[]
                    { "interactions", "node", "editor", "graph", "connections", "noodles", "ports" })
            };
            return provider;
        }

        private static void PreferencesGUI()
        {
            VerifyLoaded();
            Settings settings = NodeEditorPreferences.settings[lastKey];

            GeneralSettingsGUI(lastKey, settings);
            ColorSchemeGUI(lastKey, settings);
        }

        private static void GeneralSettingsGUI(string key, Settings settings)
        {
            //Label
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Reset All", "Reset all values to default"), GUILayout.Width(120)))
            {
                ResetPreferences();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Node Padding");
            settings.nodePaddingX = EditorGUILayout.IntField(settings.nodePaddingX);
            settings.nodePaddingY = EditorGUILayout.IntField(settings.nodePaddingY);
            EditorGUILayout.EndHorizontal();
            settings.connectionPath =
                (ConnectionPath)EditorGUILayout.EnumPopup("Connection Path Type", settings.connectionPath);


            if (EditorGUI.EndChangeCheck())
            {
                SavePreferences(key, settings);
                NodeEditorWindow.RepaintAll();
            }

            EditorGUILayout.Space();
        }

        private static GUIStyle _foldoutGroup;

        public static GUIStyle FoldoutGroup
        {
            get
            {
                if (_foldoutGroup == null)
                {
                    _foldoutGroup = new GUIStyle(EditorStyles.foldoutHeader);
                    _foldoutGroup.fontStyle = FontStyle.Normal;
                }

                return _foldoutGroup;
            }
        }

        private static bool _backgroundColorsFoldoutGroup = true;
        private static bool _headerColorsFoldoutGroup;
        private static bool _bodyColorsFoldoutGroup;

        private static void ColorSchemeGUI(string key, Settings settings)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Color Schemes", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            _backgroundColorsFoldoutGroup =
                EditorGUILayout.BeginFoldoutHeaderGroup(_backgroundColorsFoldoutGroup, "Background", FoldoutGroup);

            if (_backgroundColorsFoldoutGroup)
            {
                EditorGUI.indentLevel += 2;
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();

                // Dark Theme
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Dark Theme");
                EditorHelpers.GUILineSeparator();
                settings.BGPrimaryColorDark = EditorGUILayout.ColorField("Primary Color", settings.BGPrimaryColorDark);
                settings.BGSecondaryColorDark =
                    EditorGUILayout.ColorField("Secondary Color", settings.BGSecondaryColorDark);
                EditorGUILayout.EndVertical();

                // Light Theme
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Light Theme");
                EditorHelpers.GUILineSeparator();
                settings.BGPrimaryColorLight =
                    EditorGUILayout.ColorField("Primary Color", settings.BGPrimaryColorLight);
                settings.BGSecondaryColorLight =
                    EditorGUILayout.ColorField("Secondary Color", settings.BGSecondaryColorLight);
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
                EditorGUI.indentLevel -= 2;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            if (EditorGUI.EndChangeCheck())
            {
                SavePreferences(key, settings);
                NodeEditorWindow.RepaintAll();
            }

            _headerColorsFoldoutGroup =
                EditorGUILayout.BeginFoldoutHeaderGroup(_headerColorsFoldoutGroup, "Header", FoldoutGroup);

            if (_headerColorsFoldoutGroup)
            {
                EditorGUI.indentLevel += 2;
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Triggers");
                EditorHelpers.GUILineSeparator();
                //Clone keys so we can enumerate the dictionary and make changes.
                var typeColorKeys = new List<Type>(_currentTypeColors.Keys);

                var triggerColorKeys = typeColorKeys.FindAll(element => NodeEditorReflection.IsTypeATrigger(element));
                float triggerMinLabelSize = EditorGUIUtility.labelWidth;
                foreach (var type in triggerColorKeys)
                {
                    float labelWidth = EditorStyles.label.CalcSize(new GUIContent(type.Name)).x;
                    if (labelWidth > triggerMinLabelSize)
                    {
                        triggerMinLabelSize = labelWidth;
                    }
                }

                float defaultLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = triggerMinLabelSize;

                var interactionsColorKeys =
                    typeColorKeys.FindAll(element => NodeEditorReflection.IsTypeAnInteraction(element));

                //Display type colors. Save them if they are edited by the user
                foreach (var type in triggerColorKeys)
                {
                    string typeColorKey = type.Name;
                    Color col = _currentTypeColors[type];

                    EditorGUI.BeginChangeCheck();

                    col = EditorGUILayout.ColorField(typeColorKey, col);

                    if (EditorGUI.EndChangeCheck())
                    {
                        _currentTypeColors[type] = col;

                        if (settings.typeColors.ContainsKey(typeColorKey))
                        {
                            settings.typeColors[typeColorKey] = col;
                        }
                        else
                        {
                            settings.typeColors.Add(typeColorKey, col);
                        }

                        SavePreferences(key, settings);
                        NodeEditorWindow.RepaintAll();
                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUIUtility.labelWidth = defaultLabelWidth;

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Interactions");
                EditorHelpers.GUILineSeparator();
                //Display type colors. Save them if they are edited by the user
                foreach (var type in interactionsColorKeys)
                {
                    string typeColorKey = type.Name;
                    Color col = _currentTypeColors[type];

                    EditorGUI.BeginChangeCheck();

                    col = EditorGUILayout.ColorField(typeColorKey, col);

                    if (EditorGUI.EndChangeCheck())
                    {
                        _currentTypeColors[type] = col;

                        if (settings.typeColors.ContainsKey(typeColorKey))
                        {
                            settings.typeColors[typeColorKey] = col;
                        }
                        else
                        {
                            settings.typeColors.Add(typeColorKey, col);
                        }

                        SavePreferences(key, settings);
                        NodeEditorWindow.RepaintAll();
                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                EditorGUI.indentLevel -= 2;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            _bodyColorsFoldoutGroup =
                EditorGUILayout.BeginFoldoutHeaderGroup(_bodyColorsFoldoutGroup, "Body", FoldoutGroup);

            if (_bodyColorsFoldoutGroup)
            {
                EditorGUI.indentLevel += 2;
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();

                // Dark Theme
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Dark Theme");
                EditorHelpers.GUILineSeparator();
                settings.darkBodyColor = EditorGUILayout.ColorField("Color", settings.darkBodyColor);
                EditorGUILayout.EndVertical();

                // Light Theme
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Light Theme");
                EditorHelpers.GUILineSeparator();
                settings.lightBodyColor = EditorGUILayout.ColorField("Color", settings.lightBodyColor);
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
                EditorGUI.indentLevel -= 2;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary> Load prefs if they exist. Create if they don't </summary>
        private static Settings LoadPreferences()
        {
            // Create settings if it doesn't exist
            if (!EditorPrefs.HasKey(lastKey))
            {
                if (lastEditor != null)
                {
                    EditorPrefs.SetString(lastKey, JsonUtility.ToJson(lastEditor.GetDefaultPreferences()));
                }
                else
                {
                    EditorPrefs.SetString(lastKey, JsonUtility.ToJson(new Settings()));
                }
            }

            return JsonUtility.FromJson<Settings>(EditorPrefs.GetString(lastKey));
        }

        /// <summary> Delete all prefs </summary>
        public static void ResetPreferences()
        {
            if (EditorPrefs.HasKey(lastKey))
            {
                EditorPrefs.DeleteKey(lastKey);
            }

            if (settings.ContainsKey(lastKey))
            {
                settings.Remove(lastKey);
            }

            _currentTypeColors = new Dictionary<Type, Color>();
            VerifyLoaded();
            NodeEditorWindow.RepaintAll();
        }

        /// <summary> Save preferences in EditorPrefs </summary>
        private static void SavePreferences(string key, Settings settings)
        {
            EditorPrefs.SetString(key, JsonUtility.ToJson(settings));
        }

        /// <summary> Check if we have loaded settings for given key. If not, load them </summary>
        private static void VerifyLoaded()
        {
            if (!settings.ContainsKey(lastKey))
            {
                settings.Add(lastKey, LoadPreferences());
            }
        }

        /// <summary> Return color based on type </summary>
        public static Color GetTypeColor(Type type)
        {
            VerifyLoaded();
            if (type == null)
            {
                return new Color32(38, 38, 38, 255);
            }

            Color color;

            if (!_currentTypeColors.TryGetValue(type, out color))
            {
                return new Color32(38, 38, 38, 255);
            }

            return color;
        }
    }
}