using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using Random = System.Random;

namespace Flower.UnityObfuscator
{

    internal class NameFactory
    {
        private class NameCollection
        {
            private List<string> nameList = new List<string>();
            private Dictionary<BaseObfuscateItem, string> old_new_Dic = new Dictionary<BaseObfuscateItem, string>();
            private Dictionary<string, BaseObfuscateItem> new_old_Dic = new Dictionary<string, BaseObfuscateItem>();

            private Random random;

            public static ObfuscateNameType ObfuscateNameType { get; set; }

            private NameType nameType;

            public NameType NameType
            {
                get
                {
                    return nameType;
                }
            }

            public Dictionary<BaseObfuscateItem, string> Old_New_NameDic
            {
                get
                {
                    return old_new_Dic;
                }
            }

            public Dictionary<string, BaseObfuscateItem> New_Old_NameDic
            {
                get
                {
                    return new_old_Dic;
                }
            }

            public NameCollection(NameType nameType, ICollection<string> nameList, Random random)
            {
                this.nameType = NameType;
                this.nameList = new List<string>(nameList);
                this.random = random;
            }

            public bool AlreadHanveRandomName(BaseObfuscateItem obfuscateItem)
            {
                return old_new_Dic.ContainsKey(obfuscateItem);
            }

            public string GetAName(BaseObfuscateItem obfuscateItem)
            {
                string newName = string.Empty;

                if (!old_new_Dic.TryGetValue(obfuscateItem, out newName))
                {
                    newName = GetAName();
                    old_new_Dic.Add(obfuscateItem, newName);
                    new_old_Dic.Add(newName, obfuscateItem);
                }

                return newName;
            }

            public string GetAName(bool removeFormLib = true)
            {
                if (ObfuscateNameType == ObfuscateNameType.NameList)
                {
                    string newName = string.Empty;
                    if (nameList.Count <= 0)
                    {
                        throw new System.Exception("Not enough random name");
                    }

                    int index = random.Next(0, nameList.Count);
                    newName = nameList[index];
                    if (removeFormLib)
                        nameList.Remove(newName);
                    return newName;
                }
                else
                {
                    return ObfuscatorHelper.GetANameFromRandomChar();
                }
            }

        }


        private Dictionary<NameType, NameCollection> NameCollectionDic = new Dictionary<NameType, NameCollection>();

        private static NameFactory _instance;

        public static NameFactory Instance
        {
            get
            {
                return _instance;
            }
        }

        static NameFactory()
        {
            _instance = new NameFactory();
        }

        public void Load(ObfuscateNameType obfuscateNameType)
        {
            Random random = ObfuscatorHelper.ObfuscateRandom;

            NameCollectionDic = new Dictionary<NameType, NameCollection>();

            if (random == null)
            {
                Debug.LogError("NameFactory Init Fail: Random Param Is Null");
                return;
            }

            if (obfuscateNameType == ObfuscateNameType.NameList)
            {
                string[] strs = File.ReadAllLines(Application.dataPath + "/" + Const.NameListPath);
                int index = 0;

                int namesCountPerType = strs.Length / Enum.GetValues(typeof(NameType)).Length;

                foreach (NameType v in Enum.GetValues(typeof(NameType)))
                {
                    List<string> list = new List<string>();
                    for (int i = index; i < index + namesCountPerType; i++)
                    {
                        list.Add(strs[i]);
                    }
                    index += namesCountPerType;

                    NameCollectionDic.Add(v, new NameCollection(v, list, random));
                }
            }
            else
            {
                foreach (NameType v in Enum.GetValues(typeof(NameType)))
                {
                    List<string> list = new List<string>();
                    NameCollectionDic.Add(v, new NameCollection(v, list, random));
                }
            }

            NameCollection.ObfuscateNameType = obfuscateNameType;

        }

        private NameFactory()
        {

        }

        public bool AlreadyHaveRandomName(NameType nameType, BaseObfuscateItem obfuscateItem)
        {
            bool result = false;
            if (NameCollectionDic.ContainsKey(nameType))
            {
                result = NameCollectionDic[nameType].AlreadHanveRandomName(obfuscateItem);
            }

            return result;
        }

        public string GetRandomName(NameType nameType, BaseObfuscateItem obfuscateItem)
        {
            string newName = string.Empty;

            if (NameCollectionDic.ContainsKey(nameType))
            {
                newName = NameCollectionDic[nameType].GetAName(obfuscateItem);
            }

            return newName;
        }

        public string GetRandomName()
        {
            return NameCollectionDic[NameType.Other].GetAName(false);
        }

        public Dictionary<BaseObfuscateItem, string> GetOld_New_NameDic(NameType nameType)
        {
            if (NameCollectionDic.ContainsKey(nameType))
                return NameCollectionDic[nameType].Old_New_NameDic;

            return null;
        }

        public Dictionary<string, BaseObfuscateItem> GetNew_Old_NameDic(NameType nameType)
        {
            if (NameCollectionDic.ContainsKey(nameType))
                return NameCollectionDic[nameType].New_Old_NameDic;

            return null;
        }

        public void OutputNameMap(string path)
        {
            string result = string.Empty;
            string str = "-------------";


            foreach (var collection in NameCollectionDic)
            {
                if (collection.Key == NameType.Other) continue;

                result += str + collection.Key.ToString() + str + "\n";
                foreach (var item in collection.Value.Old_New_NameDic)
                {
                    result += item.Key.Name + " ===> " + item.Value + "\n";
                }
            }

            File.WriteAllText(path, result);
        }

    }
}
