﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cooking.Stage
{
    public enum CapsuleColliderScaleData
    {
        Height, Radius
    }
    /// <summary>スタート地点＝地面より少し上 で落下するプレイヤーの状態 初期化用</summary>
    public enum FalledFoodStateOnStart
    {
        Falled, OnStart, Other
    }
    /// <summary>
    /// 食材の持つ情報の格納場所。ポイント関連はPlayerPointクラス
    /// </summary>
    public class FoodStatus : FoodGraphic
    {
        public PlayerPoint PlayerPointProperty
        {
            get { return _playerPoint; }
        }
        private PlayerPoint _playerPoint;
        /// <summary>
        /// この食材の種類
        /// </summary>
        public FoodType FoodType
        {
            get { return foodType; }
        }
        protected FoodType foodType = FoodType.Shrimp;
        /// <summary>食材特有の変数 </summary>
        public struct Food
        {
            public Shrimp shrimp;
            public Chicken chicken;
            public Sausage sausage;
            public Egg egg;
        }
        /// <summary>
        /// 食材の種類特有変数へのアクセス
        /// </summary>
        public Food OriginalFoodProperty
        {
            get { return food; }
        }
        /// <summary>
        /// 食材特有の変数
        /// </summary>
        protected Food food;
        /// <summary>
        /// 食材の中心座標=ショットする際に打つ場所(力点) 指定しないとPivotになる 予測線の開始位置
        /// </summary>
        public Transform CenterPoint
        {
            get { return _centerPoint ? _centerPoint : this.transform; }
        }
        /// <summary>
        /// 指定しないとPivotになる 予測線の開始位置
        /// </summary>
        [SerializeField] private Transform _centerPoint = null;
        /// <summary>
        /// スタート地点初期化プレイヤーの状態
        /// </summary>
        public FalledFoodStateOnStart FalledFoodStateOnStartProperty
        {
            get { return _falledFoodStateOnStart; }
        }
        FalledFoodStateOnStart _falledFoodStateOnStart = FalledFoodStateOnStart.Falled;
        /// <summary>
        /// 回転しない食材の座標 カメラ用
        /// </summary>
        public Transform FoodPositionNotRotate
        {
            get { return _foodPositionNotRotate; }
        }
        [SerializeField] private Transform _foodPositionNotRotate = null;
        //[SerializeField] private Transform _foodPositionOnlyYRotate = null;
        /// <summary>
        /// ショット時に使用
        /// </summary>
        public Rigidbody Rigidbody
        {
            get { return _rigidbody; }
        }
        [SerializeField] private Rigidbody _rigidbody = null;
        /// <summary>
        /// 物理挙動を制御
        /// </summary>
        enum PhysicsState
        {
            Other, ShotFly, WallBound, FirstBound, ChairBound
        }
        PhysicsState _physicsState = PhysicsState.Other;
        public bool IsFoodInStartArea
        {
            get { return _isFoodInStartArea; }
        }
        private bool _isFoodInStartArea = true;
        /// <summary>
        /// 各食材のレイヤーの初期値(= Default) OnEnableで初期化
        /// </summary>
        private LayerMask _foodDefaultLayer = 0;
        public bool IsFall
        {
            get { return _isFall; }
        }
        private bool _isFall;
        public bool IsGoal
        {
            get { return _isGoal; }
        }
        private bool _isGoal;
        public bool IsFirstCollision
        {
            get { return _isFryCollision; }
        }
        /// <summary>
        /// ショットによる一回目の衝突のみ割れる スタート地点に帰ってきたとき少し落下する事に注意 リセットは衝突処理の最後にまとめてやる 一定時間で跳ねるようになる=trueになる
        /// </summary>
        private bool _isFryCollision = false;
        /// <summary>
        /// 接地点 中心CenterPointの真下にレイを飛ばして取得
        /// </summary>
        public Vector3 GroundPoint
        {
            get { return groundPoint; }
        }
        protected Vector3 groundPoint;
        public Transform IsGroundedArea
        {
            get { return _isGroundedArea ? _isGroundedArea : CenterPoint ; }
        }
        [SerializeField] Transform _isGroundedArea;
        Vector3[] _isGroundedLimitPosition;
        /// <summary>
        /// 一回目のバウンドでy方向にAddForceする力の大きさ
        /// </summary>
        [SerializeField] float _shrimpFirstBoundPower = 3;
        [SerializeField] float _chickenFirstBoundPower = 1.5f;
        /// <summary>
        /// 飛行時間の実効値 物に当たるとリセット
        /// </summary>
        private float _flyTime = 0.0f;
        private float _firstBoundTime = 0.1f;
        private float _firstBoundTimeCounter;
        private int _boundCount = 0;
        /// <summary>
        /// 転がる時間
        /// </summary>
        private float _eggRollTime = 0.3f;
        /// <summary>
        /// enum RollDirection Forward Right Forward4フレーム 0.0664秒 Right長め
        /// </summary>
        private float[] _sausageRollTime = { 0.07f, 0.5f };
        private float _rollPower = 0;
        /// <summary>
        /// 前に転がる角度
        /// </summary>
        [SerializeField] private float _forwardAngle = 60;
        /// <summary>
        /// この値(=XZ平面におけるsinの値)よりもy成分の値の絶対値が大きいと縦方向に転がる
        /// </summary>
        private float _difineForwardRollDirectionValue = Mathf.Sin(60 * Mathf.Deg2Rad);
        enum RollDirection
        {
            Forward, Right
        }
        /// <summary>
        /// ソーセージにて転がる方向により転がる時間を変える y成分が大きいとForwardになる 方向ベクトルの性質
        /// </summary>
        RollDirection _rollDirection = RollDirection.Forward;
        private bool _isGrounded = false;
        ///<summary>獲得したあわ 衝突時二重判定を防ぐ</summary>
        private List<Bubble> _gotBubbles = new List<Bubble>();
        int _gotBubbleIndex = 0;
        ///<summary>獲得した調味料 衝突時二重判定を防ぐ</summary
        private Seasoning _gotSeasoning;
        /// <summary>
        /// 衝突した相手の食材 食材衝突時二重判定を防ぐ
        /// </summary>
        private FoodStatus _collidedFood;
        /// <summary>
        /// ショット開始時呼ばれる 衝突後跳ねる挙動を制御 卵は割れるようになる
        /// </summary>
        public void FoodStatusReset()
        {
            _gotBubbles.Clear();
            _gotBubbleIndex = 0;
            _gotSeasoning = null;
            _isFryCollision = true;
            _firstBoundTimeCounter = 0;
            _flyTime = 0;
            _physicsState = PhysicsState.ShotFly;
            switch (foodType)
            {
                case FoodType.Shrimp:
                    //回転のみを止めるためいったん解除でリセット ショット(力を加える)より前に呼ばないといけないことに注意
                    UnlockConstraints();
                    FreezeRotation();
                    break;
                case FoodType.Egg:
                    //自分のターン開始時から止まっている
                    break;
                case FoodType.Chicken:
                    //回転のみを止めるためいったん解除でリセット ショット(力を加える)より前に呼ばないといけないことに注意
                    UnlockConstraints();
                    FreezeRotation();
                    break;
                case FoodType.Sausage:
                    //回転のみを止めるためいったん解除でリセット ショット(力を加える)より前に呼ばないといけないことに注意
                    UnlockConstraints();
                    FreezeRotation();
                    _rollPower = ShotManager.Instance.ShotPower / 4;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 食材初期化時に食材の種類を決める 呼ばれるタイミングを制限したかった←未実装
        /// </summary>
        /// <param name="foodType"></param>
        public void SetFoodTypeOnInitialize(FoodType foodType)
        {
            //if (StageSceneManager.Instance.GameState == StageGameState.Preparation || ShotManager.Instance.ShotModeProperty == ShotState.ShotEndMode)
            this.foodType = foodType;
            //食材の種類に合わせてコンポーネント取得
            switch (this.foodType)
            {
                case FoodType.Shrimp:
                    food.shrimp = GetComponent<Shrimp>();
                    break;
                case FoodType.Egg:
                    food.egg = GetComponent<Egg>();
                    break;
                case FoodType.Chicken:
                    food.chicken = GetComponent<Chicken>();
                    break;
                case FoodType.Sausage:
                    food.sausage = GetComponent<Sausage>();
                    break;
                default:
                    break;
            }
        }
        private void OnEnable()
        {
            if (_rigidbody == null) _rigidbody = GetComponentInChildren<Rigidbody>();
            _playerPoint = GetComponent<PlayerPoint>();
            _foodDefaultLayer = LayerMask.GetMask(LayerMask.LayerToName(this.gameObject.layer));
            if (_isGroundedArea != null)
            {
                var referencePoint = _isGroundedArea.GetChild(0).position;
                _isGroundedLimitPosition = new Vector3[] { referencePoint, referencePoint + _isGroundedArea.localScale };
                //_isGroundedArea.transform.parent = _foodPositionNotRotate;
            }
            _difineForwardRollDirectionValue = Mathf.Sin(_forwardAngle * Mathf.Deg2Rad);

        }
        protected override void Start()
        {
            base.Start();
            var seasoning = GimmickManager.Instance.TargetObjectsForAI[(int)AITargetObjectTags.Seasoning][0].GetComponent<Seasoning>();
            #if !UNITY_EDITOR
            _isGroundedArea.gameObject.GetComponent<MeshRenderer>().enabled = false;
            #endif
        }
        protected virtual void Update()
        {
            _foodPositionNotRotate.transform.position = this.transform.position;
            //自分のターンかつショット中
            if(TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex] == this  && ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode)
            {
                _flyTime += Time.deltaTime;
            }
        }
        private void FixedUpdate()
        {
            if (_isGoal)
            {
                _rigidbody.velocity = Vector3.zero;
            }
            else
            {
                switch (foodType)
                {
                    case FoodType.Shrimp:
                        switch (_physicsState)
                        {
                            case PhysicsState.Other:
                                if (_isGrounded && _flyTime < 0.1f)
                                {
                                    var velocity = _rigidbody.velocity;
                                    velocity.x /= 2;
                                    velocity.z /= 2;
                                    _rigidbody.velocity = velocity;
                                }
                                break;
                            case PhysicsState.ShotFly:
                                break;
                            case PhysicsState.WallBound:
                                //OnCollisionEnterで得るベクトルを必要に応じて代入
                                break;
                            case PhysicsState.FirstBound:
                                {
                                    _firstBoundTimeCounter += Time.deltaTime;
                                    if (_firstBoundTimeCounter >= _firstBoundTime)
                                    {
                                        _isFryCollision = true;
                                        _firstBoundTimeCounter = 0;
                                        _physicsState = PhysicsState.Other;
                                    }
                                    var velocity = _rigidbody.velocity;
                                    //衝突前の速度ベクトル
                                    var speedVector = ShotManager.Instance.transform.forward * ShotManager.Instance.ShotPower * 0.95f;
                                    //現在の速度ベクトルの符号と同じかどうかチェックし同じなら代入 壁以外のオブジェクトとぶつかった際に別方向に代入されるのを防ぐ
                                    //if (_rigidbody.velocity.x * speedVector.x >= 0 && _rigidbody.velocity.z * speedVector.z >= 0)//符号が同じなら、掛け算の答えは正になり速度の向きは同じ
                                    {
                                        velocity.x = speedVector.x;
                                        velocity.z = speedVector.z;
                                        _rigidbody.velocity = velocity;
                                        if (GetComponent<AI>() == null)
                                        {
                                            //Debug.Log(_flyTime);
                                        }
                                    }
                                    //else if (_rigidbody.velocity.magnitude > 0.1f && _rigidbody.velocity.x * speedVector.x < 0 && _rigidbody.velocity.z * speedVector.z < 0)//符号が異なる
                                    //{
                                    //    ShotManager.Instance.SetShotVector(_rigidbody.velocity, _rigidbody.velocity.magnitude);
                                    //}
                                }
                                break;
                            case PhysicsState.ChairBound:
                                {
                                    _firstBoundTimeCounter += Time.deltaTime;
                                    if (_firstBoundTimeCounter >= _firstBoundTime / 2)
                                    {
                                        _isFryCollision = true;
                                        _firstBoundTimeCounter = 0;
                                        _physicsState = PhysicsState.Other;
                                    }
                                    var velocity = _rigidbody.velocity;
                                    //衝突前の速度ベクトル
                                    var speedVector = PredictFoodPhysics.PredictFirstBoundSpeedVecor(ShotManager.Instance.transform.forward * ShotManager.Instance.ShotPower * 0.25f);
                                    //現在の速度ベクトルの符号と同じかどうかチェックし同じなら代入 壁以外のオブジェクトとぶつかった際に別方向に代入されるのを防ぐ
                                    //if (_rigidbody.velocity.x * speedVector.x >= 0 && _rigidbody.velocity.z * speedVector.z >= 0)//符号が同じなら、掛け算の答えは正になり速度の向きは同じ
                                    {
                                        velocity.x = speedVector.x;
                                        velocity.z = speedVector.z;
                                        _rigidbody.velocity = velocity;
                                    }
                                    //else if (_rigidbody.velocity.magnitude > 0.1f && _rigidbody.velocity.x * speedVector.x < 0 && _rigidbody.velocity.z * speedVector.z < 0)//符号が異なる
                                    //{
                                    //    ShotManager.Instance.SetShotVector(_rigidbody.velocity, _rigidbody.velocity.magnitude);
                                    //}
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case FoodType.Egg:
                        switch (_physicsState)
                        {
                            case PhysicsState.Other:
                                break;
                            case PhysicsState.ShotFly:
                                break;
                            case PhysicsState.WallBound:
                                //OnCollisionEnterで得るベクトルを必要に応じて代入
                                break;
                            case PhysicsState.FirstBound:
                                _firstBoundTimeCounter += Time.deltaTime;
                                if (_firstBoundTimeCounter >= _eggRollTime)
                                {
                                    _firstBoundTimeCounter = 0;
                                    _physicsState = PhysicsState.Other;
                                }
                                var velocity = _rigidbody.velocity;
                                //衝突前の速度ベクトル
                                var speedVector = PredictFoodPhysics.PredictFirstBoundSpeedVecor(ShotManager.Instance.transform.forward * ShotManager.Instance.ShotPower * 0.75f);
                                //現在の速度ベクトルの符号と同じかどうかチェックし同じなら代入 壁以外のオブジェクトとぶつかった際に別方向に代入されるのを防ぐ
                                //if (_rigidbody.velocity.x * speedVector.x >= 0 && _rigidbody.velocity.z * speedVector.z >= 0)//符号が同じなら、掛け算の答えは正になり速度の向きは同じ
                                {
                                    velocity.x = speedVector.x;
                                    velocity.z = speedVector.z;
                                    _rigidbody.velocity = velocity;
                                }
                                //else if (_rigidbody.velocity.magnitude > 0.1f && _rigidbody.velocity.x * speedVector.x < 0 && _rigidbody.velocity.z * speedVector.z < 0)//符号が同じなら、掛け算の答えは正になり速度の向きは同じ
                                //{
                                //    ShotManager.Instance.SetShotVector(_rigidbody.velocity, _rigidbody.velocity.magnitude);
                                //}
                                break;
                            default:
                                break;
                        }
                        break;
                    case FoodType.Chicken:
                        switch (_physicsState)
                        {
                            case PhysicsState.Other:
                                if (_isGrounded && _flyTime < 0.1f)
                                {
                                    var velocity = _rigidbody.velocity;
                                    velocity.x /= 2;
                                    velocity.z /= 2;
                                    _rigidbody.velocity = velocity;
                                }
                                break;
                            case PhysicsState.ShotFly:
                                break;
                            case PhysicsState.WallBound:
                                //OnCollisionEnterで得るベクトルを必要に応じて代入
                                break;
                            case PhysicsState.FirstBound:
                                {
                                    _firstBoundTimeCounter += Time.deltaTime;
                                    if (_firstBoundTimeCounter >= _firstBoundTime)
                                    {
                                        _isFryCollision = true;
                                        _firstBoundTimeCounter = 0;
                                        _physicsState = PhysicsState.Other;
                                    }
                                    var velocity = _rigidbody.velocity;
                                    //衝突前の速度ベクトル
                                    var speedVector = PredictFoodPhysics.PredictFirstBoundSpeedVecor(ShotManager.Instance.transform.forward * ShotManager.Instance.ShotPower * 0.75f);
                                    //現在の速度ベクトルの符号と同じかどうかチェックし同じなら代入 壁以外のオブジェクトとぶつかった際に別方向に代入されるのを防ぐ
                                    //if (_rigidbody.velocity.x * speedVector.x >= 0 && _rigidbody.velocity.z * speedVector.z >= 0)//符号が同じなら、掛け算の答えは正になり速度の向きは同じ
                                    {
                                        velocity.x = speedVector.x;
                                        velocity.z = speedVector.z;
                                        _rigidbody.velocity = velocity;
                                    }
                                    //else if (_rigidbody.velocity.magnitude > 0.1f && _rigidbody.velocity.x * speedVector.x < 0 && _rigidbody.velocity.z * speedVector.z < 0)//符号が同じなら、掛け算の答えは正になり速度の向きは同じ
                                    //{
                                    //    ShotManager.Instance.SetShotVector(_rigidbody.velocity, _rigidbody.velocity.magnitude);
                                    //}
                                }
                                break;
                            case PhysicsState.ChairBound:
                                {
                                    _firstBoundTimeCounter += Time.deltaTime;
                                    if (_firstBoundTimeCounter >= _firstBoundTime / 2)
                                    {
                                        _isFryCollision = true;
                                        _firstBoundTimeCounter = 0;
                                        _physicsState = PhysicsState.Other;
                                    }
                                    var velocity = _rigidbody.velocity;
                                    //衝突前の速度ベクトル
                                    var speedVector = PredictFoodPhysics.PredictFirstBoundSpeedVecor(ShotManager.Instance.transform.forward * ShotManager.Instance.ShotPower * 0.35f);
                                    //現在の速度ベクトルの符号と同じかどうかチェックし同じなら代入 壁以外のオブジェクトとぶつかった際に別方向に代入されるのを防ぐ
                                    //if (_rigidbody.velocity.x * speedVector.x >= 0 && _rigidbody.velocity.z * speedVector.z >= 0)//符号が同じなら、掛け算の答えは正になり速度の向きは同じ
                                    {
                                        velocity.x = speedVector.x;
                                        velocity.z = speedVector.z;
                                        _rigidbody.velocity = velocity;
                                    }
                                    //else if (_rigidbody.velocity.magnitude > 0.1f && _rigidbody.velocity.x * speedVector.x < 0 && _rigidbody.velocity.z * speedVector.z < 0)//符号が異なる
                                    //{
                                    //    ShotManager.Instance.SetShotVector(_rigidbody.velocity, _rigidbody.velocity.magnitude);
                                    //}
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case FoodType.Sausage:
                        switch (_physicsState)
                        {
                            case PhysicsState.Other:
                                break;
                            case PhysicsState.ShotFly:
                                break;
                            case PhysicsState.WallBound:
                                //OnCollisionEnterで得るベクトルを必要に応じて代入
                                break;
                            case PhysicsState.FirstBound:
                                _firstBoundTimeCounter += Time.deltaTime;
                                //転がる方向により転がる時間が変わる
                                switch (_rollDirection)
                                {
                                    case RollDirection.Forward:
                                        if (_firstBoundTimeCounter >= _sausageRollTime[(int)RollDirection.Forward])
                                        {
                                            //_firstBoundTimeCounter = 0;
                                            _physicsState = PhysicsState.Other;
                                        }
                                        break;
                                    case RollDirection.Right:
                                        if (_firstBoundTimeCounter >= _sausageRollTime[(int)RollDirection.Right])
                                        {
                                            //_firstBoundTimeCounter = 0;
                                            _physicsState = PhysicsState.Other;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                //空中で当たっていない
                                //if(Mathf.Abs(_rigidbody.velocity.y) < 0.01f)
                                {
                                    var velocity = _rigidbody.velocity;
                                    _rollPower -= Time.deltaTime;
                                    //衝突前の速度ベクトル
                                    var speedVector = PredictFoodPhysics.PredictFirstBoundSpeedVecor(ShotManager.Instance.transform.forward * (_rollPower));
                                    //現在の速度ベクトルの符号と同じかどうかチェックし同じなら代入 壁以外のオブジェクトとぶつかった際に別方向に代入されるのを防ぐ
                                    //if (_rigidbody.velocity.x * speedVector.x >= 0 && _rigidbody.velocity.z * speedVector.z >= 0)//符号が同じなら、掛け算の答えは正になり速度の向きは同じ
                                    {
                                        velocity.x = speedVector.x;
                                        velocity.z = speedVector.z;
                                        _rigidbody.velocity = velocity;
                                    }
                                    //else if (_rigidbody.velocity.magnitude > 0.1f && _rigidbody.velocity.x * speedVector.x < 0 && _rigidbody.velocity.z * speedVector.z < 0)//符号が同じなら、掛け算の答えは正になり速度の向きは同じ
                                    //{
                                    //    ShotManager.Instance.SetShotVector(_rigidbody.velocity, _rigidbody.velocity.magnitude);
                                    //}
                                    //絶対値にて x成分が大きい時 = cos > sin = xy平面 xの方向に大きい = 横に転がっている 45度 √2 / 2
                                    //60度では 1: ルート3 でsin=y成分が大きい y成分が √3/2よりも大きいときが60度以上 = 前に転がっているとする
                                    Vector2 directionXZ = new Vector2(speedVector.x, speedVector.z);
                                    if (Mathf.Abs(directionXZ.normalized.y) > _difineForwardRollDirectionValue)
                                    {
                                        _rollDirection = RollDirection.Forward;
                                    }
                                    else
                                    {
                                        _rollDirection = RollDirection.Right;
                                    }

                                    Debug.Log(_rollDirection);
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            if (_isGroundedArea != null)
            {
                _isGroundedArea.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
                var referencePoint = _isGroundedArea.GetChild(0).position;
                _isGroundedLimitPosition = new Vector3[] { referencePoint, referencePoint + _isGroundedArea.localScale };
                var distanceX = _isGroundedLimitPosition[(int)LimitValue.Max].x - _isGroundedLimitPosition[(int)LimitValue.Min].x;
                var distanceY = _isGroundedLimitPosition[(int)LimitValue.Max].y - _isGroundedLimitPosition[(int)LimitValue.Min].y;
                var distanceZ = _isGroundedLimitPosition[(int)LimitValue.Max].z - _isGroundedLimitPosition[(int)LimitValue.Min].z;
                var touchColliderCount = Physics.OverlapBox(_isGroundedArea.position, new Vector3(distanceX / 2, distanceY / 2, distanceZ / 2), Quaternion.identity, StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]).Length;
                if (touchColliderCount > 0)
                {
                    _isGrounded = true;
                    if (GetComponent<AI>() == null)
                        Debug.Log("接地");
                }
                else
                {
                    _isGrounded = false;
                }
            }
            else
            {
                Debug.Log("接地判定エリア無し");
            }
        }
        IEnumerator AddForce(Vector3 power)
        {
            float time = 0;
            while (time < 0.7f)
            {
                _rigidbody.AddForce(power,ForceMode.Acceleration);//調整中
                time += Time.deltaTime;
                yield return null;
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            #region//食材が切られるなど見た目が変わる処理
            switch (foodType)
            {
                case FoodType.Shrimp:
                    if (collision.gameObject.tag == TagList.Wall.ToString() && !food.shrimp.IsHeadFallOff)
                    {
                        food.shrimp.FallOffShrimpHead();
                        _playerPoint.GetPoint(GetPointType.FallOffShrimpHead);
                    }
                    else if (collision.gameObject.tag == TagList.Knife.ToString() && !food.shrimp.IsHeadFallOff)
                    {
                        food.shrimp.FallOffShrimpHead();
                        _playerPoint.GetPoint(GetPointType.FallOffShrimpHead);
                    }
                    break;
                case FoodType.Egg:
                    string collisionLayerName = LayerMask.LayerToName(collision.gameObject.layer);
                    if (collisionLayerName == StageSceneManager.Instance.GetStringLayerName(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]))
                    {
                        if (_isFryCollision)
                        {
                            if (food.egg.BreakCount >= 2)
                            {
                                ChangeMeshRendererCrackedEgg(food.egg.Shells, food.egg.InsideMeshRenderer);
                                food.egg.EggBreak();
                            }
                            //ひびが入る ショット中の最初の衝突 調味料がついていないとき
                            else
                            {
                                food.egg.EggCollide(IsSeasoningMaterial);
                                ChangeNormalEggGraphic(food.egg.BreakMaterials[1]);
                            }
                        }
                    }
                    break;
                case FoodType.Chicken:
                    if (collision.gameObject.tag == TagList.Knife.ToString() && !food.chicken.IsCut)
                    {
                        ChangeMeshRendererCutFood(food.chicken.CutMeshRenderer, foodType);
                        food.chicken.CutChicken();
                        _playerPoint.GetPoint(GetPointType.CutFood);
                    }
                    break;
                case FoodType.Sausage:
                    if (collision.gameObject.tag == TagList.Knife.ToString() && !food.sausage.IsCut)
                    {
                        ChangeMeshRendererCutFood(food.sausage.CutMeshRenderer, foodType);
                        food.sausage.CutSausage();
                        _playerPoint.GetPoint(GetPointType.CutFood);
                    }
                    break;
                default:
                    break;
            }
            #endregion
            //=================
            #region//食材共通処理
            //=================
            //自分のターンのみ
            if (TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex] == this && ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode)
            {
                //ポイント加算
                if (collision.gameObject.tag == TagList.Towel.ToString() && _playerPoint.IsFirstTowel)
                {
                    _playerPoint.GetPoint(GetPointType.FirstTowelTouch);
                }
                else if (collision.gameObject.tag == TagList.DirtDish.ToString() && _playerPoint.CanGetPointFlags[(int)GetPointOnTouch.DirtDish])
                {
                    _playerPoint.GetPoint(GetPointType.TouchDirtDish);
                }
                //==============================
                //エフェクト
                if (_isFryCollision && collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]) && collision.gameObject.tag != TagList.Wall.ToString())
                {
                    EffectManager.Instance.InstantiateEffect(collision.contacts[0].point, EffectManager.EffectPrefabID.Food_Grounded);
                }
                //==============================
                //サウンド
                if (collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]))
                {
                    //ぶつかるもので分ける
                    if (collision.gameObject.tag == TagList.Floor.ToString())
                    {
                        SoundManager.Instance.Play3DSE(SoundEffectID.food_collide0, collision.contacts[0].point);
                    }
                    else if (collision.gameObject.tag == TagList.Wall.ToString())
                    {
                        SoundManager.Instance.Play3DSE(SoundEffectID.food_collide1, collision.contacts[0].point);
                    }
                }
            }
            if (collision.gameObject.tag == TagList.Floor.ToString())
            {
                _isFall = true;
            }
            switch (_falledFoodStateOnStart)
            {
                case FalledFoodStateOnStart.Falled:
                    if (collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]))
                    {
                        //スタート地点に着地→初期化時
                        _falledFoodStateOnStart = FalledFoodStateOnStart.OnStart;
                        //FreezePosition();
                    }
                    break;
                case FalledFoodStateOnStart.OnStart:
                    break;
                case FalledFoodStateOnStart.Other:
                    break;
                default:
                    break;
            }
            #endregion
            //=================
            #region//物理挙動を制御 ステージとの衝突処理 wallは別
            //=================
            //自分がアクティブではないとき、相手が食材ならぶっ飛ばされる
            if (TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex] != this)
            {
                var otherFood = collision.gameObject.GetComponent<FoodStatus>();
                switch (foodType)
                {
                    case FoodType.Shrimp:
                        if (otherFood != null)
                        {
                            var shotManager = ShotManager.Instance;
                            var otherFoodVelocity = otherFood.Rigidbody.velocity.normalized;
                            var collisionForceVector = new Vector3(otherFoodVelocity.x, Mathf.Sin(10 * Mathf.Deg2Rad), otherFoodVelocity.z).normalized;
                            if (_boundCount < 2 && shotManager.ShotPower > (shotManager.ShotParameter.MaxShotPower + shotManager.ShotParameter.MinShotPower) / 2f)
                                StartCoroutine(AddForce(collisionForceVector * ShotManager.Instance.ShotPower));//調整中
                            else
                                _rigidbody.AddForce(collisionForceVector * ShotManager.Instance.ShotPower);// / 10);//調整中
                        }
                        break;
                    case FoodType.Egg:
                        break;
                    case FoodType.Chicken:
                        if (otherFood != null)
                        {
                            var shotManager = ShotManager.Instance;
                            var otherFoodVelocity = otherFood.Rigidbody.velocity.normalized;
                            var collisionForceVector = new Vector3(otherFoodVelocity.x, Mathf.Sin(10 * Mathf.Deg2Rad), otherFoodVelocity.z).normalized;
                            if (_boundCount < 2 && shotManager.ShotPower > (shotManager.ShotParameter.MaxShotPower + shotManager.ShotParameter.MinShotPower) / 2f)
                                _rigidbody.AddForce(collisionForceVector * ShotManager.Instance.ShotPower / 3);//調整中
                            else
                                _rigidbody.AddForce(collisionForceVector * ShotManager.Instance.ShotPower / 10);//調整中
                        }
                        break;
                    case FoodType.Sausage:
                        break;
                    default:
                        break;
                }
            }
            //自分のターンのみ有効
            if (TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex] == this)
            {
                switch (foodType)
                {
                    case FoodType.Shrimp:
                        if (!_isGoal)
                        {
                            if ((collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]) || collision.gameObject.GetComponent<FoodStatus>() != null) && collision.gameObject.tag != TagList.Wall.ToString())
                            {
                                if (_isFryCollision && ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode && _flyTime > _firstBoundTime)//アニメーション状態によってはショット開始の瞬間にぶつかるため飛行時間を追加
                                {
                                    _boundCount += 1;
                                    if (collision.gameObject.tag != TagList.Towel.ToString())
                                    {
                                        //回転固定解除
                                        TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].UnlockConstraints();
                                        if (collision.gameObject.tag == TagList.Chair.ToString())
                                        {
                                            //方向を変えない
                                            ShotManager.Instance.SetShotVector(ShotManager.Instance.transform.forward, ShotManager.Instance.ShotPower / 10);
                                            _rigidbody.AddForce(transform.up * (_shrimpFirstBoundPower) * (ShotManager.Instance.ShotPower / (ShotManager.Instance.ShotParameter.MaxShotPower * 2 * _boundCount)), ForceMode.Impulse);
                                        }
                                        else
                                        {
                                            //方向を変えない
                                            ShotManager.Instance.SetShotVector(ShotManager.Instance.transform.forward, ShotManager.Instance.ShotPower /( _boundCount + 1));
                                            _rigidbody.AddForce(transform.up * _shrimpFirstBoundPower * (ShotManager.Instance.ShotPower / (ShotManager.Instance.ShotParameter.MaxShotPower * 2 * _boundCount)), ForceMode.Impulse);
                                        }
                                    }
                                    if (_flyTime > _firstBoundTime + 0.4f && collision.gameObject.tag == TagList.Chair.ToString())
                                        _physicsState = PhysicsState.ChairBound;
                                    //小さいジャンプでは代入しない 飛んでいる間時間を数える実効値形式へ
                                    else if (_flyTime > _firstBoundTime + 0.4f && collision.gameObject.tag != TagList.Towel.ToString())
                                        _physicsState = PhysicsState.FirstBound;
                                }
                                //else if (_physicsState == PhysicsState.FirstBound)
                                //{
                                //    _physicsState = PhysicsState.Other;
                                //}
                            }
                            else if (collision.gameObject.tag == TagList.Wall.ToString())
                            {
                                //_isFirstCollision = false;
                                _rigidbody.AddForce(new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z) * ShotManager.Instance.ShotPower * 2.5f,ForceMode.Acceleration);
                                //回転固定解除
                                TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].UnlockConstraints();
                                ShotManager.Instance.SetShotVector(_rigidbody.velocity, _rigidbody.velocity.magnitude);
                            }
                            if (ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode && (collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen])) && collision.gameObject.tag != TagList.Wall.ToString())
                            {
                                if (_rigidbody.velocity.magnitude > 0.1f && _isGroundedArea != null && !_isGrounded)
                                    ShotManager.Instance.SetShotVector(_rigidbody.velocity, _rigidbody.velocity.magnitude);
                            }
                        }
                        break;
                    case FoodType.Egg:
                        if (collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]) && collision.gameObject.tag != TagList.Wall.ToString())
                        {
                            if (_isFryCollision && ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode && _flyTime > 0.1f)//アニメーション状態によってはショット開始の瞬間にぶつかるため飛行時間を追加
                            {
                                if (collision.gameObject.tag != TagList.Towel.ToString())
                                {
                                    //回転固定解除
                                    TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].UnlockConstraints();
                                    _physicsState = PhysicsState.FirstBound;
                                }
                            }
                        }
                        else if (collision.gameObject.tag == TagList.Wall.ToString())
                        {
                            //回転固定解除
                            TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].UnlockConstraints();
                        }
                        if (ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode && (collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen])))
                        {
                            if (_rigidbody.velocity.magnitude > 0.1f && _isGroundedArea != null && !_isGrounded)
                                ShotManager.Instance.SetShotVector(_rigidbody.velocity, _rigidbody.velocity.magnitude);
                        }
                        break;
                    case FoodType.Chicken:
                        if (!_isGoal)
                        {
                            if (collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]) && collision.gameObject.tag != TagList.Wall.ToString())
                            {
                                if (_isFryCollision && ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode && _flyTime > _firstBoundTime)//アニメーション状態によってはショット開始の瞬間にぶつかるため飛行時間を追加
                                {
                                    _boundCount += 1;
                                    if (collision.gameObject.tag != TagList.Towel.ToString())
                                    {
                                        //回転固定解除
                                        TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].UnlockConstraints();
                                        if (collision.gameObject.tag == TagList.Chair.ToString())
                                        {
                                            //方向を変えない
                                            ShotManager.Instance.SetShotVector(ShotManager.Instance.transform.forward, ShotManager.Instance.ShotPower / 10);
                                            _rigidbody.AddForce(transform.up * (_chickenFirstBoundPower) * (ShotManager.Instance.ShotPower / (ShotManager.Instance.ShotParameter.MaxShotPower * _boundCount * _boundCount)), ForceMode.Impulse);
                                        }
                                        else
                                        {
                                            _rigidbody.AddForce(transform.up * _chickenFirstBoundPower * (ShotManager.Instance.ShotPower / (ShotManager.Instance.ShotParameter.MaxShotPower * _boundCount * _boundCount)), ForceMode.Impulse);
                                        }
                                    }
                                    if (_flyTime > _firstBoundTime + 0.4f && collision.gameObject.tag == TagList.Chair.ToString())
                                        _physicsState = PhysicsState.ChairBound;
                                    //小さいジャンプでは代入しない 飛んでいる間時間を数える実効値形式へ
                                    else if (_flyTime > _firstBoundTime + 0.4f && collision.gameObject.tag != TagList.Towel.ToString())
                                        _physicsState = PhysicsState.FirstBound;
                                }
                                //else if (_physicsState == PhysicsState.FirstBound)
                                //{
                                //    _physicsState = PhysicsState.Other;
                                //}
                            }
                            else if (collision.gameObject.tag == TagList.Wall.ToString())
                            {
                                Debug.Log(_rigidbody.velocity);
                                //_isFirstCollision = false;
                                //回転固定解除
                                TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].UnlockConstraints();
                                ShotManager.Instance.SetShotVector(_rigidbody.velocity, _rigidbody.velocity.magnitude);
                            }
                            if (ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode && (collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen])))
                            {
                                if (_rigidbody.velocity.magnitude > 0.1f && _isGroundedArea != null && !_isGrounded)
                                    ShotManager.Instance.SetShotVector(_rigidbody.velocity, _rigidbody.velocity.magnitude);
                            }
                        }
                        break;
                    case FoodType.Sausage:
                        if (collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]) && collision.gameObject.tag != TagList.Wall.ToString())
                        {
                            if (_isFryCollision && ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode && _flyTime > 0.1f)//アニメーション状態によってはショット開始の瞬間にぶつかるため飛行時間を追加
                            {
                                if (collision.gameObject.tag != TagList.Towel.ToString())
                                {
                                    //回転固定解除
                                    TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].UnlockConstraints();
                                    _physicsState = PhysicsState.FirstBound;
                                }
                            }
                        }
                        else if (collision.gameObject.tag == TagList.Wall.ToString())
                        {
                            //回転固定解除
                            TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].UnlockConstraints();
                        }
                        if (ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode && (collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen])))
                        {
                            if (_rigidbody.velocity.magnitude > 0.1f && _isGroundedArea != null && !_isGrounded)
                                ShotManager.Instance.SetShotVector(_rigidbody.velocity, _rigidbody.velocity.magnitude);
                        }
                        break;
                    default:
                        break;
                }
            }
            #endregion
            //=================
            #region //衝突フラグ・変数初期化
            //=================
            //最初の衝突かつショット中かつ一定以上の滞空時間
            if (_isFryCollision &&( (collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]) || collision.gameObject.GetComponent<FoodStatus>() != null))
                && ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode && _flyTime > 0.1f)
            {
                switch (foodType)
                {
                    case FoodType.Shrimp:
                        if (collision.gameObject.tag != TagList.Wall.ToString())//アニメーション状態によってはショット開始の瞬間にぶつかるため飛行時間を追加
                            _isFryCollision = false;
                        break;
                    case FoodType.Egg:
                        if (_flyTime > 0.1f)//ショット直後のフラグ解除防止
                        {
                            _isFryCollision = false;
                        }
                        break;
                    case FoodType.Chicken:
                        if (collision.gameObject.tag != TagList.Wall.ToString())//アニメーション状態によってはショット開始の瞬間にぶつかるため飛行時間を追加
                            _isFryCollision = false;
                        break;
                    case FoodType.Sausage:
                        if (_flyTime > 0.1f && collision.gameObject.tag != TagList.Wall.ToString())//ショット直後のフラグ解除防止
                        {
                            _isFryCollision = false;
                        }
                        break;
                    default:
                        break;
                }
                //物理挙動変数の初期化 食材別で変わる 卵は壁に当たっても衝突ひびが入る 他は同じで床でバウンドさせるために使用する 
                _flyTime = 0;
            }
            #endregion
        }
        private void OnTriggerEnter(Collider other)
        {
            var textureList = StageSceneManager.Instance.FoodTextureList;
            if (other.tag == TagList.Finish.ToString())
            {
                EffectManager.Instance.InstantiateEffect(this.transform.position, EffectManager.EffectPrefabID.Splash);
                _isGoal = true;
                FreezeRotation();
            }
            else if (other.tag == TagList.Water.ToString())
            {
                if (_playerPoint.IsFirstWash)
                {
                    _playerPoint.GetPoint(GetPointType.FirstWash);
                }
                ChangeMaterial(StageSceneManager.Instance.FoodTextureList.normalFoodMaterials[(int)foodType], foodType);
            }
            else if (other.tag == TagList.Seasoning.ToString())
            {
                var touchSeasoning = other.GetComponent<Seasoning>();
                if (_gotSeasoning != null)
                {
                    //調味料が同じものではないならば コライダーが複数ついているオブジェクトは一回の衝突で複数の判定が呼ばれる。この判定を一回にする
                    if (touchSeasoning != _gotSeasoning)
                    {
                        GetSeasoning(touchSeasoning, textureList);
                    }
                }
                else
                {
                    GetSeasoning(touchSeasoning, textureList);
                }
            }
            else if (other.tag == TagList.RareSeasoning.ToString())
            {
                if (_playerPoint.CanGetPointFlags[(int)GetPointOnTouch.RareSeasoning])
                {
                    _playerPoint.GetPoint(GetPointType.TouchRareSeasoning);
                }
                ChangeMaterial(textureList.seasoningFoodMaterials[(int)foodType], foodType);
            }
            else if (other.tag == TagList.Bubble.ToString())
            {
                var bubble = other.GetComponent<Bubble>();
                if (_gotBubbles.Count > _gotBubbleIndex)
                {
                    //あわが同じものではないならば コライダーが複数ついているオブジェクトは一回の衝突で複数の判定が呼ばれる。この判定を一回にする
                    if (_gotBubbles[_gotBubbleIndex] != bubble )
                    {
                        GetBubble(bubble);
                        _gotBubbleIndex++;
                    }
                }
                else
                {
                    GetBubble(bubble);
                }
                Destroy(other.gameObject);
            }
            else if (other.tag == TagList.StartArea.ToString())// && !_isFoodInStartArea)//落下後復帰想定
            {
                _isFoodInStartArea = true;
                SetFoodLayer(StageSceneManager.Instance.LayerListProperty[(int)LayerList.FoodLayerInStartArea]);
            }
        }
        private void GetSeasoning(Seasoning seasoning , FoodTextureList textureList)
        {
            _gotSeasoning = seasoning;
            switch (foodType)
            {
                case FoodType.Shrimp:
                    //マテリアルが変わる前にポイント判定
                    if (foodSkinnedMeshRenderer[(int)ShrimpParts.Tail].material.mainTexture == StageSceneManager.Instance.FoodTextureList.normalTextures[(int)FoodType.Shrimp])
                    {
                        _playerPoint.GetPoint(GetPointType.TouchSeasoning);
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
            //レアエフェクトであるスターを付着
            if (seasoning.RareEffect.activeInHierarchy)
            {
                EffectManager.Instance.InstantiateEffect(this.transform.position, EffectManager.EffectPrefabID.Stars).parent = FoodPositionNotRotate.transform;
            }
            EffectManager.Instance.InstantiateEffect(this.transform.position, EffectManager.EffectPrefabID.Seasoning_Hit);
            //調味料パーティクルは付着しない
            //EffectManager.Instance.InstantiateEffect(this.transform.position, EffectManager.EffectPrefabID.Seasoning).parent = FoodPositionNotRotate.transform;
            ChangeMaterial(textureList.seasoningFoodMaterials[(int)foodType], foodType);
        }
        /// <summary>
        /// あわに触れる処理 リストに登録 演出 ポイント獲得
        /// </summary>
        /// <param name="bubble"></param>
        private void GetBubble(Bubble bubble)
        {
            EffectManager.Instance.InstantiateEffect(bubble.transform.position, EffectManager.EffectPrefabID.Foam_Break);
            _playerPoint.GetPoint(GetPointType.TouchBubble);
            _gotBubbles.Add(bubble);
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.tag == TagList.StartArea.ToString() && _isFoodInStartArea)
            {
                _isFoodInStartArea = false;
                SetFoodLayer(_foodDefaultLayer);
            }
        }
        /// <summary>
        /// ターン開始時に起き上がる
        /// </summary>
        public void ResetPlayerRotation()
        {
            transform.eulerAngles = Vector3.zero;
        }
        /// <summary>
        /// ターン開始時に呼ばれる
        /// </summary>
        public void ResetFallAndGoalFlag()
        {
            _isFall = false;
            _isGoal = false;
        }
        /// <summary>
        /// 食材のレイヤー番号を指定 StartAreaとDefault
        /// </summary>
        /// <param name="layerMask"></param>
        private void SetFoodLayer(LayerMask layerMask)
        {
            var layer = CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(layerMask);
            var foodChildObjects = this.transform.root.GetComponentsInChildren<Transform>();
            foreach (var foodChildObject in foodChildObjects)
            {
                foodChildObject.gameObject.layer = layer;
            }
        }
        /// <summary>
        /// 落下後・ゴール後にスタート地点に戻る際呼ばれる
        /// </summary>
        /// <param name="startPoint"></param>
        public void ReStart(Vector3 startPoint)
        {
            transform.position = startPoint;
            transform.eulerAngles = Vector3.zero;
            _falledFoodStateOnStart = FalledFoodStateOnStart.Falled;
        }
        /// <summary>
        /// ショット開始時アニメーション停止(空中で動きに影響を与える) ターン終了時アニメーション再生 仮
        /// </summary>
        /// <param name="isEnable"></param>
        public void PlayerAnimatioManage(bool isEnable)
        {
            switch (foodType)
            {
                case FoodType.Shrimp:
                    if (!food.shrimp.IsHeadFallOff)
                        food.shrimp.AnimationManage(isEnable);
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
        /// <summary>
        /// 落下時・ターン初期化時などプレイヤーの位置が動いたときに呼ばれる状態の初期化処理 食材の中心座標の更新など
        /// </summary>
        public void ResetFoodState()
        {
            if (_centerPoint != null)
            {
                if (_centerPoint.parent != StageSceneManager.Instance.FoodPositionsParent)
                {
                    _centerPoint.parent = StageSceneManager.Instance.FoodPositionsParent;
                }
                _centerPoint.position = this.transform.position;
            }
            //接地点の算出 中心から真下にレイを飛ばして取得
            groundPoint = GetGroundPointUnderCenter(_centerPoint.position, - _centerPoint.up);
            switch (FoodType)
            {
                case FoodType.Shrimp:
                    UnlockConstraints();
                    FreezePosition();
                    break;
                case FoodType.Egg:
                    UnlockConstraints();
                    FreezeRotation();
                    break;
                case FoodType.Chicken:
                    UnlockConstraints();
                    break;
                case FoodType.Sausage:
                    UnlockConstraints();
                    break;
                default:
                    break;
            }
        }
        public void FinishStartProcessing()
        {
            _falledFoodStateOnStart = FalledFoodStateOnStart.Other;
        }
        private Vector3 GetGroundPointUnderCenter(Vector3 originPoint , Vector3 underCenterDirection)
        {
            float rayLength = 10;
            //Rayが当たったオブジェクトの情報を入れる箱
            RaycastHit hit;    //原点        方向
            Ray ray = new Ray(originPoint, underCenterDirection);
            Debug.DrawRay(originPoint, underCenterDirection, Color.red, 100);
            //Kitchenレイヤーとレイ判定を行う
            if (Physics.Raycast(ray, out hit, rayLength, StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]))
            {
                return transform.position;
                //return hit.point;
            }
            else
            {
                return transform.position;
            }
        }
        public void SetParentObject(Transform transform)
        {
            //食材のrotationの影響を受けるのを防ぐ
            _foodPositionNotRotate.parent = transform;
            //食材の跳ねる方向を制御
            //_foodPositionOnlyYRotate.parent = transform;
        }
        /// <summary>
        /// 食材の速度を0にする
        /// </summary>
        public void StopFoodVelocity()
        {
            _rigidbody.velocity = Vector3.zero;
        }
        /// <summary>
        /// 止めたいRotation軸を指定して食材の回転を止める
        /// </summary>
        /// <param name="rigidbodyConstraints">止めたいもの</param>
        public void FreezeRotation(RigidbodyConstraints rigidbodyConstraints)
        {
            //回転の停止以外は禁止 X 16  Z 64の間には回転以外存在しない
            if (rigidbodyConstraints < RigidbodyConstraints.FreezeRotationX || rigidbodyConstraints > RigidbodyConstraints.FreezeRotationZ)
            {
                Debug.LogAssertionFormat("回転以外を止めるのは禁止 指定されたもの : {0}", rigidbodyConstraints);
                return;
            }
            _rigidbody.constraints = _rigidbody.constraints | rigidbodyConstraints;
        }
        /// <summary>
        /// ショット終了時・ショット開始時呼ばれる
        /// </summary>
        public void FreezeRotation()
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }
        /// <summary>
        /// アニメーションによる移動の防止 初期化時は着地時 基本はターン開始時に呼ばれる
        /// </summary>
        public void FreezePosition()
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezePositionX;
        }
        /// <summary>
        /// 固定解除 ショット前に動くことがある
        /// </summary>
        public void UnlockConstraints()
        {
            _rigidbody.constraints = RigidbodyConstraints.None;
        }
        /// <summary>
        /// 食材の種類によって変わるコライダーサイズへアクセス たまごは球コライダー二つ分の半径+高さ(Vector2),またはboxcolliderの大きさ(仮素材)
        /// </summary>
        /// <typeparam name="T">Vecto3 localScale Vector2 CapsuleColliderScaleData (カプセルの高さ,カプセルの半径)</typeparam>
        /// <returns>Vecto3 localScale Vector2 (カプセルの高さ,カプセルの半径)</returns>
        public T GetColliderSize<T>() where T : struct
        {
            if (typeof(T) == typeof(Vector3) || typeof(T) == typeof(Vector2))
            {
                switch (foodType)
                {
                    case FoodType.Shrimp:
                        Vector3 shrimpBoxColliderSize = food.shrimp.shrimpBoxCollider.size * 1.5f;//エビのコライダーの親オブジェクトのlocalscaleを掛け算
                        return (T)(object)shrimpBoxColliderSize;
                    case FoodType.Egg:
                        if (food.egg.HasBroken)
                        {
                            return (T)(object)new Vector3(1 , 1 , 1);
                        }
                        else
                        {
                            float eggColliderHeight = food.egg.eggCollider.height * transform.localScale.y / 2;
                            float eggColliderRadius = food.egg.eggCollider.radius * transform.localScale.y;
                            return (T)(object)new Vector2(eggColliderHeight, eggColliderRadius);
                        }
                    case FoodType.Chicken:
                        var size = food.chicken.chickenBoxCollider.size;
                        size.x *= transform.localScale.x;
                        size.y *= transform.localScale.y;
                        size.z *= transform.localScale.z;
                        Vector3 chickenBoxColliderSize = size;//エビのコライダーの親オブジェクトのlocalscaleを掛け算
                        return (T)(object)chickenBoxColliderSize;
                    case FoodType.Sausage:
                        Vector3 sausageBoxColliderSize = food.sausage.SausageBoxCollider.size;//エビのコライダーの親オブジェクトのlocalscaleを掛け算
                        return (T)(object)sausageBoxColliderSize;
                    default:
                        Debug.Log("_foodTypeがセットされていません");
                        return default(T);
                }
            }
            else
            {
                Debug.Log("型はVector3 または Vector2のどちらかです");
                return default(T);
            }
        }
    }
}
