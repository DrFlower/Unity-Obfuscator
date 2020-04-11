using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Flower.UnityObfuscator
{
    [CustomEditor(typeof(ObfuscatorConfig))]
    internal class ObfuscatorConfigEditor : Editor
    {
        private static readonly float backgroundSpaceWidth = 5f;
        private GUIStyle box;
        private string[] ObfuscateTypeStr = new string[] { "特定范围", "白名单", "两者并用" };


        public override void OnInspectorGUI()
        {

            if (box == null)
            {
                box = new GUIStyle(GUI.skin.box);
                box.margin.left = 2;
                box.margin.right = 2;
            }

            //base.OnInspectorGUI();
            serializedObject.Update();

            ObfuscatorConfig obfuscatorConfig = ((ObfuscatorConfig)target);

            GUILayout.Space(10f);
            obfuscatorConfig.enableCodeObfuscator = EditorGUILayout.ToggleLeft("Enable Code Obfuscator", obfuscatorConfig.enableCodeObfuscator, EditorStyles.boldLabel);
            GUILayout.Space(10f);
            using (new EditorGUI.DisabledGroupScope(!obfuscatorConfig.enableCodeObfuscator))
            {

                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledGroupScope(obfuscatorConfig.useTimeSpan))
                {
                    EditorGUILayout.LabelField("随机种子", new GUILayoutOption[1] { GUILayout.Width(50f) });
                    obfuscatorConfig.randomSeed = EditorGUILayout.IntField(obfuscatorConfig.randomSeed);
                }

                obfuscatorConfig.useTimeSpan = EditorGUILayout.ToggleLeft("使用时间作为随机种子", obfuscatorConfig.useTimeSpan);

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5f);

                Header("混淆名字", obfuscatorConfig.enableNameObfuscate, (enable) => obfuscatorConfig.enableNameObfuscate = enable);
                using (new EditorGUI.DisabledGroupScope(!obfuscatorConfig.enableNameObfuscate))
                {
                    DrawLeft();
                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("混淆方式", new GUILayoutOption[1] { GUILayout.Width(50f) });
                    obfuscatorConfig.nameObfuscateType = (ObfuscateType)EditorGUILayout.Popup((int)obfuscatorConfig.nameObfuscateType, ObfuscateTypeStr);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5f);
                    DrawRight();
                }

                Header("插入垃圾代码", obfuscatorConfig.enableCodeInject, (enable) => obfuscatorConfig.enableCodeInject = enable);
                using (new EditorGUI.DisabledGroupScope(!obfuscatorConfig.enableCodeInject))
                {
                    DrawLeft();
                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("混淆方式", new GUILayoutOption[1] { GUILayout.Width(50f) });
                    obfuscatorConfig.codeInjectType = (ObfuscateType)EditorGUILayout.Popup((int)obfuscatorConfig.codeInjectType, ObfuscateTypeStr);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(2f);
                    obfuscatorConfig.GarbageMethodMultiplePerClass = EditorGUILayout.IntField(new GUIContent("生成垃圾方法倍数"), obfuscatorConfig.GarbageMethodMultiplePerClass);
                    GUILayout.Space(2f);
                    obfuscatorConfig.InsertMethodCountPerMethod = EditorGUILayout.IntField(new GUIContent("调用垃圾方法数量"), obfuscatorConfig.InsertMethodCountPerMethod);
                    GUILayout.Space(5f);
                    DrawRight();
                }

            }

            //GUILayout.Space(10f);
            //Header("路径设置");
            //DrawLeft();
            //GUILayout.Space(5f);

            //GUILayout.Space(5f);
            //DrawRight();

            EditorUtility.SetDirty(obfuscatorConfig);
        }

        private void Header(string title, bool enable, Action<bool> setEnableAction)
        {
            var rect = GUILayoutUtility.GetRect(16f, 22f, FxStyles.header);
            GUI.Box(rect, title, FxStyles.header);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);


            if (e.type == EventType.Repaint)
                FxStyles.headerCheckbox.Draw(toggleRect, false, false, enable, false);


            if (e.type == EventType.MouseDown)
            {
                const float kOffset = 2f;
                toggleRect.x -= kOffset;
                toggleRect.y -= kOffset;
                toggleRect.width += kOffset * 2f;
                toggleRect.height += kOffset * 2f;
                if (toggleRect.Contains(e.mousePosition))
                {
                    setEnableAction(!enable);
                    e.Use();
                }
            }
        }

        private void Header(string title)
        {
            var rect = GUILayoutUtility.GetRect(16f, 22f, FxStyles.header);
            GUI.Box(rect, title, FxStyles.header);
        }



        private void DrawLeft()
        {
            EditorGUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
            GUILayout.Space(backgroundSpaceWidth);
            EditorGUILayout.BeginVertical(box, (GUILayoutOption[])new GUILayoutOption[1]
            {
                GUILayout.MinHeight(EditorGUIUtility.singleLineHeight)
            });
        }

        private void DrawRight()
        {
            EditorGUILayout.EndVertical();
            GUILayout.Space(backgroundSpaceWidth);
            EditorGUILayout.EndHorizontal();
        }
    }


    internal static class FxStyles
    {
        public static GUIStyle tickStyleRight;
        public static GUIStyle tickStyleLeft;
        public static GUIStyle tickStyleCenter;

        public static GUIStyle preSlider;
        public static GUIStyle preSliderThumb;
        public static GUIStyle preButton;
        public static GUIStyle preDropdown;

        public static GUIStyle preLabel;
        public static GUIStyle hueCenterCursor;
        public static GUIStyle hueRangeCursor;

        public static GUIStyle centeredBoldLabel;
        public static GUIStyle wheelThumb;
        public static Vector2 wheelThumbSize;

        public static GUIStyle header;
        public static GUIStyle headerCheckbox;
        public static GUIStyle headerFoldout;

        public static Texture2D playIcon;
        public static Texture2D checkerIcon;
        public static Texture2D paneOptionsIcon;

        public static GUIStyle centeredMiniLabel;

        static FxStyles()
        {
            tickStyleRight = new GUIStyle("Label")
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = 9
            };

            tickStyleLeft = new GUIStyle("Label")
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 9
            };

            tickStyleCenter = new GUIStyle("Label")
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9
            };

            preSlider = new GUIStyle("PreSlider");
            preSliderThumb = new GUIStyle("PreSliderThumb");
            preButton = new GUIStyle("PreButton");
            preDropdown = new GUIStyle("preDropdown");

            preLabel = new GUIStyle("ShurikenLabel");

            hueCenterCursor = new GUIStyle("ColorPicker2DThumb")
            {
                normal = { background = (Texture2D)EditorGUIUtility.LoadRequired("Builtin Skins/DarkSkin/Images/ShurikenPlus.png") },
                fixedWidth = 6,
                fixedHeight = 6
            };

            hueRangeCursor = new GUIStyle(hueCenterCursor)
            {
                normal = { background = (Texture2D)EditorGUIUtility.LoadRequired("Builtin Skins/DarkSkin/Images/CircularToggle_ON.png") }
            };

            wheelThumb = new GUIStyle("ColorPicker2DThumb");

            centeredBoldLabel = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold
            };

            centeredMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.UpperCenter
            };

            wheelThumbSize = new Vector2(
                    !Mathf.Approximately(wheelThumb.fixedWidth, 0f) ? wheelThumb.fixedWidth : wheelThumb.padding.horizontal,
                    !Mathf.Approximately(wheelThumb.fixedHeight, 0f) ? wheelThumb.fixedHeight : wheelThumb.padding.vertical
                    );

            header = new GUIStyle("ShurikenModuleTitle")
            {
                font = (new GUIStyle("Label")).font,
                border = new RectOffset(15, 7, 4, 4),
                fixedHeight = 22,
                contentOffset = new Vector2(20f, -2f),
            };

            headerCheckbox = new GUIStyle("ShurikenCheckMark");
            headerFoldout = new GUIStyle("Foldout");

            playIcon = (Texture2D)EditorGUIUtility.LoadRequired("Builtin Skins/DarkSkin/Images/IN foldout act.png");
            checkerIcon = (Texture2D)EditorGUIUtility.LoadRequired("Icons/CheckerFloor.png");

            if (EditorGUIUtility.isProSkin)
                paneOptionsIcon = (Texture2D)EditorGUIUtility.LoadRequired("Builtin Skins/DarkSkin/Images/pane options.png");
            else
                paneOptionsIcon = (Texture2D)EditorGUIUtility.LoadRequired("Builtin Skins/LightSkin/Images/pane options.png");
        }

    }

}