﻿namespace Cooking
{
    public static class Random
    {
        /// <summary>
        /// 最小値0から指定した範囲のint乱数発生 指定した数字は含まない
        /// </summary>
        /// <param name="rangeOfSeedFromZero"></param>
        /// <returns></returns>
        public static int GetRandomInt(int rangeOfSeedFromZero)
        {
            return UnityEngine.Random.Range(0, rangeOfSeedFromZero);
        }
        /// <summary>
        /// 最小値から最大値の間でfloat乱数発生
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float GetRandomFloat(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

    }
}
