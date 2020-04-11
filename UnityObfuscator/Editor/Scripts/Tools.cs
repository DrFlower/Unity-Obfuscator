using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;
using System;

namespace Flower.UnityObfuscator
{
    internal class Tools
    {
        private static List<string> whiteListClassNameOnly = new List<string>();
        private static List<string> whiteListMethod = new List<string>();

        private static readonly string[] prefabPaths = new string[] { "Assets/Res" };
        private static readonly string[] scenePaths = new string[] { "Assets/Res" };

        private static readonly string whiteListClassNameOnlyFilePath = @"WhiteListClassNameOnly.txt";
        private static readonly string whiteListMethodFilePath = @"WhiteListMethod.txt";

        /// <summary>
        /// 该函数用于测试
        /// 修改后的dll文件在 ./Library/ScriptAssemblies/Assembly-CSharp_Copy.dll
        /// </summary>
        [MenuItem("Unity Obfuscator/Test Obfuscator", false, 1)]
        private static void TestObfuscate()
        {
            string assemblyOriginalPath = Const.AssemblyDllPath;
            string assemblyMdbOriginalPath = Const.AssemblyMdbPath;

            string assemblyPath = Const.AssemblyDllCopyPath;
            string mdbPath = Const.AssemblyMdbCopyPath;

            if (File.Exists(assemblyPath))
            {
                File.Delete(assemblyPath);
            }
            if (File.Exists(mdbPath))
            {
                File.Delete(mdbPath);
            }

            File.Copy(assemblyOriginalPath, assemblyPath);
            File.Copy(assemblyMdbOriginalPath, mdbPath);

            ProcessObfuscator.DoObfuscateByConfig(assemblyPath);
        }


        [MenuItem("Unity Obfuscator/Create Obfuscator Config Asset", false, 3)]
        private static void CreateObfuscatorConfigAsset()
        {
            if (File.Exists(Const.ConfigAssetPath))
            {
                Debug.Log("ObfuscatorConfig已存在");
                return;
            }

            var exampleAsset = ScriptableObject.CreateInstance<ObfuscatorConfig>();

            AssetDatabase.CreateAsset(exampleAsset, Const.ConfigAssetPath);
            AssetDatabase.Refresh();
            Debug.Log("Create ObfuscatorConfig");
        }


        [MenuItem("Unity Obfuscator/Output WhiteList", false, 2)]
        private static void OutputWhiteList()
        {
            UnityEngine.SceneManagement.Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            try
            {
                UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
                EditorSceneManager.OpenScene(scene.path);
            }
            catch
            {

            }

            whiteListClassNameOnly = new List<string>();
            whiteListMethod = new List<string>();

            AddRangeToListWithoutDuplicate(whiteListClassNameOnly, GetClassInfoOnPrefab());
            AddRangeToListWithoutDuplicate(whiteListClassNameOnly, GetClassInfoOnSceneGO());

            string whiteListClassNameOnlyStr = ConverListStringToLines(whiteListClassNameOnly);
            string whiteListMethodStr = ConverListStringToLines(whiteListMethod);

            File.WriteAllText(whiteListClassNameOnlyFilePath, whiteListClassNameOnlyStr);
            File.WriteAllText(whiteListMethodFilePath, whiteListMethodStr);

            EditorUtility.ClearProgressBar();

            try
            {
                EditorSceneManager.OpenScene(currentScene.path);
            }
            catch
            {

            }


        }

        private static void AddRangeToListWithoutDuplicate(List<string> resultList, List<string> addList)
        {
            foreach (var item in addList)
            {
                if (!resultList.Contains(item))
                    resultList.Add(item);
            }
        }

        private static string ConverListStringToLines(List<string> list)
        {
            string str = string.Empty;
            foreach (var item in list)
            {
                str += item + '\n';
            }

            return str;
        }


        private static List<string> GetAssetsPathWithType(string[] path, string typeParam)
        {
            List<string> list = new List<string>();
            string[] GUIDs = AssetDatabase.FindAssets(typeParam, path);
            foreach (var guid in GUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                list.Add(assetPath);
            }

            return list;
        }

        public static List<string> GetClassInfoOnPrefab()
        {
            List<string> prefabList = GetAssetsPathWithType(prefabPaths, "t:Prefab");
            List<string> list = new List<string>();

            for (int i = 0; i < prefabList.Count; i++)
            {
                ShowProgress((float)i / (float)prefabList.Count, prefabList.Count, i);

                GameObject prefab = AssetDatabase.LoadAssetAtPath(prefabList[i], typeof(GameObject)) as GameObject;
                GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                foreach (Transform child in go.GetComponentsInChildren<Transform>(true))
                {
                    foreach (var component in child.GetComponents<Component>())
                    {
                        string classInfo = GetComponentType(component);
                        if (classInfo != null)
                        {
                            list.Add(classInfo);
                            //Debug.Log(string.Format("{0}:{1}", prefabList[i], classInfo));
                        }
                    }
                }
            }
            return list;
        }

        public static List<string> GetClassInfoOnSceneGO()
        {
            List<string> sceneList = GetAssetsPathWithType(scenePaths, "t:Scene");
            List<string> list = new List<string>();
            int i = 0;

            //所有场景
            foreach (var scenePath in sceneList)
            {
                EditorSceneManager.OpenScene(scenePath);
                //Debug.Log(scenePath);
                ShowProgress((float)i / (float)sceneList.Count, sceneList.Count, i);

                //场景根目录GO
                foreach (GameObject rootObj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    //Debug.Log(string.Format("{0}:{1}", scenePath, rootObj));
                    //子物体
                    foreach (Transform child in rootObj.GetComponentsInChildren<Transform>(true))
                    {
                        //物体上的所有组件
                        foreach (var component in child.GetComponents<Component>())
                        {
                            string classInfo = GetComponentType(component);
                            if (classInfo != null)
                                list.Add(classInfo);
                        }
                    }
                }
                i++;
            }

            return list;
        }

        private static bool ProcessSpecialType(Component component)
        {
            if (component == null)
                return false;

            string type = component.GetType().ToString();

            if (type.StartsWith("UnityEngine."))
            {
                switch (type)
                {
                    case "UnityEngine.UI.Button":
                        ProcessUnityEngineUIButtonType(component);
                        break;
                }

                return true;
            }

            return false;
        }

        private static void ProcessUnityEngineUIButtonType(Component component)
        {
            UnityEngine.UI.Button button = component as UnityEngine.UI.Button;
            if (button == null || button.onClick == null) return;

            int count = button.onClick.GetPersistentEventCount();
            string result = null;
            for (int i = 0; i < count; i++)
            {
                string target = button.onClick.GetPersistentTarget(i).GetType().ToString();
                string method = button.onClick.GetPersistentMethodName(i);

                string str = string.Format("{0}.{1}.{2}", button.onClick.GetPersistentTarget(i).GetType().Namespace, target, method);

                string[] strs = str.Split('.');
                result = string.Empty;
                for (int j = 0; j < strs.Length - 1; j++)
                {
                    result += strs[j];
                    if (j < strs.Length - 2)
                        result += '.';
                }
                if (strs.Length >= 2)
                    result += WhiteList.sperateChar;
                else
                {
                    result += WhiteList.nullChar;
                    result += WhiteList.sperateChar;
                }

                result += strs[strs.Length - 1];
            }

            if (result != null && !whiteListMethod.Contains(result))
                whiteListMethod.Add(result);

        }



        private static string GetComponentType(Component component)
        {
            string result = null;
            if (component == null)
                return result;

            if (!ProcessSpecialType(component))
            {
                string type = component.GetType().ToString();

                string[] strs = type.Split('.');
                result = string.Empty;
                for (int i = 0; i < strs.Length - 1; i++)
                {
                    result += strs[i];
                    if (i < strs.Length - 2)
                        result += '.';
                }
                if (strs.Length >= 2)
                    result += WhiteList.sperateChar;
                else
                {
                    result += WhiteList.nullChar;
                    result += WhiteList.sperateChar;
                }

                result += strs[strs.Length - 1];
            }
            return result;
        }

        private static void ShowProgress(float progress, int total, int cur)
        {
            EditorUtility.DisplayProgressBar("Searching", string.Format("Finding ({0}/{1}), please wait...", cur, total), progress);
        }

    }
}


