using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// 発生するギミックの制御 水・あわ・調味料
    /// </summary>
    public class GimmickManager : SingletonInstance<GimmickManager>
    {
        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// Start()より先に実行
        /// </summary>
        protected override void Awake()
        {
            CreateSingletonInstance(this, false);
        }

        #endregion
        /// <summary>
        /// レア調味料を持っているか判定用
        /// </summary>
        public Material RareMaterial
        {
            get { return _rareMaterial; }
        }
        private Material _rareMaterial = null;
        /// <summary>
        /// レア調味料を持っているか判定用
        /// </summary>
        public Material SeasoningMaterial
        {
            get { return _seasoningMaterial; }
        }
        private Material _seasoningMaterial = null;
        /// <summary>
        ///あわ移動許容範囲の2端(x.y.zの最小値と最大値)の座標 この2点の間の座標に発生させる
        /// </summary>
        public Vector3[] BubbleLimitPosition
        {
            get { return _bubbleLimitPosition; }
        }
        /// <summary>
        ///あわ移動許容範囲の2端(x.y.zの最小値と最大値)の座標 この2点の間の座標に発生させる
        /// </summary>
        private Vector3[] _bubbleLimitPosition;
        [SerializeField] private GameObject _bubblePrefab = null;
        /// <summary>
        /// 初期化時よりもあわの数が増える可能性を考慮してList
        /// </summary>
        private List<Bubble> _bubbles = new List<Bubble>();
        /// <summary>
        /// あわを生成するための情報 いつどこに生成するか
        /// </summary>
        struct BubbleInstantiateInformation
        {
            /// <summary>
            /// あわの発生位置 この値に乱数をかけて発生 位置の数はステージプレハブ固定
            /// </summary>
            public Vector3 instantiatePosition;
            /// <summary>
            /// あわを発生させるタイミングを司る プレイヤーが切り替わった回数を数える
            /// </summary>
            public int instantiateTiming;
            /// <summary>
            /// プレイヤーが切り替わるときに加算 この値がinstantiateTimingを上回ると生成される
            /// </summary>
            public int timingCounter;
        }
        private BubbleInstantiateInformation[] _bubbleInstantiateInformation;
        /// <summary>
        /// シーン内に存在しうるあわの数の上限
        /// </summary>
        [SerializeField] private int _bubbleLimitSumNumber = 50;
        /// <summary>
        /// あわが生成されるタイミング 2,8 → 3ターン以内に生成 偶数はターンが切り替わるとき 奇数は後攻のプレイヤーになった時 2で割るとターン数 8 /2 4 4ターン目は含まない
        /// </summary>
        private int[] _bubbleInstantiateTimingRange = { 4, 8 };
        /// <summary>
        /// 調味料の子オブジェクト:その領域にプレイヤーがいるかを判定するエリアオブジェクト
        /// </summary>
        private Transform[] _instantiateSeasoningPoint;
        [SerializeField] GameObject _seasoningPrefab = null;
        /// <summary>
        /// ビンに付いてる星エフェクト
        /// </summary>
        [SerializeField] GameObject _rareSeasoningEffect;
        public List<GameObject>[] TargetObjectsForAI
        {
            get { return _targetObjectsForAI; }
        }
        /// <summary>
        /// AIが目標地点とするシーン内に存在するオブジェクト あわや調味料は数固定とは限らないためリスト
        /// </summary>
        private List<GameObject>[] _targetObjectsForAI = new List<GameObject>[Enum.GetValues(typeof(AITargetObjectTags)).Length];
        // Start is called before the first frame update
        void Start()
        {
            //プレハブ内のアクティブなあわを検索
            var bubbles = FindObjectsOfType<Bubble>();
            _bubbles.AddRange(bubbles);
            _bubbleInstantiateInformation = new BubbleInstantiateInformation[bubbles.Length];
            for (int i = 0; i < bubbles.Length; i++)
            {
                _bubbleInstantiateInformation[i].instantiatePosition = bubbles[i].transform.position;
                _bubbleInstantiateInformation[i].instantiateTiming = GetInitializeInstantiateTiming();
            }
            GameObjectFindAndInitialize();
        }
        /// <summary>
        /// オブジェクトを探し、データの初期化を行う
        /// </summary>
        private void GameObjectFindAndInitialize()
        {
            for (int i = 0; i < Enum.GetValues(typeof(AITargetObjectTags)).Length; i++)
            {
                _targetObjectsForAI[i] = new List<GameObject>();
                //繰り返し変数i番目をenumへ変換し、その文字列を取得
                string targetObjecString = ((AITargetObjectTags)Enum.ToObject(typeof(AITargetObjectTags), i)).ToString();
                //仮の値を入れる
                TagList targetObjectTag = TagList.Finish;
                //AIのターゲットのタグが、タグリストの中にあるかチェック
                targetObjectTag = EnumParseMethod.TryParseAndDebugAssertFormatAndReturnResult(targetObjecString, false, targetObjectTag);
                _targetObjectsForAI[i].AddRange(GameObject.FindGameObjectsWithTag(targetObjectTag.ToString()));
            }
            _instantiateSeasoningPoint = new Transform[_targetObjectsForAI[(int)AITargetObjectTags.Seasoning].Count];
            for (int i = 0; i < _targetObjectsForAI[(int)AITargetObjectTags.Seasoning].Count; i++)
            {
                _instantiateSeasoningPoint[i] = _targetObjectsForAI[(int)AITargetObjectTags.Seasoning][i].GetComponent<Seasoning>().InstantiateSeasoningArea;
                //親子関係があると親Destroy時に削除されてしまう
                _instantiateSeasoningPoint[i].parent = this.transform;
                _instantiateSeasoningPoint[i].GetComponent<MeshRenderer>().enabled = false;//消し忘れ防止
            }
            //検索後にオフ
            foreach (var rareSeasoning in _targetObjectsForAI[(int)AITargetObjectTags.RareSeasoning])
            {
                rareSeasoning.SetActive(false);
            }
        }
        /// <summary>
        /// 生成するために必要な情報 時間・座標を管理しながらあわを生成 プレイヤーが切り替わるタイミングで呼ばれる
        /// </summary>
        public void InstantiateBubbles()
        {
            //シーン内に存在するあわの数に上限を設ける
            if (_bubbles.Count > _bubbleLimitSumNumber)
            {
                Debug.LogFormat("あわが限界{0}個に達しました", _bubbleLimitSumNumber);
                return;
            }
            for (int i = 0; i < _bubbleInstantiateInformation.Length; i++)
            {
                _bubbleInstantiateInformation[i].timingCounter++;
                if (_bubbleInstantiateInformation[i].timingCounter > _bubbleInstantiateInformation[i].instantiateTiming)
                {
                    var newBubble = Instantiate(_bubblePrefab);
                    _bubbles.Add(newBubble.GetComponent<Bubble>());
                    newBubble.transform.position = DecideInstantiatePosition(_bubbleInstantiateInformation[i]);
                    //生成タイミングを変更
                    _bubbleInstantiateInformation[i].instantiateTiming = GetInitializeInstantiateTiming();
                }
            }
        }
        /// <summary>
        /// 補正をかけてあわの生成位置を決定 座標の各要素に-1 ~ 1の範囲で加算して補正をかける
        /// </summary>
        /// <param name="bubbleInstantiateInfo"></param>
        /// <returns></returns>
        private Vector3 DecideInstantiatePosition(BubbleInstantiateInformation bubbleInstantiateInfo)
        {
            //座標の各要素に-1 ~ 1の範囲で加算して補正をかける
            return bubbleInstantiateInfo.instantiatePosition + new Vector3(Cooking.Random.GetRandomFloat(-1, 1), Cooking.Random.GetRandomFloat(-1, 1), Cooking.Random.GetRandomFloat(-1, 1));
        }

        /// <summary>
        /// 生成タイミングを初期化
        /// </summary>
        /// <param name="bubbleInstantiateInfo">あわ生成情報</param>
        private int GetInitializeInstantiateTiming()
        {
            return Cooking.Random.GetRandomInt(_bubbleInstantiateTimingRange[(int)LimitValue.Min], _bubbleInstantiateTimingRange[(int)LimitValue.Max]);
        }

        // Update is called once per frame
        void Update()
        {
            //foreach (var bubble in _bubbles)
            //{
            //    if(bubble != null)
            //        ManageBubbleMove(bubble);
            //}
        }
        /// <summary>
        /// あわの挙動の管理
        /// </summary>
        private void ManageBubbleMove(Bubble bubble)
        {
            if (bubble.transform.position.x < _bubbleLimitPosition[(int)LimitValue.Min].x || bubble.transform.position.y < _bubbleLimitPosition[(int)LimitValue.Min].y || bubble.transform.position.z < _bubbleLimitPosition[(int)LimitValue.Min].z)
            {
                bubble.ReverseBubbleSpeedVectorDirection();
            }
            else if (bubble.transform.position.x > _bubbleLimitPosition[(int)LimitValue.Max].x || bubble.transform.position.y > _bubbleLimitPosition[(int)LimitValue.Max].y || bubble.transform.position.z > _bubbleLimitPosition[(int)LimitValue.Max].z)
            {
                bubble.ReverseBubbleSpeedVectorDirection();
            }
        }
        public void AppearRareSeasoning()
        {
            foreach (var rareSeasoning in _targetObjectsForAI[(int)AITargetObjectTags.RareSeasoning])
            {
                rareSeasoning.SetActive(true);
                rareSeasoning.GetComponent<Seasoning>().ManageRareSeasoning(true);
            }
            //_rareSeasoningEffect.SetActive(true);
        }
        /// <summary>
        /// ターンが変わるときに水が出なくなる場所を制御
        /// </summary>
        public void WaterManager()
        {
            var seedId = Cooking.Random.GetRandomIntFromZero(_targetObjectsForAI[(int)AITargetObjectTags.Water].Count + 1);//確率調整
            for (int i = 0; i < _targetObjectsForAI[(int)AITargetObjectTags.Water].Count; i++)
            {
                if (seedId == i)
                {
                    _targetObjectsForAI[(int)AITargetObjectTags.Water][i].SetActive(false);
                }
                else
                {
                    _targetObjectsForAI[(int)AITargetObjectTags.Water][i].SetActive(true);
                }
            }
        }
        /// <summary>
        /// 調味料の出現の制御
        /// </summary>
        public void SeasoningManager()
        {
            for (int i = 0; i < _instantiateSeasoningPoint.Length; i++)
            {
                //各プレイヤー座標で判定
                foreach (var foodStatus in TurnManager.Instance.FoodStatuses)
                {
                    var referencePoint = _instantiateSeasoningPoint[i].GetChild(0).transform.position;
                    var instantiateLimitPosition = new Vector3[] { referencePoint, referencePoint + _instantiateSeasoningPoint[i].transform.localScale };
                    //各プレイヤー座標で判定
                    var activePlayerPosition = foodStatus.transform.position;
                    //生成可能であるかをこの後判断 プレイヤーの位置が発生位置とかぶっていて発生するのを防ぐ 内側にいるかどうか
                    if (activePlayerPosition.x > instantiateLimitPosition[(int)LimitValue.Min].x && activePlayerPosition.x < instantiateLimitPosition[(int)LimitValue.Max].x
                        && activePlayerPosition.y > instantiateLimitPosition[(int)LimitValue.Min].y && activePlayerPosition.y < instantiateLimitPosition[(int)LimitValue.Max].y
                        && activePlayerPosition.z > instantiateLimitPosition[(int)LimitValue.Min].z && activePlayerPosition.z < instantiateLimitPosition[(int)LimitValue.Max].z)
                    {
                        break;//この調味料を生成しない
                    }
                    else
                    {
                        //indexは同期されている
                        var seasoning = _targetObjectsForAI[(int)AITargetObjectTags.Seasoning][i].GetComponent<Seasoning>();
                        if (!seasoning.gameObject.activeInHierarchy)
                        {
                            SetSeasoningActiveTrue(seasoning);
                        }
                    }
                }
            }
        }

        private void SetSeasoningActiveTrue(Seasoning seasoning)
        {
            var seed = Cooking.Random.GetRandomIntFromZero(20);
            Debug.Log(seed);
            //  x(右辺) / 10(左辺)%の確率で再出現
            if (seed < 2)
            {
                Debug.Log("生成");
                seasoning.ManageSeasoningActive(true);
            }
        }
    }
}
