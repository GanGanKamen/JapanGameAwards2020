using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    /// <summary>列挙型に変換して成功したかどうかを返し 例外処理を警告するメソッドのクラス</summary>
    public class EnumParseMethod : MonoBehaviour
    {
        /// <summary>
        /// 指定された文字列を列挙型に変換して成功したかどうかを返す 例外処理警告ができるメソッドの実装
        /// </summary>
        /// <typeparam name="T">列挙型</typeparam>
        /// <param name="value">変換する文字列</param>
        /// <param name="ignoreCase">大文字と小文字を区別しない場合は true</param>
        /// <param name="result">列挙型のオブジェクト。ここに変換後の値が入る</param>
        /// <returns>正常に変換された場合は true</returns>
        public static bool TryParseAndDebugAssertFormat<T>(string value, bool ignoreCase, out T result) where T : struct , IConvertible , IFormattable
        {
            //enum型へ変換 + 変換失敗時に警告
            if (!Enum.TryParse(value, ignoreCase ,out result))
            {
                Debug.LogFormat("不適切なボタンの名前:{0}が入力されました。", value);
                result = default(T);//念のため代入
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
