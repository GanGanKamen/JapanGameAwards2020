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
        public GameObject goal;
        /// <summary>
        /// 終了時のポイント
        /// </summary>
        int[] _pointsOnFinish;
        TurnManager _turnController;
        // Start is called before the first frame update
        void Start()
        {
            ///タグ検索エラーをできるだけ防ぐ
            if (goal == null)
            {
                Debug.Log("ゴールオブジェクトがセットされていません。タグ検索されました。");
                GameObject.FindGameObjectWithTag("Finish");
            }
            _turnController = TurnManager.Instance;
        }

        // Update is called once per frame
        void Update()
        {
            switch (_gameState)
            {
                case StageGameState.Preparation:
                    ///メソッド化予定
                    if (UIManager.Instance.MainUIStateProperty == ScreenState.Start)
                    {
                        _gameState = StageGameState.Play;
                        TurnManager.Instance.CreatePlayers();
                    }
                    break;
                case StageGameState.Play:
                    {
                        if (TurnManager.Instance.TurnNumber > 10)
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
                    predictLineController.SetPredictLineInstantiatePosition(_turnController.FoodStatuses[_turnController.ActivePlayerIndex].transform.position);
                    break;
                case ScreenState.SideMode:
                    predictLineController.SetPredictLineInstantiatePosition(_turnController.FoodStatuses[_turnController.ActivePlayerIndex].transform.position);
                    break;
                case ScreenState.LookDownMode:
                    predictLineController.SetPredictLineInstantiatePosition(_turnController.FoodStatuses[_turnController.ActivePlayerIndex].transform.position);
                    break;
                case ScreenState.PowerMeterMode:
                    predictLineController.SetPredictLineInstantiatePosition(_turnController.FoodStatuses[_turnController.ActivePlayerIndex].transform.position);
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
        }
        /// <summary>
        /// 落下後(物理挙動)に実行するために使用 trueの検出を1フレーム残す
        /// </summary>
        private void FixedUpdate()
        {
            switch (_gameState)
            {
                case StageGameState.Preparation:
                    break;
                case StageGameState.Play:
                    {
                        if (_turnController.FoodStatuses[_turnController.ActivePlayerIndex].IsFall)
                        {
                            _turnController.ResetPlayerOnStartPoint();
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
        /// 得点用配列を初期化・得点を取得し、並び替えることで大小比較
        /// </summary>
        public void ComparePlayerPointOnFinish()
        {
            InitializeFinishPointArray();
            InDescendingOrder();
            //降順に並び替えたことにより、最初の要素に最もポイントの高いプレイヤーが来る
            UIManager.Instance.UpdateWinnerPlayerNumber(_turnController.GetPlayerNumber(0));
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
                        ArrayMethod.ChangeArrayValuesFromHighToLow(_turnController.PlayerIndexArray, j);
                    }
                }
            }
        }
        /// <summary>
        /// 配列初期化・ポイント取得
        /// </summary>
        private void InitializeFinishPointArray()
        {
            _pointsOnFinish = new int[_turnController.FoodStatuses.Length];
            for (int i = 0; i < _turnController.FoodStatuses.Length; i++)
            {
                _pointsOnFinish[i] = _turnController.FoodStatuses[i].playerPoint.Point;
            }
        }

        private void ChangeGameState(StageGameState stageGameState)
        {

        }
    }

}
