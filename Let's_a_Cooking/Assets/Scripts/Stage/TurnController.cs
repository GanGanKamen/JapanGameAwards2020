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
        /// <summary>
        /// 現在のターン数を表します。
        /// </summary>
        public int TurnNumber
        {
            get { return _turnNumber; }
        }
        private int _turnNumber = 0;
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
        /// <summary>
        /// プレイヤーの情報
        /// </summary>
        public FoodStatus[] foodStatuses;
        /// <summary>
        /// ゲーム開始時の座標を示すオブジェクト
        /// </summary>
        [SerializeField]Transform _startPositionObject;
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
            var startPoint = _startPositionObject.position;
            ///プレイヤーを生成 プレイヤー番号が小さいのがプレイしている人で大きいのがAI
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
            ///各プレイヤーを初期位置に配置
            for (int i = 0; i < playerSumNumber; i++)
            {
                foodStatuses[i].transform.position = startPoint;
                startPoint.x++;
            }
            ShotManager.Instance.SetShotManager(foodStatuses[_activePlayerIndex].Rigidbody);
            CameraManager.Instance.SetCameraMoveCenterPosition(foodStatuses[_activePlayerIndex].transform.position);
            PredictLineController.Instance.SetPredictLineInstantiatePosition(foodStatuses[_activePlayerIndex].transform.position);
            ///ターンを1にセットしてゲーム開始
            _turnNumber = 1;
        }

        // Update is called once per frame
        void Update()
        {
            switch (UIManager.Instance.MainUIStateProperty)
            {
                case ScreenState.ChooseFood:
                    break;
                case ScreenState.Start:
                    break;
                case ScreenState.AngleMode:
                    PredictLineController.Instance.SetPredictLineInstantiatePosition(foodStatuses[ActivePlayerIndex].transform.position);
                    break;
                case ScreenState.SideMode:
                    PredictLineController.Instance.SetPredictLineInstantiatePosition(foodStatuses[ActivePlayerIndex].transform.position);
                    break;
                case ScreenState.LookDownMode:
                    PredictLineController.Instance.SetPredictLineInstantiatePosition(foodStatuses[ActivePlayerIndex].transform.position);
                    break;
                case ScreenState.PowerMeterMode:
                    PredictLineController.Instance.SetPredictLineInstantiatePosition(foodStatuses[ActivePlayerIndex].transform.position);
                    break;
                case ScreenState.ShottingMode:
                    break;
                case ScreenState.Finish:
                    break;
                case ScreenState.Pause:
                    break;
                default:
                    break;
            }
            if (ShotManager.Instance.ShotModeProperty == ShotState.ShotEndMode)
            {
                if (foodStatuses[_activePlayerIndex].IsGoal)
                {
                    Debug.Log("Goal");
                }
                ChangeTurn();
            }
        }
        /// <summary>
        /// アクティブプレイヤーを入れ替えて、次の人のターンに切り替え
        /// </summary>
        private void ChangeTurn()
        {
            _activePlayerIndex++;
            ///次のターン数へ
            if (_activePlayerIndex == playerSumNumber)
            {
                _activePlayerIndex = 0;
                _turnNumber++;
            }
            UIManager.Instance.ResetUIMode();
            SetObjectsPositionForNextPlayer(_activePlayerIndex);
        }
        /// <summary>
        /// 次のターンのプレイヤーのためにオブジェクトの場所をリセット
        /// </summary>
        /// <param name="activePlayerIndex"></param>
        private void SetObjectsPositionForNextPlayer(int activePlayerIndex)
        {
            ShotManager.Instance.SetShotManager(foodStatuses[activePlayerIndex].Rigidbody);
            CameraManager.Instance.SetCameraMoveCenterPosition(foodStatuses[activePlayerIndex].transform.position);
            PredictLineController.Instance.SetPredictLineInstantiatePosition(foodStatuses[activePlayerIndex].transform.position);
        }
        /// <summary>
        /// プレイヤー落下時にscenecontrollerに呼ばれる
        /// </summary>
        public void ReSetPlayerOnStartPoint()
        {
            foodStatuses[_activePlayerIndex].ReStart(_startPositionObject.position);
            foodStatuses[_activePlayerIndex].Rigidbody.velocity = Vector3.zero;
            foodStatuses[_activePlayerIndex].transform.eulerAngles = Vector3.zero;
        }
    }

}
