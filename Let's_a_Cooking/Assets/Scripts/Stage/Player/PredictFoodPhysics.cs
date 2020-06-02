using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cooking.Stage
{
    /// <summary>
    /// 食材の動きを予測する
    /// </summary>
    public class PredictFoodPhysics : MonoBehaviour
    {
        static Vector3 _fallPoint = Vector3.zero, _boxColliderSize = new Vector3(10, 10, 10) * 2;
        struct CapsuleColliderSizeInformation
        {
            public Vector3 capuleColliderCenter;
            public float radius;
            public Vector3 distanceVector;
        }
        /// <summary>
        /// struct CapsuleColliderSizeInformation
        /// </summary>
        static CapsuleColliderSizeInformation _capsuleColliderSizeInformation;
        static FoodType _foodType = FoodType.Shrimp;
        /// <summary>
        /// シーン内に食材着地エリアを描画するためのオブジェクトを生成
        /// </summary>
        public static void CreatePredictFoodGroundedGameObject()
        {
            var obj = new GameObject("PredictFoodPhysics");
            obj.AddComponent<PredictFoodPhysics>();
        }
        /// <summary>
        /// シーン内に食材着地エリアを描画
        /// </summary>
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            switch (_foodType)
            {
                case FoodType.Shrimp:
                    Gizmos.DrawWireCube(_fallPoint, _boxColliderSize);
                    break;
                case FoodType.Egg:
                    Gizmos.DrawWireSphere(_capsuleColliderSizeInformation.capuleColliderCenter + _capsuleColliderSizeInformation.distanceVector, _capsuleColliderSizeInformation.radius);
                    Gizmos.DrawWireSphere(_capsuleColliderSizeInformation.capuleColliderCenter - _capsuleColliderSizeInformation.distanceVector, _capsuleColliderSizeInformation.radius);
                    break;
                case FoodType.Chicken:
                    Gizmos.DrawWireCube(_fallPoint, _boxColliderSize);
                    break;
                case FoodType.Sausage:
                    Gizmos.DrawWireCube(_fallPoint, _boxColliderSize);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// レイキャストにより算出される落下地点に至るまでの時間
        /// </summary>
        public static float FallTime
        {
            get { return _fallTime; }
        }
        private static float _fallTime = 0;
        /// <summary>
        /// box・capsuleレイキャストによる落下地点の予測 見つからないときは0ベクトルまたはnull
        /// </summary>
        /// <typeparam name="T">Vector3 または GameObject 落下地点の欲しい情報を指定</typeparam>
        /// <typeparam name="U">ボックスレイキャスト→Vector3 3辺の長さの半分 カプセルレイキャスト→Vector2 高さと半径</typeparam>
        /// <param name="predictStartPoint">予測開始地点</param>
        /// <param name="firstSpeedVector">初速度ベクトル</param>
        /// <param name="shotAngleX">食材を打ち出す地面に対して垂直方向の角度 角度-0 ~ 90に変換後渡す</param>
        /// <param name="foodType">食材の種類</param>
        /// <param name="colliderSizeInformation">コライダーのSize情報 このエリアでボックスまたはカプセルなレイキャストを行う</param>
        /// <returns></returns>
        public static T PredictFallPointByBoxRayCast<T, U>(Vector3 predictStartPoint, Vector3 firstSpeedVector, float shotAngleX, FoodType foodType, U colliderSizeInformation) where U : struct
        {
            //, activeFood.GetColliderSize<Vector3>()
            _foodType = foodType;
            int i = 1;//累積誤差発生を防ぐためのインクリメント変数
            //レイによる落下地点予測 無限ループ防止目的で 滞空時間100秒制限
            for (float flyTime = 0f; flyTime < 30; i++)
            {
                Vector3 originPoint = predictStartPoint + new Vector3(firstSpeedVector.x * flyTime, CalculateYposition(firstSpeedVector, flyTime), firstSpeedVector.z * flyTime);
                flyTime = i / 200f; //累積誤差の発生を防ぐ 精度を決める変数 精度上げると処理が重い
                Vector3 endPoint = predictStartPoint + new Vector3(firstSpeedVector.x * flyTime, CalculateYposition(firstSpeedVector, flyTime), firstSpeedVector.z * flyTime);
                Vector3 fallPoint = Vector3.zero;
                GameObject fallPointGameObject = null;
                //終点 - 始点で方向ベクトルを算出
                var direction = endPoint - originPoint;
                //レイを可視化
                Debug.DrawRay(originPoint, direction, Color.red, 0.5f);
                if (typeof(T) == typeof(GameObject))
                {
                    if (typeof(U) == typeof(Vector3))
                    {
                        fallPoint = CastFoodSizeRayOnKitchen(out fallPointGameObject, originPoint, direction, (Vector3)(object)colliderSizeInformation);
                        if (fallPointGameObject != null)
                        {
                            if (fallPoint != Vector3.zero)
                            {
                                _fallPoint = fallPoint;
                                _boxColliderSize = (Vector3)(object)colliderSizeInformation;
                            }
                            _fallTime = flyTime;
                            return (T)(object)fallPointGameObject;
                        }
                        if (flyTime > 29.9f)
                        {
                            Debug.Log("当たっていない");
                        }
                    }
                    else if (typeof(U) == typeof(Vector2))
                    {
                        fallPoint = CastFoodSizeRayOnKitchen(out fallPointGameObject, originPoint, direction, (Vector2)(object)colliderSizeInformation);
                        if (fallPointGameObject != null)
                        {
                            //ゲームオブジェクトの場合もシーン上にデバッグ用として落下地点を描画
                            //Vector3 fallPoint = CastFoodSizeRayOnKitchen<Vector3, Vector3>(originPoint, direction, (Vector3)(object)colliderSizeInformation);
                            ////_capsuleColliderSizeInformation.capuleColliderCenter = originPoint;
                            //_capsuleColliderSizeInformation.radius = capsuleColliderInformation[(int)CapsuleColliderScaleData.Radius];
                            //_capsuleColliderSizeInformation.distanceVector = distanceVector;
                            if (fallPoint != Vector3.zero)
                            {
                                _fallPoint = fallPoint;
                            }
                            _fallTime = flyTime;
                            return (T)(object)fallPointGameObject;
                        }
                        if (flyTime > 29.9f)
                        {
                            Debug.Log("当たっていない");
                        }
                    }
                }
                else if (typeof(T) == typeof(Vector3))
                {
                    //角度3.88度未満では通常レイによる落下地点を返す 原点が地面なのでflyTimeを何フレームか飛ばす必要がある(理想は1) flyTime一定以下かつ、ぶつかった場合スキップ 開始位置では当たった場所を返せないのがBoxCastの欠点 RayCastを使うのも手 食材によって異なるので調整がさらに必要
                    if (shotAngleX < 3.88f)//エビの時 さらにカット後は変わる可能性あり
                    {
                        return (T)(object)predictStartPoint;
                    }
                    if (typeof(U) == typeof(Vector3))
                    {
                        //レイを飛ばして当たった場所を保存
                        fallPoint = CastFoodSizeRayOnKitchen(out fallPointGameObject, originPoint, direction, (Vector3)(object)colliderSizeInformation);
                        if (fallPointGameObject != null)
                        {
                            if (fallPoint != Vector3.zero)
                            {
                                _fallPoint = fallPoint;
                                _boxColliderSize = (Vector3)(object)colliderSizeInformation;
                            }
                            _fallTime = flyTime;
                            return (T)(object)fallPoint;
                        }
                        if (flyTime > 29.9f)
                        {
                            Debug.Log("当たっていない");
                        }
                    }
                    else if (typeof(U) == typeof(Vector2))
                    {
                        //レイを飛ばして当たった場所を保存
                        fallPoint = CastFoodSizeRayOnKitchen(out fallPointGameObject, originPoint, direction, (Vector2)(object)colliderSizeInformation);
                        if (fallPointGameObject != null)
                        {
                            if (fallPoint != Vector3.zero)
                            {
                                _fallPoint = fallPoint;
                            }
                            _fallTime = flyTime;
                            return (T)(object)fallPoint;
                        }
                        if (flyTime > 29.9f)
                        {
                            Debug.Log("当たっていない");
                        }
                    }
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
                return default(T);
            }
        }
        /// <summary>
        /// box・capsuleレイキャストによる落下地点の予測 見つからないときは0ベクトルまたはnull
        /// </summary>
        /// <typeparam name="T">ボックスレイキャスト→Vector3 3辺の長さの半分 カプセルレイキャスト→Vector2 高さと半径</typeparam>
        /// <param name="fallPointObject">落下地点のオブジェクトの持つタグ 床判定を行う</param>
        /// <param name="predictStartPoint">予測開始地点</param>
        /// <param name="firstSpeedVector">初速度ベクトル</param>
        /// <param name="verticalAngle">食材を打ち出す地面に対して垂直方向の角度 eulerAungle.x 角度-0 ~ 90に変換後渡す</param>
        /// <param name="foodType">食材の種類</param>
        /// <param name="colliderSizeInformation">コライダーのSize情報 このエリアでボックスまたはカプセルなレイキャストを行う</param>
        /// <returns></returns>
        public static Vector3 PredictFallPointByBoxRayCast<T>(out GameObject fallPointGameObject, Vector3 predictStartPoint, Vector3 firstSpeedVector, float verticalAngle, FoodType foodType, T colliderSizeInformation) where T : struct
        {
            _foodType = foodType;
            int i = 1;//累積誤差発生を防ぐためのインクリメント変数
            //レイによる落下地点予測 無限ループ防止目的で 滞空時間100秒制限
            for (float flyTime = 0f; flyTime < 30; i++)
            {
                Vector3 originPoint = predictStartPoint + new Vector3(firstSpeedVector.x * flyTime, CalculateYposition(firstSpeedVector, flyTime), firstSpeedVector.z * flyTime);
                flyTime = i / 200f; //累積誤差の発生を防ぐ 精度を決める変数 精度上げると処理が重い
                Vector3 endPoint = predictStartPoint + new Vector3(firstSpeedVector.x * flyTime, CalculateYposition(firstSpeedVector, flyTime), firstSpeedVector.z * flyTime);
                //終点 - 始点で方向ベクトルを算出
                var direction = endPoint - originPoint;
                Vector3 fallPoint = Vector3.zero;
                //レイを可視化
                Debug.DrawRay(originPoint, direction, Color.red, 0.2f);
                if (typeof(T) == typeof(Vector3))
                {
                    fallPoint = CastFoodSizeRayOnKitchen(out fallPointGameObject , originPoint, direction, (Vector3)(object)colliderSizeInformation);
                    if (fallPointGameObject != null)
                    {
                        //ゲームオブジェクトの場合もシーン上にデバッグ用として落下地点を描画
                        fallPoint = CastFoodSizeRayOnKitchen<Vector3, Vector3>(originPoint, direction, (Vector3)(object)colliderSizeInformation);
                        if (fallPoint != Vector3.zero)
                        {
                            _fallPoint = fallPoint;
                            _boxColliderSize = (Vector3)(object)colliderSizeInformation;
                        }
                        _fallTime = flyTime;
                        return fallPoint;
                    }
                    if (flyTime > 29.9f)
                    {
                        Debug.Log("当たっていない");
                    }
                }
                else if (typeof(T) == typeof(Vector2))
                {
                    fallPoint = CastFoodSizeRayOnKitchen(out fallPointGameObject, originPoint, direction, (Vector2)(object)colliderSizeInformation);

                    if (fallPointGameObject != null)
                    {
                        fallPoint = CastFoodSizeRayOnKitchen<Vector3, Vector2>(originPoint, direction, (Vector2)(object)colliderSizeInformation);
                        if (fallPoint != Vector3.zero)
                        {
                            _fallPoint = fallPoint;
                            //_capsuleColliderSizeInformation = (Vector3)(object)colliderSizeInformation;
                        }
                        _fallTime = flyTime;
                        return fallPoint;
                    }
                    if (flyTime > 29.9f)
                    {
                        Debug.Log("当たっていない");
                    }
                }
            }
            fallPointGameObject = null;
           // Debug.LogAssertion("指定した型が想定と異なります");
            return default(Vector3);
        }
        /// <summary>
        /// 最初に衝突した時の跳ねる方向の速度ベクトル(理論値)を算出(OnCollisionEnterでは食材の回転で実行値が安定しなかったため)
        /// </summary>
        /// <param name="firstSpeedVector">初速</param>
        /// <returns></returns>
        public static Vector3 PredictFirstBoundSpeedVecor(Vector3 firstSpeedVector)
        {
            var SpeedVectorYDelta = -StageSceneManager.Instance.gravityAccelerationValue * _fallTime;
            return new Vector3(firstSpeedVector.x, firstSpeedVector.y + SpeedVectorYDelta, firstSpeedVector.z);
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
        private static T CastFoodSizeRayOnKitchen<T, U>(Vector3 originPoint, Vector3 direction, U ColliderSize) where U : struct
        {
            //Rayが当たったオブジェクトの情報を入れる箱
            RaycastHit hit;    //原点        方向
            Ray ray = new Ray(originPoint, direction);
            //ボックス
            if (typeof(U) == typeof(Vector3))
            {
                //Kitchenレイヤーとレイ判定を行う
                if (Physics.BoxCast(originPoint, (Vector3)(object)ColliderSize * 0.5f, direction, out hit, Quaternion.identity, direction.magnitude, StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]))
                {
                    if (typeof(T) == typeof(GameObject))
                    {
                        return (T)(object)hit.transform.gameObject;
                    }
                    else if (typeof(T) == typeof(Vector3))
                    {
                        return (T)(object)hit.point;
                    }
                    else
                    {
                        //Debug.LogAssertion("指定した型が想定と異なります");
                        return default(T);
                    }
                }
                else
                {
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
                        return default;
                    }
                }
            }
            //カプセル
            else if (typeof(U) == typeof(Vector2))
            {
                var capsuleColliderInformation = (Vector2)(object)ColliderSize;
                Debug.Log(capsuleColliderInformation);
                var distanceFromCapsuleCenterToSphereCenter = capsuleColliderInformation[(int)CapsuleColliderScaleData.Height] - capsuleColliderInformation[(int)CapsuleColliderScaleData.Radius];
                var distanceVector = new Vector3(0, distanceFromCapsuleCenterToSphereCenter, 0);
                //Kitchenレイヤーとレイ判定を行う
                if (Physics.CapsuleCast(originPoint - distanceVector, originPoint + distanceVector, capsuleColliderInformation[(int)CapsuleColliderScaleData.Radius], direction, out hit, direction.magnitude, StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]))
                {
                    _capsuleColliderSizeInformation.capuleColliderCenter = originPoint;
                    _capsuleColliderSizeInformation.radius = capsuleColliderInformation[(int)CapsuleColliderScaleData.Radius];
                    _capsuleColliderSizeInformation.distanceVector = distanceVector;
                    if (typeof(T) == typeof(GameObject))
                    {
                        return (T)(object)hit.transform.gameObject;
                    }
                    else if (typeof(T) == typeof(Vector3))
                    {
                        return (T)(object)hit.point;
                    }
                    else
                    {
                        //Debug.LogAssertion("指定した型が想定と異なります");
                        return default(T);
                    }
                }
                else
                {
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
                        return default;
                    }
                }
            }
            else
            {
                //Debug.LogAssertion("指定した型が想定と異なります");
                return default(T);
            }
        }
        /// <summary>
        /// レイを飛ばしてKitchenレイヤーに当たった座標を返す
        /// </summary>
        /// <param name="originPoint"></param>
        /// <param name="direction"></param>
        /// <returns>Kitchenレイヤーに当たったかどうか</returns>
        private static Vector3 CastFoodSizeRayOnKitchen<T>(out GameObject fallPointObject, Vector3 originPoint, Vector3 direction, T ColliderSize) where T : struct
        {
            //Rayが当たったオブジェクトの情報を入れる箱
            RaycastHit hit;    //原点        方向
            Ray ray = new Ray(originPoint, direction);
            //ボックス
            if (typeof(T) == typeof(Vector3))
            {
                //Kitchenレイヤーとレイ判定を行う
                if (Physics.BoxCast(originPoint, (Vector3)(object)ColliderSize * 0.5f, direction, out hit, Quaternion.identity, direction.magnitude, StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]))
                {
                    fallPointObject = hit.transform.gameObject ;
                    return hit.point;
                }
                else
                {
                    fallPointObject = null;
                    return originPoint;
                }
            }
            //カプセル
            else if (typeof(T) == typeof(Vector2))
            {
                var capsuleColliderInformation = (Vector2)(object)ColliderSize;
                var distanceFromCapsuleCenterToSphereCenter = capsuleColliderInformation[(int)CapsuleColliderScaleData.Height] - capsuleColliderInformation[(int)CapsuleColliderScaleData.Radius];
                var distanceVector = new Vector3(0, distanceFromCapsuleCenterToSphereCenter, 0);
                //Kitchenレイヤーとレイ判定を行う
                if (Physics.CapsuleCast(originPoint - distanceVector, originPoint + distanceVector, capsuleColliderInformation[(int)CapsuleColliderScaleData.Radius], direction, out hit, direction.magnitude, StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]))
                {
                    _capsuleColliderSizeInformation.capuleColliderCenter = originPoint;
                    _capsuleColliderSizeInformation.radius = capsuleColliderInformation[(int)CapsuleColliderScaleData.Radius];
                    _capsuleColliderSizeInformation.distanceVector = distanceVector;
                    fallPointObject = hit.transform.gameObject;
                    return hit.point;
                }
                else
                {
                    fallPointObject = null;
                    return originPoint;
                }
            }
            else
            {
                //Debug.LogAssertion("指定した型が想定と異なります");
                fallPointObject = null;
                return originPoint;
            }
        }
        #endregion

        #region 予備
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

        /// <summary>
        /// レイを飛ばしてKitchenレイヤーに当たった座標を返す
        /// </summary>
        /// <param name="originPoint"></param>
        /// <param name="direction"></param>
        /// <returns>Kitchenレイヤーに当たったかどうか</returns>
        private static Vector3 CastRay(Vector3 originPoint, Vector3 direction)
        {
            ///レイの長さ
            float rayLength = direction.magnitude;
            //Rayが当たったオブジェクトの情報を入れる箱
            RaycastHit hit;    //原点        方向
            Ray ray = new Ray(originPoint, direction);
            Debug.Log(Physics.BoxCast(originPoint, direction, direction, out hit));
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
        private static GameObject GetGameObjectByCastBoxRayOnKitchen(Vector3 originPoint, Vector3 direction, Vector3 boxColliderSize)
        {
            //Rayが当たったオブジェクトの情報を入れる箱
            RaycastHit hit;    //原点        方向
            Ray ray = new Ray(originPoint, direction);
            //Kitchenレイヤーとレイ判定を行う
            if (Physics.BoxCast(originPoint, boxColliderSize * 0.5f, direction, out hit, Quaternion.identity, direction.magnitude, StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]))
            {
                return hit.transform.gameObject;
            }
            return null;
        }
        #endregion

    }
}
