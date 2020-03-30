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
        [SerializeField] GameObject _aIPrefab;
        /// <summary>
        /// プレイヤーの合計人数。ローカルでよく使うため定義
        /// </summary>
        private int _playerSumNumber = 1;
        /// <summary>
        /// foodStatusesにおける要素番号。この値でアクティブプレイヤーに指示 順番決めにも有人プレイヤーの番号として使用
        /// </summary>
        public int ActivePlayerIndex
        {
            get { return _activePlayerIndex; }
        }
        private int _activePlayerIndex = 0;
        /// <summary>
        /// ターン回し用要素番号配列 1 3 2 0ならプレイヤー2 4 3 1の順にターンが回る
        /// </summary>
        public int[] IndexArray
        {
            get { return _indexArray; }
        }
        private int[] _indexArray;
        /// <summary>
        /// プレイヤーの情報
        /// </summary>
        public FoodStatus[] foodStatuses;
        /// <summary>
        /// ゲーム開始時の座標を示すオブジェクト
        /// </summary>
        [SerializeField] Transform _startPositionObject;
        /// <summary>
        /// メーターで変動させる順番を決めるための値
        /// </summary>
        public float[] OrderPower
        {
            get { return _orderPower; }
        }
       private float[] _orderPower;

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

        [SerializeField] GameObject _goal;
        // Start is called before the first frame update
        void Start()
        {
            _playerSumNumber = GameManager.Instance.playerNumber + GameManager.Instance.computerNumber;
            ///タグ検索エラーをできるだけ防ぐ
            if (_goal == null)
            {
                Debug.Log("ゴールオブジェクトがセットされていません。タグ検索されました。");
                GameObject.FindGameObjectWithTag("Finish");
            }
            _orderPower = new float[_playerSumNumber];
            _indexArray = new int[_playerSumNumber];
            for (int i = 0; i < _indexArray.Length; i++)
            {
                _indexArray[i] = i;
            }
        }

        /// <summary>
        /// ショットを打つ順番を決めるために、値を格納する要素番号・格納したい値を受け取る
        /// </summary>
        /// <param name="playerNumberIndex"></param>
        /// <param name="value"></param>
        public void DecideOrderValue(int playerNumberIndex, float value)
        {
            //ランダム要素 現状演出無しで値が最初に出て、プレイヤーはそれより大きいのを狙う形式
            _orderPower[playerNumberIndex] = value;
        }

        /// <summary>
        /// プレイヤーをショット順に並び替える
        /// </summary>
        public void PlayerInOrder()
        {
            for (int i = 0; i < _orderPower.Length - 1; i++)
            {
                for (int j = 0; j < _orderPower.Length - 1 - i; j++)
                {
                    if (_orderPower[j] < _orderPower[j + 1])
                    {
                        ///値とプレイヤーをシンクロさせて入れ替える
                        var tempValue = _orderPower[j];
                        _orderPower[j] = _orderPower[j + 1];
                        _orderPower[j + 1] = tempValue;
                        var tempFood = foodStatuses[j];
                        foodStatuses[j] = foodStatuses[j + 1];
                        foodStatuses[j + 1] = tempFood;
                        var tempIndex = _indexArray[j];
                        _indexArray[j] = _indexArray[j + 1];
                        _indexArray[j + 1] = tempIndex;
                    }
                }
            }
        }

        /// <summary>
        /// プレイヤーを生成 どの種類の食材を生成するのかという情報が必要
        /// </summary>
        public void CreatePlayers()
        {
            ///プレイヤー番号 一人目 ＝ 0番目
            int playerNumber = 0;
            foodStatuses = new FoodStatus[_playerSumNumber];
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
                foodStatuses[playerNumber] = Instantiate(_aIPrefab).GetComponent<FoodStatus>();
                foodStatuses[playerNumber].playerNumber = playerNumber + 1;
                playerNumber++;
            }
            ///各プレイヤーを初期位置に配置
            for (int i = 0; i < _playerSumNumber; i++)
            {
                foodStatuses[i].transform.position = startPoint;
                startPoint.x += 0.5f;
            }
            ///値を元にFoodStatusesを並び替える
            PlayerInOrder();
            ShotManager.Instance.SetShotManager(foodStatuses[_activePlayerIndex].Rigidbody);
            CameraManager.Instance.SetCameraMoveCenterPosition(foodStatuses[_activePlayerIndex].transform.position);
            PredictLineController.Instance.SetPredictLineInstantiatePosition(foodStatuses[_activePlayerIndex].transform.position);
            ///ターンを1にセットしてゲーム開始
            _turnNumber = 1;
        }

        // Update is called once per frame
        void Update()
        {
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
        public void ChangeTurn()
        {
            ///次のターン数へ
            _activePlayerIndex++;
            switch (StageSceneManager.Instance.GameState)
            {
                case StageGameState.Preparation:
                    {
                        ///順番決め終了
                        if (_activePlayerIndex == GameManager.Instance.playerNumber)
                        {
                            _activePlayerIndex = 0;
                            ///値を比較して順番を決定する
                        }
                    }
                    break;
                case StageGameState.Play:
                    {
                        if (_activePlayerIndex == _playerSumNumber)
                        {
                            _activePlayerIndex = 0;
                            _turnNumber++;
                        }
                        UIManager.Instance.ResetUIMode();
                        SetObjectsPositionForNextPlayer(_activePlayerIndex);
                    }
                    break;
                case StageGameState.Finish:
                    break;
                default:
                    break;
            }
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
        public void ResetPlayerOnStartPoint()
        {
            foodStatuses[_activePlayerIndex].ReStart(_startPositionObject.position);
            foodStatuses[_activePlayerIndex].Rigidbody.velocity = Vector3.zero;
            foodStatuses[_activePlayerIndex].transform.eulerAngles = Vector3.zero;
        }
    }

}
