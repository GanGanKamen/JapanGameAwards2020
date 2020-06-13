using System;
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
            get { return playerPoint; }
        }
        protected PlayerPoint playerPoint;
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
        [SerializeField] protected Transform _centerPoint = null;
        /// <summary>
        /// スタート地点初期化プレイヤーの状態
        /// </summary>
        public FalledFoodStateOnStart FalledFoodStateOnStartProperty
        {
            get { return falledFoodStateOnStart; }
        }
        protected FalledFoodStateOnStart falledFoodStateOnStart = FalledFoodStateOnStart.Falled;
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
        //public Vector3 GroundPoint
        //{
        //    get { return centerPoint; }
        //}
        //protected Vector3 centerPoint;
        public Transform IsGroundedArea
        {
            get { return _isGroundedArea ? _isGroundedArea : CenterPoint ; }
        }
        [SerializeField] Transform _isGroundedArea;
        [SerializeField] Transform _cutGroundedArea;
        Vector3[] _isGroundedLimitPosition;
        /// <summary>
        /// 一回目のバウンドでy方向にAddForceする力の大きさ
        /// </summary>
        [SerializeField] float _shrimpFirstBoundPower = 4;
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
        /// AIが移動する場所(プレイヤーも共通) 子オブジェクトのreference座標が入る
        /// </summary>
       [SerializeField] protected Transform aIMoveTransform;
        public bool IsAddForced
        {
            get { return _isAddedForced; }
        }
        private bool _isAddedForced = false;
        float addedForceTime = 0;
        public bool OnTowel
        {
            get { return _onTowel; }
        }
        private bool _onTowel;
        [SerializeField] Animator winnerAnimation;
        /// <summary>
        /// 自分のターン終了時、レイヤーを変更し他の食材のターゲット対象になる 自分のターン開始時デフォルトになることでレイの判定から外れる
        /// </summary>
        /// <param name="layerList"></param>
        public void ChangeFoodLayer(bool isActive)
        {
            if (isActive)
            {
                SetFoodLayer(_foodDefaultLayer);
            }
            else
            {
                SetFoodLayer(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]);
            }
        }
        /// <summary>
        /// ターンが変わるとき、プレイヤー全員で呼ばれる
        /// </summary>
        public virtual void ResetAddedForced()
        {
            addedForceTime = 0;
            _isAddedForced = false;
            var groundTag = GetGroundPointUnderCenter(_centerPoint.position, -_centerPoint.up).tag;
            if (groundTag == TagList.Floor.ToString())
            {
                _isFall = true;
            }
        }
        /// <summary>
        /// ショット開始時呼ばれる 衝突後跳ねる挙動を制御 卵は割れるようになる
        /// </summary>
        public void FoodStatusReset()
        {
            breakCountable = true;
            if (gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.FoodLayerInStartArea]))
            _isFoodInStartArea = true;
            _onTowel = false;
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
            playerPoint = GetComponent<PlayerPoint>();
            _foodDefaultLayer = LayerMask.GetMask(LayerMask.LayerToName(this.gameObject.layer));
            SetFoodLayer(StageSceneManager.Instance.LayerListProperty[(int)LayerList.FoodLayerInStartArea]);
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
            var seasoning = GimmickManager.Instance.TargetObjectsForAI[(int)AITargetObjectTags.Seasoning][0]?.GetComponent<Seasoning>();
            #if !UNITY_EDITOR
            _isGroundedArea.gameObject.GetComponent<MeshRenderer>().enabled = false;
            #endif
        }
        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.F) && GetComponent<AI>() == null)
            {
                transform.position = StageSceneManager.Instance.Goal[0].transform.position + new Vector3(0,2,0);
            }
            //Debug.Log(GetActiveMaterial(foodType,food).color);
            //Debug.Log(GetActiveMaterial(foodType,food).color == StageSceneManager.Instance.FoodTextureList.seasoningMaterial.color);
            //見た目かわらないことがある
            //if (IsSeasoningMaterial && GetActiveMaterial(foodType,food).color == Color.white)
            //{
            //    Debug.Log(46578);
            //    ChangeMaterial(StageSceneManager.Instance.FoodTextureList.seasoningMaterial, foodType, food);
            //}
            _foodPositionNotRotate.transform.position = this.transform.position;
            //自分のターンかつショット中
            if(TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex] == this  && ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode)
            {
                _flyTime += Time.deltaTime;
            }
            if (_isAddedForced && _rigidbody.velocity.magnitude < 0.1f)
            {
                FreezeRotation(RigidbodyConstraints.FreezeRotationX);
                FreezeRotation(RigidbodyConstraints.FreezeRotationY);
                FreezeRotation(RigidbodyConstraints.FreezeRotationZ);
            }
            if (_rigidbody.velocity.magnitude < 0.1f && _boundCount > 0)
            {
                FreezeRotation(RigidbodyConstraints.FreezeRotationX);
                FreezeRotation(RigidbodyConstraints.FreezeRotationY);
                FreezeRotation(RigidbodyConstraints.FreezeRotationZ);
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
                                    var shotManager = ShotManager.Instance;
                                    var speed = ShotManager.Instance.ShotPower * 1f;
                                    var direction = shotManager.transform.forward.normalized;
                                    //現在の速度ベクトルの符号と同じかどうかチェックし同じなら代入 壁以外のオブジェクトとぶつかった際に別方向に代入されるのを防ぐ
                                    //if (_rigidbody.velocity.x * speedVector.x >= 0 && _rigidbody.velocity.z * speedVector.z >= 0)//符号が同じなら、掛け算の答えは正になり速度の向きは同じ
                                    {
                                        velocity.x = direction.x * speed;
                                        velocity.z = direction.z * speed;
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
                    if (GetComponent<AI>() == null) ;
                        //Debug.Log("接地");
                }
                else
                {
                    _isGrounded = false;
                }
            }
            else
            {
                //Debug.Log("接地判定エリア無し");
            }
        }
        IEnumerator AddForce(Vector3 power)
        {
            _isAddedForced = true;
            SoundManager.Instance.PlaySE(SoundEffectID.food_jump1);
            float time = 0;
            SetFoodLayer(StageSceneManager.Instance.LayerListProperty[(int)LayerList.FoodCollision]);
            switch (foodType)
            {
                case FoodType.Shrimp:
                    while (time < 0.08f)
                    {
                        _rigidbody.AddForce(power, ForceMode.Impulse);//調整中
                        time += Time.deltaTime;
                        yield return null;
                        _isAddedForced = true;
                    }
                    while (time < 0.7f)
                    {
                        time += Time.deltaTime;
                        yield return null;
                        FreezeRotation(RigidbodyConstraints.FreezeRotationX);
                        FreezeRotation(RigidbodyConstraints.FreezeRotationY);
                        FreezeRotation(RigidbodyConstraints.FreezeRotationZ);
                    }
                    break;
                case FoodType.Egg:
                    yield return null;
                    if (food.egg.HasBroken)
                    {
                        while (time < 0.08f)
                        {
                            _rigidbody.AddForce(power, ForceMode.Impulse);//調整中
                            time += Time.deltaTime;
                            yield return null;
                        }
                    }
                    else
                    {
                        while (time < 0.2f)
                        {
                            _rigidbody.AddForce(power, ForceMode.Impulse);//調整中
                            time += Time.deltaTime;
                            yield return null;
                        }
                    }
                    while (time < 2)
                    {
                        time += Time.deltaTime;
                        yield return null;
                    }
                    {
                        FreezeRotation(RigidbodyConstraints.FreezeRotationX);
                        FreezeRotation(RigidbodyConstraints.FreezeRotationY);
                        FreezeRotation(RigidbodyConstraints.FreezeRotationZ);
                    }
                    break;
                case FoodType.Chicken:
                    while (time < 0.05f)
                    {
                        _rigidbody.AddForce(power, ForceMode.Impulse);//調整中
                        time += Time.deltaTime;
                        yield return null;
                        _isAddedForced = true;
                    }
                    while (time < 0.7f)
                    {
                        time += Time.deltaTime;
                        yield return null;
                        FreezeRotation(RigidbodyConstraints.FreezeRotationX);
                        FreezeRotation(RigidbodyConstraints.FreezeRotationY);
                        FreezeRotation(RigidbodyConstraints.FreezeRotationZ);
                    }
                    break;
                case FoodType.Sausage:
                    while (time < 0.1f)
                    {
                        _rigidbody.AddForce(power, ForceMode.Impulse);//調整中
                        time += Time.deltaTime;
                        yield return null;
                        _isAddedForced = true;
                    }
                    while (time < 0.7f)
                    {
                        time += Time.deltaTime;
                        yield return null;
                        FreezeRotation(RigidbodyConstraints.FreezeRotationX);
                        FreezeRotation(RigidbodyConstraints.FreezeRotationY);
                        FreezeRotation(RigidbodyConstraints.FreezeRotationZ);
                    }
                    break;
                default:
                    break;
            }
        }
        bool breakCountable = true;
        private void OnCollisionEnter(Collision collision)
        {
            #region//食材が切られるなど見た目が変わる処理
            switch (foodType)
            {
                case FoodType.Shrimp:
                    if (collision.gameObject.tag == TagList.Wall.ToString() && !food.shrimp.IsHeadFallOff)
                    {
                        food.shrimp.FallOffShrimpHead();
                        playerPoint.GetPoint(GetPointType.FallOffShrimpHead);
                    }
                    else if (collision.gameObject.tag == TagList.Knife.ToString() && !food.shrimp.IsHeadFallOff)
                    {
                        food.shrimp.FallOffShrimpHead();
                        playerPoint.GetPoint(GetPointType.FallOffShrimpHead);
                    }
                    break;
                case FoodType.Egg:
                    string collisionLayerName = LayerMask.LayerToName(collision.gameObject.layer);
                    if (collisionLayerName == StageSceneManager.Instance.GetStringLayerName(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]))
                    {
                        //アクティブターン中のみひび割れ
                        if (_isFryCollision && TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex] == this)
                        {
                            if (food.egg.BreakCount >= 2 && breakCountable)
                            {
                                food.egg.EggBreak();
                            }
                            //ひびが入る ショット中の最初の衝突 調味料がついていないとき
                            else if(!IsSeasoningMaterial && breakCountable)
                            {
                                breakCountable = false;
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
                        playerPoint.GetPoint(GetPointType.CutFood);
                        var randX = Random.GetRandomFloat(-1, 1);
                        var randY = Random.GetRandomFloat(-1, 1);
                        var randZ = Random.GetRandomFloat(-1, 1);
                        float cutPower = 2.5f;
                        var shotAngle = ShotManager.Instance.transform.forward;
                        var randVector = new Vector3(-shotAngle.x, randY, -shotAngle.z);
                        _rigidbody.AddForce(randVector * cutPower, ForceMode.Impulse);
                        food.chicken.CutChicken( -shotAngle, 1f);
                    }
                    break;
                case FoodType.Sausage:
                    if (collision.gameObject.tag == TagList.Knife.ToString() && !food.sausage.IsCut)
                    {
                        ChangeMeshRendererCutFood(food.sausage.CutMeshRenderer, foodType);
                        food.sausage.CutSausage();
                        playerPoint.GetPoint(GetPointType.CutFood);
                    }
                    break;
                default:
                    break;
            }
            #endregion
            //=================
            #region//食材共通処理
            //=================
            //減点は現在ターン中一回
            if (collision.gameObject.tag == TagList.DirtDish.ToString() && playerPoint.CanGetPointFlags[(int)GetPointOnTouch.DirtDish])
            {
                playerPoint.GetPoint(GetPointType.TouchDirtDish);
            }
            var otherFood = collision.gameObject.GetComponent<FoodStatus>(); //相手は食材
            //自分のターンのみ
            if (!_isGoal && TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex] == this && ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode)
            {
                //相手の調味料を奪う
                //=============================
                if (otherFood != null && rareSeasoningEffect == null)//自分にレア調味料があるときは起きない
                {
                    if (!otherFood.IsGoal)
                    {
                        var otherFoodColor = otherFood.GetActiveMaterial(otherFood.FoodType, otherFood.OriginalFoodProperty).color;
                        //レアエフェクト取得
                        if (otherFood.rareSeasoningEffect != null)
                        {
                            rareSeasoningEffect = EffectManager.Instance.InstantiateEffect(this.transform.position, EffectManager.EffectPrefabID.Food_Stars).gameObject;
                            rareSeasoningEffect.transform.position = transform.position;
                            rareSeasoningEffect.transform.parent = transform;
                        }
                        //自分が調味料を持っていないときに奪う
                        if (!IsSeasoningMaterial)
                        {
                            if (otherFood.IsSeasoningMaterial)
                            {
                                GetSeasoningPoint();
                                ChangeFoodMaterial(StageSceneManager.Instance.FoodTextureList);
                                otherFood.LostMaterial(otherFood.FoodType, otherFood.OriginalFoodProperty, otherFood.PlayerPointProperty);
                            }
                        }
                    }
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
            switch (falledFoodStateOnStart)
            {
                case FalledFoodStateOnStart.Falled:
                    if (collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]))
                    {
                        //スタート地点に着地→初期化時
                        falledFoodStateOnStart = FalledFoodStateOnStart.OnStart;
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
            if (!_isGoal && otherFood != null && ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode)
            {
                //自分がアクティブではないとき、相手が食材ならぶっ飛ばされる
                if (TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex] != this )
                {
                    switch (foodType)
                    {
                        case FoodType.Shrimp:
                            if (!_onTowel)
                            {
                                var otherFoodVelocity = DecideAddForceAngle(otherFood);
                                var collisionForceVector = new Vector3(otherFoodVelocity.x, 0, otherFoodVelocity.z).normalized;
                                if (!_isAddedForced)
                                {
                                    StartCoroutine(AddForce(collisionForceVector * ShotManager.Instance.ShotPower * 0.2f));//調整中
                                    Debug.Log("強くぶっとばす");
                                }
                            }
                            break;
                        case FoodType.Egg:
                            {
                                var shotManager = ShotManager.Instance;
                                var otherFoodVelocity = otherFood.Rigidbody.velocity.normalized;
                                var collisionForceVector = new Vector3(otherFoodVelocity.x, 0, otherFoodVelocity.z).normalized;
                                if (!_isAddedForced)
                                {
                                    StartCoroutine(AddForce(collisionForceVector * ShotManager.Instance.ShotPower * 0.02f));//調整中
                                    Debug.Log("強くぶっとばす");
                                }
                            }
                            break;
                        case FoodType.Chicken:
                            if (!_onTowel)
                            {
                                var otherFoodVelocity = DecideAddForceAngle(otherFood);
                                var collisionForceVector = new Vector3(otherFoodVelocity.x, 0, otherFoodVelocity.z).normalized;
                                if (!_isAddedForced)
                                {
                                    StartCoroutine(AddForce(collisionForceVector * ShotManager.Instance.ShotPower * 0.15f));//調整中
                                    Debug.Log("強くぶっとばす");
                                }
                            }
                            break;
                        case FoodType.Sausage:
                            {
                                var shotManager = ShotManager.Instance;
                                var otherFoodVelocity = otherFood.Rigidbody.velocity.normalized;
                                var collisionForceVector = new Vector3(otherFoodVelocity.x, 0, otherFoodVelocity.z).normalized;
                                if (!_isAddedForced)
                                {
                                    StartCoroutine(AddForce(collisionForceVector * ShotManager.Instance.ShotPower * 0.1f));//調整中
                                    Debug.Log("強くぶっとばす");
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                if (otherFood._isAddedForced)
                {
                    _rigidbody.velocity = Vector3.zero;
                }
            }
            //自分のターンのみ有効
            if (!_isGoal && TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex] == this)
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
                                        else if(collision.gameObject.GetComponent<FoodStatus>() == null)
                                        {
                                            //方向を変えない
                                            ShotManager.Instance.SetShotVector(ShotManager.Instance.transform.forward, ShotManager.Instance.ShotPower /( _boundCount + 0.5f));
                                            _rigidbody.AddForce(ShotManager.Instance.transform.forward * _shrimpFirstBoundPower * (ShotManager.Instance.ShotPower / (ShotManager.Instance.ShotParameter.MaxShotPower * 0.75f * _boundCount)), ForceMode.Impulse);
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
                                if(_boundCount < 1)
                                _rigidbody.AddForce(new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z) * ShotManager.Instance.ShotPower * 2.5f,ForceMode.Force);
                                //回転固定解除
                                TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].UnlockConstraints();
                                ShotManager.Instance.SetShotVector(_rigidbody.velocity, _rigidbody.velocity.magnitude);
                            }
                            if (collision.gameObject.GetComponent<FoodStatus>() == null && ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode && (collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen])) && collision.gameObject.tag != TagList.Wall.ToString())
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
                                else
                                {
                                    if (foodType == FoodType.Egg && _rigidbody.velocity.magnitude < 0.1f)
                                    {
                                        FreezeRotation(RigidbodyConstraints.FreezeRotationX);
                                        FreezeRotation(RigidbodyConstraints.FreezeRotationY);
                                        FreezeRotation(RigidbodyConstraints.FreezeRotationZ);
                                    }
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
                                        else if (collision.gameObject.GetComponent<FoodStatus>() == null)
                                        {                                            //方向を変えない
                                            ShotManager.Instance.SetShotVector(ShotManager.Instance.transform.forward, ShotManager.Instance.ShotPower / (_boundCount + 1f));
                                            _rigidbody.AddForce(transform.up * _chickenFirstBoundPower * (ShotManager.Instance.ShotPower / (ShotManager.Instance.ShotParameter.MaxShotPower * 0.5f * _boundCount)), ForceMode.Impulse);
                                        }
                                    }
                                    if (_flyTime > _firstBoundTime + 0.4f && collision.gameObject.tag == TagList.Chair.ToString())
                                        _physicsState = PhysicsState.ChairBound;
                                    //小さいジャンプでは代入しない 飛んでいる間時間を数える実効値形式へ
                                    else if (_flyTime > _firstBoundTime + 0f && collision.gameObject.tag != TagList.Towel.ToString())
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
            if (!_isGoal && _isFryCollision && ( (collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen])
                || collision.gameObject.GetComponent<FoodStatus>() != null))
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
        private void OnCollisionExit(Collision collision)
        {
        }
        private Vector3 DecideAddForceAngle(FoodStatus otherFood)
        {
            var shotManager = ShotManager.Instance;
            var otherFoodVelocity = new Vector2(otherFood.Rigidbody.velocity.x , otherFood.Rigidbody.velocity.z).normalized;
            if (Mathf.Abs(otherFoodVelocity.x) > Mathf.Abs(otherFoodVelocity.y))
            {
                if (otherFoodVelocity.x > 0)
                {
                    return Vector3.right;
                }
                else
                {
                    return -Vector3.right;
                }
            }
            else
            {
                if (otherFoodVelocity.y > 0)
                {
                    return Vector3.forward;
                }
                else
                {
                    return -Vector3.forward;
                }
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            var textureList = StageSceneManager.Instance.FoodTextureList;
            //ポイント加算
            if (other.tag == TagList.Towel.ToString() && playerPoint.IsFirstTowel)
            {
                playerPoint.GetPoint(GetPointType.FirstTowelTouch);
                _onTowel = true;
            }
            if (other.tag == TagList.NotBeAITarget.ToString())
            {
                if(other.transform.childCount > 0)
                {
                    var referencePoint = other.transform.GetChild(0);
                    var aI = GetComponent<AI>();
                    Debug.Log(referencePoint);
                    if (referencePoint != null)
                    {
                        aIMoveTransform = referencePoint;
                    }
                }
            }
            if (other.tag == TagList.Finish.ToString() && !_isGoal)
            {
                SoundManager.Instance.PlaySE(SoundEffectID.pan_frying);
                EffectManager.Instance.InstantiateEffect(this.transform.position, EffectManager.EffectPrefabID.Splash);
                _isGoal = true;
                FreezeRotation();
                var ranx = UnityEngine.Random.Range(-0.7f, 0.7f);
                var ranz = UnityEngine.Random.Range(-0.7f, 0.7f);
                var ranPos = new Vector3(ranx, 1, ranz);
                if (other.transform.parent.gameObject.tag == TagList.Pot.ToString())
                {
                    ranx = UnityEngine.Random.Range(-0.5f, 0.5f);
                    ranz = UnityEngine.Random.Range(-0.5f, 0.5f);
                    ranPos = new Vector3(ranx, 0.2f, ranz);
                }
                else
                {
                    ranPos = new Vector3(ranx, 0.2f, ranz);
                }
                var movePos = other.transform.position + ranPos;
                SetFoodLayer(StageSceneManager.Instance.LayerListProperty[(int)LayerList.FoodLayerInStartArea]);
                _rigidbody.velocity = Vector3.zero;
                transform.position = movePos;
            }
            else if (other.tag == TagList.Water.ToString())
            {
                if (playerPoint.IsFirstWash)
                {
                    playerPoint.GetPoint(GetPointType.FirstWash);
                }
                LostMaterial(foodType, food, playerPoint);        
            }
            else if (other.tag == TagList.Seasoning.ToString())
            {
                var touchSeasoning = other.GetComponent<Seasoning>();
                if (_gotSeasoning != null)
                {
                    //調味料が同じものではないならば コライダーが複数ついているオブジェクトは一回の衝突で複数の判定が呼ばれる。この判定を一回にする
                    if (touchSeasoning != _gotSeasoning)
                    {
                        GetSeasoning(touchSeasoning);
                    }
                }
                else
                {
                    GetSeasoning(touchSeasoning);
                }
            }
            else if (TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex] == this && ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode && !IsGoal && other.tag == TagList.Bubble.ToString())
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
                //_isFoodInStartArea = true;
                //SetFoodLayer(StageSceneManager.Instance.LayerListProperty[(int)LayerList.FoodLayerInStartArea]);
            }
        }
        protected override void GetSeasoning(Seasoning seasoning)
        {
            FoodTextureList textureList = StageSceneManager.Instance.FoodTextureList;
            if (!IsSeasoningMaterial)
            {
                GetSeasoningPoint();
            }
            base.GetSeasoning(seasoning);
            ChangeFoodMaterial(textureList);
            _gotSeasoning = seasoning;
        }

        private void ChangeFoodMaterial(FoodTextureList textureList)
        {
            switch (foodType)
            {
                case FoodType.Shrimp:
                    ChangeMaterial(textureList.seasoningMaterial, foodType, food);
                    break;
                case FoodType.Egg:
                    ChangeMaterial(textureList.seasoningMaterial, foodType, food);
                    break;
                case FoodType.Chicken:
                    ChangeMaterial(textureList.seasoningFoodMaterials[(int)foodType], foodType, food);
                    break;
                case FoodType.Sausage:
                    ChangeMaterial(textureList.seasoningMaterial, foodType, food);
                    break;
                default:
                    break;
            }
        }

        private void GetSeasoningPoint()
        {
            switch (foodType)
            {
                case FoodType.Shrimp:
                    //マテリアルが変わる前にポイント判定
                    if (foodSkinnedMeshRenderer[(int)ShrimpParts.Tail].material.mainTexture == StageSceneManager.Instance.FoodTextureList.normalTextures[(int)FoodType.Shrimp])
                    {
                        playerPoint.GetPoint(GetPointType.TouchSeasoning);
                    }
                    break;
                case FoodType.Egg:
                    playerPoint.GetPoint(GetPointType.TouchSeasoning);
                    break;
                case FoodType.Chicken:
                    playerPoint.GetPoint(GetPointType.TouchSeasoning);
                    break;
                case FoodType.Sausage:
                    playerPoint.GetPoint(GetPointType.TouchSeasoning);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// あわに触れる処理 リストに登録 演出 ポイント獲得
        /// </summary>
        /// <param name="bubble"></param>
        private void GetBubble(Bubble bubble)
        {
            EffectManager.Instance.InstantiateEffect(bubble.transform.position, EffectManager.EffectPrefabID.Foam_Break);
            playerPoint.GetPoint(GetPointType.TouchBubble);
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
        public void SetFoodLayer(LayerMask layerMask)
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
            FreezeRotation();
            falledFoodStateOnStart = FalledFoodStateOnStart.Falled;
            if(StageSceneManager.Instance.GameState != StageGameState.Preparation)
            SetFoodLayer(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]);
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
                    if (!food.shrimp.IsHeadFallOff || StageSceneManager.Instance.GameState == StageGameState.Finish)
                        food.shrimp.AnimationManage(isEnable);
                    break;
                case FoodType.Egg:
                    transform.eulerAngles = Vector3.zero;
                    winnerAnimation.transform.localPosition = Vector3.zero;
                    FreezeRotation();
                    //if (StageSceneManager.Instance.GameState == StageGameState.Finish)
                    {
                        winnerAnimation.enabled = isEnable;
                    }
                    break;
                case FoodType.Chicken:
                    transform.eulerAngles = Vector3.zero;
                    winnerAnimation.transform.localPosition = Vector3.zero;
                    FreezeRotation();
                    //if (StageSceneManager.Instance.GameState == StageGameState.Finish)
                    {
                        winnerAnimation.enabled = isEnable;
                    }
                    break;
                case FoodType.Sausage:
                    transform.eulerAngles = Vector3.zero;
                    winnerAnimation.transform.localPosition = Vector3.zero;
                    FreezeRotation();
                    //if (StageSceneManager.Instance.GameState == StageGameState.Finish)
                    {
                        winnerAnimation.enabled = isEnable;
                    }
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
            if (aIMoveTransform != null)
            {
                var trans = aIMoveTransform.parent.GetComponent<AIMoveForGameOver>();
                Debug.Log(trans);
                if (trans != null)
                {
                    //ある場合reference座標に移動
                    this.transform.position = trans.Move().position;
                }
                else
                {
                    //スクリプトがない場合reference分座標を移動 localPosition
                    this.transform.position += aIMoveTransform.localPosition * 10;
                }
            }
            if (_centerPoint != null)
            {
                if (_centerPoint.parent != StageSceneManager.Instance.FoodPositionsParent)
                {
                    _centerPoint.parent = StageSceneManager.Instance.FoodPositionsParent;
                }
                _centerPoint.position = this.transform.position;
            }

            aIMoveTransform = null;
            //接地点の算出 中心から真下にレイを飛ばして取得
            //groundPoint = GetGroundPointUnderCenter(_centerPoint.position, - _centerPoint.up);
            switch (FoodType)
            {
                case FoodType.Shrimp:
                    UnlockConstraints();
                    FreezePosition();
                    break;
                case FoodType.Egg:
                    UnlockConstraints();
                    if (TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex] == this)
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
            falledFoodStateOnStart = FalledFoodStateOnStart.Other;
        }
        private GameObject GetGroundPointUnderCenter(Vector3 originPoint , Vector3 underCenterDirection)
        {
            float rayLength = 10;
            //Rayが当たったオブジェクトの情報を入れる箱
            RaycastHit hit;    //原点        方向
            Ray ray = new Ray(originPoint, underCenterDirection);
            Debug.DrawRay(originPoint, underCenterDirection, Color.red, 100);
            //Kitchenレイヤーとレイ判定を行う
            if (Physics.Raycast(ray, out hit, rayLength, StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]))
            {
                return hit.transform.gameObject;
                //return hit.point;
            }
            else
            {
                return this.gameObject;
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
            var size = Vector3.zero;
            if (typeof(T) == typeof(Vector3) || typeof(T) == typeof(Vector2))
            {
                switch (foodType)
                {
                    case FoodType.Shrimp:
                        if (food.shrimp.IsHeadFallOff)
                        {
                            Vector3 shrimpBoxColliderSize = food.shrimp.shrimpTailBoxCollider.size * 1.5f;//エビのコライダーの親オブジェクトのlocalscaleを掛け算
                            return (T)(object)shrimpBoxColliderSize;
                        }
                        else
                        {
                            Vector3 shrimpBoxColliderSize = food.shrimp.shrimpBoxCollider.size * 1.5f;//エビのコライダーの親オブジェクトのlocalscaleを掛け算
                            return (T)(object)shrimpBoxColliderSize;
                        }
                    case FoodType.Egg:
                        if (food.egg.HasBroken)
                        {
                            size = food.egg.insideBoxCollider.size;
                            size.x *= transform.localScale.x;
                            size.y *= transform.localScale.y;
                            size.z *= transform.localScale.z;
                            Vector3 eggInsidecolliderSize = size;
                            return (T)(object)eggInsidecolliderSize;
                        }
                        else
                        {
                            float eggColliderHeight = food.egg.eggCollider.height * transform.localScale.y / 2;
                            float eggColliderRadius = food.egg.eggCollider.radius * transform.localScale.y;
                            return (T)(object)new Vector2(eggColliderHeight, eggColliderRadius);
                        }
                    case FoodType.Chicken:
                        if (food.chicken.IsCut)
                        {
                            size = food.chicken.chickenCutBoxCollider.size;
                        }
                        else
                        {
                            size = food.chicken.chickenBoxCollider.size;
                        }
                        size.x *= transform.localScale.x;
                        size.y *= transform.localScale.y;
                        size.z *= transform.localScale.z;
                        Vector3 chickenBoxColliderSize = size;//エビのコライダーの親オブジェクトのlocalscaleを掛け算
                        return (T)(object)chickenBoxColliderSize;
                    case FoodType.Sausage:
                        if(food.sausage.IsCut)
                        {
                            return (T)(object)food.sausage.cutSausageBoxCollider.size;
                        }
                        else
                        {
                            return (T)(object)food.sausage.SausageBoxCollider.size;
                        }
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
        public void SetTransparent()
        {
            switch (foodType)
            {
                case FoodType.Shrimp:
                    //if (food.shrimp.IsHeadFallOff)
                    //{
                    //    food.shrimp.SetTransparentMaterial();
                    //}
                    break;
                case FoodType.Egg:
                    break;
                case FoodType.Chicken:
                    if (food.chicken.IsCut)
                    {
                        food.chicken.SetTransparentMaterial();
                    }
                    break;
                case FoodType.Sausage:
                    if (food.sausage.IsCut)
                    {
                        food.sausage.SetTransparentMaterial();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
