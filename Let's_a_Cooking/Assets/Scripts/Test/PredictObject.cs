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
		//高さのベクトルを決めるオブジェクトのtransformを定義
		Transform shotAngleTransform;
		float gravityScale = 1;
		new Rigidbody rigidbody;
		public Vector3 initialSpeedVector;
		Vector3 velocity;
		float destroyTimeCounter;
		float destroyTime = 1f;
		// Start is called before the first frame update
		void Start()
		{
			//角度を決めるオブジェクトを取得
			shotAngleObj = GameObject.Find("ShotAngleObject");
			//角度を決めるオブジェクトのtransformを取得
			shotAngleTransform = shotAngleObj.GetComponent<Transform>();
			rigidbody = GetComponent<Rigidbody>();
			rigidbody.velocity = initialSpeedVector;
		}

		private void Update()
		{
			///デバッグ中は地面に触れたら消えるようにしたい。
			if (destroyTimeCounter >= destroyTime)
			{
				Destroy(this.gameObject);
				destroyTimeCounter = 0;
			}
			else
				destroyTimeCounter += Time.deltaTime;
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
	}
	//            今後必要そうなもの 落下地点は発射地点と同じ高さとは限らないため。
	//テクスチャの張替えorゾーンの生成→その二つを同時に行う・着地予想地点から飛来してきた方向の逆にレイ飛ばし・着弾予測地点にオブジェクトをおく
}