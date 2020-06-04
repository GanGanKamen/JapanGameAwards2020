using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cooking
{
    public enum SceneName
    {
       OP , Title, SelectStage, PlayScene
    }
    public class SceneChanger : SingletonInstance<SceneChanger>
	{
        static public string activeSceneName = "";
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var obj = new GameObject("SceneChanger");
            obj.AddComponent<SceneChanger>();
            activeSceneName = SceneManager.GetActiveScene().name;
        }

        protected override void Awake()
        {
            CreateSingletonInstance(this, true);
        }

        // Start is called before the first frame update
        protected override void Start()
		{
            base.Start();
		}

		// Update is called once per frame
		void Update()
		{
            activeSceneName = SceneManager.GetActiveScene().name;
            ///デバッグ用
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.A))
            {
                LoadActiveScene();
            }
#endif
        }
        /// <summary>
        /// 現在のシーンを再読み込み
        /// </summary>
        public static void LoadActiveScene()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(sceneName);
        }
        /// <summary>
        /// 指定されたシーンを読み込む
        /// </summary>
        /// <param name="sceneName">シーンを指定</param>
        public static void LoadSelectingScene(SceneName sceneName)
        {
            SceneManager.LoadScene((int)sceneName);
            if (sceneName == SceneName.Title)
            {
                //シーンの名前に応じてBGMを変更 ステージごとに再生するBGMを変更
                SoundManager.Instance.ChangeBGMOnSceneName(sceneName);
            }
            else if (sceneName == SceneName.SelectStage)
            {
                //シーンの名前に応じてBGMを変更 ステージごとに再生するBGMを変更
                SoundManager.Instance.ChangeBGMOnSceneName(sceneName);
            }
        }
    }

}

