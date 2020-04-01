using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// 配列用メソッドを用意
    /// </summary>
    public static class ArrayMethod
    {
        /// <summary>
        /// 配列の要素を一つ先の要素と入れ替えて降順にする　floatやComponentを入れ替えることを想定
        /// </summary>
        /// <typeparam name="T">floatやComponent</typeparam>
        /// <param name="values"></param>
        /// <param name="index"></param>
        public static void ChangeArrayValuesFromHighToLow<T>(T[] values, int index)
        {
            var tempValue = values[index];
            values[index] = values[index + 1];
            values[index + 1] = tempValue;
        }
    }
}
