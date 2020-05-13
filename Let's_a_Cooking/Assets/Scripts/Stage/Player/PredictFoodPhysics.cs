using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// 食材の動きを予測する
    /// </summary>
    public class PredictFoodPhysics : MonoBehaviour
    {
        /// <summary>
        /// レイキャストにより算出される落下地点に至るまでの時間
        /// </summary>
        public static float FallTime
        {
            get { return _fallTime; }
        }
        private static float _fallTime = 0;
        /// <summary>
        /// レイキャストによる落下地点の予測 見つからないときは0ベクトルまたはnull
        /// </summary>
        /// <typeparam name="T">Vector3 または GameObject 落下地点の欲しい情報を指定</typeparam>
        /// <param name="predictStartPoint">予測開始地点</param>
        /// <param name="firstSpeedVector">初速度ベクトル</param>
        /// <returns></returns>
        public static T PredictFallPointByRayCast <T> (Vector3 predictStartPoint, Vector3 firstSpeedVector)
        {
            int i = 1;//累積誤差発生を防ぐためのインクリメント変数
            //レイによる落下地点予測 無限ループ防止目的で 滞空時間100秒制限
            for (float flyTime = 0f; flyTime < 30; i++)
            {
                Vector3 originPoint = predictStartPoint + new Vector3(firstSpeedVector.x * flyTime, CalculateYposition(firstSpeedVector, flyTime), firstSpeedVector.z * flyTime);
                flyTime = i / 200f; //累積誤差の発生を防ぐ 精度を決める変数 精度上げると処理が重い
                Vector3 endPoint = predictStartPoint + new Vector3(firstSpeedVector.x * flyTime, CalculateYposition(firstSpeedVector, flyTime), firstSpeedVector.z * flyTime);
                //終点 - 始点で方向ベクトルを算出
                var direction = endPoint - originPoint;
                //レイを可視化
                Debug.DrawRay(originPoint, direction, Color.red, 0.2f);
                if (typeof(T) == typeof(GameObject))
                {
                    GameObject FallPointGameObject = GetGameObjectByCastRayOnKitchen(originPoint, direction);
                    if (FallPointGameObject != null)
                    {
                        _fallTime = flyTime;
                        return (T)(object)FallPointGameObject;
                    }
                    if (flyTime > 29.9f)
                    {
                        Debug.Log("当たっていない");
                    }
                }
                else if (typeof(T) == typeof(Vector3))
                {
                    //レイを飛ばして当たった場所を保存
                    Vector3 fallPoint = CastRayOnKitchen(originPoint, direction);
                    if (fallPoint != Vector3.zero)
                    {
                        _fallTime = flyTime;
                        return (T)(object)fallPoint;
                    }
                    if (flyTime > 29.9f)
                    {
                        Debug.Log("当たっていない");
                    }
                }
                else
                {
                    Debug.LogAssertion("指定した型が想定と異なります");
                }
            }
            if (typeof(T) == typeof(GameObject))
            {
                return (T)(object)null;
            }
            else if (typeof(T) == typeof(Vector3))
            {
                return (T)(object)Vector3.zero;
            }
            else
            {
                Debug.LogAssertion("指定した型が想定と異なります");
                return (T)(object)null;
            }
        }
        /// <summary>
        /// 最初に衝突した時の跳ねる方向の速度ベクトル(理論値)を算出(OnCollisionEnterでは食材の回転で実行値が安定しなかったため)
        /// </summary>
        /// <param name="firstSpeedVector">初速</param>
        /// <returns></returns>
        public static Vector3 PredictFirstBoundSpeedVecor(Vector3 firstSpeedVector)
        {
            var SpeedVectorYDelta = -StageSceneManager.Instance.gravityAccelerationValue * _fallTime;
            return new Vector3(firstSpeedVector.x , firstSpeedVector.y + SpeedVectorYDelta, firstSpeedVector.z );
        }

        #region privateメソッド
        /// <summary>
        /// 任意の滞空時間におけるy座標を計算 座標 = v0(初速度ベクトル) * 時間 + 1/2 * 重力加速度 * (時間)^2
        /// </summary>
        /// <param name="firstSpeedVector"></param>
        /// <param name="flyTime"></param>
        /// <returns></returns>
        private static float CalculateYposition(Vector3 firstSpeedVector, float flyTime)
        {
            return (firstSpeedVector.y * flyTime - 0.5f * StageSceneManager.Instance.gravityAccelerationValue * flyTime * flyTime);
        }
        /// <summary>
        /// レイを飛ばしてKitchenレイヤーに当たった座標を返す
        /// </summary>
        /// <param name="originPoint"></param>
        /// <param name="direction"></param>
        /// <returns>Kitchenレイヤーに当たったかどうか</returns>
        private static Vector3 CastRayOnKitchen(Vector3 originPoint, Vector3 direction)
        {
            ///レイの長さ
            float rayLength = direction.magnitude;
            //Rayが当たったオブジェクトの情報を入れる箱
            RaycastHit hit;    //原点        方向
            Ray ray = new Ray(originPoint, direction);
            //Kitchenレイヤーとレイ判定を行う
            if (Physics.Raycast(ray, out hit, rayLength, StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]))
            {
                return hit.point;
            }
            return Vector3.zero;
        }
        private static GameObject GetGameObjectByCastRayOnKitchen(Vector3 originPoint, Vector3 direction)
        {
            ///レイの長さ
            float rayLength = direction.magnitude;
            //Rayが当たったオブジェクトの情報を入れる箱
            RaycastHit hit;    //原点        方向
            Ray ray = new Ray(originPoint, direction);
            //Kitchenレイヤーとレイ判定を行う
            if (Physics.Raycast(ray, out hit, rayLength, StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]))
            {
                return hit.transform.gameObject;
            }
            return null;
        }
        #endregion
    }
}
