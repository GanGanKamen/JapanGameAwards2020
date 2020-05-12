﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    public class AI : FoodStatus
    {
        /// <summary>
        /// 射出角度
        /// </summary>
        private float ThrowingAngle = 45;

        float rad, x, y, speed;

        Vector3 targetPosition;

        Rigidbody rid;

        new private void Start()
        {
            //実験用コード
            //StartCoroutine(Shooting(GameObject.FindGameObjectWithTag("Finish")));
            //射出
           rid = GetComponent<Rigidbody>();
        }

        //private void Update()
        //{
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

            // 射出角度
            float angle = ThrowingAngle;

            // 射出速度を算出
            Vector3 velocity = CalculateVelocity(this.transform.position, targetPosition, angle);
            ShotManager.Instance.AIShot(velocity * rid.mass);
        }

        private Vector3 CalculateVelocity(Vector3 pointA, Vector3 pointB, float angle)
        {
            // 射出角をラジアンに変換
            rad = angle * Mathf.PI / 180;

            x = CalculateDistance(pointA, pointB).x;
            y = CalculateDistance(pointA, pointB).y;

            speed = Mathf.Sqrt(-Physics.gravity.y * Mathf.Pow(x, 2) / (2 * Mathf.Pow(Mathf.Cos(rad), 2) * (x * Mathf.Tan(rad) + y)));

            if (float.IsNaN(speed))
            {
                // 条件を満たす初速を算出できなければVector3.zeroを返す
                return Vector3.zero;
            }
            else
            {
                return (new Vector3(pointB.x - pointA.x, x * Mathf.Tan(rad), pointB.z - pointA.z).normalized * speed);
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
            foreach (var seasoning in GimmickManager.Instance.Seasonings)
            {
                if (seasoning != null)
                {
                    var distance = CalculateDistance(this.transform.position, seasoning.transform.position);
                    if (Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.y, 2) <= Mathf.Pow(searchDistance, 2))
                    {
                        Debug.Log(seasoning);
                        return seasoning;
                    }
                }
            }
            //foreach (var targetTowelPositionObject in GimmickManager.Instance.TargetTowelPositionObjects)
            //{
            //    if (targetTowelPositionObject != null)
            //    {
            //        var distance = CalculateDistance(this.transform.position, targetTowelPositionObject.transform.position);
            //        if (Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.y, 2) <= Mathf.Pow(searchDistance, 2))
            //        {
            //            return targetTowelPositionObject;
            //        }
            //    }
            //}
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
