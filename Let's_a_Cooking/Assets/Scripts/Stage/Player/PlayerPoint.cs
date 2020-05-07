using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cooking.Stage
{
    /// <summary>
    /// アクティブプレイヤーのプレイヤーポイント取得のため、ターンコントローラーを経由してポイント取得
    /// </summary>
    public class PlayerPoint : MonoBehaviour
    {
        /// <summary>
        /// 初期ポイント + 獲得ポイント
        /// </summary>
        public int Point
        {
            get { return _firstPoint + _getPoint; }
        }
        /// <summary>
        /// 初期ポイント
        /// </summary>
        private int _firstPoint = 100;
        /// <summary>
        /// 獲得ポイント
        /// </summary>
        private int _getPoint = 0;
        /// <summary>
        /// 初回フラグ
        /// </summary>
        bool _isFirstWash = true, _isFirstTowel = true;
        /// <summary>
        /// ポイント取得可能フラグ
        /// </summary>
        bool[] _canGetPointFlags = new bool[Enum.GetValues(typeof(GetPointOnTouch)).Length];
        /// <summary>
        /// bool配列 CanGetPoint用
        /// </summary>
        enum GetPointOnTouch
        {
            DirtDish,//減点
            Bubble,
            Seasoning,
            RareSeasoning,
            Cut
        }
        /// <summary>
        /// 自分自身のステータス レア調味料を持つかどうかはStageSceneが判定するからいらない予定
        /// </summary>
        private FoodStatus _foodStatus;

        private void OnEnable()
        {
            _foodStatus = GetComponent<FoodStatus>();
            ComponentCheck.CheckNecessaryCopmonent<FoodStatus>(this , true);
        }
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }
        /// <summary>
        /// レア調味料を持ってゴール
        /// </summary>
        public void GoalWithRareSeasoning()
        {
            //マイナスを考慮 例 -50 のとき 50点 → 100点
            _getPoint = 2 * Mathf.Abs(_getPoint);
            _firstPoint = 2 * Mathf.Abs(_getPoint);
        }
        /// <summary>
        /// 調味料が洗い流される
        /// </summary>
        public void SeasoningWashAwayed()
        {
            _getPoint /= 2;
        }
        /// <summary>
        /// 卵に亀裂が入る
        /// </summary>
        public void EggCracked()
        {
            _getPoint += 10;
        }
        /// <summary>
        /// 食材が切られたときに呼ばれる 卵は割れたとき
        /// </summary>
        public void CutFood()
        {
            _getPoint += 200;
        }
        /// <summary>
        /// ショット時に呼ばれる アニメーションなどで触れるのでこのタイミング 衝突時ポイント獲得フラグのリセット
        /// </summary>
        public void ResetGetPointBool()
        {
            for (int i = 0; i < _canGetPointFlags.Length; i++)
            {
                _canGetPointFlags[i] = true;
            }
        }
        /// <summary>
        /// あわに触れる
        /// </summary>
        private void TouchBubble()
        {
            _getPoint += 10;
            _canGetPointFlags[(int)GetPointOnTouch.Bubble] = false;
        }
        /// <summary>
        /// 調味料に触れる
        /// </summary>
        private void TouchSeasoning()
        {
            //マイナスを考慮 例 -50 のとき 50点 → 100点
            _getPoint = 2 * Mathf.Abs(_getPoint);
            _canGetPointFlags[(int)GetPointOnTouch.Seasoning] = false;
        }
        /// <summary>
        /// レア調味料に触れる
        /// </summary>
        private void TouchRareSeasoning()
        {
            //マイナスを考慮 例 -50 のとき 50点 → 100点
            _getPoint = 2 * Mathf.Abs(_getPoint);
            _canGetPointFlags[(int)GetPointOnTouch.RareSeasoning] = false;
        }
        /// <summary>
        /// 汚れた皿に触れる
        /// </summary>
        private void TouchDirtDish()
        {
            _getPoint -= 50;
            if (_firstPoint + _getPoint < 0)
            {
                _getPoint = -100;
            }
            _canGetPointFlags[(int)GetPointOnTouch.DirtDish] = false;
        }
        /// <summary>
        /// 壁に触れる 頭が取れていないことが条件
        /// </summary>
        public void TouchWall()
        {
            _getPoint += 200;
        }
        /// <summary>
        /// 初めて水洗い
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private void FirstWash()
        {
            _getPoint += 50;
            _isFirstWash = false;
        }
        /// <summary>
        /// 初めてタオルに触れる
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private void FirstTowelTouch()
        {
            _getPoint += 50;
            _isFirstTowel = false;
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Water" && _isFirstWash)
            {
                FirstWash();
            }
            /// とりあえず調味料はトリガーで
            else if (other.tag == "Seasoning" && _canGetPointFlags[(int)GetPointOnTouch.Seasoning])
            {
                TouchSeasoning();
            }
            else if (other.tag == "RareSeasoning" && _canGetPointFlags[(int)GetPointOnTouch.RareSeasoning])
            {
                TouchRareSeasoning();
            }
            else if (other.tag == "Bubble" && _canGetPointFlags[(int)GetPointOnTouch.Bubble])
            {
                TouchBubble();
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Towel" && _isFirstTowel)
            {
                FirstTowelTouch();
            }
            else if (collision.gameObject.tag == "DirtDish" && _canGetPointFlags[(int)GetPointOnTouch.DirtDish])
            {
                TouchDirtDish();
            }
        }
    }
}
