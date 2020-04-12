using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Cecil;


namespace Flower.UnityObfuscator
{
    internal class NameObfuscate : Obfuscator
    {
        protected static NameObfuscate _instance = null;

        public static NameObfuscate Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NameObfuscate();
                }
                return _instance;
            }
        }

        protected override string Symbol
        {
            get
            {
                return "NameObfuscate";
            }
        }

        private readonly bool useFullNameMap = false;

        public override void Init(ObfuscateType obfuscateType)
        {
            this.obfuscateType = obfuscateType;

            Dictionary<WhiteListType, string> whiteListPathDic = new Dictionary<WhiteListType, string>();
            Dictionary<WhiteListType, string> obfuscateListPathDic = new Dictionary<WhiteListType, string>();

            whiteListPathDic.Add(WhiteListType.Namespace, string.Format(Const.WhiteList_NamespacePath, Symbol));
            whiteListPathDic.Add(WhiteListType.NameSpcaceNameOnly, string.Format(Const.WhiteList_NamespaceNameOnlyPath, Symbol));
            whiteListPathDic.Add(WhiteListType.Class, string.Format(Const.WhiteList_ClassPath, Symbol));
            whiteListPathDic.Add(WhiteListType.ClassNameOnly, string.Format(Const.WhiteList_ClassNameOnlyPath, Symbol));
            whiteListPathDic.Add(WhiteListType.Method, string.Format(Const.WhiteList_MethodPath, Symbol));
            whiteListPathDic.Add(WhiteListType.Member, string.Format(Const.WhiteList_MemberPath, Symbol));

            obfuscateListPathDic.Add(WhiteListType.Namespace, string.Format(Const.ObfuscateList_NamespacePath, Symbol));
            obfuscateListPathDic.Add(WhiteListType.NameSpcaceNameOnly, string.Format(Const.ObfuscateList_NamespaceExceptNamespaceNamePath, Symbol));
            obfuscateListPathDic.Add(WhiteListType.Class, string.Format(Const.ObfuscateList_ClassPath, Symbol));
            obfuscateListPathDic.Add(WhiteListType.ClassNameOnly, string.Format(Const.ObfuscateList_ClassExceptClassNamePath, Symbol));
            obfuscateListPathDic.Add(WhiteListType.Method, string.Format(Const.ObfuscateList_MethodPath, Symbol));
            obfuscateListPathDic.Add(WhiteListType.Member, string.Format(Const.ObfuscateList_MemberPath, Symbol));

            whiteList = new WhiteList(whiteListPathDic);
            obfuscateList = new WhiteList(obfuscateListPathDic);
        }

        public void AddInjectMethodListToWhiteList(List<string> list)
        {
            whiteList.AddInjectMethod(CodeInject.Instance.InjectMethodList);
        }

        public override bool MethodNeedSkip(MethodDefinition method)
        {
            bool isIEnumerator = method.ReturnType.FullName == "System.Collections.IEnumerator";
            if (WhiteList.IsSkipMethod(method) || method.IsVirtual || isIEnumerator || method.IsConstructor || method.Name.StartsWith(".") || method.Name.Contains("<"))
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
            if (type.Name.Contains("<") || type.IsEnum || type.Name.Contains("`"))
            {
                return true;
            }

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

            AddInjectMethodListToWhiteList(CodeInject.Instance.InjectMethodList);

            foreach (var type in module.Types)
            {
                if (ClassNeedSkip(type))
                    continue;

                ChangeFieldName(type);
                ChangePropertyName(type);
                ChangeMethodName(type);
                ChangeClassName(type);
                ChangeNamespace(type);
            }
        }

        private bool IsSubClassOfType(TypeDefinition t, string parentClassName)
        {
            TypeReference btype = t.BaseType;
            while (btype != null)
            {
                if (btype.Name == parentClassName)
                {
                    return true;
                }
                try
                {
                    btype = btype.Resolve().BaseType;
                }
                catch (System.Exception e)
                {
                    //ModuleDefinition tmpMD = btype.Module;
                    Debug.LogError(e);
                    break;
                }

            }
            return false;
        }


        /// <summary>
        /// 修改字段名
        /// </summary>
        /// <param name="t"></param>
        private void ChangeFieldName(TypeDefinition t)
        {
            // * 遍历字段
            foreach (var field in t.Fields)
            {
                string _spaceName = t.Namespace;
                string className = t.Name;
                string fieldName = field.Name;

                if (IsChangeField(t, fieldName))
                {
                    field.Name = NameFactory.Instance.GetRandomName(NameType.Filed, useFullNameMap ? string.Format("{0}|{1}|{2}", _spaceName, className, fieldName) : fieldName);
                }
            }
        }

        private void ChangePropertyName(TypeDefinition t)
        {
            // * 遍历属性s
            foreach (var property in t.Properties)
            {
                string _spaceName = t.Namespace;
                string className = t.Name;
                string propertyName = property.Name;

                if (IsChangeProperty(t, propertyName))
                {
                    property.Name = NameFactory.Instance.GetRandomName(NameType.Property, useFullNameMap ? string.Format("{0}|{1}|{2}", _spaceName, className, propertyName) : propertyName);
                }
            }
        }

        /// <summary>
        /// 修改方法名
        /// </summary>
        /// <param name="t"></param>
        private void ChangeMethodName(TypeDefinition t)
        {
            string _spaceName = t.Namespace;
            string className = t.Name;
            // * 遍历方法
            foreach (var method in t.Methods)
            {
                string methodName = method.Name;

                if (IsChangeMethod(t, method))
                    method.Name = NameFactory.Instance.GetRandomName(NameType.Method, useFullNameMap ? string.Format("{0}|{1}|{2}", _spaceName, className, methodName) : methodName);
            }
        }

        /// <summary>
        /// 修改类名.
        /// </summary>
        /// <param name="t"></param>
        private void ChangeClassName(TypeDefinition t)
        {
            string _spaceName = t.Namespace;
            string className = t.Name;

            if (IsChangeClass(t))
                t.Name = NameFactory.Instance.GetRandomName(NameType.Class, useFullNameMap ? string.Format("{0}|{1}", _spaceName, className) : className);
        }

        /// <summary>
        /// 修改名字空间名
        /// </summary>
        /// <param name="t"></param>
        private void ChangeNamespace(TypeDefinition t)
        {
            string spaceName = t.Namespace;

            if (IsChangeNamespace(t))
                t.Namespace = NameFactory.Instance.GetRandomName(NameType.Namespace, spaceName);

        }


    }
}


