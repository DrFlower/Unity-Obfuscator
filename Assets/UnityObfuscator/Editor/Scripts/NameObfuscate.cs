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
            foreach (var field in t.Fields)
            {
                string fieldName = field.Name;
                if (IsChangeField(t, fieldName))
                {
                    field.Name = NameFactory.Instance.GetRandomName(NameType.Filed, ObfuscateItemFactory.Create(field));
                }
            }
        }

        /// <summary>
        /// 修改属性名
        /// </summary>
        /// <param name="t"></param>
        private void ChangePropertyName(TypeDefinition t)
        {
            foreach (var property in t.Properties)
            {
                string propertyName = property.Name;

                if (IsChangeProperty(t, propertyName))
                {
                    property.Name = NameFactory.Instance.GetRandomName(NameType.Property, ObfuscateItemFactory.Create(property));
                }
            }
        }

        /// <summary>
        /// 修改方法名
        /// </summary>
        /// <param name="t"></param>
        private void ChangeMethodName(TypeDefinition t)
        {
            foreach (var method in t.Methods)
            {
                if (IsChangeMethod(t, method))
                    method.Name = NameFactory.Instance.GetRandomName(NameType.Method, ObfuscateItemFactory.Create(method));
            }
        }

        /// <summary>
        /// 修改类名.
        /// </summary>
        /// <param name="t"></param>
        private void ChangeClassName(TypeDefinition t)
        {
            if (IsChangeClass(t))
                t.Name = NameFactory.Instance.GetRandomName(NameType.Class, ObfuscateItemFactory.Create(t));
        }

        /// <summary>
        /// 修改名字空间名
        /// </summary>
        /// <param name="t"></param>
        private void ChangeNamespace(TypeDefinition t)
        {
            if (IsChangeNamespace(t))
                t.Namespace = NameFactory.Instance.GetRandomName(NameType.Namespace, ObfuscateItemFactory.Create(t.Namespace, t.Module));
        }


    }
}


