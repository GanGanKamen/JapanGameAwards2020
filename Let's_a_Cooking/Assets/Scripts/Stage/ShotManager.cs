using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// ショットの方向や大きさを決めるオブジェクトにつけるクラス。
    /// ショットオブジェクトは一つのみで、各プレイヤーで使いまわす。
    /// </summary>
    public class ShotManager : MonoBehaviour
    {
        /// <summary>
        /// ショットの状態を表す。
        /// </summary>
        public ShotState ShotModeProperty
        {
            get { return _shotMode; }
            set
            {
                _shotMode = value;
            }
        }
        private ShotState _shotMode = ShotState.WaitMode;
        /// <summary>
        /// 1フレーム前のマウスの位置。ショットの方向をドラッグで決める際、回転量を計算するために使う。
        /// </summary>
        private Vector2 _lastMouseLeftButtonDownPosition;
        /// <summary>
        /// マウス感度を決める。
        /// </summary>
        [SerializeField] float _verticalMouseSensitivity = 25, _horizontallMouseSensitivity = 25;
        /// <summary>
        /// 食材を打つ力を定義。
        /// </summary>
        public float shotPower;
        /// <summary>
        /// パワー調整時の加減に使うスイッチ。
        /// </summary>
        private bool _powerUp = true;
        /// <summary>
        /// ショット時に力を加えるためのもの。
        /// </summary>
        public Rigidbody ShotRigidbody
        {
            get { return _shotRigidbody; }
            set { _shotRigidbody = value; }
        }
        private Rigidbody _shotRigidbody;
        /// <summary>
        /// 垂直方向回転の限界角度。15から90度の間を想定。プロパティを通すことで値をチェック。
        /// </summary>
        [SerializeField]float _serializedLimitVerticalAngle = 60;
        /// <summary>
        /// 垂直方向回転の限界角度に不本意な値が入らないように、値をチェック。
        /// </summary>
        private float _LimitVerticalAngle
        {
            get { return _limitVerticalAngle; }
            set
            {
                if (15 < value && value < 90)
                {
                    _limitVerticalAngle = value;
                }
                else
                {
                    _limitVerticalAngle = 60;
                }
            }
        }
        private float _limitVerticalAngle = 60;
        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// このクラスのインスタンスを取得。
        /// </summary>
        public static ShotManager Instance
        {
            get { return _instance; }
        }
        static ShotManager _instance = null;

        /// <summary>
        /// Start()より先に実行。
        /// </summary>
        private void Awake()
        {
            _instance = this;
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            _LimitVerticalAngle = _serializedLimitVerticalAngle;
        }

        // Update is called once per frame
        void Update()
        {
            switch (_shotMode)
            {
                case ShotState.WaitMode:
                    break;
                case ShotState.AngleMode:
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            _lastMouseLeftButtonDownPosition = Input.mousePosition;
                        }
                        else if (Input.GetMouseButton(0))
                        {
                            var eulerAngle = DecisionAngle();
                            transform.eulerAngles = new Vector3(eulerAngle.x, eulerAngle.y, 0);
                            _lastMouseLeftButtonDownPosition = Input.mousePosition;
                        }
                    }
                    break;
                case ShotState.PowerMeterMode:
                    {
                        ChangeShotPower();
                        //左クリックされた時に呼び出される
                        if (!MouseInputPrevention.Instance.ShotInvalid && Input.GetMouseButtonDown(0))
                        {
                            _shotMode = ShotState.Shotting;
                            //食材に力を加える処理
                            var initialSpeedVector = transform.forward * shotPower;
                            _shotRigidbody.velocity = initialSpeedVector;
                        }
                        #region デバッグコード
                        ///
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            _shotMode = ShotState.Shotting;
                            var initialSpeedVector = transform.forward * 20;
                            _shotRigidbody.velocity = initialSpeedVector;
                            //isShot = true;
                        }
                        #endregion
                    }
                    break;
                case ShotState.Shotting:
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// ショットのパワーが変動する
        /// </summary>
        private void ChangeShotPower()
        {
            if (shotPower < 5)
            {
                _powerUp = true;
                shotPower = 5;
            }
            else if (shotPower > 20)
            {
                _powerUp = false;
                shotPower = 20;
            }
            else
            {
                if (_powerUp)
                {
                    shotPower += 30 * Time.deltaTime;
                }
                else if (!_powerUp)
                {
                    shotPower -= 30 * Time.deltaTime;
                }
            }
        }
        /// <summary>
        /// ショットの角度を決める
        /// </summary>
        /// <returns></returns>
        private Vector2 DecisionAngle()
        {
            /// ショットの向きを決める際、入力を格納するための変数 。
            var horizontalRot = Input.mousePosition.x - _lastMouseLeftButtonDownPosition.x;
            var verticalRot = _lastMouseLeftButtonDownPosition.y - Input.mousePosition.y;
            //発射方向の角度の制御に使う変数の定義
            var eulerX = transform.eulerAngles.x;

            ///上下回転速度に上限を与え 。
            if (verticalRot / _verticalMouseSensitivity > 10)
            {
                eulerX = transform.eulerAngles.x + 10;
            }
            ///左右のみを移動したい時を考慮して、左右の移動量が一定より少ないとき上下回転はしないものとし 。
            if (Mathf.Abs(verticalRot) > 15)
            {
                eulerX = transform.eulerAngles.x + verticalRot / _verticalMouseSensitivity;
            }

            var eulerY = transform.eulerAngles.y;
            ///上下のみを移動したい時を考慮して、左右の移動量が一定より少ないとき左右回転はしないものとし 。
            if (Mathf.Abs(horizontalRot) > 20)
            {
                eulerY = transform.eulerAngles.y + horizontalRot / _horizontallMouseSensitivity;
            }

            //発射方向の角度の制御 水平面から+y方向へ制限し 。
            if (eulerX > 0 && eulerX <= 20)
            {
                eulerX = 0;
            }
            else if (eulerX < 360 - _limitVerticalAngle && eulerX > 270)
            {
                eulerX = 300;
            }
            return new Vector2(eulerX, eulerY);
        }
    }

}
