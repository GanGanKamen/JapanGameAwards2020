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
        public int playerNumber;
        public enum FoodType
        {
            Shrimp,
            Egg,
            Chicken,
            Sausage
        }
        public FoodType foodType = FoodType.Shrimp;
        /// <summary>
        /// 食材の中心座標=ショットする際に打つ場所(力点)
        /// </summary>
        public Transform CenterPoint
        {
            get { return _centerPoint; }
        }
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
        [SerializeField] CapsuleCollider _capsuleCollider = null;
        /// <summary>
        /// このスクリプトに置くかは未定
        /// </summary>
        [SerializeField] Animator _foodAnimator = null;
        #region グラフィック関連変数
        [SerializeField] private Material _shrimpNormalGraphic = null;
        [SerializeField] private GameObject _shrimpHead = null;
        /// <summary>
        /// えびの頭が外れているかどうか
        /// </summary>
        public bool IsHeadFallOff
        {
            get { return _isHeadFallOff; }
        }
        private bool _isHeadFallOff;
        #endregion

        private void OnEnable()
        {
            if (_rigidbody == null) _rigidbody = GetComponentInChildren<Rigidbody>();
            _playerPoint = GetComponent<PlayerPoint>();
        }
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Floor")
            {
                _isFall = true;
            }
            else if (collision.gameObject.tag == "Wall" && !_isHeadFallOff && !TurnManager.Instance.IsAITurn)//CPUは未実装
            {
                _shrimpHead.transform.parent = null;
                _shrimpHead.AddComponent<Rigidbody>();
                _isHeadFallOff = true;
                var center = _capsuleCollider.center;
                _capsuleCollider.center = new Vector3(center.x, center.y, -0.1008767f);
                _capsuleCollider.height = 0.3048875f;
                _playerPoint.TouchWall();
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
            if (other.tag == "Water")
            {
                ChangeMaterial(_shrimpNormalGraphic);
            }
            /// とりあえず調味料はトリガーで
            else if (other.tag == "Seasoning")
            {
                ChangeMaterial(other.gameObject.GetComponent<MeshRenderer>().material);
                Destroy(other.gameObject);
            }
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
            if (_foodAnimator != null)
                _foodAnimator.enabled = isEnable;
        }
        public void SetShotPointOnFoodCenter()
        {
            if(_centerPoint != null)
             _centerPoint.position = this.transform.position;
        }
    }
}
