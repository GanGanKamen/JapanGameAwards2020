using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    public class ComponentCheck : MonoBehaviour
    {
        /// <summary>
        /// そのオブジェクトに必要なコンポーネントが存在するか確認 その後オブジェクト削除またはコンポーネントを加える
        /// </summary>
        /// <typeparam name="T">チェックしたいコンポーネント</typeparam>
        /// <param name="myself">呼び出し元クラスの参照</param>
        /// <param name="isDestroyObject">指定したコンポーネントがなかった時に、ゲームオブジェクトを削除するかどうか</param>
        public static void CheckNecessaryCopmonent<T>(Component myself , bool isDestroyObject) where T : Component
        {
            if (myself.GetComponent<T>() == null)
            {
                Debug.LogFormat("必要なコンポーネントがありません");
                if (isDestroyObject)
                {
                    Debug.LogFormat("不正とみなし{0}オブジェクトを削除",myself);
                    Destroy(myself.gameObject);
                }
                else
                {
                    Debug.LogFormat("{0}にコンポーネントを加えました。", myself);
                    myself.gameObject.AddComponent<T>();
                }
            }
        }
    }
}
