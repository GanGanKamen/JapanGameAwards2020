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

    public class SceneChanger : MonoBehaviour
	{
		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
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
        }
    }

}

