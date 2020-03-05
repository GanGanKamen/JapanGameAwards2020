using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Cooking.Stage
{
	public class ChaseCamera : MonoBehaviour
	{
		//高さのベクトルを決めるオブジェクトを取得
		GameObject shotAngleObj;
		//高さのベクトルを決めるオブジェクトのtransformを定義
		Transform shotAngleTransform;
		//パワー調整時の加減に使うスイッチ
		bool powerUp = true;
		new Rigidbody rigidbody;
		//食材を打つ力を定義
		public float shotPower;
		//カメラの回転に使う変数の定義
		float X_Rot, Y_Rot;
		//食材を取得
		private GameObject food;
		Rigidbody rb;
		//shotPowerゲージを取得
		[SerializeField]
		private Slider powerGage;
		//発射方向の角度の制御に使う変数の定義
		float eulerX = 0;
		/// <summary>
		/// 予測線表示用のオブジェクトです。
		/// </summary>
		[SerializeField] GameObject predictObj;
		PredictObject predictObjectType;
		List<GameObject> tempPredictObj = new List<GameObject>();
		bool predictShootFlag = false;
		int a = 0;
		[SerializeField] float predictTimeInterval = 0.5f;
		float predictTimeCount;
		float gravityScale = 1;
		[SerializeField]
		GameObject predictShotPoint;
		GameObject point;
		bool isShot;
		// Start is called before the first frame update
		void Start()
		{
			//Debug.Log(a);
			a = 1;
			//食材を取得
			food = GameObject.Find("food");
			//食材のRigidbodyを取得
			rb = food.GetComponent<Rigidbody>();
			//角度を決めるオブジェクトを取得
			shotAngleObj = GameObject.Find("ShotAngleObject");
			//角度を決めるオブジェクトのtransformを取得
			shotAngleTransform = shotAngleObj.GetComponent<Transform>();
			point = Instantiate(predictShotPoint);
		}
		// Update is called once per frame
		void Update()
		{
			//Debug.Log(a);
			//左右キーのInput判定
			Y_Rot = Input.GetAxis("Horizontal");
			X_Rot = Input.GetAxis("Vertical");
			//if ((X_Rot == 0 && Y_Rot == 0) && !predictShootFlag)
			{
				//PredictStart();
				//predictShootFlag = true;
			}
			//else if ((X_Rot != 0 || Y_Rot != 0) && predictShootFlag)
			{
				//Destroy(tempPredictObj[0]);
				//tempPredictObj.RemoveAt(0);
				//PredictStart();
				//predictObjInterval = 0;
				//predictShootFlag = false;
			}
			//else if (X_Rot != 0 || Y_Rot != 0)
			//{
			//    //tempPredictObj = null;
			//    //PredictStart();
			//    destroyTimeCounter += Time.deltaTime;
			//}
			//Debug.Log(shotAngleTransform.transform.forward);
			//if (predictShootFlag)
			//{
			//    if (predictObjectType.predictObjInterval >= 0.1f)
			//    {
			//        PredictObjStop();
			//    }
			//    else
			//    {
			//        predictObjectType.predictObjInterval += Time.deltaTime;
			//    }
			//}
			//カメラのY軸回転
			transform.Rotate(0, Y_Rot * 2, 0);
			//力の方向を決める
			shotAngleTransform.transform.Rotate(-X_Rot, 0, 0);
			//shotPowerの加減判定
			if (shotPower <= 5)
			{
				powerUp = true;
			}
			else if (shotPower >= 20)
			{
				powerUp = false;
				shotPower = 20;
			}
			//Debug.Log(shotPower);
			//shotPowerをゲージに反映
			powerGage.value = shotPower;
			//左クリック中に呼び出される
			if (Input.GetMouseButton(0))
			{
				if (powerUp)
				{
					shotPower += 30 * Time.deltaTime / Time.timeScale;
				}
				else if (!powerUp)
				{
					shotPower -= 30 * Time.deltaTime / Time.timeScale;
				}
			}
			eulerX = shotAngleTransform.transform.rotation.eulerAngles.x;
			//発射方向の角度の制御
			if (shotAngleTransform.transform.rotation.eulerAngles.x <= 300 && shotAngleTransform.transform.rotation.eulerAngles.x >= 200)
			{
				eulerX = 300;
			}
			if (eulerX >= 0 && eulerX <= 30)
			{
				eulerX = 0;
			}
			shotAngleTransform.transform.eulerAngles = new Vector3(eulerX, shotAngleTransform.transform.rotation.eulerAngles.y, shotAngleTransform.transform.rotation.eulerAngles.z);
			//左クリックされた時に呼び出される
			if (Input.GetMouseButtonUp(0))
			{
				Time.timeScale = 1;
				//食材に力を加える処理
				rb.AddForce(shotAngleTransform.transform.forward * shotPower, ForceMode.Impulse);

			}
			else if (Input.GetKeyDown(KeyCode.Space))
			{
				rb.AddForce(shotAngleTransform.transform.forward * 20, ForceMode.Impulse);
				isShot = true;
			}

			var initialSpeedVector = shotAngleTransform.transform.forward * 20;
			if (!isShot)
			{
				if (predictTimeCount >= predictTimeInterval)
				{
					predictTimeCount = 0;
					PredictStart(initialSpeedVector);
				}
				else
					predictTimeCount += Time.deltaTime;
			}

			//rigidbody.AddForce(initialSpeedVector, ForceMode.Impulse);
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
			point.transform.position = fallPoint;
		}
		private void PredictObjStop()
		{
			rigidbody.velocity = Vector3.zero;
			rigidbody.isKinematic = true;
		}
		private void PredictStart(Vector3 initialSpeedVector)
		{
			var obj = Instantiate(predictObj);
			tempPredictObj.Add(obj);
			tempPredictObj[tempPredictObj.Count - 1].transform.position = food.transform.position; //+ new Vector3(0, 0, 0);
			obj.GetComponent<PredictObject>().initialSpeedVector = initialSpeedVector;
			obj.transform.parent = this.transform;
			//shotAngleObj.transform.GetChild(0);
			//rigidbody = tempPredictObj[tempPredictObj.Count - 1].GetComponent<Rigidbody>();
			//predictObjectType = tempPredictObj[tempPredictObj.Count - 1].GetComponent<PredictObject>();
			//rigidbody.AddForce(shotAngleTransform.transform.forward * 20, ForceMode.Impulse);
			Time.timeScale = 1;
		}
		void FixedUpdate()
		{
		}
	}
}