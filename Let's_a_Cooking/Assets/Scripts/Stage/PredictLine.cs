using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// 予測線の動きを制御します。
    /// </summary>
    public class PredictLine : MonoBehaviour
    {
        //予測オブジェクトが消えるまでのカウンター
        float desteoyCounter = 0;
        //高さのベクトルを決めるオブジェクトを取得
        GameObject shotAngleObj;
        //高さのベクトルを決めるオブジェクトのtransformを定義
        Transform shotAngleTransform;
        /// <summary>
        /// PredictLineControllerに制御されます。pria
        /// </summary>
        public Rigidbody predictLineRigidbody
        {
            get { return rigidbody; }
            ///大きすぎる値が入らないように制御可能です。
            set
            {
                rigidbody = value;
            }
        }
        private new Rigidbody rigidbody;

        public Vector3 initialSpeedVector;
        Vector3 velocity;
        public float destroyTimeCounter = 0;
        // Start is called before the first frame update
        void OnEnable()
        {
            rigidbody = GetComponent<Rigidbody>();
            //rigidbody.velocity = initialSpeedVector;
        }

        private void Update()
        {
            //Debug.Log(destroyTimeCounter);
        }

    }
    //            今後必要そうなもの 落下地点は発射地点と同じ高さとは限らないため。
    //テクスチャの張替えorゾーンの生成→その二つを同時に行う・着地予想地点から飛来してきた方向の逆にレイ飛ばし・着弾予測地点にオブジェクトをおく
}

