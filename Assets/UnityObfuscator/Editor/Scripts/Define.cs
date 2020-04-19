using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flower.UnityObfuscator
{
    internal enum NameType
    {
        Namespace,
        Class,
        Filed,
        Property,
        Method,
        Other,
    }

    internal enum WhiteListType
    {
        Namespace,
        NameSpcaceNameOnly,
        Class,
        ClassNameOnly,
        Method,
        Member
    }

    internal enum ObfuscateType
    {
        ParticularRange,
        WhiteList,
        Both,
    }

    internal enum ObfuscateNameType
    {
        RandomChar,
        NameList
    }

    internal class Const
    {
        /// <summary>
        /// 插件配置文件路径
        /// </summary>
        public static readonly string ConfigAssetPath = @"Assets/UnityObfuscator/Editor/ObfuscatorConfig.asset";

        //白名单配置文件路径
        public static readonly string WhiteList_NamespacePath = @"/UnityObfuscator/Editor/Res/WhiteList/WhiteList-{0}-Namespace.txt";//名单内命名空间不混
        public static readonly string WhiteList_NamespaceNameOnlyPath = @"/UnityObfuscator/Editor/Res/WhiteList/WhiteList-{0}-NamespaceNameOnly.txt";//名单内命名空间的类都混，命名空间的名字不混
        public static readonly string WhiteList_ClassPath = @"/UnityObfuscator/Editor/Res/WhiteList/WhiteList-{0}-Class.txt";//名单内类不混
        public static readonly string WhiteList_ClassNameOnlyPath = @"/UnityObfuscator/Editor/Res/WhiteList/WhiteList-{0}-ClassNameOnly.txt";//名单内的类的成员都混，类名不混
        public static readonly string WhiteList_MethodPath = @"/UnityObfuscator/Editor/Res/WhiteList/WhiteList-{0}-Method.txt";//名单内的方法不混
        public static readonly string WhiteList_MemberPath = @"/UnityObfuscator/Editor/Res/WhiteList/WhiteList-{0}-ClassMember.txt";//名单内的类成员不混

        //混淆范围配置文件路径
        public static readonly string ObfuscateList_NamespacePath = @"/UnityObfuscator/Editor/Res/ObfuscateList/ObfuscateList-{0}-Namespace.txt";//名单内的命名空间所有类和类成员都混
        public static readonly string ObfuscateList_NamespaceExceptNamespaceNamePath = @"/UnityObfuscator/Editor/Res/ObfuscateList/ObfuscateList-{0}-NamespaceExceptNamespaceName.txt";//名单内命名空间的类都混，命名空间的名字不混
        public static readonly string ObfuscateList_ClassPath = @"/UnityObfuscator/Editor/Res/ObfuscateList/ObfuscateList-{0}-Class.txt";//名单内的类都混
        public static readonly string ObfuscateList_ClassExceptClassNamePath = @"/UnityObfuscator/Editor/Res/ObfuscateList/ObfuscateList-{0}-ClassExceptClassName.txt";//名单内的类的类成员都混，类名不混
        public static readonly string ObfuscateList_MethodPath = @"/UnityObfuscator/Editor/Res/ObfuscateList/ObfuscateList-{0}-Method.txt";//名单内的方法都混
        public static readonly string ObfuscateList_MemberPath = @"/UnityObfuscator/Editor/Res/ObfuscateList/ObfuscateList-{0}-ClassMember.txt";//名单内的类成员都混

        public static readonly string NameMapPath = @"UnityObfuscator-Name_Obfuscate_Map.txt";
        public static readonly string InjectInfoPath = @"UnityObfuscator-InjectInfo.txt";

        public static readonly string NameListPath = @"/UnityObfuscator/Editor/Res/NameList.txt";

        public static readonly string[] ResolverSearchDirs = new string[]
        {
            @"/Applications/Unity/Unity.app/Contents/Managed/UnityEngine",
            @"/Applications/Unity/Unity.app/Contents/Managed",
            @"./Assets/Plugins/gamelib",
            @"./Library/ScriptAssemblies",
        };

        public static readonly char[] randomCharArray = { '_', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        /// <summary>
        /// 使用随机字符混淆时，命名最小长度
        /// </summary>
        public static readonly int minRandomNameLen = 4;

        /// <summary>
        /// 使用随机字符混淆时，命名最大长度
        /// </summary>
        public static readonly int maxRandomNameLen = 10;

        /// <summary>
        /// 垃圾代码库命名空间
        /// </summary>
        public static readonly string GarbageCode_Namespace = "GarbageCodeLib";

        /// <summary>
        /// 垃圾代码库类
        /// </summary>
        public static readonly string GarbageCode_Type = "GarbageCode";
    }

}

