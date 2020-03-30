﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// 予測線の動きを制御
    /// </summary>
    public class PredictLine : MonoBehaviour
    {
        /// <summary>
        /// PredictLineControllerに制御される。
        /// </summary>
        public Rigidbody predictLineRigidbody
        {
            get { return _rigidbody; }
            ///大きすぎる値が入らないように制御可能 。
            set
            {
                _rigidbody = value;
            }
        }
        private Rigidbody _rigidbody;
        public float destroyTimeCounter = 0;
        // Start is called before the first frame update
        void OnEnable()
        {
            _rigidbody = GetComponent<Rigidbody>();
            //rigidbody.velocity = initialSpeedVector;
        }

        private void Update()
        {
            var velocity = _rigidbody.velocity;
            ////予測線を飛ばす方向を取得。
            //var _predictLines_SpeedVector = ShotManager.Instance.transform.forward * 20;
            //velocity.x = _predictLines_SpeedVector.x;
            //velocity.z = _predictLines_SpeedVector.z;
            //_rigidbody.velocity = velocity;
        }

    }
    //            今後必要そうなもの 落下地点は発射地点と同じ高さとは限らないため。
    //テクスチャの張替えorゾーンの生成→その二つを同時に行う・着地予想地点から飛来してきた方向の逆にレイ飛ばし・着弾予測地点にオブジェクトをおく
}
