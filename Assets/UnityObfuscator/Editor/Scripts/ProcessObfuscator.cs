using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine.SceneManagement;
using System.IO;
using System;

namespace Flower.UnityObfuscator
{

    internal class ProcessObfuscator : IPreprocessBuild, IProcessScene, IPostprocessBuild
    {
        private static bool doObfuscate = false;
        private static bool hasObfuscated = false;

        public int callbackOrder { get { return 1000; } }

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            doObfuscate = true;
        }

        public void OnProcessScene(Scene scene)
        {
            try
            {
                if (doObfuscate && !hasObfuscated)
                    DoObfuscate();

                hasObfuscated = true;
            }
            catch (System.Exception e)
            {
                throw new System.Exception("Unity Obfuscator Error:" + e);
            }

        }

        public void OnPostprocessBuild(BuildTarget target, string path)
        {

        }

        private static void DoObfuscate(string[] assemblyDllPath, string uselessCodeLibAssemblyPath, int randomSeed, bool switchNameObfuscate, bool switchCodeInject,
            ObfuscateType nameObfuscateType, ObfuscateType codeInjectObfuscateType, ObfuscateNameType obfuscateNameType,
            int garbageMethodMultiplePerClass, int insertMethodCountPerMethod)
        {
            CodeObfuscator.DoObfuscate(assemblyDllPath, uselessCodeLibAssemblyPath, randomSeed, switchNameObfuscate, switchCodeInject,
                nameObfuscateType, codeInjectObfuscateType, obfuscateNameType, garbageMethodMultiplePerClass, insertMethodCountPerMethod);
        }


        private static void DoObfuscateByConfig(string[] assemblyPath)
        {
            ObfuscatorConfig obfuscatorConfig = AssetDatabase.LoadAssetAtPath<ObfuscatorConfig>(Const.ConfigAssetPath);

            if (obfuscatorConfig == null)
            {
                Debug.Log(Const.ConfigAssetPath + "不存在");
                return;
            }

            if (!obfuscatorConfig.enableCodeObfuscator)
                return;

            string uselessCodeLibAssemblyPath = obfuscatorConfig.uselessCodeLibPath;

            int randomSeed = obfuscatorConfig.randomSeed;
            if (obfuscatorConfig.useTimeSpan)
                randomSeed = (int)DateTime.Now.Ticks;

            bool enableNameObfuscate = obfuscatorConfig.enableNameObfuscate;
            bool enableCodeInject = obfuscatorConfig.enableCodeInject;
            ObfuscateType nameObfuscateType = obfuscatorConfig.nameObfuscateType;
            ObfuscateType codeInjectObfuscateType = obfuscatorConfig.codeInjectType;
            ObfuscateNameType obfuscateNameType = obfuscatorConfig.obfuscateNameType;
            int garbageMethodMultiplePerClass = obfuscatorConfig.GarbageMethodMultiplePerClass;
            int insertMethodCountPerMethod = obfuscatorConfig.InsertMethodCountPerMethod;

            DoObfuscate(assemblyPath, uselessCodeLibAssemblyPath, randomSeed, enableNameObfuscate, enableCodeInject, nameObfuscateType, codeInjectObfuscateType, obfuscateNameType, garbageMethodMultiplePerClass, insertMethodCountPerMethod);
        }

        private static void DoObfuscate()
        {
            ObfuscatorConfig obfuscatorConfig = AssetDatabase.LoadAssetAtPath<ObfuscatorConfig>(Const.ConfigAssetPath);

            DoObfuscateByConfig(obfuscatorConfig.obfuscateDllPaths);
        }


        //测试混淆（不需要Build，直接输出）
        public static void TestObfuscate()
        {
            ObfuscatorConfig obfuscatorConfig = AssetDatabase.LoadAssetAtPath<ObfuscatorConfig>(Const.ConfigAssetPath);

            string[] pathsConfig = obfuscatorConfig.obfuscateDllPaths;
            if (pathsConfig == null)
            {
                Debug.LogError("目标DLL路径为空");
                return;
            }
            if (!Directory.Exists(obfuscatorConfig.testOutputPath))
            {
                Debug.LogError(string.Format("找不到混淆测试输出路径:{0}", obfuscatorConfig.testOutputPath));
                return;
            }

            FileInfo[] fileInfos = new FileInfo[pathsConfig.Length];

            for (int i = 0; i < pathsConfig.Length; i++)
            {
                string path = pathsConfig[i];
                //有可能记录的是相对路径，这里如果找不到对应路径就找相对路径
                path = File.Exists(path) ? path : (Application.dataPath.Substring(0, Application.dataPath.Length - 6) + path);
                bool exists = File.Exists(path);

                if (!exists)
                {
                    Debug.Log(string.Format("找不到混淆目标文件:{0}", path));
                    return;
                }

                FileInfo fileInfo = new FileInfo(path);

                string oringalMDBPath = fileInfo.FullName + ".mdb";
                if (!File.Exists(oringalMDBPath))
                {
                    Debug.Log(string.Format("找不到该MDB文件:{0}", oringalMDBPath));
                    return;
                }

                fileInfos[i] = fileInfo;
            }

            string[] copyDllPaths = new string[pathsConfig.Length];

            for (int i = 0; i < fileInfos.Length; i++)
            {
                string targetPath = obfuscatorConfig.testOutputPath + "/" + fileInfos[i].Name;
                string oringalMDBPath = fileInfos[i].FullName + ".mdb";
                string targetMDBPath = obfuscatorConfig.testOutputPath + "/" + fileInfos[i].Name + ".mdb";

                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }

                if (File.Exists(targetMDBPath))
                {
                    File.Delete(targetMDBPath);
                }


                File.Copy(fileInfos[i].FullName, targetPath);
                File.Copy(oringalMDBPath, targetMDBPath);
                copyDllPaths[i] = targetPath;
            }

            DoObfuscateByConfig(copyDllPaths);
        }

    }

}
