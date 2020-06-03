using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cooking
{
    /// <summary>
    /// そのゲームを始める際に読み込むステージ・人数を記録
    /// </summary>
    public class GameManager : SingletonInstance<GameManager>
    {
        #region シングルトンインスタンス
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateInstance()
        {
            var obj = new GameObject("GameManager");
            obj.AddComponent<GameManager>();
            obj.AddComponent<DataManager>();
        }

        protected override void Awake()
        {
            CreateSingletonInstance(this, true);
        }

        #endregion
        /// <summary>
        /// 合計人数
        /// </summary>
        public int PlayerSumNumber
        {
            get { return _playerNumber + _computerNumber; }
        }
        public int PlayerNumber
        {
            get { return _playerNumber; }
        }
        public int ComputerNumber
        {
            get { return _computerNumber; }
        }
        /// <summary>
        ///プレイヤーの人数が想定を超えないように制御→プロパティ・例外処理
        /// </summary>
        private int _playerNumber = 1,_computerNumber = 3;
        /// <summary>
        /// 合計ステージ数:3(2020/5/18)
        /// </summary>
        public readonly int sumStageNumber = 3;
        float openingTime = 10.5f;
        float openingTimeCounter = 0;

        // Update is called once per frame
        void Update()
        {
            if (SceneManager.GetActiveScene().name == SceneName.OP.ToString())
            {
                openingTimeCounter += Time.deltaTime;
                if (openingTimeCounter > openingTime)
                {
                    SceneChanger.LoadSelectingScene(SceneName.Title);
                    openingTimeCounter = 0;
                }
            }
        }
    }
}
