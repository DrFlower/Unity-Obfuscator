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
            private Dictionary<string, string> old_new_Dic = new Dictionary<string, string>();
            private Dictionary<string, string> new_old_Dic = new Dictionary<string, string>();

            private Random random;

            private NameType nameType;

            public NameType NameType
            {
                get
                {
                    return nameType;
                }
            }

            public Dictionary<string, string> Old_New_NameDic
            {
                get
                {
                    return old_new_Dic;
                }
            }

            public Dictionary<string, string> New_Old_NameDic
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

            public string GetAName(string oldName)
            {
                string newName = string.Empty;

                if (!old_new_Dic.TryGetValue(oldName, out newName))
                {
                    newName = GetAName();
                    old_new_Dic.Add(oldName, newName);
                    new_old_Dic.Add(newName, oldName);
                }

                return newName;
            }

            public string GetAName(bool removeFormLib = true)
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

        public void Load()
        {
            Random random = ObfuscatorUtils.ObfuscateRandom;

            NameCollectionDic = new Dictionary<NameType, NameCollection>();

            if (random == null)
            {
                Debug.LogError("NameFactory Init Fail: Random Param Is Null");
                return;
            }

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

        private NameFactory()
        {
            Load();
        }

        public string GetRandomName(NameType nameType, string oldName)
        {
            string newName = string.Empty;

            if (NameCollectionDic.ContainsKey(nameType))
            {
                newName = NameCollectionDic[nameType].GetAName(oldName);
            }

            return newName;
        }

        public string GetRandomName()
        {
            return NameCollectionDic[NameType.Other].GetAName(false);
        }

        public Dictionary<string, string> GetOld_New_NameDic(NameType nameType)
        {
            if (NameCollectionDic.ContainsKey(nameType))
                return NameCollectionDic[nameType].Old_New_NameDic;

            return null;
        }

        public Dictionary<string, string> GetNew_Old_NameDic(NameType nameType)
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
                    result += item.Key + " ===> " + item.Value + "\n";
                }
            }

            File.WriteAllText(path, result);
        }

    }
}
