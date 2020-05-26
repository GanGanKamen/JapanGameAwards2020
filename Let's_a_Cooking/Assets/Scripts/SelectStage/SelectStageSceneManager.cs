using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cooking.SelectStage
{
    public class SelectStageSceneManager : MonoBehaviour
    {

        [SerializeField] GameObject[] buttonObjects;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }
        
        public void SelectStageNumber(int stageNumber)
        {
            for (int i = 0; i < buttonObjects.Length; i++)
            {
                var button = buttonObjects[i].GetComponent<Button>();

                button.enabled = false;
            }

            StartCoroutine(LoadSceneCoroutine(SceneName.PlayScene));
            Stage.StageSceneManager.SetLoadStageIndex(stageNumber);
        }

        public void OnReturnButton()
        {
            StartCoroutine(LoadSceneCoroutine(SceneName.Title));
        }

        IEnumerator LoadSceneCoroutine(SceneName sceneName)
        {
            if(sceneName == SceneName.PlayScene)
            {
                Fader.FadeInAndOut(0.8f, 1.4f, 0.8f);
                yield return new WaitForSeconds(1.5f);
                SceneChanger.LoadSelectingScene(SceneName.PlayScene);
            }
            else if (sceneName == SceneName.Title)
            {
                Fader.FadeInAndOutBlack(0.8f, 1.4f, 0.8f);
                yield return new WaitForSeconds(1.5f);
                SceneChanger.LoadSelectingScene(SceneName.Title);
            }

        }
    }
}