using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Cooking.Stage
{
    public class EffectManager : MonoBehaviour
    {
        /// <summary>
        /// エフェクトプレハブのID
        /// </summary>
        public enum EffectPrefabID
        {
            Boiling,
            Foam_Break,
            Food_Grounded, Food_Jump,
            Point_Down, Point_UP,
            Seasoning, Seasoning_Hit, Stars,
            Splash
        }

        GameObject[] effects;

        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// このクラスのインスタンスを取得。
        /// </summary>
        public static EffectManager Instance //そのスクリプトのクラスの型に変更して使う
        {
            get { return _instance; }
        }
        static EffectManager _instance = null;
        /// <summary>
        /// Start()より先に実行
        /// </summary>
        private void Awake()
        {
            _instance = this;
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            effects = new GameObject[Enum.GetValues(typeof(EffectPrefabID)).Length];

            for (int i = 0; i < Enum.GetValues(typeof(EffectPrefabID)).Length; i++)
            {
                var effectType  = (EffectPrefabID)Enum.ToObject(typeof(EffectPrefabID), i);
                effects[i] = Resources.Load<GameObject>("Effects/" + effectType.ToString());
            }
        }

        // Update is called once per frame
        void Update()
        {
            
        }
        /// <summary>
        /// エフェクトの生成, 座標と種類の指定
        /// </summary>
        /// <param name="effectPosition">座標</param>
        /// <param name="effectPrefabID">エフェクトの種類</param>
        public Transform InstantiateEffect(Vector3 effectPosition , EffectPrefabID effectPrefabID)
        {
            var obj = Instantiate(effects[(int)effectPrefabID]);
            obj.transform.position = effectPosition;
            return obj.transform;
        }
    }
}
