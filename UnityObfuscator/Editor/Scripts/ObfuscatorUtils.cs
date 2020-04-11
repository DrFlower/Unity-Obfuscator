using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flower.UnityObfuscator
{
    internal static class ObfuscatorUtils
    {
        static System.Random random;


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
    }
}

