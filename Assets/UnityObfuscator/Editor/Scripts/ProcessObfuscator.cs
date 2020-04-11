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
                    DoObfuscateByConfig(Const.AssemblyDllPath);

                hasObfuscated = true;
            }
            catch (System.Exception)
            {
                throw new System.Exception("Unity Obfuscator Error");
            }

        }

        public void OnPostprocessBuild(BuildTarget target, string path)
        {

        }

        public static void EditorObfuscatorConfig(bool enableObfuscator, bool switchNameObfuscate, bool switchCodeInject,
            int nameObfuscateType, int codeInjectObfuscateType,
            int garbageMethodMultiplePerClass, int insertMethodCountPerMethod)
        {
            EditorObfuscatorConfig(enableObfuscator, (int)DateTime.Now.Ticks, switchNameObfuscate, switchCodeInject,
                nameObfuscateType, codeInjectObfuscateType, garbageMethodMultiplePerClass, insertMethodCountPerMethod);
        }
        //混淆总开关、随机种子、开启名字混淆、开启垃圾代码插入、名字混淆方式（特定范围or白名单）、插入垃圾代码方式（特定范围or白名单）、生成垃圾方法的倍数、每个方法里调用垃圾方法的数量
        public static void EditorObfuscatorConfig(bool enableObfuscator, int randomSeed, bool enableNameObfuscate, bool enableCodeInject,
            int nameObfuscateType, int codeInjectObfuscateType,
            int garbageMethodMultiplePerClass, int insertMethodCountPerMethod)
        {
            ObfuscatorConfig obfuscatorConfig = AssetDatabase.LoadAssetAtPath<ObfuscatorConfig>(Const.ConfigAssetPath);

            if (obfuscatorConfig == null)
            {
                Debug.Log(Const.ConfigAssetPath + "不存在");
                return;
            }

            obfuscatorConfig.enableCodeObfuscator = enableObfuscator;
            obfuscatorConfig.randomSeed = randomSeed;
            obfuscatorConfig.enableNameObfuscate = enableNameObfuscate;
            obfuscatorConfig.enableCodeInject = enableCodeInject;
            obfuscatorConfig.nameObfuscateType = (ObfuscateType)nameObfuscateType;
            obfuscatorConfig.codeInjectType = (ObfuscateType)codeInjectObfuscateType;
            obfuscatorConfig.GarbageMethodMultiplePerClass = garbageMethodMultiplePerClass;
            obfuscatorConfig.InsertMethodCountPerMethod = insertMethodCountPerMethod;

            EditorUtility.SetDirty(obfuscatorConfig);
        }

        public static void DoObfuscate(string assemblyDllPath, int randomSeed, bool switchNameObfuscate, bool switchCodeInject,
            ObfuscateType nameObfuscateType, ObfuscateType codeInjectObfuscateType,
            int garbageMethodMultiplePerClass, int insertMethodCountPerMethod)
        {
            CodeObfuscator.DoObfuscate(assemblyDllPath, randomSeed, switchNameObfuscate, switchCodeInject,
                nameObfuscateType, codeInjectObfuscateType, garbageMethodMultiplePerClass, insertMethodCountPerMethod);
        }


        public static void DoObfuscateByConfig(string assemblyPath)
        {
            ObfuscatorConfig obfuscatorConfig = AssetDatabase.LoadAssetAtPath<ObfuscatorConfig>(Const.ConfigAssetPath);

            if (obfuscatorConfig == null)
            {
                Debug.Log(Const.ConfigAssetPath + "不存在");
                return;
            }

            if (!obfuscatorConfig.enableCodeObfuscator)
                return;

            int randomSeed = obfuscatorConfig.randomSeed;
            if (obfuscatorConfig.useTimeSpan)
                randomSeed = (int)DateTime.Now.Ticks;

            bool enableNameObfuscate = obfuscatorConfig.enableNameObfuscate;
            bool enableCodeInject = obfuscatorConfig.enableCodeInject;
            ObfuscateType nameObfuscateType = obfuscatorConfig.nameObfuscateType;
            ObfuscateType codeInjectObfuscateType = obfuscatorConfig.codeInjectType;
            int garbageMethodMultiplePerClass = obfuscatorConfig.GarbageMethodMultiplePerClass;
            int insertMethodCountPerMethod = obfuscatorConfig.InsertMethodCountPerMethod;

            DoObfuscate(assemblyPath, randomSeed, enableNameObfuscate, enableCodeInject, nameObfuscateType, codeInjectObfuscateType, garbageMethodMultiplePerClass, insertMethodCountPerMethod);
        }


    }

}
