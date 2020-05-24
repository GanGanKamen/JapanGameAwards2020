using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// AIのターゲットになるオブジェクトのタグリスト
    /// </summary>
    public enum AITargetObjectTags
    {
        Finish, Water, Seasoning, RareSeasoning, TowelAbovePoint, Knife, Bubble,
        Floor
    }
    /// <summary>
    /// AIの強さ
    /// </summary>
    public enum AILevel
    {
        Easy, Normal, Hard
    }
    public class AI : FoodStatus
    {
        float radius, horizontalDistance, verticalDistance, speed;

        Vector3 targetPosition;

        Rigidbody _rigidBody;
        /// <summary>
        /// 座標によって決めたターゲットのタグ
        /// </summary>
        AITargetObjectTags _targetTagByTransform;
        private float[] _randomRangeOfShotPower = new float[System.Enum.GetValues(typeof(LimitValue)).Length];
        public void SetAIShotRange(float[] shotRange)
        {
            _randomRangeOfShotPower = shotRange;
        }

        protected override void Start()
        {
            base.Start();
            if(_rigidBody == null)
            _rigidBody = GetComponent<Rigidbody>();
            Debug.Log(_randomRangeOfShotPower[0]);
        }

        //protected override void Update()
        //{
        //    base.Update();
        //}

        /// <summary>
        /// ボールを射出する
        /// </summary>
        private void ThrowingBall(GameObject targetObject)
        {
            if (targetObject == null)
            {
                Debug.Log("null");
                targetObject = StageSceneManager.Instance.Goal;
            }
            else
            {
                targetPosition = targetObject.transform.position;
            }
            //ランダム要素
            //seedId = Random.Range(0, 33 - rate);
            //射出角度 初期値45度 レイにより障害物判定で変える 加算していき85(=限界角度)でだめなら45度から減少
            float throwingAngle = 45;
            Vector3 velocity,direction;
            for (int i = 0; throwingAngle <= ShotManager.Instance.ShotParameter.LimitVerticalAngle - 10;i++)
            {
                throwingAngle = 45 + i / 2;
                direction = CalculateVelocity(groundPoint, targetPosition, throwingAngle);
                //方向取得・初速の大きさも計算
                //速度の大きさが許容範囲を超えていたらだめ
                if (speed > ShotManager.Instance.ShotParameter.MaxShotPower)
                {
                    speed = ShotManager.Instance.ShotParameter.MaxShotPower;
                }
                // 射出速度を算出
                velocity = direction * speed;
                var newTarget = this.gameObject;
                switch (foodType)
                {
                    case FoodType.Shrimp:
                        newTarget = PredictFoodPhysics.PredictFallPointByBoxRayCast<GameObject,Vector3>(transform.position, velocity, throwingAngle, foodType, GetColliderSize<Vector3>());
                        break;
                    case FoodType.Egg:
                        if (food.egg.HasBroken)
                        {
                            newTarget = PredictFoodPhysics.PredictFallPointByBoxRayCast<GameObject, Vector3>(transform.position, velocity, throwingAngle, foodType, GetColliderSize<Vector3>());
                        }
                        else
                        {
                            newTarget = PredictFoodPhysics.PredictFallPointByBoxRayCast<GameObject, Vector2>(transform.position, velocity, throwingAngle, foodType, GetColliderSize<Vector2>());
                        }
                        break;
                    case FoodType.Chicken:
                        newTarget = PredictFoodPhysics.PredictFallPointByBoxRayCast<GameObject, Vector3>(transform.position, velocity, throwingAngle, foodType, GetColliderSize<Vector3>());
                        break;
                    case FoodType.Sausage:
                        newTarget = PredictFoodPhysics.PredictFallPointByBoxRayCast<GameObject, Vector3>(transform.position, velocity, throwingAngle, foodType, GetColliderSize<Vector3>());
                        break;
                    default:
                        break;
                }
                //タオルの時は、レイによるオブジェクトタグ取得名はTowel であって TowelAbovePointではない 名前異なる
                if (_targetTagByTransform == AITargetObjectTags.TowelAbovePoint)
                {
                    if (newTarget.tag == TagList.Towel.ToString())
                    {
                        ShotManager.Instance.SetShotVector(velocity, speed);
                        ShotManager.Instance.AIShot(direction);
                        return;
                    }
                }
                //Floorに向かってはダメ・狙い通りのタグかどうか
                AITargetObjectTags targetTagByLayCast = AITargetObjectTags.Seasoning;
                targetTagByLayCast = EnumParseMethod.TryParseAndDebugAssertFormatAndReturnResult(newTarget.tag, false, targetTagByLayCast);
                if (targetTagByLayCast == _targetTagByTransform)
                {
                    ShotManager.Instance.SetShotVector(velocity, speed);
                    ShotManager.Instance.AIShot(direction);
                    return;
                }
            }
            //見つからない場合
            Debug.Log("見つからない");
            direction = CalculateVelocity(this.transform.position, targetPosition, 60);
            // 射出速度を算出
            velocity = direction * speed;
            ShotManager.Instance.SetShotVector(velocity, speed);
            ShotManager.Instance.AIShot(direction);
        }
        /// <summary>
        /// 方向取得・初速も計算
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        private Vector3 CalculateVelocity(Vector3 pointA, Vector3 pointB, float angle)
        {
            // 射出角をラジアンに変換
            radius = angle * Mathf.PI / 180;

            horizontalDistance = CalculateDistance(pointA, pointB).x;
            verticalDistance = CalculateDistance(pointA, pointB).y;

            speed = Mathf.Sqrt(-Physics.gravity.y * Mathf.Pow(horizontalDistance, 2) / (2 * Mathf.Pow(Mathf.Cos(radius), 2) * (horizontalDistance * Mathf.Tan(radius) + verticalDistance)));

            if (float.IsNaN(speed))
            {
                // 条件を満たす初速を算出できなければVector3.zeroを返す
                return Vector3.zero;
            }
            else
            {
                return (new Vector3(pointB.x - pointA.x, horizontalDistance * Mathf.Tan(radius), pointB.z - pointA.z).normalized);
            }
        }

        private Vector2 CalculateDistance(Vector3 pointA, Vector3 pointB)
        {
            // 水平方向の距離x
            var x = Vector2.Distance(new Vector2(pointA.x, pointA.z), new Vector2(pointB.x, pointB.z));

            // 垂直方向の距離y
            var y = pointA.y - pointB.y;

            return new Vector2(x, y);
        }
        /// <summary>
        /// AIのターンになったら呼ばれる
        /// </summary>
        public void TurnAI()
        {
            var targetObject = SearchTargetObject();
            StartCoroutine(Shooting(targetObject));
        }
        /// <summary>
        /// ターゲットを探す距離を決めて探す(一定半径範囲内にターゲットがあるかどうかで行動を決める) 見つからなければゴールへ
        /// </summary>
        /// <returns></returns>
        private GameObject SearchTargetObject()
        {
            int i = 25;//累積誤差発生を防ぐためのインクリメント変数
            //半径100メートルでターゲット検索 無限ループ防止目的で距離制限
            for (float distance = i / 100f; distance < 100; i++)
            {
                distance = i / 100f; //累積誤差の発生を防ぐ 精度を決める変数 精度上げると処理が重い
                var target = DecideTarget(distance);
                if (target != null)
                {
                    return target;
                }
            }
            return StageSceneManager.Instance.Goal;
        }
        /// <summary>
        /// 指定された距離で自分中心に球体の半径を設定し、その範囲の中でターゲットを探す その半径の中で見つからなければnullを返す
        /// </summary>
        /// <param name="searchDistance">探索範囲の半径</param>
        /// <returns></returns>
        private GameObject DecideTarget(float searchDistance)
        {
            _targetTagByTransform = AITargetObjectTags.TowelAbovePoint;
            if (GetComponent<PlayerPoint>().IsFirstTowel)
            {
                foreach (var targetTowelPositionObject in GimmickManager.Instance.TargetObjectsForAI[(int)AITargetObjectTags.TowelAbovePoint])
                {
                    if (targetTowelPositionObject != null)
                    {
                        var distance = CalculateDistance(this.transform.position, targetTowelPositionObject.transform.position);
                        if (Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.y, 2) <= Mathf.Pow(searchDistance, 2))
                        {
                            return targetTowelPositionObject;
                        }
                    }
                }
            }
            _targetTagByTransform = AITargetObjectTags.Seasoning;
            foreach (var seasoning in GimmickManager.Instance.TargetObjectsForAI[(int)AITargetObjectTags.Seasoning])
            {
                if (seasoning != null)
                {
                    var distance = CalculateDistance(this.transform.position, seasoning.transform.position);
                    if (Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.y, 2) <= Mathf.Pow(searchDistance, 2))
                    {
                        return seasoning;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 標的のオブジェクトに向かってショット
        /// </summary>
        /// <param name="targetObject"></param>
        /// <returns></returns>
        IEnumerator Shooting(GameObject targetObject)
        {
            //this.transform.LookAt(targetObject.transform);
            yield return new WaitForSeconds(2f);
            ThrowingBall(targetObject);
            EffectManager.Instance.InstantiateEffect(TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].transform.position, EffectManager.EffectPrefabID.Food_Jump);
            ///止まるまでAIのターン
            while (true)
            {
                if (ShotManager.Instance.ShotModeProperty == ShotState.ShotEndMode)
                {
                    break;
                }
                yield return null;
            }
        }
    }
}
