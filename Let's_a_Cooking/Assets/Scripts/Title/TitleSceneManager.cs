using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Title
{
    public class TitleSceneManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void TouchStartButton()
        {
            StartCoroutine(LoadSceneCoroutine());
        }

        IEnumerator LoadSceneCoroutine()
        {
            yield return new WaitForSeconds(1.0f);
            SceneChanger.LoadSelectingScene(SceneName.SelectStage);
        }
    }
}
