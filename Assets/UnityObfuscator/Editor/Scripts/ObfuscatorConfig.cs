using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flower.UnityObfuscator
{
    internal class ObfuscatorConfig : ScriptableObject
    {
        //随机种子
        public int randomSeed = 123;
        //使用时间作为随机种子
        public bool useTimeSpan;

        //启用代码混淆
        public bool enableCodeObfuscator;

        //使用随机字符命名
        public ObfuscateNameType obfuscateNameType;

        //启用名字混淆
        public bool enableNameObfuscate;
        //启用垃圾代码插入
        public bool enableCodeInject;

        //名字混淆的混淆方式
        public ObfuscateType nameObfuscateType;
        //垃圾代码插入的混淆方式
        public ObfuscateType codeInjectType;

        //每个类插入垃圾方法的倍数
        public int GarbageMethodMultiplePerClass = 1;
        //每个方法调用多少垃圾方法(影响性能)
        public int InsertMethodCountPerMethod = 1;

        //需要混淆的Dll的路径
        public string[] obfuscateDllPaths = new string[0];

        //垃圾代码库路径
        public string uselessCodeLibPath = "";

        //测试混淆输出路径
        public string testOutputPath = "";

    }
}


