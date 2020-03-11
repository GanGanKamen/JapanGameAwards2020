using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// 予測線を生成します。
    /// 回転に合わせて飛ばす先の地点を計算して、予測線を飛ばす地点を変更します。
    /// </summary>
    public class PredictLineController : MonoBehaviour
    {
        /// <summary>
        /// ショット前のカメラの回転は、ショットの向きを決めるオブジェクトのRotationのyを参照します(左右)。x(高さ方向の回転)は参照しません。
        /// ショット前のカメラ動作はmainカメラで行います。
        /// ショットオブジェクトは一つのみで、各プレイヤーで使いまわします。
        /// </summary>
        Shot shotObject;
        /// <summary>
        /// 予測線表示用のプレハブオブジェクトです。
        /// </summary>
        [SerializeField] GameObject predictObjectPrefab;
        /// <summary>
        /// 現在シーン上に存在している予測線オブジェクトが入ります。
        /// </summary>
        private List<PredictLine> predictLines = new List<PredictLine>();
        /// <summary>
        /// 予測線を飛ばす方向ベクトルです。
        /// </summary>
        private Vector3 predictLines_SpeedVector;
        /// <summary>
        /// 予測落下地点表示用プレハブオブジェクトです。
        /// </summary>
        [SerializeField] GameObject predictShotPointPrefab;
        /// <summary>
        /// 現在シーン上に存在している予測落下地点オブジェクトが入ります。
        /// </summary>
        private GameObject predictShotPoint;
        float predictTimeCount;
        /// <summary>
        /// 予測線が消えるまでの時間です。タイムカウンターは各predictlineが持ちます。
        /// </summary>
        [SerializeField] float destroyTime = 1f;
        [SerializeField] float predictTimeInterval = 0.1f;
        /// <summary>
        /// 表示される予測線の数です。予測線を表示する時間と、表示時間間隔によって決まります。
        /// </summary>
        private int displayCountOfpredictLines = 10;
        /// <summary>
        /// 削除するオブジェクトの番号です。古いものから順に削除します。
        /// </summary>
        int destroyIndex = 0;
        // Start is called before the first frame update
        void Start()
        {
            predictShotPoint = Instantiate(predictShotPointPrefab);
            shotObject = FindObjectOfType<Shot>();
            //割り切れる数字である想定です。
            displayCountOfpredictLines = Mathf.RoundToInt(destroyTime / predictTimeInterval);
            Debug.Log(displayCountOfpredictLines);
        }

        void FixedUpdate()
        {
            for (int i = 0; i < predictLines.Count; i++)
            {
                //完全にプログラム計算形式にすることも可能です。
                var predictRb = predictLines[i].predictLineRigidbody;
                var velocity = predictRb.velocity;
                velocity.y -= 9.81f * GameManager.Instance.gravityScale * Time.deltaTime;
                predictRb.velocity = velocity;
            }
        }
        // Update is called once per frame
        void Update()
        {
            //予測線を飛ばす方向を取得します。
            predictLines_SpeedVector = shotObject.transform.forward * 20;

            //予測線を管理します。
            if (!shotObject.IsShot)
                PredictLineManage();

            #region 予測落下地点を計算します。
            //距離(座標) = v0(初速度ベクトル) * 時間 + 1/2 * 重力加速度 * (時間)^2 //物体の大きさの分だけわずかにずれます 現在0.5fから発射
            // 0 = initialSpeedVector.y * t -1/2 *  9.81f * gravityScale * t * t
            // t ≠ 0 より tで割ると
            //1/2 * 9.81f * gravityScale * t  =  initialSpeedVector.y
            //t  =  initialSpeedVector.y /9.81f * gravityScale
            //滞空時間を算出します。座標 y = 0 に戻ってくるまでにかかる時間です。
            float t = predictLines_SpeedVector.y / (0.5f * 9.81f * GameManager.Instance.gravityScale);
            //Debug.Log(t);
            //その時間ぶんだけxz平面上で初速のxzベクトル方向に等速直線運動させて、その運動が終わった地点を落下予測地点とします。ただし、落下地点は高さ0とします。
            Vector3 fallPoint = new Vector3(predictLines_SpeedVector.x, 0, predictLines_SpeedVector.z) * t;
            predictShotPoint.transform.position = fallPoint;
            #endregion

            //落下地点の変更を予測線にも伝えます。
            ChangePredictFallPointOnXZ();
        }

        /// <summary>
        /// 予測線を管理します。予測線の生成・変更・削除を行います。
        /// </summary>
        private void PredictLineManage()
        {
            if (predictTimeCount >= predictTimeInterval)
            {
                predictTimeCount = 0;
                PredictLine predictLine = InstantiatePredictLine();
                ManagePredictLinesList(predictLine);
            }
            else
                predictTimeCount += Time.deltaTime;

            for (int i = 0; i < predictLines.Count; i++)
            {
                predictLines[i].destroyTimeCounter += Time.deltaTime;
            }
        }
        /// <summary>
        /// 予測線の動的配列を管理します。予測線の削除も行います。
        /// </summary>
        /// <param name="predictLine"></param>
        private void ManagePredictLinesList(PredictLine predictLine)
        {
            if (predictLines.Count < displayCountOfpredictLines)
                predictLines.Add(predictLine);
            else
            {
                if (predictLines[destroyIndex].destroyTimeCounter >= destroyTime)
                {
                    Destroy(predictLines[destroyIndex].gameObject);
                    //predictLines.RemoveAt(destroyIndex);
                    predictLines[destroyIndex] = predictLine;
                    if (destroyIndex < displayCountOfpredictLines)
                    {
                        destroyIndex++;
                        if (destroyIndex == displayCountOfpredictLines)
                        {
                            destroyIndex = 0;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 予測線を生成します。
        /// </summary>
        /// <returns></returns>
        private PredictLine InstantiatePredictLine()
        {
            var obj = Instantiate(predictObjectPrefab);
            var turnController = TurnController.Instance;
            obj.transform.position
                = turnController.foodStatuses[turnController.ActivePlayerIndex].transform.position; //+ new Vector3(0, 0, 0);
            var predictLine = obj.GetComponent<PredictLine>();
            predictLine.predictLineRigidbody.velocity = predictLines_SpeedVector;
            obj.transform.parent = this.transform;
            return predictLine;
        }
        /// <summary>
        /// 落下予測地点の変更を予測線を描くオブジェクトに対して渡します。XZ平面方向の変更です。
        /// </summary>
        private void ChangePredictFallPointOnXZ()
        {
            for (int i = 0; i < predictLines.Count; i++)
            {
                //完全にプログラム計算形式にすることも可能です。
                var predictRb = predictLines[i].predictLineRigidbody;
                var velocity = predictRb.velocity;
                velocity.x = predictLines_SpeedVector.x;
                velocity.z = predictLines_SpeedVector.z;
                predictRb.velocity = velocity;
            }
        }
    }

}
