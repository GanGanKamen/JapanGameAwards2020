using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// 発生するギミックの制御 水・あわ・調味料
    /// </summary>
    public class GimmickManager : MonoBehaviour
    {
        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// このクラスのインスタンスを取得。
        /// </summary>
        public static GimmickManager Instance
        {
            get { return _instance; }
        }
        static GimmickManager _instance = null;
        /// <summary>
        /// Start()より先に実行
        /// </summary>
        private void Awake()
        {
            _instance = this;
        }

        #endregion
        public GameObject[] Water
        {
            get { return _water; }
        }
        private GameObject[] _water = null;
        public GameObject[] Seasonings
        {
            get { return _seasonings; }
        }
        private GameObject[] _seasonings = null;
        public GameObject[] TargetTowelPositionObjects
        {
            get { return _targetTowelPositionObjects; }
        }
        private GameObject[] _targetTowelPositionObjects;
        private GameObject _rareSeasoning;
        /// <summary>
        /// レア調味料を持っているか判定用
        /// </summary>
        public Material RareMaterial
        {
            get { return _rareMaterial; }
        }
        [SerializeField] private Material _rareMaterial = null;
        /// <summary>
        /// シーン内に存在するあわ
        /// </summary>
        public GameObject Bubble
        {
            get { return _bubble; }
        }
        /// <summary>
        /// AI用 シーン内にあるあわ
        /// </summary>
        private GameObject _bubble;
        private float _changeDirectionTimeCounterOfBubble;
        /// <summary>
        /// あわの進行方向を変えるまでにかかる時間(乱数取得)
        /// </summary>
        private float _changeDirectionTimeOfBubble;
        /// <summary>
        /// ランダムウォーク用
        /// </summary>
        Rigidbody _bubbleRigidbody;
        /// <summary>
        ///あわ発生領域の2端(x.y.zの最小値と最大値)の座標 この2点の間の座標に発生させる
        /// </summary>
        Vector3[] _bubbleLimitPosition;

        enum LimitValue
        {
            Min,Max
        }
        private Vector3 _speedVectorOfBubble;
        [SerializeField] private float _maxSpeedOfBubble = 2f;
        private GameObject _bubbleInstantiateZone;
        [SerializeField] private GameObject _bubblePrefab = null;
        /// <summary>
        /// 現状固定位置 皿の上の中からランダム？
        /// </summary>
        private Vector3[] _instantiateSeasoningPoint;
        [SerializeField] GameObject _seasoningPrefab = null;
        [SerializeField] GameObject rareSeasoningEffect;
        // Start is called before the first frame update
        void Start()
        {
            GameObjectFindAndInitialize();
        }
        /// <summary>
        /// オブジェクトを探し、データの初期化を行う
        /// </summary>
        private void GameObjectFindAndInitialize()
        {
            //処理を早くするタグ検索
            _water = GameObject.FindGameObjectsWithTag("Water");
            _seasonings = GameObject.FindGameObjectsWithTag("Seasoning");
            _rareSeasoning = GameObject.FindGameObjectWithTag("RareSeasoning");
            _bubbleInstantiateZone = GameObject.FindGameObjectWithTag("BubbleZone");
            _bubbleInstantiateZone.SetActive(false);
            var towelsAbovePoint = GameObject.FindGameObjectsWithTag("TowelAbovePoint");
            _targetTowelPositionObjects = new GameObject[towelsAbovePoint.Length];
            _instantiateSeasoningPoint = new Vector3[_seasonings.Length];
            for (int i = 0; i < _seasonings.Length; i++)
            {
                _instantiateSeasoningPoint[i] = _seasonings[i].transform.position;
            }
            _rareSeasoning.SetActive(false);
            InstantiateBubbles();
        }

        private void InstantiateBubbles()
        {
            var referencePoint = _bubbleInstantiateZone.transform.GetChild(0).position;
            _bubble = Instantiate(_bubblePrefab);
            _bubbleRigidbody = _bubble.GetComponent<Rigidbody>();
            _bubbleLimitPosition = new Vector3[] { referencePoint, referencePoint + _bubbleInstantiateZone.transform.localScale };
            _bubble.transform.position = new Vector3(Cooking.Random.GetRandomFloat(_bubbleLimitPosition[(int)LimitValue.Min].x, _bubbleLimitPosition[(int)LimitValue.Max].x), Cooking.Random.GetRandomFloat(_bubbleLimitPosition[(int)LimitValue.Min].y, _bubbleLimitPosition[(int)LimitValue.Max].y), Cooking.Random.GetRandomFloat(_bubbleLimitPosition[(int)LimitValue.Min].z, _bubbleLimitPosition[(int)LimitValue.Max].z));
            ChangeBubbleSpeedVectorDirection();
        }

        // Update is called once per frame
        void Update()
        {
            if(_bubble != null)
            ManageBubbleMove();
        }
        /// <summary>
        /// あわの挙動の管理
        /// </summary>
        private void ManageBubbleMove()
        {
            if (_bubble.transform.position.x < _bubbleLimitPosition[(int)LimitValue.Min].x || _bubble.transform.position.y < _bubbleLimitPosition[(int)LimitValue.Min].y || _bubble.transform.position.z < _bubbleLimitPosition[(int)LimitValue.Min].z )
            {
                ChangeBubbleSpeedVectorDirection();
            }
            else if (_bubble.transform.position.x > _bubbleLimitPosition[(int)LimitValue.Max].x || _bubble.transform.position.y > _bubbleLimitPosition[(int)LimitValue.Max].y || _bubble.transform.position.z > _bubbleLimitPosition[(int)LimitValue.Max].z )
            {
                ChangeBubbleSpeedVectorDirection();
            }
            else if (_changeDirectionTimeCounterOfBubble >= _changeDirectionTimeOfBubble)
            {
                ChangeBubbleSpeedVectorDirection();
            }
            else
            {
                _changeDirectionTimeCounterOfBubble += Time.deltaTime;
            }
            _bubbleRigidbody.velocity = _speedVectorOfBubble;
        }
        private void ChangeBubbleSpeedVectorDirection()
        {
            _speedVectorOfBubble = new Vector3(Cooking.Random.GetRandomFloat(-_maxSpeedOfBubble, _maxSpeedOfBubble), Cooking.Random.GetRandomFloat(-_maxSpeedOfBubble, _maxSpeedOfBubble), Cooking.Random.GetRandomFloat(-_maxSpeedOfBubble, _maxSpeedOfBubble));
            _changeDirectionTimeOfBubble = Cooking.Random.GetRandomFloat(1, 5);
            _changeDirectionTimeCounterOfBubble = 0;
        }
        public void AppearRareSeasoning()
        {
            _rareSeasoning.SetActive(true);
            rareSeasoningEffect.SetActive(true);
        }
        /// <summary>
        /// ターンが変わるときに水が出なくなる場所を制御
        /// </summary>
        public void WaterManager()
        {
            var seedId = Cooking.Random.GetRandomInt(_water.Length);
            for (int i = 0; i < _water.Length; i++)
            {
                if (seedId == i)
                {
                    _water[i].SetActive(false);
                }
                else
                {
                    _water[i].SetActive(true);
                }
            }
        }
        /// <summary>
        /// 調味料の出現の制御
        /// </summary>
        public void SeasoningManager()
        {
            for (int i = 0; i < _seasonings.Length; i++)
            {
                ///実装予定 現状プレイヤーの位置が発生位置とかぶっていても発生するのでそれを避ける
                //if (TurnController.Instance.foodStatuses[TurnController.Instance.ActivePlayerIndex].transform.position)
                //{

                //}
                if (_seasonings[i] == null )
                {
                    ///  x / (10)%の確率で再出現
                    if (Cooking.Random.GetRandomInt(10) < 3)
                    {
                        var newSeasoning = Instantiate(_seasoningPrefab);
                        newSeasoning.transform.position = _instantiateSeasoningPoint[i];
                        _seasonings[i] = newSeasoning;//登録することでAIが検知可能に
                    }
                }
            }
        }
    }
}
