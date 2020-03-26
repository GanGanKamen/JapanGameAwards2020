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
        private StageGameState _gameState = StageGameState.ChooseFood;
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
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            switch (_gameState)
            {
                case StageGameState.ChooseFood:
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
        }
        /// <summary>
        /// 落下後(物理挙動)に実行するために使用 trueの検出を1フレーム残す
        /// </summary>
        private void FixedUpdate()
        {
            switch (_gameState)
            {
                case StageGameState.ChooseFood:
                    break;
                case StageGameState.Play:
                    {
                        var turnController = TurnController.Instance;
                        if (turnController.foodStatuses[turnController.ActivePlayerIndex].IsFall)
                        {
                            turnController.ReSetPlayerOnStartPoint();
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
