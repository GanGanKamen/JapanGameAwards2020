using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// そのゲームを始める際に読み込むステージ・人数を記録します。
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
            DontDestroyOnLoad(obj);
        }

        /// <summary>
        /// このクラスのインスタンスを取得します。
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
        /// Start()の実行より先行して処理したい内容を記述します。
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
        ///プレイヤーの人数が想定を超えないように制御→プロパティ・例外処理
        public int playerNumber = 1;
        public int computerNumber = 0;
        /// <summary>
        /// 重力の大きさが入ります。落下地点を計算するうえで必要です。Foodの重力の値を参照します。各予測線オブジェクト生成時に渡します。
        /// 食材のもつ重力の値は現在1です。この値を変える場合、スクリプトで重力制御をする必要があります。スケールではなく重さを変えると、ショット時に加えるべき力の量が変わります。
        /// 1から変えない想定です。
        /// </summary>
        public readonly float gravityScale = 1;

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
