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
        /// 表示用 プレイヤーがゲーム全体で獲得した合計ポイント(食材再スタート前も含む)UIが常に参照する
        /// </summary>
        public List <int>[] PlayerPointList
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
        public GameObject goal;
        /// <summary>
        /// 終了時の書くプレイヤーの合計ポイント
        /// </summary>
        int[] _pointsOnFinish;
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
            ///タグ検索エラーをできるだけ防ぐ
            if (goal == null)
            {
                Debug.Log("ゴールオブジェクトがセットされていません。タグ検索されました。");
                GameObject.FindGameObjectWithTag("Finish");
            }
            _turnManager = TurnManager.Instance;
        }

        // Update is called once per frame
        void Update()
        {
            //Debug.Log(_foodStateOnGame);
            switch (_gameState)
            {
                case StageGameState.Preparation:
                    ///メソッド化予定
                    if (UIManager.Instance.MainUIStateProperty == ScreenState.Start)
                    {
                        _gameState = StageGameState.Play;
                        TurnManager.Instance.CreatePlayersOnInitialize();
                    }
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
                case ScreenState.PowerMeterMode:
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
                        _turnManager.ResetPlayerOnStartPoint();
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
                    switch (UIManager.Instance.MainUIStateProperty)
                    {
                        case ScreenState.FrontMode:
                            _turnManager.ResetPlayerOnStartPoint();
                            break;
                        case ScreenState.SideMode:
                            _turnManager.ResetPlayerOnStartPoint();
                            break;
                        case ScreenState.LookDownMode:
                            _turnManager.ResetPlayerOnStartPoint();
                            break;
                        case ScreenState.PowerMeterMode:
                            _turnManager.ResetPlayerOnStartPoint();
                            break;
                        default:
                            break;
                    }
                }

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
