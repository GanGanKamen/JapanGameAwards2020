using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// 発生するギミックの制御 水・泡・調味料
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
        public Vector3 FoamPosition
        {
            get { return _foamPosition; }
        }
        /// <summary>
        /// AI用
        /// </summary>
        private Vector3 _foamPosition;
        private GameObject _foamInstantiateZone;
        [SerializeField] private GameObject _foamPrefab = null;
        /// <summary>
        /// 現状固定位置 皿の上の中からランダム？
        /// </summary>
        private Vector3[] _instantiateSeasoningPoint;
        [SerializeField] GameObject _seasoningPrefab = null;
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
            _foamInstantiateZone = GameObject.FindGameObjectWithTag("FoamZone");
            _foamInstantiateZone.SetActive(false);
            var towelsAbovePoint = GameObject.FindGameObjectsWithTag("TowelAbovePoint");
            _targetTowelPositionObjects = new GameObject[towelsAbovePoint.Length];
            _instantiateSeasoningPoint = new Vector3[_seasonings.Length];
            for (int i = 0; i < _seasonings.Length; i++)
            {
                _instantiateSeasoningPoint[i] = _seasonings[i].transform.position;
            }
            _rareSeasoning.SetActive(false);
            InstantiateFoams();
        }

        private void InstantiateFoams()
        {
            var referencePoint = _foamInstantiateZone.transform.GetChild(0).position;
            //泡発生領域の2端の座標 この2点の間の座標に発生させる
            Vector3[] foamInstantiateZone = { referencePoint, referencePoint + _foamInstantiateZone.transform.localScale };
            var foam = Instantiate(_foamPrefab);
            //AI用に保存
            _foamPosition = foam.transform.position = new Vector3(GetFloatSeedID(foamInstantiateZone[0].x, foamInstantiateZone[1].x), GetFloatSeedID(foamInstantiateZone[0].y, foamInstantiateZone[1].y), GetFloatSeedID(foamInstantiateZone[0].z, foamInstantiateZone[1].z));
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void AppearRareSeasoning()
        {
            _rareSeasoning.SetActive(true);
        }
        /// <summary>
        /// ターンが変わるときに水が出なくなる場所を制御
        /// </summary>
        public void WaterManager()
        {
            var seedId = GetIntSeedID(_water.Length);
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
                    if (GetIntSeedID(10) < 3)
                    {
                        var newSeasoning = Instantiate(_seasoningPrefab);
                        newSeasoning.transform.position = _instantiateSeasoningPoint[i];
                        _seasonings[i] = newSeasoning;//登録することでAIが検知可能に
                    }
                }
            }
        }
        /// <summary>
        /// 最小値0から指定した範囲のint乱数発生
        /// </summary>
        /// <returns></returns>
        private int GetIntSeedID(int rangeOfSeedFromZero)
        {
            return Random.Range(0, rangeOfSeedFromZero);
        }
        /// <summary>
        /// 最小値から最大値の間でfloat乱数発生
        /// </summary>
        /// <returns></returns>
        private float GetFloatSeedID(float minSeed , float maxSeed)
        {
            return Random.Range (minSeed, maxSeed);
        }
    }

}
