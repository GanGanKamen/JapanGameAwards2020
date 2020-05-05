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
        /// プレイヤーの情報 ターンを回す際に必要
        /// </summary>
        public FoodStatus[] FoodStatuses
        {
            get { return _foodStatuses; }
        }
        private FoodStatus[] _foodStatuses;
        /// <summary>
        /// メーターで変動させる順番を決めるための値
        /// </summary>
        public float[] OrderPower
        {
            get { return _orderPower; }
        }
       private float[] _orderPower;
        /// <summary>
        /// ターン終了後の処理分岐用
        /// </summary>
        enum AfterChangeTurnState
        {
            NotChange, Change, GameEnd
        }
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
            _orderPower = new float[GameManager.Instance.PlayerSumNumber];
            _playerIndexArray = new int[GameManager.Instance.PlayerSumNumber];
            _foodStatuses = new FoodStatus[GameManager.Instance.PlayerSumNumber];
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
        private void PlayerInOrder()
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
        /// 食材生成後に呼ぶ ターン制の初期化
        /// </summary>
        private void InitializeTurn()
        {
            //順番決めの値を元に_foodStatusesを並び替える 順番決めが終わった時点でactiveindexは0に初期化 流れを追いかけにくい(現状)
            PlayerInOrder();
            SetObjectsPositionForNextPlayer(_activePlayerIndex);
            CheckNextPlayerAI();
            ///ターンを1にセットしてゲーム開始
            _turnNumber = 1;
        }

        /// <summary>
        /// foodStatusに登録
        /// </summary>
        public void SetFoodStatusValue(int playerNumber , FoodStatus playerStatus)
        {
            _foodStatuses[playerNumber] = playerStatus;
        }

        // Update is called once per frame
        void Update()
        {
            if (StageSceneManager.Instance.GameState == StageGameState.FinishFoodInstantiate)
            {
                InitializeTurn();
            }
            else if (StageSceneManager.Instance.FoodStateOnGameProperty == StageSceneManager.FoodStateOnGame.ShotEnd)
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
                        if (_foodStatuses[_activePlayerIndex].IsFoodInStartArea)
                        {
                            var playerNumber = GetPlayerNumberFromActivePlayerIndex(_activePlayerIndex) - 1;
                            var startPoint = StageSceneManager.Instance.GetPlayerStartPoint(playerNumber);
                            //スタート地点へ配置
                            ResetPlayerOnStartPoint(startPoint, _activePlayerIndex);
                            InitializeOnTurnChange();
                            break;
                        }
                        else if (_foodStatuses[_activePlayerIndex].IsGoal)
                        {
                            StageSceneManager.Instance.AddPlayerPointToList(_activePlayerIndex);
                            var foodType = _foodStatuses[_activePlayerIndex].FoodType;
                            var playerNumber = GetPlayerNumberFromActivePlayerIndex(_activePlayerIndex) - 1;
                            StageSceneManager.Instance.InitializePlayerData(playerNumber, foodType, _isAITurn);
                        }
                        //次のプレイヤーに順番を回す
                        _activePlayerIndex++;
                        //次のターンにいくかどうか・ゲーム終了かで処理分岐
                        switch (IsChangeTurn())
                        {
                            case AfterChangeTurnState.NotChange:
                                break;
                            case AfterChangeTurnState.Change:
                                _activePlayerIndex = 0;
                                _turnNumber++;
                                break;
                            case AfterChangeTurnState.GameEnd:
                                //処理終了
                                return;
                            default:
                                break;
                        }
                        InitializeOnTurnChange();
                        //レア調味料発生
                        if (StageSceneManager.Instance.TurnNumberOnGameEnd - _turnNumber == 2)//ラスト3ターン
                        {
                            GimmickManager.Instance.AppearRareSeasoning();
                        }
                    }
                    break;
                case StageGameState.Finish:
                    break;
                default:
                    break;
            }
        }

        private AfterChangeTurnState IsChangeTurn()
        {
            if (_activePlayerIndex == GameManager.Instance.PlayerSumNumber)
            {
                if (_turnNumber > StageSceneManager.Instance.TurnNumberOnGameEnd)
                {
                    return AfterChangeTurnState.GameEnd;
                }
                else
                {
                    return AfterChangeTurnState.Change;
                }
            }
            else
            {
                return AfterChangeTurnState.NotChange;
            }
        }

        /// <summary>
        /// ターンが変わるときの初期化処理
        /// </summary>
        private void InitializeOnTurnChange()
        {
            //順巡り処理(0へ初期化)が終わった後にチェック
            CheckNextPlayerAI();
            _foodStatuses[_activePlayerIndex].ResetFallAndGoalFlag();
            UIManager.Instance.ResetUIMode();
            SetObjectsPositionForNextPlayer(_activePlayerIndex);
            CheckPlayerAnimationPlay();
            PredictLineManager.Instance.SetPredictLineInstantiatePosition(_foodStatuses[_activePlayerIndex].CenterPoint.position);
        }

        /// <summary>
        /// プレイヤーのアニメーションを再生するか判断
        /// </summary>
        private void CheckPlayerAnimationPlay()
        {
            if (!_isAITurn)
            {
                switch (_foodStatuses[_activePlayerIndex].FoodType)
                {
                    case FoodType.Shrimp:
                        _foodStatuses[_activePlayerIndex].PlayerAnimatioManage(true);
                        _foodStatuses[_activePlayerIndex].SetShotPointOnFoodCenter();
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
        /// 異常落下にも呼ばれる アニメーション再生中などショット前プレイヤー落下時にsceneManagerに呼ばれる + 初期化時
        /// </summary>
        /// <param name="startPoint">初期位置</param>
        /// <param name="playerIndex">プレイヤー番号</param>
        public void ResetPlayerOnStartPoint(Vector3 startPoint , int playerIndex)
        {
            _foodStatuses[playerIndex].ReStart(startPoint);
            _foodStatuses[playerIndex].Rigidbody.velocity = Vector3.zero;
            switch (_foodStatuses[playerIndex].FoodType)
            {
                case FoodType.Shrimp:
                    _foodStatuses[playerIndex].transform.eulerAngles = Vector3.zero;
                    break;
                case FoodType.Egg:
                    _foodStatuses[playerIndex].transform.eulerAngles = Vector3.zero;
                    break;
                case FoodType.Chicken:
                    _foodStatuses[playerIndex].transform.eulerAngles = Vector3.zero;
                    break;
                case FoodType.Sausage:
                    _foodStatuses[playerIndex].transform.eulerAngles = Vector3.zero;
                    break;
                default:
                    break;
            }
        }
    }
}
