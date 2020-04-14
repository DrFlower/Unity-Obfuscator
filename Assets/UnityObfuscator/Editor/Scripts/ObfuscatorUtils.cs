using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Flower.UnityObfuscator
{
    internal static class ObfuscatorHelper
    {
        static System.Random random;

        static HashSet<string> set = new HashSet<string>();


        public static void Init(int randomSeed)
        {
            random = new System.Random(randomSeed);
        }

        static public System.Random ObfuscateRandom
        {
            get
            {
                return random;
            }
        }

        public static string GetANameFromRandomChar()
        {
            string result = "init";

            int max = 1000;
            int current = 0;

            do
            {
                int len = random.Next(Const.minRandomNameLen, Const.maxRandomNameLen);
                StringBuilder sb = new StringBuilder(len);
                for (int i = 0; i < len; i++)
                {
                    int index = random.Next(0, Const.randomCharArray.Length);
                    sb.Append(Const.randomCharArray[index]);

                }
                result = sb.ToString();

                current++;
                if (current > max)
                    throw new System.Exception("Not enough random name");
            }
            while (set.Contains(result));

            set.Add(result);

            return result;
        }


    }
}

