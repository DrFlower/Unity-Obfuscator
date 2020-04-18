using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Cecil;

namespace Flower.UnityObfuscator
{
    internal class DllInfoHelper
    {
        /// <summary>
        /// 获取types(TypeDefinition)的名字信息
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static Dictionary<string, ClassInfo> GetTypesNameInfo(List<TypeDefinition> tList)
        {
            List<TypeDefinition> typeList = new List<TypeDefinition>();
            Dictionary<string, ClassInfo> classNameInfoDict = new Dictionary<string, ClassInfo>();
            foreach (var t in tList)
            {
                classNameInfoDict.Add(t.FullName, new ClassInfo(t));
                typeList.Add(t);
            }
            Dictionary<string, ClassInfo> baseTypeInfo = new Dictionary<string, ClassInfo>();
            TypeReference tmpTR;
            TypeDefinition tmpTD;
            ClassInfo tmpCNI;
            foreach (var t in typeList)
            {
                tmpTR = t.BaseType;
                try
                {
                    tmpTD = tmpTR.Resolve();
                    if (!classNameInfoDict.TryGetValue(tmpTD.FullName, out tmpCNI))
                    {
                        if (!baseTypeInfo.TryGetValue(tmpTD.FullName, out tmpCNI))
                        {
                            tmpCNI = new ClassInfo(tmpTD);
                            baseTypeInfo.Add(tmpTD.FullName, tmpCNI);
                        }
                    }
                    classNameInfoDict[t.FullName].nameSet.UnionWith(tmpCNI.nameSet);
                    tmpTR = tmpTD.BaseType;
                }
                catch
                {
                    break;
                }
            }
            return classNameInfoDict;
        }

        /// <summary>
        /// 获取垃圾函数列表
        /// </summary>
        /// <param name="tList"></param>
        /// <returns></returns>
        public static List<MethodDefinition> GetSrcMethodList(List<TypeDefinition> sList)
        {
            List<MethodDefinition> mInfoList = new List<MethodDefinition>();
            foreach (var t in sList)
            {
                foreach (var method in t.Methods)
                {
                    if (!method.Name.StartsWith(".") && method.IsStatic)
                    {
                        mInfoList.Add(method);
                    }
                }
            }
            return mInfoList;
        }

    }
}


