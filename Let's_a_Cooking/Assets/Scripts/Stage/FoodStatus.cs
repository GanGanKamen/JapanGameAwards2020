using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cooking.Stage
{
    /// <summary>
    /// 基本リードオンリーではないパラメータなどの食材の持つ情報の格納場所。
    /// </summary>
    public class FoodStatus : MonoBehaviour
    {
        /// <summary>
        /// 自分のターンであるかどうかを表す。
        /// </summary>
        ///public bool isActive; //TurnControllerに管理してもらう(現状Index番号で管理)→transform が必要で、Statusごと管理に変更
        public bool isAI;
        /// <summary>
        /// プレイヤーの番号は、ユーザーを1番から順に当てていき、その後コンピューターに割り当てる。
        /// </summary>
        public int playerNumber;
        public enum FoodType
        {
            None,
            Shrimp,
            Egg
        }
        public FoodType foodType = FoodType.None;
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
        /// <summary>
        /// スコアの獲得はFoodStatusのみで行う予定
        /// </summary>
        public int Score
        {
            get { return score; }
        }
        private int score = 100;
        private void OnEnable()
        {
            //shotPoint = transform.GetChild(0);
            _rigidbody = GetComponent<Rigidbody>();
        }
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
