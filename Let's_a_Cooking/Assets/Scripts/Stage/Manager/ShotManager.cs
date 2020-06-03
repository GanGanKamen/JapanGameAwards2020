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
        private ShotState _beforeOptionShotState = ShotState.WaitMode;
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
        /// ショットの強さで出る音が異なる
        /// </summary>
        enum ShotStrength
        {
            Weak,Normal,Powerful
        }
        /// <summary>
        /// ショットの強さで出る音が異なる
        /// </summary>
        ShotStrength _shotStrength = ShotStrength.Normal;
        /// <summary>
        /// ショット時に力を加えるため用 アクティブな食材の持つRigidbody
        /// </summary>
        private Rigidbody _shotRigidbody;
        private int _rigidbodyConstraintsIndex = 0;
        /// <summary>
        /// 0番目から順に止める
        /// </summary>
        private RigidbodyConstraints[] _rigidbodyConstraints = { RigidbodyConstraints.FreezeRotationX , RigidbodyConstraints.FreezeRotationY , RigidbodyConstraints.FreezeRotationZ };
        /// <summary>
        /// 時間経過で回転を止める
        /// </summary>
        private float _timeCounterForFreezeRotation = 0.0f;
        /// <summary>
        /// 1つのRotationを止めるまでの時間 いくつめを止めるのかによって時間を分ける
        /// </summary>
        private float[] _freezeOneOfRotationTime = { 1 , 0.5f ,0.25f };
        private TurnManager _turnManager = null;

        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// このクラスのインスタンスを取得
        /// </summary>
        public static ShotManager Instance
        {
            get { return _instance; }
        }
        static ShotManager _instance = null;

        /// <summary>
        /// Start()より先に実行
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
            _turnManager = TurnManager.Instance;
        }

        // Update is called once per frame
        void Update()
        {
            switch (_shotMode)
            {
                ///スタート時・見下ろしカメラ時・ゲーム終了時を想定
				case ShotState.WaitMode:
                    _shotPower = ChangeShotPower(_shotParameter.MinShotPower, _shotParameter.MaxShotPower, 2 * Mathf.Abs(_shotParameter.MaxShotPower - _shotParameter.MinShotPower), _shotPower);//速度ログ 5 20 (差15のとき)→ 30  差の倍速で算出   
                    break;
                case ShotState.AngleMode:
                    if (!_turnManager.IsAITurn)
                    {
                        if (TouchInput.GetTouchPhase() == TouchInfo.Moved)
                        {
                            var eulerAngle = DecisionAngle();
                            transform.eulerAngles = new Vector3(eulerAngle.x, eulerAngle.y, 0);
                        }
                        _shotPower = ChangeShotPower(_shotParameter.MinShotPower, _shotParameter.MaxShotPower, 2 * Mathf.Abs(_shotParameter.MaxShotPower - _shotParameter.MinShotPower), _shotPower);//速度ログ 5 20 (差15のとき)→ 30  差の倍速で算出   
                        if (!_turnManager.IsAITurn)
                        {
                            #region デバッグコード スペースを押すと最大パワーで飛ぶ
                            //#if UNITY_EDITOR
                            if (Input.GetKeyDown(KeyCode.Space))
                            {
                                _shotPower = ShotParameter.MaxShotPower;
                                UIManager.Instance.ChangeUI(ScreenState.ShottingMode);
                            }
                            //#endif
                            //#if UNITY_EDITOR
                            if (Input.GetKeyDown(KeyCode.M))
                            {
                                _shotPower = ShotParameter.MaxShotPower;
                                UIManager.Instance.ChangeUI(ScreenState.ShottingMode);
                            }
                            //#endif
                            #endregion
                        }
                    }
                    break;
                case ShotState.ShottingWaitMode:
                    break;
                case ShotState.ShottingMode:
                    {
                        switch (_turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].FoodType)
                        {
                            case FoodType.Shrimp:
                                ///食材が止まった + 落下・ゴール待機時間が終わったら、ショット終了
                                if (_shotRigidbody.velocity.magnitude < 0.0001f)
                                    ChangeShotState(ShotState.ShotEndMode);
                                break;
                            case FoodType.Egg:
                                {
                                    ///食材が止まった + 落下・ゴール待機時間が終わったら、ショット終了
                                    if (_shotRigidbody.velocity.magnitude < 0.000001f && _rigidbodyConstraintsIndex >= 1)//衝突した瞬間0のことがある
                                    {
                                        ChangeShotState(ShotState.ShotEndMode);
                                        //すべての回転を止める前に移動が止まる可能性もある
                                        _rigidbodyConstraintsIndex = 0;
                                    }
                                    //地面で転がっている = y方向の速度の大きさが一定より小さい この時間を測定する
                                    else if (Mathf.Abs(_shotRigidbody.velocity.y) < 0.01f)
                                    {
                                        //3(大きさ)になったら終了
                                        if (_rigidbodyConstraintsIndex == _rigidbodyConstraints.Length)
                                        {
                                            _rigidbodyConstraintsIndex = 0;
                                            break;
                                        }
                                        _timeCounterForFreezeRotation = WaitForEggFreezeRotation(_freezeOneOfRotationTime[_rigidbodyConstraintsIndex], _timeCounterForFreezeRotation);
                                        //時間を数え終わるとカウンターは0になり、再度カウントスタート
                                        if (_timeCounterForFreezeRotation == 0)
                                        {
                                            _turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].FreezeRotation(_rigidbodyConstraints[_rigidbodyConstraintsIndex]);
                                            _rigidbodyConstraintsIndex++;
                                        }
                                    }
                                }
                                break;
                            case FoodType.Chicken:
                                ///食材が止まった + 落下・ゴール待機時間が終わったら、ショット終了
                                if (_shotRigidbody.velocity.magnitude < 0.0001f)
                                    ChangeShotState(ShotState.ShotEndMode);
                                break;
                            case FoodType.Sausage:
                                ///食材が止まった + 落下・ゴール待機時間が終わったら、ショット終了
                                if (_shotRigidbody.velocity.magnitude < 0.000001f && _rigidbodyConstraintsIndex >= 1)//衝突した瞬間0のことがある
                                {
                                    ChangeShotState(ShotState.ShotEndMode);
                                    //すべての回転を止める前に移動が止まる可能性もある
                                    _rigidbodyConstraintsIndex = 0;
                                }
                                //地面で転がっている = y方向の速度の大きさが一定より小さい この時間を測定する
                                else if (Mathf.Abs(_shotRigidbody.velocity.y) < 0.01f)
                                {
                                    //3(大きさ)になったら終了
                                    if (_rigidbodyConstraintsIndex == _rigidbodyConstraints.Length)
                                    {
                                        _rigidbodyConstraintsIndex = 0;
                                        break;
                                    }
                                    _timeCounterForFreezeRotation = WaitForEggFreezeRotation(_freezeOneOfRotationTime[_rigidbodyConstraintsIndex], _timeCounterForFreezeRotation);
                                    //時間を数え終わるとカウンターは0になり、再度カウントスタート
                                    if (_timeCounterForFreezeRotation == 0)
                                    {
                                        _turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].FreezeRotation(_rigidbodyConstraints[_rigidbodyConstraintsIndex]);
                                        _rigidbodyConstraintsIndex++;
                                    }
                                }
                                break;
                            default:
                                break;
                        }
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
            _beforeOptionShotState = _shotMode;
            _shotMode = shotState;
            switch (_shotMode)
            {
                case ShotState.WaitMode:
                    break;
                case ShotState.AngleMode:
                    break;
                case ShotState.ShottingMode:
                    {
                        _turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].FoodStatusReset();
                        PredictLineManager.Instance.DestroyPredictLine();
                        {
                            switch (_turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].FoodType)
                            {
                                case FoodType.Shrimp:
                                    foreach (var food in _turnManager.FoodStatuses)
                                    {
                                        food.PlayerAnimatioManage(false);
                                    }
                                    break;
                                case FoodType.Egg:
                                    break;
                                case FoodType.Chicken:
                                    break;
                                case FoodType.Sausage:
                                    break;
                                default:
                                    break;
                            }
                        }
                        _turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].PlayerPointProperty.ResetGetPointBool();
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
        /// オプションからゲーム画面に戻る時呼ばれる
        /// </summary>
        public void ReturnOptionMode()
        {
            _shotMode = _beforeOptionShotState;
        }
        /// <summary>
        /// 現在のショットの向きを更新 速度ベクトルはメソッド内で標準化 壁反射後・AI発射時
        /// </summary>
        /// <param name="shotDirection"></param>
        /// <param name="shotPower"></param>
        public void SetShotVector(Vector3 shotDirection , float shotPower)
        {
            _shotPower = shotPower;
            this.transform.forward = shotDirection.normalized;
        }
        /// <summary>
        /// ショット状態へ移動
        /// </summary>
        public void ShotStart()
        {
            var shotPowerWidth = _shotParameter.MaxShotPower - _shotParameter.MinShotPower;
            if (_shotPower < shotPowerWidth / 3f + _shotParameter.MinShotPower)
            {
                _shotStrength = ShotStrength.Weak;
            }
            else if (_shotPower < shotPowerWidth * 2 / 3f + _shotParameter.MinShotPower)
            {
                _shotStrength = ShotStrength.Normal;
            }
            else
            {
                _shotStrength = ShotStrength.Powerful;
            }
            PlayShotSound();
            EffectManager.Instance.InstantiateEffect(_turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].transform.position, EffectManager.EffectPrefabID.Food_Jump);
            CameraManager.Instance.SetCameraPositionNearPlayer();
            ChangeShotState(ShotState.ShottingMode);
            //Shot(transform.forward);
            SameVelocityMagnitudeShot(transform.forward);
        }
        /// <summary>
        /// 力の分解をした発射処理
        /// </summary>
        /// <param name="direction">方向ベクトル</param>
        private void Shot(Vector3 direction)
        {
            var angle = transform.eulerAngles.x % 90f; //eulerAngleを0~90の範囲内にする
            var horizontalPower = Mathf.Cos(Mathf.Deg2Rad * angle) * _shotPower; 
            var verticalPower = Mathf.Sin(Mathf.Deg2Rad * angle) * _shotPower;
            var initialSpeedVector = new Vector3(direction.x * horizontalPower, direction.y * verticalPower, direction.z * horizontalPower);
            initialSpeedVector = new Vector3(direction.x , 0 , direction.z) * horizontalPower + new Vector3(0, direction.y, 0) * verticalPower;
            _shotRigidbody.velocity = initialSpeedVector;
            Debug.Log(initialSpeedVector.y);
            Debug.Log(_shotRigidbody.velocity.y);
        }
        /// <summary>
        /// 速度ベクトルが均一な発射処理
        /// </summary>
        /// <param name="direction">方向ベクトル</param>
        private void SameVelocityMagnitudeShot(Vector3 direction)
        {
            var initialSpeedVector = direction * _shotPower;
            _shotRigidbody.velocity = initialSpeedVector;
        }
        /// <summary>
        /// 力の分解をした発射処理
        /// </summary>
        /// <returns></returns>
        public Vector3 CalculateMaxShotPowerVector()
        {
            var direction = transform.forward;
            var angle = transform.eulerAngles.x % 90f; //eulerAngleを0~90の範囲内にする
            var horizontalPower = Mathf.Cos(Mathf.Deg2Rad * angle) * ShotParameter.MaxShotPower;
            var verticalPower = Mathf.Sin(Mathf.Deg2Rad * angle) * ShotParameter.MaxShotPower;
            var initialSpeedVector = new Vector3(direction.x * horizontalPower, direction.y * verticalPower, direction.z * horizontalPower);
            //Debug.Log(direction);
            return initialSpeedVector;
        }

        /// <summary>
        /// 最大パワーによる初速度ベクトルを算出 速度ベクトルが均一な発射処理
        /// </summary>
        /// <returns></returns>
        public Vector3 CalculateMaxSameVelocityMagnitudeShotPowerVector()
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
            ChangeShotState(ShotState.ShottingMode);
            UIManager.Instance.ChangeUI(ScreenState.ShottingMode);
            SameVelocityMagnitudeShot(aIShotPower);
        }
        /// <summary>
        /// 一定時間経過で卵の回転を止めていく それまで待機 戻り値は保存しておくこと 数え終わると0を返す
        /// </summary>
        /// <param name="waitTime">待つ時間</param>
        /// <param name="waitTimeCounter">カウント変数</param>
        /// <returns></returns>
        private float WaitForEggFreezeRotation(float waitTime, float waitTimeCounter)
        {
            waitTimeCounter += Time.deltaTime;
            if (waitTimeCounter > waitTime)
            {
                return 0;
            }
            return waitTimeCounter;
        }
        private void PlayShotSound()
        {
            switch (_shotStrength)
            {
                case ShotStrength.Weak:
                    SoundManager.Instance.Play3DSE(SoundEffectID.food_jump0, _turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].transform.position);
                    break;
                case ShotStrength.Normal:
                    SoundManager.Instance.Play3DSE(SoundEffectID.food_jump1, _turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].transform.position);
                    break;
                case ShotStrength.Powerful:
                    //速度速すぎて小さく聞こえたため
                    SoundManager.Instance.PlaySE(SoundEffectID.food_jump2);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 食材を打ち出す地面に対して垂直方向の角度を-0 ~ 180に変換して返す
        /// </summary>
        /// <returns></returns>
        public float GetShotAngleX()
        {
            // 回転角度を取得[-90, 90]で表されるように補正
            var rotationX = transform.eulerAngles.x % 90f;
            if (rotationX == 0)
            {
                return 0;
            }
            else
            {
                //角度の測定方向を逆転し 0~90度にして返す
                return Mathf.Abs(90 - rotationX);
            }
        }
    }
}
