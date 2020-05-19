using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cooking
{
    public enum SceneName
    {
        Title, SelectStage, PlayScene
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

        public static void LoadActiveScene()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(sceneName);
        }

        public static void LoadSelectingScene(SceneName sceneName)
        {
            SceneManager.LoadScene((int)sceneName);
        }
    }

}

