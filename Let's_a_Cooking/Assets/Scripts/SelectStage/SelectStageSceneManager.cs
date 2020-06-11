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
        [SerializeField] private Sprite defultImage;
        [SerializeField] private Sprite selectedImage;
        [SerializeField] private Button[] buttonObjects;
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
                buttonObjects[j].image.sprite = defultImage;
                stageViews[i].SetActive(false);
            }
        }

        public void SelectStageNumber(int stageNumber)
        {
            beSelectedNum = stageNumber;
            for (int i = 0; i < buttonObjects.Length; i++)
            {
                buttonObjects[i].image.sprite = defultImage;
                stageViews[i].SetActive(false);
            }
            stageViews[beSelectedNum].SetActive(true);
            buttonObjects[beSelectedNum].image.sprite = selectedImage;
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