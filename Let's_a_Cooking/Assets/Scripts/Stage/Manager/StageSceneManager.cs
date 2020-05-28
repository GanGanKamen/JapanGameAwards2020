using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System;

namespace Cooking.Stage
{
    /// <summary>
    /// スクリプトで使用するタグ ToString()して用いる
    /// </summary>
    public enum TagList
    {
        Finish, Floor, Water, Seasoning, Towel, DirtDish, RareSeasoning, Wall, TowelAbovePoint, Knife, Bubble, BubbleZone, StartArea, Chair, CameraZone , NotBeAITarget
    }
    public enum LayerList
    {
        FoodLayerInStartArea, Kitchen
    }
    public class StageSceneManager : MonoBehaviour
    {
        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// このクラスのインスタンスを取得。
        /// </summary>
        public static StageSceneManager Instance
        {
            get { return _instance; }
        }
        static StageSceneManager _instance = null;
        #endregion
        public StageGameState GameState
        {
            get { return _gameState; }
            set
            {
                _gameState = value;
            }
        }
        private StageGameState _gameState = StageGameState.Preparation;
        /// <summary>この要素番号に従ってステージを生成</summary>
        public int StageNumberIndex
        {
            get { return _stageNumberIndex; }
        }
        private static int _stageNumberIndex = 0;
        /// <summary>
        /// プレハブからステージを生成する場合はtrue Develop時false
        /// </summary>
        /// <remarks>ステージ開発用のシーンではfalseに設定します。</remarks>
        [SerializeReference] private bool _instantiateStage = true;
        /// <summary>
        /// 重力加速度の大きさ 落下地点を計算するうえで必要  各予測線オブジェクト生成時に渡す
        /// 食材のもつ重力の値は現在9.81 。この値を変える場合、スクリプトで重力制御をする必要あり スケールではなく重さを変えると、ショット時に加えるべき力の量が変わり 。
        /// </summary>
        public readonly float gravityAccelerationValue = 9.81f;
        public AILevel[] AILevels
        {
            get { return _aiLevels; }
        }
        /// <summary>
        /// 選択されたAIレベル
        /// </summary>
        private AILevel[] _aiLevels;
        /// <summary>
        /// 初期値Shrimp 選ばれた食材リスト FoodStatus用のenumへ変換
        /// </summary>
        private FoodType[] _chooseFoodTypes;
        [SerializeField] GameObject[] _playerPrefabs = new GameObject[Enum.GetValues(typeof(FoodType)).Length];
        [SerializeField] GameObject[] _aIPrefabs = new GameObject[Enum.GetValues(typeof(FoodType)).Length];
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
        private Transform _startPositionObject = null;
        public GameObject[] Goal
        {
            get { return _goals[0] ? _goals : GameObject.FindGameObjectsWithTag(TagList.Finish.ToString()); }
        }
        private GameObject[] _goals = null;
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
        public float[][] AIShotRange
        {
            get { return _aiShotRange; }
        }
        private float[][] _aiShotRange;
        private void Awake()
        {
            _instance = this;
            CheckInstantiateStageNumber(_stageNumberIndex);
            //チェック忘れ防止
            if (!_instantiateStage && SceneManager.GetActiveScene().name == SceneName.PlayScene.ToString())
            {
                _instantiateStage = true;
                Debug.Log("ステージ生成フラグのセット忘れ");
            }
            if (_instantiateStage)
            {
                InstantiateStage(_stageNumberIndex);
            }
#if UNITY_EDITOR
            PredictFoodPhysics.CreatePredictFoodGroundedGameObject();
#endif
        }

        /// <summary>
        /// セットされているステージ番号が正しいかどうか確認
        /// </summary>
        /// <param name="stageNumberIndex">ステージ番号(index)</param>
        private bool CheckInstantiateStageNumber(int stageNumberIndex)
        {
            Debug.AssertFormat(
    stageNumberIndex >= 0 && stageNumberIndex < GameManager.Instance.sumStageNumber,
    "不正なStageNo : {0}が指定されました ステージ1を読み込みます。指定番号はindexです", stageNumberIndex);
            if (stageNumberIndex >= 0 && stageNumberIndex < GameManager.Instance.sumStageNumber)
            {
                return true;
            }
            // 範囲外のステージ番号が指定された場合
            else
            {
                // 最初のステージを読み込んでエラーをださない
                _stageNumberIndex = 0;
                return false;
            }
        }
        private void InstantiateStage(int stageNumber)
        {
            // ステージプレハブを読み込む
            var stage = Resources.Load<GameObject>("Stages/Stage" + stageNumber.ToString());
            Instantiate(stage);
        }
        /// <summary>
        /// Selectステージからステージ番号を決める
        /// </summary>
        /// <param name="stageIndex"></param>
        public static void SetLoadStageIndex(int stageIndex)
        {
            _stageNumberIndex = stageIndex;
        }
        /// <summary>
        /// stringとしてレイヤーを参照
        /// </summary>
        /// <param name="layerList"></param>
        /// <returns></returns>
        public string GetStringLayerName(LayerMask layerList)
        {
            return LayerMask.LayerToName(CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(layerList));
        }
        public void SetAILevel(string selectedAILevel)
        {
            var aiLevel = AILevel.Easy;
            //文字列に変換後、正しい値を代入                                                 
            EnumParseMethod.TryParseAndDebugAssertFormat(selectedAILevel, true, out aiLevel);
            //現状AIは同じ強さ
            for (int aiNumber = 0; aiNumber < _aiLevels.Length; aiNumber++)
            {
                _aiLevels[aiNumber] = aiLevel;
            }
        }
        /// <summary>
        /// 選ばれた食材に応じてプレイヤーを生成するために選んだ種類を保存 食材を選ぶ際のマウスクリックで呼ばれる
        /// </summary>
        /// <param name="foodName">選ばれた食材の名前</param>
        public void SetChooseFoodNames(string foodName)
        {
            var foodType = FoodType.Shrimp;
            //文字列に変換後、正しい値を代入                                                  //現状プレイヤーはすべて同じ食材→最初のプレイヤーが選んだ値
            EnumParseMethod.TryParseAndDebugAssertFormat(foodName, true, out foodType);
            _chooseFoodTypes[_turnManager.ActivePlayerIndex] = foodType;
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
            _turnManager = TurnManager.Instance;
            _chooseFoodTypes = new FoodType[GameManager.Instance.PlayerSumNumber];
            _aiLevels = new AILevel[GameManager.Instance.ComputerNumber];
            var goals = GameObject.FindGameObjectsWithTag(TagList.Finish.ToString());
            _goals = new GameObject[goals.Length];
            for (int i = 0; i < _goals.Length; i++)
            {
                _goals[i] = goals[i];
            }
            for (int i = 0; i < _chooseFoodTypes.Length; i++)
            {
                _chooseFoodTypes[i] = FoodType.Shrimp;
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
                        foreach (var food in _turnManager.FoodStatuses)
                        {
                            if (food.FalledFoodStateOnStartProperty == FalledFoodStateOnStart.OnStart)
                            {
                                food.FinishStartProcessing();
                                _turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].ResetFoodState();
                                PredictLineManager.Instance.SetPredictLineInstantiatePosition(_turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].GroundPoint);
                                //PredictLineManager.Instance.SetPredictLineInstantiatePosition(food.CenterPoint.position);
                            }
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
            _aiShotRange = new float[GameManager.Instance.ComputerNumber][];
            //Resourcesからの読み取り回数を少なくしたい 
            var aIParameter = Resources.Load<AIParameter>("ScriptableObjects/AIParameter");
            int aiIndex = 0;
            //現状全AIは同じ強さ
            for (aiIndex = 0; aiIndex < _aiShotRange.Length; aiIndex++)
            {
                switch (_aiLevels[aiIndex])
                {
                    case AILevel.Easy:
                        _aiShotRange[aiIndex] = aIParameter.EasyRandomRange;
                        break;
                    case AILevel.Normal:
                        _aiShotRange[aiIndex] = aIParameter.NormalRandomRange;
                        break;
                    case AILevel.Hard:
                        _aiShotRange[aiIndex] = aIParameter.HardRandomRange;
                        break;
                    default:
                        break;
                }
            }
            aiIndex = 0;
            //プレイヤー番号が小さいのがプレイしている人で大きい数字はAI
            for (int playerNumber = 0; playerNumber < GameManager.Instance.PlayerSumNumber; playerNumber++)
            {
                //プレイヤーを生成
                if (playerNumber < GameManager.Instance.PlayerNumber)
                {
                    InitializePlayerData(playerNumber, _chooseFoodTypes[0], false, null);//現状同じ種類の食材を生成
                }
                else
                {
                    //プレイヤーと同じ食材がAIになる
                    InitializePlayerData(playerNumber, _chooseFoodTypes[0], true, _aiShotRange[aiIndex]);
                    aiIndex++;
                }
            }
            InitializePlayerPointList(GameManager.Instance.PlayerSumNumber);
            //プレイヤーの並び替えが行われる _aiShotRangeは並び替えていないので注意 (05/24 現状全員同じ難易度)
            _gameState = StageGameState.FinishFoodInstantiateAndPlayerInOrder;
        }

        /// <summary>
        /// プレイヤーの情報を初期化 TurnManagerとプレイヤー個人が持つ情報 生成時に呼ばれる 初期化時はこの後順番を並び替える
        /// </summary>
        /// <param name="foodStatusIndex">FoodStatusにセットするプレイヤーのindex番号(単なる番号ではない)初期化時はまだ番号に順番の意味はない</param>
        /// <param name="playerFoodType">食材の種類</param>
        /// <param name="isAI">AIかどうか</param>
        public void InitializePlayerData(int foodStatusIndex, FoodType playerFoodType, bool isAI, float[] shotRange)
        {
            //次の食材を生成
            FoodStatus playerStatus = InstantiateNextFood(playerFoodType, isAI);
            //AIのショットにかける補正の範囲をセット
            if (isAI)
            {
                var ai = playerStatus.GetComponent<AI>();
                ai.SetAIShotRange(shotRange);
            }
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
            if (_startPositionObject == null)
            {
                _startPositionObject = GameObject.FindGameObjectWithTag(TagList.StartArea.ToString()).transform;
            }
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
        /// <summary>
        /// オプションメニューを開くときに呼ばれる、各オブジェクトの状態変更メソッド
        /// </summary>
        public void OpenOptionMenu()
        {
            CameraManager.Instance.ChangeCameraState(CameraMode.Wait);
            ShotManager.Instance.ChangeShotState(ShotState.WaitMode);
            UIManager.Instance.ChangeButtonsEnableOnActiveUI(false);
        }
        /// <summary>
        /// オプションメニューを閉じるときに呼ばれる、各オブジェクトの状態変更メソッド
        /// </summary>
        public void CloseOptionMenu()
        {
            CameraManager.Instance.ReturnOptionMode();
            ShotManager.Instance.ReturnOptionMode();
            UIManager.Instance.ChangeButtonsEnableOnActiveUI(true);
        }
    }
}
