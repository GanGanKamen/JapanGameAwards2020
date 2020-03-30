using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        // Start is called before the first frame update
        void Start()
        {

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
                        TurnController.Instance.CreatePlayers();
                    }
                    break;
                case StageGameState.Play:
                    break;
                case StageGameState.Finish:
                    break;
                default:
                    break;
            }
            var predictLineController = PredictLineController.Instance;
            var turnController = TurnController.Instance;
            switch (UIManager.Instance.MainUIStateProperty)
            {
                case ScreenState.ChooseFood:
                    break;
                case ScreenState.DecideOrder:
                    break;
                case ScreenState.Start:
                    break;
                case ScreenState.AngleMode:
                    predictLineController.SetPredictLineInstantiatePosition(turnController.foodStatuses[turnController.ActivePlayerIndex].transform.position);
                    break;
                case ScreenState.SideMode:
                    predictLineController.SetPredictLineInstantiatePosition(turnController.foodStatuses[turnController.ActivePlayerIndex].transform.position);
                    break;
                case ScreenState.LookDownMode:
                    predictLineController.SetPredictLineInstantiatePosition(turnController.foodStatuses[turnController.ActivePlayerIndex].transform.position);
                    break;
                case ScreenState.PowerMeterMode:
                    predictLineController.SetPredictLineInstantiatePosition(turnController.foodStatuses[turnController.ActivePlayerIndex].transform.position);
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
                        var turnController = TurnController.Instance;
                        if (turnController.foodStatuses[turnController.ActivePlayerIndex].IsFall)
                        {
                            turnController.ResetPlayerOnStartPoint();
                        }
                    }
                    break;
                case StageGameState.Finish:
                    break;
                default:
                    break;
            }
        }
        private void ChangeGameState(StageGameState stageGameState)
        {

        }
    }

}
