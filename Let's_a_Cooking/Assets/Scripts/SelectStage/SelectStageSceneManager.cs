using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cooking.SelectStage
{
    public class SelectStageSceneManager : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Button goButton;
        [SerializeField] private Button[] buttonObjects;
        [SerializeField] private GameObject[] selectedObjects;
        [SerializeField] private GameObject[] stageViews;
        private int beSelectedNum = 0;

        // Start is called before the first frame update
        void Start()
        {
            ButtonsInit();
            SelectStageNumber(0);
        }

        // Update is called once per frame
        void Update()
        {

        }
        
        private void ButtonsInit()
        {
            backButton.onClick.AddListener(() => OnReturnButton());
            goButton.onClick.AddListener(() => GotoStage());
            for(int i = 0; i< buttonObjects.Length; i++)
            {
                int j = i;
                buttonObjects[j].onClick.AddListener(() => SelectStageNumber(j));
                selectedObjects[j].SetActive(false);
                stageViews[i].SetActive(false);
            }
        }

        public void SelectStageNumber(int stageNumber)
        {
            beSelectedNum = stageNumber;
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                selectedObjects[i].SetActive(false);
                stageViews[i].SetActive(false);
            }
            stageViews[beSelectedNum].SetActive(true);
            selectedObjects[beSelectedNum].SetActive(true);
        }

        public void GotoStage()
        {
            for (int i = 0; i < buttonObjects.Length; i++)
            {
                buttonObjects[i].enabled = false;
            }
            StartCoroutine(LoadSceneCoroutine(SceneName.PlayScene));
            Stage.StageSceneManager.SetLoadStageIndex(beSelectedNum);
        }

        public void OnReturnButton()
        {
            StartCoroutine(LoadSceneCoroutine(SceneName.Title));
        }

        IEnumerator LoadSceneCoroutine(SceneName sceneName)
        {
            Debug.Log(sceneName);
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