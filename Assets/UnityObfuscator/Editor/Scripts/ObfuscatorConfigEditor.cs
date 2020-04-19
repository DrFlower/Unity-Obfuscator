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
        private string[] ObfuscateTypeStr = new string[] { "Blacklist", "Whitelist", "Both" };
        private string[] ObfuscateNameTypeStr = new string[] { "Random", "Word Library" };

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
                    EditorGUILayout.LabelField("Random Seed", new GUILayoutOption[1] { GUILayout.Width(100f) });
                    obfuscatorConfig.randomSeed = EditorGUILayout.IntField(obfuscatorConfig.randomSeed);
                }

                obfuscatorConfig.useTimeSpan = EditorGUILayout.ToggleLeft("Use Time Stamp", obfuscatorConfig.useTimeSpan);

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5f);

                Header("Obfuscate Name", obfuscatorConfig.enableNameObfuscate, (enable) => obfuscatorConfig.enableNameObfuscate = enable);
                using (new EditorGUI.DisabledGroupScope(!obfuscatorConfig.enableNameObfuscate))
                {
                    DrawLeft();
                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Filter Type", new GUILayoutOption[1] { GUILayout.Width(80f) });
                    obfuscatorConfig.nameObfuscateType = (ObfuscateType)EditorGUILayout.Popup((int)obfuscatorConfig.nameObfuscateType, ObfuscateTypeStr);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Name Source", new GUILayoutOption[1] { GUILayout.Width(80f) });
                    obfuscatorConfig.obfuscateNameType = (ObfuscateNameType)EditorGUILayout.Popup((int)obfuscatorConfig.obfuscateNameType, ObfuscateNameTypeStr);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5f);
                    DrawRight();
                }

                Header("Inject Code", obfuscatorConfig.enableCodeInject, (enable) => obfuscatorConfig.enableCodeInject = enable);
                using (new EditorGUI.DisabledGroupScope(!obfuscatorConfig.enableCodeInject))
                {
                    DrawLeft();
                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Filter Type", new GUILayoutOption[1] { GUILayout.Width(80f) });
                    obfuscatorConfig.codeInjectType = (ObfuscateType)EditorGUILayout.Popup((int)obfuscatorConfig.codeInjectType, ObfuscateTypeStr);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(2f);
                    obfuscatorConfig.GarbageMethodMultiplePerClass = EditorGUILayout.IntField(new GUIContent("Generate Useless Method Multiple"), obfuscatorConfig.GarbageMethodMultiplePerClass);
                    GUILayout.Space(2f);
                    obfuscatorConfig.InsertMethodCountPerMethod = EditorGUILayout.IntField(new GUIContent("Call Useless Method Per Method"), obfuscatorConfig.InsertMethodCountPerMethod);
                    GUILayout.Space(5f);
                    DrawRight();
                }

            }

            //DLL路径设置
            GUILayout.Space(10f);
            Header("DLL Path Setting");
            DrawLeft();
            GUILayout.Space(5f);
            for (int i = 0; i < obfuscatorConfig.obfuscateDllPaths.Length; i++)
            {
                DrawDllPathItem(obfuscatorConfig.obfuscateDllPaths, i);
                GUILayout.Space(2f);
            }

            if (GUILayout.Button("+"))
            {
                AddPath();
            }
            GUILayout.Space(5f);
            DrawRight();

            //垃圾代码库路径
            GUILayout.Space(10f);
            Header("Useless Code Library Path");
            DrawLeft();
            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            obfuscatorConfig.uselessCodeLibPath = EditorGUILayout.TextField(obfuscatorConfig.uselessCodeLibPath);
            if (GUILayout.Button("Browse...", new GUILayoutOption[1] { GUILayout.Width(80f) }))
            {
                obfuscatorConfig.uselessCodeLibPath = CheckPath(EditorUtility.OpenFilePanel("", Application.dataPath, "dll"));
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            DrawRight();

            //测试面板
            GUILayout.Space(10f);
            Header("Test");
            DrawLeft();
            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Output Path", new GUILayoutOption[1] { GUILayout.Width(80f) });
            obfuscatorConfig.testOutputPath = EditorGUILayout.TextField(obfuscatorConfig.testOutputPath);
            if (GUILayout.Button("Browse...", new GUILayoutOption[1] { GUILayout.Width(80f) }))
            {
                obfuscatorConfig.testOutputPath = CheckPath(EditorUtility.OpenFolderPanel("", Application.dataPath, "dll"));
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5f);
            if (GUILayout.Button("Obfuscate", new GUILayoutOption[1] { GUILayout.Height(60f) }))
            {
                ProcessObfuscator.TestObfuscate();
            }
            GUILayout.Space(5f);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            DrawRight();

            EditorUtility.SetDirty(obfuscatorConfig);
        }

        private void AddPath()
        {
            ObfuscatorConfig obfuscatorConfig = ((ObfuscatorConfig)target);
            string[] lastData = obfuscatorConfig.obfuscateDllPaths;

            if (lastData != null && lastData.Length > 0)
            {
                string[] newData = new string[lastData.Length + 1];
                for (int i = 0; i < lastData.Length; i++)
                {
                    newData[i] = lastData[i];
                }
                obfuscatorConfig.obfuscateDllPaths = newData;
            }
            else
                obfuscatorConfig.obfuscateDllPaths = new string[1] { "" };

        }

        private void RemovePath(int index)
        {
            ObfuscatorConfig obfuscatorConfig = ((ObfuscatorConfig)target);
            if (obfuscatorConfig.obfuscateDllPaths == null || index > obfuscatorConfig.obfuscateDllPaths.Length - 1)
                return;

            string[] lastData = obfuscatorConfig.obfuscateDllPaths;
            string[] newData = new string[lastData.Length - 1];
            for (int i = 0; i < newData.Length; i++)
            {
                if (i < index)
                    newData[i] = lastData[i];
                else
                    newData[i] = lastData[i + 1];
            }
            obfuscatorConfig.obfuscateDllPaths = newData;

        }

        private string CheckPath(string path)
        {
            if (path.StartsWith(Application.dataPath.Substring(0, Application.dataPath.Length - 6)))
            {
                path = path.Substring(Application.dataPath.Length - 6);
            }

            return path;
        }

        private void DrawDllPathItem(string[] paths, int index)
        {
            ObfuscatorConfig obfuscatorConfig = ((ObfuscatorConfig)target);
            EditorGUILayout.BeginHorizontal();
            paths[index] = EditorGUILayout.TextField(paths[index]);
            if (GUILayout.Button("-", new GUILayoutOption[1] { GUILayout.Width(20f) }))
            {
                RemovePath(index);
            }
            GUILayout.Space(2f);
            if (GUILayout.Button("Browse...", new GUILayoutOption[1] { GUILayout.Width(80f) }))
            {
                paths[index] = CheckPath(EditorUtility.OpenFilePanel("", Application.dataPath, "dll"));
            }

            EditorGUILayout.EndHorizontal();
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