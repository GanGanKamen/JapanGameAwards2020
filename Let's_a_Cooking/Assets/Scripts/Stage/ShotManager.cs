using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Touches;

namespace Cooking.Stage
{
	/// <summary>
	/// ショットの方向や大きさを決めるオブジェクトにつけるクラス。
	/// ショットオブジェクトは一つのみで、各プレイヤーで使いまわす。
	/// </summary>
	public class ShotManager : ChangePowerMeter
    {
		/// <summary>
		/// ショットの状態を表す。
		/// </summary>
		public ShotState ShotModeProperty
		{
			get { return _shotMode; }
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
		/// ショットの最大パワーと最小パワー
		/// </summary>
		[SerializeField] float _maxShotPower = 20, _minShotPower = 5;
		/// <summary>
		/// ショット時に力を加えるため用 アクティブな食材の持つRigidbody
		/// </summary>
        private Rigidbody _shotRigidbody;
		/// <summary>
		/// 垂直方向回転の限界角度。15から90度の間を想定。プロパティを通すことで値をチェック。
		/// </summary>
		[SerializeField] float _serializedLimitVerticalAngle = 60;
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
                ///スタート時・見下ろしカメラ時・ゲーム終了時を想定
				case ShotState.WaitMode:
                    break;
				case ShotState.AngleMode:
					{
						///スマホ対応は途中 上下回転がうまくいかなかった
						if (TouchInput.GetTouchPhase() == TouchInfo.Down)
						{
							_lastMouseLeftButtonDownPosition = Input.mousePosition;
						}
						else if (TouchInput.GetTouchPhase() == TouchInfo.Moved)
						{
                            ///角度の決定
							var eulerAngle = DecisionAngle();
							transform.eulerAngles = new Vector3(eulerAngle.x, eulerAngle.y, 0);
							_lastMouseLeftButtonDownPosition = Input.mousePosition;
						}
					}
					break;
				case ShotState.PowerMeterMode:
					{
						ChangeShotPower(_minShotPower,_maxShotPower,30);
						//左クリックされた時に呼び出される
						if (!MouseInputPrevention.Instance.ShotInvalid && TouchInput.GetTouchPhase() == TouchInfo.Down)
						{
							ChangeShotState(ShotState.ShottingMode);
						}
						#region デバッグコード スペースを押すと最大パワーで飛ぶ
						///
						if (Input.GetKeyDown(KeyCode.Space))
						{
                            ChangeShotState(ShotState.ShottingMode);
                            var initialSpeedVector = transform.forward * 20;
							_shotRigidbody.velocity = initialSpeedVector;
							//isShot = true;
						}
						#endregion
					}
					break;
				case ShotState.ShottingMode:
                    {
                        ///食材が止まったらショット終了
                        if (_shotRigidbody.velocity.magnitude < 0.0001f)
                            ChangeShotState(ShotState.ShotEndMode);
                    }
                    break;
                case ShotState.ShotEndMode:
                    {
                        ChangeShotState(ShotState.WaitMode);
                    }
                    break;
                default:
					break;
			}
		}
		/// <summary>
		/// ショットのパワーが変動する
		/// </summary>

        /// <summary>
		/// ショットの角度を決める
		/// </summary>
		/// <returns></returns>
		private Vector2 DecisionAngle()
		{
			/// ショットの向きを決める際、入力を格納するための変数
			var horizontalRot = Input.mousePosition.x - _lastMouseLeftButtonDownPosition.x;
			var verticalRot = _lastMouseLeftButtonDownPosition.y - Input.mousePosition.y;
			//発射方向の角度の制御に使う変数の定義
			var eulerX = transform.eulerAngles.x;
			///上下回転速度に上限を与え 。
			if (verticalRot / _verticalMouseSensitivity > 10)
			{
				eulerX = transform.eulerAngles.x + 10;
			}
			///左右のみを移動したい時を考慮して、左右の移動量が一定より少ないとき上下回転はしない
			if (Mathf.Abs(verticalRot) > 15)
			{
				eulerX = transform.eulerAngles.x + verticalRot / _verticalMouseSensitivity;
			}
			var eulerY = transform.eulerAngles.y;
			///上下のみを移動したい時を考慮して、左右の移動量が一定より少ないとき左右回転はしない
			if (Mathf.Abs(horizontalRot) > 20)
			{
				eulerY = transform.eulerAngles.y + horizontalRot / _horizontallMouseSensitivity;
			}
			//発射方向の角度の制御 水平面から+y方向へ制限
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
        /// <summary>
        /// ショット状態を変更時に呼ばれる 状態が変わった時の処理
        /// </summary>
        /// <param name="shotState"></param>
        public void ChangeShotState(ShotState shotState)
        {
            _shotMode = shotState;
            switch (_shotMode)
            {
                case ShotState.WaitMode:
                    break;
                case ShotState.AngleMode:
                    break;
                case ShotState.PowerMeterMode:
                    break;
                case ShotState.ShottingMode:
                    {
                        //食材に力を加える処理
                        var initialSpeedVector = transform.forward * Power;
                        _shotRigidbody.velocity = initialSpeedVector;
                        UIManager.Instance.ChangeUI("ShottingMode");
                        PredictLineController.Instance.DestroyPredictLine();
                    }
                    break;
                case ShotState.ShotEndMode:
                    _shotRigidbody.velocity = Vector3.zero;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 次のプレイヤーに対してショットの対象をセット
        /// </summary>
        public void SetShotManager(Rigidbody nextRigidbody)
        {
            _shotRigidbody = nextRigidbody;
            transform.eulerAngles = Vector3.zero;
        }
    }
}
