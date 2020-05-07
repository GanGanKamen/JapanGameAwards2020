using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Cooking.Stage
{
    public class StageSceneManager : MonoBehaviour
    {
        public StageGameState GameState
        {
            get { return _gameState; }
            set
            {
                _gameState = value;
            }
        }
        private StageGameState _gameState = StageGameState.Preparation;
        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// このクラスのインスタンスを取得。
        /// </summary>
        public static StageSceneManager Instance
        {
            get { return _instance; }
        }
        static StageSceneManager _instance = null;

        /// <summary>
        /// Start()より先に実行。
        /// </summary>
        private void Awake()
        {
            _instance = this;
        }
        #endregion
        /// <summary>
        /// 重力の大きさが入り 。落下地点を計算するうえで必要 。Foodの重力の値を参照し 。各予測線オブジェクト生成時に渡し 。
        /// 食材のもつ重力の値は現在1 。この値を変える場合、スクリプトで重力制御をする必要があり 。スケールではなく重さを変えると、ショット時に加えるべき力の量が変わり 。
        /// 1から変えない想定 。
        /// </summary>
        public readonly float gravityScale = 1;
        /// <summary>
        /// 初期値Shrimp 選ばれた食材リスト FoodStatus用のenumへ変換
        /// </summary>
        private string[] _chooseFoodNames;
        [SerializeField] GameObject[] _playerPrefabs = new GameObject[System.Enum.GetValues(typeof(FoodType)).Length];
        [SerializeField] GameObject[] _aIPrefabs = new GameObject[System.Enum.GetValues(typeof(FoodType)).Length];
        /// <summary>
        /// 表示用 プレイヤーがゲーム全体で獲得した合計ポイント(食材再スタート前も含む)UIが常に参照する
        /// </summary>
        public List<int>[] PlayerPointList
        {
            get { return _playerPointList; }
        }
        private List<int>[] _playerPointList;
        /// <summary>
        /// やがて演出時間と同期
        /// </summary>
        private float _waitTimeNormal = 1f;
        private float _waitTimeCounterNormal = 0;
        private float _waitTimeOnFall = 2f;
        private float _waitTimeCounterOnFall = 0;
        private float _waitTimeOnGoal = 2f;
        private float _waitTimeCounterOnGoal = 0;
        /// <summary>
        /// ゲーム開始時の座標を示すオブジェクト
        /// </summary>
        [SerializeField] Transform _startPositionObject = null;
        public GameObject Goal
        {
            get { return _goal; }
        }
       [SerializeField] private GameObject _goal = null;
        /// <summary>
        /// 終了時の書くプレイヤーの合計ポイント
        /// </summary>
        int[] _pointsOnFinish;
        public int TurnNumberOnGameEnd
        {
            get { return _turnNumberOnGameEnd; }
        }
        [SerializeField] int _turnNumberOnGameEnd = 10;
        TurnManager _turnManager;
        /// <summary>
        /// ゲーム上でのアクティブプレイヤーの状態 Normal Falled Goal
        /// </summary>
        public enum FoodStateOnGame
        {
            Normal, Falled, Goal, ShotEnd
        }
        /// <summary>
        /// ゲーム上でのアクティブプレイヤーの状態 Normal Falled Goal
        /// </summary>
        public FoodStateOnGame FoodStateOnGameProperty
        {
            get { return _foodStateOnGame; }
        }
        private FoodStateOnGame _foodStateOnGame = FoodStateOnGame.Normal;
        /// <summary>
        /// ヒエラルキー整頓用の食材の親オブジェクト
        /// </summary>
        [SerializeField] Transform _foodPositionsParent = null;

        /// <summary>
        /// 選ばれた食材に応じてプレイヤーを生成するために選んだ文字列保存 食材を選ぶ際のマウスクリックで呼ばれる
        /// </summary>
        /// <param name="foodName">選ばれた食材の名前</param>
        public void SetChooseFoodNames(string foodName)
        {           
            _chooseFoodNames [_turnManager.ActivePlayerIndex] = foodName;
        }
        /// <summary>
        /// 生成するプレハブを選択 戻り値をInstantiateする
        /// </summary>
        /// <param name="foodType"></param>
        /// <param name="isAI"></param>
        /// <returns></returns>
        private GameObject ChooseInstantiatePrefab(FoodType foodType , bool isAI)
        {
            if (isAI)
            {
                return _aIPrefabs[(int)foodType];
            }
            else
            {
                return _playerPrefabs[(int)foodType];
            }
        }
        /// <summary>
        /// ポイントリストをプレイヤーの人数分用意
        /// </summary>
        /// <param name="playerNumber"></param>
        public void InitializePlayerPointList(int playerNumber)
        {
            _playerPointList = new List<int>[playerNumber];
            for (int i = 0; i < playerNumber; i++)
            {
                _playerPointList[i] = new List<int>();
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            ///タグ検索エラーを防ぐ
            if (_goal == null)
            {
                Debug.Log("ゴールオブジェクトがセットされていません。タグ検索されました。");
                GameObject.FindGameObjectWithTag("Finish");
            }
            _turnManager = TurnManager.Instance;
            _chooseFoodNames = new string[GameManager.Instance.PlayerSumNumber];
            for (int i = 0; i < _chooseFoodNames.Length; i++)
            {
                _chooseFoodNames[i] = FoodType.Shrimp.ToString();
            }
        }

        // Update is called once per frame
        void Update()
        {
            switch (_gameState)
            {
                case StageGameState.Preparation:
                    if (UIManager.Instance.MainUIStateProperty == ScreenState.Start)
                    {
                        CreatePlayersOnInitialize();
                    }
                    break;
                case StageGameState.FinishFoodInstantiate:
                    _gameState = StageGameState.Play;
                    break;
                case StageGameState.Play:
                    {
                        if (TurnManager.Instance.TurnNumber > _turnNumberOnGameEnd)
                        {
                            _gameState = StageGameState.Finish;
                            UIManager.Instance.ChangeUI("Finish");
                        }
                    }
                    break;
                case StageGameState.Finish:
                    {
                        switch (UIManager.Instance.FinishUIModeProperty)
                        {
                            case UIManager.FinishUIMode.Finish:
                                break;
                            case UIManager.FinishUIMode.Score:
                                break;
                            case UIManager.FinishUIMode.Retry:
                                SceneChanger.LoadActiveScene();
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
            var predictLineController = PredictLineManager.Instance;
            switch (UIManager.Instance.MainUIStateProperty)
            {
                case ScreenState.ChooseFood:
                    break;
                case ScreenState.DecideOrder:
                    break;
                case ScreenState.Start:
                    break;
                case ScreenState.FrontMode:
                    break;
                case ScreenState.SideMode:
                    break;
                case ScreenState.LookDownMode:
                    break;
                case ScreenState.ShottingMode:
                    switch (_foodStateOnGame)
                    {
                        case FoodStateOnGame.Normal:
                            if (_turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].IsGoal)
                            {
                                _foodStateOnGame = FoodStateOnGame.Goal;
                            }
                            else if (_turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].IsFall) // 万が一ゴール中に落下することはない
                            {
                                _foodStateOnGame = FoodStateOnGame.Falled;
                            }
                            else if (ShotManager.Instance.ShotModeProperty == ShotState.ShotEndMode)
                            {
                                _waitTimeCounterNormal = WaitForShotEndTIme(_waitTimeNormal, _waitTimeCounterNormal);//UI表示時間分だけ処理を待機
                            }
                            break;
                        case FoodStateOnGame.Falled:
                            _waitTimeCounterOnFall = WaitForShotEndTIme(_waitTimeOnFall, _waitTimeCounterOnFall);//UI表示時間分だけ処理を待機
                            break;
                        case FoodStateOnGame.Goal:
                            _waitTimeCounterOnGoal = WaitForShotEndTIme(_waitTimeOnGoal, _waitTimeCounterOnGoal);//UI表示時間分だけ処理を待機
                            break;
                        case FoodStateOnGame.ShotEnd:
                            _foodStateOnGame = FoodStateOnGame.Normal;
                            _turnManager.ChangeTurn();
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.Finish:
                    break;
                case ScreenState.Pause:
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// プレイヤーを生成してTurnManagerに情報を渡す どの種類の食材を生成するのかという情報が必要
        /// </summary>
        private void CreatePlayersOnInitialize()
        {
            //プレイヤー番号が小さいのがプレイしている人で大きい数字はAI
            for (int playerNumber = 0; playerNumber < GameManager.Instance.PlayerSumNumber; playerNumber++)
            {
                //プレイヤーを生成
                if (playerNumber < GameManager.Instance.playerNumber)
                {
                    //仮の値を入れる
                    FoodType playerFoodType = FoodType.Shrimp;
                    //文字列に変換後、正しい値を代入
                    playerFoodType = EnumParseMethod.TryParseAndDebugAssertFormatAndReturnResult(_chooseFoodNames[playerNumber], true, playerFoodType);
                    InitializePlayerData(playerNumber, playerFoodType, false);
                }
                else
                {
                    //仮の値を入れる
                    FoodType aIFoodType = FoodType.Shrimp;
                    //文字列に変換後、正しい値を代入
                    aIFoodType = EnumParseMethod.TryParseAndDebugAssertFormatAndReturnResult(_chooseFoodNames[playerNumber], true, aIFoodType);
                    InitializePlayerData(playerNumber, aIFoodType, true);
                }
            }
            InitializePlayerPointList(GameManager.Instance.PlayerSumNumber);
            _gameState = StageGameState.FinishFoodInstantiate;
        }
        /// <summary>
        /// プレイヤーの情報を初期化 TurnManagerとプレイヤー個人が持つ情報 生成時に呼ばれる
        /// </summary>
        /// <param name="playerNumber">プレイヤーの番号</param>
        /// <param name="playerFoodType">食材の種類</param>
        /// <param name="isAI">AIかどうか</param>
        public void InitializePlayerData(int playerNumber, FoodType playerFoodType ,bool isAI)
        {
            //次の食材を生成
            FoodStatus playerStatus = InstantiateNextFood(playerFoodType, isAI);
            //foodStatus配列に登録
            _turnManager.SetFoodStatusValue(playerNumber, playerStatus);
            //食材の種類を食材に渡す
            playerStatus.SetFoodTypeOnInitialize(playerFoodType);
            //Position初期化 スタート地点へ配置
            _turnManager.ResetPlayerOnStartPoint(GetPlayerStartPoint(playerNumber), playerNumber);
            //親子関係初期化
            playerStatus.SetParentObject(_foodPositionsParent);
        }

        /// <summary>
        /// UI表示時間分だけ処理を待機。時間になったらNormalへ戻る 落下してたらスタート地点へ
        /// </summary>
        private float WaitForShotEndTIme(float waitTime, float waitTimeCounter)
        {
            waitTimeCounter += Time.deltaTime;
            if (waitTimeCounter > waitTime)
            {
                switch (_foodStateOnGame)
                {
                    case FoodStateOnGame.Falled:
                        var startPoint = GetPlayerStartPoint(_turnManager.GetPlayerNumberFromActivePlayerIndex(_turnManager.ActivePlayerIndex) - 1);
                        _turnManager.ResetPlayerOnStartPoint( startPoint, _turnManager.ActivePlayerIndex);
                        break;
                    case FoodStateOnGame.Goal:
                        break;
                    default:
                        break;
                }
                _foodStateOnGame = FoodStateOnGame.ShotEnd;
                return 0;
            }
            return waitTimeCounter;
        }
        /// <summary>
        /// 落下後(物理挙動)に実行するために使用 IsFall = trueの検出を1フレーム残す
        /// </summary>
        private void FixedUpdate()
        {
            if (_gameState == StageGameState.Play)
            {
                // UI表示のない異常落下にも呼ばれる アニメーション再生中などショット前プレイヤー落下時にsceneManagerに呼ばれる
                if (_turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].IsFall)
                {
                    var startPoint = GetPlayerStartPoint(_turnManager.GetPlayerNumberFromActivePlayerIndex(_turnManager.ActivePlayerIndex) - 1);
                    switch (UIManager.Instance.MainUIStateProperty)
                    {
                        case ScreenState.FrontMode:
                            _turnManager.ResetPlayerOnStartPoint(startPoint, _turnManager.ActivePlayerIndex);
                            break;
                        case ScreenState.SideMode:
                            _turnManager.ResetPlayerOnStartPoint(startPoint, _turnManager.ActivePlayerIndex);
                            break;
                        case ScreenState.LookDownMode:
                            _turnManager.ResetPlayerOnStartPoint(startPoint, _turnManager.ActivePlayerIndex);
                            break;
                        default:
                            break;
                    }
                }

            }
        }
        /// <summary>
        /// 指定された元要素番号のプレイヤーのスタート地点を算出
        /// </summary>
        /// <param name="playerIndex"></param>
        public Vector3 GetPlayerStartPoint(int playerIndex)
        {
            return _startPositionObject.position + new Vector3(0.5f * playerIndex, 0, 0); //プレイヤー番号依存で少しずらして配置
        }

        /// <summary>
        /// 食材の種類を選択して、次の食材を生成
        /// </summary>
        /// <param name="isAI">AIを生成するかどうか</param>
        /// <returns></returns>
        public FoodStatus InstantiateNextFood(FoodType foodType , bool isAI)
        {
            if (isAI)
            {
                var nextAI = Instantiate(ChooseInstantiatePrefab(foodType, isAI)).GetComponent<FoodStatus>();
                return Instantiate(_aIPrefabs[0]).GetComponentInChildren<FoodStatus>();
            }
            else
            {
                var nextFood = Instantiate(ChooseInstantiatePrefab(foodType, isAI)).GetComponent<FoodStatus>();
                return nextFood;
            }
        }
        /// <summary>
        /// 得点用配列を初期化・得点を取得し、並び替えることで大小比較
        /// </summary>
        public void ComparePlayerPointOnFinish()
        {
            InitializeFinishPointArray();
            InDescendingOrder();
            //降順に並び替えたことにより、最初の要素に最もポイントの高いプレイヤーが来る
            UIManager.Instance.UpdateWinnerPlayerNumber(_turnManager.GetPlayerNumberFromActivePlayerIndex(0));
        }
        /// <summary>
        /// ポイントを降順に並び替え
        /// </summary>
        private void InDescendingOrder()
        {
            for (int i = 0; i < _pointsOnFinish.Length - 1; i++)
            {
                for (int j = 0; j < _pointsOnFinish.Length - 1 - i; j++)
                {
                    if (_pointsOnFinish[j] < _pointsOnFinish[j + 1])
                    {
                        ArrayMethod.ChangeArrayValuesFromHighToLow(_pointsOnFinish, j);
                        //indexArray = プレイヤー番号 をポイントの高い順に並び替える
                        ArrayMethod.ChangeArrayValuesFromHighToLow(_turnManager.PlayerIndexArray, j);
                    }
                }
            }
        }
        /// <summary>
        /// 配列初期化・ポイント取得
        /// </summary>
        private void InitializeFinishPointArray()
        {
            _pointsOnFinish = new int[_playerPointList.Length];
            for (int i = 0; i < _turnManager.FoodStatuses.Length; i++)
            {
                _pointsOnFinish[i] = GetSumPlayerPoint(i);
            }
        }
        public void AddPlayerPointToList(int playerPointIndex)
        {
            _playerPointList[playerPointIndex].Add(_turnManager.FoodStatuses[playerPointIndex].PlayerPointProperty.Point);
        }
        public int GetSumPlayerPoint(int playerIndex)
        {
            return PlayerPointList[playerIndex].Sum();
        }
        private void ChangeGameState(StageGameState stageGameState)
        {

        }
    }
}
