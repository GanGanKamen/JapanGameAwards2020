﻿using System.Collections;
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
        Floor,
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
        /// <summary>ターゲット検索の精度を決める変数 大きいほど細かい検索が可能 処理速度を決める変数</summary>
        private const int _searchAccuracy = 50;
        /// <summary>ターゲットまでの距離検索範囲の初期値</summary>
        private const float _initializeTargetDistanceValue = 25f / _searchAccuracy;
        /// <summary>ターゲットまでの距離 レイによるチェックで見つからなければ再検索を行う際に使用</summary>
        private float _targetDistance = _initializeTargetDistanceValue;
        /// <summary>ターゲットに向けての射出方向</summary>
        Vector3 _shotDirection = Vector3.zero;
        /// <summary>レイによるチェックで到達できないと判断されたオブジェクトを格納</summary>
        private List<GameObject> _ignoreObjects = new List<GameObject>();
        Rigidbody _rigidBody;
        /// <summary>
        /// 座標によって決めたターゲットのタグ
        /// </summary>
        AITargetObjectTags _targetTagByTransform;
        /// <summary>デバッグ中は狙い通りの座標にぴったり飛ぶ</summary>
        [SerializeField] private bool _isDebugMode = false;
        private float[] _randomRangeOfShotPower = new float[System.Enum.GetValues(typeof(LimitValue)).Length];
        /// <summary>今回の検索で、ターゲットになる候補となるオブジェクトを格納 タオルなどは場合によっては候補に入らない</summary>
        List<GameObject> _targetObjectOptions = new List<GameObject>();
        /// <summary>各ターゲットと自分との距離を算出して格納</summary>
        List<float> _distances = new List<float>();

        public void SetAIShotRange(float[] shotRange)
        {
            _randomRangeOfShotPower = shotRange;
        }
        protected override void Start()
        {
            base.Start();
            if (_rigidBody == null)
                _rigidBody = GetComponent<Rigidbody>();
        }
        /// <summary>
        /// AIのターンになったら呼ばれる
        /// </summary>
        public void TurnAI()
        {
            //初期化
            _ignoreObjects.Clear();
            _targetObjectOptions.Clear();
            _distances.Clear();
            _targetDistance = _initializeTargetDistanceValue;
            StartCoroutine(Shooting());
        }

        /// <summary>
        /// 標的のオブジェクトに向かってショット
        /// </summary>
        /// <param name="targetObject"></param>
        /// <returns></returns>
        IEnumerator Shooting()
        {
            GetTargetObjects();
            yield return new WaitForSeconds(2f);
            while (true)
            {
                if (_targetObjectOptions.Count == 0)
                {
                    ChangeTargetForGoal();
                    break;
                }
                else
                {
                    _targetTagByTransform = GetTargetTag();
                    var targetObject = _targetObjectOptions[0];
                    if (DetermineIfShotIsPossible(targetObject))
                    {
                        break;
                    }
                    else
                    {
                        _targetObjectOptions.RemoveAt(0);
                        _distances.RemoveAt(0);
                    }
                    //Debug.Log(canShot);
                    //yield return null;
                }
            }
            float aiShotPower = 0;
            //補正無し
            if (_isDebugMode)
            {
                aiShotPower = speed;
            }
            else
            {
                aiShotPower = GetAIShotPowerInRandomRange();
            }
            // 射出速度ベクトルを算出
            var velocity = _shotDirection * aiShotPower;
            //ショット情報を渡す
            ShotManager.Instance.SetShotVector(velocity, aiShotPower);
            //ショット
            ShotManager.Instance.AIShot(_shotDirection);
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

        private void GetTargetObjects()
        {
            if (GetComponent<PlayerPoint>().IsFirstTowel)
                _targetObjectOptions.AddRange(GimmickManager.Instance.TargetObjectsForAI[(int)AITargetObjectTags.TowelAbovePoint]);
            foreach (var seasoning in GimmickManager.Instance.TargetObjectsForAI[(int)AITargetObjectTags.Seasoning])
            {
                //調味料は無い可能性あり   
                if (seasoning != null)
                {
                    _targetObjectOptions.Add(seasoning);
                }
            }
            if (_targetObjectOptions.Count > 0)
            {
                foreach (var targetObjectOption in _targetObjectOptions)
                {
                    _distances.Add(Vector3.Distance(targetObjectOption.transform.position, this.transform.position));
                }
            }
            Debug.Log(_distances.Count);
            if (_distances.Count > 0)
            {
                //並び替える
                for (int i = 0; i < _distances.Count - 1; i++)
                {
                    for (int j = 0; j < _distances.Count - 1 - i; j++)
                    {
                        if (_distances[j] > _distances[j + 1])
                        {
                            //値とプレイヤーをシンクロさせて入れ替える 0番目に最小値
                            ArrayMethod.ChangeArrayValuesFromHighToLow(_distances, j);
                            ArrayMethod.ChangeArrayValuesFromHighToLow(_targetObjectOptions, j);
                        }
                    }
                }
            }
        }

        private AITargetObjectTags GetTargetTag()
        {
            AITargetObjectTags targetTag = AITargetObjectTags.Finish;//仮の値
            targetTag = EnumParseMethod.TryParseAndDebugAssertFormatAndReturnResult(_targetObjectOptions[0].tag, false, targetTag);
           return targetTag;
        }

        /// <summary>
        /// 実際にショットされる速度を難易度に応じて算出
        /// </summary>
        /// <returns></returns>
        private float GetAIShotPowerInRandomRange()
        {
            //難易度に応じてショットパワーを乱数調整
            return speed * Cooking.Random.GetRandomFloat(_randomRangeOfShotPower[(int)LimitValue.Min], _randomRangeOfShotPower[(int)LimitValue.Max]);
        }
        //protected override void Update()
        //{
        //    base.Update();
        //}

        /// <summary>
        /// 角度を変えながらレイを飛ばして到達可能かどうか判断する(この中で速度や角度を計算する)
        /// </summary>
        private bool DetermineIfShotIsPossible(GameObject targetObject)
        {
            if (targetObject == null)
            {
                Debug.Log("null");
                targetObject = StageSceneManager.Instance.Goal[0];
            }
            else
            {
                targetPosition = targetObject.transform.position;
            }
            //ランダム要素
            //seedId = Random.Range(0, 33 - rate);
            Vector3 velocity, direction;
            //射出角度 初期値45度 レイにより障害物判定で変える 加算していき85(=限界角度)でだめなら45度から減少
            float throwingAngle = 45;
            for (int i = 0; throwingAngle <= ShotManager.Instance.ShotParameter.LimitVerticalAngle; i++)
            {
                //まずは角度を上昇
                throwingAngle = 45 + i;//重いので1度刻み
                direction = CalculateVelocity(groundPoint, targetPosition, throwingAngle);
                //速度の大きさが許容範囲を超えていたらだめ
                if (speed > ShotManager.Instance.ShotParameter.MaxShotPower)
                {
                    speed = ShotManager.Instance.ShotParameter.MaxShotPower;
                    continue;
                }
                // 射出速度を算出
                velocity = direction * speed;
                if (CastRayByChangingShotAngle(throwingAngle, velocity, direction))
                {
                    return true;
                }
            }
            //角度のリセット
            throwingAngle = 45;
            for (int i = 0; throwingAngle >= 10; i++)
            {
                //次に減少
                throwingAngle = 45 - i;
                direction = CalculateVelocity(groundPoint, targetPosition, throwingAngle);
                //速度の大きさが許容範囲を超えていたらだめ
                if (speed > ShotManager.Instance.ShotParameter.MaxShotPower)
                {
                    speed = ShotManager.Instance.ShotParameter.MaxShotPower;
                    continue;
                }
                // 射出速度を算出
                velocity = direction * speed;
                if (CastRayByChangingShotAngle(throwingAngle, velocity, direction))
                {
                    return true;
                }
            }
            //見つからない場合
            Debug.Log("見つからない");
            _ignoreObjects.Add(targetObject);
            //仮のショット決定
            _shotDirection = CalculateVelocity(this.transform.position, targetPosition, 60);
            return false;
        }

        private bool CastRayByChangingShotAngle(float throwingAngle, Vector3 velocity, Vector3 direction)
        {
            //方向取得・初速の大きさも計算
            var newTarget = this.gameObject;
            switch (foodType)
            {
                case FoodType.Shrimp:
                    newTarget = PredictFoodPhysics.PredictFallPointByBoxRayCast<GameObject, Vector3>(transform.position, velocity, throwingAngle, foodType, GetColliderSize<Vector3>());
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
            if (newTarget != null)
            {
                Debug.Log(_targetTagByTransform);
                Debug.Log(newTarget.tag);
                //タオルの時は、レイによるオブジェクトタグ取得名はTowel であって TowelAbovePointではない 名前異なる
                if (_targetTagByTransform == AITargetObjectTags.TowelAbovePoint)
                {
                    if (newTarget.tag == TagList.Towel.ToString())
                    {
                        _shotDirection = direction;
                        return true;
                    }
                }
                else
                {
                    //Floorに向かってはダメ・狙い通りのタグかどうか
                    AITargetObjectTags targetTagByLayCast = AITargetObjectTags.Seasoning;//仮の値
                    targetTagByLayCast = EnumParseMethod.TryParseAndDebugAssertFormatAndReturnResult(newTarget.tag, false, targetTagByLayCast);
                    if (targetTagByLayCast == _targetTagByTransform)
                    {
                        _shotDirection = direction;
                        return true;
                    }
                }
            }
            return false;
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
        /// ターゲットを探す距離を決めて探す(一定半径範囲内にターゲットがあるかどうかで行動を決める) 見つからなければゴールへ
        /// </summary>
        /// <returns></returns>
        //private GameObject SearchMinDistanceTargetObject(int incrementVariable)
        //{

        //}
        /// <summary>
        /// 指定された距離で自分中心に球体の半径を設定し、その範囲の中でターゲットを探す その半径の中で見つからなければnullを返す
        /// </summary>
        /// <param name="searchDistance">探索範囲の半径</param>
        /// <returns></returns>
        private GameObject DecideTarget(float searchDistance)
        {
            _targetTagByTransform = AITargetObjectTags.TowelAbovePoint;
            //タオルに向かうのは初回のみ
            if (GetComponent<PlayerPoint>().IsFirstTowel)
            {
                foreach (var targetTowelPositionObject in GimmickManager.Instance.TargetObjectsForAI[(int)AITargetObjectTags.TowelAbovePoint])
                {
                    if (targetTowelPositionObject != null)
                    {
                        var distance = CalculateDistance(this.transform.position, targetTowelPositionObject.transform.position);
                        if (Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.y, 2) <= Mathf.Pow(searchDistance, 2))
                        {
                            if (CheckTargetObjectIsReach(targetTowelPositionObject))
                            {
                                return targetTowelPositionObject;
                            }
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
                        if (CheckTargetObjectIsReach(seasoning))
                        {
                            return seasoning;
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 到達できるオブジェクトかどうか確認
        /// </summary>
        /// <param name="targetObject"></param>
        /// <returns></returns>
        private bool CheckTargetObjectIsReach(GameObject targetObject)
        {
            foreach (var ignoreObject in _ignoreObjects)
            {
                if (ignoreObject.transform.position == targetObject.transform.position)
                {
                    Debug.LogFormat("到達不可能なオブジェクト {0}", targetObject.transform.position);
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 標的をゴールへ変更
        /// </summary>
        private void ChangeTargetForGoal()
        {
            foreach (var goal in StageSceneManager.Instance.Goal)
            {
                //ゴールが到達可能かどうかの判定 1自分からゴールまでの方向ベクトルを取得し xz成分を抜き出す 2 y成分 sin(角度→ラジアン変換)を入れて標準化しなおすことで方向ベクトルを取得 3 最大ショットパワーでレイを飛ばし垂直方向の角度を変化させる 10~ 85
                //3 ゴールタグを持つオブジェクトをレイキャストにより取得できるかどうかで判定する。4パワーの変更
                var goalVector = (goal.transform.position - transform.position).normalized;
                var maxShotSpeed = ShotManager.Instance.ShotParameter.MaxShotPower;
                int i = 0;//インクリメント変数
                float shotDirectionY;
                Vector3 shotDirectionVector, maxShotSpeedVector;
                for (float shotSpeed = maxShotSpeed; shotSpeed > 0; i++)
                {
                    shotSpeed -= i / 2;
                    for (int shotAngleVertical = 10; shotAngleVertical <= 85; shotAngleVertical++)
                    {
                        shotDirectionY = Mathf.Sin(shotAngleVertical * Mathf.Deg2Rad); // 角度をラジアンへ
                        shotDirectionVector = new Vector3(goalVector.x, shotDirectionY, goalVector.z).normalized;
                        maxShotSpeedVector = shotDirectionVector * maxShotSpeed;
                        GameObject fallPointObject = null;
                        switch (foodType)
                        {
                            case FoodType.Shrimp:
                                fallPointObject = PredictFoodPhysics.PredictFallPointByBoxRayCast<GameObject, Vector3>(transform.position, maxShotSpeedVector, shotAngleVertical, foodType, GetColliderSize<Vector3>());
                                break;
                            case FoodType.Egg:
                                if (food.egg.HasBroken)
                                {
                                    fallPointObject = PredictFoodPhysics.PredictFallPointByBoxRayCast<GameObject, Vector3>(transform.position, maxShotSpeedVector, shotAngleVertical, foodType, GetColliderSize<Vector3>());
                                }
                                else
                                {
                                    fallPointObject = PredictFoodPhysics.PredictFallPointByBoxRayCast<GameObject, Vector2>(transform.position, maxShotSpeedVector, shotAngleVertical, foodType, GetColliderSize<Vector2>());
                                }
                                break;
                            case FoodType.Chicken:
                                fallPointObject = PredictFoodPhysics.PredictFallPointByBoxRayCast<GameObject, Vector3>(transform.position, maxShotSpeedVector, shotAngleVertical, foodType, GetColliderSize<Vector3>());
                                break;
                            case FoodType.Sausage:
                                fallPointObject = PredictFoodPhysics.PredictFallPointByBoxRayCast<GameObject, Vector3>(transform.position, maxShotSpeedVector, shotAngleVertical, foodType, GetColliderSize<Vector3>());
                                break;
                            default:
                                break;
                        }
                        if (fallPointObject.tag == TagList.Finish.ToString())
                        {
                            _shotDirection = shotDirectionVector;
                            speed = maxShotSpeed;
                            return;
                        }
                    }
                }
                //ゴールが届く範囲にない場合、最もゴールに近い場所へ移動
                //1 垂直方向角度45度(仮)かつ最大パワーで回転させてレイを飛ばす(仮:1度刻み) 落下地点を配列に格納 2 落下地点の中でゴールに近いものを選ぶ 
                //3条件を加える 自分の近くではない(距離 5未満)、かつ床ではない 4食材の挙動を考慮して床から距離を置く タグfloorを検出した最後の角度から 仮：プラスマイナス5度以上離れているか確認 
                int verticalAngle = 45;
                List<float> goalDistanceFromFallPoints = new List<float>();
                shotDirectionY = Mathf.Sin(verticalAngle * Mathf.Deg2Rad); // 角度をラジアンへ
                for (int horizontalAngle = 0; horizontalAngle < 360; horizontalAngle++)
                {
                    var shotDirectionX = Mathf.Cos(horizontalAngle * Mathf.Deg2Rad); // 角度をラジアンへ
                    var shotDirectionZ = Mathf.Sin(horizontalAngle * Mathf.Deg2Rad); // 角度をラジアンへ
                    shotDirectionVector = new Vector3(shotDirectionX, shotDirectionY, shotDirectionZ).normalized;
                    maxShotSpeedVector = shotDirectionVector * maxShotSpeed;
                    Vector3 fallPoint;
                    switch (foodType)
                    {
                        case FoodType.Shrimp:
                            fallPoint = PredictFoodPhysics.PredictFallPointByBoxRayCast<Vector3, Vector3>(transform.position, maxShotSpeedVector, verticalAngle, foodType, GetColliderSize<Vector3>());
                            break;
                        case FoodType.Egg:
                            if (food.egg.HasBroken)
                            {
                                fallPoint = PredictFoodPhysics.PredictFallPointByBoxRayCast<Vector3, Vector3>(transform.position, maxShotSpeedVector, verticalAngle, foodType, GetColliderSize<Vector3>());
                            }
                            else
                            {
                                fallPoint = PredictFoodPhysics.PredictFallPointByBoxRayCast<Vector3, Vector2>(transform.position, maxShotSpeedVector, verticalAngle, foodType, GetColliderSize<Vector2>());
                            }
                            break;
                        case FoodType.Chicken:
                            fallPoint = PredictFoodPhysics.PredictFallPointByBoxRayCast<Vector3, Vector3>(transform.position, maxShotSpeedVector, verticalAngle, foodType, GetColliderSize<Vector3>());
                            break;
                        case FoodType.Sausage:
                            fallPoint = PredictFoodPhysics.PredictFallPointByBoxRayCast<Vector3, Vector3>(transform.position, maxShotSpeedVector, verticalAngle, foodType, GetColliderSize<Vector3>());
                            break;
                        default:
                            break;
                    }
                }
            }
        }
}
}
