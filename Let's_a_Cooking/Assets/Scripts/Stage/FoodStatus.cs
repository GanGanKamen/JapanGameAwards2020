using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cooking.Stage
{
    /// <summary>
    /// 基本リードオンリーではないパラメータなどの食材の持つ情報の格納場所です。
    /// </summary>
    public class FoodStatus : MonoBehaviour
    {
        /// <summary>
        /// 自分のターンであるかどうかを表します。
        /// </summary>
        ///public bool isActive; //TurnControllerに管理してもらいます(現状Index番号で管理しています)→transform が必要なので、Statusごと管理に変更

        public bool isAI;
        /// <summary>
        /// プレイヤーの番号は、ユーザーを1番から順に当てていき、その後コンピューターに割り当てます。
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
        /// ショットする際に打つ場所(力点)です。
        /// </summary>
        public Transform shotPoint;
        /// <summary>
        /// ショット時に使います。TurnControllerに管理してもらいます。
        /// </summary>
        public Rigidbody Rigidbody
        {
            get { return rigidbody;}
        }
        private new Rigidbody rigidbody;
        private void OnEnable()
        {
            shotPoint = transform.GetChild(0);
            rigidbody = GetComponent<Rigidbody>();
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
