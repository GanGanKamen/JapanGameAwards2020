using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// ターンの管理により、ゲームを進行。シーン内にあるすべてのFoodStatusの情報を参照して、アクティブ状態を切り替える。
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        /// <summary>
        /// 現在のターン数を表します。
        /// </summary>
        public int TurnNumber
        {
            get { return _turnNumber; }
        }
        private int _turnNumber = 0;
        /// <summary>
        /// AIのターンであることを表す
        /// </summary>
        public bool IsAITurn
        {
            get { return _isAITurn; }
        }
        private bool _isAITurn;
        [SerializeField] GameObject _playerPrefab = null;
        [SerializeField] GameObject _aIPrefab = null;
        /// <summary>
        /// プレイヤーの合計人数。ローカルでよく使うため定義
        /// </summary>
        private int _playerSumNumber = 1;
        /// <summary>
        /// _foodStatusesにおける要素番号。この値でアクティブプレイヤーに指示 準備段階にも有人プレイヤーの番号として使用 Chooseで仕様予定
        /// </summary>
        public int ActivePlayerIndex
        {
            get { return _activePlayerIndex; }
        }
        private int _activePlayerIndex = 0;
        /// <summary>
        /// 要素番号配列(プレイヤー番号管理・プレイヤー番号へ変換用) 用途：ターンを回す・リザルトでプレイヤー番号(=要素番号 + 1)を取得
        /// </summary>
        /// <remarks> 1 3 2 0ならプレイヤー2 4 3 1の順にターンが回る 0番目 = 最初にショットをするプレイヤーの番号 最初のターンのプレイヤーは何番のプレイヤーなのか？を知ることができる</remarks>
        public int[] PlayerIndexArray
        {
            get { return _playerIndexArray; }
        }
        private int[] _playerIndexArray;
        /// <summary>
        /// プレイヤーの情報
        /// </summary>
        public FoodStatus[] FoodStatuses
        {
            get { return _foodStatuses; }
        }
        private FoodStatus[] _foodStatuses;
        /// <summary>
        /// ゲーム開始時の座標を示すオブジェクト
        /// </summary>
        [SerializeField] Transform _startPositionObject = null;
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
        public static TurnManager Instance
        {
            get { return _instance; }
        }
        static TurnManager _instance = null;

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
            _playerSumNumber = GameManager.Instance.playerNumber + GameManager.Instance.computerNumber;
            _orderPower = new float[_playerSumNumber];
            _playerIndexArray = new int[_playerSumNumber];
            for (int i = 0; i < _playerIndexArray.Length; i++)
            {
                _playerIndexArray[i] = i;
            }
        }
        
        /// <summary>
        /// ショットを打つ順番を決める
        /// </summary>
        /// <param name="playerNumberIndex"></param>
        public float AIDecideOrderValue(int playerNumberIndex)
        {
            _orderPower[playerNumberIndex] = Random.Range(70.0f, 95.0f);
            return _orderPower[playerNumberIndex];
        }

        /// <summary>
        /// ショットを打つ順番を決める 値が決まってその値がもどってくる流れを尊重 AIに合わせた
        /// </summary>
        /// <param name="playerNumberIndex"></param>
        /// <param name="value"></param>
        public float PlayerDecideOrderValue(int playerNumberIndex, float value)
        {
            //ランダム要素 現状演出無しで値が最初に出て、プレイヤーはそれより大きいのを狙う形式
            _orderPower[playerNumberIndex] = value;
            return value;
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
                        //値とプレイヤーをシンクロさせて入れ替える
                        ArrayMethod.ChangeArrayValuesFromHighToLow(_orderPower,j);
                        ArrayMethod.ChangeArrayValuesFromHighToLow(_foodStatuses, j);
                        ArrayMethod.ChangeArrayValuesFromHighToLow(_playerIndexArray, j);
                    }
                }
            }
        }

        /// <summary>
        /// プレイヤーを生成 どの種類の食材を生成するのかという情報が必要
        /// </summary>
        public void CreatePlayersOnInitialize()
        {
            ///プレイヤー番号 一人目 ＝ 0番目
            int playerNumber = 0;
            _foodStatuses = new FoodStatus[_playerSumNumber];
            ///プレイヤーを生成 プレイヤー番号が小さいのがプレイしている人で大きいのがAI
            for (int i = 0; i < GameManager.Instance.playerNumber; i++)
            {
                _foodStatuses[playerNumber] = Instantiate(_playerPrefab).GetComponentInChildren<FoodStatus>();
                _foodStatuses[playerNumber].playerNumber = playerNumber + 1;
                playerNumber++;
            }
            ///AIを生成
            for (int i = 0; i < GameManager.Instance.computerNumber; i++)
            {
                _foodStatuses[playerNumber] = Instantiate(_aIPrefab).GetComponentInChildren<FoodStatus>();
                _foodStatuses[playerNumber].playerNumber = playerNumber + 1;
                playerNumber++;
            }
            ///各プレイヤーを初期位置に配置
            for (int i = 0; i < _playerSumNumber; i++)
            {
                _foodStatuses[i].transform.position = GetPlayerStartPoint(i);
            }
            StageSceneManager.Instance.InitializePlayerPointList(_playerSumNumber);
            StartCoroutine(WaitForCreatedPlayerStop());
        }
        /// <summary>
        /// 指定された元要素番号のプレイヤーのスタート地点を算出
        /// </summary>
        /// <param name="playerIndex"></param>
        private Vector3 GetPlayerStartPoint(int playerIndex)
        {
            var startPoint = _startPositionObject.position;
            return startPoint + new Vector3(0.5f * playerIndex, 0 , 0); //プレイヤー番号依存で少しずらして配置
        }

        /// <summary>
        /// 生成位置はシーン上で指定するので、地面の上にぴったり配置されない恐れあり。プレイヤー座標を参照する際カメラ位置がずれるのを防ぐ
        /// </summary>
        /// <returns></returns>
        IEnumerator WaitForCreatedPlayerStop()
        {
            while (StageSceneManager.Instance.GameState == StageGameState.Preparation)//(!_foodStatuses[_activePlayerIndex].OnKitchen)
            {
                yield return null;
            }
            InitializeTurn();
        }
        /// <summary>
        /// ターン制の初期化
        /// </summary>
        private void InitializeTurn()
        {
            ///順番決めの値を元に_foodStatusesを並び替える 順番決めが終わった時点でactiveindexは0に初期化 流れを追いかけにくい(現状)
            PlayerInOrder();
            SetObjectsPositionForNextPlayer(_activePlayerIndex);
            CheckNextPlayerAI();
            ///ターンを1にセットしてゲーム開始
            _turnNumber = 1;
        }

        // Update is called once per frame
        void Update()
        {
            if (StageSceneManager.Instance.FoodStateOnGameProperty == StageSceneManager.FoodStateOnGame.ShotEnd)
            {
                if (_foodStatuses[_activePlayerIndex].IsGoal)
                {
                    Debug.Log("Goal");
                }
            }
            ///デバッグ用 ゲーム終了
//#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Return) && !_isAITurn)
            {
                _turnNumber = 11;
            }
//#endif
        }
        /// <summary>
        /// アクティブプレイヤーIndexを渡せば、その要素のプレイヤーが何番のプレイヤーだったかを返す UI表示用に1加算している
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public int GetPlayerNumberFromActivePlayerIndex(int number)
        {
            return _playerIndexArray[number] + 1;
        }
        /// <summary>
        /// 次のターンのプレイヤーがAIかどうかを確認する プレイヤー作成後とターン切り替え時に呼ばれる
        /// </summary>
        void CheckNextPlayerAI()
        {
            var aI = _foodStatuses[_activePlayerIndex].GetComponent<AI>();
            if (aI != null )
            {
                _isAITurn = true;
                aI.TurnAI();
            }
            else
            {
                _isAITurn = false;
            }
        }
        /// <summary>
        /// アクティブプレイヤーを入れ替えて、次の人のターンに切り替え
        /// </summary>
        public void ChangeTurn()
        {
            switch (StageSceneManager.Instance.GameState)
            {
                case StageGameState.Preparation:
                    {
                        ///次のターン数へ
                        _activePlayerIndex++;
                        ///順番決め終了 このときAI生成されていない AI判定はGetComponentでは不可 アニメーションを入れるなら注意
                        if (_activePlayerIndex == GameManager.Instance.playerNumber)
                        {
                            _activePlayerIndex = 0;
                        }
                    }
                    break;
                    //10ターン目が終わったら終了(SceneManager)
                case StageGameState.Play:
                    {
                        if (_foodStatuses[_activePlayerIndex].IsGoal)
                        {
                            StageSceneManager.Instance.AddPlayerPointToList(_activePlayerIndex);
                            //次の食材を生成して登録
                            if (_isAITurn)
                                _foodStatuses[_activePlayerIndex] = Instantiate(_aIPrefab).GetComponentInChildren<FoodStatus>();
                            else
                                _foodStatuses[_activePlayerIndex] = Instantiate(_playerPrefab).GetComponentInChildren<FoodStatus>();
                            //スタート地点へ配置
                            ResetPlayerOnStartPoint();
                        }
                        //次のターン数へ
                        _activePlayerIndex++;
                        if (_activePlayerIndex == _playerSumNumber)
                        {
                            _activePlayerIndex = 0;
                            _turnNumber++;
                            if (_turnNumber > StageSceneManager.Instance.TurnNumberOnGameEnd)
                            {
                                break;
                            }
                        }
                        ///順巡り処理(0へ初期化)が終わった後にチェック
                        CheckNextPlayerAI();
                        UIManager.Instance.ResetUIMode();
                        SetObjectsPositionForNextPlayer(_activePlayerIndex);
                        if (!_isAITurn)
                        {
                            _foodStatuses[_activePlayerIndex].PlayerAnimatioManage(true);
                            _foodStatuses[_activePlayerIndex].SetShotPointOnFoodCenter();
                            PredictLineManager.Instance.SetPredictLineInstantiatePosition(_foodStatuses[_activePlayerIndex].CenterPoint.position);
                        }
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
            GimmickManager.Instance.WaterManager();
            GimmickManager.Instance.SeasoningManager();
            ShotManager.Instance.SetShotManager(_foodStatuses[activePlayerIndex].Rigidbody);
            CameraManager.Instance.SetCameraMoveCenterPosition(_foodStatuses[activePlayerIndex].transform.position);
            PredictLineManager.Instance.SetActivePredictShotPoint(!_isAITurn);
        }
        /// <summary>
        /// UI表示のない異常落下にも呼ばれる アニメーション再生中などショット前プレイヤー落下時にsceneManagerに呼ばれる
        /// </summary>
        public void ResetPlayerOnStartPoint()
        {
            var startPoint = GetPlayerStartPoint(GetPlayerNumberFromActivePlayerIndex(_activePlayerIndex) - 1); //プレイヤー番号 - 1 が元の要素番号
            _foodStatuses[_activePlayerIndex].ReStart(startPoint);
            _foodStatuses[_activePlayerIndex].Rigidbody.velocity = Vector3.zero;
            _foodStatuses[_activePlayerIndex].transform.eulerAngles = Vector3.zero;
        }
    }
}
