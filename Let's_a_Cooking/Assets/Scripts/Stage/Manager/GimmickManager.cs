using System;
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
        /// <summary>
        /// レア調味料を持っているか判定用
        /// </summary>
        public Material RareMaterial
        {
            get { return _rareMaterial; }
        }
        [SerializeField] private Material _rareMaterial = null;
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
        private Vector3 _speedVectorOfBubble;
        [SerializeField] private float _maxSpeedOfBubble = 2f;
        private GameObject _bubbleInstantiateZone;
        [SerializeField] private GameObject _bubblePrefab = null;
        /// <summary>
        /// 調味料の子オブジェクト:その領域にプレイヤーがいるかを判定するエリアオブジェクト
        /// </summary>
        private Transform[] _instantiateSeasoningPoint;
        [SerializeField] GameObject _seasoningPrefab = null;
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
            GameObjectFindAndInitialize();
        }
        /// <summary>
        /// オブジェクトを探し、データの初期化を行う
        /// </summary>
        private void GameObjectFindAndInitialize()
        {
            _bubbleInstantiateZone = GameObject.FindGameObjectWithTag(TagList.BubbleZone.ToString());
            _bubbleInstantiateZone.SetActive(false);
            InstantiateBubbles();//生成後に検索
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
                _instantiateSeasoningPoint[i] = _targetObjectsForAI[(int)AITargetObjectTags.Seasoning][i].transform.GetChild(0);
                //親子関係があると親Destroy時に削除されてしまう
                _instantiateSeasoningPoint[i].parent = null;
            }
            //検索後にオフ
            foreach (var rareSeasoning in _targetObjectsForAI[(int)AITargetObjectTags.RareSeasoning])
            {
                rareSeasoning.SetActive(false);
            }
        }
        /// <summary>
        /// 登録はこのメソッドの後に行う(配列の実体化後)
        /// </summary>
        private void InstantiateBubbles()
        {
            var referencePoint = _bubbleInstantiateZone.transform.GetChild(0).position;
            var bubble = Instantiate(_bubblePrefab);
            _bubbleRigidbody = bubble.GetComponent<Rigidbody>();
            _bubbleLimitPosition = new Vector3[] { referencePoint, referencePoint + _bubbleInstantiateZone.transform.localScale };
            bubble.transform.position = new Vector3(Cooking.Random.GetRandomFloat(_bubbleLimitPosition[(int)LimitValue.Min].x, _bubbleLimitPosition[(int)LimitValue.Max].x), Cooking.Random.GetRandomFloat(_bubbleLimitPosition[(int)LimitValue.Min].y, _bubbleLimitPosition[(int)LimitValue.Max].y), Cooking.Random.GetRandomFloat(_bubbleLimitPosition[(int)LimitValue.Min].z, _bubbleLimitPosition[(int)LimitValue.Max].z));
            ChangeBubbleSpeedVectorDirection();
        }

        // Update is called once per frame
        void Update()
        {
            if(_targetObjectsForAI[(int)AITargetObjectTags.Bubble][0] != null)
            ManageBubbleMove(_targetObjectsForAI[(int)AITargetObjectTags.Bubble][0]);//現状1つ
        }
        /// <summary>
        /// あわの挙動の管理
        /// </summary>
        private void ManageBubbleMove(GameObject bubble)
        {
            if (bubble.transform.position.x < _bubbleLimitPosition[(int)LimitValue.Min].x || bubble.transform.position.y < _bubbleLimitPosition[(int)LimitValue.Min].y || bubble.transform.position.z < _bubbleLimitPosition[(int)LimitValue.Min].z )
            {
                ChangeBubbleSpeedVectorDirection();
            }
            else if (bubble.transform.position.x > _bubbleLimitPosition[(int)LimitValue.Max].x || bubble.transform.position.y > _bubbleLimitPosition[(int)LimitValue.Max].y || bubble.transform.position.z > _bubbleLimitPosition[(int)LimitValue.Max].z )
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
            foreach (var rareSeasoning in _targetObjectsForAI[(int)AITargetObjectTags.RareSeasoning])
            {
                rareSeasoning.SetActive(true);
            }
            _rareSeasoningEffect.SetActive(true);
        }
        /// <summary>
        /// ターンが変わるときに水が出なくなる場所を制御
        /// </summary>
        public void WaterManager()
        {
            var seedId = Cooking.Random.GetRandomInt(_targetObjectsForAI[(int)AITargetObjectTags.Water].Count + 1);//確率調整
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
                    //indexは同期されている
                    var seasoning = _targetObjectsForAI[(int)AITargetObjectTags.Seasoning][i];
                    if (seasoning != null)
                    {
                        break;//この調味料での確認は不要 次の調味料へ
                    }
                    else
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
                            InstantiateSeasoning(i, _instantiateSeasoningPoint[i].position);
                        }
                    }
                }
            }
        }

        private void InstantiateSeasoning(int seasoningIndex , Vector3 newSeasoningPosition)
        {
            ///  x(右辺) / 10(左辺)%の確率で再出現
            if (Cooking.Random.GetRandomInt(10) < 3)
            {
                var newSeasoning = Instantiate(_seasoningPrefab);
                newSeasoning.transform.position = newSeasoningPosition;
                _targetObjectsForAI[(int)AITargetObjectTags.Seasoning][seasoningIndex] = newSeasoning;//登録することでAIが検知可能に
            }
        }
    }
}
