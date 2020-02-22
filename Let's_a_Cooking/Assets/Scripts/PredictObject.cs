using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    public class PredictObject : MonoBehaviour
    {
        //予測オブジェクトが消えるまでのカウンター
        float desteoyCounter = 0;
        /// <summary>
        /// 予測線表示時間間隔です。
        /// </summary>
        public float predictObjInterval;
        //高さのベクトルを決めるオブジェクトを取得
        GameObject shotAngleObj;
        [SerializeField]
        GameObject predictShotPoint;
        //高さのベクトルを決めるオブジェクトのtransformを定義
        Transform shotAngleTransform;
        GameObject point;
        float gravityScale = 1;
        new Rigidbody rigidbody;
        Vector3 initialSpeedVector;
        Vector3 velocity;
        // Start is called before the first frame update
        void Start()
        {
            //角度を決めるオブジェクトを取得
            shotAngleObj = GameObject.Find("ShotAngleObject");
            //角度を決めるオブジェクトのtransformを取得
            shotAngleTransform = shotAngleObj.GetComponent<Transform>();
            rigidbody = GetComponent<Rigidbody>();
            initialSpeedVector = shotAngleTransform.transform.forward * 20;
            //rigidbody.AddForce(initialSpeedVector, ForceMode.Impulse);
            rigidbody.velocity = initialSpeedVector;
            //距離(座標) = v0(初速度ベクトル) * 時間 + 1/2 * 重力加速度 * (時間)^2 //物体の大きさの分だけわずかにずれます 現在0.5fから発射
            // 0 = initialSpeedVector.y * t -1/2 *  9.81f * gravityScale * t * t
            // t ≠ 0 より tで割ると
            //1/2 * 9.81f * gravityScale * t  =  initialSpeedVector.y
            //t  =  initialSpeedVector.y /9.81f * gravityScale
            //滞空時間を算出します。座標 y = 0 に戻ってくるまでにかかる時間です。
            float t = initialSpeedVector.y / (0.5f * 9.81f * gravityScale);
            Debug.Log(t);
            //その時間ぶんだけxz平面上で初速のxzベクトル方向に等速直線運動させて、その運動が終わった地点を落下予測地点とします。ただし、落下地点は高さ0とします。
            Vector3 fallPoint = new Vector3(initialSpeedVector.x, 0, initialSpeedVector.z) * t;
            point = Instantiate(predictShotPoint);
            point.transform.position = fallPoint;
        }
        // Update is called once per frame
        void FixedUpdate()
        {
            velocity = rigidbody.velocity;
            //shotAngleTransform.transform.forward  = direction: xyzの方向ベクトル。オブジェクト生成時に決まります。   20 : 初速度　あとはy成分の加速度 減速するのはy方向のみ -9.81(重力加速度) * m(質量 仮に1 係数として調整)
            //velocity = direction * 20;
            velocity.y -= 9.81f * gravityScale * Time.deltaTime;
            rigidbody.velocity = velocity;
        }
        private void OnDestroy()
        {
            Destroy(point);
        }
    }
    //            今後必要そうなもの 落下地点は発射地点と同じ高さとは限らないため。
    //テクスチャの張替えorゾーンの生成→その二つを同時に行う・着地予想地点から飛来してきた方向の逆にレイ飛ばし・着弾予測地点にオブジェクトをおく
}