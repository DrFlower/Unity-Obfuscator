using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.IO;

namespace Flower.UnityObfuscator
{
    internal static class CodeObfuscator
    {
        public static void DoObfuscate(string[] assemblyPath, string uselessCodeLibAssemblyPath, int randomSeed, bool enableNameObfuscate, bool enableCodeInject,
            ObfuscateType nameObfuscateType, ObfuscateType codeInjectObfuscateType, ObfuscateNameType obfuscateNameType, int garbageMethodMultiplePerClass, int insertMethodCountPerMethod)
        {
            if (Application.isPlaying || EditorApplication.isCompiling)
            {
                Debug.Log("You need stop play mode or wait compiling finished");
                return;
            }

            if (assemblyPath.Length <= 0)
            {
                Debug.LogError("Obfuscate dll paths length: 0");
            }

            Debug.Log("Code Obfuscate Start");

            var resolver = new DefaultAssemblyResolver();
            foreach (var item in Const.ResolverSearchDirs)
            {
                resolver.AddSearchDirectory(item);
            }
            var readerParameters = new ReaderParameters { AssemblyResolver = resolver, ReadSymbols = true };

            AssemblyDefinition[] assemblies = new AssemblyDefinition[assemblyPath.Length];
            for (int i = 0; i < assemblyPath.Length; i++)
            {
                var assembly = AssemblyDefinition.ReadAssembly(assemblyPath[i], readerParameters);

                if (assembly == null)
                {
                    Debug.LogError(string.Format("Code Obfuscate Load assembly failed: {0}", assemblyPath[i]));
                    return;
                }

                assemblies[i] = assembly;
            }

            AssemblyDefinition garbageCodeAssmbly = null;
            if (enableCodeInject)
            {
                garbageCodeAssmbly = AssemblyDefinition.ReadAssembly(uselessCodeLibAssemblyPath, readerParameters);

                if (garbageCodeAssmbly == null)
                {
                    Debug.LogError(string.Format("Code Obfuscate Load assembly failed: {0}", uselessCodeLibAssemblyPath));
                    return;
                }
            }

            try
            {
                //初始化组件
                ObfuscatorHelper.Init(randomSeed);
                NameObfuscate.Instance.Init(nameObfuscateType);
                CodeInject.Instance.Init(codeInjectObfuscateType, garbageMethodMultiplePerClass, insertMethodCountPerMethod);
                NameFactory.Instance.Load(obfuscateNameType);


                //混淆并注入垃圾代码
                for (int i = 0; i < assemblies.Length; i++)
                {
                    var module = assemblies[i].MainModule;

                    if (enableCodeInject)
                        CodeInject.Instance.DoObfuscate(assemblies[i], garbageCodeAssmbly);
                    if (enableNameObfuscate)
                        NameObfuscate.Instance.DoObfuscate(assemblies[i]);
                }

                //把每个dll对其他被混淆的dll的引用名字修改为混淆后的名字
                if (enableNameObfuscate)
                {
                    foreach (var assembly in assemblies)
                    {
                        foreach (var item in assembly.MainModule.GetMemberReferences())
                        {
                            try
                            {
                                if (item is FieldReference)
                                {
                                    FieldReference fieldReference = item as FieldReference;
                                    Dictionary<BaseObfuscateItem, string> dic = NameFactory.Instance.GetOld_New_NameDic(NameType.Filed);
                                    FieldObfuscateItem fieldObfuscateItem = new FieldObfuscateItem(fieldReference.DeclaringType.Namespace, fieldReference.DeclaringType.Name, fieldReference.Name);
                                    if (NameFactory.Instance.AlreadyHaveRandomName(NameType.Filed, fieldObfuscateItem))
                                    {
                                        item.Name = NameFactory.Instance.GetRandomName(NameType.Filed, fieldObfuscateItem);
                                    }
                                }
                                else if (item is PropertyReference)
                                {
                                    PropertyReference propertyReference = item as PropertyReference;

                                    PropertyObfuscateItem propertyObfuscateItem = new PropertyObfuscateItem(propertyReference.DeclaringType.Namespace, propertyReference.DeclaringType.Name, propertyReference.Name);

                                    if (NameFactory.Instance.AlreadyHaveRandomName(NameType.Property, propertyObfuscateItem))
                                    {
                                        item.Name = NameFactory.Instance.GetRandomName(NameType.Property, propertyObfuscateItem);
                                    }


                                }
                                else if (item is MethodReference)
                                {
                                    MethodReference methodReference = item as MethodReference;

                                    MethodObfuscateItem methodObfuscateItem = new MethodObfuscateItem(methodReference.DeclaringType.Namespace, methodReference.DeclaringType.Name, methodReference.Name);

                                    if (NameFactory.Instance.AlreadyHaveRandomName(NameType.Method, methodObfuscateItem))
                                    {
                                        item.Name = NameFactory.Instance.GetRandomName(NameType.Method, methodObfuscateItem);
                                    }
                                }
                            }
                            catch
                            {
                                continue;
                            }
                        }

                        foreach (var item in assembly.MainModule.GetTypeReferences())
                        {
                            try
                            {
                                TypeDefinition typeDefinition = item.Resolve();
                                TypeObfuscateItem typeObfuscateItem = ObfuscateItemFactory.Create(typeDefinition);
                                NamespaceObfuscateItem namespaceObfuscateItem = ObfuscateItemFactory.Create(typeDefinition.Namespace, typeDefinition.Module);

                                if (NameFactory.Instance.AlreadyHaveRandomName(NameType.Class, typeObfuscateItem))
                                {
                                    item.Name = NameFactory.Instance.GetRandomName(NameType.Class, typeObfuscateItem);
                                }
                                if (NameFactory.Instance.AlreadyHaveRandomName(NameType.Namespace, namespaceObfuscateItem))
                                {
                                    item.Namespace = NameFactory.Instance.GetRandomName(NameType.Namespace, namespaceObfuscateItem);
                                }
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }

                }


                for (int i = 0; i < assemblies.Length; i++)
                {
                    assemblies[i].Write(assemblyPath[i], new WriterParameters { WriteSymbols = true });
                }

                Debug.Log("Code Obfuscate Completed!");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("Code Obfuscate failed: {0}", ex));
            }
            finally
            {
                for (int i = 0; i < assemblies.Length; i++)
                {
                    assemblies[i].MainModule.SymbolReader.Dispose();
                }

                if (garbageCodeAssmbly != null && garbageCodeAssmbly.MainModule.SymbolReader != null)
                {
                    garbageCodeAssmbly.MainModule.SymbolReader.Dispose();
                }

                //输出 名字-混淆后名字 的map
                NameFactory.Instance.OutputNameMap(Const.NameMapPath);
            }

        }

    }
}


