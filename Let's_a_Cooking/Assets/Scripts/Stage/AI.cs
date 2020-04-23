using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    public class AI : FoodStatus
    {
        /// <summary>
        /// 射出するオブジェクト
        /// </summary>
        public GameObject ThrowingObject;

        /// <summary>
        /// 射出角度
        /// </summary>
        [SerializeField, Range(0F, 90F)]
        private float ThrowingAngle;

        float rad, x, y, speed;

        Vector3 targetPosition;

        Rigidbody rid;
        /// <summary>
        /// 一定半径範囲内に調味料があるかどうかで行動を決める
        /// </summary>
        float _searchArea = 50;

        private void Start()
        {
            //実験用コード
            //StartCoroutine(Shooting(GameObject.FindGameObjectWithTag("Finish")));
            // 射出
            rid = ThrowingObject.GetComponent<Rigidbody>();
        }

        private void Update()
        {
        }

        /// <summary>
        /// ボールを射出する
        /// </summary>
        private void ThrowingBall(GameObject targetObject)
        {
            targetPosition = targetObject.transform.position;
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

        public void TurnAI()
        {
            StartCoroutine(Shooting(DecideTarget()));
        }

        private GameObject DecideTarget()
        {
            foreach (var seasoning in GimmickManager.Instance.Seasonings)
            {
                if (seasoning != null)
                {
                    var distance = CalculateDistance(this.transform.position, seasoning.transform.position);
                    if (Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.y, 2) <= Mathf.Pow(_searchArea, 2))
                    {
                        return seasoning;
                    }
                }
            }

            return StageSceneManager.Instance.goal;
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
