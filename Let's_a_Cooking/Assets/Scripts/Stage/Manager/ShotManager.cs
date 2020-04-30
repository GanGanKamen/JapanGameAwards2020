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
        public ShotParameter ShotParameter
        {
            get { return _shotParameter; }
        }
        /// <summary>
        /// ショットパラメータリスト 限界高低角度 最大最小パワー
        /// </summary>
        private ShotParameter _shotParameter;
        /// <summary>
        /// マウス感度を決める。
        /// </summary>
        [SerializeField] float _verticalMouseSensitivity = 25, _horizontallMouseSensitivity = 25;
        /// <summary>
        /// メーターで変動させるショットの強さ
        /// </summary>
        public float ShotPower
        {
            get { return _shotPower; }
        }
        private float _shotPower;
        /// <summary>
        /// ショット時に力を加えるため用 アクティブな食材の持つRigidbody
        /// </summary>
        private Rigidbody _shotRigidbody;

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
            _shotParameter = Resources.Load<ShotParameter>("ScriptableObjects/ShotParameter");
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            _shotPower = _shotParameter.MinShotPower;
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
                    if (!TurnManager.Instance.IsAITurn)
                    {
                        if (TouchInput.GetTouchPhase() == TouchInfo.Moved)
                        {
                            var eulerAngle = DecisionAngle();
                            transform.eulerAngles = new Vector3(eulerAngle.x, eulerAngle.y, 0);
                        }
                        _shotPower = ChangeShotPower(_shotParameter.MinShotPower, _shotParameter.MaxShotPower, 2 * Mathf.Abs(_shotParameter.MaxShotPower - _shotParameter.MinShotPower), _shotPower);//速度ログ 5 20 (差15のとき)→ 30  差の倍速で算出   
                        if (!TurnManager.Instance.IsAITurn)
                        {
                            //左クリックされた時に呼び出される
                            //if (!PreventTouchInputCollision.Instance.ShotInvalid[(int)PreventTouchInputCollision.ButtonName.ShotButton] && TouchInput.GetTouchPhase() == TouchInfo.Down)
                            //{
                            //}
                            #region デバッグコード スペースを押すと最大パワーで飛ぶ

                            //#if UNITY_EDITOR
                            if (Input.GetKeyDown(KeyCode.Space))
                            {
                                UIManager.Instance.ChangeUI("ShottingMode");
                                Shot(CalculateMaxShotPowerVector());
                            }
                            //#endif
                            #endregion
                        }
                    }
                    break;
                case ShotState.ShottingMode:
                    {
                        ///食材が止まった + 落下・ゴール待機時間が終わったら、ショット終了
                        if (_shotRigidbody.velocity.magnitude < 0.0001f)//&& StageSceneManager.Instance.FoodStateOnGameProperty == StageSceneManager.FoodStateOnGame.ShotEnd)
                            ChangeShotState(ShotState.ShotEndMode);
                    }
                    break;
                case ShotState.ShotEndMode:
                    {
                        if (StageSceneManager.Instance.FoodStateOnGameProperty == StageSceneManager.FoodStateOnGame.ShotEnd)
                            ChangeShotState(ShotState.WaitMode);
                    }
                    break;
                default:
                    break;
            }
        }
        /// <summary>
		/// ショットの角度を決める
		/// </summary>
		/// <returns></returns>
		private Vector2 DecisionAngle()
        {
            var touchInputPosition = TouchInput.GetDeltaPosition();
            /// ショットの向きを決める際、入力を格納するための変数
            var horizontalRot = touchInputPosition.x;
            var verticalRot = -touchInputPosition.y;
            //発射方向の角度の制御に使う変数の定義
            var eulerX = transform.eulerAngles.x;
            ///上下回転速度に上限
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
            else if (eulerX < 360 - _shotParameter.LimitVerticalAngle && eulerX > 180)
            {
                eulerX = 360 - _shotParameter.LimitVerticalAngle;
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
                case ShotState.ShottingMode:
                    {
                        PredictLineManager.Instance.DestroyPredictLine();
                        TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].PlayerAnimatioManage(false);
                        TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].PlayerPointProperty.ResetGetPointBool();
                    }
                    break;
                case ShotState.ShotEndMode:
                    _shotRigidbody.velocity = Vector3.zero;
                    break;
                default:
                    break;
            }
        }
        public void StopShotPowerMeter()
        {
            ChangeShotState(ShotState.ShottingMode);
            Shot(transform.forward * _shotPower);
        }
        /// <summary>
        ///食材に力を加える処理
        /// </summary>
        private void Shot(Vector3 shotPower)
        {
            var initialSpeedVector = shotPower;
            _shotRigidbody.velocity = initialSpeedVector;
        }
        public Vector3 CalculateMaxShotPowerVector()
        {
            return transform.forward * _shotParameter.MaxShotPower;
        }
        /// <summary>
        /// 次のプレイヤーに対してショットの対象をセット 角度さえ渡せばAIでも演出可能
        /// </summary>
        public void SetShotManager(Rigidbody nextRigidbody)
        {
            _shotRigidbody = nextRigidbody;
            transform.eulerAngles = Vector3.zero;
        }
        /// <summary>
        /// AIによるショット
        /// </summary>
        /// <param name="aIShotPower"></param>
        public void AIShot(Vector3 aIShotPower)
        {
            Shot(aIShotPower);
            ChangeShotState(ShotState.ShottingMode);
            UIManager.Instance.ChangeUI("ShottingMode");
        }
    }
}
