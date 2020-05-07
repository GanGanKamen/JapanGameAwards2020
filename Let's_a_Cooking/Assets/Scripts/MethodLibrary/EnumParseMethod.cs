using System;
using UnityEngine;

namespace Cooking
{
    /// <summary>列挙型に変換して成功したかどうかを返し 例外処理を警告するメソッドのクラス</summary>
    public static class EnumParseMethod
    {
        /// <summary>
        /// 指定された文字列を列挙型に変換して成功したかどうかを返す 例外処理警告ができるメソッド
        /// </summary>
        /// <typeparam name="T">列挙型</typeparam>
        /// <param name="value">変換する文字列</param>
        /// <param name="ignoreCase">大文字と小文字を区別しない場合は true</param>
        /// <param name="result">ここに変換後の列挙型の値が入る</param>
        /// <returns>正常に変換された場合は true</returns>
        public static bool TryParseAndDebugAssertFormat<T>(string value, bool ignoreCase, out T result) where T : struct , IConvertible , IFormattable
        {
            //enum型へ変換 + 変換失敗時に警告
            if (!Enum.TryParse(value, ignoreCase ,out result))
            {
                Debug.LogFormat("不適切なボタンの名前:{0}が入力されました。", value);
                result = default(T);
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 指定された文字列を列挙型に変換して返還後の値を返す さらに例外処理警告ができるメソッド
        /// </summary>
        /// <typeparam name="T">列挙型</typeparam>
        /// <param name="value">変換する文字列</param>
        /// <param name="ignoreCase">大文字と小文字を区別しない場合は true</param>
        /// <param name="enumValue">列挙型変数 仮の値を入れて渡す必要がある</param>
        /// <returns></returns>
        public static T TryParseAndDebugAssertFormatAndReturnResult<T>(string value, bool ignoreCase, T enumValue) where T : struct , IConvertible , IFormattable
        {
            //enum型へ変換 + 変換失敗時に警告
            if (!Enum.TryParse(value, ignoreCase ,out enumValue))
            {
                Debug.LogFormat("不適切なボタンの名前:{0}が入力されました。", value);
                enumValue = default(T);
                return enumValue;
            }
            else
            {
                return enumValue;
            }
        }
    }
}
