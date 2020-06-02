using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// シングルトンにしたいコンポーネントクラスに継承させメソッド実行
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonInstance <T> : MonoBehaviour  where T : Component 
    {
        /// <summary>
        /// このクラスのインスタンスを取得
        /// </summary>
        public static T Instance
        {
            get
            {
                return _instance;
            }
        }
        private static T _instance = null;

        /// <summary>
        /// CreateSingletonInstance()を実行しシングルトンインスタンスを必ず生成する 実装を強制
        /// </summary>
        protected abstract void Awake();

        /// <summary>
        /// シングルトンインスタンスの生成
        /// </summary>
        /// <param name="singletonInstance">シングルトンにするコンポーネントクラス</param>
        /// <param name="isDontDestroyOnLoadGameObject">シーンをまたぐオブジェクトかどうか</param>
        protected void CreateSingletonInstance(T singletonInstance , bool isDontDestroyOnLoadGameObject)
        {
            // 初回作成時
            if (_instance == null)
            {
                _instance = singletonInstance;
                if (isDontDestroyOnLoadGameObject)
                {
                    // シーンをまたいで削除されないように設定
                    DontDestroyOnLoad(gameObject);
                }
            }
            // 2個目以降の作成時 この部分を間違いなく実装可能
            else
            {
                Debug.LogAssertion("シングルトンインスタンスである" + singletonInstance + "がシーン内に複数存在したため削除");
                Destroy(gameObject);
            }
        }
        protected virtual void Start()
        {
            if (_instance == null)
            {
                Debug.Log("Awakeにて、CreateSingletonInstanceメソッドを呼んでください");
            }
        }
    }
}
