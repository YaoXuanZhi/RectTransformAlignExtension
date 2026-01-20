using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using L10n.Editor;
using System.IO;

namespace InterwovenCode
{
    [CustomEditor(typeof(RectTransform))]
    [CanEditMultipleObjects]
    public class RectTransformAlignInspectorEditor : DecoratorEditor
    {
        private static LocalizationProvider localizationProvider;

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            localizationProvider ??= new LocalizationProvider();
            LocalizationManager.LoadLanguage();
        }

        private static class RectToolbarStyles
        {
            public static GUIStyle ToolbarBg;
            public static GUIStyle IconButton;
            static Texture2D _toolbarNormalTexture;
            static Texture2D _toolbarHoverTexture;
            public const float GroupSpacing = 20f;

            public static Color disableColor = new Color(1f, 1f, 1f, 0.4f);
            public static Color hoverColor = new Color(1f, 1f, 1f, 1f);
            public static Color normalColor = new Color(1f, 1f, 1f, 0.6f);

            public static void Ensure()
            {
                if (ToolbarBg != null) return;

                _toolbarNormalTexture = CreateBorderTexture(
                    bg: normalColor,
                    border: normalColor
                );

                _toolbarHoverTexture = CreateBorderTexture(
                    bg: hoverColor,
                    border: hoverColor
                );

                ToolbarBg = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = _toolbarNormalTexture },

                    fixedHeight = 40,
                    padding = new RectOffset(0, 0, 6, 6)
                };

                IconButton = new GUIStyle(GUI.skin.button)
                {
                    hover = { background = _toolbarHoverTexture },
                    fixedWidth = 25,
                    fixedHeight = 25,
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(2, 2, 0, 0),
                };
            }

            static Texture2D CreateBorderTexture(UnityEngine.Color bg, UnityEngine.Color border)
            {
                // 3x3 是 IMGUI 边框的最小稳定尺寸
                var tex = new Texture2D(3, 3);
                tex.hideFlags = HideFlags.HideAndDontSave;
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Clamp;

                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        bool isBorder = x == 0 || x == 2 || y == 0 || y == 2;
                        tex.SetPixel(x, y, isBorder ? border : bg);
                    }
                }

                tex.Apply();
                return tex;
            }
        }

        public RectTransformAlignInspectorEditor() : base("RectTransformEditor")
        {
        }

        void DrawRectTransformToolbar()
        {
            RectToolbarStyles.Ensure();

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                bool canAlign = AlignLogic.CanAlign();
                DrawIconButton("UI_Align_Left", L10nHelper.Tr("Align Left"), delegate { AlignLogic.Align(AlignType.Left); }, canAlign);
                DrawIconButton("UI_Align_Vertical", L10nHelper.Tr("Align Vertical"), delegate { AlignLogic.Align(AlignType.Vertical); }, canAlign);
                DrawIconButton("UI_Align_Right", L10nHelper.Tr("Align Right"), delegate { AlignLogic.Align(AlignType.Right); }, canAlign);

                GUILayout.Space(RectToolbarStyles.GroupSpacing);

                DrawIconButton("UI_Align_Top", L10nHelper.Tr("Align Top"), delegate { AlignLogic.Align(AlignType.Top); }, canAlign);
                DrawIconButton("UI_Align_Horizontal", L10nHelper.Tr("Align Horizontal"), delegate { AlignLogic.Align(AlignType.Horizontal); }, canAlign);
                DrawIconButton("UI_Align_Bottom", L10nHelper.Tr("Align Bottom"), delegate { AlignLogic.Align(AlignType.Bottom); }, canAlign);

                GUILayout.Space(RectToolbarStyles.GroupSpacing);

                bool canGrid = AlignLogic.CanGrid();
                DrawIconButton("UI_Grid_Horizontal", L10nHelper.Tr("Grid Horizontal"), delegate { AlignLogic.Grid(GridType.Horizontal); }, canGrid);
                DrawIconButton("UI_Grid_Vertical", L10nHelper.Tr("Grid Vertical"), delegate { AlignLogic.Grid(GridType.Vertical); }, canGrid);

                // localizationProvider.OnGUI();

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        void DrawIconButton(string iconName, string tooltip, Action onClick, bool enabled)
        {
            Rect rect = GUILayoutUtility.GetRect(
                RectToolbarStyles.IconButton.fixedWidth,
                RectToolbarStyles.IconButton.fixedHeight,
                RectToolbarStyles.IconButton
            );

            Color oldColor = GUI.color;
            bool prevEnabled = GUI.enabled;
            GUI.enabled = enabled;
            var isHover = rect.Contains(Event.current.mousePosition);
            var tex = LoadIcon(iconName, isHover && enabled);
            if (!enabled)
            {
                GUI.color = RectToolbarStyles.disableColor;
            }
            else if (isHover)
            {
                GUI.color = RectToolbarStyles.hoverColor;
            }
            else
            {
                GUI.color = RectToolbarStyles.normalColor;
            }

            var content = new GUIContent(tex, tooltip);
            if (GUI.Button(rect, content, RectToolbarStyles.IconButton))
            {
                if (enabled)
                {
                    onClick?.Invoke();
                }
            }

            GUI.color = oldColor;
            GUI.enabled = prevEnabled;
        }

        static readonly Dictionary<string, Texture2D> _iconCache = new();

        Texture2D LoadIcon(string name, bool isHover = false)
        {
            if (isHover)
            {
                name = $"{name}_S";
            }

            if (_iconCache.TryGetValue(name, out var tex))
                return tex;

            var iconPath = Path.Combine(Utils.EditorResourcesPath, $"Icon/AlignGrid/{name}.png");

            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);

            _iconCache[name] = tex;
            return tex;
        }

        public override void OnInspectorGUI()
        {
            DrawRectTransformToolbar();
            EditorGUILayout.Space(4);

            base.OnInspectorGUI();
        }
    }
}
