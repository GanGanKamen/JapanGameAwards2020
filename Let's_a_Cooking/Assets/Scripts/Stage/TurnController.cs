using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// ターンの管理により、ゲームを進行。シーン内にあるすべてのFoodStatusの情報を参照して、アクティブ状態を切り替える。
    /// </summary>
    public class TurnController : MonoBehaviour
    {
        [SerializeField] GameObject _playerPrefab;
        /// <summary>
        /// プレイヤーの合計人数。
        /// </summary>
        public int playerSumNumber = 1;
        /// <summary>
        /// アクティブプレイヤーを管理。foodStatusesにおけるこの要素番号のプレイヤーがアクティブ。この値をもとに各プレイヤーに指示。
        /// </summary>
        public int ActivePlayerIndex
        {
            get { return _activePlayerIndex; }
        }
        private int _activePlayerIndex = 0;
        public FoodStatus[] foodStatuses;

        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// このクラスのインスタンスを取得。
        /// </summary>
        public static TurnController Instance
        {
            get { return _instance; }
        }
        static TurnController _instance = null;

        /// <summary>
        /// Start()より先に実行
        /// </summary>
        private void Awake()
        {
            _instance = this;
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
        }
        /// <summary>
        /// プレイヤーを生成 どの種類の食材を生成するのかという情報が必要
        /// </summary>
        public void CreatePlayers()
        {
            ///プレイヤー番号
            int playerNumber = 0;
            playerSumNumber = GameManager.Instance.playerNumber + GameManager.Instance.computerNumber;
            foodStatuses = new FoodStatus[playerSumNumber];
            ///プレイヤーを生成
            for (int i = 0; i < GameManager.Instance.playerNumber; i++)
            {
                foodStatuses[playerNumber] = Instantiate(_playerPrefab).GetComponent<FoodStatus>();
                foodStatuses[playerNumber].playerNumber = playerNumber + 1;
                playerNumber++;
            }
            ///AIを生成
            for (int i = 0; i < GameManager.Instance.computerNumber; i++)
            {
                foodStatuses[playerNumber] = Instantiate(_playerPrefab).GetComponent<FoodStatus>();
                foodStatuses[playerNumber].playerNumber = playerNumber + 1;
                foodStatuses[playerNumber].isAI = true;
                playerNumber++;
            }
            ShotManager.Instance.ShotRigidbody = foodStatuses[0].Rigidbody;
            ShotManager.Instance.ShotModeProperty = ShotState.AngleMode;
            UIManager.Instance.MainUIStateProperty = ScreenState.Shotting;
        }

        // Update is called once per frame
        void Update()
        {

        }
        /// <summary>
        /// アクティブプレイヤーを入れ替えて、次に人のターンに切り替え。
        /// </summary>
        void ChangeActivePlayer()
        {
            _activePlayerIndex++;
        }
    }

}
