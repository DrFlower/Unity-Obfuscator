using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Cecil;

namespace Flower.UnityObfuscator
{
    internal class ClassInfo
    {

        class FieldPair
        {
            private readonly FieldDefinition oldField;
            private readonly FieldDefinition newField;

            public FieldDefinition OldField
            {
                get
                {
                    return oldField;
                }
            }
            public FieldDefinition NewField
            {
                get
                {
                    return newField;
                }
            }
            public FieldPair(FieldDefinition oldf, FieldDefinition newf)
            {
                this.oldField = oldf;
                this.newField = newf;
            }

        }

        class MethodPair
        {
            private readonly MethodDefinition oldMethod;
            private readonly MethodDefinition newMethod;

            public MethodDefinition OldMethod
            {
                get
                {
                    return oldMethod;
                }
            }
            public MethodDefinition NewMethod
            {
                get
                {
                    return newMethod;
                }
            }
            public MethodPair(MethodDefinition oldm, MethodDefinition newm)
            {
                this.oldMethod = oldm;
                this.newMethod = newm;
            }
        }

        /// <summary>
        /// 类中的名字集合
        /// </summary>
        public HashSet<string> nameSet;
        /// <summary>
        /// 加入的名字
        /// </summary>
        public HashSet<string> joinName;
        public TypeDefinition typeDefinition;

        Dictionary<string, List<MethodPair>> joinMethodDict;
        Dictionary<string, List<FieldPair>> joinFieldDict;
        private readonly TypeDefinition _typeDefinition;

        public ClassInfo(TypeDefinition t)
        {
            typeDefinition = t;
            nameSet = new HashSet<string>();
            joinName = new HashSet<string>();
            joinMethodDict = new Dictionary<string, List<MethodPair>>();
            joinFieldDict = new Dictionary<string, List<FieldPair>>();
            GetClassNameInfo(t, nameSet);
        }

        /// <summary>
        /// 获取类的名字集合
        /// </summary>
        /// <param name="t"></param>
        /// <param name="nameSet"></param>
        private void GetClassNameInfo(TypeDefinition t, HashSet<string> nameSet)
        {
            if (t == null)
            {
                return;
            }
            nameSet.Add(t.Name);
            // * 遍历属性
            foreach (var prop in t.Properties)
            {
                nameSet.Add(prop.Name);
            }
            // * 遍历字段
            foreach (var field in t.Fields)
            {
                nameSet.Add(field.Name);
            }
            // * 遍历方法
            foreach (var method in t.Methods)
            {
                nameSet.Add(method.Name);
            }
        }

        /// <summary>
        /// 加入名字
        /// 会保证加入名字的唯一性
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string AddName(string name)
        {
            while (nameSet.Contains(name) || joinName.Contains(name))
            {
                name = GenName(name);
            }
            joinName.Add(name);
            return name;
        }

        /// <summary>
        /// 获取方法
        /// </summary>
        /// <param name="oldm">老方法</param>
        /// <param name="newm">该类创建的对应的新方法</param>
        /// <returns>是否有对应的新方法</returns>
        public bool GetMethod(MethodDefinition oldm, out MethodDefinition newm)
        {
            newm = null;
            List<MethodPair> _mpList;
            if (joinMethodDict.TryGetValue(oldm.FullName, out _mpList))
            {
                foreach (var item in _mpList)
                {
                    if (item.OldMethod == oldm)
                    {
                        newm = item.NewMethod;
                        break;
                    }
                }
            }
            return newm != null;
        }

        /// <summary>
        /// 加入方法
        /// </summary>
        /// <param name="oldm">老方法</param>
        /// <param name="newm">新方法</param>
        public void AddMehtod(MethodDefinition oldm, MethodDefinition newm)
        {
            List<MethodPair> _mpList;
            if (!joinMethodDict.TryGetValue(oldm.FullName, out _mpList))
            {
                _mpList = new List<MethodPair>();
            }
            _mpList.Add(new MethodPair(oldm, newm));
            joinMethodDict.Add(oldm.FullName, _mpList);
        }

        /// <summary>
        /// 获取变量
        /// </summary>
        /// <param name="oldf">老变量</param>
        /// <param name="newf">该类创建的对应的新变量</param>
        /// <returns>是否有对应的新方法</returns>
        public bool GetField(FieldDefinition oldf, out FieldDefinition newf)
        {
            newf = null;
            List<FieldPair> _fpList;
            if (joinFieldDict.TryGetValue(oldf.FullName, out _fpList))
            {
                for (int i = 0; i < _fpList.Count; i++)
                {
                    var item = _fpList[i];
                    if (item.OldField == oldf)
                    {
                        newf = item.NewField;
                        break;
                    }
                }
            }
            return newf != null;
        }
        /// <summary>
        /// 加入变量  
        /// </summary>
        /// <param name="oldf">老变量</param>
        /// <param name="newf">新变量</param>
        public void AddField(FieldDefinition oldf, FieldDefinition newf)
        {
            List<FieldPair> _fpList;
            if (!joinFieldDict.TryGetValue(oldf.FullName, out _fpList))
            {
                _fpList = new List<FieldPair>();
            }
            _fpList.Add(new FieldPair(oldf, newf));
            joinFieldDict.Add(oldf.FullName, _fpList);
        }

        /// <summary>
        /// 生成名字（在名字参数后加一串随机数字）
        /// </summary>
        /// <param name="name">名字</param>
        /// <returns></returns>
        private string GenName(string name)
        {
            return NameFactory.Instance.GetRandomName();
        }
    }
}

