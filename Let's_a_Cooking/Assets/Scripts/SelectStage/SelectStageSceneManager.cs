using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.SelectStage
{
    public class SelectStageSceneManager : MonoBehaviour
    {
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