using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Mono.Cecil;

namespace Flower.UnityObfuscator
{
    internal class WhiteList
    {
        public static readonly char sperateChar = '|';
        public static readonly char nullChar = '*';

        private Dictionary<WhiteListType, List<string>> dic = new Dictionary<WhiteListType, List<string>>();

        private static List<string> injectMethod = new List<string>();

        private static HashSet<string> unityMethodWhiteList = new HashSet<string> { //函数黑名单
            #region MonoBehaviour Message
            "Awake",
            "FixedUpdate",
            "LateUpdate",
            "OnAnimatorIK",
            "OnAnimatorMove",
            "OnApplicationFocus",
            "OnApplicationPause",
            "OnApplicationQuit",
            "OnAudioFilterRead",
            "OnBecameInvisible",
            "OnBecameVisible",
            "OnCollisionEnter",
            "OnCollisionEnter2D",
            "OnCollisionExit",
            "OnCollisionExit2D",
            "OnCollisionStay",
            "OnCollisionStay2D",
            "OnConnectedToServer",
            "OnControllerColliderHit",
            "OnDestroy",
            "OnDisable",
            "OnDisconnectedFromServer",
            "OnDrawGizmos",
            "OnDrawGizmosSelected",
            "OnEnable",
            "OnFailedToConnect",
            "OnFailedToConnectToMasterServer",
            "OnGUI",
            "OnJointBreak",
            "OnJointBreak2D",
            "OnMasterServerEvent",
            "OnMouseDown",
            "OnMouseDrag",
            "OnMouseEnter",
            "OnMouseExit",
            "OnMouseOver",
            "OnMouseUp",
            "OnMouseUpAsButton",
            "OnNetworkInstantiate",
            "OnParticleCollision",
            "OnParticleTrigger",
            "OnPlayerConnected",
            "OnPlayerDisconnected",
            "OnPostRender",
            "OnPreCull",
            "OnPreRender",
            "OnRenderImage",
            "OnRenderObject",
            "OnSerializeNetworkView",
            "OnServerInitialized",
            "OnTransformChildrenChanged",
            "OnTransformParentChanged",
            "OnTriggerEnter",
            "OnTriggerEnter2D",
            "OnTriggerExit",
            "OnTriggerExit2D",
            "OnTriggerStay",
            "OnTriggerStay2D",
            "OnValidate",
            "OnWillRenderObject",
            "Reset",
            "Start",
            "Update",
            #endregion
        };

        public WhiteList(Dictionary<WhiteListType, string> configFilePathDic)
        {
            foreach (var item in configFilePathDic)
            {
                LoadWhiteList(item.Key, item.Value);
            }
            CheckWhiteListData();
        }

        private void LoadWhiteList(WhiteListType whiteListType, string path)
        {
            if (dic.ContainsKey(whiteListType))
            {
                Debug.LogError("Init White List Error: Key Duplicated");
                return;
            }
            else
            {
                string[] lines = File.ReadAllLines(Application.dataPath + path);
                dic.Add(whiteListType, new List<string>(lines));
            }
        }

        public void AddInjectMethod(List<string> list)
        {
            injectMethod.AddRange(list);
        }

        private void CheckWhiteListData()
        {
            Func<string, int, bool> check = (str, splitCount) =>
                {
                    return !(str.Contains(" ") || str.Split(sperateChar).Length != splitCount);
                };

            foreach (var item in dic)
            {
                switch (item.Key)
                {
                    case WhiteListType.NameSpace:
                        foreach (var str in item.Value)
                        {
                            if (!check(str, 1))
                                Debug.LogError("Check White List Data Error: " + item.Key.ToString() + ":" + str);
                        }
                        break;
                    case WhiteListType.Class:
                        foreach (var str in item.Value)
                        {
                            if (!check(str, 2))
                                Debug.LogError("Check White List Data Error: " + item.Key.ToString() + ":" + str);
                        }
                        break;
                    case WhiteListType.Method:
                        foreach (var str in item.Value)
                        {
                            if (!check(str, 3))
                                Debug.LogError("Check White List Data Error: " + item.Key.ToString() + ":" + str);
                        }
                        break;
                    case WhiteListType.Member:
                        foreach (var str in item.Value)
                        {
                            if (!check(str, 3))
                                Debug.LogError("Check White List Data Error: " + item.Key.ToString() + ":" + str);
                        }
                        break;
                }
            }
        }

        public bool IsWhiteListNamespace(string _namespace, bool checkEmpty = false)
        {
            if (string.IsNullOrEmpty(_namespace))
            {
                return checkEmpty;
            }
            string[] strs = _namespace.Split('.');
            return Check(strs, WhiteListType.NameSpace);
        }

        public bool IsWhiteListNamespcaeNameOnly(string _namespace, bool checkEmpty = false)
        {
            if (string.IsNullOrEmpty(_namespace))
            {
                return checkEmpty;
            }
            string[] strs = _namespace.Split('.');
            return Check(strs, WhiteListType.NameSpcaceNameOnly);
        }

        public bool IsWhiteListClass(string className, string _namespace = null)
        {

            if (string.IsNullOrEmpty(className))
            {
                Debug.LogError("Class Param Error:" + className);

                return false;
            }

            return Check(new string[] { (string.IsNullOrEmpty(_namespace) ? nullChar.ToString() : _namespace), className }, WhiteListType.Class);
        }

        public bool IsWhiteListClassNameOnly(string className, string _namespace = null)
        {

            if (string.IsNullOrEmpty(className))
            {
                Debug.LogError("Class Param Error:" + className);

                return false;
            }

            return Check(new string[] { (string.IsNullOrEmpty(_namespace) ? nullChar.ToString() : _namespace), className }, WhiteListType.ClassNameOnly);
        }

        public bool IsWhiteListMethod(string method, string className = null, string _namespace = null)
        {
            if (string.IsNullOrEmpty(method))
            {
                Debug.LogError("Method Param Error:" + method);

                return false;
            }

            string _namespaceStr = (string.IsNullOrEmpty(_namespace) ? nullChar.ToString() : _namespace);
            string _classNameStr = (string.IsNullOrEmpty(className) ? nullChar.ToString() : className);

            return Check(new string[] { _namespaceStr, _classNameStr, method }, WhiteListType.Method);
        }

        public bool IsWhiteListClassMember(string member, string className = null, string _namespace = null)
        {
            if (string.IsNullOrEmpty(member))
            {
                Debug.LogError("Member Param Error:" + member);

                return false;
            }

            return Check(new string[] { (string.IsNullOrEmpty(_namespace) ? nullChar.ToString() : _namespace), (className == null ? nullChar.ToString() : className), member }, WhiteListType.Member);
        }

        public static bool IsSkipMethod(MethodDefinition method)
        {
            string methodName = method.Name;

            if (string.IsNullOrEmpty(methodName))
            {
                return true;
            }
            else if (unityMethodWhiteList.Contains(methodName))
            {
                return true;
            }
            else
            {
                string _namespaceStr = (string.IsNullOrEmpty(method.DeclaringType.Namespace) ? nullChar.ToString() : method.DeclaringType.Namespace);
                string _classNameStr = (string.IsNullOrEmpty(method.DeclaringType.Name) ? nullChar.ToString() : method.DeclaringType.Name);

                string str = string.Format("{0}{1}{2}{3}{4}", _namespaceStr, sperateChar, _classNameStr, sperateChar, methodName);

                if (injectMethod.Contains(str))
                {
                    //Debug.Log(str);
                    return true;
                }
            }

            return false;
        }

        private bool Check(string[] strs, WhiteListType whiteListType)
        {
            if (!dic.ContainsKey(whiteListType))
                return false;

            string temp = string.Empty;
            for (int i = 0; i < strs.Length; i++)
            {
                temp += strs[i];
            }

            foreach (var item in dic[whiteListType])
            {
                string[] whiteList = item.Split(sperateChar);
                bool partCheck = true;


                for (int i = 0; i < whiteList.Length; i++)
                {
                    if (!(whiteList[i] == strs[i] || (whiteList[i] == nullChar.ToString() /*&& i < whiteList.Length - 1*/)))
                    {
                        partCheck = false;
                        break;
                    }

                    //System.Func<string, bool> func = (str) =>
                    //{
                    //    return (strs[0] == str && str == whiteList[0]);

                    //};


                    //if (whiteListType == WhiteListType.NameSpace)
                    //{
                    //    if (func("MalbersAnimations") || func("UnityEngine") || func("Ricimi") || func("XftWeapon"))
                    //    {
                    //        Debug.Log(i + "   " + temp + ":" + item + "--------" + strs[i] + ":" + whiteList[i] + "  " + partCheck.ToString());
                    //    }
                    //}
                }

                if (partCheck == true)
                    return true;
            }

            return false;
        }

    }

}
