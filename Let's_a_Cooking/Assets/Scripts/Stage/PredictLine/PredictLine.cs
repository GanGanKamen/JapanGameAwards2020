using System.Collections;
using System.Collections.Generic;
using Touches;
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
        TrailRenderer _trailRenderer;
        // Start is called before the first frame update
        void OnEnable()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _trailRenderer = GetComponent<TrailRenderer>();
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
            if (TouchInput.GetTouchPhase() == TouchInfo.Moved)
            {
                _trailRenderer.emitting = false;
            }
            //destroyTimeCounter += Time.deltaTime;
            //////管理失敗用
            //if (destroyTimeCounter > _trailRenderer.time)
            //{
            //    Destroy(gameObject);
            //}
        }
    }
    //            今後必要そうなもの 落下地点は発射地点と同じ高さとは限らないため。
    //テクスチャの張替えorゾーンの生成→その二つを同時に行う・着地予想地点から飛来してきた方向の逆にレイ飛ばし・着弾予測地点にオブジェクトをおく
}

