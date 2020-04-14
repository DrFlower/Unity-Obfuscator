using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;
using System.IO;

namespace Flower.UnityObfuscator
{
    internal class CodeInject : Obfuscator
    {
        private Dictionary<string, string> codeInjectInfoDic = new Dictionary<string, string>();
        private List<string> injectMethodList = new List<string>();

        private int GarbageMethodMultiplePerClass;
        private int InsertMethodCountPerMethod;

        protected static CodeInject _instance = null;

        public static CodeInject Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CodeInject();
                }
                return _instance;
            }
        }

        protected override string Symbol
        {
            get
            {
                return "CodeInject";
            }
        }

        public List<string> InjectMethodList
        {
            get
            {
                return injectMethodList;
            }
        }

        public override void Init(ObfuscateType obfuscateType)
        {
            this.obfuscateType = obfuscateType;

            Dictionary<WhiteListType, string> whiteListPathDic = new Dictionary<WhiteListType, string>();
            Dictionary<WhiteListType, string> obfuscateListPathDic = new Dictionary<WhiteListType, string>();

            whiteListPathDic.Add(WhiteListType.Namespace, string.Format(Const.WhiteList_NamespacePath, Symbol));
            whiteListPathDic.Add(WhiteListType.Class, string.Format(Const.WhiteList_ClassPath, Symbol));
            whiteListPathDic.Add(WhiteListType.Method, string.Format(Const.WhiteList_MethodPath, Symbol));

            obfuscateListPathDic.Add(WhiteListType.Namespace, string.Format(Const.ObfuscateList_NamespacePath, Symbol));
            obfuscateListPathDic.Add(WhiteListType.Class, string.Format(Const.ObfuscateList_ClassPath, Symbol));
            obfuscateListPathDic.Add(WhiteListType.Method, string.Format(Const.ObfuscateList_MethodPath, Symbol));



            whiteList = new WhiteList(whiteListPathDic);
            obfuscateList = new WhiteList(obfuscateListPathDic);
            codeInjectInfoDic = new Dictionary<string, string>();
            injectMethodList = new List<string>();
        }

        public void Init(ObfuscateType obfuscateType, int garbageMethodMultiplePerClass, int insertMethodCountPerMethod)
        {
            Init(obfuscateType);

            if (garbageMethodMultiplePerClass < 0 || insertMethodCountPerMethod < 0)
            {
                Debug.LogError("Insert Garbage Code Param Error");
                return;
            }

            GarbageMethodMultiplePerClass = garbageMethodMultiplePerClass;
            InsertMethodCountPerMethod = insertMethodCountPerMethod;
        }

        public override bool MethodNeedSkip(MethodDefinition method)
        {
            bool isIEnumerator = method.ReturnType.FullName == "System.Collections.IEnumerator";
            if (WhiteList.IsSkipMethod(method) || method.IsVirtual || isIEnumerator || method.IsConstructor || method.Name.StartsWith(".")
                || method.Name.Contains("<") || !method.HasBody)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool ClassNeedSkip(TypeDefinition type)
        {
            if (type.IsEnum)
            {
                return true;
            }

            return false;
        }

        protected override bool IsChangeField(TypeDefinition t, string fieldName)
        {
            return false;
        }

        protected override bool IsChangeProperty(TypeDefinition t, string propertyName)
        {
            return false;
        }

        public void DoObfuscate(AssemblyDefinition assembly)
        {
            var module = assembly.MainModule;

            if (module == null)
            {
                Debug.LogError("Target assembly is null");
                return;
            }

            try
            {
                List<TypeDefinition> targetTypeList = new List<TypeDefinition>();
                TypeDefinition garbageType = GetGarbageType(assembly);
                foreach (var type in module.Types)
                {
                    if (!type.IsClass || type.IsAbstract || type.Name.StartsWith("<") || type.Name.Contains("`") || type.HasGenericParameters)
                    {
                        continue;
                    }

                    targetTypeList.Add(type);

                }
                Dictionary<string, ClassInfo> classInfoDic = DllInfoHelper.GetTypesNameInfo(targetTypeList);
                InsertMethodToClass(targetTypeList, garbageType, classInfoDic);
                InsertMethodToMethod(targetTypeList, classInfoDic);
                assembly.MainModule.Types.Remove(garbageType);

                OutputNameMap(Const.InjectInfoPath);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("CSCodeChangeInject failed: {0}", ex));
            }


        }

        public TypeDefinition GetGarbageType(AssemblyDefinition assembly)
        {
            if (assembly == null) return null;

            foreach (TypeDefinition item in assembly.MainModule.Types)
            {
                if (item.Namespace == Const.GarbageCode_Namespace && item.Name == Const.GarbageCode_Type)
                {
                    return item;
                }
            }
            return null;
        }

        public void OutputNameMap(string path)
        {
            string result = string.Empty;


            foreach (var item in codeInjectInfoDic)
            {
                result += item.Key + " ===> " + item.Value + "\n";
            }

            File.WriteAllText(path, result);
        }

        #region Inject Details

        private void InsertMethodToClass(List<TypeDefinition> typeList, TypeDefinition garbageType, Dictionary<string, ClassInfo> tClassNameInfoDict)
        {
            foreach (var t in typeList)
            {
                int insertedNum = 0;
                ClassInfo classInfo = tClassNameInfoDict[t.FullName];
                int insertMethodNum = t.Methods.Count * GarbageMethodMultiplePerClass;
                int garbageCodeSnippetTotal = garbageType.Methods.Count;
                while (insertedNum < insertMethodNum)
                {
                    // 添加函数
                    string mName = classInfo.AddName(NameFactory.Instance.GetRandomName());
                    MethodAttributes mAttr = MethodAttributes.HideBySig | MethodAttributes.FamANDAssem | MethodAttributes.Private | MethodAttributes.Static;
                    TypeReference mRtTr = t.Module.TypeSystem.Void;
                    MethodDefinition targetMethod = new MethodDefinition(mName, mAttr, mRtTr);
                    t.Methods.Add(targetMethod);
                    targetMethod.DeclaringType = t;
                    int garbageCodeSnippetIdx = ObfuscatorHelper.ObfuscateRandom.Next(1, garbageCodeSnippetTotal);
                    MethodDefinition sourceMethod = garbageType.Methods[garbageCodeSnippetIdx];
                    while (sourceMethod.IsConstructor)
                    {
                        garbageCodeSnippetIdx = ObfuscatorHelper.ObfuscateRandom.Next(0, garbageCodeSnippetTotal);
                        sourceMethod = garbageType.Methods[garbageCodeSnippetIdx];
                    }
                    string namespaceStr = string.IsNullOrEmpty(t.Namespace) ? (WhiteList.nullChar).ToString() : t.Namespace;

                    injectMethodList.Add(string.Format("{0}{1}{2}{3}{4}", namespaceStr, WhiteList.sperateChar, t.Name, WhiteList.sperateChar, mName));
                    CopyMethod(t, targetMethod, sourceMethod);
                    insertedNum++;
                }
            }
        }

        private void InsertMethodToMethod(List<TypeDefinition> typeList, Dictionary<string, ClassInfo> classInfoDic)
        {
            foreach (var t in typeList)
            {
                ClassInfo classInfo = classInfoDic[t.FullName];
                List<string> jnList = new List<string>(classInfo.joinName);
                foreach (var m in t.Methods)
                {
                    if (classInfo.joinName.Contains(m.Name) || !IsChangeMethod(t, m))
                    {
                        continue;
                    }
                    var worker = m.Body.GetILProcessor();
                    Instruction ins;
                    int insertNum = InsertMethodCountPerMethod;
                    List<Instruction> slotList;
                    GetInsertSlotList(m, out slotList, insertNum);

                    var parameters = m.Parameters;
                    string parametersStr = string.Empty;
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        parametersStr += parameters[i].ParameterType;
                        if (i != parameters.Count - 1)
                            parametersStr += ",";
                    }

                    string insertMethodStr = string.Empty;

                    for (int i = 0; i < insertNum; i++)
                    {
                        if (jnList.Count > 0)
                        {
                            string mname = jnList[ObfuscatorHelper.ObfuscateRandom.Next(0, jnList.Count)];
                            var mdArr = t.Methods.Where(item => item.Name == mname);
                            foreach (var md in mdArr)
                            {
                                int insNum = m.Body.Instructions.Count;
                                ins = slotList[i];
                                Instruction tmp = worker.Create(OpCodes.Call, t.Module.ImportReference(md));
                                worker.InsertBefore(ins, tmp);
                                insertMethodStr += md.Name;
                                if (i != insertNum - 1)
                                    insertMethodStr += ",";
                                break;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(insertMethodStr))
                        codeInjectInfoDic.Add(string.Format("({3}){0}.{1}({2})", t.Name, m.Name, parametersStr, m.ReturnType), insertMethodStr);
                }
            }
        }


        private void GetInsertSlotList(MethodDefinition m, out List<Instruction> slotList, int LimitNum)
        {
            slotList = new List<Instruction>();
            for (int i = 0; i < m.Body.Instructions.Count; i++)
            {

                Instruction ins = m.Body.Instructions[i];
                if (ins.OpCode.Equals(OpCodes.Nop) ||
                    ins.OpCode.Equals(OpCodes.Endfinally) ||
                    ins.OpCode.Equals(OpCodes.Endfilter) ||
                    ins.OpCode.Equals(OpCodes.Br) ||
                    ins.OpCode.Equals(OpCodes.Br_S) ||
                    ins.OpCode.Equals(OpCodes.Brfalse) ||
                    ins.OpCode.Equals(OpCodes.Brfalse_S) ||
                    ins.OpCode.Equals(OpCodes.Brtrue) ||
                    ins.OpCode.Equals(OpCodes.Brtrue_S))
                {
                    slotList.Add(ins);
                }
            }
            while (slotList.Count < LimitNum)
            {
                slotList.Add(GetDefaultInsertSlot(m));
            }
            while (slotList.Count > LimitNum)
            {
                slotList.RemoveAt(ObfuscatorHelper.ObfuscateRandom.Next(0, slotList.Count));
            }
        }

        private Instruction GetDefaultInsertSlot(MethodDefinition m)
        {
            int count = m.Body.Instructions.Count;
            if (count == 1)
            {
                return m.Body.Instructions[0];
            }
            else
            {
                int rNum = ObfuscatorHelper.ObfuscateRandom.Next(0, 100);
                if (rNum > 50)
                {
                    OpCode op = m.Body.Instructions[count - 2].OpCode;
                    if (op.Equals(OpCodes.Endfinally) || op.Equals(OpCodes.Endfilter))
                    {
                        return m.Body.Instructions[count - 2];
                    }
                    else
                    {
                        return m.Body.Instructions[count - 1];
                    }
                }
                else
                {
                    return m.Body.Instructions[0];
                }
            }
        }

        /// <summary>
        /// Copy a method from one module to another.  If the same method exists in the target module, the caller
        /// is responsible to delete it first.
        /// The sourceMethod makes calls to other methods, we divide the calls into two types:
        /// 1. MethodDefinition : these are methods that are defined in the same module as the sourceMethod;
        /// 2. MethodReference : these are methods that are defined in a different module
        /// For type 1 calls, we will copy these MethodDefinitions to the same target typedef.
        /// For type 2 calls, we will not copy the called method
        /// 
        /// Another limitation: any TypeDefinitions that are used in the sourceMethod will not be copied to the target module; a 
        /// typereference is created instead.
        /// 该函数原版：https://groups.google.com/forum/#!msg/mono-cecil/uoMLJEZrQ1Q/ewthqjEk-jEJ
        /// <summary>
        /// 函数拷贝
        /// </summary>
        /// <param name="t">目标函数所在类</param>
        /// <param name="targetMethod">目标函数</param>
        /// <param name="sourceMethod">原函数</param>
        /// <returns></returns>
        private MethodDefinition CopyMethod(TypeDefinition t, MethodDefinition targetMethod, MethodDefinition sourceMethod)
        {
            TypeDefinition copyToTypedef = t;
            ModuleDefinition targetModule = copyToTypedef.Module;

            // Copy the parameters;  目前都是无参函数
            foreach (ParameterDefinition p in sourceMethod.Parameters)
            {
                ParameterDefinition nP = new ParameterDefinition(NameFactory.Instance.GetRandomName(), p.Attributes, targetModule.ImportReference(p.ParameterType));
                targetMethod.Parameters.Add(nP);
            }

            // copy the body
            MethodBody nBody = targetMethod.Body;
            MethodBody oldBody = sourceMethod.Body;

            nBody.InitLocals = oldBody.InitLocals;

            // copy the local variable definition
            foreach (VariableDefinition v in oldBody.Variables)
            {
                VariableDefinition nv = new VariableDefinition(targetModule.ImportReference(v.VariableType));
                nBody.Variables.Add(nv);
            }

            // copy the IL; we only need to take care of reference and method definitions
            Mono.Collections.Generic.Collection<Instruction> col = nBody.Instructions;
            foreach (Instruction i in oldBody.Instructions)
            {
                object operand = i.Operand;
                if (operand == null)
                {
                    col.Add(Instruction.Create(i.OpCode));
                    continue;
                }

                // for any methodef that this method calls, we will copy it
                if (operand is MethodDefinition)
                {
                    MethodDefinition dmethod = operand as MethodDefinition;
                    MethodDefinition newMethod = dmethod;
                    // MethodDefinition newMethod = CopyMethod (cInfo, dmethod);
                    col.Add(Instruction.Create(i.OpCode, newMethod));
                    continue;
                }

                // for member reference, import it
                if (operand is FieldReference)
                {
                    // original code
                    FieldReference fref = operand as FieldReference;
                    FieldReference newf = targetModule.ImportReference(fref);
                    col.Add(Instruction.Create(i.OpCode, newf));
                    continue;

                    // ? 只用引用
                    // FieldReference fref = operand as FieldReference;
                    // FieldDefinition newfield;
                    // if (!cInfo.GetField (fref.Resolve (), out newfield)) {
                    // 	string newFieldName = cInfo.AddName (fref.Name);
                    // 	newfield = new FieldDefinition (fref.Name, fref.Resolve ().Attributes, fref.FieldType);
                    // 	copyToTypedef.Fields.Add (newfield);
                    // 	cInfo.AddField (fref.Resolve (), newfield);
                    // }
                    // col.Add (Instruction.Create (i.OpCode, newfield));
                    // continue;
                }

                if (operand is TypeReference)
                {
                    TypeReference tref = operand as TypeReference;
                    TypeReference newf = targetModule.ImportReference(tref);
                    col.Add(Instruction.Create(i.OpCode, newf));
                    continue;
                }

                if (operand is TypeDefinition)
                {
                    TypeDefinition tdef = operand as TypeDefinition;
                    TypeReference newf = targetModule.ImportReference(tdef);
                    col.Add(Instruction.Create(i.OpCode, newf));
                    continue;
                }

                if (operand is VariableDefinition)
                {
                    VariableDefinition vdef = operand as VariableDefinition;
                    // VariableDefinition newf = new VariableDefinition(vdef.Name,targetModule.ImportReference (vdef.VariableType));
                    col.Add(Instruction.Create(i.OpCode, vdef));
                    continue;
                }

                if (operand is MethodReference)
                {
                    MethodReference mref = operand as MethodReference;
                    MethodReference newf = targetModule.ImportReference(mref);
                    col.Add(Instruction.Create(i.OpCode, newf));
                    continue;
                }

                if (operand.GetType() == typeof(double))
                {
                    col.Add(Instruction.Create(i.OpCode, (double)operand));
                    continue;
                }

                if (operand.GetType() == typeof(float))
                {
                    col.Add(Instruction.Create(i.OpCode, (float)operand));
                    continue;
                }

                if (operand.GetType() == typeof(long))
                {
                    col.Add(Instruction.Create(i.OpCode, (long)operand));
                    continue;
                }

                if (operand.GetType() == typeof(byte))
                {
                    col.Add(Instruction.Create(i.OpCode, (byte)operand));
                    continue;
                }

                if (operand.GetType() == typeof(sbyte))
                {
                    col.Add(Instruction.Create(i.OpCode, (sbyte)operand));
                    continue;
                }

                if (operand.GetType() == typeof(string))
                {
                    col.Add(Instruction.Create(i.OpCode, (string)operand));
                    continue;
                }

                if (operand.GetType() == typeof(int))
                {
                    col.Add(Instruction.Create(i.OpCode, (int)operand));
                    continue;
                }
                if (operand.GetType() == typeof(Instruction))
                {
                    col.Add(Instruction.Create(i.OpCode, (Instruction)operand));
                    continue;
                }

                if (operand.GetType() == typeof(Instruction[]))
                {
                    col.Add(Instruction.Create(i.OpCode, (Instruction[])operand));
                    continue;
                }

                Debug.Log("warning this instruction is skip");
                Debug.Log(i.OpCode);
                Debug.Log(operand.GetType());
            }

            // copy the exception handler blocks

            foreach (ExceptionHandler eh in oldBody.ExceptionHandlers)
            {
                ExceptionHandler neh = new ExceptionHandler(eh.HandlerType);
                neh.CatchType = targetModule.ImportReference(eh.CatchType);

                // we need to setup neh.Start and End; these are instructions; we need to locate it in the source by index
                if (eh.TryStart != null)
                {
                    int idx = oldBody.Instructions.IndexOf(eh.TryStart);
                    neh.TryStart = col[idx];
                }
                if (eh.TryEnd != null)
                {
                    int idx = oldBody.Instructions.IndexOf(eh.TryEnd);
                    neh.TryEnd = col[idx];
                }

                nBody.ExceptionHandlers.Add(neh);
            }

            return targetMethod;
        }
        #endregion
    }

}
