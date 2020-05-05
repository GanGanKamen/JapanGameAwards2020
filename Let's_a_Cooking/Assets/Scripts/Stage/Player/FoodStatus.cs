using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cooking.Stage
{
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
        /// プレイヤーの番号は、ユーザーを1番から順に当てていき、その後コンピューターに割り当てる。
        /// </summary>
        private int _playerNumber;
        /// <summary>
        /// この食材の種類
        /// </summary>
        public FoodType FoodType
        {
            get { return _foodType; }
        }
        private FoodType _foodType = FoodType.Shrimp;
        private Shrimp _shrimp;
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
        /// ショット時に使用。TurnControllerに管理してもらう。
        /// </summary>
        public Rigidbody Rigidbody
        {
            get { return _rigidbody; }
        }
        [SerializeField] private Rigidbody _rigidbody = null;
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
        /// <summary>
        /// キッチン(＝椅子の上テーブルの上まな板・皿などステージとして認識できるもの)の上にいるかを判定 初期化時以外でも使用するなら、Physics.OverlapAreaによる範囲指定などの必要がある
        /// </summary>
        public bool OnKitchen
        {
            get { return _onKitchen; }
        }
        private bool _onKitchen = false;
        /// <summary>
        /// 食材初期化時に食材の種類を決める 呼ばれるタイミングを制限したかった←未実装
        /// </summary>
        /// <param name="foodType"></param>
        public void SetFoodTypeOnInitialize(FoodType foodType)
        {
            //if (StageSceneManager.Instance.GameState == StageGameState.Preparation || ShotManager.Instance.ShotModeProperty == ShotState.ShotEndMode)
                _foodType = foodType;
        }

        private void OnEnable()
        {
            if (_rigidbody == null) _rigidbody = GetComponentInChildren<Rigidbody>();
            _playerPoint = GetComponent<PlayerPoint>();
            _shrimp = GetComponent<Shrimp>();
        }
        protected override void Start()
        {
            base.Start();
        }
        // Update is called once per frame
        void Update()
        {
            Debug.Log(_foodType);
        }
        private void OnCollisionEnter(Collision collision)
        {
            switch (_foodType)
            {
                case FoodType.Shrimp:
                    if (collision.gameObject.tag == "Wall" && !_shrimp.IsHeadFallOff && !TurnManager.Instance.IsAITurn)//CPUは未実装
                    {
                        _shrimp.FallOffShrimpHead();
                        _playerPoint.TouchWall();
                    }
                    else if (collision.gameObject.tag == "Knife" && !_shrimp.IsHeadFallOff && !TurnManager.Instance.IsAITurn)//CPUは未実装
                    {
                        _shrimp.FallOffShrimpHead();
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
            if (collision.gameObject.tag == "Floor")
            {
                _isFall = true;
            }
            //初期化変数 着地
            _onKitchen = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Finish")
            {
                _isGoal = true;
            }
            else if (other.tag == "Water")
            {
                ChangeMaterial(_foodNormalGraphic , _foodType);
            }
            // とりあえず調味料はトリガーで
            else if (other.tag == "Seasoning")
            {
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
                Destroy(other.gameObject);
            }
        }
        public void SetPlayerNumber(int playerNumber )
        {
            _playerNumber = playerNumber;
        }
        /// <summary>
        /// 落下後・ゴール後にスタート地点に戻る際呼ばれる
        /// </summary>
        /// <param name="startPoint"></param>
        public void ReStart(Vector3 startPoint)
        {
            transform.position = startPoint;
            _isFall = false;
            _isGoal = false;
        }
        /// <summary>
        /// ショット開始時アニメーション停止(空中で動きに影響を与える) ターン終了時アニメーション再生 仮
        /// </summary>
        /// <param name="isEnable"></param>
        public void PlayerAnimatioManage(bool isEnable)
        {
            if(!_shrimp.IsHeadFallOff)
            _shrimp.AnimationManage(isEnable);
        }
        public void SetShotPointOnFoodCenter()
        {
            if (_centerPoint != null)
                _centerPoint.position = this.transform.position;
        }
    }
}
