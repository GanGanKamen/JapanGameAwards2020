using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cooking.Stage
{
    /// <summary>スタート地点＝地面より少し上 で落下するプレイヤーの状態 初期化用</summary>
    public enum FalledFoodStateOnStart
    {
        Falled,OnStart,Other
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
            get { return _foodType; }
        }
        private FoodType _foodType = FoodType.Shrimp;
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
            get { return _food; }
        }
        /// <summary>
        /// 食材特有の変数
        /// </summary>
        private Food _food;
        /// <summary>
        /// 食材の中心座標=ショットする際に打つ場所(力点) 指定しないとPivotになる 予測線の開始位置
        /// </summary>
        public Transform CenterPoint
        {
            get { return _centerPoint ? _centerPoint : this.transform ; }
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
            Other,ShotFly, WallBound,FirstBound
        }
        PhysicsState _physicsState = PhysicsState.Other;
        private float _firstFlyTime = 0.1f;
        private float _firstFlyTimeCounter;
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
            get { return _isFirstCollision; }
        }
        /// <summary>
        /// ショットによる一回目の衝突のみ割れる スタート地点に帰ってきたとき少し落下する事に注意 
        /// </summary>
        private bool _isFirstCollision = false;
        /// <summary>
        /// ショット開始時呼ばれる 衝突後跳ねる挙動を制御 卵は割れるようになる
        /// </summary>
        public void CollisionValueReset()
        {
            _isFirstCollision = true;
            _firstFlyTimeCounter = 0;
            _physicsState = PhysicsState.Other;
        }

        /// <summary>
        /// 食材初期化時に食材の種類を決める 呼ばれるタイミングを制限したかった←未実装
        /// </summary>
        /// <param name="foodType"></param>
        public void SetFoodTypeOnInitialize(FoodType foodType)
        {
            //if (StageSceneManager.Instance.GameState == StageGameState.Preparation || ShotManager.Instance.ShotModeProperty == ShotState.ShotEndMode)
            _foodType = foodType;
            //食材の種類に合わせてコンポーネント取得
            switch (_foodType)
            {
                case FoodType.Shrimp:
                    _food.shrimp = GetComponent<Shrimp>();
                    break;
                case FoodType.Egg:
                    _food.egg = GetComponent<Egg>();
                    break;
                case FoodType.Chicken:
                    _food.chicken = GetComponent<Chicken>();
                    break;
                case FoodType.Sausage:
                    _food.sausage = GetComponent<Sausage>();
                    break;
                default:
                    break;
            }
        }
        private void OnEnable()
        {
            if (_rigidbody == null) _rigidbody = GetComponentInChildren<Rigidbody>();
            _playerPoint = GetComponent<PlayerPoint>();
            _foodDefaultLayer = this.gameObject.layer;
        }

        protected override void Start()
        {
            base.Start();
        }
        void Update()
        {
           _foodPositionNotRotate.transform.position = this.transform.position;
        }
        private void FixedUpdate()
        {
            switch (_foodType)
            {
                case FoodType.Shrimp:
                    switch (_physicsState)
                    {
                        case PhysicsState.Other:
                            break;
                        case PhysicsState.ShotFly:
                            break;
                        case PhysicsState.WallBound:
                            //OnCollisionEnterで得るベクトルを必要に応じて代入？いらないかも
                            break;
                        case PhysicsState.FirstBound:
                            _firstFlyTimeCounter += Time.deltaTime;
                            if (_firstFlyTimeCounter >= _firstFlyTime)
                            {
                                _physicsState = PhysicsState.Other;
                            }
                            var velocity = _rigidbody.velocity;
                            //衝突前の速度ベクトル
                            var speedVector = PredictFoodPhysics.PredictFirstBoundSpeedVecor(ShotManager.Instance.transform.forward * ShotManager.Instance.ShotPower * 0.75f);
                            velocity.x = speedVector.x;
                            velocity.z = speedVector.z;
                            _rigidbody.velocity = velocity;
                            break;
                        default:
                            break;
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
        private void OnCollisionEnter(Collision collision)
        {
            #region//食材が切られるなど見た目が変わる処理
            switch (_foodType)
            {
                case FoodType.Shrimp:
                    if (collision.gameObject.tag == "Wall" && !_food.shrimp.IsHeadFallOff)
                    {
                        _food.shrimp.FallOffShrimpHead();
                        _playerPoint.TouchWall();
                    }
                    else if (collision.gameObject.tag == "Knife" && !_food.shrimp.IsHeadFallOff )
                    {
                        _food.shrimp.FallOffShrimpHead();
                        _playerPoint.CutFood();
                    }
                    break;
                case FoodType.Egg:
                    string collisionLayerName = LayerMask.LayerToName(collision.gameObject.layer);
                    if (collisionLayerName == StageSceneManager.Instance.GetStringLayerName(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]))
                    {
                        if (_food.egg.BreakCount >= 2 && _isFirstCollision)
                        {
                           ChangeMeshRendererCrackedEgg(_food.egg.Shells , _food.egg.InsideMeshRenderer);
                            _food.egg.EggBreak();
                        }
                        //ひびが入る ショット中の最初の衝突 調味料がついていないとき
                        else if(_isFirstCollision)
                        {
                            _food.egg.EggCollide(IsSeasoningMaterial);
                            ChangeNormalEggGraphic(_food.egg.BreakMaterials[1]);
                            _isFirstCollision = false;
                        }
                    }
                    break;
                case FoodType.Chicken:
                    if (collision.gameObject.tag == "Knife" && !_food.chicken.IsCut)
                    {
                        ChangeMeshRendererCutFood(_food.chicken.CutMeshRenderer , _foodType);
                        _food.chicken.CutChicken();
                        _playerPoint.CutFood();
                    }
                    break;
                case FoodType.Sausage:
                    if (collision.gameObject.tag == "Knife" && !_food.sausage.IsCut)
                    {
                        ChangeMeshRendererCutFood(_food.sausage.CutMeshRenderer , _foodType);
                        _food.sausage.CutSausage();
                        _playerPoint.CutFood();
                    }
                    break;
                default:
                    break;
            }
            #endregion
            //=================
            #region//食材共通処理
            //=================
            if (ShotManager.Instance.ShotModeProperty == ShotState.ShottingMode)
            {
                if (collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerValue(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]) && collision.gameObject.tag != "Wall")
                {
                    EffectManager.Instance.InstantiateEffect(collision.contacts[0].point, EffectManager.EffectPrefabID.Food_Grounded);
                }

            }
            if (collision.gameObject.tag == "Floor")
            {
                _isFall = true;
            }
            switch (_falledFoodStateOnStart)
            {
                case FalledFoodStateOnStart.Falled:
                    if (collision.gameObject.layer == StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen])
                    {
                        //スタート地点に着地→初期化時
                        _falledFoodStateOnStart = FalledFoodStateOnStart.OnStart;
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
            #region//ステージとの衝突処理 wallは別
            //=======================
            switch (_foodType)
            {
                case FoodType.Shrimp:
                    if (collision.gameObject.layer == CalculateLayerNumber.ChangeSingleLayerNumberFromLayerValue(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]) && collision.gameObject.tag != "Wall")
                    {
                        if (_isFirstCollision && !TurnManager.Instance.IsAITurn )
                        {
                            _isFirstCollision = false;
                            //小さいジャンプでは代入しない
                            if(PredictFoodPhysics.FallTime > 0.5f)
                            _physicsState = PhysicsState.FirstBound;
                        }
                        //else if (_physicsState == PhysicsState.FirstBound)
                        //{
                        //    _physicsState = PhysicsState.Other;
                        //}
                    }
                    else if(collision.gameObject.tag == "Wall")
                    {
                        Debug.Log(_rigidbody.velocity);
                        _isFirstCollision = false;
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
            #endregion
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Finish")
            {
                EffectManager.Instance.InstantiateEffect(this.transform.position, EffectManager.EffectPrefabID.Splash);
                _isGoal = true;
            }
            else if (other.tag == "Water")
            {
                ChangeMaterial(_foodNormalGraphic , _foodType);
            }
            // とりあえず調味料はトリガーで
            else if (other.tag == "Seasoning")
            {
                EffectManager.Instance.InstantiateEffect(this.transform.position, EffectManager.EffectPrefabID.Seasoning_Hit);
                EffectManager.Instance.InstantiateEffect(this.transform.position, EffectManager.EffectPrefabID.Seasoning).parent = GetComponent<FoodStatus>().FoodPositionNotRotate.transform;
                ChangeMaterial(other.gameObject.GetComponent<MeshRenderer>().material , _foodType);
                Destroy(other.gameObject);
            }
            else if (other.tag == "RareSeasoning")
            {
                ChangeMaterial(other.gameObject.GetComponent<MeshRenderer>().material , _foodType);
                Destroy(other.gameObject);
            }
            else if (other.tag == "Bubble")
            {
                EffectManager.Instance.InstantiateEffect(other.transform.position, EffectManager.EffectPrefabID.Foam_Break);
                Destroy(other.gameObject);
            }
            else if (other.tag == "StartArea")// && !_isFoodInStartArea)//落下後復帰想定
            {
                _isFoodInStartArea = true;
                SetFoodLayer(StageSceneManager.Instance.LayerListProperty[(int)LayerList.FoodLayerInStartArea]);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "StartArea" && _isFoodInStartArea)
            {
                _isFoodInStartArea = false;
                SetFoodLayer(_foodDefaultLayer);
            }
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
            var layer = CalculateLayerNumber.ChangeSingleLayerNumberFromLayerValue(layerMask);
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
            switch (_foodType)
            {
                case FoodType.Shrimp:
                    if (!_food.shrimp.IsHeadFallOff)
                        _food.shrimp.AnimationManage(isEnable);
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
        public void SetShotPointOnFoodCenter()
        {
            if (_centerPoint != null)
                _centerPoint.position = this.transform.position;
            _falledFoodStateOnStart = FalledFoodStateOnStart.Other;
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
                Debug.LogAssertionFormat("回転以外を止めるのは禁止 指定されたもの : {0}",rigidbodyConstraints);
                return;
            }
            _rigidbody.constraints = _rigidbody.constraints | rigidbodyConstraints;
        }
        /// <summary>
        /// ショット終了時呼ばれる
        /// </summary>
        public void FreezeAllRotations()
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }
        /// <summary>
        /// FreezeRotationの解除 ショット前に動くことがあるのためショット直前
        /// </summary>
        public void UnlockFreezeRotation()
        {
            _rigidbody.constraints = RigidbodyConstraints.None;
        }
    }
}
