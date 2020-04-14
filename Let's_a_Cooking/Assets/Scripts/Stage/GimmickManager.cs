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
        private GameObject[] _water;

        public GameObject[] Seasonings
        {
            get { return _seasonings; }
        }
        private GameObject[] _seasonings;
        /// <summary>
        /// 現状固定位置 皿の上の中からランダム？
        /// </summary>
        Vector3[] _instantiateSeasoningPoint;

        [SerializeField] GameObject _seasoningPrefab;
        // Start is called before the first frame update
        void Start()
        {
            //処理を早くするタグ検索
            _water = GameObject.FindGameObjectsWithTag("Water");
            _seasonings = GameObject.FindGameObjectsWithTag("Seasoning");
            _instantiateSeasoningPoint = new Vector3[_seasonings.Length];
            for (int i = 0; i < _seasonings.Length; i++)
            {
                _instantiateSeasoningPoint[i] = _seasonings[i].transform.position; 
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
        /// <summary>
        /// ターンが変わるときに水が出なくなる場所を制御
        /// </summary>
        public void WaterManager()
        {
            var seedId = GetSeedID(_water.Length);
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
                    ///50%の確率
                    if (GetSeedID(10) < 5)
                    {
                        var newSeasoning = Instantiate(_seasoningPrefab);
                        newSeasoning.transform.position = _instantiateSeasoningPoint[i];
                        _seasonings[i] = newSeasoning;//登録することでAIが検知可能に
                    }
                }
            }
        }

        /// <summary>
        /// 乱数発生
        /// </summary>
        /// <returns></returns>
        private int GetSeedID(int rangeOfSeed)
        {
            return Random.Range(0, rangeOfSeed);
        }
    }

}
