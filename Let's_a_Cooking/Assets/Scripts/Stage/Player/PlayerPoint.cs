﻿using System.Collections;
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
        private const int _firstPoint = 100;
        /// <summary>
        /// 獲得ポイント
        /// </summary>
        private int _getPoint = 0;
        /// <summary>
        /// 初回フラグ
        /// </summary>
        bool _isFirstWash = true, _isFirstTowel = true;
        /// <summary>
        /// 汚れた皿に触れたときポイントを失うフラグ
        /// </summary>
        bool _lostPointOnTouchDirtDish = true;
        /// <summary>
        /// 自分自身のステータス
        /// </summary>
        private FoodStatus _foodStatus;

        private void OnEnable()
        {
            _foodStatus = GetComponent<FoodStatus>();
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
        /// ショット時に呼ばれる アニメーションなどで触れるのでこのタイミング 衝突時ポイント獲得フラグのリセット
        /// </summary>
        public void ResetGetPointBool()
        {
            _lostPointOnTouchDirtDish = true;
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
            _lostPointOnTouchDirtDish = false;
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
        /// <summary>
        /// 調味料に触れる
        /// </summary>
        private void TouchSeasoning()
        {
            //マイナスを考慮 例 -50 のとき 50点 → 100点
            _getPoint = 2 * Mathf.Abs(_getPoint);
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Water" && _isFirstWash)
            {
                FirstWash();
            }
            /// とりあえず調味料はトリガーで
            else if (other.tag == "Seasoning")
            {
                TouchSeasoning();
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Towel" && _isFirstTowel)
            {
                FirstTowelTouch();
            }
            else if (collision.gameObject.tag == "DirtDish" && _lostPointOnTouchDirtDish)
            {
                TouchDirtDish();
            }
        }
    }
}
