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

            StartCoroutine(LoadSceneCoroutine());
            Stage.StageSceneManager.SetLoadStageIndex(stageNumber);
        }

        IEnumerator LoadSceneCoroutine()
        {
            yield return new WaitForSeconds(1.0f);
            SceneChanger.LoadSelectingScene(SceneName.PlayScene);
        }
    }
}