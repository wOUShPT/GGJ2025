// Copyright (c) AstralShift. All rights reserved.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AstralShift.QTI
{
    public static class QTIEditorResources
    {
        public static GeneralResources General
        {
            get { return _general != null ? _general : _general = new GeneralResources(); }
        }

        private static GeneralResources _general;

        public static GraphViewResources GraphView
        {
            get
            {
                return _graphViewResources != null
                    ? _graphViewResources
                    : _graphViewResources = new GraphViewResources();
            }
        }

        private static GraphViewResources _graphViewResources;

        public class GeneralResources
        {
            private static Texture2D _fullLogoDark;

            private static readonly string FullLogoPathDarkMode =
                "Assets/Quick_Trigger_Interaction/Icons/QTI_Banner_Trigger_Dark_Mode.png";

            private static Texture2D _fullLogoLight;

            private static readonly string FullLogoPathLightMode =
                "Assets/Quick_Trigger_Interaction/Icons/QTI_Banner_Trigger_Light_Mode.png";

            private static Texture2D _addIconDark;

            private static readonly string addIconDarkPath =
                "Assets/Quick_Trigger_Interaction/Icons/Shortcuts/add_l.png";

            private static Texture2D _addIconLight;

            private static readonly string AddIconLightPath =
                "Assets/Quick_Trigger_Interaction/Icons/Shortcuts/add_d.png";

            private static Texture2D _removeIconDark;

            private static readonly string RemoveIconDarkPath =
                "Assets/Quick_Trigger_Interaction/Icons/Shortcuts/remove_l.png";

            private static Texture2D _removeIconLight;

            private static readonly string RemoveIconLightPath =
                "Assets/Quick_Trigger_Interaction/Icons/Shortcuts/remove_d.png";

            private static Texture2D _addBetweenIconDark;

            private static readonly string AddBetweenIconDarkPath =
                "Assets/Quick_Trigger_Interaction/Icons/Shortcuts/addBetween_l.png";

            private static Texture2D _addBetweenIconLight;

            private static readonly string AddBetweenIconLightPath =
                "Assets/Quick_Trigger_Interaction/Icons/Shortcuts/addBetween_d.png";

            private static Texture2D _replaceIconDark;

            private static readonly string ReplaceIconDarkPath =
                "Assets/Quick_Trigger_Interaction/Icons/Shortcuts/replace_l.png";

            private static Texture2D _replaceIconLight;

            private static readonly string ReplaceIconLightPath =
                "Assets/Quick_Trigger_Interaction/Icons/Shortcuts/replace_d.png";

            private static Texture2D _removeAllIconDark;

            private static readonly string RemoveAllIconDarkPath =
                "Assets/Quick_Trigger_Interaction/Icons/Shortcuts/removeAll_l.png";

            private static Texture2D _removeAllIconLight;

            private static readonly string RemoveAllIconLightPath =
                "Assets/Quick_Trigger_Interaction/Icons/Shortcuts/removeAll_d.png";


            public Texture2D FullLogo
            {
                get
                {
                    if (EditorGUIUtility.isProSkin)
                    {
                        return FullLogoDark;
                    }
                    else
                    {
                        return FullLogoLight;
                    }
                }
            }

            public Texture2D FullLogoDark
            {
                get
                {
                    if (_fullLogoDark == null)
                    {
                        _fullLogoDark = new Texture2D(1, 1);
                        _fullLogoDark.LoadImage(System.IO.File.ReadAllBytes(FullLogoPathDarkMode));
                        _fullLogoDark.Apply();
                    }

                    return _fullLogoDark;
                }
            }

            public Texture2D FullLogoLight
            {
                get
                {
                    if (_fullLogoLight == null)
                    {
                        _fullLogoLight = new Texture2D(1, 1);
                        _fullLogoLight.LoadImage(System.IO.File.ReadAllBytes(FullLogoPathLightMode));
                        _fullLogoLight.Apply();
                    }

                    return _fullLogoLight;
                }
            }

            public Texture2D AddIcon
            {
                get
                {
                    if (EditorGUIUtility.isProSkin)
                    {
                        return AddIconDark;
                    }
                    else
                    {
                        return AddIconLight;
                    }
                }
            }

            public Texture2D AddIconDark
            {
                get
                {
                    if (_addIconDark == null)
                    {
                        _addIconDark = AssetDatabase.LoadAssetAtPath<Texture2D>(addIconDarkPath);
                    }

                    return _addIconDark;
                }
            }

            public Texture2D AddIconLight
            {
                get
                {
                    if (_addIconLight == null)
                    {
                        _addIconLight = AssetDatabase.LoadAssetAtPath<Texture2D>(AddIconLightPath);
                    }

                    return _addIconLight;
                }
            }


            public Texture2D AddBetweenIcon
            {
                get
                {
                    if (EditorGUIUtility.isProSkin)
                    {
                        return AddBetweenIconDark;
                    }
                    else
                    {
                        return AddBetweenIconLight;
                    }
                }
            }

            public Texture2D AddBetweenIconDark
            {
                get
                {
                    if (_addBetweenIconDark == null)
                    {
                        _addBetweenIconDark = AssetDatabase.LoadAssetAtPath<Texture2D>(AddBetweenIconDarkPath);
                    }

                    return _addBetweenIconDark;
                }
            }

            public Texture2D AddBetweenIconLight
            {
                get
                {
                    if (_addBetweenIconLight == null)
                    {
                        _addBetweenIconLight = AssetDatabase.LoadAssetAtPath<Texture2D>(AddBetweenIconLightPath);
                    }

                    return _addBetweenIconLight;
                }
            }


            public Texture2D RemoveIcon
            {
                get
                {
                    if (EditorGUIUtility.isProSkin)
                    {
                        return RemoveIconDark;
                    }
                    else
                    {
                        return RemoveIconLight;
                    }
                }
            }

            public Texture2D RemoveIconDark
            {
                get
                {
                    if (_removeIconDark == null)
                    {
                        _removeIconDark = AssetDatabase.LoadAssetAtPath<Texture2D>(RemoveIconDarkPath);
                    }

                    return _removeIconDark;
                }
            }

            public Texture2D RemoveIconLight
            {
                get
                {
                    if (_removeIconLight == null)
                    {
                        _removeIconLight = AssetDatabase.LoadAssetAtPath<Texture2D>(RemoveIconLightPath);
                    }

                    return _removeIconLight;
                }
            }


            public Texture2D ReplaceIcon
            {
                get
                {
                    if (EditorGUIUtility.isProSkin)
                    {
                        return ReplaceIconDark;
                    }
                    else
                    {
                        return ReplaceIconLight;
                    }
                }
            }

            public Texture2D ReplaceIconDark
            {
                get
                {
                    if (_replaceIconDark == null)
                    {
                        _replaceIconDark = AssetDatabase.LoadAssetAtPath<Texture2D>(ReplaceIconDarkPath);
                    }

                    return _replaceIconDark;
                }
            }

            public Texture2D ReplaceIconLight
            {
                get
                {
                    if (_replaceIconLight == null)
                    {
                        _replaceIconLight = AssetDatabase.LoadAssetAtPath<Texture2D>(ReplaceIconLightPath);
                    }

                    return _replaceIconLight;
                }
            }


            public Texture2D RemoveAllIcon
            {
                get
                {
                    if (EditorGUIUtility.isProSkin)
                    {
                        return RemoveAllIconDark;
                    }
                    else
                    {
                        return RemoveAllIconLight;
                    }
                }
            }

            public Texture2D RemoveAllIconDark
            {
                get
                {
                    if (_removeAllIconDark == null)
                    {
                        _removeAllIconDark = AssetDatabase.LoadAssetAtPath<Texture2D>(RemoveAllIconDarkPath);
                    }

                    return _removeAllIconDark;
                }
            }

            public Texture2D RemoveAllIconLight
            {
                get
                {
                    if (_removeAllIconLight == null)
                    {
                        _removeAllIconLight = AssetDatabase.LoadAssetAtPath<Texture2D>(RemoveAllIconLightPath);
                    }

                    return _removeAllIconLight;
                }
            }

            public GeneralStyles Styles
            {
                get { return _styles != null ? _styles : _styles = new GeneralStyles(); }
            }

            public GeneralStyles _styles;

            public class GeneralStyles
            {
                public readonly GUIContent GraphViewButtonContent =
                    new GUIContent("Graph View", "Opens QTI Graph View Window");
            }
        }

        public class GraphViewResources
        {
            public static Texture2D NodeHeaderTex
            {
                get
                {
                    return _nodeHeaderTex != null
                        ? _nodeHeaderTex
                        : _nodeHeaderTex = Resources.Load<Texture2D>("QTI_GraphView_Node_Header");
                }
            }

            private static Texture2D _nodeHeaderTex;

            public static Texture2D NodeBodyTex
            {
                get
                {
                    return _nodeBodyTex != null
                        ? _nodeBodyTex
                        : _nodeBodyTex = Resources.Load<Texture2D>("QTI_GraphView_Node_Body");
                }
            }

            private static Texture2D _nodeBodyTex;

            public static Texture2D NodeHighlightTex
            {
                get
                {
                    return _nodeHighlightTex != null
                        ? _nodeHighlightTex
                        : _nodeHighlightTex = Resources.Load<Texture2D>("QTI_GraphView_Node_Body");
                }
            }

            private static Texture2D _nodeHighlightTex;

            public static Texture2D ConnectionDotTex
            {
                get
                {
                    return _connectionDotTex != null
                        ? _connectionDotTex
                        : _connectionDotTex = Resources.Load<Texture2D>("QTI_GraphView_Node_ConnectionDot");
                }
            }

            private static Texture2D _connectionDotTex;

            public static Texture2D ConnectionDotOuterTex
            {
                get
                {
                    return _connectionDotOuterTex != null
                        ? _connectionDotOuterTex
                        : _connectionDotOuterTex = Resources.Load<Texture2D>("QTI_GraphView_Node_ConnectionDot_Outer");
                }
            }

            private static Texture2D _connectionDotOuterTex;

            public static Texture2D NodeLogoTex
            {
                get
                {
                    return _nodeLogoTex != null
                        ? _nodeLogoTex
                        : _nodeLogoTex = Resources.Load<Texture2D>("QTI_GraphView_Logo");
                }
            }

            private static Texture2D _nodeLogoTex;

            // Styles
            public GraphViewStyles Styles
            {
                get { return _styles != null ? _styles : _styles = new GraphViewStyles(); }
            }

            public GraphViewStyles _styles;

            public static GUIStyle OutputPort
            {
                get { return new GUIStyle(EditorStyles.label) { alignment = TextAnchor.UpperRight }; }
            }

            public class GraphViewStyles
            {
                public GUIStyle InputPort;
                public GUIStyle OutputPort;
                public GUIStyle NodeHeader;
                public GUIStyle NodeHeaderLabel;
                public GUIStyle NodeBodyDark;
                public GUIStyle NodeBodyLight;
                public GUIStyle NodeSection;
                public GUIStyle Tooltip;
                public GUIStyle NodeHighlight;

                public GraphViewStyles()
                {
                    GUIStyle baseStyle = new GUIStyle("Label");
                    baseStyle.fixedHeight = 18;

                    InputPort = new GUIStyle(baseStyle);
                    InputPort.alignment = TextAnchor.UpperLeft;
                    InputPort.padding.left = 0;
                    InputPort.active.background = ConnectionDotTex;
                    InputPort.normal.background = ConnectionDotOuterTex;

                    OutputPort = new GUIStyle(baseStyle);
                    OutputPort.alignment = TextAnchor.UpperRight;
                    OutputPort.padding.right = 0;
                    OutputPort.active.background = ConnectionDotTex;
                    OutputPort.normal.background = ConnectionDotOuterTex;

                    NodeHeader = new GUIStyle();
                    NodeHeader.normal.background = NodeHeaderTex;
                    NodeHeader.border = new RectOffset(32, 32, 32, 4);
                    NodeHeader.padding = new RectOffset(16, 16, 16, 0);

                    NodeHeaderLabel = new GUIStyle();
                    NodeHeaderLabel.alignment = TextAnchor.MiddleCenter;
                    NodeHeaderLabel.fontStyle = FontStyle.Bold;
                    NodeHeaderLabel.fontSize = 14;
                    NodeHeaderLabel.normal.textColor = Color.white;

                    NodeBodyDark = new GUIStyle();
                    NodeBodyDark.normal.background = NodeBodyTex;
                    NodeBodyDark.border = new RectOffset(32, 32, 32, 32);
                    NodeBodyDark.padding = new RectOffset(16, 16, 16, 32);

                    NodeBodyLight = new GUIStyle();
                    NodeBodyLight.normal.background = NodeBodyTex;
                    NodeBodyLight.border = new RectOffset(32, 32, 32, 32);
                    NodeBodyLight.padding = new RectOffset(16, 16, 16, 32);

                    NodeHighlight = new GUIStyle();
                    NodeHighlight.normal.background = NodeHighlightTex;
                    NodeHighlight.border = new RectOffset(32, 32, 32, 32);

                    // Sections Bar Style
                    NodeSection = EditorStyles.toolbarButton;
                    NodeSection.fixedHeight = EditorGUIUtility.singleLineHeight;
                    NodeSection.fontStyle = FontStyle.Bold;
                    NodeSection.alignment = TextAnchor.MiddleLeft;

                    Tooltip = new GUIStyle("helpBox");
                    Tooltip.alignment = TextAnchor.MiddleCenter;
                }
            }

            public static Texture2D GenerateGridTexture(Color line, Color bg)
            {
                Texture2D tex = new Texture2D(64, 64);
                Color[] cols = new Color[64 * 64];
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        Color col = bg;
                        if (y % 16 == 0 || x % 16 == 0)
                        {
                            col = Color.Lerp(line, bg, 0.65f);
                        }

                        if (y == 63 || x == 63)
                        {
                            col = Color.Lerp(line, bg, 0.35f);
                        }

                        cols[(y * 64) + x] = col;
                    }
                }

                tex.SetPixels(cols);
                tex.wrapMode = TextureWrapMode.Repeat;
                tex.filterMode = FilterMode.Bilinear;
                tex.name = "Grid";
                tex.Apply();
                return tex;
            }

            public static Texture2D GenerateCrossTexture(Color line)
            {
                Texture2D tex = new Texture2D(64, 64);
                Color[] cols = new Color[64 * 64];
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        Color col = line;
                        if (y != 31 && x != 31)
                        {
                            col.a = 0;
                        }

                        cols[(y * 64) + x] = col;
                    }
                }

                tex.SetPixels(cols);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
                tex.name = "Grid";
                tex.Apply();
                return tex;
            }
        }
    }
}
#endif