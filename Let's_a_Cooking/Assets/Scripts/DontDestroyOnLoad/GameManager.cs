using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// そのゲームを始める際に読み込むステージ・人数を記録
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region シングルトンインスタンス
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateInstance()
        {
            var obj = new GameObject("GameManager");
            obj.AddComponent<GameManager>();
            obj.AddComponent<SceneChanger>();
            obj.AddComponent<DataManager>();
        }

        /// <summary>
        /// このクラスのインスタンスを取得
        /// </summary>
        public static GameManager Instance
        {
            get
            {
                return instance;
            }
        }
        private static GameManager instance = null;

        /// <summary>
        /// Start()の実行より先行して処理したい内容を記述
        /// </summary>
        void Awake()
        {
            // 初回作成時
            if (instance == null)
            {
                instance = this;
                // シーンをまたいで削除されないように設定
                DontDestroyOnLoad(gameObject);
                // セーブデータを読み込む
                //Load();
            }
            // 2個目以降の作成時
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion
        /// <summary>
        /// 合計人数
        /// </summary>
        public int PlayerSumNumber
        {
            get { return playerNumber + computerNumber; }
        }
        /// <summary>
        ///プレイヤーの人数が想定を超えないように制御→プロパティ・例外処理
        /// </summary>
        public int playerNumber = 1,computerNumber = 1;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
