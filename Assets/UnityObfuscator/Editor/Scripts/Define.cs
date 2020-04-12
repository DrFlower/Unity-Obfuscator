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

    internal class Const
    {
        //----------------------------------------------------------------------
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

        public static readonly string AssemblyMdbPath = "./Library/ScriptAssemblies/Assembly-CSharp.dll.mdb";
        public static readonly string AssemblyDllPath = "./Library/ScriptAssemblies/Assembly-CSharp.dll";

        public static readonly string AssemblyMdbCopyPath = "./Library/ScriptAssemblies/Assembly-CSharp_Copy.dll.mdb";
        public static readonly string AssemblyDllCopyPath = "./Library/ScriptAssemblies/Assembly-CSharp_Copy.dll";

        public static readonly string AssemblyGarbageDllPath = "./Library/ScriptAssemblies/Assembly-CSharp.dll";

        public static readonly string[] ResolverSearchDirs = new string[]
        {
            @"/Applications/Unity/Unity.app/Contents/Managed/UnityEngine",
            @"/Applications/Unity/Unity.app/Contents/Managed",
            @"./Assets/Plugins/gamelib",
            @"./Library/ScriptAssemblies",
        };

        public static readonly string GarbageCode_Namespace = "GarbageCodeLib";
        public static readonly string GarbageCode_Type = "GarbageCode";
    }

}

