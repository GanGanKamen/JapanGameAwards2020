using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// ターンの管理により、ゲームを進行します。シーン内にあるすべてのFoodStatusの情報を参照して、アクティブ状態を切り替えます。
    /// </summary>
    public class TurnController : MonoBehaviour
    {

        [SerializeField] GameObject playerPrefab;
        /// <summary>
        /// プレイヤーの合計人数です。
        /// </summary>
        public int playerSumNumber = 1;
        /// <summary>
        /// アクティブプレイヤーを管理します。foodStatusesにおけるこの要素番号のプレイヤーがアクティブです。この値をもとに各プレイヤーに指示します。
        /// </summary>
        public int ActivePlayerIndex
        {
            get { return activePlayerIndex; }
        }
        private int activePlayerIndex = 0;
        public FoodStatus[] foodStatuses;
        /// <summary>
        /// ショット前のカメラの回転は、ショットの向きを決めるオブジェクトのRotationのyを参照します(左右)。x(高さ方向の回転)は参照しません。
        /// ショット前のカメラ動作はmainカメラで行います。
        /// ショットオブジェクトは一つのみで、各プレイヤーで使いまわします。
        /// </summary>
        Shot shotObject;

        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// このクラスのインスタンスを取得します。
        /// </summary>
        public static TurnController Instance
        {
            get { return instance; }
        }
        static TurnController instance = null;

        /// <summary>
        /// Start()より先に実行されます。
        /// </summary>
        private void Awake()
        {
            instance = this;
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            shotObject = FindObjectOfType<Shot>();
            ///プレイヤー番号です。
            int playerNumber = 0;
            playerSumNumber = GameManager.Instance.playerNumber + GameManager.Instance.computerNumber;
            foodStatuses = new FoodStatus[playerSumNumber];
            ///プレイヤーを生成します。
            for (int i = 0; i < GameManager.Instance.playerNumber; i++)
            {
                foodStatuses[playerNumber] = Instantiate(playerPrefab).GetComponent<FoodStatus>();
                foodStatuses[playerNumber].playerNumber = playerNumber + 1;
                playerNumber++;
            }
            ///AIを生成します。
            for (int i = 0; i < GameManager.Instance.computerNumber; i++)
            {
                foodStatuses[playerNumber] = Instantiate(playerPrefab).GetComponent<FoodStatus>();
                foodStatuses[playerNumber].playerNumber = playerNumber + 1;
                foodStatuses[playerNumber].isAI = true;
                playerNumber++;
            }
            shotObject.ShotRigidbody = foodStatuses[0].Rigidbody;
        }

        // Update is called once per frame
        void Update()
        {

        }
        /// <summary>
        /// アクティブプレイヤーを入れ替えて、次に人のターンに切り替えます。
        /// </summary>
        void ChangeActivePlayer()
        {
            activePlayerIndex++;
        }
    }

}
