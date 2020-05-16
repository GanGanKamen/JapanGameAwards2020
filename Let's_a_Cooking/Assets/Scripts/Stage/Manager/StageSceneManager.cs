using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Cooking.Stage
{
    /// <summary>
    /// スクリプトで使用するタグ ToString()して用いる
    /// </summary>
    public enum TagList
    {
       Finish , Floor , Water, Seasoning, Towel, DirtDish, RareSeasoning, Wall, TowelAbovePoint, Knife, Bubble , BubbleZone, StartArea
    }
    public enum LayerList
    {
        FoodLayerInStartArea,Kitchen
    }
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
        /// 重力加速度の大きさ 落下地点を計算するうえで必要  各予測線オブジェクト生成時に渡す
        /// 食材のもつ重力の値は現在9.81 。この値を変える場合、スクリプトで重力制御をする必要あり スケールではなく重さを変えると、ショット時に加えるべき力の量が変わり 。
        /// </summary>
        public readonly float gravityAccelerationValue = 9.81f;
        private AILevel[] aiLevels; 
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
        private float _waitTimeNormal = 1f;
        private float _waitTimeOnFall = 2f;
        private float _waitTimeOnGoal = 2f;
        private float _waitTimeCounterOnShotEnd = 0;
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
        /// 終了時の各プレイヤーの合計ポイント
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
            Normal, Falled, Goal, WaitForFoodStop, ShotEnd
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
        /// ヒエラルキー整頓用の食材の位置情報を制御するオブジェクト用の親オブジェクト
        /// </summary>
        public Transform FoodPositionsParent
        {
            get { return _foodPositionsParent; }
        }
        /// <summary>
        /// ヒエラルキー整頓用の食材の位置情報を制御するオブジェクト用の親オブジェクト
        /// </summary>
        [SerializeField] Transform _foodPositionsParent = null;
        /// <summary>
        /// LayerList[0]FoodLayerInStartArea,[1]Kitchen intとして参照 10進数に変換が必要 GetStringLayerName()はstring
        /// </summary>
        public LayerMask[] LayerListProperty
        {
            get { return _layerList; }
        }
        [SerializeField, Header("[0]FoodLayerInStartArea, [1]Kitchen stringとして使うものリスト")]
        private LayerMask[] _layerList = null;
        /// <summary>
        /// stringとしてレイヤーを参照
        /// </summary>
        /// <param name="layerList"></param>
        /// <returns></returns>
        public string GetStringLayerName(LayerMask layerList)
        {
            return LayerMask.LayerToName(CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(layerList));
        }

        /// <summary>
        /// 選ばれた食材に応じてプレイヤーを生成するために選んだ文字列保存 食材を選ぶ際のマウスクリックで呼ばれる
        /// </summary>
        /// <param name="foodName">選ばれた食材の名前</param>
        public void SetChooseFoodNames(string foodName)
        {
            _chooseFoodNames[_turnManager.ActivePlayerIndex] = foodName;
        }
        /// <summary>
        /// 生成するプレハブを選択 戻り値をInstantiateする
        /// </summary>
        /// <param name="foodType"></param>
        /// <param name="isAI"></param>
        /// <returns></returns>
        private GameObject ChooseInstantiatePrefab(FoodType foodType, bool isAI)
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
                GameObject.FindGameObjectWithTag(TagList.Finish.ToString());
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
                case StageGameState.FinishFoodInstantiateAndPlayerInOrder:
                    _gameState = StageGameState.Play;
                    break;
                case StageGameState.Play:
                    {
                        if (_turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].FalledFoodStateOnStartProperty == FalledFoodStateOnStart.OnStart)
                        {
                            _turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].ResetFoodState();
                            PredictLineManager.Instance.SetPredictLineInstantiatePosition(_turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].CenterPoint.position);
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
                case ScreenState.InitializeChoose:
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
                                _waitTimeCounterOnShotEnd = WaitForShotEndTIme(_waitTimeNormal, _waitTimeCounterOnShotEnd);//UI表示時間分だけ処理を待機
                            }
                            break;
                        case FoodStateOnGame.Falled:
                            _waitTimeCounterOnShotEnd = WaitForShotEndTIme(_waitTimeOnFall, _waitTimeCounterOnShotEnd);//UI表示時間分だけ処理を待機
                            break;
                        case FoodStateOnGame.Goal:
                            _waitTimeCounterOnShotEnd = WaitForShotEndTIme(_waitTimeOnGoal, _waitTimeCounterOnShotEnd);//UI表示時間分だけ処理を待機
                            break;
                        case FoodStateOnGame.WaitForFoodStop:
                            //プレイヤーが止まるまで待機
                            if (ShotManager.Instance.ShotModeProperty == ShotState.ShotEndMode)
                            {
                                _foodStateOnGame = FoodStateOnGame.ShotEnd;
                                //落下してたらスタートへ戻す
                                if (_turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].IsFall)
                                {
                                    var startPoint = GetPlayerStartPoint(_turnManager.GetPlayerNumberFromActivePlayerIndex(_turnManager.ActivePlayerIndex) - 1);
                                    _turnManager.ResetPlayerOnStartPoint(startPoint, _turnManager.ActivePlayerIndex);
                                }
                            }
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

        public void GameEnd()
        {
            _gameState = StageGameState.Finish;
            UIManager.Instance.ChangeUI(ScreenState.Finish);
        }

        /// <summary>
        /// プレイヤーを生成してTurnManagerに情報を渡す どの種類の食材を生成するのかという情報が必要
        /// </summary>
        private void CreatePlayersOnInitialize()
        {
            //仮の値を入れて変換した値を取得
            FoodType playerFoodType = GetFoodType(FoodType.Shrimp);
            //プレイヤー番号が小さいのがプレイしている人で大きい数字はAI
            for (int playerNumber = 0; playerNumber < GameManager.Instance.PlayerSumNumber; playerNumber++)
            {
                //プレイヤーを生成
                if (playerNumber < GameManager.Instance.playerNumber)
                {
                    InitializePlayerData(playerNumber, playerFoodType, false);
                }
                else
                {
                    //プレイヤーと同じ食材がAIになる
                    InitializePlayerData(playerNumber, playerFoodType, true);
                }
            }
            InitializePlayerPointList(GameManager.Instance.PlayerSumNumber);
            //プレイヤーの並び替えが行われる
            _gameState = StageGameState.FinishFoodInstantiateAndPlayerInOrder;
        }

        public FoodType GetFoodType(FoodType foodType)
        {
            //文字列に変換後、正しい値を代入                                                  //現状プレイヤーはすべて同じ食材→最初のプレイヤーが選んだ値
            return EnumParseMethod.TryParseAndDebugAssertFormatAndReturnResult(_chooseFoodNames[0], true, foodType);
        }

        /// <summary>
        /// プレイヤーの情報を初期化 TurnManagerとプレイヤー個人が持つ情報 生成時に呼ばれる 初期化時はこの後順番を並び替える
        /// </summary>
        /// <param name="foodStatusIndex">FoodStatusにセットするプレイヤーのindex番号(単なる番号ではない)初期化時はまだ番号に順番の意味はない</param>
        /// <param name="playerFoodType">食材の種類</param>
        /// <param name="isAI">AIかどうか</param>
        public void InitializePlayerData(int foodStatusIndex, FoodType playerFoodType, bool isAI)
        {
            //次の食材を生成
            FoodStatus playerStatus = InstantiateNextFood(playerFoodType, isAI);
            //foodStatus配列に登録
            _turnManager.SetFoodStatusValue(foodStatusIndex, playerStatus);
            //食材の種類を食材に渡す
            playerStatus.SetFoodTypeOnInitialize(playerFoodType);
            //Position初期化 スタート地点へ配置
            _turnManager.ResetPlayerOnStartPoint(GetPlayerStartPoint(foodStatusIndex), foodStatusIndex);
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
                _foodStateOnGame = FoodStateOnGame.WaitForFoodStop;
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
        public FoodStatus InstantiateNextFood(FoodType foodType, bool isAI)
        {
            if (isAI)
            {
                var nextAI = Instantiate(ChooseInstantiatePrefab(foodType, isAI)).GetComponent<FoodStatus>();
                return nextAI;
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
