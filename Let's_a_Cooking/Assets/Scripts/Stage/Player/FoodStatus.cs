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
        public PlayerPoint playerPoint;
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
        /// ショットする際に打つ場所(力点)。
        /// </summary>
        public Transform shotPoint;
        /// <summary>
        /// ショット時に使用。TurnControllerに管理してもらう。
        /// </summary>
        public Rigidbody Rigidbody
        {
            get { return _rigidbody;}
        }
        private Rigidbody _rigidbody;
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
        #region グラフィック関連変数
        [SerializeField] protected SkinnedMeshRenderer _skinnedMeshRenderer;
        [SerializeField] private Material _ebiBlack;
        [SerializeField] private Material _ebi;
        #endregion
        private void OnEnable()
        {
            //shotPoint = transform.GetChild(0);
            _rigidbody = GetComponent<Rigidbody>();
            playerPoint = GetComponent<PlayerPoint>();
        }
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            //if (!TurnController.Instance.IsAITurn)
            //Debug.Log(Rigidbody.velocity.magnitude);
        }
        private void OnCollisionEnter(Collision collision)
        {
            if(collision.gameObject.tag == "Floor")
            {
                _isFall = true;
            }
            else if (collision.gameObject.tag == "Finish")
            {
                _isGoal = true;
            }
            //初期化時以外でも使用するなら、範囲指定などの必要がある
            //else if (collision.gameObject.tag == "Kitchen") 現状タグ無し
            {
                _onKitchen = true;
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Water")
            {
                CleanSeasoning(_skinnedMeshRenderer, _ebi);
            }
            /// とりあえず調味料はトリガーで
            else if (other.tag == "Seasoning")
            {
                GetSeasoning(_skinnedMeshRenderer, _ebiBlack);
                Destroy(other.gameObject);
                Debug.Log(_skinnedMeshRenderer.materials[0]);
            }
        }
        /// <summary>
        /// スタート地点に戻されます。
        /// </summary>
        /// <param name="startPoint"></param>
        public void ReStart(Vector3 startPoint)
        {
            transform.position = startPoint;
            _isFall = false;
        }
    }

}
