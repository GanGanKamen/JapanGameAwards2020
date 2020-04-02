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
            ///メソッド化予定
            if (_gameState == StageGameState.ChooseFood && UIManager.Instance.MainUIStateProperty == ScreenState.Start)
            {
                _gameState = StageGameState.Play;
                TurnController.Instance.CreatePlayers();
            }
        }
        private void ChangeGameState(StageGameState stageGameState)
        {

        }
    }

}
