using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// AIのターゲットになるオブジェクトのタグリスト
    /// </summary>
    public enum AITargetObjectTags
    {
        Finish, Water, Seasoning, RareSeasoning, TowelAbovePoint, Knife, Bubble,
        Floor, Food
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
        private bool _searchEnd;
        enum NearFallFloorAngle
        {
            None, Left, Right, Both
        }
        enum NearFallFloorPower
        {
            None, Strong, Weak, Both
        }
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
            _searchEnd = false;
            _ignoreObjects.Clear();
            _targetObjectOptions.Clear();
            _distances.Clear();
            _targetDistance = _initializeTargetDistanceValue;
            StartCoroutine(DecideShotTarget());
        }

        /// <summary>
        /// 標的のオブジェクトに向かってショット
        /// </summary>
        /// <param name="targetObject"></param>
        /// <returns></returns>
        IEnumerator DecideShotTarget()
        {
            while (UIManager.Instance.MainUIStateProperty == ScreenState.Start)
            {
                yield return null;
            }
            GetTargetObjects();
            while (true)
            {
                if (TurnManager.Instance.RemainingTurns < 2 || _targetObjectOptions.Count == 0 || _distances.Count == 0)
                {
                    Debug.Log("ターゲット無し");
                    ChangeTargetForGoal();
                    break;
                }
                else
                {
                    //必ずFinishになる
                    _targetTagByTransform = GetTargetTag();
                    var targetObject = _targetObjectOptions[0];
                    if (DetermineIfShotIsPossible(targetObject))
                    {
                        // Debug.LogFormat("{0}発見", targetObject.name);
                        _searchEnd = true;
                        break;
                    }
                    else
                    {
                        _targetObjectOptions.RemoveAt(0);
                        _distances.RemoveAt(0);
                    }
                }
            }
            if (_searchEnd)
            {
                yield return new WaitForSeconds(2f);
            }
            else
            {
                float timeCounter = 0;
                while (!_searchEnd)
                {
                    Debug.Log(65);
                    //まれに無限ループ→5秒で抜ける
                    timeCounter += Time.deltaTime;
                    //if (timeCounter > 5)
                    //{
                    //    Debug.Log("探索失敗");
                    //    break;
                    //}
                    yield return null;
                }
            }
            StartCoroutine(Shot());
        }
        /// <summary>
        /// ショット _searchEndがtrueになったら呼ぶ
        /// </summary>
        /// <returns></returns>
        IEnumerator Shot()
        {
            if (speed > ShotManager.Instance.ShotParameter.MaxShotPower)
            {
                Debug.Log("想定以上のショットパワーです");
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
        /// <summary>
        /// 標的をゴールへ変更
        /// </summary>
        private void ChangeTargetForGoal()
        {
            Vector3 goalVector;
            float shotDirectionY;
            GameObject[] goalObject = StageSceneManager.Instance.Goal;
            float maxShotSpeed = ShotManager.Instance.ShotParameter.MaxShotPower;
            foreach (var goal in goalObject)
            {
                //ゴールが到達可能かどうかの判定 1自分からゴールまでの方向ベクトルを取得し xz成分を抜き出す 2 y成分 sin(角度→ラジアン変換)を入れて標準化しなおすことで方向ベクトルを取得 3 最大ショットパワーでレイを飛ばし垂直方向の角度を変化させる 10~ 85
                //3 ゴールタグを持つオブジェクトをレイキャストにより取得できるかどうかで判定する。4パワーの変更
                goalVector = (goal.transform.position - transform.position).normalized;
                int i = 0;//インクリメント変数
                Vector3 shotDirectionVector = Vector3.zero, maxShotSpeedVector = Vector3.zero;
                for (float shotSpeed = maxShotSpeed; shotSpeed > 0; i++)
                {
                    shotSpeed = i / 2;
                    for (int shotAngleVertical = 10; shotAngleVertical <= 85; shotAngleVertical++)
                    {
                        shotDirectionY = Mathf.Sin(shotAngleVertical * Mathf.Deg2Rad); // 角度をラジアンへ
                        shotDirectionVector = new Vector3(goalVector.x, shotDirectionY, goalVector.z).normalized;
                        maxShotSpeedVector = shotDirectionVector * maxShotSpeed;
                        GameObject fallPointObject = null;
                        FindNewTargetByRayCast(shotAngleVertical, maxShotSpeedVector, out fallPointObject);
                        if (fallPointObject.tag == TagList.Finish.ToString())
                        {
                            _shotDirection = shotDirectionVector;
                            speed = maxShotSpeed;
                            Debug.Log("ゴール発見");
                            _searchEnd = true;
                            return;
                        }
                    }
                }
                if (_isDebugMode)
                {
                    //    return;
                }
            }
            //ゴールに届かない場合、ゴールの標的をひとつにして、最も近い場所を目指す
            goalVector = (goalObject[0].transform.position - transform.position).normalized;
            //ゴールが届く範囲にない場合、最もゴールに近い場所へ移動
            //1 垂直方向角度45度(仮)かつ最大パワーで回転させてレイを飛ばす(仮:1度刻み) 落下地点を配列に格納 2 落下地点の中でゴールに近いものを選ぶ 
            //3条件を加える 自分の近くではない(距離 5未満)、かつ床ではない 4食材の挙動を考慮して床から距離を置く?不要 タグfloorを検出した最後の角度から 仮：プラスマイナス5度以上離れているか確認 
            const int incrementValue = 2;//角度の増加量
            var goalVectorXZ = new Vector2(goalVector.x, goalVector.z).normalized;
            int goalAngle = (int)(Mathf.Asin(goalVectorXZ.y) * Mathf.Rad2Deg);//水平面におけるゴールへのベクトルと、ワールドx軸と平行な平面のなす角度
            int horizontalAngle = goalAngle;
            //重ければ配列へ fallPointCount  (int)((halfLimitAngle * 2) / incrementValue); これをさらにVerticalAngle分用意する 
            int verticalAngle = 45;//45度は落下確率高い 落下地点のy座標が今いる座標よりそれなりに(仮:1m)低いなら角度を上げる
            shotDirectionY = Mathf.Sin(verticalAngle * Mathf.Deg2Rad); // 角度をラジアンへ
            StartCoroutine(DecideVerticalAngleCoroutine(goalObject[0], incrementValue, goalAngle, horizontalAngle));
        }

        /// <summary>
        /// 落下地点の座標を得る・オブジェクトのタグを得る・角度を決める
        /// </summary>
        /// <param name="verticalAngle"></param>
        /// <param name="goal"></param>
        /// <param name="maxShotSpeed"></param>
        /// <param name="i"></param>
        /// <param name="shotDirectionY"></param>
        /// <param name="maxShotSpeedVector"></param>
        /// <param name="halfLimitAngle"></param>
        /// <param name="incrementValue"></param>
        /// <param name="goalAngle"></param>
        /// <param name="horizontalAngle"></param>
        /// <param name="fallPointIndex"></param>
        /// <param name="fallPointCount"></param>
        /// <param name="fallPoint"></param>
        /// <param name="fallPointGameObject"></param>
        /// <param name="goalDistanceFromFallPoints"></param>
        /// <returns></returns>
        IEnumerator DecideVerticalAngleCoroutine(GameObject goal, int incrementValue, int goalAngle, int horizontalAngle)
        {
            Debug.Log(76);
            List<Vector3> fallPoint = new List<Vector3>();
            List<float> goalDistanceFromFallPoints = new List<float>();
            //速度ベクトル 渡す前に決める
            List<Vector3> shotSpeedVectorList = new List<Vector3>();
            //速度の大きさ 戻り値
            List<float> shotSpeedList = new List<float>();
            var goalVector = (goal.transform.position - transform.position).normalized;
            //重ければ減らす必要あり
            int i = 0;
            int halfLimitAngle = 90;//全方向では処理が重い
            int fallPointCount = (int)((halfLimitAngle * 2) / incrementValue);
            int fallPointIndex = 0;
            var verticalAngle = 45;//45度は落下確率高い 落下地点のy座標が今いる座標よりそれなりに(仮:1m)低いなら角度を上げる 
            var shotDirectionY = Mathf.Sin(verticalAngle * Mathf.Deg2Rad); // 角度をラジアンへ
            for (verticalAngle = 20; verticalAngle <= ShotManager.Instance.ShotParameter.LimitVerticalMaxAngle; verticalAngle += 2)
            {
                for (i = 0; horizontalAngle < goalAngle + halfLimitAngle; i++, horizontalAngle = goalAngle + i * incrementValue)
                {
                    GetFallPointObjectByRayCast(goal, horizontalAngle, fallPoint, goalDistanceFromFallPoints, shotSpeedVectorList, shotSpeedList, ref fallPointIndex, verticalAngle, shotDirectionY);
                }
                yield return null;
                horizontalAngle = goalAngle - 1 * incrementValue;
                //角度が偶数奇数でずれる可能性があるので上限条件は数
                for (i = 1; fallPointIndex < fallPointCount; i++, horizontalAngle = goalAngle - i * incrementValue)
                {
                    GetFallPointObjectByRayCast(goal, horizontalAngle, fallPoint, goalDistanceFromFallPoints, shotSpeedVectorList, shotSpeedList, ref fallPointIndex, verticalAngle, shotDirectionY);
                    //Debug.Log(fallPoint[i]);
                }
                //if (verticalAngle % 5 == 0)
                {
                    yield return null;
                }
            }
            int minDistanceIndex = goalDistanceFromFallPoints.IndexOf(goalDistanceFromFallPoints.Min());
            //最小値に向かって飛ぶ レイ計算で使った値もリストに格納
            _shotDirection = shotSpeedVectorList[minDistanceIndex];// CalculateVelocity(this.transform.position, fallPoint[minDistanceIndex], 60);
            speed = shotSpeedList[minDistanceIndex];
            Debug.Log(shotSpeedList[minDistanceIndex]);
            Debug.Log(fallPoint[minDistanceIndex]);
            Debug.Log(shotSpeedVectorList[minDistanceIndex]);
            _searchEnd = true;
        }

        private void GetFallPointObjectByRayCast(GameObject goal, int horizontalAngle, List<Vector3> fallPoint, List<float> goalDistanceFromFallPoints, List<Vector3> shotSpeedVectorList, List<float> shotSpeed, ref int fallPointIndex, int verticalAngle, float shotDirectionY)
        {
            GameObject fallPointGameObject;
            var shotDirectionVector = GetShotDirectionVector(horizontalAngle, shotDirectionY);
            GetFallPointByRayCast(out fallPointGameObject, goal, verticalAngle, shotDirectionVector, fallPointIndex, fallPoint, goalDistanceFromFallPoints, shotSpeedVectorList, shotSpeed);//方向 速度の両方を格納 別々の変数
            if (fallPointGameObject.tag != TagList.Floor.ToString() && fallPointGameObject.tag != TagList.NotBeAITarget.ToString())
            {
                fallPointIndex++;
            }
        }

        private Vector3 GetShotDirectionVector(int horizontalAngle, float shotDirectionY)
        {
            var shotDirectionX = Mathf.Cos(horizontalAngle * Mathf.Deg2Rad); // 角度をラジアンへ
            var shotDirectionZ = Mathf.Sin(horizontalAngle * Mathf.Deg2Rad); // 角度をラジアンへ
            return new Vector3(shotDirectionX, shotDirectionY, shotDirectionZ).normalized;
        }

        //outで渡した落下地点(補正をかける前の落下予測地点をメソッドの中で最初に格納)を呼び出し側でListに入れる 戻り値が改善された速度の大きさ
        private float ImproveShotPointAndShotSpeedVector(out GameObject fallPointGameObject, Vector3 shotDirectionVector, int verticalAngle, out Vector3 fallPoint)
        {
            //最終チェックによる改善
            //AIが落下しそうな場所に飛ばないようにする
            //方法　最終verticalAngleで maxpowerを大きくする maxpower + 1 ~ 3まで  横で±5度 maxpower固定(仮)
            //この範囲で床が存在しないかチェックする 床があったら床を検知したところから遠くなるように角度とパワーを調節する  同時にやれば角度の変化は不要 小さい時の落下を作るとしたらスキップになるが現状無し
            //このチェックの結果で最適化されたポジションを目標地点候補の中に入れる
            NearFallFloorPower nearFallFloorPower = NearFallFloorPower.None;
            float maxShotSpeed = ShotManager.Instance.ShotParameter.MaxShotPower;
            //基本は最大パワーでうつ
            var checkSpeedByIncrease = maxShotSpeed;
            //補正無しの値
            fallPoint = FindNewTargetByRayCast(verticalAngle, shotDirectionVector * maxShotSpeed, out fallPointGameObject);
            var originFallPointObject = fallPointGameObject;
            var newFallPointPowerIncrease = Vector3.zero;
            //パワー増加
            for (int i = 1; checkSpeedByIncrease <= maxShotSpeed + 3; i++)
            {
                //速度を大きくしていき、落ちたら最大パワーより小さくする処理
                checkSpeedByIncrease = maxShotSpeed + i / 2;
                newFallPointPowerIncrease = FindNewTargetByRayCast(verticalAngle, shotDirectionVector * checkSpeedByIncrease, out fallPointGameObject);//情報のずれ怖い
                if (fallPointGameObject.tag == TagList.Floor.ToString() || fallPointGameObject.tag == TagList.NotBeAITarget.ToString())
                {
                    switch (foodType)
                    {
                        case FoodType.Shrimp:
                            checkSpeedByIncrease = maxShotSpeed + (i / 2 - 4);
                            break;
                        case FoodType.Egg:
                            checkSpeedByIncrease = maxShotSpeed + (i / 2 - 4.5f);
                            break;
                        case FoodType.Chicken:
                            checkSpeedByIncrease = maxShotSpeed + (i / 2 - 4);
                            break;
                        case FoodType.Sausage:
                            checkSpeedByIncrease = maxShotSpeed + (i / 2 - 4);
                            break;
                        default:
                            break;
                    }
                    nearFallFloorPower = NearFallFloorPower.Strong;
                    //パワーのチェック終了
                    break;
                }
            }
            //パワー減少 →不要 最大パワーよりも大きくすることはできないため、小さくて落下してより強く打つことはできない
            switch (nearFallFloorPower)
            {
                case NearFallFloorPower.None:
                    //fallPointは補正無しの値
                    //ゲームオブジェクトは元に戻す
                    fallPointGameObject = originFallPointObject;
                    return maxShotSpeed;
                case NearFallFloorPower.Strong:
                    //ゲームオブジェクトは変更
                    fallPoint = newFallPointPowerIncrease;
                    return checkSpeedByIncrease;
                default:
                    break;
            }
            return 0f;
        }

        private void GetFallPointByRayCast(out GameObject fallPointGameObject, GameObject goal, int verticalAngle, Vector3 shotDirectionVector, int fallPointIndex,
            List<Vector3> fallPoint, List<float> goalDistanceFromFallPoints, List<Vector3> shotSpeedVectorList, List<float> shotSpeedList)
        {
            float maxShotSpeed = ShotManager.Instance.ShotParameter.MaxShotPower;
            var maxShotSpeedVector = shotDirectionVector * maxShotSpeed;
            fallPointGameObject = null;
            var fallPosition = Vector3.zero;
            var shotSpeed = ImproveShotPointAndShotSpeedVector(out fallPointGameObject, shotDirectionVector, verticalAngle, out fallPosition);
            if (fallPointGameObject.tag != TagList.NotBeAITarget.ToString())
                AddVariablesToLists(verticalAngle, fallPointGameObject, goal, shotDirectionVector, fallPointIndex, fallPoint, goalDistanceFromFallPoints, shotSpeedVectorList, shotSpeedList, fallPosition, shotSpeed);
        }
        private void AddVariablesToLists(int verticalAngle, GameObject fallPointGameObject, GameObject goal, Vector3 shotDirectionVector, int fallPointIndex, List<Vector3> fallPoint, List<float> goalDistanceFromFallPoints, List<Vector3> shotSpeedVectorList, List<float> shotSpeedList, Vector3 fallPosition, float shotSpeed)
        {
            if (fallPointGameObject != null)
            {
                if (fallPointGameObject.tag != TagList.Floor.ToString() && fallPointGameObject.tag != TagList.NotBeAITarget.ToString())
                {
                    //いす(クッション部分だけChair)は真ん中(transform.position)オンリーに飛ぶ
                    if (fallPointGameObject.tag == TagList.Chair.ToString())
                    {
                        fallPoint.Add(fallPointGameObject.transform.position);
                        goalDistanceFromFallPoints.Add(Vector3.Distance(goal.transform.position, fallPointGameObject.transform.position));
                        shotSpeedVectorList.Add(CalculateVelocity(groundPoint, fallPointGameObject.transform.position, verticalAngle));
                        shotSpeedList.Add(shotSpeed);
                    }
                    else if (fallPosition.y >= StageSceneManager.Instance.ChairYPosition)
                    {
                        fallPoint.Add(fallPosition);
                        goalDistanceFromFallPoints.Add(Vector3.Distance(goal.transform.position, fallPosition));
                        shotSpeedVectorList.Add(shotDirectionVector);
                        shotSpeedList.Add(shotSpeed);
                    }
                }
            }
        }

        private void GetTargetObjects()
        {
            if (rareSeasoningEffect == null)
            {
                //現状同じレベル レア優先でそれ以外狙わない
                if (StageSceneManager.Instance.AILevels[0] == AILevel.Hard)
                {
                    foreach (var seasoning in GimmickManager.Instance.TargetObjectsForAI[(int)AITargetObjectTags.Seasoning])
                    {
                        //調味料は無い可能性あり   
                        if (seasoning.GetComponent<Seasoning>().RareEffect.activeInHierarchy)
                        {
                            _targetObjectOptions.Add(seasoning);
                            return;
                        }
                    }
                }
            }
            //タオル
            if (GetComponent<PlayerPoint>().IsFirstTowel)
                _targetObjectOptions.AddRange(GimmickManager.Instance.TargetObjectsForAI[(int)AITargetObjectTags.TowelAbovePoint]);
            //調味料 倍になるのはポイントがあるときのみ
            if (playerPoint.Point > 100)
            {
                switch (foodType)
                {
                    case FoodType.Shrimp:
                        if (!IsSeasoningMaterial)
                        {
                            foreach (var seasoning in GimmickManager.Instance.TargetObjectsForAI[(int)AITargetObjectTags.Seasoning])
                            {
                                //Debug.Log(seasoning.activeInHierarchy);
                                //調味料は無い可能性あり   
                                if (seasoning.activeInHierarchy)
                                {
                                    _targetObjectOptions.Add(seasoning);
                                }
                            }
                        }
                        break;
                    case FoodType.Egg:
                        SearchSeasoning();
                        break;
                    case FoodType.Chicken:
                        SearchSeasoning();
                        break;
                    case FoodType.Sausage:
                        SearchSeasoning();
                        break;
                    default:
                        break;
                }
            }
            //ナイフ
            switch (foodType)
            {
                case FoodType.Shrimp:
                    if (!food.shrimp.IsHeadFallOff)
                    {
                        foreach (var knife in GimmickManager.Instance.TargetObjectsForAI[(int)AITargetObjectTags.Knife])
                        {
                            _targetObjectOptions.Add(knife);
                        }
                    }
                    break;
                case FoodType.Egg:
                    break;
                case FoodType.Chicken:
                    if (!food.chicken.IsCut)
                    {
                        foreach (var knife in GimmickManager.Instance.TargetObjectsForAI[(int)AITargetObjectTags.Knife])
                        {
                            _targetObjectOptions.Add(knife);
                        }
                    }
                    break;
                case FoodType.Sausage:
                    if (!food.sausage.IsCut)
                    {
                        foreach (var knife in GimmickManager.Instance.TargetObjectsForAI[(int)AITargetObjectTags.Knife])
                        {
                            _targetObjectOptions.Add(knife);
                        }
                    }
                    break;
                default:
                    break;
            }
            //水 洗っていないかつ調味料がついていないとき
            if (PlayerPointProperty.IsFirstWash)
            {
                switch (foodType)
                {
                    case FoodType.Shrimp:
                        if (!IsSeasoningMaterial)
                        {
                            SearchWater();
                        }
                        break;
                    case FoodType.Egg:
                        if (!IsSeasoningMaterial)
                        {
                            SearchWater();
                        }
                        break;
                    case FoodType.Chicken:
                        if (!IsSeasoningMaterial)
                        {
                            SearchWater();
                        }
                        break;
                    case FoodType.Sausage:
                        if (!IsSeasoningMaterial)
                        {
                            SearchWater();
                        }
                        break;
                    default:
                        break;
                }
            }
            //他の食材 自分が調味料を持っていないとき狙う AI同士で狙い続けるのは面白くないので、食材ごとに乱数生成
            if (!IsSeasoningMaterial)
            {
                foreach (var otherFood in TurnManager.Instance.FoodStatuses)
                {
                    if (otherFood != this)
                    {
                        int seed = Cooking.Random.GetRandomIntFromZero(10);
                        //自分のポイントが高い時は高確率で狙う
                        if (playerPoint.Point > 300)
                        {
                            //80%で狙う
                            if (seed < 8)
                            {
                                if (!otherFood.IsFoodInStartArea && otherFood.IsSeasoningMaterial)
                                {
                                    _targetObjectOptions.Add(otherFood.gameObject);
                                }
                            }
                        }
                        else
                        {
                            //50%で狙う
                            if (seed < 5)
                            {
                                if (!otherFood.IsFoodInStartArea && otherFood.IsSeasoningMaterial)
                                {
                                    _targetObjectOptions.Add(otherFood.gameObject);
                                }
                            }
                        }
                    }
                }
            }
            if (_targetObjectOptions.Count > 0)
            {
                foreach (var targetObjectOption in _targetObjectOptions)
                {
                    _distances.Add(Vector3.Distance(targetObjectOption.transform.position, this.transform.position));
                }
            }

            // Debug.Log(_distances.Count);
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

        private void SearchWater()
        {
            foreach (var water in GimmickManager.Instance.TargetObjectsForAI[(int)AITargetObjectTags.Water])
            {
                //水は出ていない可能性あり   
                if (water.activeInHierarchy)
                {
                    _targetObjectOptions.Add(water);
                }
            }
        }

        private void SearchSeasoning()
        {
            if (!IsSeasoningMaterial)
            {
                foreach (var seasoning in GimmickManager.Instance.TargetObjectsForAI[(int)AITargetObjectTags.Seasoning])
                {
                    Debug.Log(seasoning.activeInHierarchy);
                    //調味料は無い可能性あり   
                    if (seasoning.activeInHierarchy)
                    {
                        _targetObjectOptions.Add(seasoning);
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
        /// speedに補正をかけて、実際にショットされる速度を難易度に応じて算出
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
            //射出角度
            float throwingAngle = ShotManager.Instance.ShotParameter.LimitVerticalMaxAngle;
            //相対関係によって検索方向を変える
            if (targetObject.transform.position.y > this.transform.position.y)
            {
                //射出角度 初期値45度 レイにより障害物判定で変える
                throwingAngle = ShotManager.Instance.ShotParameter.LimitVerticalMaxAngle;
                for (int i = 0; throwingAngle >= 45; i++)
                {
                    //角度減少で検索
                    throwingAngle = ShotManager.Instance.ShotParameter.LimitVerticalMaxAngle - i;//重いので1度刻み
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

            }
            else
            {
                //射出角度 初期値45度 レイにより障害物判定で変える 加算していき85(=限界角度)でだめなら45度から減少
                throwingAngle = 45;
                for (int i = 0; throwingAngle <= ShotManager.Instance.ShotParameter.LimitVerticalMaxAngle; i++)
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
            //_ignoreObjects.Add(targetObject);
            //仮のショット決定                                                          //VerticalAngle
            _shotDirection = CalculateVelocity(this.transform.position, targetPosition, 60);
            return false;
        }

        private bool CastRayByChangingShotAngle(float throwingAngle, Vector3 velocity, Vector3 direction)
        {
            //方向取得・初速の大きさも計算
            GameObject newTarget = this.gameObject;
            FindNewTargetByRayCast(throwingAngle, velocity, out newTarget);
            if (newTarget != null)
            {
                //タオルの時は、レイによるオブジェクトタグ取得名はTowel であって TowelAbovePointではない 名前異なる
                if (_targetTagByTransform == AITargetObjectTags.TowelAbovePoint)
                {
                    if (newTarget.tag == TagList.Towel.ToString())
                    {
                        _shotDirection = direction;
                        return true;
                    }
                }
                //食材の時はタグではなくコンポーネントクラスを持つかどうかで判定
                else if (_targetTagByTransform == AITargetObjectTags.Food)
                {
                    if (newTarget.GetComponent<FoodStatus>() != null)
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
        /// 食材の種類に対応した当たり判定レイキャストを用いて、新たなる標的を探す 落下地点の座標を返す
        /// </summary>
        /// <param name="verticalAngle"></param>
        /// <param name="firstSpeedVector">初速度ベクトル(方向×大きさ)</param>
        /// <param name="newTarget"></param>
        /// <returns></returns>
        private Vector3 FindNewTargetByRayCast(float verticalAngle, Vector3 firstSpeedVector, out GameObject newTarget)
        {
            switch (foodType)
            {
                case FoodType.Shrimp:
                    return PredictFoodPhysics.PredictFallPointByBoxRayCast(out newTarget, transform.position, firstSpeedVector, verticalAngle, foodType, GetColliderSize<Vector3>());
                case FoodType.Egg:
                    if (food.egg.HasBroken)
                    {
                        return PredictFoodPhysics.PredictFallPointByBoxRayCast(out newTarget, transform.position, firstSpeedVector, verticalAngle, foodType, GetColliderSize<Vector3>());
                    }
                    else
                    {
                        return PredictFoodPhysics.PredictFallPointByBoxRayCast(out newTarget, transform.position, firstSpeedVector, verticalAngle, foodType, GetColliderSize<Vector2>());
                    }
                case FoodType.Chicken:
                    return PredictFoodPhysics.PredictFallPointByBoxRayCast(out newTarget, transform.position, firstSpeedVector, verticalAngle, foodType, GetColliderSize<Vector3>());
                case FoodType.Sausage:
                    return PredictFoodPhysics.PredictFallPointByBoxRayCast(out newTarget, transform.position, firstSpeedVector, verticalAngle, foodType, GetColliderSize<Vector3>());
                default:
                    break;
            }
            newTarget = null;
            return Vector3.zero;
        }
        Vector3 CompareShotVectors(Vector3 startPosition ,Vector3 targetPosition)
        {
            for (int i = Mathf.CeilToInt(ShotManager.Instance.ShotParameter.MinShotPower); i <= ShotManager.Instance.ShotParameter.MaxShotPower; i++)
            {
                //結果速度がspeedに入る 方向はXZだけ必ず同じ
                var firstVectorDirection = CalculateVelocity(startPosition, targetPosition, i);
                float directionY = Mathf.Sin(i * Mathf.Deg2Rad);
                var newFirstVectorDirection = new Vector3(firstVectorDirection.x, directionY, firstVectorDirection.z);
                if (Mathf.Abs(ShotManager.Instance.AICalculateMaxShotPowerVector(i, newFirstVectorDirection, speed).y - firstVectorDirection.y )< 0.1f)
                {
                    return ShotManager.Instance.AICalculateMaxShotPowerVector(i, newFirstVectorDirection, speed);
                }
            }
            return Vector3.zero;
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
            switch (foodType)
            {
                case FoodType.Shrimp:
                    if (foodSkinnedMeshRenderer[(int)ShrimpParts.Tail].material.mainTexture == StageSceneManager.Instance.FoodTextureList.normalTextures[(int)FoodType.Shrimp])
                    {
                        _targetTagByTransform = AITargetObjectTags.Seasoning;
                    }
                    break;
                case FoodType.Egg:

                    break;
                case FoodType.Chicken:
                    break;
                case FoodType.Sausage:
                    break;
                default:
                    break;
            }
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

    }
}
